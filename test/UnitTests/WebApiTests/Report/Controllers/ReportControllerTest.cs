
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApi.Report.Controllers;

namespace VSS.Raptor.Service.WebApiTests.Report.Controllers
{
  [TestClass]
  public class ReportControllerTest
  {
    [TestMethod]
    [Ignore]
    // Full integration test that requires a raptor stack running
    public void PD_PostProjectStatisticsFullIntegration()
    {

//      long[] excludedSsIds = new long[1]; // excluded surveyed surfaces
//      excludedSsIds[0] = 0;// 13513;
      long[] excludedSsIds = null;// new long[0]; // excluded surveyed surfaces
      // return results

      // make request parameters
      ProjectStatisticsRequest request = ProjectStatisticsRequest.CreateStatisticsParameters(435, excludedSsIds);
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      ReportController controller = new ReportController(mockRaptorClient.Object, mockLogger.Object);

      // Act
      ProjectStatisticsResult result = controller.PostProjectStatistics(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
    }
  }
}
