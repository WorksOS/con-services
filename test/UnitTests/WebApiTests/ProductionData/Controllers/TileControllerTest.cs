
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class TileControllerTest
  {
    [TestMethod]
    // Full integration test that requires a raptor stack running
    [Ignore]
    public void PostGetTilesFullIntegration()
    {
      List<ColorPalette> palettes = new List<ColorPalette>
                                         {
                                           ColorPalette.CreateColorPalette(Colors.Aqua, 1),
                                           ColorPalette.CreateColorPalette(Colors.Lime, 2),
                                           ColorPalette.CreateColorPalette(Colors.Purple, 3)
                                         };

      LiftBuildSettings liftSettings = LiftBuildSettings.CreateLiftBuildSettings(null, false, 0.2, 0.05, 0, LiftDetectionType.AutoMapReset, LiftThicknessType.Compacted,
      null, false, null, null, null, null, null, false, null, MachineSpeedTarget.CreateMachineSpeedTarget(120, 150));

      TileRequest request = TileRequest.CreateTileRequest(1158, null, DisplayMode.TargetSpeedSummary, palettes, liftSettings, RaptorConverters.VolumesType.None, 0.0, null, null, 0, null, 0,
          FilterLayerMethod.None, null, BoundingBox2DGrid.CreateBoundingBox2DGrid(2804.995, 1242.357, 2806.012, 1243.38), 128, 128);
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      TileController controller = new TileController(mockRaptorClient.Object, mockLogger.Object);

      byte[] result = controller.PostRaw(request);

      // Assert
      Assert.IsNotNull(result);
    } 
  }
}
