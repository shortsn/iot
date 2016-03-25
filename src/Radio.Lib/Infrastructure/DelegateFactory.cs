using System;
using System.Threading;
using System.Threading.Tasks;

namespace Radio.Lib.Infrastructure {
  public sealed class DelegateFactory<TService> : IFactory<TService> {
    private readonly Func<CancellationToken, Task<TService>> _factory_method;

    public DelegateFactory(Func<CancellationToken, Task<TService>> factory_method) {
      _factory_method = factory_method;
    }

    public Task<TService> CreateAsync(CancellationToken cancellation_token = default(CancellationToken))
      => _factory_method(cancellation_token);
  }
}
