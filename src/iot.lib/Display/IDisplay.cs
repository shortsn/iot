using System;

namespace iot.lib.Display {
  public interface IDisplay : IDisposable {
    byte LineCount { get; }
    byte CharCount { get; }

    void Initialize(bool turnOnDisplay = true, bool turnOnCursor = false, bool blinkCursor = false, bool cursorDirection = true, bool textShift = false);
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
