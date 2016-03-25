using System;
using System.Reactive;

namespace Radio.Lib.Radio {
  public interface IRadioViewModel : IDisposable {
    void Stop();
    IObservable<Unit> StopStream { get; }
  }
}
