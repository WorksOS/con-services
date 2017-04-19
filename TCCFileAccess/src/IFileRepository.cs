using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TCCFileAccess.Models;

namespace TCCFileAccess
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
        Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName);
        Task<DirResult> GetFolders(Organization org, DateTime lastModifiedUTC, string path);
        Task<DateTime> GetLastChangedTime(string filespaceId, string path);
        Task<bool> FolderExists(string filespaceId, string folder);
        Task<PutFileResponse> PutFile(Organization org, string path, string filename, Stream contents, long sizeOfContents);
    }
}