using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Radio.Lib.Display {
  public sealed class Display_16x2_I2C : IDisplay {

    private const byte LCD_WRITE = 0x07;

    private readonly byte _D4;
    private readonly byte _D5;
    private readonly byte _D6;
    private readonly byte _D7;
    private readonly byte _EN;
    private readonly byte _RW;
    private readonly byte _RS;
    private readonly byte _BL;

    public byte LineCount { get; } = 2;
    public byte CharCount { get; } = 16;
    
    private readonly byte[] _line_address = new byte[] { 0x00, 0x40 };

    private byte _back_light = 0x01;

    private readonly I2cDevice _i2c_port;
    private bool _disposed = false;

    public Display_16x2_I2C(I2cDevice i2c_port, byte RS, byte RW, byte EN, byte D4, byte D5, byte D6, byte D7, byte BL) {
      _RS = RS;
      _RW = RW;
      _EN = EN;
      _D4 = D4;
      _D5 = D5;
      _D6 = D6;
      _D7 = D7;
      _BL = BL;
      _i2c_port = i2c_port;
    }

    public async static Task<Display_16x2_I2C> ConnectAsync(byte deviceAddress = 0x3F, string i2c_controller_name = "I2C1", byte Rs = 0x00, byte Rw = 0x01, byte En = 0x02, byte D4 = 0x04, byte D5 = 0x05, byte D6 = 0x06, byte D7 = 0x07, byte Bl = 0x03) {
      var i2c = await InitI2C(deviceAddress, i2c_controller_name).ConfigureAwait(false);
      return new Display_16x2_I2C(i2c, Rs, Rw, En, D4, D5, D6, D7, Bl);
    }

    private async static Task<I2cDevice> InitI2C(byte device_address, string controller_name) {
      var i2c_settings = new I2cConnectionSettings(device_address) {
        BusSpeed = I2cBusSpeed.FastMode,
      };
      var device_selector = I2cDevice.GetDeviceSelector(controller_name);
      var i2c_device_controllers = await DeviceInformation.FindAllAsync(device_selector);
      return await I2cDevice.FromIdAsync(i2c_device_controllers[0].Id, i2c_settings);
    }

    public void Initialize(bool turnOnDisplay = true, bool turnOnCursor = false, bool blinkCursor = false, bool cursorDirection = true, bool textShift = false) {
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));

      PulseEnable(Convert.ToByte((1 << _D5)));

      PulseEnable(Convert.ToByte((1 << _D5)));
      PulseEnable(Convert.ToByte((1 << _D7)));

      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D7) | (Convert.ToByte(turnOnDisplay) << _D6) | (Convert.ToByte(turnOnCursor) << _D5) | (Convert.ToByte(blinkCursor) << _D4)));

      Clear();

      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D6) | (Convert.ToByte(cursorDirection) << _D5) | (Convert.ToByte(textShift) << _D4)));
    }

    public void BacklightOn() {
      _back_light = 0x01;
      SendCommand(0x00);
    }

    public void BacklightOff() {
      _back_light = 0x00;
      SendCommand(0x00);
    }

    public void PrintLine(byte line, string text) {
      SetCursor(0, line);
      PrintString(text.PadRight(CharCount));
    }

    public void PrintString(string text) {
      for (int i = 0; i < text.Length; i++) {
        PrintChar(text[i]);
      }
    }

    public void PrintChar(char letter) {
      SendData(Convert.ToByte(letter));
    }

    public void PrintSymbol(byte address) {
      SendData(address);
    }

    public void CreateChar(byte address, byte[] data) {
      SendCommand(Convert.ToByte(0x40 | (address << 3)));
      for (var i = 0; i < data.Length; i++) {
        SendData(data[i]);
      }
      Clear();
    }

    public void SetCursor(byte position, byte line) {
      var command = Convert.ToByte(position | _line_address[line] | (1 << LCD_WRITE));
      SendCommand(command);
    }
    
    public void Clear() {
      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D4)));
      Task.Delay(5).Wait();
    }

    private void Write(byte data, byte Rs) {
      PulseEnable(Convert.ToByte((data & 0xf0) | (Rs << _RS)));
      PulseEnable(Convert.ToByte((data & 0x0f) << 4 | (Rs << _RS)));
    }

    private void PulseEnable(byte data) {
      if (_disposed) {
        return;
      }
      _i2c_port.Write(new byte[] { Convert.ToByte(data | (1 << _EN) | (_back_light << _BL)) }); // Enable bit HIGH
      _i2c_port.Write(new byte[] { Convert.ToByte(data | (_back_light << _BL)) }); // Enable bit LOW
    }

    private void SendData(byte data) {
      Write(data, 1);
    }

    private void SendCommand(byte data) {
      Write(data, 0);
    }
   
    private void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          Clear();
          _i2c_port.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
