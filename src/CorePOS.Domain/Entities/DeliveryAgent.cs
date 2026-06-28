using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class DeliveryAgent : BaseEntity
{
    public string  Name      { get; private set; } = string.Empty;
    public string? Phone     { get; private set; }
    public int?    BranchId  { get; private set; }
    public bool    IsActive  { get; private set; } = true;

    public Branch? Branch { get; private set; }

    protected DeliveryAgent() { }

    public static DeliveryAgent Create(string name, string? phone = null, int? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Delivery agent name is required.");
        return new DeliveryAgent { Name = name.Trim(), Phone = phone?.Trim(), BranchId = branchId };
    }

    public void Update(string name, string? phone)
    {
        Name  = name.Trim();
        Phone = phone?.Trim();
    }

    public void Activate()   => IsActive = true;
    public void Deactivate() => IsActive = false;
}
