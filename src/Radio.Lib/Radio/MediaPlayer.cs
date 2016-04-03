using System;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Radio.Lib.Radio {
  public sealed class MediaPlayerApi : IRadioApi {

    private bool _is_disposed = false;

    private readonly MediaPlayer _mediaplayer;
    private readonly MediaPlaybackList _playlist;

    private readonly Subject<Unit> _stop_subject = new Subject<Unit>();

    public IObservable<Unit> StopStream => _stop_subject.AsObservable();

    private readonly CompositeDisposable _disposables = new CompositeDisposable();
    private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

    public MediaPlayerApi() {
      // active subscriptions should be destroyed first
      _disposables.Add(_subscriptions);
      _disposables.Add(_stop_subject);

      _mediaplayer = BackgroundMediaPlayer.Current;
      _playlist = new MediaPlaybackList();

      InitPlaylist(_playlist);

      _mediaplayer.AutoPlay = true;
      _mediaplayer.Source = _playlist;


      CreateSubscriptions();
    }
    
    private void CreateSubscriptions() { }

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
