using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using CorePOS.Application.Interfaces;

namespace CorePOS.Infrastructure.License;

/// <summary>
/// License service — Phase 12.
/// 
/// LICENSE FILE: license.dat in AppBaseDirectory
/// FORMAT: JSON payload → AES-256-CBC encrypt → Base64 → HMAC-SHA256 signature
/// MACHINE BINDING: CPU ID + Motherboard SN + MAC address → SHA256 hash
/// 
/// TRIAL: 7 days from first install date (stored in registry + license.dat)
/// ACTIVATION: code = Base64(AES(JSON{type,expiry,machine,licensed_to}))
///             server or offline generator creates the code
/// </summary>
public class LicenseService : ILicenseService
{
    private readonly ILogger<LicenseService> _logger;

    // Symmetric key for license encryption (change this secret in production!)
    // In production: store in a native DLL or obfuscate
    private static readonly byte[] LicenseKey = Encoding.UTF8.GetBytes("C0reP0S_L1c3nse_K3y_2024!@#$%^&*");
    private static readonly byte[] HmacKey    = Encoding.UTF8.GetBytes("C0reP0S_HMAC_S3cr3t_K3y_XYZ12345");

    private const string LicenseFileName    = "license.dat";
    private const string TrialRegKey        = @"SOFTWARE\CoreTech\CorePOS";
    private const string TrialRegValue      = "InstallDate";
    private const int    TrialDays          = 7;

    private LicenseInfo? _cachedLicense;

    public LicenseService(ILogger<LicenseService> logger)
    {
        _logger = logger;
    }

    // ════════════════════════════════════════════════════════════════
    // VALIDATE LICENSE
    // ════════════════════════════════════════════════════════════════
    public async Task<LicenseInfo> ValidateLicenseAsync(CancellationToken ct = default)
    {
        if (_cachedLicense != null) return _cachedLicense;

        try
        {
            var licensePath = GetLicensePath();

            // No license file → check/start trial
            if (!File.Exists(licensePath))
            {
                _cachedLicense = GetOrStartTrial();
                return _cachedLicense;
            }

            // Read and validate license file
            var fileContent = await File.ReadAllTextAsync(licensePath, ct);
            var payload     = DecryptAndVerifyLicense(fileContent);

            if (payload == null)
            {
                _logger.LogWarning("License file tampered or invalid");
                _cachedLicense = new LicenseInfo { Status = LicenseStatus.Invalid };
                return _cachedLicense;
            }

            // Check machine binding
            var currentMachine = GetMachineFingerprint();
            if (!string.IsNullOrEmpty(payload.MachineId) &&
                payload.MachineId != currentMachine &&
                payload.MachineId != "ANY")
            {
                _logger.LogWarning("License machine mismatch. Expected={Expected} Got={Got}",
                    payload.MachineId, currentMachine);
                _cachedLicense = new LicenseInfo { Status = LicenseStatus.Invalid };
                return _cachedLicense;
            }

            // Check expiry
            var now = DateTime.Today;
            if (payload.ExpiryDate < now)
            {
                _cachedLicense = new LicenseInfo
                {
                    Status        = LicenseStatus.Expired,
                    LicenseType   = payload.LicenseType,
                    ExpiryDate    = payload.ExpiryDate,
                    DaysRemaining = 0,
                    LicensedTo    = payload.LicensedTo,
                    MachineId     = currentMachine
                };
                return _cachedLicense;
            }

            var daysRemaining = (int)(payload.ExpiryDate - now).TotalDays;
            _cachedLicense = new LicenseInfo
            {
                Status        = LicenseStatus.Active,
                LicenseType   = payload.LicenseType,
                ExpiryDate    = payload.ExpiryDate,
                DaysRemaining = daysRemaining,
                LicensedTo    = payload.LicensedTo,
                MachineId     = currentMachine,
                LicenseCode   = fileContent[..Math.Min(20, fileContent.Length)] + "..."
            };

            return _cachedLicense;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "License validation error");
            return new LicenseInfo { Status = LicenseStatus.Invalid };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // ACTIVATE LICENSE
    // ════════════════════════════════════════════════════════════════
    public async Task<LicenseActivationResult> ActivateLicenseAsync(
        string activationCode, CancellationToken ct = default)
    {
        try
        {
            activationCode = activationCode.Trim().Replace(" ", "").Replace("-", "");

            // Decrypt activation code
            var payload = DecryptActivationCode(activationCode);
            if (payload == null)
                return new LicenseActivationResult
                { Success = false, Error = "كود التفعيل غير صالح" };

            // Verify machine ID
            var machine = GetMachineFingerprint();
            if (!string.IsNullOrEmpty(payload.MachineId) &&
                payload.MachineId != machine &&
                payload.MachineId != "ANY")
                return new LicenseActivationResult
                { Success = false, Error = "كود التفعيل مخصص لجهاز آخر" };

            // Check expiry
            if (payload.ExpiryDate < DateTime.Today)
                return new LicenseActivationResult
                { Success = false, Error = "كود التفعيل منتهي الصلاحية" };

            // Write license file
            payload.MachineId = machine;
            var encryptedLicense = EncryptAndSignLicense(payload);
            var licensePath = GetLicensePath();
            await File.WriteAllTextAsync(licensePath, encryptedLicense, ct);

            // Clear cache
            _cachedLicense = null;

            _logger.LogInformation("License activated: {Type} until {Date} for {User}",
                payload.LicenseType, payload.ExpiryDate, payload.LicensedTo);

            return new LicenseActivationResult
            {
                Success     = true,
                LicenseType = payload.LicenseType,
                ExpiryDate  = payload.ExpiryDate,
                LicensedTo  = payload.LicensedTo
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Activation error");
            return new LicenseActivationResult { Success = false, Error = ex.Message };
        }
    }

    // ════════════════════════════════════════════════════════════════
    // MACHINE FINGERPRINT
    // ════════════════════════════════════════════════════════════════
    public string GetMachineFingerprint()
    {
        try
        {
            var parts = new List<string>();

            // CPU ID
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                    parts.Add(obj["ProcessorId"]?.ToString() ?? "");
            }
            catch { parts.Add("NO_CPU"); }

            // Motherboard serial
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get())
                    parts.Add(obj["SerialNumber"]?.ToString() ?? "");
            }
            catch { parts.Add("NO_MB"); }

            // First physical network adapter MAC
            try
            {
                var mac = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                n.OperationalStatus == OperationalStatus.Up)
                    .Select(n => n.GetPhysicalAddress().ToString())
                    .FirstOrDefault(m => !string.IsNullOrEmpty(m) && m != "000000000000");
                parts.Add(mac ?? "NO_MAC");
            }
            catch { parts.Add("NO_MAC"); }

            // Hash all parts together
            var raw  = string.Join("|", parts.Where(p => !string.IsNullOrEmpty(p)));
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash)[..16].ToUpperInvariant();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get machine fingerprint");
            return "UNKNOWN_MACHINE";
        }
    }

    public bool IsFeatureAllowed(string featureKey)
    {
        if (_cachedLicense == null) return false;
        if (!_cachedLicense.IsValid) return false;

        // Trial: limit features
        if (_cachedLicense.IsTrial)
        {
            var trialRestricted = new[] { "MultipleUsers", "CloudBackup", "AdvancedReports" };
            return !trialRestricted.Contains(featureKey);
        }

        // Standard: most features
        if (_cachedLicense.LicenseType == "Standard")
        {
            var premiumOnly = new[] { "MultipleUsers", "CloudBackup" };
            return !premiumOnly.Contains(featureKey);
        }

        // Professional: all features
        return true;
    }

    // ════════════════════════════════════════════════════════════════
    // TRIAL MANAGEMENT
    // ════════════════════════════════════════════════════════════════
    private LicenseInfo GetOrStartTrial()
    {
        var machine      = GetMachineFingerprint();
        DateTime installDate;

        // Read from registry
        try
        {
            using var key = Microsoft.Win32.Registry.LocalMachine
                .OpenSubKey(TrialRegKey, writable: true)
                ?? Microsoft.Win32.Registry.LocalMachine
                    .CreateSubKey(TrialRegKey);

            var stored = key?.GetValue(TrialRegValue)?.ToString();
            if (!string.IsNullOrEmpty(stored) && DateTime.TryParse(stored, out var d))
            {
                installDate = d;
            }
            else
            {
                installDate = DateTime.Today;
                key?.SetValue(TrialRegValue, installDate.ToString("O"));
            }
        }
        catch
        {
            installDate = DateTime.Today;
        }

        var expiryDate    = installDate.AddDays(TrialDays);
        var daysRemaining = Math.Max(0, (int)(expiryDate - DateTime.Today).TotalDays);

        if (daysRemaining <= 0)
            return new LicenseInfo
            {
                Status     = LicenseStatus.Expired,
                LicenseType= "Trial",
                ExpiryDate = expiryDate,
                MachineId  = machine
            };

        return new LicenseInfo
        {
            Status        = LicenseStatus.Trial,
            LicenseType   = "Trial",
            ExpiryDate    = expiryDate,
            DaysRemaining = daysRemaining,
            MachineId     = machine
        };
    }

    // ════════════════════════════════════════════════════════════════
    // CRYPTO HELPERS
    // ════════════════════════════════════════════════════════════════
    private string EncryptAndSignLicense(LicensePayload payload)
    {
        var json     = JsonSerializer.Serialize(payload);
        var jsonBytes= Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key       = LicenseKey;
        aes.GenerateIV();

        using var ms         = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);   // prepend IV
        using var cs         = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(jsonBytes, 0, jsonBytes.Length);
        cs.FlushFinalBlock();

        var encrypted = ms.ToArray();
        var b64       = Convert.ToBase64String(encrypted);

        // HMAC signature
        using var hmac = new HMACSHA256(HmacKey);
        var sig = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(b64)));

        return $"{b64}|{sig}";
    }

    private LicensePayload? DecryptAndVerifyLicense(string fileContent)
    {
        try
        {
            var parts = fileContent.Split('|');
            if (parts.Length != 2) return null;

            var b64 = parts[0];
            var sig = parts[1];

            // Verify HMAC
            using var hmac = new HMACSHA256(HmacKey);
            var expectedSig = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(b64)));
            if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(sig),
                Convert.FromBase64String(expectedSig)))
                return null;

            // Decrypt
            var encrypted = Convert.FromBase64String(b64);
            using var aes = Aes.Create();
            aes.Key       = LicenseKey;
            var iv        = encrypted[..16];
            aes.IV        = iv;

            using var ms = new MemoryStream(encrypted, 16, encrypted.Length - 16);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            var json = sr.ReadToEnd();

            return JsonSerializer.Deserialize<LicensePayload>(json);
        }
        catch { return null; }
    }

    private static LicensePayload? DecryptActivationCode(string code)
    {
        try
        {
            var bytes = Convert.FromBase64String(code);
            using var aes = Aes.Create();
            aes.Key       = LicenseKey;
            var iv        = bytes[..16];
            aes.IV        = iv;

            using var ms = new MemoryStream(bytes, 16, bytes.Length - 16);
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return JsonSerializer.Deserialize<LicensePayload>(sr.ReadToEnd());
        }
        catch { return null; }
    }

    private static string GetLicensePath()
        => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LicenseFileName);
}

// ── License payload (serialized inside license.dat) ───────────────
public class LicensePayload
{
    public string   LicenseType  { get; set; } = "Trial";
    public DateTime ExpiryDate   { get; set; }
    public string   LicensedTo   { get; set; } = string.Empty;
    public string   MachineId    { get; set; } = string.Empty;
    public DateTime IssuedDate   { get; set; } = DateTime.Today;
    public string   IssuedBy     { get; set; } = "Core Tech";
}
