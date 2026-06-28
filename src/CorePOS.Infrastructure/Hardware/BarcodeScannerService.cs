using System.IO.Ports;
using CorePOS.Application.Interfaces;
using ZXing;
using ZXing.Common;

namespace CorePOS.Infrastructure.Hardware;

public class BarcodeScannerService : IBarcodeService, IDisposable
{
    private SerialPort?         _port;
    private Action<string>?     _onScanned;
    private CancellationToken   _ct;
    private bool                _listening;

    // ── Barcode Generation ────────────────────────────────
    public byte[] GenerateBarcode(string value, int width = 300, int height = 80)
    {
        var writer = new ZXing.BarcodeWriterPixelData
        {
            Format  = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width   = width,
                Height  = height,
                Margin  = 5,
                PureBarcode = false
            }
        };

        var pixelData = writer.Write(value);
        using var bmp = new System.Drawing.Bitmap(
            pixelData.Width, pixelData.Height,
            System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        var bmpData = bmp.LockBits(
            new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppRgb);

        try
        {
            System.Runtime.InteropServices.Marshal.Copy(
                pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);
        }
        finally { bmp.UnlockBits(bmpData); }

        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public byte[] GenerateQrCode(string value, int size = 200)
    {
        var writer = new ZXing.BarcodeWriterPixelData
        {
            Format  = BarcodeFormat.QR_CODE,
            Options = new EncodingOptions { Width = size, Height = size, Margin = 2 }
        };
        var pixelData = writer.Write(value);
        using var bmp = new System.Drawing.Bitmap(size, size,
            System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        var bmpData = bmp.LockBits(
            new System.Drawing.Rectangle(0, 0, size, size),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bmpData.Scan0, pixelData.Pixels.Length);
        bmp.UnlockBits(bmpData);
        using var ms = new MemoryStream();
        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public string? ScanFromImage(byte[] imageBytes)
    {
        try
        {
            using var ms  = new MemoryStream(imageBytes);
            using var bmp = new System.Drawing.Bitmap(ms);
            var reader = new ZXing.BarcodeReaderGeneric();
            var result = reader.Decode(new ZXing.RGBLuminanceSource(
                imageBytes, bmp.Width, bmp.Height));
            return result?.Text;
        }
        catch { return null; }
    }

    // ── Serial Port Scanner ───────────────────────────────
    public async Task StartListeningAsync(
        Action<string> onScanned, CancellationToken ct = default)
    {
        if (_listening) return;
        _onScanned = onScanned;
        _ct        = ct;
        _listening = true;

        // USB HID scanners send keystrokes — handled by WinForms KeyPress.
        // Serial scanners need serial port listener:
        await Task.Run(() =>
        {
            try
            {
                // Port configured in settings — skip if not configured
                if (_port is null || !_port.IsOpen) return;
                _port.DataReceived += (s, e) =>
                {
                    var data = _port.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data)) onScanned?.Invoke(data);
                };
            }
            catch { /* Ignore serial port errors */ }
        }, ct);
    }

    public void StopListening()
    {
        _listening = false;
        _onScanned = null;
        try { _port?.Close(); } catch { }
    }

    public void Configure(string portName, int baudRate = 9600)
    {
        _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
        {
            ReadTimeout  = 500,
            WriteTimeout = 500,
            NewLine      = "\r"
        };
        try { _port.Open(); } catch { /* Port not available */ }
    }

    public void Dispose()
    {
        StopListening();
        _port?.Dispose();
    }
}
