using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using Radio.Lib.Radio;
using System;

namespace Radio.Lib.WebApi {
  
  [RestController(InstanceCreationType.Singleton)]
  public sealed class RadioApiController {

    private readonly IRadioController _model;

    public RadioApiController(IRadioController model) {
      _model = model;
    }

    [UriFormat("/radio")]
    public GetResponse GETRadio()
      => new GetResponse(GetResponse.ResponseStatus.OK, "foobar");

    [UriFormat("/foobar")]
    public GetResponse GET_foobar() {
      _model.Stop();
      return new GetResponse(GetResponse.ResponseStatus.OK, "stopped");
    }
  }
}
