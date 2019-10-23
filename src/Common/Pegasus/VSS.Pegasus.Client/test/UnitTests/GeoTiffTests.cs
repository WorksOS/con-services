using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.DataOcean.Client.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class GeoTiffTests : PegasusClientTestsBase
  {
    [Fact]
    public async Task CanGenerateGeoTiffTilesMissingGeoTiffFile()
    {
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };

      _ = new DataOceanFile { Id = Guid.NewGuid(), Name = geoTiffFileName, ParentId = expectedTopFolderResult.Id };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(geoTiffFullName, null)).ReturnsAsync((Guid?)null);

      var gracefulMock = new Mock<IWebRequest>();

      await ProcessWithFailure(gracefulMock, dataOceanMock,
        $"Failed to find GeoTIFF file {geoTiffFullName}. Has it been uploaded successfully?", false);
    }

    [Fact]
    public async Task CanGenerateGeoTiffTilesFailToCreateExecution()
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = geoTiffFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(geoTiffFullName).GeneratedTilesFolder;

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(geoTiffFullName, null)).ReturnsAsync(expectedFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{Path.DirectorySeparatorChar}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var config = serviceProvider.GetRequiredService<Common.Abstractions.Configuration.IConfigurationStore>();
      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var baseRoute = "/api/executions";
      var createExecutionUrl = $"{pegasusBaseUrl}{baseRoute}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync((PegasusExecutionResult)null);

      await ProcessWithFailure(gracefulMock, dataOceanMock,
        $"Failed to create execution for {geoTiffFullName}", false);
    }

    [Fact]
    public async Task CanGenerateGeoTiffTilesFailedToStartExecution()
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = geoTiffFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(geoTiffFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(geoTiffFullName, null)).ReturnsAsync(expectedFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{Path.DirectorySeparatorChar}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var expectedExecution =
        NewGeoTiffPegasusExecution(expectedFileResult, subFolderName, ExecutionStatus.NOT_READY);

      var expectedExecutionResult = new PegasusExecutionResult { Execution = expectedExecution };

      _ = new PegasusExecutionAttemptResult
      {
        ExecutionAttempt = new PegasusExecutionAttempt { Id = Guid.NewGuid(), Status = ExecutionStatus.EXECUTING }
      };

      var config = serviceProvider.GetRequiredService<Common.Abstractions.Configuration.IConfigurationStore>();
      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var baseRoute = "/api/executions";
      var createExecutionUrl = $"{pegasusBaseUrl}{baseRoute}";
      var startExecutionUrl = $"{pegasusBaseUrl}{baseRoute}/{expectedExecution.Id}/start";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedExecutionResult);
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionAttemptResult>(startExecutionUrl, null, null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync((PegasusExecutionAttemptResult)null);

      await ProcessWithFailure(gracefulMock, dataOceanMock,
        $"Failed to start execution for {geoTiffFullName}", false);
    }

    [Fact]
    public async Task CanGenerateGeoTiffTilesSuccess()
    {
      var result = await CanGenerateGeoTiffTiles(ExecutionStatus.SUCCEEDED);

      Assert.NotNull(result);
      Assert.NotNull(result.Extents);
      Assert.NotNull(result.Extents.CoordSystem);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.North, result.Extents.North);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.South, result.Extents.South);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.East, result.Extents.East);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.West, result.Extents.West);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.CoordSystem.Type, result.Extents.CoordSystem.Type);
      Assert.Equal(expectedGeoTiffTileMetadata.Extents.CoordSystem.Value, result.Extents.CoordSystem.Value);
      Assert.Equal(expectedGeoTiffTileMetadata.MinZoom, result.MinZoom);
      Assert.Equal(expectedGeoTiffTileMetadata.MaxZoom, result.MaxZoom);
      Assert.Equal(expectedGeoTiffTileMetadata.TileCount, result.TileCount);
    }

    [Fact]
    public async Task CanGenerateGeoTiffTilesFailed()
    {
      var ex = await Assert.ThrowsAsync<ServiceException>(() => CanGenerateGeoTiffTiles(ExecutionStatus.FAILED));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to generate tiles for {geoTiffFullName}", ex.GetResult.Message);
    }

    [Fact]
    public async Task CanGenerateGeoTiffTilesTimeout()
    {
      Assert.Null(await CanGenerateGeoTiffTiles(ExecutionStatus.EXECUTING));
    }
    
    private Task<TileMetadata> CanGenerateGeoTiffTiles(string status)
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = geoTiffFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(geoTiffFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(geoTiffFullName, null)).ReturnsAsync(expectedFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{Path.DirectorySeparatorChar}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var expectedExecution = NewGeoTiffPegasusExecution(expectedFileResult, subFolderName, status);
      var expectedExecutionResult = new PegasusExecutionResult { Execution = expectedExecution };
      var expectedExecutionAttemptResult = new PegasusExecutionAttemptResult
      {
        ExecutionAttempt = new PegasusExecutionAttempt { Id = Guid.NewGuid(), Status = ExecutionStatus.EXECUTING }
      };

      var config = serviceProvider.GetRequiredService<Common.Abstractions.Configuration.IConfigurationStore>();
      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var baseRoute = "/api/executions";
      var createExecutionUrl = $"{pegasusBaseUrl}{baseRoute}";
      var startExecutionUrl = $"{pegasusBaseUrl}{baseRoute}/{expectedExecution.Id}/start";
      var executionStatusUrl = $"{pegasusBaseUrl}{baseRoute}/{expectedExecution.Id}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedExecutionResult);
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionAttemptResult>(startExecutionUrl, null, null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync(expectedExecutionAttemptResult);
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(executionStatusUrl, null, null, HttpMethod.Get, null, 3,
          false)).ReturnsAsync(expectedExecutionResult);

      return ProcessWithSuccess(gracefulMock, dataOceanMock, subFolderPath, false);
    }

    private static PegasusExecution NewGeoTiffPegasusExecution(DataOceanFile expectedFileResult, string subFolderName, string status)
    {
      return new PegasusExecution
      {
        Id = Guid.NewGuid(),
        ProcedureId = new Guid("f61c965b-0828-40b6-8980-26c7ee164566"),
        Parameters = new GeoTiffPegasusExecutionParameters
        {
          GeoTiffFileId = expectedFileResult.Id,
          ParentId = expectedFileResult.ParentId,
          TileOrder = "YX",
          TileExportFormat = "xyz",
          TileOutputFormat = "PNGRASTER",
          TileCrs = "EPSG:3857",
          MultiFile = "true",
          Public = "false",
          Name = subFolderName,
        },
        ExecutionStatus = status
      };
    }
  }
}
