using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
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

    public async Task<Stream> GetFile(string filespaceId, string fullName)
    {
      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      return new MemoryStream(buffer);
    }

    public Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CopyFile(string filespaceId, string srcFullName, string dstFullName)
    {
      throw new NotImplementedException();
    }

    public Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path)
    {
      throw new NotImplementedException();
    }

    public Task<DirResult> GetFileList(string filespaceId, string path, string fileMasks = null)
    {
      throw new NotImplementedException();
    }

    public Task<DateTime> GetLastChangedTime(string filespaceId, string path)
    {
      throw new NotImplementedException();
    }

    public Task<bool> FolderExists(string filespaceId, string folder)
    {
      return Task.FromResult(false);
    }

    public Task<bool> FileExists(string filespaceId, string filename)
    {
      return Task.FromResult(false);
    }

    public Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents)
    {
      throw new NotImplementedException();
    }

    public Task<bool> PutFile(string filespaceId, string path, string filename, Stream contents, long sizeOfContents)
    {
      return Task.FromResult(true);
    }

    public Task<bool> DeleteFolder(string filespaceId, string path)
    {
      throw new NotImplementedException();
    }

    public Task<bool> DeleteFile(string filespaceId, string fullName)
    {
      return Task.FromResult(true);
    }

    public Task<bool> MakeFolder(string filespaceId, string path)
    {
      return Task.FromResult(true);
    }

    public Task<string> CreateFileJob(string filespaceId, string path)
    {
      throw new NotImplementedException();
    }

    public Task<CheckFileJobStatusResult> CheckFileJobStatus(string jobId)
    {
      throw new NotImplementedException();
    }

    public Task<GetFileJobResultResult> GetFileJobResult(string fileId)
    {
      throw new NotImplementedException();
    }

    public Task<string> ExportToWebFormat(string srcFilespaceId, string srcPath, string dstFilespaceId, string dstPath, int zoomLevel)
    {
      throw new NotImplementedException();
    }

    public Task<string> CheckExportJob(string jobId)
    {
      throw new NotImplementedException();
    }
  }
}
