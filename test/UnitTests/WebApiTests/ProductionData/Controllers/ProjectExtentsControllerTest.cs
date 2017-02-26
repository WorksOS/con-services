
using BoundingExtents;
using Microsoft.Extensions.Logging;
using VLPDDecls;
using VSS.Raptor.Service.WebApi.ProductionData.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace VSS.Raptor.Service.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProjectExtentsControllerTest
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
      ProjectExtentsSubmitter submitter = new ProjectExtentsSubmitter(mockLogger.Object, mockRaptorClient.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      ContractExecutionResult result = submitter.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
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
      ProjectExtentsSubmitter submitter = new ProjectExtentsSubmitter(mockLogger.Object, mockRaptorClient.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));

    }



  }

}
