using System;

namespace iot.lib.ShiftRegister {
  public interface IShiftRegister : IDisposable {
    void SendByte(byte data);
  }
}
