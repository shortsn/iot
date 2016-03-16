using System;

namespace Radio.Lib.ShiftRegister {
  public interface IShiftRegister : IDisposable {
    void SendByte(byte data);
  }
}
