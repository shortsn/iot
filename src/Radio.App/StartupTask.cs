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
using System.Threading.Tasks;
using System.Reactive.Linq;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Radio.App
{
    public sealed class StartupTask : IBackgroundTask
    {
    BackgroundTaskDeferral _deferral;
    readonly CompositeDisposable _disposables = new CompositeDisposable();

    public async void Run(IBackgroundTaskInstance taskInstance) {
      _deferral = taskInstance.GetDeferral();

      try {
        using (var container = Bootstrapper.CreateContainer()) {
          using (var radio = container.Resolve<IRadioViewModel>()) {
            await radio.StopStream.FirstAsync();
          }
        }
      } finally {
        _deferral.Complete();
      }


      /*

      var playbacklist = new MediaPlaybackList();
      playbacklist.Items.Add(CreatePlaybackItem("Fritz", @"http://fritz.de/livemp3"));
      playbacklist.Items.Add(CreatePlaybackItem("live", @"http://mp3.planetradio.de/planetradio/hqlivestream.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("itunes top 40", @"http://mp3.planetradio.de/plrchannels/hqitunes.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("the club", @"http://mp3.planetradio.de/plrchannels/hqtheclub.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("night wax", @"http://mp3.planetradio.de/plrchannels/hqnightwax.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("black beats", @"http://mp3.planetradio.de/plrchannels/hqblackbeats.mp3"));

      var media_player = BackgroundMediaPlayer.Current;
      media_player.AutoPlay = false;
      media_player.Source = playbacklist;

      //var restRouteHandler = new RestRouteHandler();
      //restRouteHandler.RegisterController<RadioController>("Testparameter");

      //var httpServer = new HttpServer(8800);
      //httpServer.RegisterRoute("api", restRouteHandler);
      //httpServer.StartServerAsync().Wait();

  
      _disposables.Add(container);

      var buttons = await container.Resolve<IFactory<IReadOnlyDictionary<int, IPushButton>>>().CreateAsync().ConfigureAwait(false);
      buttons
        .Values
        .ToList()
        .ForEach(button => {
          _disposables.Add(button);
          _disposables.Add(
              button
                .StateStream
                .Subscribe(state => {
                  Debug.WriteLine($"Button changed {state}");
                })
            );
        });

      var display = await container.Resolve<IFactory<IDisplay>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(display);
      var shift_register = await container.Resolve<IFactory<IShiftRegister>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(shift_register);
      var ad_converter = await container.Resolve<IFactory<IAnalogDigitalConverter>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(ad_converter);

      //display.PrintSymbol(0x00);

      


      

  */
    }
  }
}
