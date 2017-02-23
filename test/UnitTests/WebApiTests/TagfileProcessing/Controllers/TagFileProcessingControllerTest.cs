
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using VLPDDecls;
using TAGProcServiceDecls;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Executors;
using VSS.Raptor.Service.WebApiModels.TagfileProcessing.Models;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApi.TagFileProcessing.Controllers;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiTests.TagfileProcessing.Controllers
{
  [TestClass]
  [DeploymentItem(@"App_data\","App_Data")]  
  public class TagFileProcessingControllerTest
  {
    private WGS84Fence CreateAFence()
    {
      List<WGSPoint> points = new List<WGSPoint>();
      points.Add(WGSPoint.CreatePoint(0.631986074660308, -2.00757760231466));
      points.Add(WGSPoint.CreatePoint(0.631907507374149, -2.00758733949739));
      points.Add(WGSPoint.CreatePoint(0.631904485465203, -2.00744352879854));
      points.Add(WGSPoint.CreatePoint(0.631987283352491, -2.00743753668608));
      return WGS84Fence.CreateWGS84Fence(points.ToArray());
    }

    [TestMethod]
    public void TagP_PostTagFileFullIntegration()
    {

      // Accepting override ProjectID for file 1551J025SW--ACOM--121030234410.tag (MachineID = ACOM, AssetID = 1244020666025812)
      //8/10/2014 15:18:43.288	( 6712) [TSVOICSnippetProcessor 0]: Processed file 1551J025SW--ACOM--121030234410.tag (containing 225 epochs and produced 20 cell passes) from machine '1551J025SW' type 39 (asset ID 1244020666025812) into project 544 with seed position lat=0.631935, lon=-2.007484 and extents=MinX: 2724.590, MaxX:2726.290, MinY: 1193.230, MaxY:1193.570, MinZ: 622.988, MaxZ:623.111
     

      const string tagFileName = "1551J025SW--ACOM--121030234410.tag";
      var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      string tagFilePath = Path.Combine(baseDir, "App_Data");
      tagFilePath = Path.Combine(tagFilePath, tagFileName);
      byte[] tagData = ControllerTestUtil.FileToByteArray(tagFilePath);
      WGS84Fence fence = CreateAFence();
      TagFileRequest request = TagFileRequest.CreateTagFile(tagFileName, tagData, 544, fence, 1244020666025812,
          false, false);
      TWGS84FenceContainer fenceContainer = RaptorConverters.convertWGS84Fence(request.boundary);
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockTagProcessor = new Mock<ITagProcessor>();
      TagFileController controller = new TagFileController(mockRaptorClient.Object, mockTagProcessor.Object, mockLogger.Object);

      // Act
      ContractExecutionResult result = controller.Post(request);
      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success", result.Message);
    }


    
    [TestMethod]
    public void TagP_TagFileSubmitterSuccessful()
    {
      byte[] tagData = new byte[] { 0x1, 0x2, 0x3 }; 
      WGS84Fence fence = CreateAFence();
      TagFileRequest request = TagFileRequest.CreateTagFile("dummy.tag", tagData, 544, fence, 1244020666025812,false, false);
      TWGS84FenceContainer fenceContainer = RaptorConverters.convertWGS84Fence(request.boundary);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>(); TTAGProcServerProcessResult raptorResult = TTAGProcServerProcessResult.tpsprOK;
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         request.fileName,
         new MemoryStream(request.data),
         request.projectId ?? -1,
         0,
         0,
         fenceContainer,
         "")).Returns(raptorResult);

      // create submitter
      TagFileExecutor submitter = new TagFileExecutor(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);


      // Act
      ContractExecutionResult result = submitter.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == "success");
    }

    [TestMethod]
    public void TagP_TagFileSubmitterException()
    {
      byte[] tagData = new byte[] { 0x1, 0x2, 0x3 };
      WGS84Fence fence = CreateAFence();
      TagFileRequest request = TagFileRequest.CreateTagFile("dummy.tag", tagData, 544, fence, 1244020666025812, false, false);
      TWGS84FenceContainer fenceContainer = RaptorConverters.convertWGS84Fence(request.boundary);

      // create the mock PDSClient with successful result
      var mockTagProcessor = new Mock<ITagProcessor>();
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      TTAGProcServerProcessResult raptorResult = TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions;
      mockTagProcessor.Setup(prj => prj.ProjectDataServerTAGProcessorClient().SubmitTAGFileToTAGFileProcessor(
         request.fileName,
         It.IsAny<MemoryStream>(),
         request.projectId ?? -1,
         0,
         0,
         1244020666025812,
         It.IsAny<TWGS84FenceContainer>(),
         "")).Returns(raptorResult);

      // create submitter
      TagFileExecutor submitter = new TagFileExecutor(mockLogger.Object, mockRaptorClient.Object, mockTagProcessor.Object);

      Assert.ThrowsException<ServiceException>(() => submitter.Process(request));
    }



  }
    
}


