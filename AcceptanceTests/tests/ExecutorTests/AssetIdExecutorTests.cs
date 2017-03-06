using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using log4netExtensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.GenericConfiguration;
using VSS.TagFileAuth.Service.WebApiModels.Enums;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using Repositories;

namespace RepositoryTests
{
  [TestClass]
  public class AssetIdExecutorTests : ExecutorTestData
  {

    [TestMethod]
    public void CanCallAssetIDExecutorWithNonExistingDeviceAsset()
    {
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAsset()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }


    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAssetAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAssetAndCustomerSub_NoOwnerCustomer()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = null;
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      // customer sub but the asset has not owning Customer
      owningCustomerUID = Guid.NewGuid();
      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    //  todo waiting for AssetSubs to be implemented in MDConsumer
    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAssetAndAssetSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM. todo waiting for AssetSubs to be implemented in MDConsumer");
    }

    //  todo waiting for AssetSubs to be implemented in MDConsumer
    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingDeviceAssetAndAssetAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(-1, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      isCreatedOk = CreateCustomerSub(owningCustomerUID.Value, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      isCreatedOk = CreateAssetSub(assetUID, owningCustomerUID.Value, "3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Asset subscription");

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(16, result.machineLevel, "executor returned incorrect serviceType, should be 16 3dPM. todo waiting for AssetSubs to be implemented in MDConsumer");
    }


    [TestMethod]
    public void CanCallAssetIDExecutorWithNonExistingProject()
    {
      int legacyProjectId = new Random().Next(0, int.MaxValue);

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingProject()
    {
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      var isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsFalse(result.result, "unsuccessful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(0, result.machineLevel, "executor returned incorrect serviceType, should be unknown(0)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingProjectAndCustomerSub()
    {
      // tests path where only ProjectId and goes via CheckForManual3DCustomerBasedSub()
      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = Guid.NewGuid();
      var isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, 0, "");
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(-1, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }

    [TestMethod]
    public void CanCallAssetIDExecutorWithExistingProjectAndDeviceAndCustomerSub()
    {
      Guid assetUID = Guid.NewGuid();
      long legacyAssetId = new Random().Next(0, int.MaxValue);
      Guid? owningCustomerUID = Guid.NewGuid();
      Guid deviceUID = Guid.NewGuid();
      string deviceSerialNumber = "The radio serial " + deviceUID.ToString();
      DeviceTypeEnum deviceType = DeviceTypeEnum.Series522;
      var isCreatedOk = CreateAssetDeviceAssociation(assetUID, legacyAssetId, owningCustomerUID, deviceUID, deviceSerialNumber, deviceType.ToString());
      Assert.IsTrue(isCreatedOk, "created assetDevice association");

      Guid projectUID = Guid.NewGuid();
      int legacyProjectId = new Random().Next(0, int.MaxValue);
      Guid customerUID = owningCustomerUID.Value;
      isCreatedOk = CreateProject(projectUID, legacyProjectId, customerUID);
      Assert.IsTrue(isCreatedOk, "created project");

      isCreatedOk = CreateCustomerSub(customerUID, "Manual 3D Project Monitoring");
      Assert.IsTrue(isCreatedOk, "created Customer subscription");

      GetAssetIdRequest assetIdRequest = GetAssetIdRequest.CreateGetAssetIdRequest(legacyProjectId, (int)deviceType, deviceSerialNumber);
      assetIdRequest.Validate();

      var result = RequestExecutorContainer.Build<AssetIdExecutor>(factory, logger).Process(assetIdRequest) as GetAssetIdResult;
      Assert.IsNotNull(result, "executor should always return a result");
      Assert.IsTrue(result.result, "successful");
      Assert.AreEqual(legacyAssetId, result.assetId, "executor returned incorrect LegacyAssetId");
      Assert.AreEqual(18, result.machineLevel, "executor returned incorrect serviceType, should be Man 3d pm (CG==18)");
    }
  }
}

