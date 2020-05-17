using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.RadioSerialMap;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

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

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, radioSerialDeviceUid);


      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
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
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
      executor.CustomRadioSerialMapper = ServiceProvider.GetService<ICustomRadioSerialProjectMap>();

      var result = await executor.ProcessAsync(getProjectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_EC520device_WithNoLicense_AndProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", "ec520Serial", 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = (DeviceData)null;
      var projectListForRadioSerial = new ProjectDataResult();
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 0 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = new DeviceData { CustomerUID = ec520AccountUid, DeviceUID = ec520Uid };
      var projectListForEC520 = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var ec520DeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(projectUid, ec520Uid);


      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
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
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };

      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 0 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
        ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
        ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
        expectedGetProjectAndAssetUidsResult, expectedCode: 3001, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(1)
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_EC520device_DeviceNotActive()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { Code = 100, Message = "Unable to locate device by serialNumber in cws" };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest } };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
        ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
        ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
        expectedGetProjectAndAssetUidsResult, expectedCode: 3100, expectedMessage: "Unable to locate device by serialNumber in cws"
      );
    }


    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndNoProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult();
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
        ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
        ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
        expectedGetProjectAndAssetUidsResult, expectedCode: 3044, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(44)
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_CBdevice_WithLicense_AndTooManyProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };
      var projectOfInterest2 = new ProjectData { ProjectUID = Guid.NewGuid().ToString(), ProjectType = ProjectType.Standard, CustomerUID = projectAccountUid };

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, DateTime.UtcNow.AddDays(-3));

      var radioSerialDeviceUid = Guid.NewGuid().ToString();
      var radioSerialAccountUid = Guid.NewGuid().ToString();
      var radioSerialDevice = new DeviceData { CustomerUID = radioSerialAccountUid, DeviceUID = radioSerialDeviceUid };
      var projectListForRadioSerial = new ProjectDataResult() { ProjectDescriptors = new List<ProjectData>() { projectOfInterest, projectOfInterest2 } };
      var radioSerialDeviceLicenseResponseModel = new DeviceLicenseResponseModel() { Total = 1 };

      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      var ec520Device = (DeviceData)null;
      var projectListForEC520 = (ProjectDataResult)null;
      var ec520DeviceLicenseResponseModel = (DeviceLicenseResponseModel)null;

      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, radioSerialDeviceUid);


      await ExecuteAuto
      (getProjectAndAssetUidsRequest,
        //projectAccountUid, projectForProjectUid, projectListForProjectAccountUid, projectDeviceLicenseResponseModel,
        radioSerialDeviceUid, radioSerialAccountUid, radioSerialDevice, projectListForRadioSerial, radioSerialDeviceLicenseResponseModel,
        ec520Device, projectListForEC520, ec520DeviceLicenseResponseModel,
        ServiceProvider.GetService<ICustomRadioSerialProjectMap>(),
        expectedGetProjectAndAssetUidsResult, expectedCode: 3049, expectedMessage: ContractExecutionStatesEnum.FirstNameWithOffset(49)
      );
    }

    private async Task ExecuteAuto(GetProjectAndAssetUidsRequest request,
      string radioSerialDeviceUid, string radioSerialcustomerUid, DeviceData radioSerialDevice, ProjectDataResult projectListForRadioSerial, DeviceLicenseResponseModel radioSerialDeviceLicenseResponseModel,
      DeviceData ec520Device, ProjectDataResult projectListForEC520, DeviceLicenseResponseModel ec520DeviceLicenseResponseModel,
      ICustomRadioSerialProjectMap customRadioSerialMapper,
      GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult, int expectedCode, string expectedMessage
    )
    {
      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, It.IsAny<HeaderDictionary>())).ReturnsAsync(radioSerialDevice);
      if (radioSerialDevice != null)
      {
        projectProxy.Setup(p => p.GetIntersectingProjects(radioSerialDevice.CustomerUID, It.IsAny<double>(), It.IsAny<double>(), null, It.IsAny<HeaderDictionary>()))
          .ReturnsAsync(projectListForRadioSerial);
        deviceProxy.Setup(d => d.GetProjectsForDevice(radioSerialDeviceUid, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForRadioSerial);
        cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(radioSerialcustomerUid), It.IsAny<HeaderDictionary>())).ReturnsAsync(radioSerialDeviceLicenseResponseModel);
      }

      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, It.IsAny<HeaderDictionary>())).ReturnsAsync(ec520Device);
      if (ec520Device != null)
      {
        projectProxy.Setup(p => p.GetIntersectingProjects(ec520Device.CustomerUID, It.IsAny<double>(), It.IsAny<double>(), null, It.IsAny<HeaderDictionary>()))
          .ReturnsAsync(projectListForEC520);
        deviceProxy.Setup(d => d.GetProjectsForDevice(ec520Device.DeviceUID, It.IsAny<HeaderDictionary>())).ReturnsAsync(projectListForEC520);
        cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(ec520Device.CustomerUID), It.IsAny<HeaderDictionary>())).ReturnsAsync(ec520DeviceLicenseResponseModel);
      }

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore, authorization.Object,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object, requestCustomHeaders);
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
