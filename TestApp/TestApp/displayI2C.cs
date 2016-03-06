/**
 *  Character-LCD-over-I2C 
 *  ===================
 *  Connect HD44780 LCD character display to Windows 10 IoT devices via I2C and PCF8574
 *
 *  Author: Jaroslav Zivny
 *  Version: 1.1
 *  Keywords: Windows IoT, LCD, HD44780, PCF8574, I2C bus, Raspberry Pi 2
 *  Git: https://github.com/DzeryCZ/Character-LCD-over-I2C
**/

using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace TestApp {
  internal sealed class DisplayI2C {

    private const byte LCD_WRITE = 0x07;

    private readonly byte _D4;
    private readonly byte _D5;
    private readonly byte _D6;
    private readonly byte _D7;
    private readonly byte _EN;
    private readonly byte _RW;
    private readonly byte _RS;
    private readonly byte _BL;

    private readonly byte[] _line_address = new byte[] { 0x00, 0x40 };

    private byte _back_light = 0x01;

    private readonly I2cDevice _i2c_port;

    public DisplayI2C(I2cDevice i2c_port, byte RS, byte RW, byte EN, byte D4, byte D5, byte D6, byte D7, byte BL) {
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

    public async static Task<DisplayI2C> Connect(byte deviceAddress, string controllerName, byte Rs = 0x00, byte Rw = 0x01, byte En = 0x02, byte D4 = 0x04, byte D5 = 0x05, byte D6 = 0x06, byte D7 = 0x07, byte Bl = 0x03) {
      var i2c = await StartI2C(deviceAddress, controllerName).ConfigureAwait(false);
      return new DisplayI2C(i2c, Rs, Rw, En, D4, D5, D6, D7, Bl);
    }

    /**
    * Start I2C Communication
    **/
    private async static Task<I2cDevice> StartI2C(byte device_address, string controller_name) {
      var i2c_settings = new I2cConnectionSettings(device_address);
      i2c_settings.BusSpeed = I2cBusSpeed.FastMode;
      var device_selector = I2cDevice.GetDeviceSelector(controller_name);
      var i2c_device_controllers = await DeviceInformation.FindAllAsync(device_selector);
      return await I2cDevice.FromIdAsync(i2c_device_controllers[0].Id, i2c_settings);
    }

    /**
    * Initialization
    **/
    public void Initialize(bool turnOnDisplay = true, bool turnOnCursor = false, bool blinkCursor = false, bool cursorDirection = true, bool textShift = false) {
      /* Init sequence */
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));
      PulseEnable(Convert.ToByte((1 << _D5) | (1 << _D4)));

      /*  Init 4-bit mode */
      PulseEnable(Convert.ToByte((1 << _D5)));

      /* Init 4-bit mode + 2 line */
      PulseEnable(Convert.ToByte((1 << _D5)));
      PulseEnable(Convert.ToByte((1 << _D7)));

      /* Turn on display, cursor */
      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D7) | (Convert.ToByte(turnOnDisplay) << _D6) | (Convert.ToByte(turnOnCursor) << _D5) | (Convert.ToByte(blinkCursor) << _D4)));

      ClearScreen();

      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D6) | (Convert.ToByte(cursorDirection) << _D5) | (Convert.ToByte(textShift) << _D4)));
    }

    /**
    * Turn the backlight ON.
    **/
    public void TurnOnBacklight() {
      _back_light = 0x01;
      SendCommand(0x00);
    }

    /**
    * Turn the backlight OFF.
    **/
    public void TurnOffBacklight() {
      _back_light = 0x00;
      SendCommand(0x00);
    }

    /**
    * Can print string onto display
    **/
    public void PrintString(string text) {
      for (int i = 0; i < text.Length; i++) {
        PrintChar(text[i]);
      }
    }

    /**
    * Print single character onto display
    **/
    public void PrintChar(char letter) {
      Write(Convert.ToByte(letter), 1);
    }
    
    public void GoToPosition(byte column, byte row) {
      var command = Convert.ToByte(column | _line_address[row] | (1 << LCD_WRITE));
      SendCommand(command);
    }
    
    /**
    * Send data to display
    **/
    public void SendData(byte data) {
      Write(data, 1);
    }

    /**
    * Send command to display
    **/
    public void SendCommand(byte data) {
      Write(data, 0);
    }

    /**
    * Clear display and set cursor at start of the first line
    **/
    public void ClearScreen() {
      PulseEnable(0);
      PulseEnable(Convert.ToByte((1 << _D4)));
      //Task.Delay(5).Wait();
    }

    /**
    * Send pure data to display
    **/
    public void Write(byte data, byte Rs) {
      PulseEnable(Convert.ToByte((data & 0xf0) | (Rs << _RS)));
      PulseEnable(Convert.ToByte((data & 0x0f) << 4 | (Rs << _RS)));
      //Task.Delay(5).Wait(); //In case of problem with displaying wrong characters uncomment this part
    }

    /**
    * Create falling edge of "enable" pin to write data/inctruction to display
    */
    private void PulseEnable(byte data) {
      _i2c_port.Write(new byte[] { Convert.ToByte(data | (1 << _EN) | (_back_light << _BL)) }); // Enable bit HIGH
      _i2c_port.Write(new byte[] { Convert.ToByte(data | (_back_light << _BL)) }); // Enable bit LOW
      //Task.Delay(2).Wait(); //In case of problem with displaying wrong characters uncomment this part
    }

    /**
    * Save custom symbol to CGRAM
    **/
    public void CreateSymbol(byte[] data, byte address) {
      SendCommand(Convert.ToByte(0x40 | (address << 3)));
      for (var i = 0; i < data.Length; i++) {
        SendData(data[i]);
      }
      ClearScreen();
    }

    /**
    * Print custom symbol
    **/
    public void PrintSymbol(byte address) {
      SendData(address);
    }

  }
}
