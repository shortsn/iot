using Devkoes.Restup.WebServer.File;
using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Rest;
using Radio.Lib.Infrastructure;
using Radio.Lib.Radio;
using Radio.Lib.WebApi;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;

namespace Radio.Lib {
  public sealed class RadioService : IRadioService {

    private bool _disposed = false;

    private CompositeDisposable _disposables = new CompositeDisposable();
    private readonly Lazy<Task> _initializer;
    private readonly Subject<Unit> _stop_subject = new Subject<Unit>();
    
    public RadioService(IRadioController model, IFactory<HttpServer> webserver_factory) {
      _disposables.Add(_stop_subject);
      _initializer = new Lazy<Task>(() => InitializeServiceAsync(model, webserver_factory), true);
    }

    private async Task InitializeServiceAsync(IRadioController model, IFactory<HttpServer> webserver_factory) {
      var webserver = await webserver_factory.CreateAsync().ConfigureAwait(false);

      var route_handler = new RestRouteHandler();
      route_handler.RegisterController<RadioApiController>(model);

      webserver.RegisterRoute(string.Empty, new StaticFileRouteHandler("Site", new WebApi.PhysicalFileSystem()));
      webserver.RegisterRoute("api", route_handler);

      _disposables.Add(model.StopStream.Do(_ => Debug.WriteLine("model stop request")).Subscribe(_ => StopService()));

      Debug.WriteLine("starting webserver");
      await webserver.StartServerAsync().ConfigureAwait(false);
      Debug.WriteLine("service started");

      await _stop_subject.FirstOrDefaultAsync();
      Debug.WriteLine("service stopping");
      webserver.StopServer();
      Debug.WriteLine("service stopped");
    }

    public Task RunServiceAsync()
      => _initializer.Value;

    public void StopService() {
      _stop_subject.OnNext(Unit.Default);
    }


    void Dispose(bool disposing) {
      if (!_disposed) {
        if (disposing) {
          _stop_subject.OnCompleted();
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
