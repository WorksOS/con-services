using BoundingExtents;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

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
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      var getExtentsResults = true;
      long[] excludedSsIds = new long[1]; // excluded surveyed surfaces
      excludedSsIds[0] = 1;
      // Raptor exclusion list class
      TSurveyedSurfaceID[] exclSSList = new TSurveyedSurfaceID[1];
      // return results
      var extents = new T3DBoundingWorldExtent();
      // mock return result
      // this wont work as we need to mock higher level
      mockRaptorClient.Setup(prj => prj.GetDataModelExtents(544, It.IsAny<TSurveyedSurfaceID[]>(), out extents)).Returns(getExtentsResults);

      // create submitter
      var submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      var result = submitter.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    [TestMethod]
    public void PD_PostProjectExtentsFail()
    {

      // create the mock RaptorClient with successful result
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();
      var trexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
      var getExtentsResults = false;
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
      ProjectExtentsSubmitter submitter = RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object, trexCompactionDataProxy: trexCompactionDataProxy.Object);
      // make request parameters
      ExtentRequest request = ExtentRequest.CreateExtentRequest(544, excludedSsIds);

      // Act
      // Call controller
      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));
    }
  }
}
