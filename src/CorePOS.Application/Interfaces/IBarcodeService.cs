namespace CorePOS.Application.Interfaces;

public interface IBarcodeService
{
    byte[]  GenerateBarcode(string value, int width = 300, int height = 80);
    byte[]  GenerateQrCode(string value, int size = 200);
    string? ScanFromImage(byte[] imageBytes);
    Task StartListeningAsync(Action<string> onScanned, CancellationToken ct = default);
    void    StopListening();
}
