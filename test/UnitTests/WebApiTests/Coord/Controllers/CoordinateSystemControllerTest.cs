using System;
using System.IO;
using System.Reflection;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VLPDDecls;
using VSS.Raptor.Service.WebApiModels.Coord.Executors;
using VSS.Raptor.Service.WebApiModels.Coord.Models;
using VSS.Raptor.Service.WebApi.Coord.Controllers;
using VSS.Raptor.Service.WebApiModels.Coord.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;



namespace VSS.Raptor.Service.WebApiTests.Coord.Controllers
{
    [TestClass]
    [DeploymentItem(@"App_data\", "App_Data")]
    public class CoordinateSystemControllerTest
    {
        private const long PD_MODEL_ID = 544; // Dimensions 2012 project...

        #region Post
        /// <summary>
        /// Full integration test that requires a raptor stack running for posting a CS definition data.
        /// </summary>
        /// 
        [TestMethod]        
        [Ignore]
        public void CS_PostCoordinateSytemFile_FullIntegration()
        {
            const string csFileName = "BootCamp 2012.dc";
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string csFilePath = Path.Combine(baseDir, "App_Data");
            string csFileFullName = Path.Combine(csFilePath, csFileName);

            Assert.IsTrue(File.Exists(csFileFullName), String.Format("Coordinate System definition file {0} does not exist.", csFileFullName));

            ContractExecutionResult result = null;

            byte[] csFileContent = ControllerTestUtil.FileToByteArray(csFileFullName);

            CoordinateSystemFile request = CoordinateSystemFile.CreateCoordinateSystemFile(PD_MODEL_ID, csFileContent, csFileName);
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();
            CoordinateSystemController controller = new CoordinateSystemController(mockRaptorClient.Object, mockLogger.Object);
            result = controller.Post(request);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message == "success", result.Message);
        }

        /// <summary>
        ///  Uses the mock PDS client to post a CS file with successful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateSystemControllerPostSuccessful()
        {
            byte[] csFileContent = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

            CoordinateSystemFile request = CoordinateSystemFile.CreateCoordinateSystemFile(PD_MODEL_ID, csFileContent, "dummy.dc");

            // Create the mock PDSClient with successful result...
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();

            TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneOK;

            TCoordinateSystemSettings csSettings;

            mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
              new MemoryStream(request.csFileContent),
              request.csFileName,
              request.projectId ?? -1, out csSettings)).Returns(raptorResult);

            // Create an executor...
            CoordinateSystemExecutorPost executor = new CoordinateSystemExecutorPost(mockLogger.Object, mockRaptorClient.Object);

            ContractExecutionResult result = executor.Process(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message == "success", result.Message);
        }

        /// <summary>
        /// Uses the mock PDS client to post a CS file with unsuccessful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateSystemControllerPostFailed()
        {
            byte[] csFileContent = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

            CoordinateSystemFile request = CoordinateSystemFile.CreateCoordinateSystemFile(PD_MODEL_ID, csFileContent, "dummy.dc");

            // Create the mock PDSClient with unsuccessful result...
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();
            TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneCouldNotConvertCSDFile;

            TCoordinateSystemSettings csSettings;

            mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
               It.IsAny<MemoryStream>(),
              request.csFileName,
              request.projectId ?? -1, out csSettings)).Returns(raptorResult);

            // Create an executor...
            CoordinateSystemExecutorPost executor = new CoordinateSystemExecutorPost(mockLogger.Object, mockRaptorClient.Object);

            Assert.ThrowsException<ServiceException>(() => executor.Process(request));
        }
    #endregion

    #region Get
    /// <summary>
    /// Full integration test that requires a raptor stack running for getting CS settings.
    /// </summary>
    /// 
    [TestMethod]
        [Ignore]
        public void CS_GetCoordinateSytemSettings_FullIntegration()
        {
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();
            CoordinateSystemController controller = new CoordinateSystemController(mockRaptorClient.Object, mockLogger.Object);
            ContractExecutionResult result = controller.Get(PD_MODEL_ID);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message == "success", result.Message);
        }

        /// <summary>
        ///  Uses the mock PDS client to get CS settings with successful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateSystemControllerGetSuccessful()
        {
            ProjectID request = ProjectID.CreateProjectID(PD_MODEL_ID);

            // Create the mock PDSClient with successful result...
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();

            TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneOK;

            TCoordinateSystemSettings csSettings;

            mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.projectId.Value, out csSettings)).Returns(raptorResult);

            // Create an executor...
            CoordinateSystemExecutorGet executor = new CoordinateSystemExecutorGet(mockLogger.Object, mockRaptorClient.Object);

            ContractExecutionResult result = executor.Process(request);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Message == "success", result.Message);
        }

        /// <summary>
        ///  Uses the mock PDS client to get CS settings with unsuccessful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateSystemControllerGettFailed()
        {
            ProjectID request = ProjectID.CreateProjectID(PD_MODEL_ID);

            // Create the mock PDSClient with unsuccessful result...
            var mockRaptorClient = new Mock<IASNodeClient>();
            var mockLogger = new Mock<ILoggerFactory>();
            TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneNoSuchDataModel;

            TCoordinateSystemSettings csSettings;

            mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.projectId.Value, out csSettings)).Returns(raptorResult);

            // Create an executor...
            CoordinateSystemExecutorGet executor = new CoordinateSystemExecutorGet(mockLogger.Object, mockRaptorClient.Object);

            Assert.ThrowsException<ServiceException>(() => executor.Process(request));

        }
        #endregion

    #region Coordinate Conversion
    /// <summary>
    /// Full integration test that requires a raptor stack running for the coordinate conversion task.
    /// </summary>
    /// 
    [TestMethod]
        [Ignore]
        public void CS_CoordinateConversion_FullIntegration()
        {
          var mockRaptorClient = new Mock<IASNodeClient>();
          var mockLogger = new Mock<ILoggerFactory>();
          CoordinateSystemController controller = new CoordinateSystemController(mockRaptorClient.Object, mockLogger.Object);
          CoordinateConversionRequest request = CoordinateConversionRequest.HelpSample;
          // NE to LL conversion...
          CoordinateConversionResult result = controller.Post(request);

          Assert.IsNotNull(result);
          Assert.IsTrue(result.Message == "success", result.Message);

          // LL to NE conversion...
          CoordinateConversionRequest request1 = CoordinateConversionRequest.CreateCoordinateConversionRequest(request.projectId.Value, TwoDCoordinateConversionType.LatLonToNorthEast, result.conversionCoordinates);
          
          result = controller.Post(request1);

          Assert.IsNotNull(result);
          Assert.IsTrue(result.Message == "success", result.Message);

          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[0].x) == Math.Truncate(result.conversionCoordinates[0].x), "1st point East value does not match!");
          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[0].y) == Math.Truncate(result.conversionCoordinates[0].y), "1st point North value does not match!");
          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[1].x) == Math.Truncate(result.conversionCoordinates[1].x), "2nd point East value does not match!");
          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[1].y) == Math.Truncate(result.conversionCoordinates[1].y), "2nd point North value does not match!");
          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[2].x) == Math.Truncate(result.conversionCoordinates[2].x), "3rd point East value does not match!");
          Assert.IsTrue(Math.Truncate(request.conversionCoordinates[2].y) == Math.Truncate(result.conversionCoordinates[2].y), "3rd point North value does not match!");
        }

        /// <summary>
        ///  Uses the mock PDS client to post a coordinate conversion request with successful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateConversionSuccessful()
        {
          CoordinateConversionRequest request = CoordinateConversionRequest.HelpSample;

          // Create the mock PDSClient with successful result...
           var mockRaptorClient = new Mock<IASNodeClient>();
           var mockLogger = new Mock<ILoggerFactory>();
          TCoordReturnCode raptorResult = TCoordReturnCode.nercNoError;

          TCoordPointList pointList;

          mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
            request.projectId ?? -1,
            It.IsAny<TWGS84FenceContainer>(),
            request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
            out pointList)).Returns(raptorResult);

          // Create an executor...
          CoordinateConversionExecutor executor = new CoordinateConversionExecutor(mockLogger.Object, mockRaptorClient.Object);

          ContractExecutionResult result = executor.Process(request);

          // Assert
          Assert.IsNotNull(result);
          Assert.IsTrue(result.Message == "success", result.Message);
        }

        /// <summary>
        ///  Uses the mock PDS client to post a coordinate conversion request with unsuccessful result...
        /// </summary>
        /// 
        [TestMethod]
        public void CS_CoordinateConversionFailed()
        {
          CoordinateConversionRequest request = CoordinateConversionRequest.HelpSample;

          // Create the mock PDSClient with successful result...
          var mockRaptorClient = new Mock<IASNodeClient>();
          var mockLogger = new Mock<ILoggerFactory>();

          TCoordReturnCode raptorResult = TCoordReturnCode.nercFailedToConvertCoords;

          TCoordPointList pointList;

          mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
            request.projectId ?? -1,
            It.IsAny<TWGS84FenceContainer>(),
            request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
            out pointList)).Returns(raptorResult);

          // Create an executor...
          CoordinateConversionExecutor executor = new CoordinateConversionExecutor(mockLogger.Object, mockRaptorClient.Object);

          Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
    #endregion
  }
}
