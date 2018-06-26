using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
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
  public class ProjectAndAssetUidsExecutorTests : ExecutorBaseTests
  {
    private ILoggerFactory _loggerFactory;
    //private string _projectUidToBeDiscovered;
    //private string _assetUid;
    //private DateTime _timeOfLocation;
    //private string _assetOwningCustomerUid;

    //private GetProjectUidRequest _projectUidRequest;

    private Mock<ICustomerRepository> _customerRepo;

    private Mock<IAssetRepository> _assetRepo;

    //private AssetDeviceIds _assetDeviceIds;
    private Mock<IDeviceRepository> _deviceRepo;

    //private List<Subscription> _subscriptions;
    private Mock<ISubscriptionRepository> _subscriptionRepo;

    //private List<Project> _projects;
    private Mock<IProjectRepository> _projectRepo;



    [TestInitialize]
    public override void InitTest()
    {
      base.InitTest();

      //_projectUidToBeDiscovered = Guid.NewGuid().ToString();
      //_assetUid = Guid.NewGuid().ToString();
      //_timeOfLocation = DateTime.UtcNow;
      //_assetOwningCustomerUid = Guid.NewGuid().ToString();
      //_projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(6, "radSer45", 91, 181, _timeOfLocation);
      _loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      _customerRepo = new Mock<ICustomerRepository>();

      _assetRepo = new Mock<IAssetRepository>();

      //_assetDeviceIds = new AssetDeviceIds() { OwningCustomerUID = _assetOwningCustomerUid, AssetUID = _assetUid };
      _deviceRepo = new Mock<IDeviceRepository>();

      //_subscriptions = new List<Subscription>()
      //{
      //  new Subscription()
      //  {
      //    CustomerUID = _assetOwningCustomerUid,
      //    ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring,
      //    StartDate = _timeOfLocation.AddDays(-10),
      //    EndDate = _timeOfLocation.AddDays(10),
      //    SubscriptionUID = Guid.NewGuid().ToString(),
      //    LastActionedUTC = DateTime.UtcNow
      //  }
      //};

      //_projects = new List<Project>()
      //{
      //  new Project()
      //  {
      //    ProjectUID = _projectUidToBeDiscovered,
      //    CustomerUID = _assetOwningCustomerUid,
      //    ProjectType = ProjectType.ProjectMonitoring
      //  }
      //};
      _projectRepo = new Mock<IProjectRepository>();
      _subscriptionRepo = new Mock<ISubscriptionRepository>();
    }

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoAssetDeviceAssociation()
    {
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorTests>(), configStore,
        assetRepository, _deviceRepo.Object, customerRepository, projectRepository, subscriptionRepository);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest("", 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3033);
    }

    //[TestMethod]
    //public async Task ProjectUidExecutor_StandardProjectAnd3dPmSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
    //  IEnumerable < Subscription > eSubs = _subscriptions.ToList();
    //  _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

    //  _projects[0].ProjectType = ProjectType.Standard;
    //  _projectRepo.Setup(d => d.GetStandardProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation)).ReturnsAsync(_projects);

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, _projectUidToBeDiscovered, 0);
    //}


    //[TestMethod]
    //public async Task ProjectUidExecutor_StandardProjectAndNo3dPmSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
    //  _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

    //  _projects[0].ProjectType = ProjectType.Standard;
    //  _projectRepo.Setup(d => d.GetStandardProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation)).ReturnsAsync(_projects);

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, string.Empty, 3029);
    //}

    //[TestMethod]
    //public async Task ProjectUidExecutor_PMProjectAndPMSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _projects[0].ProjectType = ProjectType.ProjectMonitoring;
    //  _projectRepo.Setup(d => d.GetProjectMonitoringProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation, (int)ProjectType.ProjectMonitoring, (int)ServiceTypeEnum.ProjectMonitoring)).ReturnsAsync(_projects);

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, _projectUidToBeDiscovered, 0);
    //}

    //[TestMethod]
    //public async Task ProjectUidExecutor_PMProjectAndNoPMSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _projects[0].ProjectType = ProjectType.ProjectMonitoring;
    //  _projectRepo.Setup(d => d.GetProjectMonitoringProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation, (int)ProjectType.ProjectMonitoring, (int)ServiceTypeEnum.ProjectMonitoring)).ReturnsAsync(new List<Project>());

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, string.Empty, 3029);
    //}

    //[TestMethod]
    //public async Task ProjectUidExecutor_LandfillProjectAndSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _projects[0].ProjectType = ProjectType.LandFill;
    //  _projectRepo.Setup(d => d.GetProjectMonitoringProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation, (int)ProjectType.LandFill, (int)ServiceTypeEnum.Landfill)).ReturnsAsync(_projects);

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, _projectUidToBeDiscovered, 0);
    //}

    //[TestMethod]
    //public async Task ProjectUidExecutor_LandfillProjectAndNoSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _projects[0].ProjectType = ProjectType.LandFill;
    //  _projectRepo.Setup(d => d.GetProjectMonitoringProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation, (int)ProjectType.LandFill, (int)ServiceTypeEnum.Landfill)).ReturnsAsync(new List<Project>());

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(_loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, string.Empty, 3029);
    //}


    //[TestMethod]
    //public async Task ProjectUidExecutor_MultiProjectAndSubscription()
    //{
    //  _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_assetDeviceIds);

    //  _subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
    //  IEnumerable<Subscription> eSubs = _subscriptions.ToList();
    //  _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(_assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

    //  _projects[0].ProjectType = ProjectType.Standard;
    //  _projectRepo.Setup(d => d.GetStandardProject(_assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), _timeOfLocation)).ReturnsAsync(_projects);

    //  _projects[0].ProjectType = ProjectType.ProjectMonitoring;
    //  _projects[0].ProjectUID = Guid.NewGuid().ToString();
    //  _projectRepo.Setup(d => d.GetProjectMonitoringProject(_assetOwningCustomerUid, It.IsAny<double>(),
    //    It.IsAny<double>(), _timeOfLocation, (int) ProjectType.ProjectMonitoring,
    //    (int) ServiceTypeEnum.ProjectMonitoring)).ReturnsAsync(_projects);

    //  var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(
    //    _loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
    //    assetRepository, _deviceRepo.Object, customerRepository, _projectRepo.Object, _subscriptionRepo.Object);
    //  var result = await executor.ProcessAsync(_projectUidRequest) as GetProjectUidResult;

    //  ValidateResult(result, string.Empty, 3032);
    //}

    private void ValidateResult(GetProjectAndAssetUidsResult result, string expectedProjectUid, string expectedAssetUid, int expectedCode)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect AssetUid");
      Assert.AreEqual(expectedCode, result.Code, "executor returned incorrect result code");
    }
  }
}