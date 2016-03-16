using System;

namespace Radio.Lib.Display {
  public interface IDisplay : IDisposable {
    byte LineCount { get; }
    byte CharCount { get; }
    void BacklightOn();
    void BacklightOff();
    void PrintLine(byte line, string text);
    void PrintString(string text);
    void PrintChar(char letter);
    void PrintSymbol(byte address);
    void CreateChar(byte address, byte[] data);
    void SetCursor(byte position, byte line);
    void Clear();
  }
}
