using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Http;
using Devkoes.Restup.WebServer.Models.Schemas;
using Devkoes.Restup.WebServer.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Vorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 dokumentiert.

namespace App1 {
  /// <summary>
  /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
  /// </summary>
  /// 

  [RestController(InstanceCreationType.Singleton)]
  public class ParameterController {
    public class DataReceived {
      public int ID { get; set; }
      public string PropName { get; set; }
    }

    [UriFormat("/simpleparameter/{id}/property/{propName}")]
    public GetResponse GetWithSimpleParameters(int id, string propName) {
      return new GetResponse(
          GetResponse.ResponseStatus.OK,
          new DataReceived() { ID = id, PropName = propName });
    }
  }

  public sealed partial class MainPage : Page {
    public MainPage() {
      this.InitializeComponent();


      var myClient = new HttpClient();
      var myRequest = new HttpRequestMessage(HttpMethod.Get, "http://streams.br.de/bayern1_2.m3u");
      var foo = new StreamReader(myClient.GetStreamAsync("http://streams.br.de/bayern1_2.m3u").Result).ReadToEnd();

      var response = myClient.SendAsync(myRequest).Result;

      var restRouteHandler = new RestRouteHandler();
      restRouteHandler.RegisterController<ParameterController>();

      var httpServer = new HttpServer(8800);
      httpServer.RegisterRoute("api", restRouteHandler);
      httpServer.StartServerAsync().Wait();

      var playbacklist = new MediaPlaybackList();
      playbacklist.Items.Add(CreatePlaybackItem("Fritz", @"http://fritz.de/livemp3"));
      playbacklist.Items.Add(CreatePlaybackItem("live", @"http://mp3.planetradio.de/planetradio/hqlivestream.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("itunes top 40", @"http://mp3.planetradio.de/plrchannels/hqitunes.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("the club", @"http://mp3.planetradio.de/plrchannels/hqtheclub.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("night wax", @"http://mp3.planetradio.de/plrchannels/hqnightwax.mp3"));
      playbacklist.Items.Add(CreatePlaybackItem("black beats", @"http://mp3.planetradio.de/plrchannels/hqblackbeats.mp3"));

      var media_player = BackgroundMediaPlayer.Current;
      media_player.AutoPlay = true;
      media_player.Source = playbacklist;

    }

    private MediaPlaybackItem CreatePlaybackItem(string name, string uri) {
      var source = MediaSource.CreateFromUri(new Uri(uri));
      source.CustomProperties["Name"] = name;
      return new MediaPlaybackItem(source);
    }
  }
}
