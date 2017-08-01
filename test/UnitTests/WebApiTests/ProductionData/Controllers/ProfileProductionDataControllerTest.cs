using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICOptionsDecls;
using System;
using System.IO;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;
using VSS.Productivity3D.WebApiModels.ProductionData.Helpers;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProfileProductionDataControllerTest
  {
    private const long PD_MODEL_ID = 544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    /// 
    private ProfileProductionDataRequest CreateRequest()
    {
      return ProfileProductionDataRequest.CreateProfileProductionData(
        PD_MODEL_ID,
        new Guid(),
        ProductionDataType.Height,
        null,
        -1,
        null,
        null,
        ProfileLLPoints.HelpSample,
        1,
        120,
        null,
        true
        );
    }


    /// <summary>
    /// Uses the mock PDS client to post a request with a successful result...
    /// </summary>
    /// 
    [TestMethod]
    public void PD_PostProfileProductionDataSuccessful()
    {
      ProfileProductionDataRequest request = CreateRequest();

      MemoryStream raptorResult = new MemoryStream();

      Assert.IsTrue(RaptorConverters.DesignDescriptor(request.alignmentDesign).IsNull(), "A linear profile expected.");

      VLPDDecls.TWGS84Point startPt, endPt;

      bool positionsAreGrid;

      ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

      ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
           = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
           (request.projectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.alignmentDesign),
            request.returnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      ProfileProductionDataExecutor executor = new ProfileProductionDataExecutor(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
    }

    /// <summary>
    /// Uses the mock PDS client to post a request with unsuccessful result...
    /// </summary>
    /// 
    [TestMethod]
    public void PD_PostProfileProductionDataFailed()
    {
      ProfileProductionDataRequest request = CreateRequest();

      MemoryStream raptorResult = null;

      Assert.IsTrue(RaptorConverters.DesignDescriptor(request.alignmentDesign).IsNull(), "A linear profile expected.");

      VLPDDecls.TWGS84Point startPt, endPt;

      bool positionsAreGrid;

      ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

      ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
           = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
           (request.projectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId, null),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.alignmentDesign),
            request.returnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      ProfileProductionDataExecutor executor = new ProfileProductionDataExecutor(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
  }
}