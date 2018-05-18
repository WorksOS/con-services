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
  public class ProjectUidExecutorTests : ExecutorBaseTests
  {
    private ILoggerFactory loggerFactory;
    private string projectUidToBeDiscovered;
    private string assetUid;
    private DateTime timeOfLocation;
    private string assetOwningCustomerUid;

    private GetProjectUidRequest projectUidRequest;

    private AssetDeviceIds assetDeviceIds;
    private Mock<IDeviceRepository> deviceRepo;

    private List<Subscription> subscriptions;
    private Mock<ISubscriptionRepository> subscriptionRepo;

    private List<Project> projects;
    private Mock<IProjectRepository> projectRepo;



    [TestInitialize]
    public virtual void InitTest()
    {
      base.InitTest();

      projectUidToBeDiscovered = Guid.NewGuid().ToString();
      assetUid = Guid.NewGuid().ToString();
      timeOfLocation = DateTime.UtcNow;
      assetOwningCustomerUid = Guid.NewGuid().ToString();
      projectUidRequest = GetProjectUidRequest.CreateGetProjectUidRequest(6, "radSer45", 91, 181, timeOfLocation);
      loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

      assetDeviceIds = new AssetDeviceIds() { OwningCustomerUID = assetOwningCustomerUid, AssetUID = assetUid };
      deviceRepo = new Mock<IDeviceRepository>();
     
      subscriptions = new List<Subscription>()
      {
        new Subscription()
        {
          CustomerUID = assetOwningCustomerUid,
          ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring,
          StartDate = timeOfLocation.AddDays(-10),
          EndDate = timeOfLocation.AddDays(10),
          SubscriptionUID = Guid.NewGuid().ToString(),
          LastActionedUTC = DateTime.UtcNow
        }
      };
     
      projects = new List<Project>()
      {
        new Project()
        {
          ProjectUID = projectUidToBeDiscovered,
          CustomerUID = assetOwningCustomerUid,
          ProjectType = ProjectType.ProjectMonitoring
        }
      };
      projectRepo = new Mock<IProjectRepository>();
      subscriptionRepo = new Mock<ISubscriptionRepository>();
    }

    [TestMethod]
    public async Task ProjectUidExecutor_InvalidParameters()
    {
      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionRepository);

      var ex = await Assert.ThrowsExceptionAsync<ServiceException>(() => executor.ProcessAsync((GetProjectUidRequest)null));
      Assert.AreEqual(HttpStatusCode.BadRequest, ex.Code);
      Assert.AreEqual(-3, ex.GetResult.Code);
      Assert.AreEqual("Serialization error", ex.GetResult.Message);
    }

    [TestMethod]
    public async Task ProjectUidExecutor_NoAssetDeviceAssociation()
    {
      deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync((AssetDeviceIds)null);

      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepo.Object, customerRepository, projectRepository, subscriptionRepository);
      var result = await executor.ProcessAsync(projectUidRequest) as GetProjectUidResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(string.Empty, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(3033, result.Code, "executor returned incorrect result code");
    }

    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAnd3dPmSubscription()
    {
      deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDeviceIds);

      subscriptions[0].ServiceTypeID = (int)ServiceTypeEnum.ThreeDProjectMonitoring;
      IEnumerable < Subscription > eSubs = subscriptions.ToList();
      subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      projects[0].ProjectType = ProjectType.Standard;
      projectRepo.Setup(d => d.GetStandardProject(assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), timeOfLocation)).ReturnsAsync(projects);
      
      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepo.Object, customerRepository, projectRepo.Object, subscriptionRepo.Object);
      var result = await executor.ProcessAsync(projectUidRequest) as GetProjectUidResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUidToBeDiscovered, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
    }

    [TestMethod]
    public async Task ProjectUidExecutor_StandardProjectAndNo3dPmSubscription()
    {
      deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDeviceIds);

      IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
      subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      projects[0].ProjectType = ProjectType.Standard;
      projectRepo.Setup(d => d.GetStandardProject(assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), timeOfLocation)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepo.Object, customerRepository, projectRepo.Object, subscriptionRepo.Object);
      var result = await executor.ProcessAsync(projectUidRequest) as GetProjectUidResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(string.Empty, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(3029, result.Code, "executor returned incorrect result code");
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndPMSubscription()
    {
      deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDeviceIds);

      subscriptions[0].ServiceTypeID = (int) ServiceTypeEnum.ProjectMonitoring;
      IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
      subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);
      
      projects[0].ProjectType = ProjectType.ProjectMonitoring;
      projectRepo.Setup(d => d.GetProjectMonitoringProject(assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), timeOfLocation, (int)ProjectType.ProjectMonitoring, (int)ServiceTypeEnum.ProjectMonitoring)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepo.Object, customerRepository, projectRepo.Object, subscriptionRepo.Object);
      var result = await executor.ProcessAsync(projectUidRequest) as GetProjectUidResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(projectUidToBeDiscovered, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(0, result.Code, "executor returned incorrect result code");
    }

    [TestMethod]
    public async Task ProjectUidExecutor_PMProjectAndNoPMSubscription()
    {
     deviceRepo.Setup(d => d.GetAssociatedAsset(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(assetDeviceIds);

      IEnumerable<Subscription> eSubs = (new List<Subscription>()).ToList();
      subscriptionRepo.Setup(d => d.GetSubscriptionsByAsset(assetUid, It.IsAny<DateTime>())).ReturnsAsync(eSubs);

      projects[0].ProjectType = ProjectType.ProjectMonitoring;
      projectRepo.Setup(d => d.GetProjectMonitoringProject(assetOwningCustomerUid, It.IsAny<double>(), It.IsAny<double>(), timeOfLocation, (int)ProjectType.ProjectMonitoring, (int)ServiceTypeEnum.ProjectMonitoring)).ReturnsAsync(projects);

      var executor = RequestExecutorContainer.Build<ProjectUidExecutor>(loggerFactory.CreateLogger<ProjectUidExecutorTests>(), configStore,
        assetRepository, deviceRepo.Object, customerRepository, projectRepo.Object, subscriptionRepo.Object);
      var result = await executor.ProcessAsync(projectUidRequest) as GetProjectUidResult;

      Assert.IsNotNull(result, "executor returned nothing");
      Assert.AreEqual(string.Empty, result.ProjectUid, "executor returned incorrect ProjectUid");
      Assert.AreEqual(3029, result.Code, "executor returned incorrect result code");
    }

  }
}