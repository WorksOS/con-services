using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.DataOcean.Client.Models;
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
    public void CanGenerateDxfTilesMissingDxfFile()
    {
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      const string dcFileName = "dummy.dc";
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      const string dxfFileName = "dummy.dxf";

      var dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
      var dcFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}";

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync((Guid?)null);
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync(expectedDcFileResult.Id);

      var gracefulMock = new Mock<IWebRequest>();

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var result =
        client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null).Result;
      Assert.Null(result);
    }

    [Fact]
    public void CanGenerateDxfTilesMissingDcFile()
    {
      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };
      const string dcFileName = "dummy.dc";
      const string dxfFileName = "dummy.dxf";
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
      var dcFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}";

      var dataOceanMock = new Mock<IDataOceanClient>();
      dataOceanMock.Setup(d => d.GetFileId(dxfFullName, null)).ReturnsAsync(expectedDxfFileResult.Id);
      dataOceanMock.Setup(d => d.GetFileId(dcFullName, null)).ReturnsAsync((Guid?)null);

      var gracefulMock = new Mock<IWebRequest>();

      serviceCollection.AddTransient<IWebRequest>(g => gracefulMock.Object);
      serviceCollection.AddTransient<IDataOceanClient>(g => dataOceanMock.Object);
      var serviceProvider2 = serviceCollection.BuildServiceProvider();
      var client = serviceProvider2.GetRequiredService<IPegasusClient>();

      var result =
        client.GenerateDxfTiles(dcFullName, dxfFullName, DxfUnitsType.Meters, null).Result;
      Assert.Null(result);
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
    public void CanGenerateDxfTilesFailed()
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
    private Task<TileMetadata> CanGenerateDxfTiles(ExecutionStatus status)
    {
      //Set up DataOcean stuff

      const string topLevelFolderName = "unittest";
      var expectedTopFolderResult = new DataOceanDirectory { Id = Guid.NewGuid(), Name = topLevelFolderName };     
      const string dcFileName = "dummy.dc";
      var expectedDcFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dcFileName, ParentId = expectedTopFolderResult.Id };
      const string dxfFileName = "dummy.dxf";
      var expectedDxfFileResult = new DataOceanFile { Id = Guid.NewGuid(), Name = dxfFileName, ParentId = expectedTopFolderResult.Id };

      var dxfFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dxfFileName}";
      var dcFullName = $"{Path.DirectorySeparatorChar}{topLevelFolderName}{Path.DirectorySeparatorChar}{dcFileName}";
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
    #endregion
  }
}
