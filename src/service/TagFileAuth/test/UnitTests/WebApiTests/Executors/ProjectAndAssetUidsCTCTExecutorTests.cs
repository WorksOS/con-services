using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsCTCTExecutorTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;
    private string _projectUidToBeDiscovered;
    private string _assetUid;
    private DateTime _timeOfLocation;
    private string _assetOwningCustomerUid;
    private string _tccOwningCustomerUid;

    private GetProjectAndAssetUidsRequest _projectAndAssetUidsRequest;
    private string radioSerial = "radSer45";
    private string ec520Serial = "ecSer";
    private string tccOrgUid = "tccOrgUid_guid";

    private AssetDeviceIds _assetDeviceIds;
    private Mock<IDeviceRepository> _deviceRepo;

    private List<Subscription> _subscriptions;
    private Mock<ISubscriptionRepository> _subscriptionRepo;

    private List<Project> _projects;
    private Mock<IProjectRepository> _projectRepo;

    private Mock<ICustomerRepository> _customerRepo;


    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      _projectUidToBeDiscovered = Guid.NewGuid().ToString();
      _assetUid = Guid.NewGuid().ToString();
      _timeOfLocation = DateTime.UtcNow;
      _assetOwningCustomerUid = Guid.NewGuid().ToString();
      _tccOwningCustomerUid = Guid.NewGuid().ToString();
      _projectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, 6, radioSerial, string.Empty, string.Empty, 80, 160, _timeOfLocation);
      _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      _assetDeviceIds = new AssetDeviceIds { OwningCustomerUID = _assetOwningCustomerUid, AssetUID = _assetUid };
      _deviceRepo = new Mock<IDeviceRepository>();
     
      _subscriptions = new List<Subscription>
                       {
        new Subscription
        {
          CustomerUID = _assetOwningCustomerUid,
          ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring,
          StartDate = _timeOfLocation.AddDays(-10),
          EndDate = _timeOfLocation.AddDays(10),
          SubscriptionUID = Guid.NewGuid().ToString(),
          LastActionedUTC = DateTime.UtcNow
        }
      };
     
      _projects = new List<Project>
                  {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _assetOwningCustomerUid,
          ProjectType = ProjectType.ProjectMonitoring
        }
      };
      _projectRepo = new Mock<IProjectRepository>();
      _subscriptionRepo = new Mock<ISubscriptionRepository>();
      _customerRepo = new Mock<ICustomerRepository>();
    }

    [TestMethod]
    public async Task ProjectUidExecutor_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoAssetDeviceAssociation()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, false, 3033);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAnd3dPmSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // asset, assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      IEnumerable < Subscription > eSubs = _subscriptions.ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      _projects[0].ProjectType = ProjectType.Standard;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);
      
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, _assetUid, true, 0);
    }


    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAndNo3dPmSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // asset, NO assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      _projects[0].ProjectType = ProjectType.Standard;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(),It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, _assetUid, false, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndPMSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsRequest.TccOrgUid = tccOrgUid;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // NO asset, No assetSub
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() {CustomerUID = _tccOwningCustomerUid};
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      _projects[0].ProjectType = ProjectType.ProjectMonitoring;
      _projects[0].SubscriptionUID = Guid.NewGuid().ToString();
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, true, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndNoPMSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsRequest.TccOrgUid = tccOrgUid;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // NO asset, No assetSub
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, NO PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      _projects[0].ProjectType = ProjectType.ProjectMonitoring;
      _projects[0].SubscriptionUID = string.Empty;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, false, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_LandfillProjectAndSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsRequest.TccOrgUid = tccOrgUid;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // NO asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, LFSub and LF project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      _projects[0].ProjectType = ProjectType.LandFill;
      _projects[0].SubscriptionUID = Guid.NewGuid().ToString();
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, true, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_LandfillProjectAndNoSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsRequest.TccOrgUid = tccOrgUid;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // NO asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, NO LFSub and LF project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      _projects[0].ProjectType = ProjectType.LandFill;
      _projects[0].SubscriptionUID = string.Empty;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, false, 0);
    }


    [TestMethod]
    public async Task ProjectUidExecutor_MultiProjectAndSubscription()
    {
      _projectAndAssetUidsRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsRequest.TccOrgUid = tccOrgUid;
      var errorCodeResult = _projectAndAssetUidsRequest.Validate(true);
      Assert.AreEqual(0, errorCodeResult);

      // asset, assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      IEnumerable<Subscription> eSubs = _subscriptions.ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);
      
      _projects[0].ProjectType = ProjectType.Standard;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.Standard }, _timeOfLocation)).ReturnsAsync(_projects);

      // tccOrg, PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      
      _projects[0].ProjectType = ProjectType.ProjectMonitoring;
      _projects[0].ProjectUID = Guid.NewGuid().ToString();
      _projects[0].SubscriptionUID = Guid.NewGuid().ToString();
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(_projects);


      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsCTCTExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsCTCTExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, _assetUid, true, 3049);
    }

    private void ValidateResult(GetProjectAndAssetUidsResult result, string expectedProjectUid, string expectedAssetUid, bool expectedHasValidSubscription, int expectedCode)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect AssetUid");
      Assert.AreEqual(expectedHasValidSubscription, result.HasValidSub, "executor returned incorrect HasValidSub");
      Assert.AreEqual(expectedCode, result.Code, "executor returned incorrect result code");
    }
  }
}
