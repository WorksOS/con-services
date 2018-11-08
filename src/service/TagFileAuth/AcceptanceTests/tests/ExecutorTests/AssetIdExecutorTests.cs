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
  public class AssetIdExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public async Task AssetIDExecutor_NonExistingDeviceAssetAsync()
    {
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor = RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
        assetRepo, deviceRepo, customerRepo,
        projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingDeviceAsset()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }


    [TestMethod]
    public async Task AssetIDExecutor_ExistingDeviceAssetAndCustomerSub()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUid = Guid.NewGuid();
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateCustomerSub(owningCustomerUid.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingDeviceAssetAndCustomerSub_NoOwnerCustomer()
    {
      Guid assetUid = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUid = null;
      Guid deviceUid = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUid;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUid, legacyAssetId, owningCustomerUid, deviceUid,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      // customer sub but the asset has not owning Customer
      owningCustomerUid = Guid.NewGuid();
      isCreatedOk = CreateCustomerSub(owningCustomerUid.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingDeviceAssetAndAssetSub()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM.");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingDeviceAssetAndAssetAndCustomerSub()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM.");
    }

    [TestMethod]
    public async Task AssetIDExecutor_NonExistingProject()
    {
      int legacyProjectId = new Random().Next(0, int.MaxValue);

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProject()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.Result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndCustomerSub()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDevice_WithProjectCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? assetCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, assetCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid projectCustomerUID = Guid.NewGuid();
      CreateCustomer(projectCustomerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, projectCustomerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(projectCustomerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created man3d sub on the Projects Customer");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int) deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDevice_WithAssetCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? assetCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, assetCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid projectCustomerUID = Guid.NewGuid();
      CreateCustomer(projectCustomerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, projectCustomerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(assetCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created man3d sub on the Assets Customer");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDevice_WithProjectAndAssetCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? assetCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, assetCustomerUID, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid projectCustomerUID = Guid.NewGuid();
      CreateCustomer(projectCustomerUID, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, projectCustomerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(projectCustomerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created man3d sub on the Projects Customer");

      isCreatedOk = CreateCustomerSub(assetCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created man3d sub on the Assets Customer");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndSingleAssetSub_Manual()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 3dPM (CG==16)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndMultipleAssetSubs_Manual()
    {
      // this test a fix for #59069 "TagFileAuth: Manual Import fails where Auto succeeded"
      // where the asset has 2x 3dpm subs (1 for the dealer and the other for the customer). 
      // When the tag file is loaded automatically, either sub will be used, 
      // however when done manually, the sub MUST have the same customerUID as the Project. 

      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? customerUid = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, customerUid, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateCustomer(customerUid.Value, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUid.Value,
        VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      Assert.IsTrue(isCreatedOk, "created project");

      Guid? dealerCustomerUID = Guid.NewGuid();
      isCreatedOk = CreateAssetSub(assetUID, dealerCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription for the dealer");

      isCreatedOk = CreateAssetSub(assetUID, customerUid.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 3dPM (CG==16)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndMultipleAssetSubs_ManualReversed()
    {
      // this test a fix for #59069 "TagFileAuth: Manual Import fails where Auto succeeded"
      // where the asset has 2x 3dpm subs (1 for the dealer and the other for the customer). 
      // When the tag file is loaded automatically, either sub will be used, 
      // however when done manually, the sub MUST have the same customerUID as the Project. 

      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? customerUid = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, customerUid, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateCustomer(customerUid.Value, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUid.Value,
        VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateAssetSub(assetUID, customerUid.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      Guid? dealerCustomerUID = Guid.NewGuid();
      isCreatedOk = CreateAssetSub(assetUID, dealerCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription for the dealer");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 3dPM (CG==16)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndMultipleAssetSubs_Auto()
    {
      // similar to manual except that GetAssetIdRequest has ProjectID = -1

      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? customerUid = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID;
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, customerUid, deviceUID,
        deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      CreateCustomer(customerUid.Value, "");
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUid.Value,
        VSS.VisionLink.Interfaces.Events.MasterData.Models.ProjectType.LandFill);
      Assert.IsTrue(isCreatedOk, "created project");

      Guid? dealerCustomerUID = Guid.NewGuid();
      isCreatedOk = CreateAssetSub(assetUID, dealerCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription for the dealer");

      isCreatedOk = CreateAssetSub(assetUID, customerUid.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      GetAssetIdRequest assetIdRequest =
        GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore,
          assetRepo, deviceRepo, customerRepo,
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 3dPM (CG==16)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndCustomerSub_SNM940ToFindSNM941()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public async Task AssetIDExecutor_ExistingProjectAndDeviceAndCustomerSub_SNM941ToNOTFindSNM940()
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

      var executor =
        RequestExecutorContainer.Build<AssetIdExecutor>(logger, configStore, 
          assetRepo, deviceRepo, customerRepo, 
          projectRepo, subscriptionRepo);
      var result = await executor.ProcessAsync(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.Result, "successful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }
  }
}