using System;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {
    //Setup address
    private const string I2C_CONTROLLER_NAME = "I2C1";
    private const byte DEVICE_I2C_ADDRESS = 0x3F;

    private const int LED_PIN = 4;
    private GpioPin ledPin;

    private const string SPI_CONTROLLER_NAME = "SPI0";
    private const int SPI_CHIP_SELECT_LINE = 0;
    private SpiDevice SpiADC;

    private int adcValue;

    public void Run(IBackgroundTaskInstance taskInstance) {
      
      InitAll().Wait();

      // Here is I2C bus and Display itself initialized.
      //
      //  I2C bus is initialized by library constructor. There is also defined PCF8574 pins 
      //  Default `DEVICE_I2C_ADDRESS` is `0x27` (you can change it by A0-2 pins on PCF8574 - for more info please read datasheet)
      //  `I2C_CONTROLLER_NAME` for Raspberry Pi 2 is `"I2C1"`
      //  For Arduino it should be `"I2C5"`, but I did't test it.
      //  Other arguments should be: RS = 0, RW = 1, EN = 2, D4 = 4, D5 = 5, D6 = 6, D7 = 7, BL = 3
      //  But it depends on your PCF8574.
      var lcd = DisplayI2C.Connect(DEVICE_I2C_ADDRESS, I2C_CONTROLLER_NAME).Result;
      lcd.Initialize();


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

      // data of symbol by lines                          //address of symbol
      lcd.CreateSymbol(new byte[] { 0x00, 0x00, 0x0A, 0x00, 0x11, 0x0E, 0x00, 0x00 }, 0x00);

      // Here is printed string
      lcd.PrintString("Good morning,");

      //// Navigation to second line
      //lcd.GoToPosition(0, 1);
      //// Here is printed string
      //lcd.PrintString("gentlemans!!!1");

      // Here is printed our new symbol (emoticon)
      lcd.PrintSymbol(0x00);

      lcd.GoToPosition(0, 1);

      int x = 0;

      while (true) {
        ReadADC();
        LightLED();

        Task.Delay(1000).Wait();
        lcd.GoToPosition(0, 1);
        lcd.PrintString(x++.ToString());
      }
    }

    private async Task InitAll() {
      InitGpio();         /* Initialize GPIO to toggle the LED                          */
      await InitSPI();    /* Initialize the SPI bus for communicating with the ADC      */
    }

    private async Task InitSPI() {

      var settings = new SpiConnectionSettings(SPI_CHIP_SELECT_LINE);
      settings.ClockFrequency = 500000;   /* 0.5MHz clock rate                                        */
      settings.Mode = SpiMode.Mode0;      /* The ADC expects idle-low clock polarity so we use Mode0  */

      string spiAqs = SpiDevice.GetDeviceSelector(SPI_CONTROLLER_NAME);
      var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);
      SpiADC = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);

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

    /* Turn on/off the LED depending on the potentiometer position    */
    private void LightLED() {
      int adcResolution = 1024;

      /* Turn on LED if pot is rotated more halfway through its range */
      if (adcValue > adcResolution / 2) {
        ledPin.Write(GpioPinValue.Low);
      }
      /* Otherwise turn it off                                        */
      else {
        ledPin.Write(GpioPinValue.High);
      }
    }

    public void ReadADC() {
      var channel = 0;
      byte[] readBuffer = new byte[3]; /* Buffer to hold read data*/
      byte[] writeBuffer = new byte[3] { 1, (byte)(8 + channel << 4), 0 };
      
      SpiADC.TransferFullDuplex(writeBuffer, readBuffer); /* Read data from the ADC                           */
      adcValue = convertToInt(readBuffer);

      System.Diagnostics.Debug.WriteLine($"{ Math.Round(adcValue/102.4, 0, MidpointRounding.ToEven)}");
    }

    public int convertToInt([ReadOnlyArray] byte[] data) {
      int result = 0;
      result = data[1] & 0x03;
      result <<= 8;
      result += data[2];
      return result;
    }
  }
}
