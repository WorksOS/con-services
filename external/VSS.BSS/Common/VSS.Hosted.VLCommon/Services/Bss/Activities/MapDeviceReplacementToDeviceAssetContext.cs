using System;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon
{
  public class MapDeviceReplacementToDeviceAssetContext : Activity
  {
    public override ActivityResult Execute(Inputs inputs)
    {
      var message = inputs.Get<DeviceReplacement>();
      var context = inputs.GetOrNew<DeviceAssetContext>();

      AddSummary("Mapping message to Context");

      MapMessageToDeviceAssetContext(message, context);

      context.OldDeviceAsset = MapExistingDeviceToDeviceAssetContext(message.OldIBKey);
      context.NewDeviceAsset = MapExistingDeviceToDeviceAssetContext(message.NewIBKey);

      return Success();
    }

    private void MapMessageToDeviceAssetContext(DeviceReplacement message, DeviceAssetContext context)
    {
      AddSummary("Mapping message to DeviceAssetContext");

      context.OldIBKey = message.OldIBKey;
      AddSummary("OldIBKey: {0}", context.OldIBKey);

      context.NewIBKey = message.NewIBKey;
      AddSummary("NewIBKey: {0}", context.NewIBKey);

      context.ActionUTC = Convert.ToDateTime(message.ActionUTC);
      AddSummary("ActionUTC: {0}", context.ActionUTC);

      context.SequenceNumber = message.SequenceNumber;
      AddSummary("SequenceNumber: {0}", context.OldIBKey);
    }

    private DeviceAssetDto MapExistingDeviceToDeviceAssetContext(string ibKey)
    {
      var existingDeviceDto = Bss.Services.Devices().GetDeviceByIbKey(ibKey);

      if (existingDeviceDto == null || !existingDeviceDto.Exists)
      {
        AddSummary("Device not found for IBKey: {0}", ibKey);
        return new DeviceAssetDto();
      }

      AddSummary("Found device for IBKey: {0}.", ibKey);

      var deviceAsset = GetDeviceAssetDto(existingDeviceDto);

      AddSummary(deviceAsset.PropertiesAndValues().ToNewLineTabbedString());

      return deviceAsset;
    }

    private DeviceAssetDto GetDeviceAssetDto(ExistingDeviceDto deviceDto)
    {
      return new DeviceAssetDto
      {
        AssetId = deviceDto.AssetId,
        Name = deviceDto.Asset.Name,
        DeviceId = deviceDto.Id,
        GpsDeviceId = deviceDto.GpsDeviceId,
        IbKey = deviceDto.IbKey,
        Type = deviceDto.Type,
        OwnerBSSID = deviceDto.OwnerBssId,
        DeviceState = deviceDto.DeviceState,
        InsertUTC = deviceDto.Asset.InsertUtc
      };
    }

  }
}
