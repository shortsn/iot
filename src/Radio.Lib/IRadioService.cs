using System;
using System.Threading.Tasks;

namespace Radio.Lib {
  public interface IRadioService : IDisposable {
    Task RunServiceAsync();
    void StopService();
  }
}
