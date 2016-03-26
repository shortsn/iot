using Devkoes.Restup.WebServer.File;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Radio.Lib.WebApi {
  public sealed class EmbeddedResourceFile : IFile {

    private readonly Assembly _assembly;
    private readonly string _resource_path;

    public string ContentType { get; }

    public EmbeddedResourceFile(Assembly assembly, string relative_resource_path) {
      _assembly = assembly;
      _resource_path = string.Concat(relative_resource_path);
      var extension = Path.GetExtension(relative_resource_path);
      ContentType = MimeTypeHelper.Mapping[extension];
    }

    public Task<Stream> OpenStreamForReadAsync()
      => Task.FromResult(_assembly.GetManifestResourceStream(_resource_path));
  }
}
