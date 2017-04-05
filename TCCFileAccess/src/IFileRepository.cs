using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TagFileHarvester.Models;

namespace TCCFileAccess.Implementation
{
    public interface IFileRepository
    {
        Task<List<Organization>> ListOrganizations();
        Task<List<string>> ListFolders(Organization org, DateTime lastModifiedUTC);
        Task<List<TCCFile>> ListFiles(Organization org, string path, DateTime createdAfterUTC);

        /// <summary>
        /// Gets the file. The resulting stream should be disposed after read completed
        /// </summary>
        /// <param name="org">The org.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        Stream GetFile(Organization org, string fullName);

        Task<bool> MoveFile(Organization org, string srcFullName, string dstFullName);
        Task<List<string>> GetFolders(Organization org, DateTime lastModifiedUTC, string path);
        Task<DateTime> GetLastChangedTime(string filespaceId, string path);
        Task<List<TCCFile>> GetFiles(string filespaceId, string syncFolder, DateTime createdAfterUTC);
        Task<bool> FolderExists(string filespaceId, string folder);
        void ListFiles(DirResult entry, List<TCCFile> files, DateTime createdAfterUTC, string path);
    }
}