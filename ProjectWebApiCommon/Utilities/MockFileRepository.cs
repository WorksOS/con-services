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
      public async Task<List<Organization>> ListOrganizations()
      {
        throw new NotImplementedException();
      }

      public async Task<Stream> GetFile(Organization org, string fullName)
      {
        throw new NotImplementedException();
      }

      public async Task<Stream> GetFile(string filespaceId, string fullName)
      {
        throw new NotImplementedException();
      }

      public async Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
      {
        throw new NotImplementedException();
      }

      public async Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
      {
        throw new NotImplementedException();
      }

      public async Task<DateTime> GetLastChangedTime(string filespaceId, string path)
      {
        throw new NotImplementedException();
      }

      public async Task<bool> FolderExists(string filespaceId, string folder)
      {
        throw new NotImplementedException();
      }

      public async Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
      {
        return new PutFileResponse() {entryId = "whatIsThis", path = path, success = "true"};
      }
    }
}
