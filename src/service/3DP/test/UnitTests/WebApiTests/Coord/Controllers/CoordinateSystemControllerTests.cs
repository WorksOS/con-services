using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Coord.Executors;
using VSS.Productivity3D.WebApiModels.Coord.Models;

namespace VSS.Productivity3D.WebApiTests.Coord.Controllers
{
  [TestClass]
  public class CoordinateSystemControllerTests
  {
    private const long PD_MODEL_ID = 544; // Dimensions 2012 project...



    #region Post

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
        request.ProjectId ?? -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorPost executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
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
        request.ProjectId ?? -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorPost executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
    #endregion

    #region PostValidate

    /// <summary>
    ///  Uses the mock PDS client to post a CS file with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateSystemControllerPostValidateSuccessful()
    {
      byte[] csFileContent = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

      CoordinateSystemFileValidationRequest request = CoordinateSystemFileValidationRequest.CreateCoordinateSystemFileValidationRequest(csFileContent, "dummy.dc");

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneOK;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
        new MemoryStream(request.csFileContent),
        request.csFileName,
        -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorPost executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    /// <summary>
    /// Uses the mock PDS client to post a CS file with unsuccessful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateSystemControllerPostValidationFailed()
    {
      byte[] csFileContent = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 };

      CoordinateSystemFileValidationRequest request = CoordinateSystemFileValidationRequest.CreateCoordinateSystemFileValidationRequest(csFileContent, "dummy.dc");

      // Create the mock PDSClient with unsuccessful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneCouldNotConvertCSDFile;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
         It.IsAny<MemoryStream>(),
        request.csFileName,
        -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorPost executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }

    #endregion

    #region Get

    /// <summary>
    ///  Uses the mock PDS client to get CS settings with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateSystemControllerGetSuccessful()
    {
      ProjectID request = new ProjectID(PD_MODEL_ID);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneOK;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.ProjectId.Value, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorGet executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    /// <summary>
    ///  Uses the mock PDS client to get CS settings with unsuccessful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateSystemControllerGettFailed()
    {
      ProjectID request = new ProjectID(PD_MODEL_ID);

      // Create the mock PDSClient with unsuccessful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      TASNodeErrorStatus raptorResult = TASNodeErrorStatus.asneNoSuchDataModel;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.ProjectId.Value, out csSettings)).Returns(raptorResult);

      // Create an executor...
      CoordinateSystemExecutorGet executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));

    }
    #endregion

    #region Coordinate Conversion

    /// <summary>
    ///  Uses the mock PDS client to post a coordinate conversion request with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateConversionSuccessful()
    {
      var request = CoordinateConversionRequest.CreateCoordinateConversionRequest(1, TwoDCoordinateConversionType.NorthEastToLatLon,
          new[]
          {
                TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381043.710, 807625.050),
                TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381821.617, 807359.462),
                TwoDConversionCoordinate.CreateTwoDConversionCoordinate(380781.358, 806969.174),
          });

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      TCoordReturnCode raptorResult = TCoordReturnCode.nercNoError;

      TCoordPointList pointList;

      mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
        request.ProjectId ?? -1,
        It.IsAny<TWGS84FenceContainer>(),
        request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
        out pointList)).Returns(raptorResult);

      // Create an executor...
      CoordinateConversionExecutor executor = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(mockLogger.Object, mockRaptorClient.Object);

      ContractExecutionResult result = executor.Process(request);

      // Assert
      Assert.IsNotNull(result);
      Assert.IsTrue(result.Message == ContractExecutionResult.DefaultMessage, result.Message);
    }

    /// <summary>
    ///  Uses the mock PDS client to post a coordinate conversion request with unsuccessful result...
    /// </summary>
    /// 
    [TestMethod]
    public void CS_CoordinateConversionFailed()
    {
      var request = CoordinateConversionRequest.CreateCoordinateConversionRequest(1, TwoDCoordinateConversionType.NorthEastToLatLon,
        new[]
        {
              TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381043.710, 807625.050),
              TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381821.617, 807359.462),
              TwoDConversionCoordinate.CreateTwoDConversionCoordinate(380781.358, 806969.174),
        });

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();

      TCoordReturnCode raptorResult = TCoordReturnCode.nercFailedToConvertCoords;

      TCoordPointList pointList;

      mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
        request.ProjectId ?? -1,
        It.IsAny<TWGS84FenceContainer>(),
        request.conversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
        out pointList)).Returns(raptorResult);

      // Create an executor...
      CoordinateConversionExecutor executor = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(mockLogger.Object, mockRaptorClient.Object);

      Assert.ThrowsException<ServiceException>(() => executor.Process(request));
    }
    #endregion
  }
}
