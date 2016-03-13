using System;
using Windows.ApplicationModel.Background;
using System.Reactive.Linq;
using Windows.Devices.Gpio;
using TestApp.AnalogDigitalConverter;
using System.Reactive.Disposables;

using Windows.Media.Playback;
using Windows.Media.Core;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {

    BackgroundTaskDeferral _deferral;
    readonly CompositeDisposable _disposables = new CompositeDisposable();
    
    public void Run(IBackgroundTaskInstance taskInstance) {
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

      var buttons = new[] { 21, 20, 16, 26, 19, 13 };

      Task
        .WhenAll(buttons.Select(PushButton.ConnectAsync)).Result.ToList()
        .ForEach(button => {
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

      var shift_register = SR_74HC595N.ConnectAsync().Result;
      _disposables.Add(shift_register);
      var mcp3008 = ADC_MCP3008_SPI.ConnectAsync().Result;
      _disposables.Add(mcp3008);
      var display = Display_16x2_I2C.ConnectAsync().Result;
      _disposables.Add(display);
      display.Initialize();

      // Here is created new symbol
      // Take a look at data - it's smile emoticon
      // 0x00 => 00000
      // 0x00 => 00000
      // 0x0A => 01010
      // 0x00 => 00000
      // 0x11 => 10001
      // 0x0E => 01110
      // 0x00 => 00000
      // 0x00 => 00000 
      display.CreateChar(0x00, new byte[] { 0x00, 0x00, 0x0A, 0x00, 0x11, 0x0E, 0x00, 0x00 });

      //display.ClearScreen();
      //display.BacklightOff();

      //display.PrintSymbol(0x00);

      //Task.Delay(2000).Wait();
      //display.BacklightOn();

      var sequence = new byte[] { 0, 1, 3, 7, 15, 31 };
      
      
      mcp3008
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