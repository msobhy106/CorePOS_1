using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Warehouse : AuditableEntity
{
    public string  Code        { get; private set; } = string.Empty;
    public string  Name        { get; private set; } = string.Empty;
    public string  NameAr      { get; private set; } = string.Empty;
    public int     BranchId    { get; private set; }
    public string? Address     { get; private set; }
    public string? ManagerName { get; private set; }
    public bool    IsMain      { get; private set; }
    public bool    IsActive    { get; private set; } = true;

    public Branch? Branch { get; private set; }

    protected Warehouse() { }

    public static Warehouse Create(string code, string name, string nameAr, int branchId,
        string? address = null, string? managerName = null, bool isMain = false)
    {
        if (string.IsNullOrWhiteSpace(code))   throw new ArgumentException("Warehouse code is required.");
        if (string.IsNullOrWhiteSpace(name))   throw new ArgumentException("Warehouse name is required.");
        if (string.IsNullOrWhiteSpace(nameAr)) throw new ArgumentException("Arabic warehouse name is required.");
        if (branchId <= 0)                     throw new ArgumentException("Valid branch ID is required.");

        return new Warehouse
        {
            Code = code.Trim().ToUpperInvariant(), Name = name.Trim(),
            NameAr = nameAr.Trim(), BranchId = branchId,
            Address = address, ManagerName = managerName, IsMain = isMain
        };
    }

    public void Update(string name, string nameAr, string? address, string? managerName)
    {
        Name = name.Trim(); NameAr = nameAr.Trim();
        Address = address; ManagerName = managerName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
