using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesignProfiler.ComputeDesignBoundary.RPC;
using DesignProfilerDecls;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class DesignControllerTest
  {
    [TestMethod]
    public void PD_GetDesignBoundariesAsGeoJasonSuccessful()
    {
      /*
      // Create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var designs = new List<FileData>() { new FileData() { Name = "File 1", ImportedFileType = ImportedFileType.DesignSurface, IsActivated = true }};
      var getDesignBoundariesResult = true;

      MemoryStream memoryStream = new MemoryStream();
      TDesignProfilerRequestResult designProfilerResult = TDesignProfilerRequestResult.dppiOK;
      
      // Mock return result
      // This wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDesignBoundary(It.IsAny<TDesignProfilerServiceRPCVerb_CalculateDesignBoundary_Args>(), out memoryStream, out designProfilerResult)).Returns(getDesignBoundariesResult);

      // Create executor
      DesignExecutor executor = new DesignExecutor(mockLogger.Object, mockRaptorClient.Object, designs);
      // Make request parameters
      DesignBoundariesRequest request = DesignBoundariesRequest.CreateDesignBoundariesRequest(544, 0.05);

      // Act
      // Call controller
      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
      */
    }
  }
}
