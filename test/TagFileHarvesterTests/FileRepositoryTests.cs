using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileHarvester.Implementation;
using VSS.Productivity3D.TagFileHarvester.Models;


namespace VSS.Productivity3D.TagFileHarvesterTests
{
  [TestClass]
  class FileRepositoryTests
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
 
    [TestMethod]
    public void ListOrganizationsTest()
    {
      FileRepository.Log = log;
      FileRepository repository = new FileRepository();
      List<Organization> orgs = repository.ListOrganizations();
    }


    [TestMethod]
    public void ListFoldersTest()
    {
      FileRepository.Log = log;
      FileRepository repository = new FileRepository();
      bool fromCache;
      List<string> folders = repository.ListFolders(new Organization { filespaceId = filespaceId }, out fromCache);
    }

    [TestMethod]
    public void ListFilesTest()
    {
      FileRepository.Log = log;
      FileRepository repository = new FileRepository();
      List<FileRepository.TagFile> files = repository.ListFiles(new Organization { filespaceId = filespaceId }, "/WG");
      //Folders of vss-nz1 to use for testing
      // "/D5 PoundRd"
      // "/WG"
    }

 
    [TestMethod]
    [Ignore]
    public void MoveFileTest()
    {
      FileRepository.Log = log;
      FileRepository repository = new FileRepository();
      bool moved = repository.MoveFile(new Organization { filespaceId = filespaceId }, "/D5 PoundRd/Machine Control Data/.Production-Data/Test/mytest.tag", "/D5 PoundRd/Machine Control Data/.Production-Data (Archived)/Test/mytest.tag");
    }


    [TestMethod]
    public void GetFileTest()
    {
  FileRepository.Log = log;
      FileRepository repository = new FileRepository();
      Stream stream = repository.GetFile(new Organization { filespaceId = filespaceId }, "/D5 PoundRd/Machine Control Data/.Production-Data (Archived)/Test/mytest.tag");
      if (stream != null)
      {
        using (FileStream fs = File.Open(@"C:\temp\mytest.tag", FileMode.Create, FileAccess.Write, FileShare.None))
        {
          stream.CopyTo(fs);
        }
      }
    }

    private string filespaceId = "u036574d5-f108-454a-80ef-cece64eae27b";//Trimble Synchronizer Data folder for vss-nz1
  }
}
