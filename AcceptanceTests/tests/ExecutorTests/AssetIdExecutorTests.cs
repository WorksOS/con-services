using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace ExecutorTests
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public void AssetIDExecutor_NonExistingDeviceAsset()
    {
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingDeviceAsset()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }


    [TestMethod]
    public void AssetIDExecutor_ExistingDeviceAssetAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingDeviceAssetAndCustomerSub_NoOwnerCustomer()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = null;
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      // customer sub but the asset has not owning Customer
      owningCustomerUID = Guid.NewGuid();
      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingDeviceAssetAndAssetSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM.");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingDeviceAssetAndAssetAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM.");
    }

    [TestMethod]
    public void AssetIDExecutor_NonExistingProject()
    {
      int legacyProjectId = new Random().Next(0, int.MaxValue);

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      var isCreatedOk = CreateCustomer(customerUID, "");
      Assert.IsTrue(isCreatedOk, "created customer");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProjectAndCustomerSub()
    {
      // tests path where only ProjectId and goes via CheckForManual3DCustomerBasedSub()
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();

      CreateCustomer(customerUID, "");
      CreateProject(projectUID, legacyProjectId, customerUID);

      var isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProjectAndDeviceAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = owningCustomerUID.Value;
      CreateCustomer(customerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProjectAndDeviceAndAssetSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = owningCustomerUID.Value;
      CreateCustomer(customerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID,
        VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 3dPM (CG==16)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProjectAndDeviceAndCustomerSub_SNM940ToFindSNM941()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum actualDeviceType = DeviceTypeEnum.SNM941;
      DeviceTypeEnum requestedDeviceType = DeviceTypeEnum.SNM940;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, actualDeviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = owningCustomerUID.Value;
      CreateCustomer(customerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int) requestedDeviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public void AssetIDExecutor_ExistingProjectAndDeviceAndCustomerSub_SNM941ToNOTFindSNM940()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum actualDeviceType = DeviceTypeEnum.SNM940;
      DeviceTypeEnum requestedDeviceType = DeviceTypeEnum.SNM941;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID,
        deviceSerialNumber, actualDeviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = owningCustomerUID.Value;
      CreateCustomer(customerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int) requestedDeviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result =
        RequestExecutorContainer.Build<AssetIdExecutor>(Factory, Logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }
  }
}