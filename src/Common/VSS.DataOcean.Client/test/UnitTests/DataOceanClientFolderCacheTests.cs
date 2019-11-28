using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Proxies.Interfaces;
using Xunit;
using Moq;
using System.IO;
using VSS.DataOcean.Client.ResultHandling;
using VSS.DataOcean.Client.Models;
using System.Collections.Generic;
using System.Net.Http;
using VSS.Common.Abstractions.Configuration;
using FluentAssertions;

namespace VSS.DataOcean.Client.UnitTests
{
  public class DataOceanClientFolderCacheTests : BaseDataOceanClientCacheTests
  {
   
    [Fact]
    public async Task SingleFolderInCache()
    {
      var fileName = $"{DataOceanUtil.PathSeparator}tiles{DataOceanUtil.PathSeparator}tiles.json";
      var downloadUrl = TestConstants.DownloadUrl;
      var substitutedDownloadUrl = downloadUrl.Replace("{path}", fileName.Substring(1));
      var expectedResult = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3};
      var stream = new MemoryStream(expectedResult);
      var expectedDownloadResult = new StreamContent(stream);

      var config = ServiceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var dataOceanRootFolderId = config.GetValueString("DATA_OCEAN_ROOT_FOLDER_ID");
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory {Id = Guid.NewGuid(), Name = folderName, ParentId = Guid.Parse(dataOceanRootFolderId)};
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult {Directories = new List<DataOceanDirectory> {expectedFolderResult}};

      const string multiFileName = "dummy.dxf_Tiles$";
      var fileUid = Guid.NewGuid();
      var expectedFileBrowseResult = SetupExpectedMutipleFileVersionsResult(fileUid, multiFileName, expectedFolderResult.Id, downloadUrl);

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

      ServiceCollection.AddTransient(g => gracefulMock.Object);
      ServiceProvider = ServiceCollection.BuildServiceProvider();
      var client = ServiceProvider.GetRequiredService<IDataOceanClient>();

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
      folderPath?.DataOceanFolderId.Should().NotBeNull();
      folderPath?.DataOceanFolderId.Should().Be(expectedFolderResult.Id.ToString());
      folderPath?.Nodes.Count.Should().Be(0);
    }
  }
}
