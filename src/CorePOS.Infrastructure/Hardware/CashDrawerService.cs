using System.IO.Ports;
using CorePOS.Application.Interfaces;

namespace CorePOS.Infrastructure.Hardware;

public class CashDrawerService : ICashDrawerService
{
    private readonly ISettingsRepository _settings;
    private SerialPort? _port;

    public CashDrawerService(ISettingsRepository settings) => _settings = settings;

    public bool IsConnected => _port?.IsOpen ?? false;

    public async Task<bool> OpenAsync(CancellationToken ct = default)
    {
        try
        {
            var enabled = await _settings.GetBoolAsync("CashDrawerEnabled", false, ct);
            if (!enabled) return true;   // silently succeed if not configured

            var portName = await _settings.GetStringAsync("CashDrawerPort", "COM1", ct);

            // ESC/POS cash drawer kick command
            var cmd = new byte[] { 0x1B, 0x70, 0x00, 0x19, 0xFA };

            if (portName.Equals("USB", StringComparison.OrdinalIgnoreCase))
            {
                // Trigger via receipt printer (most USB cash drawers)
                var printerName = await _settings.GetStringAsync("ReceiptPrinterName", "", ct);
                if (!string.IsNullOrEmpty(printerName))
                {
                    Printing.RawPrinterHelper.SendBytesToPrinter(printerName, cmd);
                    return true;
                }
            }
            else
            {
                // Serial port cash drawer
                _port ??= new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
                if (!_port.IsOpen) _port.Open();
                _port.Write(cmd, 0, cmd.Length);
                return true;
            }
            return false;
        }
        catch { return false; }
    }
}
