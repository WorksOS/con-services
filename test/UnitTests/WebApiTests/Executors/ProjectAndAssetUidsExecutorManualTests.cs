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
    public async Task TRexExecutor_Sad_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectAndAssetUidsRequest)null));

      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StandardProjectProjectNotFound()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 


      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      Project manualImportProject = null;
      var intersectingProjectsList = new List<Project> {};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

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
    public async Task TRexExecutor_Manual_Happy_StandardProjectProjectCustomerHasManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }};

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>(){}; 

      var manualImportProject = new Project(){ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

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
    public async Task TRexExecutor_Manual_Sad_StandardProjectAssetCustomerHasManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 
       

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() 
         { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }};

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

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
    public async Task TRexExecutor_Manual_Sad_StandardProjectNoSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 


      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, assetUid, 3039, "Manual Import: unable to locate any valid subscriptions");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StandardProjectNoManualSubBut3dSubForAssetCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 

      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring, CustomerUID = assetCustomerUid}}; 
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectionProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectionProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, assetUid, 3039, "Manual Import: unable to locate any valid subscriptions");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StandardProjectNoManualSubBut3dSubForProjectCustomer()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 

      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.ThreeDProjectMonitoring, CustomerUID = projectCustomerUid}}; 
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

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
    public async Task TRexExecutor_Manual_Sad_StandardProjectProjectDoesntIntersectSpatially()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}};

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.Standard,
        CustomerUID = projectCustomerUid
      };
      var intersectionProjectsList = new List<Project> {};
      IEnumerable<Project> intersectingProjects = intersectionProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid};
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, assetUid, 3041, "Manual Import: no intersecting projects found");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StandardProjectManualSubAndTimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }}; 

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var projectStartDate = DateTime.UtcNow.AddDays(-4);
      var projectEndDate = DateTime.UtcNow.AddDays(-3);
      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid, StartDate = projectStartDate, EndDate = projectEndDate};
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, projectStartDate.AddDays(-1));
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StandardProjectWhichIsDeleted()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>();

      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerSubs = new List<Subscription>() { };

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid, IsDeleted = true};
      var intersectionProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectionProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3043, "Manual Import: cannot import to an archived project");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LandfillProjectLandfillSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>();  

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var projectStartDate = DateTime.UtcNow.AddDays(-4);
      var projectEndDate = DateTime.UtcNow.AddDays(-3);
      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.LandFill, CustomerUID = projectCustomerUid, StartDate = projectStartDate, EndDate = projectEndDate, SubscriptionEndDate = projectEndDate.AddDays(1), ServiceTypeID = (int) ServiceTypeEnum.Landfill};
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, projectStartDate.AddDays(1));
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LandfillProjectLandfillSubAndTimeOutsideProjectDates()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>();  

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var projectStartDate = DateTime.UtcNow.AddDays(-4);
      var projectEndDate = DateTime.UtcNow.AddDays(-3);
      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.LandFill, CustomerUID = projectCustomerUid, StartDate = projectStartDate, EndDate = projectEndDate, SubscriptionEndDate = projectEndDate.AddDays(1), ServiceTypeID = (int)ServiceTypeEnum.Landfill };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, projectStartDate.AddDays(-1));
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, assetUid, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_CivilProjectNotSupported()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>(); 

      var assetUid = Guid.NewGuid().ToString();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.ProjectMonitoring, CustomerUID = projectCustomerUid };
      var intersectionProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectionProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3044, "Manual Import: cannot import to a Civil type project");
    }
    
    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LandfillProjectAndProjectManualSub()
    {
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        { new Subscription() { ServiceTypeID = (int)ServiceTypeEnum.Manual3DProjectMonitoring }}; 

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var projectStartDate = DateTime.UtcNow.AddDays(-4);
      var projectEndDate = DateTime.UtcNow.AddDays(-3);
      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.LandFill, CustomerUID = projectCustomerUid, StartDate = projectStartDate, EndDate = projectEndDate, SubscriptionEndDate = null};
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>())).ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(_loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest = GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181, projectStartDate.AddDays(1));
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3045, "Manual Import: landfill project does not have a valid subscription at that time");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_StandardProjectNoSubNoRadioSerial()
    {
      // standard Project requires a known asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>() { };

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>(); 
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { }; 

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      var assetDevice = new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid};
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDevice);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3040, "Manual Import: unable to locate any valid projectSubscriptions or locate asset");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StandardProjectManSubDeviceNotFound()
    {
      // standard Project requires a known asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}};

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      //var assetDevice = new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid};
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds) null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer45", "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, string.Empty, 0, "success");
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_StandardProjectMan3dSubManualDeviceType()
    {
      // standard Project requires a known asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}};

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.Standard, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      //var assetDevice = new AssetDeviceIds() {AssetUID = assetUid, OwningCustomerUID = assetCustomerUid};
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 0, "radSer45", "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, string.Empty, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Sad_LandfillProjectManualSubNoRadioSerial()
    {
      // landfill Project requires a landfill sub - doesn't require asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>()
        {new Subscription() {ServiceTypeID = (int) ServiceTypeEnum.Manual3DProjectMonitoring}};

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var manualImportProject = new Project() { ProjectUID = projectUid, ProjectType = ProjectType.LandFill, CustomerUID = projectCustomerUid };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      // var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((AssetDeviceIds) null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, string.Empty, string.Empty, 3045,
        "Manual Import: landfill project does not have a valid subscription at that time");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LandfillProjectLandfillNoRadioSerial()
    {
      // landfill Project requires a landfill sub - doesn't require asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>() { };

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var projectStartDate = DateTime.UtcNow.AddYears(-1);
      var projectEndDate = projectStartDate.AddYears(2);
      var projectSubType = (int) ServiceTypeEnum.Landfill;
      var projectSubStartDate = DateTime.UtcNow.AddYears(-1);
      var projectSubEndDate = new DateTime(9999, 12, 31).Date;
      ;
      var manualImportProject = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = projectStartDate,
        EndDate = projectEndDate,
        ServiceTypeID = projectSubType,
        SubscriptionStartDate = projectSubStartDate,
        SubscriptionEndDate = projectSubEndDate
      };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      // var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((AssetDeviceIds) null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, string.Empty, "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, string.Empty, 0, "success");
    }


    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LandfillProjectLandfillSubDeviceNotFound()
    {
      // landfill Project requires a landfill sub - doesn't require asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>() { };

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var projectStartDate = DateTime.UtcNow.AddYears(-1);
      var projectEndDate = projectStartDate.AddYears(2);
      var projectSubType = (int) ServiceTypeEnum.Landfill;
      var projectSubStartDate = DateTime.UtcNow.AddYears(-1);
      var projectSubEndDate = new DateTime(9999, 12, 31).Date;
      ;
      var manualImportProject = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = projectStartDate,
        EndDate = projectEndDate,
        ServiceTypeID = projectSubType,
        SubscriptionStartDate = projectSubStartDate,
        SubscriptionEndDate = projectSubEndDate
      };
      var intersectingProjectsList = new List<Project> {manualImportProject};
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      // var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((AssetDeviceIds) null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 6, "radSer 450", "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, string.Empty, 0, "success");
    }

    [TestMethod]
    public async Task TRexExecutor_Manual_Happy_LandfillProjectLandfillSubManualDeviceType()
    {
      // landfill Project requires a landfill sub - doesn't require asset
      var projectUid = Guid.NewGuid().ToString();
      var projectCustomerUid = Guid.NewGuid().ToString();
      var projectCustomerSubs = new List<Subscription>() { };

      var assetUid = Guid.NewGuid().ToString();
      var assetSubs = new List<Subscription>();
      var assetCustomerUid = Guid.NewGuid().ToString();
      var assetCustomerSubs = new List<Subscription>() { };

      var projectStartDate = DateTime.UtcNow.AddYears(-1);
      var projectEndDate = projectStartDate.AddYears(2);
      var projectSubType = (int)ServiceTypeEnum.Landfill;
      var projectSubStartDate = DateTime.UtcNow.AddYears(-1);
      var projectSubEndDate = new DateTime(9999, 12, 31).Date;
      ;
      var manualImportProject = new Project()
      {
        ProjectUID = projectUid,
        ProjectType = ProjectType.LandFill,
        CustomerUID = projectCustomerUid,
        StartDate = projectStartDate,
        EndDate = projectEndDate,
        ServiceTypeID = projectSubType,
        SubscriptionStartDate = projectSubStartDate,
        SubscriptionEndDate = projectSubEndDate
      };
      var intersectingProjectsList = new List<Project> { manualImportProject };
      IEnumerable<Project> intersectingProjects = intersectingProjectsList.AsEnumerable();
      _projectRepo.Setup(p => p.GetProject(It.IsAny<string>())).ReturnsAsync(manualImportProject);
      _projectRepo.Setup(p => p.GetIntersectingProjects(It.IsAny<string>(), It.IsAny<double>(), It.IsAny<double>(),
        It.IsAny<int[]>(), It.IsAny<DateTime?>())).ReturnsAsync(intersectingProjects);

      // var assetDevice = new AssetDeviceIds() { AssetUID = assetUid, OwningCustomerUID = assetCustomerUid };
      _deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((AssetDeviceIds)null);

      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(projectCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(projectCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByCustomer(assetCustomerUid, It.IsAny<DateTime>()))
        .ReturnsAsync(assetCustomerSubs);
      _subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(assetSubs);

      var executor = RequestExecutorContainer.Build<ProjectAndAssetUidsExecutor>(
        _loggerFactory.CreateLogger<ProjectAndAssetUidsExecutorManualTests>(), configStore,
        _assetRepo.Object, _deviceRepo.Object, _customerRepo.Object, _projectRepo.Object, _subscriptionRepo.Object);

      var projectAndAssetUidsRequest =
        GetProjectAndAssetUidsRequest.CreateGetProjectAndAssetUidsRequest(projectUid, 0, "radSer 450", "", 91, 181,
          DateTime.UtcNow);
      var result = await executor.ProcessAsync(projectAndAssetUidsRequest) as GetProjectAndAssetUidsResult;

      ValidateResult(result, projectUid, string.Empty, 0, "success");
    }




    //    [TestMethod]
    //    public async Task testtodo()
    //    {
    //      var customerUid = Guid.NewGuid().ToString();
    //      var latitude = 45.45;
    //      var longitude = 67.77;
    //      var projectTypes = new int[] {(int) ProjectType.LandFill, (int) ProjectType.Standard};
    //      DateTime? timeOfPosition = DateTime.UtcNow;

    //      var select = GetIntersectingProjectsTest(customerUid, latitude, longitude, 
    //        projectTypes, timeOfPosition);

    //    }

    //    private string GetIntersectingProjectsTest(string customerUid,
    //     double latitude, double longitude, int[] projectTypes, DateTime? timeOfPosition)
    //    {
    //      var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";
    //      var projectTypesString = string.Empty;
    //      if (projectTypes.Any())
    //      {
    //        projectTypesString += " AND p.fk_ProjectTypeID IN ( ";
    //        for (int i = 0; i < projectTypes.Length; i++)
    //        {
    //          projectTypesString += projectTypes[i] + ((i < projectTypes.Length - 1) ? "," : "");
    //        }

    //        projectTypesString += " ) ";
    //      }
    //      var timeRangeString = string.Empty;
    //      if (timeOfPosition != null)
    //      {
    //        timeRangeString = $"  AND '{timeOfPosition.Value.Date}' BETWEEN p.StartDate AND p.EndDate ";
    //      }

    //      var select = "SELECT DISTINCT " +
    //                   "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
    //                   "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT, " +
    //                   "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
    //                   "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, " +
    //                   "        ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID " +
    //                   "      FROM Project p " +
    //                   "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
    //                   "        LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID " +
    //                   "        LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID " +
    //                   "       WHERE     p.IsDeleted = 0 " +
    //                   $"        AND cp.fk_CustomerUID = '{customerUid}' " +
    //                   $"       {projectTypesString} " +
    //                   $"       {timeRangeString} " +
    //                   $"        AND st_Intersects({point}, PolygonST) = 1";
    //;
    //      return select;
    //    }

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