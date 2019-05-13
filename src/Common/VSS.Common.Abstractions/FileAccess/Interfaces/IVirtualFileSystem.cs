using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace VSS.Common.Abstractions.FileAccess.Interfaces
{
  public interface IVirtualFileSystem
  {
    Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders);
    Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders);
    Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders);
    Task<bool> DeleteFolder(string path, IDictionary<string, string> customHeaders);
    Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders);
    Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders);
    Task<Stream> GetFile(string filePath, IDictionary<string, string> customHeaders);
  }
}
