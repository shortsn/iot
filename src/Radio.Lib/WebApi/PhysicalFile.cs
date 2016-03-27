using Devkoes.Restup.WebServer.File;
using System.Threading.Tasks;
using Windows.Storage;
using System.IO;

namespace Radio.Lib.WebApi {

  public sealed class PhysicalFile : IFile {

    private readonly StorageFile _storage_file;

    public string ContentType { get; }
    
    public PhysicalFile(StorageFile storageFile) {
      _storage_file = storageFile;
      var default_type = storageFile.ContentType;
      ContentType = MimeTypeProvider.GetMimeType(storageFile.FileType) ?? default_type;
    }

    public Task<Stream> OpenStreamForReadAsync() =>
      _storage_file.OpenStreamForReadAsync();

  }
}