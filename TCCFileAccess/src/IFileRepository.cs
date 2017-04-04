using System;
using System.Collections.Generic;
using System.IO;
using TagFileHarvester.Models;

namespace TCCFileAccess.Implementation
{
    public interface IFileRepository
    {
        List<Organization> ListOrganizations();
        List<string> ListFolders(Organization org, DateTime lastModifiedUTC);
        List<TCCFile> ListFiles(Organization org, string path, DateTime createdAfterUTC);

        /// <summary>
        /// Gets the file. The resulting stream should be disposed after read completed
        /// </summary>
        /// <param name="org">The org.</param>
        /// <param name="fullName">The full name.</param>
        /// <returns></returns>
        Stream GetFile(Organization org, string fullName);

        bool MoveFile(Organization org, string srcFullName, string dstFullName);
        List<string> GetFolders(Organization org, DateTime lastModifiedUTC, string path);
        DateTime GetLastChangedTime(string filespaceId, string path);
        List<TCCFile> GetFiles(string filespaceId, string syncFolder, DateTime createdAfterUTC);
        bool FolderExists(string filespaceId, string folder);
        void ListFiles(DirResult entry, List<TCCFile> files, DateTime createdAfterUTC, string path);
    }
}