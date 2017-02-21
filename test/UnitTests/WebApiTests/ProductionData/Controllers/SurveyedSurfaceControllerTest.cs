using System;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class SurveyedSurfaceControllerTest
  {
    private const long PD_MODEL_ID = 1;//544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the SurveyedSurfaceRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    /// 
    private SurveyedSurfaceRequest CreateRequest()
    {
      return SurveyedSurfaceRequest.CreateSurveyedSurfaceRequest(
        PD_MODEL_ID,
        DesignDescriptor.HelpSample,
        DateTime.UtcNow
        );
    }

    /// <summary>
    /// Full integration test that requires a raptor stack running for getting a list of Surveyed Surfaces.
    /// </summary>
    /// 
    [TestMethod]
    [Ignore]
    public void PD_GetSurveyedSurfacesFullIntegration()
    {
      SurveyedSurfaceRequest request = CreateRequest();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      SurveyedSurfaceController controller = new SurveyedSurfaceController(mockRaptorClient.Object, mockLogger.Object);

      ContractExecutionResult result = controller.Get(request.projectId.Value);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
    }

    /// <summary>
    /// Full integration test that requires a raptor stack running for posting a Surveyed Surface data.
    /// </summary>
    /// 
    [TestMethod]
    [Ignore]
    public void PD_PostSurveyedSurfaceFullIntegration()
    {
      SurveyedSurfaceRequest request = CreateRequest();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      SurveyedSurfaceController controller = new SurveyedSurfaceController(mockRaptorClient.Object, mockLogger.Object);

      ContractExecutionResult result = controller.Post(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "Surveyed Surface data successfully saved.", result.Message);
    }

    /// <summary>
    /// Full integration test that requires a raptor stack running for deleting a Surveyed Surface data.
    /// </summary>
    /// 
    [TestMethod]
    [Ignore]
    public void PD_DeleteSurveyedSurfaceFullIntegration()
    {
      SurveyedSurfaceRequest request = CreateRequest();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      SurveyedSurfaceController controller = new SurveyedSurfaceController(mockRaptorClient.Object, mockLogger.Object);

      ContractExecutionResult result = controller.GetDel(request.projectId.Value, request.SurveyedSurface.id);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "Surveyed Surface data successfully deleted.", result.Message);
    }

    /// <summary>
    /// Full integration test that requires a raptor stack running for updating an existing Surveyed Surface data.
    /// </summary>
    /// 
    [TestMethod]
    [Ignore]
    public void PD_UpdateSurveyedSurfaceFullIntegration()
    {
      SurveyedSurfaceRequest request = CreateRequest();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      SurveyedSurfaceController controller = new SurveyedSurfaceController(mockRaptorClient.Object, mockLogger.Object);
      ContractExecutionResult result = controller.PostPut(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "Surveyed Surface data successfully updated.", result.Message);
    }
  }
}
