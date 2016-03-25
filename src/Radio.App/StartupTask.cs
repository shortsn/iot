using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Media.Playback;
using Radio.Lib.Infrastructure;
using Radio.Lib.Input;
using DryIoc;
using Windows.Media.Core;
using Radio.Lib.Display;
using Radio.Lib.ShiftRegister;
using Radio.Lib.AnalogDigitalConverter;
using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Rest;
using Radio.Lib.WebApi;
using System.Reactive.Disposables;
using System.Diagnostics;
using Radio.Lib.Radio;
using System.Reactive.Linq;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Radio.App
{
    public sealed class StartupTask : IBackgroundTask {

    public async void Run(IBackgroundTaskInstance taskInstance) {
      var deferral = taskInstance.GetDeferral();
      try {
        using (var container = Bootstrapper.CreateContainer()) {
          using (var radio = container.Resolve<IRadioViewModel>()) {
            await radio.StopStream.FirstAsync();
          }
        }
      } finally {
        deferral.Complete();
      }
      
      //var restRouteHandler = new RestRouteHandler();
      //restRouteHandler.RegisterController<RadioController>("Testparameter");

      //var httpServer = new HttpServer(8800);
      //httpServer.RegisterRoute("api", restRouteHandler);
      //httpServer.StartServerAsync().Wait();

    }
  }
}
