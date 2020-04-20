using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsExecutorAutoTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();
      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    // CCSSSCON-207 maybe some tests on the 2 device status?


    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.Parse(projectOfInterest.StartDate).AddDays(1));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new List<ProjectData>() { projectOfInterest };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (List<ProjectData>)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, radioSerialDeviceUid);


      await ExecuteAuto
        (getProjectAndAssetUidsRequest,
          //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_RadioSerialMapOverride()
    {
      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "123", string.Empty, 0, 0, DateTime.MinValue);
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult("896c7a36-e079-4b67-a79c-b209398f01ca", "b00c62b3-4eee-472e-9814-c31379e94bd5");

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object);
      executor.CustomRadioSerialMapper = ServiceProvider.GetService<ICustomRadioSerialProjectMap>();

      var result = await executor.ProcessAsync(getProjectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_EC520device_WithNoLicense_AndProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", "ec520Serial", 91, 181, DateTime.Parse(projectOfInterest.StartDate).AddDays(1));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = (DeviceData)null;
      var projectListForRadioSerial = new List<ProjectData>();
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 0 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = new DeviceData { CustomerUID = ec520AccountUid, DeviceUID = ec520Uid };
      var projectListForEC520 = new List<ProjectData>() { projectOfInterest };
      var ec520DeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, ec520Uid);


      await ExecuteAuto
        (getProjectAndAssetUidsRequest,
          //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 0, expectedMessage: "success"
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_EC520device_WithNoLicense_HasProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.Parse(projectOfInterest.StartDate).AddDays(1));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new List<ProjectData>() { projectOfInterest };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 0 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (List<ProjectData>)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
        (getProjectAndAssetUidsRequest,
          //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3001, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(1)
        );
    }


    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndNoProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.Parse(projectOfInterest.StartDate).AddDays(1));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new List<ProjectData>();
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (List<ProjectData>)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
        (getProjectAndAssetUidsRequest,
          //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3048, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(48)
        );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndTooManyProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };
      var projectOfInterest2 = new ProjectData
      {
        ProjectUID = Guid.NewGuid().ToString(),
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4).ToString(),
        EndDate = DateTime.UtcNow.AddDays(-3).ToString()
      };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.Parse(projectOfInterest.StartDate).AddDays(1));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new List<ProjectData>() { projectOfInterest, projectOfInterest2 };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (List<ProjectData>)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
        (getProjectAndAssetUidsRequest,
          //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
          radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
          ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
          ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
          expectedGetProjectAndAssetUidsResult, expectedCode: 3049, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(49)
        );
    }

    private async Task ExecuteAuto(GetProjectAndAssetUidsRequest request,
      DeviceData radioSerialDevice, List<ProjectData> projectListForRadioSerial, DeviceLicenseResponseModel radioSerialDeviceLicenseResponseModel,
      DeviceData ec520Device, List<ProjectData> projectListForEC520, DeviceLicenseResponseModel ec520DeviceLicenseResponseModel,
      ICustomRadioSerialProjectMap customRadioSerialMapper,
      GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult, int expectedCode, string expectedMessage
      )
    {
      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, null)).ReturnsAsync(radioSerialDevice);
      if (radioSerialDevice != null)
      {
        projectProxy.Setup(p => p.GetIntersectingProjectsApplicationContext(radioSerialDevice.CustomerUID, It.IsAny<double>(), It.IsAny<double>(), null, It.IsAny<DateTime?>(), null))
          .ReturnsAsync(projectListForRadioSerial);
        deviceProxy.Setup(d => d.GetProjectsForDevice(radioSerialDevice.DeviceUID, null)).ReturnsAsync(projectListForRadioSerial);
        cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(radioSerialDevice.CustomerUID), null)).ReturnsAsync(radioSerialDeviceLicenseResponseModel);
      }

      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, null)).ReturnsAsync(ec520Device);
      if (ec520Device != null)
      {
        projectProxy.Setup(p => p.GetIntersectingProjectsApplicationContext(ec520Device.CustomerUID, It.IsAny<double>(), It.IsAny<double>(), null, It.IsAny<DateTime?>(), null))
          .ReturnsAsync(projectListForEC520);
        deviceProxy.Setup(d => d.GetProjectsForDevice(ec520Device.DeviceUID, null)).ReturnsAsync(projectListForEC520);
        cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(ec520Device.CustomerUID), null)).ReturnsAsync(ec520DeviceLicenseResponseModel);
      }


      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object);
      executor.CustomRadioSerialMapper = customRadioSerialMapper;

      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, expectedCode, expectedMessage);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult,
      int resultCode, string resultMessage)
    {
      Assert.IsNotNull(actualResult, "executor returned nothing");
      Assert.AreEqual(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedGetProjectAndAssetUidsResult.DeviceUid, actualResult.DeviceUid, "executor returned incorrect DeviceUid");
      Assert.AreEqual(resultCode, actualResult.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, actualResult.Message, "executor returned incorrect result message");
    }
  }
}
