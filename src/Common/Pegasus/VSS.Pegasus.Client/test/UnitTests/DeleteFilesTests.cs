using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.DataOcean.Client;
using VSS.MasterData.Proxies.Interfaces;
using Xunit;

namespace VSS.Pegasus.Client.UnitTests
{
  public class DeleteFilesTests : PegasusClientTestsBase
  {
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
  }
}
