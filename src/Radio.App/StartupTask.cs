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

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace Radio.App
{
    public sealed class StartupTask : IBackgroundTask
    {
    BackgroundTaskDeferral _deferral;
    readonly CompositeDisposable _disposables = new CompositeDisposable();

    public async void Run(IBackgroundTaskInstance taskInstance) {
      _deferral = taskInstance.GetDeferral();


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

      var restRouteHandler = new RestRouteHandler();
      restRouteHandler.RegisterController<RadioController>("Testparameter");

      var httpServer = new HttpServer(8800);
      httpServer.RegisterRoute("api", restRouteHandler);
      httpServer.StartServerAsync().Wait();

      var container = Bootstrapper.CreateContainer();

      var radio = container.Resolve<IRadioViewModel>();


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

      //var controller = GpioController.GetDefaultAsync().AsTask().Result;
      //_button = controller.OpenPin(21);

      //if (_button.IsDriveModeSupported(GpioPinDriveMode.InputPullUp)) {
      //  _button.SetDriveMode(GpioPinDriveMode.InputPullUp);
      //} else {
      //  _button.SetDriveMode(GpioPinDriveMode.Input);
      //}
      //_button.DebounceTimeout = TimeSpan.FromMilliseconds(50);
      //_button.ValueChanged += (pin, args) => {
      //  if (args.Edge == GpioPinEdge.RisingEdge)
      //  if (media_player.CurrentState == MediaPlayerState.Playing)
      //    media_player.Pause();
      //  else
      //    media_player.Play();
      //};


      var display = await container.Resolve<IFactory<IDisplay>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(display);
      var shift_register = await container.Resolve<IFactory<IShiftRegister>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(shift_register);
      var ad_converter = await container.Resolve<IFactory<IAnalogDigitalConverter>>().CreateAsync().ConfigureAwait(false);
      _disposables.Add(ad_converter);

      //display.PrintSymbol(0x00);

      var sequence = new byte[] { 0, 1, 3, 7, 15, 31 };


      ad_converter
        .MonitorPorts(TimeSpan.FromMilliseconds(250), 0, 1, 2, 3, 4)
        .MapAndDistinct(value => {
          switch (value.Port) {
            case 0:
              return new PortValue<double>(value.Port, Math.Round(value.Value / 1024d, 1, MidpointRounding.AwayFromZero));
            case 3:
              return new PortValue<double>(value.Port, Math.Round(value.Value / 204.8d, 0, MidpointRounding.AwayFromZero));
            default:
              return new PortValue<double>(value.Port, Math.Round(value.Value / 102.4d, 0, MidpointRounding.AwayFromZero));
          }
        })
        .Subscribe(value => {
          Debug.WriteLine($"Port: {value.Port} Value: {value.Value}");

          switch (value.Port) {
            case 0:
              display.PrintLine(1, $"Volume: {value.Value.ToString("P0")}");
              media_player.Volume = value.Value;
              break;
            case 1:
              if (value.Value == 10) {
                try {
                  Debug.WriteLine("Shutdown");
                  _disposables.Dispose();
                } finally {
                  _deferral.Complete();
                }
              } else
                media_player.Play();
              break;
            case 3:
              var index = (uint)value.Value;
              shift_register.SendByte(sequence[index]);
              playbacklist.MoveTo(index);
              var item = playbacklist.Items[(int)index];
              display.PrintLine(0, item.Source.CustomProperties["Name"].ToString());
              break;
          }
        },
        ex => Debug.WriteLine(ex)
       );
    }

    private MediaPlaybackItem CreatePlaybackItem(string name, string uri) {
      var source = MediaSource.CreateFromUri(new Uri(uri));
      source.CustomProperties["Name"] = name;
      return new MediaPlaybackItem(source);
    }
  }
}
