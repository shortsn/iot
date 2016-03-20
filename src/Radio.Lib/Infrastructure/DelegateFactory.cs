using System;

namespace Radio.Lib.Infrastructure {
  public sealed class DelegateFactory<TService> : IFactory<TService> {

    private readonly Func<TService> _factory_method;

    public DelegateFactory(Func<TService> factory_method) {
      _factory_method = factory_method;
    }

    public TService Create()
      => _factory_method();

  }
}
