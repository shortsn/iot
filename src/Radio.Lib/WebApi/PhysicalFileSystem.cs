using Devkoes.Restup.WebServer.File;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Radio.Lib.WebApi {

  public class PhysicalFileSystem : IFileSystem {


    public bool Exists(string absoluteBasePathUri) 
      => Directory.Exists(absoluteBasePathUri);
    
    public async Task<IFile> GetFileFromPathAsync(string path) {
      if (!File.Exists(path)) {
        throw new FileNotFoundException("File does not exist.", path);
      }

      var storageFile = await StorageFile.GetFileFromPathAsync(path);
      return new PhysicalFile(storageFile);
    }
  }
}
