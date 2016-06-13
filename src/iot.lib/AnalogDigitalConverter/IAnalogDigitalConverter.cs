using System;

namespace iot.lib.AnalogDigitalConverter {
  public interface IAnalogDigitalConverter : IDisposable {
    int Resolution { get; }
    int ReadValue(byte channel);
    IObservable<PortValue<int>> MonitorPorts(TimeSpan interval, params byte[] ports);
  }
}
