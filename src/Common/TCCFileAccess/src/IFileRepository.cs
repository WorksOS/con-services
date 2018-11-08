using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VSS.TCCFileAccess.Models;

namespace VSS.TCCFileAccess
{
    public interface IFileRepository
    {
        Task<List<Organization>> ListOrganizations();
        /// <summary>
        /// Gets the file. The resulting stream should be disposed after read completed
        /// </summary>
        /// <param name="org">The org.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        Task<Stream> GetFile(Organization org, string fullName);
        /// <summary>
        /// Gets the file. The resulting stream should be disposed after read completed
        /// </summary>
        /// <param name="filespaceId">The file space ID.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        Task<Stream> GetFile(string filespaceId, string fullName);
        Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName);
        Task<bool> CopyFile(string filespaceId, string srcFullName, string dstFullName);
        Task<bool> CopyFile(string srcFilespaceId, string dstFilespaceId, string srcFullName, string dstFullName);
        Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path);
        Task<DirResult> GetFileList(string filespaceId, string path, string fileMasks=null);
        Task<DateTime> GetLastChangedTime(string filespaceId, string path);
        Task<bool> FolderExists(string filespaceId, string folder);
        Task<bool> FileExists(string filespaceId, string filename);
        Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents);
        Task<bool> PutFile(string filespaceId, string path, string filename, Stream contents, long sizeOfContents);
        Task<bool> DeleteFolder(string filespaceId, string path);
        Task<bool> DeleteFile(string filespaceId, string fullName);
        Task<bool> MakeFolder(string filespaceId, string path);
        Task<string> CreateFileJob(string filespaceId, string path);
        Task<CheckFileJobStatusResult> CheckFileJobStatus(string jobId);
        Task<GetFileJobResultResult> GetFileJobResult(string fileId);
        Task<string> ExportToWebFormat(string srcFilespaceId, string srcPath,
          string dstFilespaceId, string dstPath, int zoomLevel);
        Task<string> CheckExportJob(string jobId);
    }
}