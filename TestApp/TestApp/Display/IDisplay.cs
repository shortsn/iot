using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Display {
  interface IDisplay : IDisposable {
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
