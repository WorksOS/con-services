using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using VSS.Productivity3D.WebApiModels.Enums;
using VSS.Productivity3D.WebApiModels.Executors;
using VSS.Productivity3D.WebApiModels.Models;
using VSS.Productivity3D.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace RepositoryTests
{
  [TestClass]
  public class ProjectBoundariesAtDateExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_NonExistingAsset()
    {
      int legacyAssetId = new Random().Next(0, int.MaxValue);
      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);

      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAsset()
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

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSub()
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

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject()
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

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(1, result.projectBoundaries.Length, "executor returned incorrect points result");
      Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
      Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject()
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
      CreateCustomerSub(owningCustomerUID, "Manual 3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(1, result.projectBoundaries.Length, "executor returned incorrect points result");
      Assert.AreEqual(legacyProjectId, result.projectBoundaries[0].ProjectID, "executor returned incorrect legacyProjectId");
      Assert.AreEqual(5, result.projectBoundaries[0].Boundary.FencePoints.Length, "executor returned incorrect projectBoundary");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAnd2Projects()
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
      CreateCustomerSub(owningCustomerUID, "Manual 3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      Guid projectUID2 = Guid.NewGuid();
      int legacyProjectId2 = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID2, legacyProjectId2, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill, "POLYGON((170 10, 190 10, 190 40, 180 40, 170 40, 170 10))");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
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
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_MissingAssetOwner()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      // note no owningCustomerUID
      CreateAssetDeviceAssociation(assetUID, legacyAssetId, null, deviceUID, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUID, tccOrgId);
      CreateCustomerSub(owningCustomerUID, "Manual 3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_DifferentCustomers()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid assetOwningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUID, legacyAssetId, assetOwningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(assetOwningCustomerUID, tccOrgId);

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid projectCustomerUID = Guid.NewGuid();
      CreateProject(projectUID, legacyProjectId, projectCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);
      CreateCustomerSub(projectCustomerUID, "Manual 3D Project Monitoring");

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndCustSubAndProject_tagFileUtcMismatch()
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
      CreateCustomerSub(owningCustomerUID, "Manual 3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = new DateTime(2016, 02, 01).AddDays(-1); // this is projectStartDate
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject_AccountOwner()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      string tccOrgId = "";

      CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      CreateCustomer(owningCustomerUID, tccOrgId, CustomerType.Account);
      CreateAssetSub(assetUID, owningCustomerUID, "3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

    [TestMethod]
    public void ProjectBoundariesAtDateExecutor_ExistingAssetAndAssetSubAndProject_subForDifferentCustomer()
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

      Guid assetSubCustomerUID = Guid.NewGuid();
      CreateAssetSub(assetUID, assetSubCustomerUID, "3D Project Monitoring");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateProject(projectUID, legacyProjectId, owningCustomerUID, VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.Standard);

      DateTime timeOfPositionUtc = DateTime.UtcNow.AddHours(-2);
      GetProjectBoundariesAtDateRequest projectBoundariesAtDateExecutorRequest = GetProjectBoundariesAtDateRequest.CreateGetProjectBoundariesAtDateRequest(legacyAssetId, timeOfPositionUtc);
      projectBoundariesAtDateExecutorRequest.Validate();

      var result = RequestExecutorContainer.Build<ProjectBoundariesAtDateExecutor>(factory, logger).Process(projectBoundariesAtDateExecutorRequest) as GetProjectBoundariesAtDateResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.IsNotNull(result.projectBoundaries, "executor returned incorrect projectBoundary result");
      Assert.AreEqual(0, result.projectBoundaries.Length, "executor returned incorrect points result");
    }

  }
}

