using System;

namespace Radio.Lib.Input {
  public interface IPushButton : IDisposable {
    int Id { get; }
    IObservable<bool> StateStream { get; }
    TimeSpan DebounceTimeout { get; }
  }
}
