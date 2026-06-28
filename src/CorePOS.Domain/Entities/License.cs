using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class License : BaseEntity
{
    public string      LicenseKey     { get; private set; } = string.Empty;
    public string?     ActivationCode { get; private set; }
    public string?     MachineId      { get; private set; }
    public LicenseType LicenseType    { get; private set; } = LicenseType.Trial;
    public DateTime    StartDate      { get; private set; }
    public DateTime    ExpiryDate     { get; private set; }
    public bool        IsActive       { get; private set; } = true;
    public DateTime    CreatedAt      { get; private set; } = DateTime.UtcNow;

    protected License() { }

    public static License CreateTrial(int trialDays = 7)
    {
        var now = DateTime.UtcNow;
        return new License
        {
            LicenseKey  = $"TRIAL-{Guid.NewGuid():N}".ToUpperInvariant(),
            LicenseType = LicenseType.Trial,
            StartDate   = now,
            ExpiryDate  = now.AddDays(trialDays),
            IsActive    = true
        };
    }

    public void Activate(string activationCode, string machineId, DateTime expiryDate,
        LicenseType licenseType = LicenseType.Standard)
    {
        if (string.IsNullOrWhiteSpace(activationCode)) throw new ArgumentException("Activation code is required.");
        if (string.IsNullOrWhiteSpace(machineId))      throw new ArgumentException("Machine ID is required.");
        if (expiryDate <= DateTime.UtcNow)             throw new ArgumentException("Expiry date must be in the future.");

        ActivationCode = activationCode.Trim().ToUpperInvariant();
        MachineId      = machineId.Trim();
        LicenseType    = licenseType;
        ExpiryDate     = expiryDate;
        IsActive       = true;
    }

    public bool IsExpired         => DateTime.UtcNow > ExpiryDate;
    public bool IsValid           => IsActive && !IsExpired;
    public bool IsTrial           => LicenseType == LicenseType.Trial;
    public int  DaysRemaining     => Math.Max(0, (ExpiryDate - DateTime.UtcNow).Days);
    public bool IsAboutToExpire   => DaysRemaining <= 7;

    public void Deactivate() => IsActive = false;
}
