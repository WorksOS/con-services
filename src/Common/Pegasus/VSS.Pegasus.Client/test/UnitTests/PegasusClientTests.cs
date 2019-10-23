using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Serilog;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.DataOcean.Client.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using VSS.Serilog.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class PegasusClientTests
  {
    private readonly IServiceProvider serviceProvider;
    private readonly IServiceCollection serviceCollection;

    public PegasusClientTests()
    {
      serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Pegasus.Client.UnitTests.log")))
        .AddSingleton<Common.Abstractions.Configuration.IConfigurationStore, GenericConfiguration>()
        .AddTransient<IPegasusClient, PegasusClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    #region DXF
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
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{Path.DirectorySeparatorChar}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

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

    #endregion

    [Theory]
    [InlineData(dxfFileName, true)]
    [InlineData(geoTiffFileName, true)]
    [InlineData(dxfFileName, false)]
    [InlineData(geoTiffFileName, false)]
    public void CanDeleteTiles(string fileName, bool success)
    {
      var fullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{fileName}";
      var gracefulMock = new Mock<IWebRequest>();

      var dataOceanMock = new Mock<IDataOceanClient>();
      var tileFolderFullName = new DataOceanFileUtil(fullName).GeneratedTilesFolder;

      dataOceanMock.Setup(d => d.DeleteFile(tileFolderFullName, null)).ReturnsAsync(success);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceCollection.AddTransient(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var result = client.DeleteTiles(fullName, null).Result;
      Assert.Equal(success, result);
    }

    #region GeoTIFF   

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

    #endregion

    private Task<TileMetadata> CanGenerateDxfTiles(string status)
    {
      //Set up DataOcean stuff
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length - 1];

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId($"{Path.DirectorySeparatorChar}{topLevelFolderName}", null)).ReturnsAsync(expectedTopFolderResult.Id);

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

    private PegasusExecution NewDxfPegasusExecution(DataOceanFile expectedDcFileResult, DataOceanFile expectedDxfFileResult, string subFolderName, string units, string status)
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

    private PegasusExecution NewGeoTiffPegasusExecution(DataOceanFile expectedFileResult, string subFolderName, string status)
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

    private async Task ProcessWithFailure(Mock<IWebRequest> gracefulMock, Mock<IDataOceanClient> dataOceanMock, string expectedMessage, bool isDxf)
    {
      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceCollection.AddTransient(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var ex = await Assert.ThrowsAsync<ServiceException>(() => isDxf
                                                            ? client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null, SetJobValues)
                                                            : client.GenerateGeoTiffTiles(geoTiffFullName, null, SetJobValues));

      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal(expectedMessage, ex.GetResult.Message);
    }

    protected void SetJobValues(IDictionary<string, string> setJobIdAction)
    { }

    private Task<TileMetadata> ProcessWithSuccess(Mock<IWebRequest> gracefulMock, Mock<IDataOceanClient> dataOceanMock, string subFolderPath, bool isDxf)
    {
      //Set up tile metadata stuff
      var byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(isDxf ? expectedDxfTileMetadata : expectedGeoTiffTileMetadata));
      var expectedStream = new MemoryStream(byteArray);
      var tileMetadataFileName = $"{subFolderPath}/tiles/{(isDxf ? "tiles" : "xyz")}.json";

      dataOceanMock.Setup(d => d.GetFile(tileMetadataFileName, null)).ReturnsAsync(expectedStream);

      serviceCollection.AddTransient(g => gracefulMock.Object);
      serviceCollection.AddTransient(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var result = isDxf
        ? client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null, SetJobValues)
        : client.GenerateGeoTiffTiles(geoTiffFullName, null, SetJobValues);

      return result;
    }

    private readonly TileMetadata expectedDxfTileMetadata = new TileMetadata
    {
      Extents = new Extents
      {
        North = 0.6581020324759275,
        South = 0.6573494852112898,
        East = -1.9427990915164108,
        West = -1.9437871937920903,
        CoordSystem = new CoordSystem
        {
          Type = "EPSG",
          Value = "EPSG:4326"
        }
      },
      MaxZoom = 21,
      TileCount = 79
    };

    private readonly TileMetadata expectedGeoTiffTileMetadata = new TileMetadata
    {
      Extents = new Extents
      {
        North = -5390165.40129631,
        South = -5390801.399866779,
        East = 19196052.636336002,
        West = 19195665.919370692,
        CoordSystem = new CoordSystem
        {
          Type = "EPSG",
          Value = "EPSG:3857"
        }
      },
      MaxZoom = 23,
      TileCount = 14916
    };

    private const string topLevelFolderName = "unittest";
    private const string geoTiffFileName = "dummy.tiff";
    private const string dcFileName = "dummy.dc";
    private const string dxfFileName = "dummy.dxf";
    private readonly string dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
    public string dcFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}";
    private readonly string geoTiffFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{geoTiffFileName}";
  }
}
