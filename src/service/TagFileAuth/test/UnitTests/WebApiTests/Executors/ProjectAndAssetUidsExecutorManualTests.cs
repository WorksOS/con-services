using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Common.Abstractions.Clients.CWS.Models;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsExecutorManualTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_ProjectAccountLicense_DeviceButNoLicense()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectAccountUid = Guid.NewGuid().ToString();
      var projectOfInterest = new ProjectData
                              {
                                ProjectUID = projectUid,
                                ProjectType = ProjectType.Standard,
                                CustomerUID = projectAccountUid,
                                StartDate = DateTime.UtcNow.AddDays(-4),
                                EndDate = DateTime.UtcNow.AddDays(-3)
                              };

      var assetUid = Guid.NewGuid().ToString();
      var assetAccountUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var ec520AccountUid = Guid.NewGuid().ToString();

      await Execute
      (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectAccountUid: projectAccountUid,
        projectDeviceLicenseResponseModel: new DeviceLicenseResponseModel() {Total = 1},
        assetUid: assetUid,
        assetDevice: new DeviceData { CustomerUID = assetAccountUid, DeviceUID = assetUid },
        ec520Uid: ec520Uid,
        ec520Device: (DeviceData)null,
        deviceAccountUid: assetAccountUid,
        deviceDeviceLicenseResponseModel: new DeviceLicenseResponseModel(),
        projectOfInterest: projectOfInterest,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    // todoMaverick
    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_940_And_Ec520()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: (new List<Subscription>
    //                          {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    }),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { projectOfInterest },
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: assetUid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_Ec520()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.EC520, string.Empty, "ec520Serial",  91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: (new List<Subscription>
    //                          {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    }),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { projectOfInterest },
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: ec520Uid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj__ProjectNotFound()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  Project projectOfInterest = null;

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty,91, 181, DateTime.UtcNow),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project>(),
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3038,
    //    expectedMessageResult: "Unable to find the Project requested"
    //  );

    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_AssetCustMan3d()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>
    //                       {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    },
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: assetUid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3039,
    //    expectedMessageResult: "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_Asset3dSub_MatchesProjectCustomer()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = projectCustomerUid },
    //    assetSubs: new List<Subscription>
    //               {
    //      new Subscription
    //      {
    //        ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
    //        CustomerUID = projectCustomerUid
    //      }
    //    },
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: assetUid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_Asset3dSub_NoMatchForProjectCustomer()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
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
    //        CustomerUID = assetCustomerUid
    //      }
    //    },
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3039,
    //    expectedMessageResult: "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_PrjMan3d_ProjectDoesntIntersectSpatially()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty,  91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>
    //                         {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    },
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> { },
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3041,
    //    expectedMessageResult: "Manual Import: no intersecting projects found"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_TimeOutsideProjectDates()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>
    //                         {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    },
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: assetUid,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_ProjectIsDeleted()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3),
    //    IsDeleted = true
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3043,
    //    expectedMessageResult: "Manual Import: cannot import to an archived project"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub_NoRadioSerial()
    //{
    //  // standard Project requires a known asset, if project has no Man3d

    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3),
    //    IsDeleted = true
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>(),
    //    assetUid: assetUid,
    //    assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: string.Empty,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 3043,
    //    expectedMessageResult: "Manual Import: cannot import to an archived project"
    //  );
    //}


    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_DeviceNotFound()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.SNM940, "snm940Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>
    //                         {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    },
    //    assetUid: assetUid,
    //    assetDevice: (AssetDeviceIds)null,
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    //[TestMethod]
    //public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_ManualDeviceType()
    //{
    //  var projectUid = Guid.NewGuid().ToString();
    //  var projectCustomerUid = Guid.NewGuid().ToString();
    //  var projectOfInterest = new Project
    //                          {
    //    ProjectUID = projectUid,
    //    ProjectType = ProjectType.Standard,
    //    CustomerUID = projectCustomerUid,
    //    StartDate = DateTime.UtcNow.AddDays(-4),
    //    EndDate = DateTime.UtcNow.AddDays(-3)
    //  };

    //  string assetUid = Guid.NewGuid().ToString();
    //  string ec520Uid = Guid.NewGuid().ToString();
    //  string assetCustomerUid = Guid.NewGuid().ToString();

    //  await Execute
    //  (request: new GetProjectAndAssetUidsRequest(projectUid, (int)TagFileDeviceTypeEnum.ManualImport, String.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
    //    projectUid: projectUid,
    //    projectCustomerUid: projectCustomerUid,
    //    projectCustomerSubs: new List<Subscription>
    //                         {
    //      new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
    //    },
    //    assetUid: assetUid,
    //    assetDevice: (AssetDeviceIds)null,
    //    assetSubs: new List<Subscription>(),
    //    ec520Uid: ec520Uid,
    //    ec520Device: (AssetDeviceIds)null,
    //    ec520Subs: new List<Subscription>(),
    //    assetCustomerUid: assetCustomerUid,
    //    assetCustomerSubs: new List<Subscription>(),
    //    projectOfInterest: projectOfInterest,
    //    intersectingProjects: new List<Project> {projectOfInterest},
    //    expectedProjectUidResult: projectUid,
    //    expectedAssetUidResult: string.Empty,
    //    expectedCodeResult: 0,
    //    expectedMessageResult: "success"
    //  );
    //}

    [TestMethod]
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), ConfigStore,
         cwsAccountClient.Object, projectProxy.Object, deviceProxy.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() =>
        executor.ProcessAsync((GetProjectAndAssetUidsRequest) null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

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
      cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(projectAccountUid), null)).ReturnsAsync(projectDeviceLicenseResponseModel);
      cwsAccountClient.Setup(p => p.GetDeviceLicenses(new Guid(deviceAccountUid), null)).ReturnsAsync(deviceDeviceLicenseResponseModel);

      projectProxy.Setup(p => p.GetProjectApplicationContext(projectUid, null)).ReturnsAsync(projectOfInterest);
      projectProxy.Setup(p => p.GetIntersectingProjectsApplicationContext(projectAccountUid, It.IsAny<double>(), It.IsAny<double>(), projectUid, null, null))
            .ReturnsAsync(new List<ProjectData>() { projectOfInterest });

      deviceProxy.Setup(d => d.GetDevice(request.RadioSerial, null)).ReturnsAsync(assetDevice);
      deviceProxy.Setup(d => d.GetDevice(request.Ec520Serial, null)).ReturnsAsync(ec520Device);
      deviceProxy.Setup(d => d.GetProjectsForDevice(assetUid, null)).ReturnsAsync(new List<ProjectData>() { projectOfInterest });


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
