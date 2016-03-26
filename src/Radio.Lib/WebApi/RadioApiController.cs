using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using System;

namespace Radio.Lib.WebApi {
  
  [RestController(InstanceCreationType.Singleton)]
  public sealed class RadioApiController {

    private readonly string _test;

    public RadioApiController(string test) {
      _test = test;
    }

    [UriFormat("/radio")]
    public GetResponse GetWithSimpleParameters()
      => new GetResponse(GetResponse.ResponseStatus.OK, _test);
  }
}
