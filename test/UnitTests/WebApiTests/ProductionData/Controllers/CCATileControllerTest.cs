using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApi.Coord.Controllers;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class CCATileControllerTest
  {
    /// <summary>
    /// Full integration test that requires a Raptor stack running...
    /// </summary>
    [TestMethod]
    public void GetTilesFullIntegration()
    {
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockProjectProxy = new Mock<IProjectProxy>();
      var mockGeofenceProxy = new Mock<IGeofenceProxy>();
      CCATileController controller = new CCATileController(mockProjectProxy.Object, mockGeofenceProxy.Object, mockLogger.Object, mockRaptorClient.Object);

      double[] bbox = new double[] { -91.73583984375, 29.916852233070173, -91.73309326171875, 29.91923280484215 };

      BinaryImageResponseContainer result = controller.Get
      (
          123,
          1,
          "Landfill Compactor 123",
          false,
          DateTime.UtcNow,
          DateTime.UtcNow.AddDays(5),
          bbox.ToString(),
          1,
          300,
          300,
          new Guid()
      );

      // Assert
      Assert.IsNotNull(result);
    }
  }
}
