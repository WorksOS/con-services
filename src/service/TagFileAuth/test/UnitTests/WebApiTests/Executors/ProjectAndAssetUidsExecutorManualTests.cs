using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsExecutorManualTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;

    private Mock<IAssetRepository> _assetRepo;
    private Mock<IDeviceRepository> _deviceRepo;
    private Mock<ICustomerRepository> _customerRepo;
    private Mock<ISubscriptionRepository> _subscriptionRepo;
    private Mock<IProjectRepository> _projectRepo;

    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      _assetRepo = new Mock<IAssetRepository>();
      _deviceRepo = new Mock<IDeviceRepository>();
      _customerRepo = new Mock<ICustomerRepository>();
      _projectRepo = new Mock<IProjectRepository>();
      _subscriptionRepo = new Mock<ISubscriptionRepository>();
    }

    #region StandardProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: (new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        }),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj__ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      Project projectOfInterest = null;

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, DateTime.UtcNow),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3038,
        expectedMessageResult: "Unable to find the Project requested"
      );

    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_AssetCustMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3039,
        expectedMessageResult: "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_Asset3dSub_MatchesProjectCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            CustomerUID = projectCustomerUid
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = projectCustomerUid },
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_Asset3dSub_NoMatchForProjectCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            CustomerUID = assetCustomerUid
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3039,
        expectedMessageResult: "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_PrjMan3d_ProjectDoesntIntersectSpatially()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { },
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3041,
        expectedMessageResult: "Manual Import: no intersecting projects found"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_TimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_ProjectIsDeleted()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3),
        IsDeleted = true
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3043,
        expectedMessageResult: "Manual Import: cannot import to an archived project"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub_NoRadioSerial()
    {
      // standard Project requires a known asset, if project has no Man3d

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3),
        IsDeleted = true
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3043,
        expectedMessageResult: "Manual Import: cannot import to an archived project"
      );
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_DeviceNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty,  91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: (AssetDeviceIds) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_ManualDeviceType()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 0, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: (AssetDeviceIds) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    #endregion StandardProjects

    #region CivilProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_CvPrj_NotSupported()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3044,
        expectedMessageResult: "Manual Import: cannot import to a Civil type project"
      );
    }

    #endregion CivilProjects

    #region LandfillProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_TimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LFPrj_PrjMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        SubscriptionEndDate = null
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3045,
        expectedMessageResult: "Manual Import: landfill project does not have a valid subscription at that time"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LFPrj_PrjMan3d_NoRadioSerial()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        SubscriptionEndDate = null
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>()
        {
          new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}
        },
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3045,
        expectedMessageResult: "Manual Import: landfill project does not have a valid subscription at that time"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_NoRadioSerial()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = new DateTime(9999, 12, 31).Date
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: (AssetDeviceIds) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_DeviceNotFound()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = new DateTime(9999, 12, 31).Date
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer 450", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: (AssetDeviceIds) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_ManualDeviceType()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = new DateTime(9999, 12, 31).Date
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 0, "radSer 450", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: (AssetDeviceIds) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    #endregion LandfillProjects


    [TestMethod]
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() =>
        executor.ProcessAsync((GetProjectAndAssetUidsRequest) null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    private async Task Execute(GetProjectAndAssetUidsRequest request,
      string projectUid, string projectCustomerUid, List<Subscription> projectCustomerSubs,
      string assetUid, List<Subscription> assetSubs, string assetCustomerUid, List<Subscription> assetCustomerSubs,
      Project projectOfInterest, List<Project> intersectingProjects,
      AssetDeviceIds assetDevice,
      string expectedProjectUidResult, string expectedAssetUidResult, int expectedCodeResult,
      string expectedMessageResult
    )
    {
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetProjectBasedSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetProjectBasedSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(request) as GetProjectAndAssetUidsResult;

      ValidateResult(result, expectedProjectUidResult, expectedAssetUidResult, expectedCodeResult,
        expectedMessageResult);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult result, string expectedProjectUid, string expectedAssetUid,
      int resultCode, string resultMessage)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect AssetUid");
      Assert.AreEqual(resultCode, result.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, result.Message, "executor returned incorrect result message");
    }
  }

}