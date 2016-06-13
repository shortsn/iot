using System;
using System.Collections.Generic;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Diagnostics;
using iot.lib.Input;
using Radio.Lib.Infrastructure;
using iot.lib.Display;
using iot.lib.ShiftRegister;
using iot.lib.AnalogDigitalConverter;
using System.Threading.Tasks;

namespace Radio.Lib.Radio {
  public sealed class RadioController : IRadioApi {

    private bool _is_disposed = false;

    private IReadOnlyDictionary<int, IPushButton> _push_buttons;
    private IDisplay _display;
    private IShiftRegister _shift_register;
    private IAnalogDigitalConverter _analog_digital_converter;

    private MediaPlayer _mediaplayer;
    private MediaPlaybackList _playlist;

    private readonly Subject<Unit> _stop_subject = new Subject<Unit>();

    public IObservable<Unit> StopStream => _stop_subject.AsObservable();

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

    public RadioController(IFactory<IReadOnlyDictionary<int, IPushButton>> button_factory, IFactory<IDisplay> display_factory, IFactory<IShiftRegister> shift_register_factory, IFactory<IAnalogDigitalConverter> ad_converter_factory) {
      // active subscriptions should be destroyed first
      _disposables.Add(_subscriptions);
      _disposables.Add(_stop_subject);
      InitializeAsync(button_factory, display_factory, shift_register_factory, ad_converter_factory).GetAwaiter().GetResult();
      CreateSubscriptions();
    }

    private async Task InitializeAsync(IFactory<IReadOnlyDictionary<int, IPushButton>> button_factory, IFactory<IDisplay> display_factory, IFactory<IShiftRegister> shift_register_factory, IFactory<IAnalogDigitalConverter> ad_converter_factory) {
      _mediaplayer = BackgroundMediaPlayer.Current;
      _playlist = new MediaPlaybackList();

      InitPlaylist(_playlist);

      _mediaplayer.AutoPlay = false;
      _mediaplayer.Source = _playlist;

      _push_buttons = await button_factory.CreateAsync().ConfigureAwait(false);

      foreach (var button in _push_buttons.Values) {
        _disposables.Add(button);
      }

      _display = await display_factory.CreateAsync().ConfigureAwait(false);

      _disposables.Add(_display);
      //display.PrintSymbol(0x00);

      _shift_register = await shift_register_factory.CreateAsync().ConfigureAwait(false);
      _disposables.Add(_shift_register);

      _analog_digital_converter = await ad_converter_factory.CreateAsync().ConfigureAwait(false);
      _disposables.Add(_analog_digital_converter);
    }

    private void CreateSubscriptions() {
      foreach (var button in _push_buttons.Values) {
        _subscriptions.Add(
          button
            .StateStream
            .Subscribe(state => {
              Debug.WriteLine($"Button changed {state}");
            })
        );
      }

      var sequence = new byte[] { 0, 1, 3, 7, 15, 31 };

      _subscriptions.Add(_analog_digital_converter
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
              _display.PrintLine(1, $"Volume: {value.Value.ToString("P0")}");
              _mediaplayer.Volume = value.Value;
              break;
            case 1:
              if (value.Value == 10) {
                Stop();
              } else
                _mediaplayer.Play();
              break;
            case 3:
              var index = (uint)value.Value;
              _shift_register.SendByte(sequence[index]);
              _playlist.MoveTo(index);
              var item = _playlist.Items[(int)index];
              _display.PrintLine(0, item.Source.CustomProperties["Name"].ToString());
              break;
          }
        },
        ex => Debug.WriteLine(ex)
       ));
    }

    private static void InitPlaylist(MediaPlaybackList playbacklist) {
      playbacklist.Items.Add(CreatePlaybackItem("Fritz", @"http://fritz.de/livemp3"));
      playbacklist.Items.Add(CreatePlaybackItem("live", @"http://mp3.planetradio.de/planetradio/hqlivestream.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("itunes top 40", @"http://mp3.planetradio.de/plrchannels/hqitunes.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("the club", @"http://mp3.planetradio.de/plrchannels/hqtheclub.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("night wax", @"http://mp3.planetradio.de/plrchannels/hqnightwax.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("black beats", @"http://mp3.planetradio.de/plrchannels/hqblackbeats.mp3"));
    }
    
    public void Stop() {
      _stop_subject.OnNext(Unit.Default);
    }

    private static MediaPlaybackItem CreatePlaybackItem(string name, string uri) {
      var source = MediaSource.CreateFromUri(new Uri(uri));
      source.CustomProperties["Name"] = name;
      return new MediaPlaybackItem(source);
    }
    
    void Dispose(bool disposing) {
      if (!_is_disposed) {
        if (disposing) {
          _stop_subject.OnCompleted();
          _disposables.Dispose();
        }
        _is_disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
