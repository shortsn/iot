using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;

namespace TestApp {
  internal sealed class PushButton : IDisposable {

    private bool _disposed = false;

    private readonly CompositeDisposable _disposables = new CompositeDisposable();

    private readonly GpioPin _button_pin;
    private readonly BehaviorSubject<bool> _button_state;

    public TimeSpan DebounceTimeout => _button_pin.DebounceTimeout;

    public IObservable<bool> StateStream { get; }

    public PushButton(GpioPin button_pin, TimeSpan debounce_timeout) {
      InitializeButtonPin(button_pin, debounce_timeout);

      _button_pin = button_pin;
      _disposables.Add(_button_pin);

      _button_state = new BehaviorSubject<bool>(button_pin.Read().Equals(GpioPinValue.Low));
      _disposables.Add(_button_state);
      _disposables.Add(
          Observable
          .FromEventPattern<TypedEventHandler<GpioPin, GpioPinValueChangedEventArgs>, GpioPinValueChangedEventArgs>(handler => _button_pin.ValueChanged += handler, handler => _button_pin.ValueChanged -= handler)
          .Subscribe(args => _button_state.OnNext(args.EventArgs.Edge.Equals(GpioPinEdge.FallingEdge)))
        );

      StateStream = _button_state.AsObservable();
    }

    public async static Task<PushButton> ConnectAsync(int button_gpio) {
      var controller = await GpioController.GetDefaultAsync();
      return new PushButton(controller.OpenPin(button_gpio), TimeSpan.FromMilliseconds(50));
    }

    private static void InitializeButtonPin(GpioPin gpio, TimeSpan debounce_timeout) {
      var drive_mode = GpioPinDriveMode.InputPullUp;
      if (!gpio.IsDriveModeSupported(drive_mode)) {
        throw new NotSupportedException($"DriveModeSupported {drive_mode} it not supported. Pin {gpio.PinNumber}");
      }
      gpio.SetDriveMode(GpioPinDriveMode.InputPullUp);
      gpio.DebounceTimeout = debounce_timeout;
    }

    private void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          _disposables.Dispose();
        }
        _disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }
  }
}
