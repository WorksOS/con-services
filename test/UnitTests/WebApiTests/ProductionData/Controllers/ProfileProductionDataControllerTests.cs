using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICOptionsDecls;
using System;
using System.IO;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApiTests.ProductionData.Controllers
{
  [TestClass]
  public class ProfileProductionDataControllerTests
  {
    private const long PD_MODEL_ID = 544; // Dimensions 2012 project...

    /// <summary>
    /// Creates an instance of the ProfileProductionDataRequest class.
    /// </summary>
    /// <returns>The created instance.</returns>
    /// 
    private ProfileProductionDataRequest CreateRequest()
    {
      var profileLLPoints = ProfileLLPoints.CreateProfileLLPoints(35.109149 * ConversionConstants.DEGREES_TO_RADIANS,
        -106.6040765 * ConversionConstants.DEGREES_TO_RADIANS,
        35.109149 * ConversionConstants.DEGREES_TO_RADIANS,
        -104.28745 * ConversionConstants.DEGREES_TO_RADIANS);

      return ProfileProductionDataRequest.CreateProfileProductionData(
        PD_MODEL_ID,
        new Guid(),
        ProductionDataType.Height,
        null,
        -1,
        null,
        null,
        profileLLPoints,
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

      ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

      ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
           = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
           (request.ProjectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.alignmentDesign),
            request.returnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      ProfileProductionDataExecutor executor = RequestExecutorContainerFactory.Build<ProfileProductionDataExecutor>(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
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

      ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out VLPDDecls.TWGS84Point startPt, out var endPt, out bool positionsAreGrid);

      ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
           = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
           (request.ProjectId.Value,
            -1, // don't care
            positionsAreGrid,
            startPt,
            endPt,
            RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId, null),
            RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
            RaptorConverters.DesignDescriptor(request.alignmentDesign),
            request.returnAllPassesAndLayers);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      mockRaptorClient.Setup(prj => prj.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()/*args*/)).Returns(raptorResult);

      // Create an executor...
      ProfileProductionDataExecutor executor = RequestExecutorContainerFactory.Build<ProfileProductionDataExecutor>(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
  }
}
