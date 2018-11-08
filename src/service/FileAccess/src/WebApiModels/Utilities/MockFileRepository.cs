using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TCCFileAccess;
using VSS.TCCFileAccess.Models;

namespace VSS.Productivity3D.FileAccess.WebAPI.Models.Utilities
{
  public class MockFileRepository : IFileRepository
  {
    private readonly ILogger<MockFileRepository> log;

    public MockFileRepository(ILoggerFactory logger)
    {
      log = logger.CreateLogger<MockFileRepository>();
    }

    public Task<List<Organization>> ListOrganizations()
    {
      log.LogDebug($"{nameof(ListOrganizations)}");

      return Task.FromResult(new List<Organization>
      {
        new Organization
        {
          filespaceId = "5u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgDisplayName = "the orgDisplayName",
          orgId = "u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
          orgTitle = "the orgTitle",
          shortName = "the sn"
        }
      });
    }

    public Task<Stream> GetFile(Organization org, string fullName)
    {
      log.LogDebug($"{nameof(GetFile)}: org={org.shortName} {org.filespaceId}, fullName={fullName}");

      throw new NotImplementedException();
    }

    public Task<Stream> GetFile(string filespaceId, string fullName)
    {
      log.LogDebug($"{nameof(GetFile)}: filespaceId={filespaceId}, fullName={fullName}");

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };

      var doWeHaveThisfilespaceId = ListOrganizations().Result.Exists(o => o.filespaceId == filespaceId);
      log.LogDebug($"{nameof(GetFile)}: Found filespaceId: {doWeHaveThisfilespaceId}");

      return doWeHaveThisfilespaceId
        ? Task.FromResult((Stream)new MemoryStream(buffer))
        : Task.FromResult((Stream)null);
    }

    public Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CopyFile(string filespaceId, string srcFullName, string dstFullName)
    {
      throw new NotImplementedException();
    }

    public Task<bool> CopyFile(string srcFilespaceId, string destFilespaceId, string srcFullName, string dstFullName)
    {
      return Task.FromResult(true);
    }

    public Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUtc, string path)
    {
      throw new NotImplementedException();
    }

    public Task<DirResult> GetFileList(string filespaceId, string path, string fileMasks = null)
    {
      var directoryList = new DirResult
      {
        entryName = path,
        entries = new [] { new DirResult
          { entryName = fileMasks, createTime = DateTime.UtcNow, modifyTime = DateTime.UtcNow } }
      };
      return Task.FromResult(directoryList);
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

    public Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents,
      long sizeOfContents)
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

    public Task<string> ExportToWebFormat(string srcFilespaceId, string srcPath, string dstFilespaceId, string dstPath,
      int zoomLevel)
    {
      throw new NotImplementedException();
    }

    public Task<string> CheckExportJob(string jobId)
    {
      throw new NotImplementedException();
    }
  }
}
