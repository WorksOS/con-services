using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VSS.DataOcean.Client
{
  public interface IDataOceanClient
  {
    Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders);
    Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders);
    Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders);
    Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders = null);
    Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders);

    Task<Guid?> GetFolderId(string path, IDictionary<string, string> customHeaders);
    Task<Guid?> GetFileId(string fullName, IDictionary<string, string> customHeaders);
    Task<Stream> GetFile(string fullName, IDictionary<string, string> customHeaders);

  }
}
