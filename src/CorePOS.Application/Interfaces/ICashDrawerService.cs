namespace CorePOS.Application.Interfaces;

public interface ICashDrawerService
{
    Task<bool> OpenAsync(CancellationToken ct = default);
    bool IsConnected { get; }
}
