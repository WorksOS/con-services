using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Serilog.Extensions;
using Xunit;

namespace VSS.TCCFileAccess.UnitTests
{
  public class TCCFileAccessTests
  {
    public IServiceProvider ServiceProvider;

    public TCCFileAccessTests()
    {
      var loggerFactory = new LoggerFactory().AddSerilog(SerilogExtensions.Configure("TCCFileAccess.UnitTests.log"));
      var serviceCollection = new ServiceCollection();

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IFileRepository, FileRepository>();
      ServiceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task TestConcurrencyTCCAccess()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      await fileaccess.GetFolders(org, DateTime.MinValue, "/");
    }

    [Fact]
    public void CanParseUTF8Files()
    {
      var filename = "abcпривет123.txt";
      var noExtName = Path.GetFileNameWithoutExtension(filename);
      Assert.Equal("abcпривет123", noExtName);
    }

    [Fact]
    public void CanCreateFileAccessService()
    {
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      Assert.NotNull(fileaccess);
    }

    [Fact]
    public async Task CanListOrgs()
    {
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      Assert.NotNull(orgs);
    }

    [Fact]
    public async Task CanListFolders()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var folders = await fileaccess.GetFolders(org, DateTime.MinValue, "/");
      Assert.True(folders.entries.Length > 0);
    }

    [Fact]
    public async Task CanListFiles()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileList = await fileaccess.GetFileList(filespaceId, "/barney");
      Assert.True(fileList.entries.Length > 0);
    }


    [Fact]
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
      Assert.True(copied);
    }

    [Fact]
    public async Task CanUploadFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest1.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var exists = await fileaccess.FolderExists(org.filespaceId, folderName);
      if (!exists)
      {
        await fileaccess.MakeFolder(org.filespaceId, folderName);
      }
      using (var fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        var fileuploadresult = await fileaccess.PutFile(org, folderName, filename, fileStream, fileStream.Length);
        Assert.NotNull(fileuploadresult);
        Assert.True(fileuploadresult.success);
      }
    }

    [Fact]
    public async Task CanDownloadFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest1.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var exists = await fileaccess.FolderExists(org.filespaceId, folderName);
      if (!exists)
      {
        await fileaccess.MakeFolder(org.filespaceId, folderName);
      }
      using (var fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        await fileaccess.PutFile(org, folderName, filename, fileStream, fileStream.Length);
        var downloadFileResult = await fileaccess.GetFile(org, folderName + "/" + filename);
        Assert.Equal(downloadFileResult.Length, fileStream.Length);
      }
    }

    [Fact]
    public async Task CanCheckFolderExists()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, "/77561/1158");
      Assert.True(exists);
    }

    [Fact]
    public async Task CanCheckFolderDoesNotExist()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, "/123456789/987654321");
      Assert.False(exists);
    }

    [Fact]
    public async Task CanCheckFileExists()
    {
      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FileExists(filespaceId, "/77561/1158/Building Pad - DesignMap.dxf");
      Assert.True(exists);
    }

    [Fact]
    public async Task CanDeleteFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest1.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();

      var exists = await fileaccess.FolderExists(filespaceId, folderName);
      if (!exists)
      {
        await fileaccess.MakeFolder(filespaceId, folderName);
      }
      using (var fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        await fileaccess.PutFile(filespaceId, folderName, filename, fileStream, fileStream.Length);
      }

      var deleted = await fileaccess.DeleteFile(filespaceId, folderName + "/" + filename);
      Assert.True(deleted);
    }

    [Fact]
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
      Assert.True(success);
    }

    [Fact]
    public async Task CanDeleteFolder()
    {
      const string folderPath = "/unittest/folder";
      const string filename = "unittest1.json";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, folderPath);
      if (!exists)
      {
        await fileaccess.MakeFolder(filespaceId, folderPath);
      }
      //Put something in it to test recursive
      using (var fileStream = File.Open("appsettings.json", FileMode.Open))
      {
        await fileaccess.PutFile(filespaceId, folderPath, filename, fileStream, fileStream.Length);
      }
      var success = await fileaccess.DeleteFolder(filespaceId, folderPath);
      Assert.True(success);
    }

    [Fact]
    public async Task CanDoFileJob()
    {
      const string path = "/FileJobUnitTest/CERA.bg.dxf";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var jobId = await fileaccess.CreateFileJob(filespaceId, path);
      Assert.NotNull(jobId);

      string fileId = null;
      var done = false;
      while (!done)
      {
        var fileJobStatus = await fileaccess.CheckFileJobStatus(jobId);
        Assert.NotNull(fileJobStatus);
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
      Assert.NotNull(jobResult);
      Assert.NotNull(jobResult.extents);
    }

    [Fact(Skip = "This test can be ignored since tile generatiion is being moved across to DataOcean")]
    public async Task CanDoExportToWebFormat()
    {
      const string srcPath = "/FileJobUnitTest/CERA.bg.dxf";
      const string dstPath = "/FileJobUnitTest/CERA.bg.dxf_Tiles$/Z15.html";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      var jobId = await fileaccess.ExportToWebFormat(filespaceId, srcPath, filespaceId, dstPath, 15);
      Assert.NotNull(jobId);

      var exportStatus = await fileaccess.CheckExportJob(jobId);
      Assert.NotNull(exportStatus);
    }

    [Fact]
    public async Task CanCacheTileFile()
    {
      const string filename = "/barney/CacheUnitTest.DXF_Tiles$/Z14/6420/2960.png";

      var configuration = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = ServiceProvider.GetRequiredService<IFileRepository>();
      //First time not cached
      await fileaccess.GetFile(filespaceId, filename);
      //Second time cached
      await fileaccess.GetFile(filespaceId, filename);
    }

    [Fact]
    public void FileIsCacheable()
    {
      const string cacheableFileName = "/barney/CacheUnitTest.DXF_Tiles$/Z14/6420/2960.png";
      const string nonCacheableFileName = "/barney/dummy.png";

      Assert.True(TCCFile.FileCacheable(cacheableFileName), "File should be cacheable");
      Assert.False(TCCFile.FileCacheable(nonCacheableFileName), "File should not be cacheable");
    }

    [Fact]
    public void CanExtractFileName()
    {
      const string baseFileName = "/barney/CacheUnitTest.DXF";
      const string cacheableFileName = baseFileName + "_Tiles$/Z14/6420/2960.png";
      const string nonCacheableFileName = "/barney/dummy.png";

      Assert.Equal(baseFileName, TCCFile.ExtractFileNameFromTileFullName(cacheableFileName));
      Assert.Throws<ArgumentException>(() => TCCFile.ExtractFileNameFromTileFullName(nonCacheableFileName));
    }
  }
}
