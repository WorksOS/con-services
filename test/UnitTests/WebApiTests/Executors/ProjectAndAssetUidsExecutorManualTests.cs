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

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_ManualImport_ProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); // Man3d


      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); // 3dpm
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; // Man3d

      Project projectOfInterest = null;
      var projects = new List<Project> {};
      IEnumerable<Project> intersectingProjects = projects.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3038, "Unable to find the Project requested");
    }

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_ManualImport_HappyPath_ProjectCustomerHasManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }}; // Man3d

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); // 3dpm
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>(){}; // Man3d

      var projectOfInterest = new Project(){ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var projects = new List<Project> {projectOfInterest};
      IEnumerable<Project> intersectingProjects = projects.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid};
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync( assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 0, "success");
    }

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_ManualImport_HappyPath_AssetCustomerHasManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); // Man3d
       

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); // 3dpm
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() 
         { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }}; // Man3d

      var projectOfInterest = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var projects = new List<Project> { projectOfInterest };
      IEnumerable<Project> intersectingProjects = projects.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 0, "success");
    }

    [TestMethod]
    public async Task ProjectAndAssetUidsExecutor_ManualImport_HappyPath_NoManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); // Man3d


      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); // 3dpm
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; // Man3d

      var projectOfInterest = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var projects = new List<Project> { projectOfInterest };
      IEnumerable<Project> intersectingProjects = projects.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(projectOfInterest);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<int[]>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<DateTime>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 3039, "Unable to locate any valid subscriptions for Manual Import");
    }

    //var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync(projectAndAssetUidsRequest));

    //Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
    //Assert.AreEqual(3039, ex.GetResult.Code);
    //Assert.AreEqual("Unable to locate any valid subscriptions for Manual Import", ex.GetResult.Message);


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