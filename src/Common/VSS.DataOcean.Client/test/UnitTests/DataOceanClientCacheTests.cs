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
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Caching.Memory;
using Serilog;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Serilog.Extensions;
using VSS.Productivity3D.Push.Abstractions.Notifications;
using VSS.Productivity3D.Push.Clients.Notifications;
using FluentAssertions;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Cache.MemoryCache;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanClientCacheTests
  {
    private IServiceProvider serviceProvider;
    private IServiceCollection serviceCollection;

    public DataOceanClientCacheTests()
    {
      serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.DataOcean.Client.UnitTests.log")))
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddSingleton<IMemoryCache, MemoryCache>()
        .AddSingleton<IDataCache, InMemoryDataCache>();

      //This is real one to be added in services using DataOcean client. We mock it below for unit tests.
      serviceCollection.AddSingleton<IDataOceanClient, DataOceanClient>();
      serviceCollection.AddSingleton(new Mock<IServiceResolution>().Object);
      serviceCollection.AddSingleton<INotificationHubClient, NotificationHubClient>(); 

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

   
    [Fact]
    public async Task OneFolderInCache()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}tiles.json";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var fileUid = Guid.NewGuid();
      var updatedAt = DateTime.UtcNow.AddHours(-2);
      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };
      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceProvider = serviceCollection.BuildServiceProvider();
      var client = serviceProvider.GetRequiredService<IDataOceanClient>();

      var folderCache = client.GetFolderCache();
      folderCache.Should().NotBeNull();
      var root = folderCache.GetRootFolder(dataOceanRootFolderId);
      root.DataOceanFolderId.Should().Be(dataOceanRootFolderId);
      root.Nodes.Count.Should().Be(0);

      var fullFileName = $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      resultStream.Should().NotBeNull();

      folderCache = client.GetFolderCache();
      folderCache.Should().NotBeNull();
      root = folderCache.GetRootFolder(dataOceanRootFolderId);
      root.DataOceanFolderId.Should().Be(dataOceanRootFolderId);
      root.Nodes.Count.Should().Be(1);
      root.Nodes.TryGetValue(folderName, out var folderPath);
      folderPath.Should().NotBeNull();
      folderPath.DataOceanFolderId.Should().NotBeNull();
      folderPath.DataOceanFolderId.Should().Be(expectedFolderResult.Id.ToString());
      folderPath.Nodes.Count.Should().Be(0);
    }

    [Fact]
    public async Task GoodTileNoCache()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}tiles.json";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var fileUid = Guid.NewGuid();
      var updatedAt = DateTime.UtcNow.AddHours(-2);
      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };
      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync(expectedDownloadResult);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceProvider = serviceCollection.BuildServiceProvider();
      var client = serviceProvider.GetRequiredService<IDataOceanClient>();

      client.GetTileCache().GetCache().CacheKeys.Count.Should().Be(0);

      var fullFileName = $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      resultStream.Should().NotBeNull();

      client.GetTileCache().GetCache().CacheKeys.Count.Should().Be(0);

      // Generate an event, that will trigger a call to project repo for the file
      var rasterTileNotificationParameters = new RasterTileNotificationParameters
      {
        FileUid = Guid.Parse("ED279023-6A51-45B7-B4D0-2A5BF1ECA60C")
      };
      var notificationHub = serviceProvider.GetService<INotificationHubClient>() as NotificationHubClient;
      Assert.NotNull(notificationHub);
      var tasks = notificationHub.ProcessNotificationAsTasks(new ProjectFileRasterTilesGeneratedNotification(rasterTileNotificationParameters));

      // Ensure the tasks complete
      Task.WaitAll(tasks.ToArray());

      client.GetTileCache().GetCache().CacheKeys.Count.Should().Be(0);
    }

    [Fact]
    public async Task BadTileResponseNullHasNoCache()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}tiles.json";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      
      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      const string multiFileName = "dummy.dxf_Tiles$";
      var fileUid = Guid.NewGuid();
      var updatedAt = DateTime.UtcNow.AddHours(-2);
      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };
      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };

      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={folderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ReturnsAsync((HttpContent)null);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceProvider = serviceCollection.BuildServiceProvider();
      var client = serviceProvider.GetRequiredService<IDataOceanClient>();

      client.GetTileCache().GetCache().CacheKeys.Count.Should().Be(0);

      var fullFileName = $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{folderName}{DataOceanUtil.PathSeparator}{multiFileName}{fileName}";
      var resultStream = await client.GetFile(fullFileName, null);
      resultStream.Should().BeNull();
    }

    [Fact]
    public async Task BadTileHasCache()
    {
      var fileUid = Guid.NewGuid(); 
      var multiFileName = $"{fileUid}_Tiles$";
      var tileIdentifier = $"tiles{DataOceanUtil.PathSeparator}xyz{DataOceanUtil.PathSeparator}21{DataOceanUtil.PathSeparator}822028{DataOceanUtil.PathSeparator}378508.png";
      var tileDetail = $"{multiFileName}{DataOceanUtil.PathSeparator}{tileIdentifier}";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", tileIdentifier);

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      var customerFolderName = "customerFolderName"; 
      var projectFolderName = "projectFolderName";
      var expectedCustomerFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = customerFolderName, ParentId = Guid.Parse(dataOceanRootFolderId) };
      var expectedCustomerBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedCustomerFolderResult } };
      var expectedProjectFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = projectFolderName,
        ParentId = expectedCustomerFolderResult.Id
      };
      var expectedProjectBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedProjectFolderResult } };

      var browseCustomerUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={customerFolderName}&owner=true&parent_id={dataOceanRootFolderId}";
      var browseProjectUrl = $"{dataOceanBaseUrl}/api/browse/keyset_directories?name={projectFolderName}&owner=true&parent_id={expectedCustomerFolderResult.Id}";

      var updatedAt = DateTime.UtcNow.AddHours(-2);
      var expectedFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedProjectFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt
      };
      var otherFileResult = new DataOceanFile
      {
        Id = fileUid,
        Name = multiFileName,
        ParentId = expectedProjectFolderResult.Id,
        Multifile = true,
        RegionPreferences = new List<string> { "us1" },
        Status = "AVAILABLE",
        DataOceanDownload = new DataOceanTransfer { Url = downloadUrl },
        UpdatedAt = updatedAt.AddHours(-5)
      };
      var expectedFileBrowseResult = new BrowseFilesResult { Files = new List<DataOceanFile> { expectedFileResult, otherFileResult } };

      // got this "http://nowhere.in.particular/api/browse/keyset_files?name=f68e8360-eefb-4269-b456-37aa0a3f86b5_Tiles$&owner=true&parent_id=b39967e6-5733-4f8b-9d0c-31bfbad5c2c9"
      var browseFileUrl = $"{dataOceanBaseUrl}/api/browse/keyset_files?name={multiFileName}&owner=true&parent_id={expectedProjectFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseCustomerUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedCustomerBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseProjectUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedProjectBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequestAsStreamContent(substitutedDownloadUrl, HttpMethod.Get, null, null, null, 0, false))
        .ThrowsAsync(new HttpRequestException(HttpStatusCode.Forbidden.ToString()));

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceProvider = serviceCollection.BuildServiceProvider();
      var client = serviceProvider.GetRequiredService<IDataOceanClient>();

      (await client.GetTileCache().IsTileKnownToBeMissing(tileDetail)).Should().BeFalse();

      // "/a3f51fdf-69e4-4d80-b734-e72e4f9b36d9/customerFolderName/projectFolderName/cb44d207-0bec-46ad-8bb3-4388137eae53_Tiles$/tiles/xyz/21/822028/378508.png"
      var fullFileName = $"{DataOceanUtil.PathSeparator}{dataOceanRootFolderId}{DataOceanUtil.PathSeparator}{customerFolderName}{DataOceanUtil.PathSeparator}{projectFolderName}{DataOceanUtil.PathSeparator}{tileDetail}";
      var resultStream = await client.GetFile(fullFileName, null);
      resultStream.Should().BeNull();
      (await client.GetTileCache().IsTileKnownToBeMissing(tileDetail)).Should().BeTrue();

      // Generate an event, that will trigger a call to project repo for the file
      var rasterTileNotificationParameters = new RasterTileNotificationParameters
      {
        FileUid = fileUid
      };
      var notificationHub = serviceProvider.GetService<INotificationHubClient>() as NotificationHubClient;
      var tasks = notificationHub.ProcessNotificationAsTasks(new ProjectFileRasterTilesGeneratedNotification(rasterTileNotificationParameters));

      // Ensure the tasks complete
      Task.WaitAll(tasks.ToArray());

      (await client.GetTileCache().IsTileKnownToBeMissing(tileDetail)).Should().BeFalse();
    }
  }
}
