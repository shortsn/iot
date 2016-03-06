using System;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {

    private const string I2C_CONTROLLER_NAME = "I2C1";
    private const byte DEVICE_I2C_ADDRESS = 0x3F;

    private const string SPI_CONTROLLER_NAME = "SPI0";
    private const int SPI_CHIP_SELECT_LINE = 0;

    private const int LED_PIN = 4;
    private GpioPin ledPin;

    public void Run(IBackgroundTaskInstance taskInstance) {
      InitGpio();

      using (var mcp3008 = MCP3008.Connect(SPI_CHIP_SELECT_LINE, SPI_CONTROLLER_NAME).Result) {
        using (var display = DisplayI2C.Connect(DEVICE_I2C_ADDRESS, I2C_CONTROLLER_NAME).Result) {
          display.Initialize();

          // Here is created new symbol
          // Take a look at data - it's smile emoticon
          // 0x00 => 00000
          // 0x00 => 00000
          // 0x0A => 01010
          // 0x00 => 00000
          // 0x11 => 10001
          // 0x0E => 01110
          // 0x00 => 00000
          // 0x00 => 00000 

          display.ClearScreen();
          display.BacklightOff();

          Task.Delay(2000).Wait();
          display.BacklightOn();

          display.CreateSymbol(new byte[] { 0x00, 0x00, 0x0A, 0x00, 0x11, 0x0E, 0x00, 0x00 }, 0x00);
          display.PrintString("Good morning,");
          display.PrintSymbol(0x00);
          display.GoToPosition(0, 1);

          while (true) {
            var value = mcp3008.ReadValue(0);
            System.Diagnostics.Debug.WriteLine($"{ Math.Round(value / 102.4, 0, MidpointRounding.ToEven)}");
            LightLED(value);
            display.GoToPosition(0, 1);
            display.PrintString($"Value: {value} ");
            Task.Delay(500).Wait();
          }
        }
      }
    }
    
    private void InitGpio() {
      var gpio = GpioController.GetDefault();

      /* Show an error if there is no GPIO controller */
      if (gpio == null) {
        throw new Exception("There is no GPIO controller on this device");
      }

      ledPin = gpio.OpenPin(LED_PIN);

      /* GPIO state is initially undefined, so we assign a default value before enabling as output */
      ledPin.Write(GpioPinValue.High);
      ledPin.SetDriveMode(GpioPinDriveMode.Output);
    }

    private void LightLED(int value) {
      if (value > 512) {
        ledPin.Write(GpioPinValue.Low);
      } else {
        ledPin.Write(GpioPinValue.High);
      }
    }
  }
}
