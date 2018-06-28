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
    public async Task ProjectAndAssetUidsExecutor_ManualImport_HappyPath_ProjectHasManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var assetUid = Guid.NewGuid().ToString();
      var latitude = 89.777;
      var longitude = 34.555;
      var timeOfLocationUtc = DateTime.UtcNow;
      
      //"Manual 3D Project Monitoring"
      var projectCustomerSubs = new List<Subscription>()
        { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }};

      var projectCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();

      //"Manual 3D Project Monitoring"
      var assetCustomerSubs = new List<Subscription>(){};
      // var projectTypes = new int[]{(int)ProjectType.LandFill, (int)ProjectType.Standard, (int)ProjectType.ProjectMonitoring }; // projectTypes. += ProjectType.LandFill;
      var projects = new List<Project>() { }; projects.Add(new Project(){ProjectUID = projectUid, CustomerUID = projectCustomerUid, ProjectType = ProjectType.Standard});
      IEnumerable<Project> pp = projects.AsEnumerable();

      //"3D Project Monitoring"
      var assetSubs = new List<Subscription>();
      // HttpStatusCode httpStatusCode = HttpStatusCode.OK;
      var resultCode = 0;
      var resultMessage = string.Empty;

      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid});
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>())).ReturnsAsync(pp);

      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync( new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid});

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);


      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, resultCode, resultMessage);
    }

    ///// <summary>
    /////     Gets any project of the specified type/s (or all),
    /////     which the lat/long is within
    ///// </summary>
    ///// <param name="customerUid"></param>
    ///// <param name="projectTypes"></param>
    ///// <param name="latitude"></param>
    ///// <param name="longitude"></param>
    ///// <param name="timeOfPosition"></param>
    ///// <returns>The project</returns>
    //public async Task<IEnumerable<Project>> GetIntersectingProjects(string customerUid, int[] projectTypes,
    //  double latitude, double longitude, DateTime timeOfPosition)
    //{
    //  var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";
    //  var projectTypesString = string.Empty;
    //  if (projectTypes.Any())
    //  {
    //    projectTypesString += " p.fk_ProjectTypeID IN ( ";
    //    for (int i = 0; i < projectTypes.Length; i++)
    //    {
    //      projectTypesString += projectTypes[i] + ((i < projectTypes.Length - 1) ? "," : "");
    //    }

    //    projectTypesString += " ) AND ";
    //  }

    //  var select = "SELECT DISTINCT " +
    //               "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
    //               "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT, " +
    //               "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
    //               "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID " +
    //               "      FROM Project p " +
    //               "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
    //               $"      WHERE {projectTypesString} " +
    //               "            p.IsDeleted = 0 " +
    //               "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
    //               "        AND cp.fk_CustomerUID = @customerUID " +
    //               $"        AND st_Intersects({point}, PolygonST) = 1";

    //  var projects =
    //    await QueryWithAsyncPolicy<Project>(select, new { customerUID = customerUid, timeOfPosition = timeOfPosition.Date });

    //  return projects;
    //}

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

    private void ValidateResult(GetProjectAndAssetUidsResult result, string expectedProjectUid, string expectedAssetUid, int resultCode, string resultMessage)
    {
      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(expectedProjectUid, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(expectedAssetUid, result.AssetUid, "executor returned incorrect AssetUid");
      Assert.AreEqual(resultCode, result.Code, "executor returned incorrect result code");
      Assert.AreEqual(resultMessage, result.Message, "executor returned incorrect result message");
    }
  }
}