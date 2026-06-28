using CorePOS.Domain.Common;
using CorePOS.Domain.Enums;

namespace CorePOS.Domain.Entities;

public class Category : AuditableEntity
{
    public string        Code      { get; private set; } = string.Empty;
    public string        Name      { get; private set; } = string.Empty;
    public string        NameAr    { get; private set; } = string.Empty;
    public int?          ParentId  { get; private set; }
    public CategoryLevel Level     { get; private set; } = CategoryLevel.Main;
    public int           SortOrder { get; private set; }
    public bool          IsActive  { get; private set; } = true;

    public Category?           Parent      { get; private set; }
    private readonly List<Category> _children = [];
    public IReadOnlyCollection<Category> Children => _children.AsReadOnly();

    protected Category() { }

    public static Category CreateMain(string code, string name, string nameAr, int sortOrder = 0)
        => new() { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(), NameAr = nameAr.Trim(),
                   ParentId = null, Level = CategoryLevel.Main, SortOrder = sortOrder };

    public static Category CreateSub(string code, string name, string nameAr, int parentId, int sortOrder = 0)
    {
        if (parentId <= 0) throw new ArgumentException("Sub-category requires a valid parent ID.");
        return new() { Code = code.Trim().ToUpperInvariant(), Name = name.Trim(), NameAr = nameAr.Trim(),
                       ParentId = parentId, Level = CategoryLevel.Sub, SortOrder = sortOrder };
    }

    public void Update(string name, string nameAr, int sortOrder)
    {
        Name = name.Trim(); NameAr = nameAr.Trim(); SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()   { IsActive = true;  UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
}
