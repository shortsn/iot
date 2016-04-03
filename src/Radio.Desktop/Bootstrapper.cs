using DryIoc;
using System;
using System.Threading.Tasks;
using Radio.Lib.Infrastructure;
using System.Threading;
using Radio.Lib.Radio;
using Radio.Lib;
using Devkoes.Restup.WebServer.Http;

namespace Radio.App {
  internal static class Bootstrapper {

    public static IContainer CreateContainer()
      => new Container().RegisterServices();

    private static TContainer RegisterServices<TContainer>(this TContainer container) where TContainer : IRegistrator {

      container.Register<IRadioApi, MediaPlayerApi>(Reuse.Singleton);
      container.Register<IRadioService, RadioService>(Reuse.Singleton);
      
      container.RegisterFactory(_ => Task.FromResult(new HttpServer(80)));

      return container;
    }

    private static void RegisterFactory<TService>(this IRegistrator container, Func<CancellationToken, Task<TService>> factory_method) 
      => container.RegisterDelegate<IFactory<TService>>(_ => new DelegateFactory<TService>(factory_method), Reuse.Singleton);

  }
}
