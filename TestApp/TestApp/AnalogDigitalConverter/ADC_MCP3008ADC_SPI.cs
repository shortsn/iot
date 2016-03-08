using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TestApp.AnalogDigitalConverter;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;

namespace TestApp {
  internal sealed class ADC_MCP3008_SPI : IDisposable {

    public int Resolution { get; } = 1024;
    private readonly SpiDevice _spi_port;
    private bool _disposed = false;
    
    private ADC_MCP3008_SPI(SpiDevice spi_port) {
      _spi_port = spi_port;
    }

    public async static Task<ADC_MCP3008_SPI> ConnectAsync(byte spi_chip_select_line = 0, string spi_controller_name = "SPI0") {
      var spi_port = await InitSPI(spi_chip_select_line, spi_controller_name).ConfigureAwait(false);
      return new ADC_MCP3008_SPI(spi_port);
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
      byte[] write_buffer = new byte[3] { 1, Convert.ToByte(8 + channel << 4), 0 };
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

    public IObservable<PortValue<int>> MonitorPorts(TimeSpan interval, params byte[] ports)
      => Observable
      .Interval(interval)
      .SelectMany(_ => ports.Select(port => new PortValue<int>(port, ReadValue(port))))
      .GroupBy(value => value.Port)
      .Select(port => port.DistinctUntilChanged())
      .SelectMany(values => values);

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
