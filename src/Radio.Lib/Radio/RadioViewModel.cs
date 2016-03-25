using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Threading;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reactive.Threading.Tasks;
using Radio.Lib.Input;
using Radio.Lib.Infrastructure;
using Radio.Lib.Display;
using Radio.Lib.ShiftRegister;
using Radio.Lib.AnalogDigitalConverter;

namespace Radio.Lib.Radio {
  public sealed class RadioViewModel : IRadioViewModel {

    private bool _is_disposed = false;

    IReadOnlyDictionary<int, IPushButton> PushButtons { get; }
    IDisplay Display { get; }
    IShiftRegister ShiftRegister { get; }
    IAnalogDigitalConverter AnalogDigitalConverter { get; }

    MediaPlayer Mediaplayer { get; }
    MediaPlaybackList Playlist { get; }

    private readonly Subject<Unit> _stop_subject = new Subject<Unit>();
    public IObservable<Unit> StopStream => _stop_subject.AsObservable();

    CompositeDisposable Disposables { get; }
    
    public RadioViewModel(IFactory<IReadOnlyDictionary<int, IPushButton>> button_factory, IFactory<IDisplay> display_factory, IFactory<IShiftRegister> shift_register_factory, IFactory<IAnalogDigitalConverter> ad_converter_factory) {
      Mediaplayer = BackgroundMediaPlayer.Current;
      Playlist = new MediaPlaybackList();

      InitPlaylist(Playlist);

      Mediaplayer.AutoPlay = false;
      Mediaplayer.Source = Playlist;

      Disposables = new CompositeDisposable();

      Disposables.Add(_stop_subject);

      PushButtons = button_factory.CreateAsync().GetAwaiter().GetResult();

      foreach (var button in PushButtons.Values) {
        Disposables.Add(button);
      }

      Display = display_factory.CreateAsync().GetAwaiter().GetResult();
      Disposables.Add(Display);
      //display.PrintSymbol(0x00);

      ShiftRegister = shift_register_factory.CreateAsync().GetAwaiter().GetResult();
      Disposables.Add(ShiftRegister);

      AnalogDigitalConverter = ad_converter_factory.CreateAsync().GetAwaiter().GetResult();
      Disposables.Add(AnalogDigitalConverter);

      InitializeSubscriptions();
    }

    private void InitializeSubscriptions() {

      foreach (var button in PushButtons.Values) {
        Disposables.Add(
          button
            .StateStream
            .Subscribe(state => {
              Debug.WriteLine($"Button changed {state}");
            })
        );
      }

      var sequence = new byte[] { 0, 1, 3, 7, 15, 31 };

      Disposables.Add(AnalogDigitalConverter
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
              Display.PrintLine(1, $"Volume: {value.Value.ToString("P0")}");
              Mediaplayer.Volume = value.Value;
              break;
            case 1:
              if (value.Value == 10) {
                Stop();
              } else
                Mediaplayer.Play();
              break;
            case 3:
              var index = (uint)value.Value;
              ShiftRegister.SendByte(sequence[index]);
              Playlist.MoveTo(index);
              var item = Playlist.Items[(int)index];
              Display.PrintLine(0, item.Source.CustomProperties["Name"].ToString());
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
          Disposables.Dispose();
        }
        _is_disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }

  }
}
