using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VSS.DataOcean.Client
{
  public interface IDataOceanClient
  {
    Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
    Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
    Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
    Task<bool> PutFile(string path, string filename, Stream contents, bool isDataOceanCustomerProjectFolderStructure, IDictionary<string, string> customHeaders = null);
    Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);

    Task<Guid?> GetFolderId(string path, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
    Task<Guid?> GetFileId(string fullName, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
    Task<Stream> GetFile(string fullName, IDictionary<string, string> customHeaders, bool isDataOceanCustomerProjectFolderStructure);
  }
}
