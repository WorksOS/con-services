using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class BssDeviceServiceTests : BssUnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void GetDeviceTypeByPartNumber_DeviceTypeFound_ReturnsDeviceType()
    {
      string partNumber = "79323-30";
      DeviceTypeEnum? result = Services.Devices().GetDeviceTypeByPartNumber(partNumber);
      Assert.AreEqual(DeviceTypeEnum.PL321, result, "Result should be PL321.");
    }

    [DatabaseTest]
    [TestMethod]
    public void GetDeviceTypeByPartNumber_DeviceTypeNotFound_ReturnsNull()
    {
      string partNumber = "SOME_NUMBER_THAT_DOES_NOT_EXIST";
      DeviceTypeEnum? result = Services.Devices().GetDeviceTypeByPartNumber(partNumber);
      Assert.IsNull(result, "Null value is expected.");
    }

    [DatabaseTest]
    [TestMethod]
    public void GetDeviceByIbKey_ExistingDevice_ReturnsDeviceAndInstalledOnAssetAndOwner()
    {
      var testOwner = Entity.Customer.Dealer.Save();
      var testDevice = Entity.Device.MTS521.OwnerBssId(testOwner.BSSID).Save();
      var testAsset = Entity.Asset.WithDevice(testDevice).Save();

      var device = Services.Devices().GetDeviceByIbKey(testDevice.IBKey);

      Assert.IsTrue(device.Exists, "Device does not exist");
      Assert.AreEqual(testDevice.ID, device.Id, "Id not equal");
      Assert.AreEqual(testDevice.GpsDeviceID, device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, device.Type);
      Assert.AreEqual(testDevice.OwnerBSSID, device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsTrue(device.OwnerExists, "Owner does not exist");
      Assert.AreEqual(testOwner.ID, device.OwnerId, "OwnerId not equal");
      Assert.AreEqual(testOwner.BSSID, device.Owner.BssId, "Owner BssId not equal");
      Assert.AreEqual(testOwner.Name, device.Owner.Name, "Owner name not equal");
      Assert.AreEqual(testOwner.fk_CustomerTypeID, (int)device.Owner.Type, "Owner type not equal");

      Assert.IsTrue(device.AssetExists, "Asset does not exist");
      Assert.AreEqual(testAsset.Name, device.Asset.Name, "Name not equal");
      Assert.AreEqual(testAsset.SerialNumberVIN, device.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(testAsset.fk_MakeCode, device.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(testAsset.Model, device.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(testAsset.ManufactureYear, device.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(string.Format("{0:MM/dd/yyyy HH:mm}", testAsset.InsertUTC), string.Format("{0:MM/dd/yyyy HH:mm}", device.Asset.InsertUtc), "InsertUTC is not equal");
    }

    [TestMethod]
    [DatabaseTest]
    public void CreateDevice_Success()
    {
      AssetDeviceContext _context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          Type = DeviceTypeEnum.Series521,
          GpsDeviceId = IdGen.GetId().ToString(),
          IbKey = IdGen.GetId().ToString(),
          PartNumber = IdGen.GetId().ToString(),
          OwnerBssId = IdGen.GetId().ToString()
        }
      };

      var device = Services.Devices().CreateDevice(_context.IBDevice);
      Assert.IsNotNull(device);

      var savedDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).SingleOrDefault();
      Assert.IsNotNull(savedDevice);
      Assert.AreEqual((int)_context.IBDevice.Type, savedDevice.fk_DeviceTypeID, "Device types should match.");
      Assert.AreEqual(_context.IBDevice.GpsDeviceId, savedDevice.GpsDeviceID, "GPSDeviceIDs should match.");
      Assert.AreEqual(_context.IBDevice.IbKey, savedDevice.IBKey, "IBKeys should match.");
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID, "Device State should be installed.");
    }

    [TestMethod]
    [DatabaseTest]
    public void ReconfigureDevice_OldDeviceToProvision_NewDeviceToSubscribed_PLOutMessagesWereCreated()
    {
      Device oldDevice;
      Device newDevice;
      ReconfigureDeviceTestSetup(out oldDevice, out newDevice);

      Services.Devices().ReconfigureDevice(
        oldDevice.ID, oldDevice.GpsDeviceID, DeviceTypeEnum.PL321,
        newDevice.ID, newDevice.GpsDeviceID, DeviceTypeEnum.PL321,
        DateTime.UtcNow);

      oldDevice = Ctx.OpContext.DeviceReadOnly.First(x => x.ID == oldDevice.ID);
      newDevice = Ctx.OpContext.DeviceReadOnly.First(x => x.ID == newDevice.ID);

      List<PLOut> ploutRecords = (from r in Ctx.RawContext.PLOutReadOnly where r.ModuleCode == newDevice.GpsDeviceID select r).ToList<PLOut>();
      Assert.IsNotNull(ploutRecords, "Expected records in PLOut table for new device");
      Assert.AreNotEqual(0, ploutRecords.Count, "Expected records in PLOut table for new device");
    }

    private static void ReconfigureDeviceTestSetup(out Device oldDevice, out Device newDevice)
    {
      var dealer = Entity.Customer.Dealer.BssId(IdGen.StringId()).Save();

      oldDevice = Entity.Device.PL321
        .IbKey(IdGen.StringId())
        .OwnerBssId(dealer.BSSID)
        .GpsDeviceId(IdGen.StringId())
        .DeviceState(DeviceStateEnum.Subscribed)
        .SyncWithNhRaw().Save();

      newDevice = Entity.Device.PL321
        .IbKey(IdGen.StringId())
        .OwnerBssId(dealer.BSSID)
        .GpsDeviceId(IdGen.StringId())
        .DeviceState(DeviceStateEnum.Provisioned)
        .SyncWithNhRaw().Save();

      var asset = Entity.Asset.WithDevice(newDevice).WithCoreService().Save();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TransferOwnership_DeviceIdNotDefined_Exception()
    {
      new BssDeviceService().TransferOwnership(0, "NewOwnerBssId");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void TransferOwnership_OwnerBssIdNotDefined_Exception()
    {
      new BssDeviceService().TransferOwnership(IdGen.GetId(), string.Empty);
    }

    [DatabaseTest]
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void TransferOwnership_CurrentOwnerAndNewOwnerAreEqual_Exception()
    {
      var device = Entity.Device.MTS522.Save();

      new BssDeviceService().TransferOwnership(device.ID, device.OwnerBSSID);
    }

    [DatabaseTest]
    [TestMethod]
    public void TransferOwnership_Success_ReturnsTrue()
    {
      var device = Entity.Device.MTS522.Save();

      bool success = new BssDeviceService().TransferOwnership(device.ID, "NewOwnerBssId");

      Assert.IsTrue(success, "Service did not return true.");
    }

    #region Tests requiring Environment.MachineName to match in order to pass

    [TestMethod]
    public void IsDeviceReadOnly_PL321_IsProductionBSS_ReturnsFalse()
    {
      string ibKey = IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "true";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL321, ibKey);
      Assert.IsFalse(result, "Should return False");
    }

    [TestMethod]
    public void IsDeviceReadOnly_PL321_IsNotProductionBSS_ReturnsTrue()
    {
      string ibKey = IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "false";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL321, ibKey);
      Assert.IsTrue(result, "Should return True");
    }

    [TestMethod]
    public void IsDeviceReadOnly_PL121_IsProductionBSS_ReturnsFalse()
    {
      string ibKey = IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "true";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL121, ibKey);
      Assert.IsFalse(result, "Should return False");
    }

    [TestMethod]
    public void IsDeviceReadOnly_PL121_IsNotProductionBSS_ReturnsTrue()
    {
      string ibKey = IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "false";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL121, ibKey);
      Assert.IsTrue(result, "Should return True");
    }

    [TestMethod]
    public void IsDeviceReadOnly_MTS521_Failure()
    {
      string ibKey = IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "true";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.Series521, ibKey);
      Assert.IsTrue(result, "Should return True");
    }

    [TestMethod]
    public void IsDeviceReadOnly_PL321_NegativeIBKey_Failure()
    {
      string ibKey = "-" + IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "true";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL321, ibKey);
      Assert.IsTrue(result, "Should return True");
    }

    [TestMethod]
    public void IsDeviceReadOnly_PL321_NonProdEnv_Failure()
    {
      string ibKey = "-" + IdGen.GetId().ToString();
      ConfigurationManager.AppSettings["IsProductionBSS"] = "false";
      DeviceConfig.ResetEnvironmentFlag();

      var result = Services.Devices().IsDeviceReadOnly(DeviceTypeEnum.PL321, ibKey);
      Assert.IsTrue(result, "Should return True");
    }
    #endregion

    #region Device (De)Registered Test cases

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_NoService_StateUpdatedToSubscribed_Success()
    {
      var savedDevice = UpdateDeviceState(createService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_ActiveCoreService_StateUpdatedToSubscribed_Success()
    {
      var savedDevice = UpdateDeviceState();
      Assert.AreEqual((int)DeviceStateEnum.Subscribed, savedDevice.fk_DeviceStateID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_ExpiredCoreService_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDeviceState(isActiveService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_ExpiredHalth_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDeviceState(isCore: false, isActiveService: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_ActiveHealth_StateUpdatedToProvisioned_Success()
    {
      var savedDevice = UpdateDeviceState(isCore: false);
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, savedDevice.fk_DeviceStateID);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceState_StateUpdatedToDeRegistered_Success()
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.StringId()).Save();
      new BssDeviceService().UpdateDeviceState(device.ID, DeviceStateEnum.DeregisteredTechnician);
      var savedDevice = Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
      Assert.AreEqual((int)DeviceStateEnum.DeregisteredTechnician, savedDevice.fk_DeviceStateID);
    }

    private Device UpdateDeviceState(bool createService = true, bool isCore = true, bool isActiveService = true)
    {
      var owner = Entity.Customer.Account.BssId(IdGen.StringId()).Save();
      var customer = Entity.Customer.EndCustomer.BssId(IdGen.StringId()).Save();
      var rel = Entity.CustomerRelationship.Relate(customer, owner).Save();
      var device = Entity.Device.PL321.IbKey(IdGen.StringId()).OwnerBssId(owner.BSSID).GpsDeviceId(IdGen.StringId()).DeviceState(DeviceStateEnum.DeregisteredTechnician).Save();
      var asset = Entity.Asset.WithDevice(device).Save();
      Service service;

      if (createService)
      {
        if (isCore)
        {
          if (isActiveService)
            service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId())
              .WithView(t => t.ForAsset(asset).ForCustomer(customer)).Save();
          else
            service = Entity.Service.Essentials.ForDevice(device).BssPlanLineId(IdGen.StringId()).CancellationDate(DateTime.UtcNow.AddDays(-20))
              .WithView(t => t.ForAsset(asset).ForCustomer(customer).StartsOn(DateTime.UtcNow.AddDays(-20)).EndsOn(DateTime.UtcNow.AddDays(-10))).Save();
        }
        else
        {
          if (isActiveService)
            service = Entity.Service.Health.ForDevice(device).BssPlanLineId(IdGen.StringId())
              .WithView(t => t.ForAsset(asset).ForCustomer(customer)).Save();
          else
            service = Entity.Service.Health.ForDevice(device).BssPlanLineId(IdGen.StringId()).CancellationDate(DateTime.UtcNow.AddDays(-20))
              .WithView(t => t.ForAsset(asset).ForCustomer(customer).StartsOn(DateTime.UtcNow.AddDays(-20)).EndsOn(DateTime.UtcNow.AddDays(-10))).Save();
        }
      }

      new BssDeviceService().RegisterDevice(device.ID);

      return Ctx.OpContext.DeviceReadOnly.Where(t => t.ID == device.ID).Single();
    }

    #endregion
  }
}
