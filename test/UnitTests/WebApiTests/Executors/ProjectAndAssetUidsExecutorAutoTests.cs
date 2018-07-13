using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Moq;
using VSS.Common.Exceptions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsExecutorAutoTests : ExecutorBaseTests
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

    #region standardProjects

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_StdPrj()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, "radSer 450", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> {projectOfInterest},
        assetDevice: new AssetDeviceIds {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid},
        customerTccOrg: (CustomerTccOrg) null,
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: assetUid,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_StdPrj_NoneFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, "radSer 450", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        customerTccOrg: (CustomerTccOrg)null,
        expectedProjectUidResult: string.Empty, 
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_StdPrj_NoAssetFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, "radSer 450", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: (AssetDeviceIds) null,
        customerTccOrg: (CustomerTccOrg)null,
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3047,
        expectedMessageResult: "Auto Import: no asset or tccOrgId identified"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_StdPrj_NoRadioSerial()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, string.Empty, string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        customerTccOrg: (CustomerTccOrg)null, 
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3047,
        expectedMessageResult: "Auto Import: no asset or tccOrgId identified"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_StdPrj_TimeOutsideProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, "radSer 450", string.Empty, 91, 181, projectOfInterest.StartDate.AddDays(-99)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        customerTccOrg: (CustomerTccOrg)null, 
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }

    #endregion standardProjects

    #region landfillProjects

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_LFPrj()
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
        ServiceTypeID = (int)ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };
  
      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty, 
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: (AssetDeviceIds) null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID},
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_LFPrj_NoneFound()
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
        ServiceTypeID = (int)ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>( ),
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID },
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_LFPrj_NoTCCOrgFound()
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
        ServiceTypeID = (int)ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: (CustomerTccOrg) null,
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3047,
        expectedMessageResult: "Auto Import: no asset or tccOrgId identified"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_LFPrj_TimeOutsideProject()
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
        ServiceTypeID = (int)ServiceTypeEnum.Landfill,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID },
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }

    #endregion landfillProjects

    #region PMProjects

    [TestMethod]
    public async Task TRexExecutor_Auto_Happy_PMPrj()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID },
        expectedProjectUidResult: projectUid,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 0,
        expectedMessageResult: "success"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_PMPrj_NoneFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID },
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_PMPrj_NoTCCOrgFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project> { projectOfInterest },
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: (CustomerTccOrg)null,
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3047,
        expectedMessageResult: "Auto Import: no asset or tccOrgId identified"
      );
    }

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_PMPrj_TimeOutsideProject()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterest = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddDays(6)
      };

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 0, string.Empty, "tccOrgId", 91, 181, projectOfInterest.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: string.Empty,
        assetSubs: new List<Subscription>(),
        assetCustomerUid: string.Empty,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterest,
        intersectingProjects: new List<Project>(),
        assetDevice: (AssetDeviceIds)null,
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterest.CustomerUID },
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3048,
        expectedMessageResult: "Auto Import: no project meets the time/location/subscription requirements"
      );
    }
    #endregion PMProjects

    [TestMethod]
    public async Task TRexExecutor_Auto_Sad_TooManyProjects()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectOfInterestStd = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3)
      };
      var projectOfInterestPM = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.ProjectMonitoring,
        CustomerUID = projectCustomerUid,
        StartDate = DateTime.UtcNow.AddDays(-4),
        EndDate = DateTime.UtcNow.AddDays(3),
        ServiceTypeID = (int)ServiceTypeEnum.ProjectMonitoring,
        SubscriptionStartDate = DateTime.UtcNow.AddYears(-1),
        SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
      };

      string assetUid = Guid.NewGuid().ToString();
      string assetCustomerUid = Guid.NewGuid().ToString();

      await Execute
      (request: GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(string.Empty, 6, "radSer 450", "tccOrgId", 91, 181, projectOfInterestStd.StartDate.AddDays(1)),
        projectUid: projectUid,
        projectCustomerUid: projectCustomerUid,
        projectCustomerSubs: new List<Subscription>(),
        assetUid: assetUid,
        assetSubs: new List<Subscription>()
        {
          new Subscription()
          {
            ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring,
            StartDate = DateTime.UtcNow.AddYears(-1).Date,
            EndDate = new DateTime(9999, 12, 31).Date
          }
        },
        assetCustomerUid: assetCustomerUid,
        assetCustomerSubs: new List<Subscription>(),
        projectOfInterest: projectOfInterestStd,
        intersectingProjects: new List<Project> { projectOfInterestStd, projectOfInterestPM, },
        assetDevice: new AssetDeviceIds { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid },
        customerTccOrg: new CustomerTccOrg { CustomerUID = projectOfInterestPM.CustomerUID },
        expectedProjectUidResult: string.Empty,
        expectedAssetUidResult: string.Empty,
        expectedCodeResult: 3049,
        expectedMessageResult: "Auto Import: more than 1 project meets the time/location/subscription requirements"
      );
    }


    private async Task Execute(GetProjectAndAssetUidsRequest request,
      string projectUid, string projectCustomerUid, List<Subscription> projectCustomerSubs,
      string assetUid, List<Subscription> assetSubs, string assetCustomerUid, List<Subscription> assetCustomerSubs,
      Project projectOfInterest, List<Project> intersectingProjects,
      AssetDeviceIds assetDevice, CustomerTccOrg customerTccOrg,
      string expectedProjectUidResult, string expectedAssetUidResult, int expectedCodeResult,
      string expectedMessageResult
    )
    {
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      IEnumerable<Project> enumIntersectingProjects = intersectingProjects.AsEnumerable();
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>()))
        .ReturnsAsync(enumIntersectingProjects);

      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(It.IsAny<string>())).ReturnsAsync(customerTccOrg);

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