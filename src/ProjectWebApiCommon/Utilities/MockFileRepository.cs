using System;
using System.Collections.Generic;
using System.IO;
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

    public async Task<bool> FileExists(string filespaceId, string filename)
    {
      throw new NotImplementedException();
    }

    public Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
    {
      throw new NotImplementedException();
    }

    public async Task<bool> PutFile(string filespaceId, string path, string filename, Stream contents, long sizeOfContents)
    {
      return true;
    }

    public async Task<bool> DeleteFolder(string filespaceId, string path)
    {
      throw new NotImplementedException();
    }

    public async Task<bool> DeleteFile(string filespaceId, string fullName)
    {
      return true;
    }

    public async Task<bool> MakeFolder(string filespaceId, string path)
    {
      return true;
    }
  }
}
