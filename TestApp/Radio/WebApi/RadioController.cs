using Devkoes.Restup.WebServer.Attributes;
using Devkoes.Restup.WebServer.Models.Schemas;
using System;

namespace TestApp.WebApi {
  
  [RestController(InstanceCreationType.Singleton)]
  internal sealed class RadioController {

    private readonly string _test;

    public RadioController(string test) {
      _test = test;
    }

    [UriFormat("/radio")]
    public GetResponse GetWithSimpleParameters()
      => new GetResponse(GetResponse.ResponseStatus.OK, _test);
  }
}
