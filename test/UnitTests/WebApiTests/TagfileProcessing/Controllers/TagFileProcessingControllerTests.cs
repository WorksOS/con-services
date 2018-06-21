using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using TAGProcServiceDecls;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;

namespace VSS.Productivity3D.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  public class TagFileProcessingControllerTests
  {
    private WGS84Fence CreateAFence()
    {
      List<WGSPoint3D> points = new List<WGSPoint3D>
      {
        WGSPoint3D.CreatePoint(0.631986074660308, -2.00757760231466),
        WGSPoint3D.CreatePoint(0.631907507374149, -2.00758733949739),
        WGSPoint3D.CreatePoint(0.631904485465203, -2.00744352879854),
        WGSPoint3D.CreatePoint(0.631987283352491, -2.00743753668608)
      };

      return WGS84Fence.CreateWGS84Fence(points.ToArray());
    }

    [TestMethod]
    public void TagP_TagFileSubmitterSuccessful()
    {
      byte[] tagData = { 0x1, 0x2, 0x3 };
      WGS84Fence fence = CreateAFence();
      TagFileRequest request = TagFileRequest.CreateTagFile("dummy.tag", tagData, 544, fence, 1244020666025812, false, false);
      TWGS84FenceContainer fenceContainer = RaptorConverters.convertWGS84Fence(request.Boundary);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>(); TTAGProcServerProcessResult raptorResult = TTAGProcServerProcessResult.tpsprOK;
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         request.FileName,
         new MemoryStream(request.Data),
         request.ProjectId ?? -1,
         0,
         0,
         fenceContainer,
         "")).Returns(raptorResult);

      // create submitter
      TagFileExecutor submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);


      // Act
      ContractExecutionResult result = submitter.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage);
    }

    [TestMethod]
    [Ignore]
    public void TagP_TagFileSubmitterException()
    {
      byte[] tagData = new byte[] { 0x1, 0x2, 0x3 };
      WGS84Fence fence = CreateAFence();
      TagFileRequest request = TagFileRequest.CreateTagFile("dummy.tag", tagData, 544, fence, 1244020666025812, false, false);
      RaptorConverters.convertWGS84Fence(request.Boundary);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      TTAGProcServerProcessResult raptorResult = TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions;
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         request.FileName,
         It.IsAny<MemoryStream>(),
         request.ProjectId ?? -1,
         0,
         0,
         1244020666025812,
         It.IsAny<TWGS84FenceContainer>(),
         "")).Returns(raptorResult);

      // create submitter
      TagFileExecutor submitter = RequestExecutorContainerFactory.Build<TagFileExecutor>(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);

      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));
    }
  }
}
