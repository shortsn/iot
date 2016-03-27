using Devkoes.Restup.WebServer.File;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Radio.Lib.WebApi {

  public class RedirectedPhysicalFileSystem : IFileSystem {

    private readonly string _relative_root_directory;
    private string _absoluteBasePathUri;

    public RedirectedPhysicalFileSystem(string relative_root_directory = "") {
      _relative_root_directory = relative_root_directory;
    }

    public bool Exists(string absoluteBasePathUri) {
      _absoluteBasePathUri = absoluteBasePathUri;
      return Directory.Exists(GetRedirectedPath(absoluteBasePathUri));
    }

    private string GetRedirectedPath(string path) 
      => path.Insert(_absoluteBasePathUri.Length, Path.DirectorySeparatorChar + _relative_root_directory);

    public async Task<IFile> GetFileFromPathAsync(string path) {
      var storageFile = await StorageFile.GetFileFromPathAsync(GetRedirectedPath(path));
      return new PhysicalFile(storageFile);
    }
  }
}
