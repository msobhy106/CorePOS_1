using CorePOS.Domain.Common;

namespace CorePOS.Domain.Entities;

public class Permission : BaseEntity
{
    public string ModuleKey    { get; private set; } = string.Empty;
    public string ActionKey    { get; private set; } = string.Empty;
    public string ModuleNameAr { get; private set; } = string.Empty;
    public string ActionNameAr { get; private set; } = string.Empty;

    protected Permission() { }

    public static Permission Create(string moduleKey, string actionKey, string moduleNameAr, string actionNameAr)
        => new() { ModuleKey = moduleKey, ActionKey = actionKey, ModuleNameAr = moduleNameAr, ActionNameAr = actionNameAr };

    public string FullKey => $"{ModuleKey}.{ActionKey}";
}
