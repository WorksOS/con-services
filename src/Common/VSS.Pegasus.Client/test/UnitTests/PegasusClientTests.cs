using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.DataOcean.Client.Models;
using VSS.DataOcean.Client.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class PegasusClientTests
  {
    private IServiceProvider serviceProvider;
    private IServiceCollection serviceCollection;

    public PegasusClientTests()
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
      serviceCollection.AddTransient<IPegasusClient, PegasusClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      _ = serviceProvider.GetRequiredService<ILoggerFactory>();

    }

    [Fact]
    public void CanGenerateDxfTilesMissingDxfFile()
    {
      var result = CanGenerateDxfTilesMissingFile("dummy.dxf", "dummy.dc", false);
      Assert.Null(result);
    }

    [Fact]
    public void CanGenerateDxfTilesMissingDcFile()
    {
      var result = CanGenerateDxfTilesMissingFile("dummy.dc", "dummy.dxf", true);
      Assert.Null(result);
    }


    [Fact]
    public void CanGenerateDxfTilesSuccess()
    {
      var expectedResult = new TileMetadata();//TODO
      var result = CanGenerateDxfTiles(ExecutionStatus.FINISHED).Result;
      Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void CanGenerateDxfTilesGenerationFailed()
    {
      var result = CanGenerateDxfTiles(ExecutionStatus.FAILED).Result;
      Assert.Null(result);
    }

    [Fact]
    public void CanGenerateDxfTilesTimeout()
    {
      var result = CanGenerateDxfTiles(ExecutionStatus.EXECUTING).Result;
      Assert.Null(result);
    }


    #region privates
    private TileMetadata CanGenerateDxfTilesMissingFile(string missingFileName, string presentFileName, bool dcIsMissing)
    {
      const string folderName = "unittest";
      var expectedFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = folderName };
      var expectedFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedFolderResult } };

      var expectedMissingFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile>() };
      var expectedPresentFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = presentFileName, ParentId = expectedFolderResult.Id };
      var expectedPresentFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedPresentFileResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={folderName}&owner=true";
      var browseMissingFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={missingFileName}&owner=true&parent_id={expectedFolderResult.Id}";
      var browsePresentFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={presentFileName}&owner=true&parent_id={expectedFolderResult.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseMissingFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedMissingFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browsePresentFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedPresentFileBrowseResult));

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var dxfFileName = dcIsMissing ? presentFileName : missingFileName;
      var dcFileName = dcIsMissing ? missingFileName : presentFileName;
      var result =
        client.GenerateDxfTiles($"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{dcFileName}",
          $"{Path.DirectorySeparatorChar}{folderName}{Path.DirectorySeparatorChar}{dxfFileName}",
          DxfUnitsType.Meters, null).Result;
      return result;
    }

    private Task<TileMetadata> CanGenerateDxfTiles(ExecutionStatus status)
    {
      //Set up DataOcean stuff

      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedTopFolderBrowseResult =
        new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory> { expectedTopFolderResult } };
     
      const string dcFileName = "dummy.dc";
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDcFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedDcFileResult } };
      const string dxfFileName = "dummy.dxf";
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileBrowseResult = new BrowseFilesResult() { Files = new List<DataOceanFile> { expectedDxfFileResult } };

      var dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
      var subFolderName = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id
      };
      //Since pegasus client does both create and get folder, need to have folder in results
      var expectedSubBrowseResult = new BrowseDirectoriesResult { Directories = new List<DataOceanDirectory>{ expectedSubFolderResult } };

      var config = serviceProvider.GetRequiredService<IConfigurationStore>();
      var dataOceanBaseUrl = config.GetValueString("DATA_OCEAN_URL");
      var browseTopFolderUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={topLevelFolderName}&owner=true";
      var browseSubUrl = $"{dataOceanBaseUrl}/api/browse/directories?name={subFolderName}&owner=true";
      var browseDcFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={dcFileName}&owner=true&parent_id={expectedTopFolderResult.Id}";
      var browseDxfFileUrl = $"{dataOceanBaseUrl}/api/browse/files?name={dxfFileName}&owner=true&parent_id={expectedTopFolderResult.Id}";
      var createFolderUrl = $"{dataOceanBaseUrl}/api/directories";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseTopFolderUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedTopFolderBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseDirectoriesResult>(browseSubUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedSubBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseDcFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedDcFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<BrowseFilesResult>(browseDxfFileUrl, null, null, HttpMethod.Get, null, 3, false))
        .Returns(Task.FromResult(expectedDxfFileBrowseResult));
      gracefulMock
        .Setup(g => g.ExecuteRequest<DataOceanDirectory>(createFolderUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedSubFolderResult);

      //Set up Pegasus stuff
      var units = DxfUnitsType.UsSurveyFeet.ToString();
      var expectedExecution = new PegasusExecution
      {
        Id = Guid.NewGuid(),
        ProcedureId = new Guid("b8431158-1917-4d18-9f2e-e26b255900b7"),
        Parameters = new PegasusExecutionParameters
        {
          DcFileId = expectedDcFileResult.Id,
          DxfFileId = expectedDxfFileResult.Id,
          ParentId = expectedDxfFileResult.ParentId,
          MaxZoom = 21,
          TileType = "xyz",
          TileOrder = "YX",
          MultiFile = true,
          Public = false,
          Name = subFolderName,
          AngularUnit = units,
          PlaneUnit = units,
          VerticalUnit = units
        },
        ExecutionStatus = status        
      };

      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var createExecutionUrl = $"{pegasusBaseUrl}/api/executions";
      var startExecutionUrl = $"{pegasusBaseUrl}/api/executions/{expectedExecution.Id}/start";
      var executionStatusUrl = $"{pegasusBaseUrl}/api/executions/{expectedExecution.Id}";

      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecution>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedExecution);
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecution>(startExecutionUrl, null, null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedExecution);
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecution>(executionStatusUrl, null, null, HttpMethod.Get, null, 3,
          false)).ReturnsAsync(expectedExecution);

      //TODO: set up Getfile stuff for multifile and tile metadata

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var result =
        client.GenerateDxfTiles($"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}",
          dxfFullName, DxfUnitsType.Meters, null);
      return result;
    }
    #endregion
  }
}
