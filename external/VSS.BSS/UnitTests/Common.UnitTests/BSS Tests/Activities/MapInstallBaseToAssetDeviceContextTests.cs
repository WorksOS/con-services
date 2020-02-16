using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class MapInstallBaseToAssetDeviceContextTests : BssUnitTestBase
  {
    private Inputs Inputs;
    private MapInstallBaseToAssetDeviceContext Activity;

    [TestInitialize]
    public void MapInstallBaseToAssetDeviceContextTests_Init()
    {
      Inputs = new Inputs();
      Activity = new MapInstallBaseToAssetDeviceContext();
    }

    [TestMethod]
    public void Execute_MessageMappedToIBDevice()
    {
      var serviceFake = new BssDeviceServiceFake(DeviceTypeEnum.Series521);
      Services.Devices = () => serviceFake;

      var message = BSS.IBCreated.Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(message.IBKey, context.IBDevice.IbKey, "IbKey not equal");
      Assert.AreEqual(message.GPSDeviceID, context.IBDevice.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(message.PartNumber, context.IBDevice.PartNumber, "PartNumber not equal");
      Assert.AreEqual(message.OwnerBSSID, context.IBDevice.OwnerBssId, "BssId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.IBDevice.Type, "DeviceType not equal");
    }

    [TestMethod]
    public void Execute_ActionIsCreated_ImpliedActionIsNotApplicable()
    {
      var message = BSS.IBCreated.Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.NotApplicable, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_ActionIsUpdated_SameAsset_ImpliedActionIsNotApplicable()
    {
      var message = BSS.IBUpdated.Build();
      message.PreviousEquipmentSN = message.EquipmentSN;
      message.PreviousMakeCode = message.MakeCode;

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.NotApplicable, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_ActionIsUpdated_DeviceStateNotDefined_ImpliedActionIsNotApplicable()
    {
      var message = BSS.IBUpdated.ImplyDeviceTransfer().Build();
      message.DeviceState = null;

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.NotApplicable, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_ActionIsUpdated_PreviousDeviceStateNotDefined_ImpliedActionIsNotApplicable()
    {
      var message = BSS.IBUpdated.ImplyDeviceTransfer().Build();
      message.PreviousDeviceState = null;

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.NotApplicable, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_DeviceStateActivePreviousNotActive_DifferentAsset_ImpliedActionIsDeviceReplacement()
    {
      var message = BSS.IBUpdated.ImplyDeviceReplacement().Build();

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.DeviceReplacement, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_DeviceStateNotActivePreviousNotActive_DifferentAsset_ImpliedActionIsDeviceTransfer()
    {
      var message = BSS.IBUpdated.ImplyDeviceTransfer().Build();

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(BssImpliedAction.DeviceTransfer, context.ImpliedAction);
    }

    [TestMethod]
    public void Execute_DeviceTypeNull_MessageMappedToIBDeviceWithNullDeviceType()
    {
      var serviceFake = new BssDeviceServiceFake((DeviceTypeEnum?)null);
      Services.Devices = () => serviceFake;

      var message = BSS.IBCreated.Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(message.IBKey, context.IBDevice.IbKey, "IbKey not equal");
      Assert.AreEqual(message.GPSDeviceID, context.IBDevice.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(message.PartNumber, context.IBDevice.PartNumber, "PartNumber not equal");
      Assert.AreEqual(message.OwnerBSSID, context.IBDevice.OwnerBssId, "BssId not equal");
      Assert.IsNull(context.IBDevice.Type, "DeviceType is not null");
    }

    [TestMethod]
    public void Execute_MessageMappedToIBAsset()
    {
      string model = "MODEL";
      string productFamily = "PRODUCT_FAMILY";
      var serviceFake = new BssAssetServiceFake(model, productFamily);
      Services.Assets = () => serviceFake;

      var message = BSS.IBCreated.Build();

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(message.EquipmentLabel, context.IBAsset.Name, "Name not equal");
      Assert.AreEqual(message.EquipmentSN, context.IBAsset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(message.MakeCode, context.IBAsset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(int.Parse(message.ModelYear), context.IBAsset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(message.EquipmentVIN, context.IBAsset.AssetVinSN, "AssetVinSN is not equal");
    }

    [TestMethod]
    public void Execute_MessageMappedToIBAsset_SerialNumberHasTrailingSpace()
    {
      string model = "MODEL";
      string productFamily = "PRODUCT_FAMILY";
      var serviceFake = new BssAssetServiceFake(model, productFamily);
      Services.Assets = () => serviceFake;

      var message = BSS.IBCreated.Build();
      message.EquipmentSN += " ";

      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.AreEqual(message.EquipmentLabel, context.IBAsset.Name, "Name not equal");
      Assert.AreEqual(message.EquipmentSN.Trim(), context.IBAsset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(message.MakeCode, context.IBAsset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(int.Parse(message.ModelYear), context.IBAsset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(message.EquipmentVIN, context.IBAsset.AssetVinSN, "AssetVinSN is not equal");
    }

    //[DatabaseTest]
    [TestMethod]
    public void Execute_DeviceAndInstalledAssetAndOwnerExistsForIBKey_MappedToDeviceContext()
    {
      var owner = Entity.Customer.Dealer.Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.IBCreated.IBKey(device.IBKey).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Device.Exists, "Device does not exist");
      Assert.AreEqual(device.ID, context.Device.Id, "Id not equal");
      Assert.AreEqual(device.GpsDeviceID, context.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.Device.Type);
      Assert.AreEqual(device.OwnerBSSID, context.Device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsTrue(context.Device.OwnerExists, "Owner does not exist");
      Assert.AreEqual(owner.ID, context.Device.OwnerId, "OwnerId not equal");
      Assert.AreEqual(owner.BSSID, context.Device.Owner.BssId, "Owner BssId not equal");
      Assert.AreEqual(owner.Name, context.Device.Owner.Name, "Owner name not equal");
      Assert.AreEqual(owner.fk_CustomerTypeID, (int)context.Device.Owner.Type, "Owner type not equal");

      Assert.IsTrue(context.Device.AssetExists, "Asset does not exist");
      Assert.AreEqual(asset.Name, context.Device.Asset.Name, "Name not equal");
      Assert.AreEqual(asset.SerialNumberVIN, context.Device.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(asset.fk_MakeCode, context.Device.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(asset.Model, context.Device.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(asset.ManufactureYear, context.Device.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(asset.InsertUTC.ToString(), context.Device.Asset.InsertUtc.ToString(), "InsertUTC is not equal");
      Assert.AreEqual(asset.EquipmentVIN, context.Device.Asset.AssetVinSN, "Asset VIN Serial Number is not equial");
    }

    //[DatabaseTest]
    [TestMethod]
    public void Execute_InstalledOnAssetDoesNotExistForIBKey_AssetNotMappedToDeviceContext()
    {
      var owner = Entity.Customer.Dealer.Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();

      var message = BSS.IBCreated.IBKey(device.IBKey).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Device.Exists, "Device does not exist");
      Assert.AreEqual(device.ID, context.Device.Id, "Id not equal");
      Assert.AreEqual(device.GpsDeviceID, context.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.Device.Type);
      Assert.AreEqual(device.OwnerBSSID, context.Device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsTrue(context.Device.OwnerExists, "Owner does not exist");
      Assert.AreEqual(owner.ID, context.Device.OwnerId, "OwnerId not equal");
      Assert.AreEqual(owner.BSSID, context.Device.Owner.BssId, "Owner BssId not equal");
      Assert.AreEqual(owner.Name, context.Device.Owner.Name, "Owner name not equal");
      Assert.AreEqual(owner.fk_CustomerTypeID, (int)context.Device.Owner.Type, "Owner type not equal");

      Assert.IsFalse(context.Device.AssetExists, "Asset does exist");
    }

    //[DatabaseTest]
    [TestMethod]
    public void Execute_DeviceOwnerDoesNotExistForIBKey_OwnerNotMappedToDeviceContext()
    {
      var device = Entity.Device.MTS521.OwnerBssId(string.Empty).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.IBCreated.IBKey(device.IBKey).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Device.Exists, "Device does not exist");
      Assert.AreEqual(device.ID, context.Device.Id, "Id not equal");
      Assert.AreEqual(device.GpsDeviceID, context.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.Device.Type);
      Assert.AreEqual(device.OwnerBSSID, context.Device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsFalse(context.Device.OwnerExists, "Owner does not exist");

      Assert.IsTrue(context.Device.AssetExists, "Asset does not exist");
      Assert.AreEqual(asset.Name, context.Device.Asset.Name, "Name not equal");
      Assert.AreEqual(asset.SerialNumberVIN, context.Device.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(asset.fk_MakeCode, context.Device.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(asset.Model, context.Device.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(asset.ManufactureYear, context.Device.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(asset.InsertUTC.ToString(), context.Device.Asset.InsertUtc.ToString(), "InsertUTC is not equal");
      Assert.AreEqual(asset.EquipmentVIN, context.Device.Asset.AssetVinSN, "Asset VIN Serial Number is not equial");
    }

    [TestMethod]
    public void Execute_AssetAndInstalledDeviceAndDeviceOwnerExistsForSerialNumberAndMakeCode_MapToAssetContext()
    {
      var owner = Entity.Customer.Dealer.Save();
      var device = Entity.Device.MTS521.OwnerBssId(owner.BSSID).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.IBCreated.EquipmentSN(asset.SerialNumberVIN).MakeCode(asset.fk_MakeCode).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Asset.Exists, "Asset does not exist");
      Assert.AreEqual(asset.Name, context.Asset.Name, "Name not equal");
      Assert.AreEqual(asset.SerialNumberVIN, context.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(asset.fk_MakeCode, context.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(asset.Model, context.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(asset.ManufactureYear, context.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(asset.InsertUTC.ToString(), context.Asset.InsertUtc.ToString(), "InsertUTC is not equal");

      Assert.IsTrue(context.Asset.DeviceExists, "Device does not exist");
      Assert.AreEqual(device.ID, context.Asset.DeviceId, "Id not equal");
      Assert.AreEqual(device.GpsDeviceID, context.Asset.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.Asset.Device.Type);
      Assert.AreEqual(device.OwnerBSSID, context.Asset.Device.OwnerBssId, "OwnerBssId not equal.");
      Assert.AreEqual(asset.EquipmentVIN, context.Asset.AssetVinSN, "Asset VIN Serial Number is not equial");

      Assert.IsTrue(context.Asset.DeviceOwnerExists, "Owner does not exist");
      Assert.AreEqual(owner.ID, context.Asset.DeviceOwnerId, "OwnerId not equal");
      Assert.AreEqual(owner.BSSID, context.Asset.DeviceOwner.BssId, "Owner BssId not equal");
      Assert.AreEqual(owner.Name, context.Asset.DeviceOwner.Name, "Owner name not equal");
      Assert.AreEqual(owner.fk_CustomerTypeID, (int)context.Asset.DeviceOwner.Type, "Owner type not equal");
    }

    [TestMethod]
    public void Execute_InstalledDeviceDoesNotExistsForSerialNumberAndMakeCode_MapToAssetContext()
    {
      var asset = Entity.Asset.Save();

      var message = BSS.IBCreated.EquipmentSN(asset.SerialNumberVIN).MakeCode(asset.fk_MakeCode).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Asset.Exists, "Asset does not exist");
      Assert.AreEqual(asset.Name, context.Asset.Name, "Name not equal");
      Assert.AreEqual(asset.SerialNumberVIN, context.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(asset.fk_MakeCode, context.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(asset.Model, context.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(asset.ManufactureYear, context.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(asset.InsertUTC.ToString(), context.Asset.InsertUtc.ToString(), "InsertUTC is not equal");
      Assert.AreEqual(asset.EquipmentVIN, context.Asset.AssetVinSN, "Asset VIN Serial Number is not equial");

      Assert.IsFalse(context.Asset.DeviceExists, "Device exists");

      Assert.IsFalse(context.Asset.DeviceOwnerExists, "Owner exists");
    }

    [TestMethod]
    public void Execute_DeviceOwnerDoesNotExistForSerialNumberAndMakeCode_MapToAssetContext()
    {
      var device = Entity.Device.MTS521.OwnerBssId(string.Empty).Save();
      var asset = Entity.Asset.WithDevice(device).Save();

      var message = BSS.IBCreated.EquipmentSN(asset.SerialNumberVIN).MakeCode(asset.fk_MakeCode).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Asset.Exists, "Asset does not exist");
      Assert.AreEqual(asset.Name, context.Asset.Name, "Name not equal");
      Assert.AreEqual(asset.SerialNumberVIN, context.Asset.SerialNumber, "SerialNumber not equal");
      Assert.AreEqual(asset.fk_MakeCode, context.Asset.MakeCode, "MakeCode not equal");
      Assert.AreEqual(asset.Model, context.Asset.Model, "Mdoel not equal");
      Assert.AreEqual(asset.ManufactureYear, context.Asset.ManufactureYear, "ManufactureYear not equal");
      Assert.AreEqual(asset.InsertUTC.ToString(), context.Asset.InsertUtc.ToString(), "InsertUTC is not equal");
      Assert.AreEqual(asset.EquipmentVIN, context.Asset.AssetVinSN, "Asset VIN Serial Number is not equial");

      Assert.IsTrue(context.Asset.DeviceExists, "Device does not exist");
      Assert.AreEqual(device.ID, context.Asset.DeviceId, "Id not equal");
      Assert.AreEqual(device.GpsDeviceID, context.Asset.Device.GpsDeviceId, "GpsDeviceId not equal");
      Assert.AreEqual(DeviceTypeEnum.Series521, context.Asset.Device.Type);
      Assert.AreEqual(device.OwnerBSSID, context.Asset.Device.OwnerBssId, "OwnerBssId not equal.");

      Assert.IsFalse(context.Asset.DeviceOwnerExists, "Owner exists");
    }

    [TestMethod]
    public void Execute_OwnerExistsForOwnerBssId_MapToOwnerContext()
    {
      var owner = Entity.Customer.Dealer.Save();

      var message = BSS.IBCreated.OwnerBssId(owner.BSSID).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsTrue(context.Owner.Exists, "Owner does not exist");
      Assert.AreEqual(owner.ID, context.Owner.Id, "Id not equal");
      Assert.AreEqual(owner.Name, context.Owner.Name, "Name not equal");
      Assert.AreEqual(owner.BSSID, context.Owner.BssId, "BssId not equal");
      Assert.AreEqual(owner.fk_CustomerTypeID, (int)context.Owner.Type, "Type not equal");
      Assert.AreEqual(owner.ID, context.Owner.RegisteredDealerId, "RegisteredDealerId not equal");
      Assert.AreEqual(owner.fk_DealerNetworkID, (int)context.Owner.RegisteredDealerNetwork, "RegisteredDealerNetwork not equal");
    }

    [TestMethod]
    public void Execute_OwnerDoesNotExistForOwnerBssId_NotMappedToOwnerContext()
    {
      var message = BSS.IBCreated.OwnerBssId(IdGen.GetId().ToString()).Build();
      Inputs.Add<InstallBase>(message);

      Activity.Execute(Inputs);

      var context = Inputs.Get<AssetDeviceContext>();

      Assert.IsFalse(context.Owner.Exists, "Owner exists");
    }
  }
}
