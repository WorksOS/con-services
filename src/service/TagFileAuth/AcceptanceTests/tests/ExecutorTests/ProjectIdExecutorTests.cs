using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectIdExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public async Task ProjectIDExecutor_NonExistingAsset()
    {
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      double latitude = 0.0;
      double longitude = 0.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, "");
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ExistingAsset()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;

      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      double latitude = 0.0;
      double longitude = 0.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, "");
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ExistingAssetAndAssetSubAndStandardProject_inside()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      isCreatedOk = CreateCustomer(owningCustomerUID, tccOrgId);
      Assert.IsTrue(isCreatedOk, "created Customer");

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created AssetSub 3dPM");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      isCreatedOk = CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);
      Assert.IsTrue(isCreatedOk, "created project");

      double latitude = 15.0; 
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ExistingAssetAndAssetSubAndStandardProject_outside()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUID, tccOrgId);
      CreateAssetSub(assetUID, owningCustomerUID, "3D Project Monitoring");
      
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      double latitude = 50.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ExistingAssetAndPMProjectAndSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = Guid.NewGuid().ToString();

      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      isCreatedOk = CreateCustomer(owningCustomerUID, tccOrgId);
      Assert.IsTrue(isCreatedOk, "created Customer");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      isCreatedOk = CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.ProjectMonitoring);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateProjectSub(projectUID, owningCustomerUID, "Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created project sub");

      double latitude = 15.0; 
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_JohnDoeAssetAndPMProjectAndSub()
    {
      long legacyAssetId = -1; // john doe
      Guid owningCustomerUID = Guid.NewGuid();
      string tccOrgId = Guid.NewGuid().ToString();

      CreateCustomer(owningCustomerUID, tccOrgId);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.ProjectMonitoring);
      CreateProjectSub(projectUID, owningCustomerUID, "Project Monitoring");
      
      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_JohnDoeAssetAndPMProjectAndSub_tccOrgDoesntExist()
    {
      long legacyAssetId = -1; // john doe
      Guid owningCustomerUID = Guid.NewGuid();
      string tccOrgIdCreated = Guid.NewGuid().ToString();
      string tccOrgIdQueried = Guid.NewGuid().ToString();

      CreateCustomer(owningCustomerUID, tccOrgIdCreated);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.ProjectMonitoring);
      CreateProjectSub(projectUID, owningCustomerUID, "Project Monitoring");

      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgIdQueried);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ManualAssetAndPMProjectAndSub()
    {
      long legacyAssetId = -2; // 'manualImport'
      Guid owningCustomerUID = Guid.NewGuid();
      string tccOrgId = Guid.NewGuid().ToString();

      CreateCustomer(owningCustomerUID, tccOrgId);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.ProjectMonitoring);
      CreateProjectSub(projectUID, owningCustomerUID, "Project Monitoring");

      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-3, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ExistingAssetAndLandfillProjectAndSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = Guid.NewGuid().ToString();

      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      isCreatedOk = CreateCustomer(owningCustomerUID, tccOrgId);
      Assert.IsTrue(isCreatedOk, "created Customer");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      isCreatedOk = CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateProjectSub(projectUID, owningCustomerUID, "Landfill");
      Assert.IsTrue(isCreatedOk, "created project sub");

      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_JohnDoeAssetAndLandfillProjectAndSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = -1; // John doe
      Guid owningCustomerUID = Guid.NewGuid();
      string tccOrgId = Guid.NewGuid().ToString();

      CreateCustomer(owningCustomerUID, tccOrgId);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      CreateProjectSub(projectUID, owningCustomerUID, "Landfill");

      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_ManualAssetAndLandfillProjectAndSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = -2; // 'manualImport'
      Guid owningCustomerUID = Guid.NewGuid();
      string tccOrgId = Guid.NewGuid().ToString();

      CreateCustomer(owningCustomerUID, tccOrgId);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      CreateProjectSub(projectUID, owningCustomerUID, "Landfill");

      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.AreEqual(legacyProjectId, result.projectId, "executor returned incorrect LegacyProjectId");
    }

    [TestMethod]
    public async Task ProjectIDExecutor_OverlappingLFAndPMProjects()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = Guid.NewGuid().ToString();

      CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUID, tccOrgId);
      
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.ProjectMonitoring);
      CreateProjectSub(projectUID, owningCustomerUID, "Project Monitoring");
     
      Guid projectUIDOverlapping = Guid.NewGuid();
      int legacyProjectIdOverlapping = new Random().Next(0, int.MaxValue);

      CreateProject(projectUIDOverlapping, legacyProjectIdOverlapping, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      CreateProjectSub(projectUIDOverlapping, owningCustomerUID, "Landfill");
      
      double latitude = 15.0;
      double longitude = 180.0;
      double height = 0.0;
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectIdRequest projectIdRequest = GetProjectIdRequest.CreateGetProjectIdRequest(legacyAssetId, latitude, longitude, height, timeOfPositionUtc, tccOrgId);
      projectIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectIdExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectIdRequest) as GetProjectIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-2, result.projectId, "executor should return -2 as overlapping valid projects are illegal");
    }

  }
}

