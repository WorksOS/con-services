using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Serilog;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Pegasus.Client.Models;
using VSS.Serilog.Extensions;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class PegasusClientTestsBase
  {
    protected readonly IServiceProvider serviceProvider;
    protected readonly IServiceCollection serviceCollection;

    protected const string topLevelFolderName = "unittest";
    protected const string geoTiffFileName = "dummy.tiff";
    protected const string dcFileName = "dummy.dc";
    protected const string dxfFileName = "dummy.dxf";
    protected readonly string dxfFullName = $"{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{dxfFileName}";
    protected string dcFullName = $"{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{dcFileName}";
    protected readonly string geoTiffFullName = $"{DataOceanUtil.PathSeparator}{topLevelFolderName}{DataOceanUtil.PathSeparator}{geoTiffFileName}";

    public PegasusClientTestsBase()
    {
      serviceCollection = new ServiceCollection()
        .AddLogging()
        .AddSingleton(new LoggerFactory().AddSerilog(SerilogExtensions.Configure("VSS.Pegasus.Client.UnitTests.log")))
        .AddSingleton<Common.Abstractions.Configuration.IConfigurationStore, GenericConfiguration>()
        .AddTransient<IPegasusClient, PegasusClient>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    protected void SetJobValues(IHeaderDictionary setJobIdAction)
    { }

    protected async Task ProcessWithFailure(Mock<IWebRequest> gracefulMock, Mock<IDataOceanClient> dataOceanMock, string expectedMessage, bool isDxf)
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

    protected Task<TileMetadata> ProcessWithSuccess(Mock<IWebRequest> gracefulMock, Mock<IDataOceanClient> dataOceanMock, string subFolderPath, bool isDxf)
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

    protected readonly TileMetadata expectedDxfTileMetadata = new TileMetadata
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

    protected readonly TileMetadata expectedGeoTiffTileMetadata = new TileMetadata
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
  }
}
