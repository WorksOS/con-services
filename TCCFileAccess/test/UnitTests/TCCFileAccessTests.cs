﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4netExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TCCFileAccess;
using VSS.GenericConfiguration;

namespace UnitTests
{
  [TestClass]
  public class TCCFileAccessTests
  {
    public IServiceProvider serviceProvider = null;

    [TestInitialize]
    public virtual void InitTest()
    {
      var serviceCollection = new ServiceCollection();

      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();
      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);

      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      serviceCollection.AddLogging();
      serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, VSS.GenericConfiguration.GenericConfiguration>();
      serviceCollection.AddTransient<IFileRepository, FileRepository>();
      serviceProvider = serviceCollection.BuildServiceProvider();
    }



    [TestMethod]
    public void CanCreateFileAccessService()
    {
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      Assert.IsNotNull(fileaccess);
    }

    [TestMethod]
    public async Task CanListOrgs()
    {
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      Assert.IsNotNull(orgs);
    }

    [TestMethod]
    public async Task CanListFoldersFiles()
    {
      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var orgs = await fileaccess.ListOrganizations();
      var org = (from o in orgs where o.shortName == orgName select o).First();
      var folders = await fileaccess.GetFolders(org, DateTime.MinValue, "/");
      Assert.IsTrue(folders.entries.Length > 0);
    }

    [TestMethod]
    public async Task CanUploadFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest.json";

      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
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

      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var orgName = configuration.GetValueString("TCCORG");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
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
      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, "/77561/1158");
      Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task CanCheckFileExists()
    {
      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FileExists(filespaceId, "/77561/1158/Building Pad - DesignMap.dxf");
      Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task CanDeleteFile()
    {
      const string folderName = "/barney";
      const string filename = "unittest.json";

      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      
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
      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
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
      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");

      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var exists = await fileaccess.FolderExists(filespaceId, folderPath);
      if (!exists)
      {
        await fileaccess.MakeFolder(filespaceId, folderPath);
      }
      var success = await fileaccess.DeleteFolder(filespaceId, folderPath);
      Assert.IsTrue(success);
    }

    [TestMethod]
    public async Task CanDoFileJob()
    {
      const string path = "/FileJobUnitTest/CERA.bg.dxf";

      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
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
    }

    [TestMethod]
    public async Task CanDoExportToWebFormat()
    {
      const string srcPath = "/FileJobUnitTest/CERA.bg.dxf";
      const string dstPath = "/FileJobUnitTest/CERA.bg.dxf_Tiles$/Z15.html";

      var configuration = serviceProvider.GetRequiredService<IConfigurationStore>();
      var filespaceId = configuration.GetValueString("TCCFILESPACEID");
      var fileaccess = serviceProvider.GetRequiredService<IFileRepository>();
      var jobId = await fileaccess.ExportToWebFormat(filespaceId, srcPath, filespaceId, dstPath, 15);
      Assert.IsNotNull(jobId, "Failed to export to web format"); 

      var exportStatus = await fileaccess.CheckExportJob(jobId);
      Assert.IsNotNull(exportStatus, "Failed to check export job");
    }

  }
}
