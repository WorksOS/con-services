using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.FileAccess.Interfaces;

namespace VSS.Common.Abstractions.FileAccess
{
  public abstract class BaseVirtualFileSystem : IVirtualFileSystem
  {
    protected ILogger log;

    protected BaseVirtualFileSystem(ILoggerFactory loggerFactory)
    {
      log = loggerFactory.CreateLogger(GetType());
    }


    public abstract Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders);
    public abstract Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders);
    public abstract Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders);
    public abstract Task<bool> DeleteFolder(string path, IDictionary<string, string> customHeaders);
    public abstract Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders);
    public abstract Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders);
    public abstract Task<Stream> GetFile(string filePath, IDictionary<string, string> customHeaders);
  }
}
