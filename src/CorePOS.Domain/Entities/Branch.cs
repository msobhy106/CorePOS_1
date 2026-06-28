using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Branch : AuditableEntity
{
    public string  Code        { get; private set; } = string.Empty;
    public string  Name        { get; private set; } = string.Empty;
    public string  NameAr      { get; private set; } = string.Empty;
    public string? Address     { get; private set; }
    public string? Phone       { get; private set; }
    public string? ManagerName { get; private set; }
    public bool    IsMain      { get; private set; }
    public bool    IsActive    { get; private set; } = true;

    private readonly List<Warehouse> _warehouses = [];
    public IReadOnlyCollection<Warehouse> Warehouses => _warehouses.AsReadOnly();

    private readonly List<CashBox> _cashBoxes = [];
    public IReadOnlyCollection<CashBox> CashBoxes => _cashBoxes.AsReadOnly();

    protected Branch() { }

    public static Branch Create(string code, string name, string nameAr,
        string? address = null, string? phone = null, string? managerName = null, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(code))   throw new ArgumentException("Branch code is required.");
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("Branch name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic branch name is required.");

        return new Branch
        {
            Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
            NameAr = nameAr.Trim(), Address = address, Phone = phone,
            ManagerName = managerName, IsMain = isMain
        };
    }

    public void Update(string name, string nameAr, string? address, string? phone, string? managerName)
    {
        Name = name.Trim(); NameAr = nameAr.Trim();
        Address = address; Phone = phone; ManagerName = managerName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
