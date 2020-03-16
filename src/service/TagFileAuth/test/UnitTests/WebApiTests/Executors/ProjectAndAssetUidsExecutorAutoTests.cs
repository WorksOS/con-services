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
      var projectOfInterest = new ProjectData
                              {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectAccountUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();

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
        deviceAccountLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<ProjectData> {projectOfInterest},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    // todoMaverick
    //[TestMethod]
    //public async Task TRexExecutor_Auto_Happy_Ec520()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int) TagFileDeviceTypeEnum.EC520, string.Empty, "ec520Serial", 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { projectOfInterest },
    //    customerTccOrg: (CustomerTccOrg)null,
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: ec520Uid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Auto_Sad_CBdevice_NoneFound()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int) TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    ec520Uid: ec520Uid,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project>(),
    //    customerTccOrg: (CustomerTccOrg)null,
    //    expectedProjectUidResult: string.Empty, 
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3048,
    //    expectedMessageResult: "Auto Import: for this serialNumber/TCCorgId, no project meets the time/location/subscription requirements"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Auto_Sad_CBdevice_NoAssetFound()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int) TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: (AssetDeviceIds)null, 
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    ec520Uid: String.Empty,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { projectOfInterest },
    //    customerTccOrg: (CustomerTccOrg)null,
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3047,
    //    expectedMessageResult: "Auto Import: no asset or tccOrgId is identifiable from the request"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Auto_Sad_CBdevice_NoRadioSerial()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int) TagFileDeviceTypeEnum.SNM940, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    ec520Uid: String.Empty,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { projectOfInterest },
    //    customerTccOrg: (CustomerTccOrg)null, 
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3047,
    //    expectedMessageResult: "Auto Import: no asset or tccOrgId is identifiable from the request"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Auto_Sad_CBdevice_TimeOutsideProject()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int) TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-99)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    ec520Uid: String.Empty,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project>(),
    //    customerTccOrg: (CustomerTccOrg)null, 
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3048,
    //    expectedMessageResult: "Auto Import: for this serialNumber/TCCorgId, no project meets the time/location/subscription requirements"
    //  );
    //}
    
    //[TestMethod]
    //public async Task TRexExecutor_Auto_Sad_TooManyProjects()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterestStd = new Project
    //                             {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3)
    //  };
    //  var projectOfInterestPM = new Project
    //                            {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(3),
    //    ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
    //    SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
    //    SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(string.Empty, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", "tccOrgId", 91, 181, projectOfInterestStd.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        StartDate = DateTime.UtcNow.AddYears(-1).Date,
    //        EndDate = new DateTime(9999, 12, 31).Date
    //      }
    //    },
    //    ec520Uid: string.Empty,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterestStd,
    //    intersectingProjects: new List<Project> { projectOfInterestStd, projectOfInterestPM, },
    //    customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterestPM.CustomerUID },
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3049,
    //    expectedMessageResult: "More than 1 project meets the time/location/subscription requirements"
    //  );
    //}


    private async Task Execute(GetProjectAndAssetUidsRequest request,
      string projectUid, string projectAccountUid, DeviceLicenseResponseModel projectDeviceLicenseResponseModel,
      string assetUid, DeviceData assetDevice, 
      string ec520Uid, DeviceData ec520Device, 
      string deviceAccountUid, DeviceLicenseResponseModel deviceAccountLicenseResponseModel,
      ProjectData projectOfInterest, List<ProjectData> intersectingProjects,
      string expectedProjectUidResult, string expectedAssetUidResult, int expectedCodeResult,
      string expectedMessageResult
    )
    {
      customerProxy.Setup(d => d.GetDeviceLicenses(projectAccountUid))
        .ReturnsAsync(projectDeviceLicenseResponseModel);
      customerProxy.Setup(d => d.GetDeviceLicenses(deviceAccountUid))
        .ReturnsAsync(deviceAccountLicenseResponseModel);
      
      projectProxy.Setup(d => d.GetIntersectingProjects(deviceAccountUid, It.IsAny<double>(), It.IsAny<double>(), null, It.IsAny<DateTime>(), null))
        .ReturnsAsync(new List<ProjectData> { projectOfInterest });

      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial)).ReturnsAsync(assetDevice);
      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial)).ReturnsAsync(ec520Device);
      deviceProxy.Setup(d => d.GetProjects(assetUid)).ReturnsAsync(new List<ProjectData> { projectOfInterest });

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore,
        projectProxy.Object, customerProxy.Object, deviceProxy.Object);
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
