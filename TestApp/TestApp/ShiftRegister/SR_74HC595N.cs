using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace TestApp {
  internal sealed class SR_74HC595N : IDisposable {

    private bool _disposed = false;

    // SER
    private readonly GpioPin _serial;
    
    // OE
    private readonly GpioPin _outputEnable;
    
    // SRCLK
    private readonly GpioPin _shiftRegisterClock;
    
    // RCLK
    private readonly GpioPin _registerClock;
    
    // SRCLR
    private readonly GpioPin _shiftRegisterClear;

    public SR_74HC595N(GpioPin serial, GpioPin output_enable, GpioPin register_clock, GpioPin shift_register_clock, GpioPin shift_register_clear) {
      _serial = serial;
      _outputEnable = output_enable;
      _registerClock = register_clock;
      _shiftRegisterClock = shift_register_clock;
      _shiftRegisterClear = shift_register_clear;

      _shiftRegisterClock.Write(GpioPinValue.Low);
      _shiftRegisterClock.SetDriveMode(GpioPinDriveMode.Output);

      _serial.Write(GpioPinValue.Low);
      _serial.SetDriveMode(GpioPinDriveMode.Output);

      _registerClock.Write(GpioPinValue.Low);
      _registerClock.SetDriveMode(GpioPinDriveMode.Output);

      _outputEnable.Write(GpioPinValue.Low);
      _outputEnable.SetDriveMode(GpioPinDriveMode.Output);

      _shiftRegisterClear.Write(GpioPinValue.Low);
      _shiftRegisterClear.SetDriveMode(GpioPinDriveMode.Output);

      _registerClock.Write(GpioPinValue.High);
      _registerClock.Write(GpioPinValue.Low);
      _shiftRegisterClear.Write(GpioPinValue.High);
    }

    public async static Task<SR_74HC595N> ConnectAsync(int serial_gpio = 27, int output_enable_gpio = 6, int register_clock_gpio = 5, int shift_register_clock_gpio = 18, int shift_register_clear_gpio = 12) {
      var controller = await GpioController.GetDefaultAsync();
      var serial = controller.OpenPin(serial_gpio);
      var output_enable = controller.OpenPin(output_enable_gpio);
      var register_clock = controller.OpenPin(register_clock_gpio);
      var shift_register_clock = controller.OpenPin(shift_register_clock_gpio);
      var shift_register_clear = controller.OpenPin(shift_register_clear_gpio);
      
      return new SR_74HC595N(serial, output_enable, register_clock, shift_register_clock, shift_register_clear);
    }

    private void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          _serial.Dispose();
          _outputEnable.Dispose();
          _registerClock.Dispose();
          _shiftRegisterClock.Dispose();
          _shiftRegisterClear.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
