using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.Core;
using System.Threading;

namespace Radio.Lib.Radio {
  public sealed class RadioViewModel : IRadioViewModel {

    private bool _is_disposed = false;
    
    public RadioViewModel() {

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
      */


    }


    private MediaPlaybackItem CreatePlaybackItem(string name, string uri) {
      var source = MediaSource.CreateFromUri(new Uri(uri));
      source.CustomProperties["Name"] = name;
      return new MediaPlaybackItem(source);
    }
    
    
    void Dispose(bool disposing) {
      if (!_is_disposed) {
        if (disposing) {
          // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
        }
        _is_disposed = true;
      }
    }

    public void Dispose() {
      Dispose(true);
    }
  }
}
