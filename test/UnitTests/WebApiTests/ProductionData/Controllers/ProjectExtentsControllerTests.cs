using BoundingExtents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProjectExtentsControllerTests
  {

    [TestMethod]
    public void PD_PostProjectExtentsSuccessful()
    {

      // create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>(); var getExtentsResults = true;
      long[] excludedSsIds = new long[1]; // excluded surveyed surfaces
      excludedSsIds[0] = 1;
      // Raptor exclusion list class
      TSurveyedSurfaceID[] exclSSList = new TSurveyedSurfaceID[1];
      // return results
      BoundingExtents.T3DBoundingWorldExtent extents = new T3DBoundingWorldExtent();
      // mock return result
      // this wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDataModelExtents(544, It.IsAny<TSurveyedSurfaceID[]>(), out extents)).Returns(getExtentsResults);

      // create submitter
      ProjectExtentsSubmitter submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      ContractExecutionResult result = submitter.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    [TestMethod]
    public void PD_PostProjectExtentsFail()
    {

      // create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>(); var getExtentsResults = false;
      long[] excludedSsIds = new long[1]; // excluded surveyed surfaces
      excludedSsIds[0] = 1;
      // Raptor exclusion list class
      TSurveyedSurfaceID[] exclSSList = new TSurveyedSurfaceID[1];
      // return results
      BoundingExtents.T3DBoundingWorldExtent extents = new T3DBoundingWorldExtent();
      // mock return result
      // this wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDataModelExtents(544, It.IsAny<TSurveyedSurfaceID[]>(), out extents)).Returns(getExtentsResults);

      // create submitter
      ProjectExtentsSubmitter submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));
    }
  }
}