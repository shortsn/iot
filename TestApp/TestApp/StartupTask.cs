using System;
using Windows.ApplicationModel.Background;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Windows.Devices.Gpio;
using Microsoft.Maker.Media.UniversalMediaEngine;
using System.Collections.Generic;
using TestApp.AnalogDigitalConverter;
using System.Threading;
using System.Reactive.Disposables;

namespace TestApp {
  public sealed class StartupTask : IBackgroundTask {

    BackgroundTaskDeferral _deferral;
    readonly CompositeDisposable _disposables = new CompositeDisposable();

    public void Run(IBackgroundTaskInstance taskInstance) {
      _deferral = taskInstance.GetDeferral();

      var mediaengine = new MediaEngine();
      var result = mediaengine.InitializeAsync().AsTask().Result;

      mediaengine.MediaStateChanged += Mediaengine_MediaStateChanged;

      var radio_stations = new Tuple<string, string>[] {
        Tuple.Create("fritz", @"http://fritz.de/livemp3"),
        Tuple.Create("planet radio", @"http://mp3.planetradio.de/planetradio/hqlivestream.mp3"),
        Tuple.Create("itunes hot 40", @"http://mp3.planetradio.de/plrchannels/hqitunes.mp3"),
        Tuple.Create("the club", @"http://mp3.planetradio.de/plrchannels/hqtheclub.mp3"),
        Tuple.Create("nightwax", @"http://mp3.planetradio.de/plrchannels/hqnightwax.mp3"),
        Tuple.Create("black beats", @"http://mp3.planetradio.de/plrchannels/hqblackbeats.mp3" ),
      };

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
          System.Diagnostics.Debug.WriteLine($"Port: {value.Port} Value: {value.Value}");

          switch (value.Port) {
            case 0:
              display.PrintLine(1, $"Volume: {value.Value.ToString("P0")}");
              mediaengine.Volume = value.Value;
              break;
            case 1:
              if (value.Value == 10) {
                try {
                  System.Diagnostics.Debug.WriteLine("Shutdown");
                  _disposables.Dispose();
                } finally {
                  _deferral.Complete();
                }
              }
              break;
            case 3:
              var index = (int)value.Value;
              shift_register.SendByte(sequence[index]);
              var radio_station = radio_stations[index];
              mediaengine.Play(radio_station.Item2);
              display.PrintLine(0, radio_station.Item1);
              break;
          }
        }, 
        ex => System.Diagnostics.Debug.WriteLine(ex)
       );

      //while ((channel0 + channel1) < 110) {

      //  channel0 = Convert.ToInt16(mcp3008.ReadValue(0) / 102.4);
      //  var index = channel0 / 2;
      //  //System.Diagnostics.Debug.WriteLine(index);
      //  shift_register.SendByte(sequence[index]);

      //  var next_radio_station = radio_stations[index];

      //  if (next_radio_station != current_radio_station) {
      //    current_radio_station = next_radio_station;

      //  }

      //  channel1 = Convert.ToInt16(mcp3008.ReadValue(1) / 10.24);
      //  display.PrintLine(1, $"Channel 2:{ channel1 }");

      //  var volume = channel1 / 100d;
      //  mediaengine.Volume = volume;

      //  Task.Delay(250).Wait();
      //}


    }

    private void Mediaengine_MediaStateChanged(MediaState state) {
      System.Diagnostics.Debug.WriteLine(state.ToString());
    }

  }
}