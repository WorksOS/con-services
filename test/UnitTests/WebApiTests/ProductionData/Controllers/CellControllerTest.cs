
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class CellPassesControllerTest
  {
    #region Cell Passes
    [TestMethod]
    [Ignore]
    // Full integration test that requires a raptor stack running
    public void CPR_PostGetCellProfileFullIntegration()
    {

//      long[] excludedSsIds = new long[1]; // excluded surveyed surfaces
 //     excludedSsIds[0] = 1;

      // make request parameters
      CellAddress cellAddress = CellAddress.CreateCellAddress(1, 2);
      
      //CellAddress cellAddress = CellAddress.CreateCellAddress(2729, 1213);

      //Point position = Point.CreatePoint(2794.878, 1207.189);

      LiftBuildSettings liftBuildSettings = LiftBuildSettings.CreateLiftBuildSettings(
        CCVRangePercentage.CreateCcvRangePercentage(80, 130),
        true,
        0.2,
        0.05,
        0,
        LiftDetectionType.AutoMapReset,
        LiftThicknessType.Compacted,
        MDPRangePercentage.CreateMdpRangePercentage(80, 130),
        true,
        null,
        15,
        115,
        TargetPassCountRange.CreateTargetPassCountRange(5, 5),
        TemperatureWarningLevels.CreateTemperatureWarningLevels(190, 220),
        false, 
        LiftThicknessTarget.HelpSample,
        null
        );

      //CellPassesRequest request = CellPassesRequest.CreateCellPassRequest(1158, null, position, null, liftBuildSettings, 2, -1, null);

      CellPassesRequest request = CellPassesRequest.CreateCellPassRequest(544, cellAddress, null, null, liftBuildSettings, 2, -1, null);
      /*
            CellAddress cellAddress = CellAddress.CreateCellAddress(1, 2);

            CellPassesRequest request = CellPassesRequest.CreateCellPassRequest(544, cellAddress, null, null, null, 2, -1, null);
      */
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      CellController controller = new CellController(mockRaptorClient.Object, mockLogger.Object);
      ContractExecutionResult result = controller.Post(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success");
    }
    #endregion

    #region Cell Datum
    [TestMethod]
    [Ignore]
    // Full integration test that requires a raptor stack running
    public void CDR_PostGetCellDatumFullIntegration()
    {
      WGSPoint wgsPosition = WGSPoint.CreatePoint(0.631943070966667, -2.00746597541667);
      //Point position = Point.CreatePoint(2818.0593750416, 1243.5710337668);

      LiftBuildSettings liftBuildSettings = LiftBuildSettings.CreateLiftBuildSettings(
        CCVRangePercentage.CreateCcvRangePercentage(80, 130),
        true,
        0.2,
        0.05,
        0,
        LiftDetectionType.AutoMapReset,
        LiftThicknessType.Compacted,
        MDPRangePercentage.CreateMdpRangePercentage(80, 130),
        true,
        null,
        15,
        115,
        TargetPassCountRange.CreateTargetPassCountRange(5, 5),
        TemperatureWarningLevels.CreateTemperatureWarningLevels(190, 220),
        false, LiftThicknessTarget.HelpSample, 
        null
        );

      /*
      DesignDescriptor desc = DesignDescriptor.CreateDesignDescriptor(
        3113,
        FileDescriptor.CreateFileDescriptor("u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01", "/77561/1158", "Large Sites Road - Trimble Road.ttm"),
        0);
      */

      CellDatumRequest request = CellDatumRequest.CreateCellDatumRequest(1158, DisplayMode.Height, wgsPosition, null, null, -1, liftBuildSettings, null);
      /*
            CellAddress cellAddress = CellAddress.CreateCellAddress(1, 2);

            CellPassesRequest request = CellPassesRequest.CreateCellPassRequest(544, cellAddress, null, null, null, 2, -1, null);
      */
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      CellController controller = new CellController(mockRaptorClient.Object, mockLogger.Object);
      ContractExecutionResult result = controller.Post(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success");
    }

    #endregion
  }

}
