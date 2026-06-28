namespace CorePOS.Application.Interfaces;

// ════════════════════════════════════════════════════════════════════
// LICENSE SERVICE INTERFACE
// ════════════════════════════════════════════════════════════════════
public interface ILicenseService
{
    /// <summary>Validate license on startup. Returns current license state.</summary>
    Task<LicenseInfo> ValidateLicenseAsync(CancellationToken ct = default);

    /// <summary>Activate license using an activation code.</summary>
    Task<LicenseActivationResult> ActivateLicenseAsync(
        string activationCode, CancellationToken ct = default);

    /// <summary>Get machine fingerprint (hardware ID).</summary>
    string GetMachineFingerprint();

    /// <summary>Check if a specific feature is allowed by current license.</summary>
    bool IsFeatureAllowed(string featureKey);
}

// ════════════════════════════════════════════════════════════════════
// LICENSE DTOs
// ════════════════════════════════════════════════════════════════════
public record LicenseInfo
{
    public LicenseStatus Status           { get; init; }
    public string        LicenseType      { get; init; } = "Trial";  // Trial | Standard | Professional
    public DateTime?     ExpiryDate       { get; init; }
    public int           DaysRemaining    { get; init; }
    public string        LicensedTo       { get; init; } = string.Empty;
    public string        MachineId        { get; init; } = string.Empty;
    public string        LicenseCode      { get; init; } = string.Empty;
    public bool          IsValid          => Status == LicenseStatus.Active || Status == LicenseStatus.Trial;
    public bool          IsTrial          => Status == LicenseStatus.Trial;
    public bool          IsExpired        => Status == LicenseStatus.Expired;
    public bool          WillExpireSoon   => DaysRemaining <= 7 && DaysRemaining > 0;
    public string        StatusAr         => Status switch
    {
        LicenseStatus.Trial    => $"تجريبي ({DaysRemaining} يوم متبقي)",
        LicenseStatus.Active   => $"مفعّل — ينتهي: {ExpiryDate:dd/MM/yyyy}",
        LicenseStatus.Expired  => "منتهي الصلاحية",
        LicenseStatus.Invalid  => "ترخيص غير صالح",
        LicenseStatus.NotFound => "لا يوجد ترخيص",
        _                      => "غير معروف"
    };
}

public record LicenseActivationResult
{
    public bool    Success     { get; init; }
    public string? Error       { get; init; }
    public string  LicenseType { get; init; } = string.Empty;
    public DateTime ExpiryDate { get; init; }
    public string  LicensedTo  { get; init; } = string.Empty;
}

public enum LicenseStatus
{
    NotFound,  // No license file
    Trial,     // Trial period active
    Active,    // Valid license
    Expired,   // License or trial expired
    Invalid    // Tampered / wrong machine
}
