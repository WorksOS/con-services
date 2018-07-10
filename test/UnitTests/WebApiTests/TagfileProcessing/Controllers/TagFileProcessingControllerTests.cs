using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingControllerTests
  {
    private WGS84Fence CreateAFence()
    {
      var points = new List<WGSPoint3D>
      {
        WGSPoint3D.CreatePoint(0.631986074660308, -2.00757760231466),
        WGSPoint3D.CreatePoint(0.631907507374149, -2.00758733949739),
        WGSPoint3D.CreatePoint(0.631904485465203, -2.00744352879854),
        WGSPoint3D.CreatePoint(0.631987283352491, -2.00743753668608)
      };

      return WGS84Fence.CreateWGS84Fence(points.ToArray());
    }

    private (TagFileRequestLegacy fileRequest, TWGS84FenceContainer container) CreateRequest(byte[] tagData, long projectId, bool createFence = true)
    {
      WGS84Fence fence = null;

      if (createFence)
      {
        fence = CreateAFence();
      }

      var request = TagFileRequestLegacy.CreateTagFile("dummy.tag", tagData, projectId, fence, 1244020666025812, false, false);
      var fenceContainer = request.Boundary != null
        ? RaptorConverters.convertWGS84Fence(request.Boundary)
        : TWGS84FenceContainer.Null();

      return (fileRequest: request, container: fenceContainer);
    }

    [TestMethod]
    public void TagP_TagFileSubmitterSuccessful()
    {
      var requestData = CreateRequest(new byte[] { 0x1, 0x2, 0x3 }, 544);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         requestData.fileRequest.FileName,
         new MemoryStream(requestData.fileRequest.Data),
         requestData.fileRequest.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
         0,
         0,
         requestData.container,
         "")).Returns(TTAGProcServerProcessResult.tpsprOK);

      // create submitter
      var submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);

      // Act
      var result = submitter.Process(requestData.fileRequest);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    [Ignore]
    public void TagP_TagFileSubmitterException()
    {
      var requestData = CreateRequest(new byte[] { 0x1, 0x2, 0x3 }, 544);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      var request = requestData.fileRequest;

      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
        request.FileName,
        new MemoryStream(request.Data),
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, 0, 0, request.MachineId ?? -1,
        request.Boundary != null
          ? RaptorConverters.convertWGS84Fence(request.Boundary)
          : TWGS84FenceContainer.Null(), request.TccOrgId)).Returns(TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions);

      var submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);

      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));
    }

    /// <summary>
    /// Tag files can be manually or automatically submitted.
    /// Manual submission requires a project Id be provided and the service determines the project boundary.
    /// Automatic submission requires no project Id be send, and the service therefore cannot resolve a boundary.
    ///
    /// Any variation of this combation is illegal and should be stopped by the executor.
    /// </summary>
    [TestMethod]
    [DataRow(544, false, "Failed to process tagfile with error: Manual tag file submissions must include a boundary fence.")]
    [DataRow(VelociraptorConstants.NO_PROJECT_ID, true, "Failed to process tagfile with error: Automatic tag file submissions cannot include boundary fence.")]
    public void Should_fail_if_project_and_boundary_dont_validate(long projectId, bool createFence, string exceptionMessage)
    {
      var requestData = CreateRequest(new byte[] { 0x1, 0x2, 0x3 }, projectId, createFence);

      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      var submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);

      Assert.ThrowsException<ServiceException>(() => submitter.Process(requestData.fileRequest), exceptionMessage);
    }
  }
}
