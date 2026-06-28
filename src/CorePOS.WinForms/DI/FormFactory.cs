using Microsoft.Extensions.DependencyInjection;

namespace CorePOS.WinForms.DI;

/// <summary>
/// Creates WinForms forms via the DI container.
/// Forms must be registered as Transient.
/// </summary>
public class FormFactory
{
    private readonly IServiceProvider _sp;
    public FormFactory(IServiceProvider sp) => _sp = sp;

    public T Create<T>() where T : Form
        => _sp.GetRequiredService<T>();
}
