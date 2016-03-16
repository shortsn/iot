using DryIoc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.AnalogDigitalConverter;
using TestApp.Display;
using TestApp.ShiftRegister;

namespace TestApp {
  internal static class Bootstrapper {

    public static IContainer CreateContainer()
      => new Container()
          .RegisterServices();

    private static TContainer RegisterServices<TContainer>(this TContainer container) where TContainer : IRegistrator {

      container.RegisterDelegate<IDisplay>(_ => InitializeDisplay().Result, Reuse.Singleton);
      container.RegisterDelegate<IShiftRegister>(_ => SR_74HC595N.ConnectAsync().Result, Reuse.Singleton);
      container.RegisterDelegate<IAnalogDigitalConverter>(_ => ADC_MCP3008_SPI.ConnectAsync().Result, Reuse.Singleton);

      return container;
    }

    private async static Task<Display_16x2_I2C> InitializeDisplay() {
      var display = await Display_16x2_I2C.ConnectAsync().ConfigureAwait(false);
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

      return display;
    }

   

  }
}
