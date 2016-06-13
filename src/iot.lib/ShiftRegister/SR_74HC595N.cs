using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace iot.lib.ShiftRegister {
  public sealed class SR_74HC595N : IShiftRegister {

    private bool _disposed = false;

    // SER
    private readonly GpioPin _serial;

    // OE
    private readonly GpioPin _output_enable;

    // SRCLK
    private readonly GpioPin _shift_register_clock;

    // RCLK
    private readonly GpioPin _register_clock;

    // SRCLR
    private readonly GpioPin _shift_register_clear;

    public SR_74HC595N(GpioPin serial, GpioPin output_enable, GpioPin register_clock, GpioPin shift_register_clock, GpioPin shift_register_clear) {
      shift_register_clock.Write(GpioPinValue.Low);
      shift_register_clock.SetDriveMode(GpioPinDriveMode.Output);

      serial.Write(GpioPinValue.Low);
      serial.SetDriveMode(GpioPinDriveMode.Output);

      register_clock.Write(GpioPinValue.Low);
      register_clock.SetDriveMode(GpioPinDriveMode.Output);

      output_enable.Write(GpioPinValue.Low);
      output_enable.SetDriveMode(GpioPinDriveMode.Output);

      shift_register_clear.Write(GpioPinValue.Low);
      shift_register_clear.SetDriveMode(GpioPinDriveMode.Output);

      register_clock.Write(GpioPinValue.High);
      register_clock.Write(GpioPinValue.Low);
      shift_register_clear.Write(GpioPinValue.High);

      _serial = serial;
      _output_enable = output_enable;
      _register_clock = register_clock;
      _shift_register_clock = shift_register_clock;
      _shift_register_clear = shift_register_clear;

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
    
    public void SendByte(byte data) {
      _register_clock.Write(GpioPinValue.Low);

      for (var counter = 0; counter <= 7; counter++) {
        SendBit((~data & (0x80 >> counter)) > 0);
      }

      _register_clock.Write(GpioPinValue.High);
    }

    private void SendBit(bool high) {
      _serial.Write(high ? GpioPinValue.High : GpioPinValue.Low);
      _shift_register_clock.Write(GpioPinValue.High);
      _shift_register_clock.Write(GpioPinValue.Low);
    }

    private void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          _serial.Dispose();
          _output_enable.Dispose();
          _register_clock.Dispose();
          _shift_register_clock.Dispose();
          _shift_register_clear.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
