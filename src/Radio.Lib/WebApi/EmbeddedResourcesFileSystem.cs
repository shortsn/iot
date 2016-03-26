﻿using Devkoes.Restup.WebServer.File;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Radio.Lib.WebApi {
  public sealed class EmbeddedResourcesFileSystem : IFileSystem {

    private readonly string _root_namespace = string.Empty;
    private string _absoluteBasePathUri = string.Empty;
    private readonly Assembly _assembly;

    public EmbeddedResourcesFileSystem(Assembly assembly, string root_namespace) {
      _root_namespace = root_namespace;
      _assembly = assembly;
    }

    public bool Exists(string absoluteBasePathUri) {
      _absoluteBasePathUri = absoluteBasePathUri ?? string.Empty;
      return true;
    }

    public Task<IFile> GetFileFromPathAsync(string path) {
      var relative_filename = path.Substring(_absoluteBasePathUri.Length).Replace(Path.DirectorySeparatorChar, '.');
      var resource_name = string.Concat(_root_namespace, relative_filename);
      return Task.FromResult<IFile>(new EmbeddedResourceFile(_assembly, resource_name));
    }
  }
}
