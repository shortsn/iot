using System;
using System.Reactive;

namespace Radio.Lib.Radio {
  public interface IRadioApi : IDisposable {
    void Stop();
    IObservable<Unit> StopStream { get; }
  }
}
