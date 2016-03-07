using System;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Microsoft.Maker.Media.UniversalMediaEngine;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {
    
    private const int LED_PIN = 4;
    private GpioPin ledPin;

    BackgroundTaskDeferral deferral;

    public void Run(IBackgroundTaskInstance taskInstance) {
      //InitGpio();
      deferral = taskInstance.GetDeferral();

      var mediaengine = new MediaEngine();
      var result = mediaengine.InitializeAsync().AsTask().Result;
      
      mediaengine.MediaStateChanged += Mediaengine_MediaStateChanged;

      mediaengine.Play(@"http://mp3.planetradio.de/planetradio/hqlivestream.mp3");
     
      using (var shift_register = SR_74HC595N.ConnectAsync().Result) {
        using (var mcp3008 = ADC_MCP3008.ConnectAsync().Result) {
          using (var display = DisplayI2C.ConnectAsync().Result) {
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
            display.CreateChar(0x00, new byte[] { 0x00, 0x00, 0x0A, 0x00, 0x11, 0x0E, 0x00, 0x00 });
            
            //display.ClearScreen();
            //display.BacklightOff();

            //display.PrintSymbol(0x00);

            //Task.Delay(2000).Wait();
            //display.BacklightOn();


            var sequence = new byte[] { 0, 1, 3, 7, 15, 31 };

            var channel0 = 0;
            var channel1 = 0;

            var counter = 0;

            while ((channel0 + channel1) < 110) {
              display.SetCursor(0, 0);
              channel0 = Convert.ToInt16(mcp3008.ReadValue(0) / 102.4);
              display.PrintString($"Channel 1:{ channel0 }  ");

              //LightLED(value);

              //if (counter >= sequence.Length) {
              //  counter = 0;
              //}

              var index = channel0 / 2;
              //System.Diagnostics.Debug.WriteLine(index);
              shift_register.SendByte(sequence[index]);

              display.SetCursor(0, 1);
              channel1 = Convert.ToInt16(mcp3008.ReadValue(1) / 10.24);
              display.PrintString($"Channel 2:{ channel1 }  ");

              var volume = channel1 / 100d;
              mediaengine.Volume = volume;

              Task.Delay(250).Wait();
            }

            deferral.Complete();
          }
        }
      }
    }

    private void Mediaengine_MediaStateChanged(MediaState state) {
      System.Diagnostics.Debug.WriteLine(state.ToString());
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
