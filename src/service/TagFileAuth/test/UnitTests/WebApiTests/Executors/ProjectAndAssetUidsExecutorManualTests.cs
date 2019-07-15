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
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.TRex.Gateway.Common.Abstractions;

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
    private Mock<ITRexCompactionDataProxy> _tRexCompactionDataProxy;

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
      _tRexCompactionDataProxy = new Mock<ITRexCompactionDataProxy>();
    }

    #region StandardProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
         new List<Subscription>
          { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}},
        assetUid,
        new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds() { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_940_And_Ec520()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}},
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> { projectOfInterest },
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_Ec520()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.EC520, string.Empty, "ec520Serial", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}},
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> { projectOfInterest },
        projectUid,
        ec520Uid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj__ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, DateTime.UtcNow),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        null,
        new List<Project>(),
        string.Empty,
        string.Empty,
        3038,
        "Unable to find the Project requested"
      );

    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_AssetCustMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>
        { new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3039,
        "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_Asset3dSub_MatchesProjectCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = projectCustomerUid },
        new List<Subscription>
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            CustomerUID = projectCustomerUid
          }
        },
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_Asset3dSub_NoMatchForProjectCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>
        {
          new Subscription
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            CustomerUID = assetCustomerUid
          }
        },
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3039,
        "Manual Import: got asset. Unable to locate any valid project, or asset subscriptions"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_PrjMan3d_ProjectDoesntIntersectSpatially()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project>{},
        string.Empty,
        string.Empty,
        3041,
        "Manual Import: no intersecting projects found"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_TimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_ProjectIsDeleted()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3),
        IsDeleted = true
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3043,
        "Manual Import: cannot import to an archived project"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StdPrj_NoSub_NoRadioSerial()
    {
      // standard Project requires a known asset, if project has no Man3d

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3),
        IsDeleted = true
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3043,
        "Manual Import: cannot import to an archived project"
      );
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_DeviceNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty,  91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        null,
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        string.Empty,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_ManualDeviceType()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.MANUALDEVICE, String.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        null,
        new List<Subscription>(),
        ec520Uid,
        null,
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        string.Empty,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StdPrj_PrjMan3d_NEEResolved()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 0, 0, projectOfInterest.StartDate.AddDays(1), 300345.670, 600000.64),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> { projectOfInterest },
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    #endregion StandardProjects

    #region CivilProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_CvPrj_NotSupported()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(-3)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3044,
        "Manual Import: cannot import to a Civil type project"
      );
    }

    #endregion CivilProjects


    #region LandfillProjects

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_TimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int) ServiceTypeEnum.Landfill,
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        assetUid,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LFPrj_PrjMan3d()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        SubscriptionEndDate = null
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3045,
        "Manual Import: landfill project does not have a valid subscription at that time"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LFPrj_PrjMan3d_NoRadioSerial()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        SubscriptionEndDate = null
      };

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>
        { new Subscription {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring} },
        assetUid,
        new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        ec520Uid,
        new AssetDeviceIds { AssetUID = ec520Uid, OwningCustomerUID = assetCustomerUid },
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        string.Empty,
        string.Empty,
        3045,
        "Manual Import: landfill project does not have a valid subscription at that time"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_NoRadioSerial()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
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

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        null,
        new List<Subscription>(),
        ec520Uid,
        null,
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        string.Empty,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_DeviceNotFound()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
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

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.SNM940, "snm940Serial", string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        null,
        new List<Subscription>(),
        ec520Uid,
        null,
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        string.Empty,
        0,
        "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LFPrj_LFSub_ManualDeviceType()
    {
      // landfill Project requires a landfill sub - doesn't require asset

      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project
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

      var assetUid = Guid.NewGuid().ToString();
      var ec520Uid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (new GetProjectAndAssetUidsRequest(projectUid, (int)DeviceTypeEnum.MANUALDEVICE, string.Empty, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-10)),
        projectUid,
        projectCustomerUid,
        new List<Subscription>(),
        assetUid,
        null,
        new List<Subscription>(),
        ec520Uid,
        null,
        new List<Subscription>(),
        assetCustomerUid,
        new List<Subscription>(),
        projectOfInterest,
        new List<Project> {projectOfInterest},
        projectUid,
        string.Empty,
        0,
        "success"
      );
    }

    #endregion LandfillProjects

    #region dataRepo
    [TestMethod]
    [DataRow("77e6bd66-54d8-4651-8907-88b15d81b2d7", 0, 0, 50.0, 50.0)] 
    public async Task TRexExecutor_GetCSIBForProject_HappyPath(
      string projectUid, double northing, double easting,
      double expectedLat, double expectedLong)
    {
      var csibResult = new CSIBResult("blah blah");
      _tRexCompactionDataProxy.Setup(d => d.SendDataGetRequest<CSIBResult>(It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(csibResult);

      var dataRepository = new DataRepository(null, null,
        null, null, null, _tRexCompactionDataProxy.Object, new Dictionary<string, string>());

      var latLongDegrees = await dataRepository.GenerateLatLong(projectUid, northing, easting);
      Assert.AreEqual(latLongDegrees.Lat, expectedLat);
      Assert.AreEqual(latLongDegrees.Lat, expectedLong);
    }

    [TestMethod]
    [DataRow("77e6bd66-54d8-4651-8907-88b15d81b2d7", 0, 0, 0.0, 0.0)]
    public async Task TRexExecutor_GetCSIBForProject_UnHappyPath(
      string projectUid, double northing, double easting,
      double expectedLat, double expectedLong)
    {
      var csibResult = new CSIBResult(String.Empty);
      _tRexCompactionDataProxy.Setup(d => d.SendDataGetRequest<CSIBResult>(It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(csibResult);

      var dataRepository = new DataRepository(null, null,
        null, null, null, _tRexCompactionDataProxy.Object, new Dictionary<string, string>());

      var latLongDegrees = await dataRepository.GenerateLatLong(projectUid, northing, easting);
      Assert.AreEqual(latLongDegrees.Lat, expectedLat);
      Assert.AreEqual(latLongDegrees.Lat, expectedLong);
    }

    #endregion dateRepo


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
      string assetUid, AssetDeviceIds assetDevice, List<Subscription> assetSubs,
      string ec520Uid, AssetDeviceIds ec520Device, List<Subscription> ec520Subs,
      string assetCustomerUid, List<Subscription> assetCustomerSubs,
      Project projectOfInterest, List<Project> intersectingProjects,
      string expectedProjectUidResult, string expectedAssetUidResult, int expectedCodeResult,
      string expectedMessageResult
    )
    {
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>(), It.IsAny<string>())).ReturnsAsync(intersectingProjects);

      _deviceRepo.Setup(d => d.GetAssociatedAsset(request.RadioSerial, It.IsAny<string>())).ReturnsAsync(assetDevice);
      _deviceRepo.Setup(d => d.GetAssociatedAsset(request.Ec520Serial, It.IsAny<string>())).ReturnsAsync(ec520Device);

      _subscriptionRepo.Setup(d => d.GetProjectBasedSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetProjectBasedSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(ec520Uid, It.IsAny<DateTime>())).ReturnsAsync(ec520Subs);

      _tRexCompactionDataProxy.Setup(d => d.SendDataGetRequest<CSIBResult>(It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<IDictionary<string, string>>(), It.IsAny<IDictionary<string, string>>()))
        .ReturnsAsync(new CSIBResult("blahblah"));

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object,
        null, null,
        _tRexCompactionDataProxy.Object, new Dictionary<string, string>());
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
