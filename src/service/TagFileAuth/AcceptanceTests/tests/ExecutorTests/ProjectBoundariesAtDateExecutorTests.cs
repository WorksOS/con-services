using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ExecutorTests
{
  [TestClass]
  public class ProjectBoundariesAtDateExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_NonExistingAsset()
    {
      int legacyAssetId = new Random().Next(0, int.MaxValue);
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAsset()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSub()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateAssetSub(assetUid, owningCustomerUid, "3D Project Monitoring");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateAssetSub(assetUid, owningCustomerUid, "3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(1, result.projectBoundaries.Length, "executor returned incorrect points result");
      Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
      Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateCustomerSub(owningCustomerUid, "Manual 3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(1, result.projectBoundaries.Length, "executor returned incorrect points result");
      Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
      Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAnd2Projects()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateCustomerSub(owningCustomerUid, "Manual 3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      Guid projectUid2 = Guid.NewGuid();
      int legacyProjectId2 = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid2, legacyProjectId2, owningCustomerUid, ProjectType.LandFill, "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(2, result.projectBoundaries.Length, "executor returned incorrect points result");

      if (result.projectBoundaries[0].ProjectID == legacyProjectId)
      {
        Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
        Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
        Assert.AreEqual(legacyProjectId2, result.projectBoundaries[1].ProjectID, "executor returned incorrect legacyProjectId2");
        Assert.AreEqual(6, result.projectBoundaries[1].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary2");
      }
      else
      {
        Assert.AreEqual(legacyProjectId2, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
        Assert.AreEqual(6, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
        Assert.AreEqual(legacyProjectId, result.projectBoundaries[1].ProjectID, "executor returned incorrect legacyProjectId2");
        Assert.AreEqual(5, result.projectBoundaries[1].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary2");
      }

    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAnd2Projects_OneDeleted()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber,
        deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateCustomerSub(owningCustomerUid, "Manual 3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid,
        ProjectType.Standard);

      Guid projectUid2 = Guid.NewGuid();
      int legacyProjectId2 = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid2, legacyProjectId2, owningCustomerUid,
        ProjectType.LandFill,
        "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))");
      DeleteProject(projectUid2);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest =
        GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore,
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result =
        await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(1, result.projectBoundaries.Length, "executor returned incorrect number of projects");

      Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
      Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_MissingAssetOwner()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      // note no owningCustomerUID
      CreateAssetDeviceAssociation(assetUid, legacyAssetId, null, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateCustomerSub(owningCustomerUid, "Manual 3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_DifferentCustomers()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid assetOwningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, assetOwningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(assetOwningCustomerUid, tccOrgId);

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid projectCustomerUid = Guid.NewGuid();
      CreateProject(projectUid, legacyProjectId, projectCustomerUid, ProjectType.Standard);
      CreateCustomerSub(projectCustomerUid, "Manual 3D Project Monitoring");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_tagFileUtcMismatch()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);
      CreateCustomerSub(owningCustomerUid, "Manual 3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = new DateTime(2016, 02, 01).AddDays(-1); // this is projectStartDate
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject_AccountOwner()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId, CustomerType.Account);
      CreateAssetSub(assetUid, owningCustomerUid, "3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore, 
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public async Task ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject_subForDifferentCustomerAsync()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUid, tccOrgId);

      Guid assetSubCustomerUid = Guid.NewGuid();
      CreateAssetSub(assetUid, assetSubCustomerUid, "3D Project Monitoring");

      Guid projectUid = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUid, legacyProjectId, owningCustomerUid, ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var executor = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(logger, configStore,
        assetRepo, deviceRepo, customerRepo, 
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

  }
}

