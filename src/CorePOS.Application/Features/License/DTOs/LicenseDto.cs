using CorePOS.Application.Interfaces;

namespace CorePOS.Application.Features.License.DTOs;

public class LicenseDto
{
    public LicenseStatus Status          { get; set; }
    public string        LicenseType     { get; set; } = "Trial";
    public DateTime?     ExpiryDate      { get; set; }
    public int           DaysRemaining   { get; set; }
    public string        LicensedTo      { get; set; } = string.Empty;
    public string        MachineId       { get; set; } = string.Empty;
    public bool          IsValid         { get; set; }
    public bool          WillExpireSoon  { get; set; }
    public string        StatusAr        { get; set; } = string.Empty;
}
