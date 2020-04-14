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
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class DxfTests : PegasusClientTestsBase
  {
    [Fact]
    public async Task CanGenerateDxfTilesMissingDxfFile()
    {
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync((Guid?)null);
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);

      var gracefulMock = new Mock<IWebRequest>();

      await ProcessWithFailure(gracefulMock, dataOceanMock,
        $"Failed to find DXF file {dxfFullName}. Has it been uploaded successfully?", true);
    }

    [Fact]
    public async Task CanGenerateDxfTilesMissingDcFile()
    {
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync((Guid?)null);

      var gracefulMock = new Mock<IWebRequest>();

      await ProcessWithFailure(gracefulMock, dataOceanMock,
        $"Failed to find coordinate system file {dcFullName}. Has it been uploaded successfully?", true);
    }

    [Fact]
    public async Task CanGenerateDxfTilesFailToCreateExecution()
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{DataOceanUtil.PathSeparator}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var config = serviceProvider.GetRequiredService<Common.Abstractions.Configuration.IConfigurationStore>();
      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var baseRoute = "/api/executions";
      var createExecutionUrl = $"{pegasusBaseUrl}{baseRoute}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync((PegasusExecutionResult)null);

      await ProcessWithFailure(gracefulMock, dataOceanMock, $"Failed to create execution for {dxfFullName}", true);
    }

    [Fact]
    public async Task CanGenerateDxfTilesFailedToStartExecution()
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(DataOceanUtil.PathSeparator);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{DataOceanUtil.PathSeparator}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var units = DxfUnitsType.UsSurveyFeet.ToString();
      var expectedExecution =
        NewDxfPegasusExecution(expectedDcFileResult, expectedDxfFileResult, subFolderName, units, ExecutionStatus.NOT_READY);

      var expectedExecutionResult = new PegasusExecutionResult { Execution = expectedExecution };

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

      await ProcessWithFailure(gracefulMock, dataOceanMock, $"Failed to start execution for {dxfFullName}", true);
    }

    [Fact]
    public async Task CanGenerateDxfTilesSuccess()
    {
      var result = await CanGenerateDxfTiles(ExecutionStatus.SUCCEEDED);

      Assert.NotNull(result);
      Assert.NotNull(result.Extents);
      Assert.NotNull(result.Extents.CoordSystem);
      Assert.Equal(expectedDxfTileMetadata.Extents.North, result.Extents.North);
      Assert.Equal(expectedDxfTileMetadata.Extents.South, result.Extents.South);
      Assert.Equal(expectedDxfTileMetadata.Extents.East, result.Extents.East);
      Assert.Equal(expectedDxfTileMetadata.Extents.West, result.Extents.West);
      Assert.Equal(expectedDxfTileMetadata.Extents.CoordSystem.Type, result.Extents.CoordSystem.Type);
      Assert.Equal(expectedDxfTileMetadata.Extents.CoordSystem.Value, result.Extents.CoordSystem.Value);
      Assert.Equal(expectedDxfTileMetadata.MinZoom, result.MinZoom);
      Assert.Equal(expectedDxfTileMetadata.MaxZoom, result.MaxZoom);
      Assert.Equal(expectedDxfTileMetadata.TileCount, result.TileCount);
    }

    [Fact]
    public async Task CanGenerateDxfTilesFailed()
    {
      var ex = await Assert.ThrowsAsync<ServiceException>(() => CanGenerateDxfTiles(ExecutionStatus.FAILED));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to generate tiles for {dxfFullName}", ex.GetResult.Message);
    }

    [Fact]
    public async Task CanGenerateDxfTilesTimeout()
    {
      Assert.Null(await CanGenerateDxfTiles(ExecutionStatus.EXECUTING));
    }

    private Task<TileMetadata> CanGenerateDxfTiles(string status)
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(DataOceanUtil.PathSeparator);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{DataOceanUtil.PathSeparator}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

      //Set up Pegasus stuff
      var units = DxfUnitsType.UsSurveyFeet.ToString();
      var expectedExecution =
        NewDxfPegasusExecution(expectedDcFileResult, expectedDxfFileResult, subFolderName, units, status);
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

      return ProcessWithSuccess(gracefulMock, dataOceanMock, subFolderPath, true);
    }

    private static PegasusExecution NewDxfPegasusExecution(DataOceanFile expectedDcFileResult, DataOceanFile expectedDxfFileResult, string subFolderName, string units, string status)
    {
      return new PegasusExecution
      {
        Id = Guid.NewGuid(),
        ProcedureId = new Guid("b8431158-1917-4d18-9f2e-e26b255900b7"),
        Parameters = new DxfPegasusExecutionParameters
        {
          DcFileId = expectedDcFileResult.Id,
          DxfFileId = expectedDxfFileResult.Id,
          ParentId = expectedDxfFileResult.ParentId,
          MaxZoom = "21",
          TileType = "xyz",
          TileOrder = "YX",
          MultiFile = "true",
          Public = "false",
          Name = subFolderName,
          AngularUnit = units,
          PlaneUnit = units,
          VerticalUnit = units
        },
        ExecutionStatus = status
      };
    }
  }
}
