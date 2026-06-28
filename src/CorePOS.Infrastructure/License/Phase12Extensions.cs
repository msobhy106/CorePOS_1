using Microsoft.Extensions.DependencyInjection;
using CorePOS.Application.Interfaces;
using CorePOS.Infrastructure.License;
using CorePOS.WinForms.Forms.License;

namespace CorePOS.Infrastructure;

/// <summary>Phase 12 — License service DI registration.</summary>
public static class Phase12ServiceExtensions
{
    public static IServiceCollection AddPhase12Services(
        this IServiceCollection services)
    {
        services.AddSingleton<ILicenseService, LicenseService>();
        return services;
    }
}

// ════════════════════════════════════════════════════════════════════
// UPDATED Program.cs STARTUP WITH LICENSE CHECK
// Add this inside Main() before showing LoginForm:
// ════════════════════════════════════════════════════════════════════
/*
// ── License check ──────────────────────────────────────────────
var licenseService = ServiceProvider.GetRequiredService<ILicenseService>();
var licenseInfo    = await licenseService.ValidateLicenseAsync();

if (!licenseInfo.IsValid)
{
    using var guard = new LicenseGuard(licenseService, licenseInfo);
    Application.Run(guard);
    if (!guard.AllowContinue) return;
}
else if (licenseInfo.WillExpireSoon)
{
    MessageBox.Show(
        $"⚠ تنبيه: ترخيص البرنامج سينتهي خلال {licenseInfo.DaysRemaining} أيام.\n" +
        "يرجى تجديد الترخيص لتجنب الانقطاع.",
        "تحذير الترخيص",
        MessageBoxButtons.OK,
        MessageBoxIcon.Warning);
}

// ── Show login ──────────────────────────────────────────────────
var loginForm = ServiceProvider.GetRequiredService<LoginForm>();
Application.Run(loginForm);
*/

// ════════════════════════════════════════════════════════════════════
// LICENSE CODE GENERATOR — Internal Tool for Core Tech
// This is a standalone console helper to generate activation codes.
// Run: LicenseGenerator.exe <machineId> <type> <days> <licensedTo>
// ════════════════════════════════════════════════════════════════════
namespace CorePOS.Tools.LicenseGenerator;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public static class LicenseCodeGenerator
{
    // Must match LicenseService keys
    private static readonly byte[] LicenseKey = Encoding.UTF8.GetBytes("C0reP0S_L1c3nse_K3y_2024!@#$%^&*");

    /// <summary>
    /// Generate an activation code for a customer.
    /// </summary>
    /// <param name="machineId">Customer's machine fingerprint (or "ANY" for floating)</param>
    /// <param name="licenseType">Trial | Standard | Professional</param>
    /// <param name="daysValid">Number of days the license is valid</param>
    /// <param name="licensedTo">Customer name / company</param>
    public static string GenerateCode(
        string machineId,
        string licenseType,
        int    daysValid,
        string licensedTo)
    {
        var payload = new
        {
            LicenseType = licenseType,
            ExpiryDate  = DateTime.Today.AddDays(daysValid),
            LicensedTo  = licensedTo,
            MachineId   = machineId,
            IssuedDate  = DateTime.Today,
            IssuedBy    = "Core Tech"
        };

        var json  = JsonSerializer.Serialize(payload);
        var bytes = Encoding.UTF8.GetBytes(json);

        using var aes = Aes.Create();
        aes.Key       = LicenseKey;
        aes.GenerateIV();

        using var ms = new MemoryStream();
        ms.Write(aes.IV, 0, aes.IV.Length);
        using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
        cs.Write(bytes, 0, bytes.Length);
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    /// <summary>
    /// Generate a formatted activation code with dashes for readability.
    /// Example: ABCD-EFGH-IJKL-MNOP
    /// </summary>
    public static string GenerateFormattedCode(
        string machineId, string licenseType, int daysValid, string licensedTo)
    {
        var raw = GenerateCode(machineId, licenseType, daysValid, licensedTo);
        // Remove Base64 padding and special chars
        var clean = raw.Replace("+", "A").Replace("/", "B").Replace("=", "");
        // Group into chunks of 4
        var chunks = Enumerable.Range(0, (int)Math.Ceiling(clean.Length / 4.0))
            .Select(i => clean.Substring(i * 4, Math.Min(4, clean.Length - i * 4)));
        return string.Join("-", chunks);
    }

    // ── Console entry point (for use as standalone tool) ──────────
    public static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("═══════════════════════════════════════");
        Console.WriteLine("   CorePOS License Code Generator");
        Console.WriteLine("   Core Tech — Internal Tool");
        Console.WriteLine("═══════════════════════════════════════\n");

        string machineId, licenseType, licensedTo;
        int    daysValid;

        if (args.Length >= 4)
        {
            machineId   = args[0];
            licenseType = args[1];
            daysValid   = int.Parse(args[2]);
            licensedTo  = args[3];
        }
        else
        {
            Console.Write("معرّف الجهاز (أو ANY للترخيص المرن): ");
            machineId   = Console.ReadLine()?.Trim() ?? "ANY";

            Console.Write("نوع الترخيص (Standard/Professional): ");
            licenseType = Console.ReadLine()?.Trim() ?? "Standard";

            Console.Write("عدد الأيام (365 = سنة): ");
            daysValid   = int.TryParse(Console.ReadLine(), out var d) ? d : 365;

            Console.Write("مرخص لـ (اسم العميل/الشركة): ");
            licensedTo  = Console.ReadLine()?.Trim() ?? "Customer";
        }

        var code        = GenerateCode(machineId, licenseType, daysValid, licensedTo);
        var expiryDate  = DateTime.Today.AddDays(daysValid);

        Console.WriteLine("\n══════════ كود التفعيل ══════════");
        Console.WriteLine(code);
        Console.WriteLine("═══════════════════════════════════\n");
        Console.WriteLine($"معرّف الجهاز : {machineId}");
        Console.WriteLine($"النوع        : {licenseType}");
        Console.WriteLine($"صالح حتى     : {expiryDate:dd/MM/yyyy}");
        Console.WriteLine($"مرخص لـ      : {licensedTo}");
        Console.WriteLine("\nانسخ الكود وأرسله للعميل.");
        Console.WriteLine("اضغط أي زر للخروج...");
        Console.ReadKey();
    }
}
