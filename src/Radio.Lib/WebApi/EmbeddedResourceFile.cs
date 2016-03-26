using Devkoes.Restup.WebServer.File;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System;
using System.Linq;

namespace Radio.Lib.WebApi {
  public sealed class EmbeddedResourceFile : IFile {

    private readonly Assembly _assembly;
    private readonly string _resource_path;

    public string ContentType { get; }

    public bool Exists { get; }

    public EmbeddedResourceFile(Assembly assembly, string relative_resource_path) {
      _assembly = assembly;
      _resource_path = string.Concat(relative_resource_path);
      var extension = Path.GetExtension(relative_resource_path);
      ContentType = MimeTypeHelper.Mapping[extension];
      Exists = _assembly.GetManifestResourceNames().Any(name => name.Equals(_resource_path));
    }

    public Task<Stream> OpenStreamForReadAsync() {
      if (!Exists) {
        return Task.FromException<Stream>(new FileNotFoundException($"Embedded Resource {_resource_path} does not exist."));
      }
      
      try {
        var stream = _assembly.GetManifestResourceStream(_resource_path);
        return Task.FromResult(stream);
      } catch (Exception ex) {
        return Task.FromException<Stream>(ex);
      }
    }
      
  }
}
