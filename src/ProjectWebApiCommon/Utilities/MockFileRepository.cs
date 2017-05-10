using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TCCFileAccess;
using TCCFileAccess.Models;

namespace ProjectWebApiCommon.Utilities
{
    public class MockFileRepository : IFileRepository
    {
      public Task<List<Organization>> ListOrganizations()
      {
        throw new NotImplementedException();
      }

      public Task<Stream> GetFile(Organization org, string fullName)
      {
        throw new NotImplementedException();
      }

      public Task<Stream> GetFile(string filespaceId, string fullName)
      {
        throw new NotImplementedException();
      }

      public Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
      {
        throw new NotImplementedException();
      }

      public Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
      {
        throw new NotImplementedException();
      }

      public Task<DateTime> GetLastChangedTime(string filespaceId, string path)
      {
        throw new NotImplementedException();
      }

      public Task<bool> FolderExists(string filespaceId, string folder)
      {
        throw new NotImplementedException();
      }

      public Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
      {
        return Task.FromResult(new PutFileResponse() {entryId = "whatIsThis", path = path, success = "true"});
      }
    }
}
