using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;

namespace VSS.TCCFileAccess.UnitTests
{
  [TestClass]
  public class TCCFileAccessTests
  {
    public IServiceProvider ServiceProvider;


    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      Log4NetProvider.RepoName = loggerRepoName;
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);


      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IFileRepository, FileRepository>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }


    [TestMethod]
    public async Task TestConcurrencyTCCAccess()
    {
      var serviceCollection = new ServiceCollection();
      string loggerRepoName = "UnitTestLogTest";
      var logPath = Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IFileRepository, FileRepository>();
      ServiceProvider = serviceCollection.BuildServiceProvider();

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var folders = await fileaccess.GetFolders(org, DateTime.MinValue, "/");
    }

    [TestMethod]
    public void CanParseUTF8Files()
    {
      string filename = "abcпривет123.txt";
      var noExtName = Path.GetFileNameWithoutExtension(filename);
      Assert.AreEqual("abcпривет123", noExtName);
    }

    [TestMethod]
    public void CanCreateFileAccessService()
    {
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      Assert.IsNotNull(fileaccess);
    }

    [TestMethod]
    public async Task CanListOrgs()
    {
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      Assert.IsNotNull(orgs);
    }

    [TestMethod]
    public async Task CanListFolders()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var folders = await fileaccess.GetFolders(org, DateTime.MinValue, "/");
      Assert.IsTrue(folders.entries.Length > 0);
    }

    [TestMethod]
    public async Task CanListFiles()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileList = await fileaccess.GetFileList(filespaceId, "/barney");
      Assert.IsTrue(fileList.entries.Length > 0);
    }


    [TestMethod]
    public async Task CanCopyFile()
    {
      const string srcFolderName = "/barney";
      const string dstFolderName = "/unittest";
      const string filename = "file-for-get-file-test.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var copied = await fileaccess.CopyFile(filespaceId, $"{srcFolderName}/{filename}",
        $"{dstFolderName}/{filename}");
      Assert.IsTrue(copied);
    }

    [TestMethod]
    public async Task CanUploadFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var exists = await fileaccess.FolderExists(org.filespaceId, folderName);
      if (!exists)
      {
        var success = await fileaccess.MakeFolder(org.filespaceId, folderName);
      }
      using (FileStream fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        var fileuploadresult = await fileaccess.PutFile(org, folderName, filename, fileStream, fileStream.Length);
        Assert.IsNotNull(fileuploadresult);
        Assert.IsTrue(fileuploadresult.success);
      }
    }

    [TestMethod]
    public async Task CanDownloadFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var exists = await fileaccess.FolderExists(org.filespaceId, folderName);
      if (!exists)
      {
        var success = await fileaccess.MakeFolder(org.filespaceId, folderName);
      }
      using (FileStream fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        var fileuploadresult = await fileaccess.PutFile(org, folderName, filename, fileStream, fileStream.Length);
        var downloadFileResult = await fileaccess.GetFile(org, folderName + "/" + filename);
        Assert.AreEqual(downloadFileResult.Length, fileStream.Length);
      }
    }

    [TestMethod]
    public async Task CanCheckFolderExists()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, "/77561/1158");
      Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task CanCheckFolderDoesNotExist()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, "/123456789/987654321");
      Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task CanCheckFileExists()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FileExists(filespaceId, "/77561/1158/Building Pad - DesignMap.dxf");
      Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task CanDeleteFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      
      var exists = await fileaccess.FolderExists(filespaceId, folderName);
      if (!exists)
      {
        var success = await fileaccess.MakeFolder(filespaceId, folderName);
      }
      using (FileStream fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        var fileuploadresult = await fileaccess.PutFile(filespaceId, folderName, filename, fileStream, fileStream.Length);
      }

      var deleted = await fileaccess.DeleteFile(filespaceId, folderName + "/" + filename);
      Assert.IsTrue(deleted);
    }

    [TestMethod]
    public async Task CanMakeFolder()
    {
      const string folderPath = "/unittest/folder";
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, folderPath);
      if (exists)
      {
        await fileaccess.DeleteFolder(filespaceId, folderPath);
      }
      var success = await fileaccess.MakeFolder(filespaceId, folderPath);
      Assert.IsTrue(success);
    }

    [TestMethod]
    public async Task CanDeleteFolder()
    {
      const string folderPath = "/unittest/folder";
      const string filename = "unittest.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, folderPath);
      if (!exists)
      {
        await fileaccess.MakeFolder(filespaceId, folderPath);
      }
      //Put something in it to test recursive
      using (FileStream fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        var fileuploadresult = await fileaccess.PutFile(filespaceId, folderPath, filename, fileStream, fileStream.Length);
      }
      var success = await fileaccess.DeleteFolder(filespaceId, folderPath);
      Assert.IsTrue(success);
    }

    [TestMethod]
    public async Task CanDoFileJob()
    {
      const string path = "/FileJobUnitTest/CERA.bg.dxf";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var jobId = await fileaccess.CreateFileJob(filespaceId, path);
      Assert.IsNotNull(jobId, "Failed to create file job");

      string fileId = null;
      var done = false;
      while (!done)
      {
        var fileJobStatus = await fileaccess.CheckFileJobStatus(jobId);
        Assert.IsNotNull(fileJobStatus, "Failed to check file job status");
        done = fileJobStatus.status == "COMPLETED";
        if (done)
        {
          fileId = fileJobStatus.renderOutputInfo[0].fileId;
        }
        else
        {
          Thread.Sleep(2000);
        }
      }

      var jobResult = await fileaccess.GetFileJobResult(fileId);
      Assert.IsNotNull(jobResult, "Failed to get file job result");
      Assert.IsNotNull(jobResult.extents, "DXF extents are null");
    }

    [TestMethod]
    public async Task CanDoExportToWebFormat()
    {
      const string srcPath = "/FileJobUnitTest/CERA.bg.dxf";
      const string dstPath = "/FileJobUnitTest/CERA.bg.dxf_Tiles$/Z15.html";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var jobId = await fileaccess.ExportToWebFormat(filespaceId, srcPath, filespaceId, dstPath, 15);
      Assert.IsNotNull(jobId, "Failed to export to web format"); 

      var exportStatus = await fileaccess.CheckExportJob(jobId);
      Assert.IsNotNull(exportStatus, "Failed to check export job");
    }

    [TestMethod]
    public async Task CanCacheTileFile()
    {
      const string filename = "/barney/CacheUnitTest.DXF_Tiles$/Z14/6420/2960.png";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      //First time not cached
      var downloadFileResult = await fileaccess.GetFile(filespaceId, filename);
      //Second time cached
      downloadFileResult = await fileaccess.GetFile(filespaceId, filename);
    }

    [TestMethod]
    public void FileIsCacheable()
    {
      const string cacheableFileName = "/barney/CacheUnitTest.DXF_Tiles$/Z14/6420/2960.png";
      const string nonCacheableFileName = "/barney/dummy.png";

      Assert.IsTrue(TCCFile.FileCacheable(cacheableFileName), "File should be cacheable");
      Assert.IsFalse(TCCFile.FileCacheable(nonCacheableFileName), "File should not be cacheable");
    }

    [TestMethod]
    public void CanExtractFileName()
    {
      const string baseFileName = "/barney/CacheUnitTest.DXF";     
      const string cacheableFileName = baseFileName + "_Tiles$/Z14/6420/2960.png";
      const string nonCacheableFileName = "/barney/dummy.png";

      Assert.AreEqual(baseFileName, TCCFile.ExtractFileNameFromTileFullName(cacheableFileName), "Wrong extracted file name");
      Assert.ThrowsException<ArgumentException>(() => TCCFile.ExtractFileNameFromTileFullName(nonCacheableFileName));
    }

  }
}
