using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.DataOcean.Client.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
      serviceCollection.AddSingleton<ConfigurationStore.IConfigurationStore, GenericConfiguration>();
      serviceCollection.AddTransient<IPegasusClient, PegasusClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      _ = serviceProvider.GetRequiredService<ILoggerFactory>();

    }

    [Fact]
    public async Task CanGenerateDxfTilesMissingDxfFile()
    {
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync((Guid?)null);
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);

      var gracefulMock = new Mock<IWebRequest>();

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var ex = await Assert.ThrowsAsync<ServiceException>(() => client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to find DXF file {dxfFullName}. Has it been uploaded successfully?", ex.GetResult.Message);
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

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var ex = await Assert.ThrowsAsync<ServiceException>(() => client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to find coordinate system file {dcFullName}. Has it been uploaded successfully?", ex.GetResult.Message);
    }

    [Fact]
    public async Task CanGenerateDxfTilesFailToCreateExecution()
    {
      //Set up DataOcean stuff

      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length - 1];
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id,
      };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId(subFolderPath, null)).ReturnsAsync(expectedSubFolderResult.Id);

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
        ExecutionStatus = ExecutionStatus.NOT_READY
      };

      var config = serviceProvider.GetRequiredService<ConfigurationStore.IConfigurationStore>();
      var pegasusBaseUrl = config.GetValueString("PEGASUS_URL");
      var baseRoute = "/api/executions";
      var createExecutionUrl = $"{pegasusBaseUrl}{baseRoute}";

      var gracefulMock = new Mock<IWebRequest>();
      gracefulMock
        .Setup(g => g.ExecuteRequest<PegasusExecutionResult>(createExecutionUrl, It.IsAny<MemoryStream>(), null, HttpMethod.Post, null, 3,
          false)).ReturnsAsync((PegasusExecutionResult)null);
  
      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var ex =
        await Assert.ThrowsAsync<ServiceException>(() => client.GenerateDxfTiles($"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}",
          dxfFullName, DxfUnitsType.Meters, null));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to create execution for {dxfFullName}", ex.GetResult.Message);
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
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id,
      };

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId(subFolderPath, null)).ReturnsAsync(expectedSubFolderResult.Id);

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
        ExecutionStatus = ExecutionStatus.NOT_READY
      };
      var expectedExecutionResult = new PegasusExecutionResult { Execution = expectedExecution };
      var expectedExecutionAttemptResult = new PegasusExecutionAttemptResult
      {
        ExecutionAttempt = new PegasusExecutionAttempt { Id = Guid.NewGuid(), Status = ExecutionStatus.EXECUTING }
      };

      var config = serviceProvider.GetRequiredService<ConfigurationStore.IConfigurationStore>();
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

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var ex =
        await Assert.ThrowsAsync<ServiceException>(() => client.GenerateDxfTiles($"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}",
          dxfFullName, DxfUnitsType.Meters, null));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to start execution for {dxfFullName}", ex.GetResult.Message);
    }

    [Fact]
    public void CanGenerateDxfTilesSuccess()
    {
      var result = CanGenerateDxfTiles(ExecutionStatus.FINISHED).Result;
      Assert.NotNull(result);
      Assert.NotNull(result.Extents);
      Assert.NotNull(result.Extents.CoordSystem);
      Assert.Equal(expectedTileMetadata.Extents.North, result.Extents.North);
      Assert.Equal(expectedTileMetadata.Extents.South, result.Extents.South);
      Assert.Equal(expectedTileMetadata.Extents.East, result.Extents.East);
      Assert.Equal(expectedTileMetadata.Extents.West, result.Extents.West);
      Assert.Equal(expectedTileMetadata.Extents.CoordSystem.Type, result.Extents.CoordSystem.Type);
      Assert.Equal(expectedTileMetadata.Extents.CoordSystem.Value, result.Extents.CoordSystem.Value);
      Assert.Equal(expectedTileMetadata.MinZoom, result.MinZoom);
      Assert.Equal(expectedTileMetadata.MaxZoom, result.MaxZoom);
      Assert.Equal(expectedTileMetadata.TileCount, result.TileCount);
    }

    [Fact]
    public async Task CanGenerateDxfTilesFailed()
    {
      var  ex = await Assert.ThrowsAsync<ServiceException>(() => CanGenerateDxfTiles(ExecutionStatus.FAILED));
      Assert.Equal(HttpStatusCode.InternalServerError, ex.Code);
      Assert.Equal(ContractExecutionStatesEnum.InternalProcessingError, ex.GetResult.Code);
      Assert.Equal($"Failed to generate DXF tiles for {dxfFullName}", ex.GetResult.Message);
    }

    [Fact]
    public void CanGenerateDxfTilesTimeout()
    {
      var result = CanGenerateDxfTiles(ExecutionStatus.EXECUTING).Result;
      Assert.Null(result);
    }


    #region privates
    private Task<TileMetadata> CanGenerateDxfTiles(ExecutionStatus status)
    {
      //Set up DataOcean stuff

      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };     
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var subFolderPath = new DataOceanFileUtil(dxfFullName).GeneratedTilesFolder;
      var parts = subFolderPath.Split(Path.DirectorySeparatorChar);
      var subFolderName = parts[parts.Length-1];
      var expectedSubFolderResult = new DataOceanDirectory
      {
        Id = Guid.NewGuid(),
        Name = subFolderName,
        ParentId = expectedTopFolderResult.Id,
      };
     
      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.MakeFolder(subFolderPath, null)).ReturnsAsync(true);
      dataOceanMock.Setup(d => d.GetFolderId(subFolderPath, null)).ReturnsAsync(expectedSubFolderResult.Id);

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
      var expectedExecutionResult = new PegasusExecutionResult { Execution = expectedExecution };
      var expectedExecutionAttemptResult = new PegasusExecutionAttemptResult
      {
        ExecutionAttempt = new PegasusExecutionAttempt { Id = Guid.NewGuid(), Status = ExecutionStatus.EXECUTING }
      };

      var config = serviceProvider.GetRequiredService<ConfigurationStore.IConfigurationStore>();
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

      //Set up tile metadata stuff
      byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(expectedTileMetadata));
      var expectedStream = new MemoryStream(byteArray);
      var tileMetadataFileName = $"{subFolderPath}{Path.DirectorySeparatorChar}tiles{Path.DirectorySeparatorChar}tiles.json";

      dataOceanMock.Setup(d => d.GetFile(tileMetadataFileName, null)).ReturnsAsync(expectedStream);

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();
      var result =
        client.GenerateDxfTiles($"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}",
          dxfFullName, DxfUnitsType.Meters, null);
      return result;
    }

    private TileMetadata expectedTileMetadata = new TileMetadata
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

    private const string topLevelFolderName = "unittest";
    private const string dcFileName = "dummy.dc";
    private const string dxfFileName = "dummy.dxf";
    private string dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
    private string dcFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}";

    #endregion
  }
}
