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
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace WebApiTests.Executors
{
  [TestClass]
  public class ProjectAndAssetUidsEarthWorksExecutorTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;
    private string _projectUidToBeDiscovered;
    private string _assetUid;
    private DateTime _timeOfLocation;
    private string _assetOwningCustomerUid;
    private string _tccOwningCustomerUid;

    private GetProjectAndAssetUidsEarthWorksRequest _projectAndAssetUidsEarthWorksRequest;
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
      _projectAndAssetUidsEarthWorksRequest = new GetProjectAndAssetUidsEarthWorksRequest(string.Empty, radioSerial, string.Empty, 80, 160, _timeOfLocation);
      _loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();

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
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, DeviceRepository, CustomerRepository, ProjectRepository, SubscriptionRepository);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsEarthWorksRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoAssetDeviceAssociation()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, CustomerRepository, ProjectRepository, SubscriptionRepository);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, string.Empty, string.Empty, false, 3033);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAnd3dPmSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();
      
      // asset, assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      IEnumerable < Subscription > eSubs = _subscriptions.ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _assetOwningCustomerUid,
          ProjectType = ProjectType.Standard
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.Standard }, _timeOfLocation)).ReturnsAsync(projects);
      
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, CustomerRepository, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, _assetUid, _assetOwningCustomerUid, true, 0);
    }


    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAndNo3dPmSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // asset, NO assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      _projects[0].ProjectType = ProjectType.Standard;
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.Standard }, _timeOfLocation)).ReturnsAsync(_projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, CustomerRepository, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, _assetUid, _assetOwningCustomerUid, false, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndPMSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.TccOrgUid = tccOrgUid;
      _projectAndAssetUidsEarthWorksRequest.Validate();
      
      // NO asset, No assetSub
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() {CustomerUID = _tccOwningCustomerUid};
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _tccOwningCustomerUid,
          ProjectType = ProjectType.ProjectMonitoring,
          SubscriptionUID = Guid.NewGuid().ToString()
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, _tccOwningCustomerUid, true, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndNoPMSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.TccOrgUid = tccOrgUid;
      _projectAndAssetUidsEarthWorksRequest.Validate();

      // NO asset, No assetSub
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, NO PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _tccOwningCustomerUid,
          ProjectType = ProjectType.ProjectMonitoring,
          SubscriptionUID = string.Empty
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, _tccOwningCustomerUid, false, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_LandfillProjectAndSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.TccOrgUid = tccOrgUid;
      _projectAndAssetUidsEarthWorksRequest.Validate();
     
      // NO asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, LFSub and LF project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);
      var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _tccOwningCustomerUid,
          ProjectType = ProjectType.LandFill,
          SubscriptionUID = Guid.NewGuid().ToString()
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, _tccOwningCustomerUid, true, 0);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_LandfillProjectAndNoSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.TccOrgUid = tccOrgUid;
      _projectAndAssetUidsEarthWorksRequest.Validate();
     
      // NO asset
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      // tccOrg, NO LFSub and LF project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);  
      var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = _projectUidToBeDiscovered,
          CustomerUID = _tccOwningCustomerUid,
          ProjectType = ProjectType.LandFill,
          SubscriptionUID = string.Empty
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, _projectUidToBeDiscovered, string.Empty, _tccOwningCustomerUid, false, 0);
    }


    [TestMethod]
    public async Task ProjectUidExecutor_MultiProjectAndSubscription()
    {
      _projectAndAssetUidsEarthWorksRequest.Ec520Serial = ec520Serial;
      _projectAndAssetUidsEarthWorksRequest.TccOrgUid = tccOrgUid;
      _projectAndAssetUidsEarthWorksRequest.Validate();
      
      // asset, assetSub and standard project
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

      _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      IEnumerable<Subscription> eSubs = _subscriptions.ToList();
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

     var projects = new List<Project>
      {
        new Project
        {
          ProjectUID = Guid.NewGuid().ToString(),
          CustomerUID = _assetOwningCustomerUid,
          ProjectType = ProjectType.Standard
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.Standard }, _timeOfLocation)).ReturnsAsync(projects);

      // tccOrg, PMSub and PM project
      var customerTccOrg = new CustomerTccOrg() { CustomerUID = _tccOwningCustomerUid };
      _customerRepo.Setup(c => c.GetCustomerWithTccOrg(tccOrgUid)).ReturnsAsync(customerTccOrg);

      var projectsPM = new List<Project>
      {
        new Project
        {
          ProjectUID = Guid.NewGuid().ToString(),
          CustomerUID = _tccOwningCustomerUid,
          ProjectType = ProjectType.ProjectMonitoring,
          SubscriptionUID = Guid.NewGuid().ToString()
        }
      };
      _projectRepo.Setup(d => d.GetIntersectingProjects(_tccOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), new int[] { (int)ProjectType.ProjectMonitoring, (int)ProjectType.LandFill }, _timeOfLocation)).ReturnsAsync(projectsPM);


      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsEarthWorksExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsEarthWorksExecutorTests>(), ConfigStore,
        AssetRepository, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);
      var result = await executor.ProcessAsync(_projectAndAssetUidsEarthWorksRequest) as GetProjectAndAssetUidsEarthWorksResult;

      ValidateResult(result, string.Empty, _assetUid, _assetOwningCustomerUid, true, 3049);
    }

    private void ValidateResult(GetProjectAndAssetUidsEarthWorksResult result, string expectedProjectUid, string expectedAssetUid, string expectedCustomerUid, bool expectedHasValidSubscription, int expectedCode)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectTrn");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect AssetUid");
      Assert.AreEqual(expectedCustomerUid, result.CustomerUid, "executor returned incorrect CustomerUid");
      Assert.AreEqual(expectedHasValidSubscription, result.HasValidSub, "executor returned incorrect HasValidSub");
      Assert.AreEqual(expectedCode, result.Code, "executor returned incorrect result code");
    }
  }
}
