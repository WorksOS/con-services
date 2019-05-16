using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.FileAccess;

namespace VSS.TCCFileAccess.VirtualFileSystem
{
  public class TccVirtualFileSystem : BaseVirtualFileSystem
  {
    public TccVirtualFileSystem(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    public override Task<bool> FolderExists(string path, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> FileExists(string filename, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> MakeFolder(string path, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> DeleteFolder(string path, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> PutFile(string path, string filename, Stream contents, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<bool> DeleteFile(string fullName, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }

    public override Task<Stream> GetFile(string filePath, IDictionary<string, string> customHeaders)
    {
      throw new NotImplementedException();
    }
  }
}
