using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.DataOcean.Client.Models;

namespace VSS.DataOcean.Client
{
  public interface IDataOceanClient
  {
    Task<bool> FolderExists(string path, IHeaderDictionary customHeaders);
    Task<bool> FileExists(string filename, IHeaderDictionary customHeaders);
    Task<bool> MakeFolder(string path, IHeaderDictionary customHeaders);
    Task<bool> PutFile(string path, string filename, Stream contents, IHeaderDictionary customHeaders = null);
    Task<bool> DeleteFile(string fullName, IHeaderDictionary customHeaders);

    Task<Guid?> GetFolderId(string path, IHeaderDictionary customHeaders);
    Task<Guid?> GetFileId(string fullName, IHeaderDictionary customHeaders);
    Task<Stream> GetFile(string fullName, IHeaderDictionary customHeaders);

    // interface for testing
    DataOceanFolderCache GetFolderCache();
    DataOceanMissingTileCache GetTileCache();
  }
}
