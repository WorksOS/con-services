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
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

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
    public async Task TRexExecutor_Auto_Happy_CBdevice()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();
      
      var projectOfInterest = new ProjectData
                              {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

    
      await Execute
      (request: new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectAccountUid: projectAccountUid,
        projectDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        assetUid: assetUid,
        assetDevice: new DeviceData { CustomerUID = assetCustomerUid, DeviceUID = assetUid },
        ec520Uid: ec520Uid,
        ec520Device: (DeviceData)null,
        deviceAccountUid: assetCustomerUid,
        deviceDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: projectOfInterest,       
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }
    
    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_Ec520()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520CustomerUid = Guid.NewGuid().ToString();
      
      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = ec520CustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      await Execute
      (request: new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.EC520, string.Empty, "ec520Serial", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectAccountUid: projectAccountUid,
        projectDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        assetUid: assetUid,
        assetDevice: new DeviceData { CustomerUID = assetCustomerUid, DeviceUID = assetUid },
        ec520Uid: ec520Uid,
        ec520Device: new DeviceData { CustomerUID = ec520CustomerUid, DeviceUID = ec520Uid },
        deviceAccountUid: ec520CustomerUid,
        deviceDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: projectOfInterest,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: ec520Uid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_CBdevice_NoDeviceFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();

      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };


      await Execute
      (request: new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectAccountUid: projectAccountUid,
        projectDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        assetUid: assetUid,
        assetDevice: null,
        ec520Uid: ec520Uid,
        ec520Device: (DeviceData)null,
        deviceAccountUid: assetCustomerUid,
        deviceDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: projectOfInterest,
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3047,
        expectedMessageResult: "Auto Import: no asset or tccOrgId is identifiable from the request"
      );
    }

    /* todoMaverick
    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_CBdevice_NoProjectFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();

      var projectOfInterest = new ProjectData
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };


      await Execute
      (request: new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectAccountUid: projectAccountUid,
        projectDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        assetUid: assetUid,
        assetDevice: new DeviceData { CustomerUID = assetCustomerUid, DeviceUID = assetUid },
        ec520Uid: ec520Uid,
        ec520Device: (DeviceData)null,
        deviceAccountUid: assetCustomerUid,
        deviceDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: null,
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: for this serialNumber/TCCorgId, no project meets the time/location/subscription requirements"
      );
    }
    */


    private async Task Execute(GetProjectAndAssetUidsRequest request,
      string projectUid, string projectAccountUid, DeviceLicenseResponseModel projectDeviceLicenseResponseModel,
      string assetUid, DeviceData assetDevice, 
      string ec520Uid, DeviceData ec520Device, 
      string deviceAccountUid, DeviceLicenseResponseModel deviceDeviceLicenseResponseModel,
      ProjectData projectOfInterest, 
      string expectedProjectUidResult, string expectedAssetUidResult, int expectedCodeResult,
      string expectedMessageResult
    )
    {
      cwsAccountClient.Setup(d => d.GetDeviceLicenses(projectAccountUid, null)).ReturnsAsync(projectDeviceLicenseResponseModel);
      cwsAccountClient.Setup(d => d.GetDeviceLicenses(deviceAccountUid, null)).ReturnsAsync(deviceDeviceLicenseResponseModel);
      
      projectProxy.Setup(d => d.GetIntersectingProjectsApplicationContext(deviceAccountUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string>(), It.IsAny<DateTime>(), null))
        .ReturnsAsync(new List<ProjectData> { projectOfInterest });

      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, null)).ReturnsAsync(assetDevice);
      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, null)).ReturnsAsync(ec520Device);
      deviceProxy.Setup(d => d.GetProjects(assetUid, null)).ReturnsAsync(new List<ProjectData>() { projectOfInterest });
      deviceProxy.Setup(d => d.GetProjects(ec520Uid, null)).ReturnsAsync(new List<ProjectData>() { projectOfInterest });

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore,
        cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedProjectUidResult, expectedAssetUidResult, expectedCodeResult,
        expectedMessageResult);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult result, string expectedProjectUid, string expectedAssetUid,
      int resultCode, string resultMessage)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.DeviceUid, "executor returned incorrect DeviceUid");
      Assert.AreEqual(resultCode, result.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, result.Message, "executor returned incorrect result message");
    }
  }
}
