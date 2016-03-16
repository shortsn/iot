using System;

namespace TestApp.ShiftRegister {
  interface IShiftRegister : IDisposable {
    void SendByte(byte data);
  }
}
