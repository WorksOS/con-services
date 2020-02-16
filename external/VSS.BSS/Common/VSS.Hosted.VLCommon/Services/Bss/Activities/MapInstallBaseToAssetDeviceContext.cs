using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss.Schema.V2;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public class MapInstallBaseToAssetDeviceContext : Activity
  {
    public override ActivityResult Execute(Inputs inputs)
    {
      var message = inputs.Get<InstallBase>();
      var context = inputs.GetOrNew<AssetDeviceContext>();

      context.TransferDate = DateTime.Parse(message.ActionUTC);
      context.ImpliedAction = GetImpliedAction(message);

      AddSummary("Mapping message to AssetDeviceContext");

      MapMessageToIBDeviceContext(message, context);

      MapMessageToIBAssetContext(message, context);

      MapExistingDeviceToDeviceContext(message.IBKey, context);

      MapExistingAssetToAssetContext(message.EquipmentSN, message.MakeCode, context);

      MapExistingOwnerToOwnerContext(message.OwnerBSSID, context);

      return Success();
    }

    private BssImpliedAction GetImpliedAction(InstallBase message)
    {
      // Only applies for UPDATED action
      if (message.Action != ActionEnum.Updated.ToString())
        return BssImpliedAction.NotApplicable;

      // Does not apply when DeviceState and PreviousDeviceState are defined
      if (message.DeviceState.IsNotDefined() || message.PreviousDeviceState.IsNotDefined())
        return BssImpliedAction.NotApplicable;

      // Does not apply if the device is on the same asset
      if (message.EquipmentSN.IsStringEqual(message.PreviousEquipmentSN) &&
         message.MakeCode.IsStringEqual(message.PreviousMakeCode))
        return BssImpliedAction.NotApplicable;

      if (message.PreviousDeviceState.IsStringEqual("NOTACTIVE") && message.DeviceState.IsStringEqual("ACTIVE"))
        return BssImpliedAction.DeviceReplacement;

      return BssImpliedAction.DeviceTransfer;
    }

    #region Mapping Methods

    private void MapMessageToIBDeviceContext(InstallBase message, AssetDeviceContext context)
    {
      AddSummary("Mapped message to IBDevice");
      context.IBDevice.IbKey = message.IBKey;
      context.IBDevice.GpsDeviceId = message.GPSDeviceID;
      context.IBDevice.PartNumber = message.PartNumber;
      context.IBDevice.OwnerBssId = message.OwnerBSSID;
      context.IBDevice.FirmwareVersionId = message.FirmwareVersionID;
      context.IBDevice.SIMSerialNumber = message.SIMSerialNumber;
      context.IBDevice.CellularModemIMEA = message.CellularModemIMEA;

      DeviceTypeEnum? deviceType = Services.Devices().GetDeviceTypeByPartNumber(message.PartNumber);

      if (deviceType == null)
      {
        AddSummary("DeviceType not found for PartNumber: {0}", message.PartNumber);
      }
      else
      {
        context.IBDevice.Type = deviceType.Value;
        AddSummary("DeviceType: {0} found for PartNumber {1}", deviceType.Value, message.PartNumber);
      }

      AddSummary(context.IBDevice.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapMessageToIBAssetContext(InstallBase message, AssetDeviceContext context)
    {
      AddSummary("Mapped message to IBAsset.");
      context.IBAsset.Name = message.EquipmentLabel;
      context.IBAsset.SerialNumber = message.EquipmentSN;
      context.IBAsset.AssetVinSN = (string.IsNullOrWhiteSpace(message.EquipmentVIN)) ? null : message.EquipmentVIN;
      context.IBAsset.MakeCode = message.MakeCode;
      context.IBAsset.Model = message.Model;
      if (message.ModelYear.IsDefined())
        context.IBAsset.ManufactureYear = int.Parse(message.ModelYear);

      //string productFamily = string.Empty;
      //string salesModel = string.Empty;

      //Services.Assets().GetAssetModelInformation(message.Model, message.EquipmentSN, message.MakeCode, ref productFamily, ref salesModel);

      //context.IBAsset.Model = salesModel;
      //context.IBAsset.ProductFamily = productFamily;

      AddSummary(context.IBAsset.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapExistingDeviceToDeviceContext(string ibKey, AssetDeviceContext context)
    {
      ExistingDeviceDto device = Services.Devices().GetDeviceByIbKey(ibKey);

      if (device == null || !device.Exists)
      {
        AddSummary("Device not found for IBKey: {0}", ibKey);
        return;
      }

      AddSummary("Found device for IBKey: {0}.", ibKey);    
      context.Device = device;

      if (context.Device.AssetExists)
        AddSummary("Device is currently installed on an asset.");
      else
        AddSummary("Device is not installed on an asset.");

      if (context.Device.OwnerExists)
        AddSummary("Found device's owner.");
      else
        AddSummary("Device's owner not found.");

      AddSummary(context.Device.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapExistingAssetToAssetContext(string serialNumber, string makeCode, AssetDeviceContext context)
    {
      //There is a bug with Asset.ComputeAssetId() method that does not take case sensitivity into account.
      //Rewiring this to lookup an asset directly using SerialNumber+MakeCode
      ExistingAssetDto asset = Services.Assets().GetAssetBySerialNumberMakeCode(serialNumber, makeCode);

      if (asset == null || !asset.Exists)
      {
        AddSummary("Asset not found for SerialNumber: {0} and MakeCode: {1}.", serialNumber, makeCode);
        return;
      }

      AddSummary("Asset found for SerialNumber: {0} and MakeCode: {1} to Asset context.", serialNumber, makeCode);
      context.Asset = asset;

      if (context.Asset.DeviceExists)
      {
        AddSummary("Found Asset's currently installed device.");
        
        if (context.Asset.DeviceOwnerExists)
          AddSummary("Found Owner of Asset's currently installed device.");
        else
          AddSummary("Asset's installed device owner not found.");
      }
      else
        AddSummary("Asset does not have a device installed.");

      

      AddSummary(context.Asset.PropertiesAndValues().ToNewLineTabbedString());
    }

    private void MapExistingOwnerToOwnerContext(string ownerBssid, AssetDeviceContext context)
    {
      var owner = Services.Customers().GetCustomerByBssId(ownerBssid);

      if(owner == null)
      {
        AddSummary("Owner not found for BssId: {0}.", ownerBssid);
        return;
      }

      AddSummary("Owner found for BssId: {0}.", ownerBssid);
      context.Owner.Id = owner.ID;
      context.Owner.MapOwner(owner);
        
      if(context.Owner.Type == CustomerTypeEnum.Dealer)
      {
        context.Owner.RegisteredDealerId = owner.ID;
        context.Owner.RegisteredDealerNetwork = (DealerNetworkEnum) owner.fk_DealerNetworkID;
      }
      else
      {
        var registeredDealer = Services.Customers().GetParentDealerByChildCustomerId(context.Owner.Id);

        if(registeredDealer != null && registeredDealer.Item1 != null)
        {
          context.Owner.RegisteredDealerId = registeredDealer.Item1.ID;
          context.Owner.RegisteredDealerNetwork = (DealerNetworkEnum)registeredDealer.Item1.fk_DealerNetworkID;
        }
      }

      AddSummary(context.Owner.PropertiesAndValues().ToNewLineTabbedString());
    }

    #endregion
  }
}