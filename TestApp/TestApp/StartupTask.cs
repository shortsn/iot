using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
using Microsoft.IoT.Devices.Adc;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {
    //Setup address
    private const string I2C_CONTROLLER_NAME = "I2C1"; //use for RPI2
    private const byte DEVICE_I2C_ADDRESS = 0x3F; // 7-bit I2C address of the port expander

    //Setup pins
    private const byte EN = 0x02;
    private const byte RW = 0x01;
    private const byte RS = 0x00;
    private const byte D4 = 0x04;
    private const byte D5 = 0x05;
    private const byte D6 = 0x06;
    private const byte D7 = 0x07;
    private const byte BL = 0x03;


    private const int LED_PIN = 4; // Use pin 12 if you are using DragonBoard
    private GpioPin ledPin;

    private const string SPI_CONTROLLER_NAME = "SPI0";  /* Friendly name for Raspberry Pi 2 SPI controller          */
    private const int SPI_CHIP_SELECT_LINE = 0;       /* Line 0 maps to physical pin number 24 on the Rpi2        */
    private SpiDevice SpiADC;

    private int adcValue;

    public void Run(IBackgroundTaskInstance taskInstance) {

      using (var adc = new MCP3008 { ChipSelectLine = SPI_CHIP_SELECT_LINE, ControllerName = SPI_CONTROLLER_NAME }) {
        adc.AcquireChannel(0);
        var value = adc.ReadValue(0);
      }

      InitAll().Wait();

      // Here is I2C bus and Display itself initialized.
      //
      //  I2C bus is initialized by library constructor. There is also defined PCF8574 pins 
      //  Default `DEVICE_I2C_ADDRESS` is `0x27` (you can change it by A0-2 pins on PCF8574 - for more info please read datasheet)
      //  `I2C_CONTROLLER_NAME` for Raspberry Pi 2 is `"I2C1"`
      //  For Arduino it should be `"I2C5"`, but I did't test it.
      //  Other arguments should be: RS = 0, RW = 1, EN = 2, D4 = 4, D5 = 5, D6 = 6, D7 = 7, BL = 3
      //  But it depends on your PCF8574.
      displayI2C lcd = new displayI2C(DEVICE_I2C_ADDRESS, I2C_CONTROLLER_NAME, RS, RW, EN, D4, D5, D6, D7, BL);

      //Initialization of HD44780 display do by init method.
      //By arguments you can turnOnDisplay, turnOnCursor, blinkCursor, cursorDirection and textShift (in thius order)
      lcd.init();


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
      lcd.createSymbol(new byte[] { 0x00, 0x00, 0x0A, 0x00, 0x11, 0x0E, 0x00, 0x00 }, 0x00);

      // Here is printed string
      lcd.prints("Good morning,");

      // Navigation to second line
      lcd.gotoxy(0, 1);
      // Here is printed string
      lcd.prints("gentlemans!!!1");

      // Here is printed our new symbol (emoticon)
      lcd.printSymbol(0x00);

      lcd.gotoxy(0, 1);
      lcd.prints("                ");

      int x = 0;

      while (true) {
        ReadADC();
        LightLED();

        Task.Delay(1000).Wait();
        lcd.gotoxy(0, 1);
        lcd.prints(x++.ToString());
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
