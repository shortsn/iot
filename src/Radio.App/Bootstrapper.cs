using DryIoc;
using System;
using System.Threading.Tasks;
using Radio.Lib.Display;
using Radio.Lib.ShiftRegister;
using Radio.Lib.AnalogDigitalConverter;
using Radio.Lib.Infrastructure;
using Radio.Lib.Input;
using System.Collections.Generic;
using System.Linq;

namespace Radio.App {
  internal static class Bootstrapper {

    public static IContainer CreateContainer()
      => new Container().RegisterServices();

    private static TContainer RegisterServices<TContainer>(this TContainer container) where TContainer : IRegistrator {
      container.RegisterFactory<IDisplay>(() => InitializeDisplay().Result);
      container.RegisterFactory<IShiftRegister>(() => SR_74HC595N.ConnectAsync().Result);
      container.RegisterFactory<IAnalogDigitalConverter>(() => ADC_MCP3008_SPI.ConnectAsync().Result);
      container.RegisterFactory<IReadOnlyDictionary<int, IPushButton>>(() => InitializeButtons().Result);

      return container;
    }

    private async static Task<Dictionary<int, IPushButton>> InitializeButtons() {
      var button_pins = new[] { 21, 20, 16, 26, 19, 13 };
      IPushButton[] buttons = await Task.WhenAll(button_pins.Select(PushButton.ConnectAsync)).ConfigureAwait(false);
      return buttons.ToDictionary(b => b.Id);
    }

    private static void RegisterFactory<TService>(this IRegistrator container, Func<TService> factory_method) 
      => container.RegisterDelegate<IFactory<TService>>(_ => new DelegateFactory<TService>(factory_method), Reuse.Singleton);

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
