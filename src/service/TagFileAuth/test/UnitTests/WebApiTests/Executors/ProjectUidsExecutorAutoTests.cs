using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Productivity3D.Models.Models.Coords;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectUidsExecutorAutoTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();
      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "CB450Serial", insideLat, insideLong);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(projectUid, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_EC520device_WithProject_UsingLL()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "ec520Serial", insideLat, insideLong);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(projectUid, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_EC520device_WithProject_UsingNE()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var northing = 67.8;
      var easting = 21.3;
      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "ec520Serial", 0.0, 0.0, northing, easting);
      // expected convertNEtoLL result
      var points = new [] {new TwoDConversionCoordinate(insideLong, insideLat)};
      var coordinateConversionResult = new CoordinateConversionResult(points);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(projectUid, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        coordinateConversionResult,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_EC520device_WithProject_NoValidPosition_Calculated()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var northing = 67.8;
      var easting = 21.3;
      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "ec520Serial", 0.0, 0.0, northing, easting);
      var coordinateConversionResult = new CoordinateConversionResult(new TwoDConversionCoordinate[0]);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        coordinateConversionResult,
        expectedResult, expectedCode: 3044, expectedMessage: "No projects found at the location provided"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_EC520device_DeviceNotActive()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "cb450serial", 91, 181);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { Code = 100, Message = "Unable to locate device by serialNumber in cws" };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(string.Empty, string.Empty, string.Empty);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 3100, expectedMessage: "Unable to locate device by serialNumber in cws"
      );
    }


    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithNoProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "cb460serial", 91, 181);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult();

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 3048, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(48)
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithNoOverlappingProjects()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "CB460Serial", 91, 181);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 3044, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(44)
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_TooManyProjects()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };
      var projectOfInterest2 = new ProjectData
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = false,
        ProjectGeofenceWKT = projectBoundary
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(string.Empty, "CB450Serial", insideLat, insideLong);

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest, projectOfInterest2 } };

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);
      
      await ExecuteAuto
      (getProjectUidsRequest,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 3049, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(49)
      );
    }

    private async Task ExecuteAuto(GetProjectUidsRequest request,
      DeviceData platformDevice, ProjectDataResult projectListForPlatform,
      CoordinateConversionResult coordinateConversionResult,
      GetProjectUidsResult expectedResult, int expectedCode, string expectedMessage
    )
    {
      deviceProxy.Setup(d => d.GetDevice(request.PlatformSerial, It.IsAny<HeaderDictionary>())).ReturnsAsync(platformDevice);
      if (platformDevice != null)
        deviceProxy.Setup(d => d.GetProjectsForDevice(platformDevice.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForPlatform);

      if (coordinateConversionResult != null)
        tRexCompactionDataProxy.Setup(x => x.SendDataPostRequest<CoordinateConversionResult, CoordinateConversionRequest>(
          It.IsAny<CoordinateConversionRequest>(),
          It.IsAny<string>(),
          It.IsAny<HeaderDictionary>(), false))
        .ReturnsAsync(coordinateConversionResult);

      var executor = RequestExecutorContainer.Build<ProjectUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectUidsExecutorManualTests>(), ConfigStore, authorization.Object,
        projectProxy.Object, deviceProxy.Object, tRexCompactionDataProxy.Object, requestCustomHeaders);

      var result = await executor.ProcessAsync(request) as GetProjectUidsResult;

      ValidateResult(result, expectedResult, expectedCode, expectedMessage);
    }

    private void ValidateResult(GetProjectUidsResult actualResult, GetProjectUidsResult expectedResult,
      int resultCode, string resultMessage)
    {
      Assert.IsNotNull(actualResult, "executor returned nothing");
      Assert.AreEqual(expectedResult.ProjectUid, actualResult.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedResult.DeviceUid, actualResult.DeviceUid, "executor returned incorrect DeviceUid");
      Assert.AreEqual(expectedResult.CustomerUid, actualResult.CustomerUid, "executor returned incorrect CustomerUid");
      Assert.AreEqual(resultCode, actualResult.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, actualResult.Message, "executor returned incorrect result message");
    }
  }
}
