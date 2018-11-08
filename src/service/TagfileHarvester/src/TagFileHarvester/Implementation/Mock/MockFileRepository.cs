using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Interfaces;
using VSS.Productivity3D.TagFileHarvester.Models;

namespace VSS.Productivity3D.TagFileHarvesterTests.MockRepositories
{
  public class MockFileRepository : IFileRepository
  {
    private List<Organization> orgs =new List<Organization>();
    private static Random random =new Random(DateTime.Now.Millisecond);
    public const int OrgIncrement = 10;

    public void DeleteOrgs(Organization[] removeOrgs)
    {
      removeOrgs.ForEach(o=> { orgs.Remove(o); });
    }

    public void Clean()
    {
      orgs.Clear();
    }


    public List<Organization> ListOrganizations()
    {
      Initialize();
      Thread.Sleep(TimeSpan.FromSeconds(2));
      return orgs;
    }

    public List<string> ListFolders(Organization org, out bool fromCache)
    {
      var result = Enumerable.Repeat(RandomString(), 15).Select(s => RandomString()).ToList();
      result.Add(String.Empty);
      fromCache = false;
      return result;
    }

    public List<FileRepository.TagFile> ListFiles(Organization org, string path)
    {
      //TODO fix test here
      return null;
      //return Enumerable.Repeat(RandomString(), 5).Select(s => path==String.Empty?String.Empty:path + '\\' + RandomString() + ".tag" ).ToList();
    }

    public Stream GetFile(Organization org, string fullName)
    {
      Thread.Sleep(TimeSpan.FromSeconds(1));
      if (fullName == String.Empty) return null;
      return new MemoryStream(new byte[]{0,1,2,3,4,5,6,7,8,9});
    }

    public bool MoveFile(Organization org, string srcFullName, string dstFullName)
    {
      Thread.Sleep(TimeSpan.FromSeconds(1.5));
      return true;
    }

    public bool IsAnythingInCahe(Organization org)
    {
      throw new NotImplementedException();
    }

    public void RemoveObsoleteFilesFromCache(Organization org, List<FileRepository.TagFile> files)
    {
      throw new NotImplementedException();
    }

    public void CleanCache(Organization org)
    {
      throw new NotImplementedException();
    }

    public void WipeCachePriorToDate(Organization org, DateTime createdUtc)
    {
      throw new NotImplementedException();
    }

    private bool Initialize()
    {
      for (int i = 0; i < OrgIncrement; i++)
        orgs.Add(new Organization(){filespaceId = RandomString(),shortName = RandomString(),orgId = RandomString()});
      return true;
    }

    private static string RandomString()
    {
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(
          Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
    }
  }
}
