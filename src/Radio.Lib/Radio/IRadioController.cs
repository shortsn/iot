using System;
using System.Reactive;

namespace Radio.Lib.Radio {
  public interface IRadioController : IDisposable {
    void Stop();
    IObservable<Unit> StopStream { get; }
  }
}
