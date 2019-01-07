using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using Xunit;
using Moq;
using System.IO;
using VSS.DataOcean.Client.ResultHandling;
using VSS.DataOcean.Client.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanClientTests
  {
    private IServiceProvider serviceProvider;
    private IServiceCollection serviceCollection;

    public DataOceanClientTests()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection.AddSingleton<IConfigurationStore, GenericConfiguration>();
      //This is real one to be added in services using DataOcean client. We mock it below for unit tests.
      //serviceCollection.AddSingleton<IWebRequest, GracefulWebRequest>();
      serviceCollection.AddTransient<IDataOceanClient, DataOceanClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      _ = serviceProvider.GetRequiredService<ILoggerFactory>();

    }

    //Use [Theory] with [InlineData] for parameterized tests
    [Fact]
    public async Task CanCheckTopLevelFolderExists()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = folderName};
      var expectedBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedFolderResult}};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.FolderExists($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckSubFolderExists()
    {
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = topLevelFolderName};
      var expectedTopBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedTopFolderResult}};
      const string subFolderName = "anything";
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id
      };
      var expectedSubBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedSubFolderResult}};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseTopUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={topLevelFolderName}&owner=true";
      var browseSubUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={subFolderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseTopUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedTopBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseSubUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedSubBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FolderExists(
          $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{subFolderName}", null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckFolderDoesNotExist()
    {
      const string folderName = "unittest";
      var expectedBrowseResult = new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory>()};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.FolderExists($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.False(success);
    }

    [Fact]
    public async Task CanCheckFileExists()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = folderName};
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedFolderResult}};

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile {Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult() {Files = new List<DataOceanFile> {expectedFileResult}};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FileExists($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanCheckFileDoesNotExist()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = folderName};
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedFolderResult}};

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult() {Files = new List<DataOceanFile>()};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.FileExists($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.False(success);
    }

    [Fact]
    public async Task CanCreateTopLevelFolder()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = folderName};
      var expectedBrowseResult = new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory>()};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var createUrl = $"{dataOceanBaseUrl}/api/directories";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanDirectoryResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(new DataOceanDirectoryResult{Directory  = expectedFolderResult});

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success = await client.MakeFolder($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.True(success);

      //Check it also succeeds when the folder already exists
      expectedBrowseResult.Directories = new List<DataOceanDirectory> {expectedFolderResult};
      success = await client.MakeFolder($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.True(success);

    }


    [Fact]
    public async Task CanCreateSubFolder()
    {
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = topLevelFolderName};
      var expectedTopBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedTopFolderResult}};
      const string subFolderName = "anything";
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id
      };
      var expectedSubBrowseResult = new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory>()};

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseTopUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={topLevelFolderName}&owner=true";
      var browseSubUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={subFolderName}&owner=true";
      var createUrl = $"{dataOceanBaseUrl}/api/directories";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseTopUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedTopBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseSubUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedSubBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanDirectoryResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(new DataOceanDirectoryResult { Directory = expectedSubFolderResult });

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.MakeFolder(
          $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{subFolderName}", null);
      Assert.True(success);

      //Check it also succeeds when the folder already exists
      expectedSubBrowseResult.Directories = new List<DataOceanDirectory> {expectedSubFolderResult};
      success = await client.MakeFolder(
        $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{subFolderName}", null);
      Assert.True(success);

    }
  

    [Fact]
    public void CanPutFileSuccess()
    {
      var success = CanPutFile("AVAILABLE").Result;
      Assert.True(success);
    }

    [Fact]
    public void CanPutFileUploadFailed()
    {
      var success = CanPutFile("UPLOAD_FAILED").Result;
      Assert.False(success);
    }

    [Fact]
    public void CanPutFileTimeout()
    {
      var success = CanPutFile("UPLOADABLE").Result;
      Assert.False(success);
    }

    [Fact]
    public async Task CanDeleteExistingFile()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";
      var deleteFileUrl = $"{dataOceanBaseUrl}/api/files/{expectedFileResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest(deleteFileUrl, null, null, HttpMethod.Delete, null, 3, false))
        .Returns(Task.CompletedTask);

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.DeleteFile($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.True(success);
    }

    [Fact]
    public async Task CanDeleteNonExistingFile()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var success =
        await client.DeleteFile($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.True(success);
    }


    [Fact]
    public async Task CanGetExistingFolderId()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id = await client.GetFolderId($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.Equal(expectedFolderResult.Id, Id);
    }

    [Fact]
    public async Task CanGetNonExistingFolderId()
    {
      const string folderName = "unittest";
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id = await client.GetFolderId($"{Path.DirectorySeparatorChar}{folderName}", null);
      Assert.Null(Id);
    }


    [Fact]
    public async Task CanGetExistingFileId()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = fileName, ParentId = expectedFolderResult.Id };
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id =
        await client.GetFileId($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.Equal(expectedFileResult.Id, Id);
    }

    [Fact]
    public async Task CanGetNonExistingFileId()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      var Id =
        await client.GetFileId($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
          null);
      Assert.Null(Id);
    }

    [Fact]
    public async Task CanGetExistingSingleFile()
    {
      var downloadUrl = "https://fs-ro-us1.staging-tdata-cdn.com/r/2a0de221-1ab6-42e1-9c1e-08f4aa0cde59?Signature=Fw~5911qUEoVm5UPWPeNv79RonR8OFkaQ6XfXEmo86i9Fu7-wtiXevP9WYkrFbpDW3A7YAZva2qMLbrstuSGojpitZrArPpyAcowP2jurHQk~fvuHk601gkZMJNmmxJpaakSRoUH4rUiaan8ATC0k1OZKne5qX-vPgebvO3mZswQbrcEbwvln98pyXVHRX530epu58r05pOKPCSi-DuK8RUjtQleH-DZ3c~A3q7q73dhkMbumMFQMjDDOEzifQF1gZ7pOtpiUfcjxxU0Xzh3lX7WLGJn0TcFq6Cctylgpg0Xguw3dL4qpL2LAfjn7nNsQtuAukPUzmymCHcnP1qWJg__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvMmEwZGUyMjEtMWFiNi00MmUxLTljMWUtMDhmNGFhMGNkZTU5LyoiLCJDb25kaXRpb24iOnsiRGF0ZUxlc3NUaGFuIjp7IkFXUzpFcG9jaFRpbWUiOjE1NDQ1Njg0NDd9fX1dfQ__&Key-Pair-Id=APKAJZFK5OCWBA5LQHUQ";
      var expectedResult = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileResult = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = fileName,
        ParentId = expectedFolderResult.Id,
        Multifile = false,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl }
      };
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(downloadUrl, HttpMethod.Get, null, null, null, 3, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var resultStream = await client.GetFile($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}", 
        null);
      using (var ms = new MemoryStream())
      {
        resultStream.CopyTo(ms);
        var result = ms.ToArray();
        Assert.Equal(expectedResult, result);
      }
    }

    [Fact]
    public async Task CanGetNonExistingSingleFile()
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string fileName = "dummy.dxf";
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile>() };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={fileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var resultStream = await client.GetFile($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{fileName}",
        null);
      Assert.Null(resultStream);
    }

    [Fact]
    public async Task CanGetExistingMultiFile()
    {
      var fileName = $"{Path.DirectorySeparatorChar}tiles{Path.DirectorySeparatorChar}tiles.json";
      var downloadUrl = "https://fs-ro-us1.staging-tdata-cdn.com/r/2a0de221-1ab6-42e1-9c1e-08f4aa0cde59/{path}?Signature=Fw~5911qUEoVm5UPWPeNv79RonR8OFkaQ6XfXEmo86i9Fu7-wtiXevP9WYkrFbpDW3A7YAZva2qMLbrstuSGojpitZrArPpyAcowP2jurHQk~fvuHk601gkZMJNmmxJpaakSRoUH4rUiaan8ATC0k1OZKne5qX-vPgebvO3mZswQbrcEbwvln98pyXVHRX530epu58r05pOKPCSi-DuK8RUjtQleH-DZ3c~A3q7q73dhkMbumMFQMjDDOEzifQF1gZ7pOtpiUfcjxxU0Xzh3lX7WLGJn0TcFq6Cctylgpg0Xguw3dL4qpL2LAfjn7nNsQtuAukPUzmymCHcnP1qWJg__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvMmEwZGUyMjEtMWFiNi00MmUxLTljMWUtMDhmNGFhMGNkZTU5LyoiLCJDb25kaXRpb24iOnsiRGF0ZUxlc3NUaGFuIjp7IkFXUzpFcG9jaFRpbWUiOjE1NDQ1Njg0NDd9fX1dfQ__&Key-Pair-Id=APKAJZFK5OCWBA5LQHUQ";
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var expectedFileResult = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl }
      };
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 3, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var fullFileName = $"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      using (var ms = new MemoryStream())
      {
        resultStream.CopyTo(ms);
        var result = ms.ToArray();
        Assert.Equal(expectedResult, result);
      }
    }

    [Fact]
    public async Task CanGetNonExistingMultiFile()
    {
      var fileName = $"{Path.DirectorySeparatorChar}tiles{Path.DirectorySeparatorChar}15{Path.DirectorySeparatorChar}18756{Path.DirectorySeparatorChar}2834.png";
      var downloadUrl = "https://fs-ro-us1.staging-tdata-cdn.com/r/2a0de221-1ab6-42e1-9c1e-08f4aa0cde59/{path}?Signature=Fw~5911qUEoVm5UPWPeNv79RonR8OFkaQ6XfXEmo86i9Fu7-wtiXevP9WYkrFbpDW3A7YAZva2qMLbrstuSGojpitZrArPpyAcowP2jurHQk~fvuHk601gkZMJNmmxJpaakSRoUH4rUiaan8ATC0k1OZKne5qX-vPgebvO3mZswQbrcEbwvln98pyXVHRX530epu58r05pOKPCSi-DuK8RUjtQleH-DZ3c~A3q7q73dhkMbumMFQMjDDOEzifQF1gZ7pOtpiUfcjxxU0Xzh3lX7WLGJn0TcFq6Cctylgpg0Xguw3dL4qpL2LAfjn7nNsQtuAukPUzmymCHcnP1qWJg__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy1yby11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvMmEwZGUyMjEtMWFiNi00MmUxLTljMWUtMDhmNGFhMGNkZTU5LyoiLCJDb25kaXRpb24iOnsiRGF0ZUxlc3NUaGFuIjp7IkFXUzpFcG9jaFRpbWUiOjE1NDQ1Njg0NDd9fX1dfQ__&Key-Pair-Id=APKAJZFK5OCWBA5LQHUQ";
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var expectedFileResult = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl }
      };
      var expectedFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 3, false))
        .ReturnsAsync((HttpContent)null);

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();

      var fullFileName = $"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      Assert.Null(resultStream);
    }

    #region privates
    private Task<bool> CanPutFile(string status)
    {
      var uploadUrl = "https://fs-us1.staging-tdata-cdn.com/r/11591287-648f-4c60-ae5c-80b61b12d78b?Signature=lD3yNv-YesLoQWwCYPVo-dzh9Xw0Q5kiCkPkckv67tOP1e~AfFiJv9jYAqmES0vQgkQvSqzvK4RJ2l2gXybdq3pvEDxeFbQtvAW-6hHMBd7q~KUMi4gW4GSD-mWtiH~4~576SEUn-uZl6reyaM6yRqPXjS2VhBJGnBWzdhU~HVEiMJERR5MSfZp~oXfi~Gq-0NbeiXF-zIp1EIuH-cEP69MZg4zXRQn~wbHdrkBeVQeaziPVo1Keg~xDoi5TkyBnfV5Lpc3ZlRvlgHdPPrLuzOKbHXnsH2rPSD3naZPNfxtCq-V8YapD2NnZGyTPX-2FE77y3~X8k-rWPboU210WdA__&Policy=eyJTdGF0ZW1lbnQiOlt7IlJlc291cmNlIjoiaHR0cHM6Ly9mcy11czEuc3RhZ2luZy10ZGF0YS1jZG4uY29tL3IvMTE1OTEyODctNjQ4Zi00YzYwLWFlNWMtODBiNjFiMTJkNzhiIiwiQ29uZGl0aW9uIjp7IkRhdGVMZXNzVGhhbiI6eyJBV1M6RXBvY2hUaW1lIjoxNTQxNjI1NzA1fX19XX0_&Key-Pair-Id=APKAJZFK5OCWBA5LQHUQ";

      const string fileName = "dummy.dxf";
      var expectedFile = new DataOceanFile
      {
        Id = Guid.NewGuid(),
        Name = fileName,
        Multifile = false,
        RegionPreferences = new List<string> { "us1" },
        Status = status,
        DataOceanUpload = new DataOceanTransfer { Url = uploadUrl }
      };
      var expectedFileResult = new DataOceanFileResult {File = expectedFile};
      var folderName = $"{Path.DirectorySeparatorChar}";
      var expectedBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>() };
      var expectedUploadResult = new StringContent("some ok result");

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var createUrl = $"{dataOceanBaseUrl}/api/files";
      var getUrl = $"{createUrl}/{expectedFile.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock.Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseUrl, null, null, HttpMethod.Get, null, 3, false))
      .Returns(Task.FromResult(expectedBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanFileResult>(createUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3, false))
        .ReturnsAsync(expectedFileResult);
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(uploadUrl, HttpMethod.Put, null, It.IsAny<Stream>(), null, 3, false))
        .ReturnsAsync(expectedUploadResult);
      gracefulMock.Setup(g => g.ExecuteRequest<DataOceanFileResult>(getUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IDataOceanClient>();
      return client.PutFile(folderName, fileName, null, null);
    }
    #endregion
  }

}
