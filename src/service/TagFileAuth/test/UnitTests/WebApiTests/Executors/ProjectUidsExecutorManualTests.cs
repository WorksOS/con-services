using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
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
  public class ProjectUidsExecutorManualTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "ec520Serial", 91, 181);
      var projectForProjectUid = (ProjectData)null;

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteManual
        (getProjectUidsRequest, projectForProjectUid,
        platformSerialDevice, projectListForPlatformSerial,
          null,
        expectedResult, expectedCode: 3038, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(38)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectArchived()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = CwsProjectType.AcceptsTagFiles,
        CustomerUID = projectAccountUid,
        IsArchived = true
      };

      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "CB450", 91, 181);
      var projectForProjectUid = projectOfInterest;

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteManual
        (getProjectUidsRequest, projectForProjectUid,
         platformSerialDevice, projectListForPlatformSerial,
          null,
         expectedResult, expectedCode: 3043, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(43)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_ProjectDoesntIntersect()
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

      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "CB450", outsideLat, insideLong);
      var projectForProjectUid = projectOfInterest;

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var expectedResult = new GetProjectUidsResult(string.Empty, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteManual
        (getProjectUidsRequest, projectForProjectUid,
         platformSerialDevice, projectListForPlatformSerial,
          null,
         expectedResult, expectedCode: 3041, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(41)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectAndDevice()
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

      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "ec520Serial", insideLat, insideLong);
      var projectForProjectUid = projectOfInterest;

      var platformSerialDeviceUid = Guid.NewGuid().ToString();
      var platformSerialAccountUid = Guid.NewGuid().ToString();
      var platformSerialDevice = new DeviceData { CustomerUID = platformSerialAccountUid, DeviceUID = platformSerialDeviceUid };
      var projectListForPlatformSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var expectedResult = new GetProjectUidsResult(projectUid, platformSerialDeviceUid, platformSerialAccountUid);

      await ExecuteManual
      (getProjectUidsRequest, projectForProjectUid,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_Project_NoDevice_UsingLL()
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

      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "CB450", insideLat, insideLong);
      var projectForProjectUid = projectOfInterest;

      var platformSerialDevice = (DeviceData)null;
      var projectListForPlatformSerial = (ProjectDataResult)null;

      var expectedResult = new GetProjectUidsResult(projectUid, string.Empty, string.Empty);

      await ExecuteManual
      (getProjectUidsRequest, projectForProjectUid,
        platformSerialDevice, projectListForPlatformSerial,
        null,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_Project_NoDevice_UsingNE()
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
      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "cb460Serial", 0, 0, northing, easting);
      // expected convertNEtoLL result
      var points = new[] { new TwoDConversionCoordinate(insideLong.LonDegreesToRadians(), insideLat.LatDegreesToRadians()) };
      var coordinateConversionResult = new CoordinateConversionResult(points);

      var projectForProjectUid = projectOfInterest;

      var platformSerialDevice = (DeviceData)null;
      var projectListForplatformSerial = (ProjectDataResult)null;

      var expectedResult = new GetProjectUidsResult(projectUid, string.Empty, string.Empty);

      await ExecuteManual
      (getProjectUidsRequest, projectForProjectUid,
        platformSerialDevice, projectListForplatformSerial,
        coordinateConversionResult,
        expectedResult, expectedCode: 0, expectedMessage: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_Project_NoValidPosition_Calculated()
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
      var getProjectUidsRequest = new GetProjectUidsRequest(projectUid, "cb450serial", 0, 0, northing, easting);
      var coordinateConversionResult = new CoordinateConversionResult(new TwoDConversionCoordinate[0]);

      var projectForProjectUid = projectOfInterest;

      var platformSerialDevice = (DeviceData)null;
      var projectListForPlatformSerial = (ProjectDataResult)null;

      var expectedResult = new GetProjectUidsResult(string.Empty, string.Empty, string.Empty);

      await ExecuteManual
      (getProjectUidsRequest, projectForProjectUid,
        platformSerialDevice, projectListForPlatformSerial,
        coordinateConversionResult,
        expectedResult, expectedCode: 3018, expectedMessage: "Manual Import: Unable to determine lat/long from northing/easting position"
      );
    }


    [TestMethod]
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectUidsExecutorManualTests>(), ConfigStore, authorization.Object,
         projectProxy.Object, deviceProxy.Object, tRexCompactionDataProxy.Object, requestCustomHeaders);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() =>
        executor.ProcessAsync((GetProjectUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    private async Task ExecuteManual(GetProjectUidsRequest request, ProjectData projectForProjectUid,
      DeviceData platformDevice, ProjectDataResult projectListForPlatform,
      CoordinateConversionResult coordinateConversionResult,
      GetProjectUidsResult expectedResult, int expectedCode, string expectedMessage
  )
    {
      projectProxy.Setup(p => p.GetProject(request.ProjectUid, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectForProjectUid);

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
