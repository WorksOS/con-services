using System.IO;
using System.Threading.Tasks;
using ASNodeDecls;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VLPDDecls;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.WebApi.Models.Coord.Executors;

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
    public async Task CS_CoordinateSystemControllerPostSuccessful()
    {
      byte[] csFileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      var request = new CoordinateSystemFile(PD_MODEL_ID, csFileContent, "dummy.dc");

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneOK;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
        new MemoryStream(request.CSFileContent),
        request.CSFileName,
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      var result = await executor.ProcessAsync(request);

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
      byte[] csFileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      var request = new CoordinateSystemFile(PD_MODEL_ID, csFileContent, "dummy.dc");

      // Create the mock PDSClient with unsuccessful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneCouldNotConvertCSDFile;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
         It.IsAny<MemoryStream>(),
        request.CSFileName,
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
    #endregion

    #region PostValidate

    /// <summary>
    ///  Uses the mock PDS client to post a CS file with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public async Task CS_CoordinateSystemControllerPostValidateSuccessful()
    {
      byte[] csFileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      var request = new CoordinateSystemFileValidationRequest(csFileContent, "dummy.dc");

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneOK;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
        new MemoryStream(request.CSFileContent),
        request.CSFileName,
        -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      var result = await executor.ProcessAsync(request);

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
      byte[] csFileContent = { 0, 1, 2, 3, 4, 5, 6, 7 };

      var request = new CoordinateSystemFileValidationRequest(csFileContent, "dummy.dc");

      // Create the mock PDSClient with unsuccessful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneCouldNotConvertCSDFile;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.PassSelectedCoordinateSystemFile(
         It.IsAny<MemoryStream>(),
        request.CSFileName,
        -1, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorPost>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }

    #endregion

    #region Get

    /// <summary>
    ///  Uses the mock PDS client to get CS settings with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public async Task CS_CoordinateSystemControllerGetSuccessful()
    {
      var request = new ProjectID(PD_MODEL_ID);

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneOK;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.ProjectId.Value, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      var result = await executor.ProcessAsync(request);

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
      var request = new ProjectID(PD_MODEL_ID);

      // Create the mock PDSClient with unsuccessful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TASNodeErrorStatus.asneNoSuchDataModel;

      TCoordinateSystemSettings csSettings;

      mockRaptorClient.Setup(prj => prj.RequestCoordinateSystemDetails(request.ProjectId.Value, out csSettings)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateSystemExecutorGet>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));

    }
    #endregion

    #region Coordinate Conversion

    /// <summary>
    ///  Uses the mock PDS client to post a coordinate conversion request with successful result...
    /// </summary>
    /// 
    [TestMethod]
    public async Task CS_CoordinateConversionSuccessful()
    {
      var request = new CoordinateConversionRequest(1, TwoDCoordinateConversionType.NorthEastToLatLon,
          new[]
          {
                new TwoDConversionCoordinate(381043.710, 807625.050),
                new TwoDConversionCoordinate(381821.617, 807359.462),
                new TwoDConversionCoordinate(380781.358, 806969.174),
          });

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TCoordReturnCode.nercNoError;

      TCoordPointList pointList;

      mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        It.IsAny<TWGS84FenceContainer>(),
        request.ConversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
        out pointList)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      var result = await executor.ProcessAsync(request);

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
      var request = new CoordinateConversionRequest(1, TwoDCoordinateConversionType.NorthEastToLatLon,
        new[]
        {
          new TwoDConversionCoordinate(381043.710, 807625.050),
          new TwoDConversionCoordinate(381821.617, 807359.462),
          new TwoDConversionCoordinate(380781.358, 806969.174),
        });

      // Create the mock PDSClient with successful result...
      var mockRaptorClient = new Mock<IASNodeClient>();
      var mockLogger = new Mock<ILoggerFactory>();
      var mockConfigStore = new Mock<IConfigurationStore>();

      var raptorResult = TCoordReturnCode.nercFailedToConvertCoords;

      TCoordPointList pointList;

      mockRaptorClient.Setup(prj => prj.GetGridCoordinates(
        request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        It.IsAny<TWGS84FenceContainer>(),
        request.ConversionType == TwoDCoordinateConversionType.LatLonToNorthEast ? TCoordConversionType.ctLLHtoNEE : TCoordConversionType.ctNEEtoLLH,
        out pointList)).Returns(raptorResult);

      // Create an executor...
      var executor = RequestExecutorContainerFactory.Build<CoordinateConversionExecutor>(mockLogger.Object, mockRaptorClient.Object, configStore: mockConfigStore.Object);

      Assert.ThrowsExceptionAsync<ServiceException>(async () => await executor.ProcessAsync(request));
    }
    #endregion
  }
}
