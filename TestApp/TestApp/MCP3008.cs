using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace TestApp {
  internal sealed class MCP3008 : IDisposable {

    private readonly SpiDevice _spi_port;
    private bool _disposed = false;

    private MCP3008(SpiDevice spi_port) {
      _spi_port = spi_port;
    }

    public async static Task<MCP3008> Connect(byte spi_chip_select_line, string controller_name) {
      var spi_port = await InitSPI(spi_chip_select_line, controller_name).ConfigureAwait(false);
      return new MCP3008(spi_port);
    }

    private async static Task<SpiDevice> InitSPI(byte spi_chip_select_line, string controller_name) {
      var settings = new SpiConnectionSettings(spi_chip_select_line) {
        ClockFrequency = 500000,
        Mode = SpiMode.Mode0,
      };
      var device_selector = SpiDevice.GetDeviceSelector(controller_name);
      var spi_device_controllers = await DeviceInformation.FindAllAsync(device_selector);
      return await SpiDevice.FromIdAsync(spi_device_controllers[0].Id, settings);
    }

    public int ReadValue(byte channel) {
      byte[] read_buffer = new byte[3];
      byte[] write_buffer = new byte[3] { 1, (byte)(8 + channel << 4), 0 };
      _spi_port.TransferFullDuplex(write_buffer, read_buffer);
      return convertToInt(read_buffer);
    }

    private int convertToInt([ReadOnlyArray] byte[] data) {
      int result = 0;
      result = data[1] & 0x03;
      result <<= 8;
      result += data[2];
      return result;
    }

    private void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          _spi_port.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }
  }
}
