using System.Collections.Generic;
using System.IO;
using TagFileHarvester.Implementation;
using TagFileHarvester.Models;

namespace TagFileHarvester.Interfaces
{
  public interface IFileRepository
  {
    //TODO probably we need ticket to be hidden in repository
    List<Organization> ListOrganizations();
    List<string> ListFolders(Organization org, out bool fromCache);
    List<FileRepository.TagFile> ListFiles(Organization org, string path);
    Stream GetFile(Organization org, string fullName);
    bool MoveFile(Organization org, string srcFullName, string dstFullName);
    bool IsAnythingInCahe(Organization org);
    void RemoveObsoleteFilesFromCache(Organization org, List<FileRepository.TagFile> files);
    void CleanCache(Organization org);
  }
}