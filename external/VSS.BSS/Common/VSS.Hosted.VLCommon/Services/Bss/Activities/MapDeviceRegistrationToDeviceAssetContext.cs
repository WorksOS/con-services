using System;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace VSS.Hosted.VLCommon
{
  public class MapDeviceRegistrationToDeviceAssetContext : Activity
  {
    public override ActivityResult Execute(Inputs inputs)
    {
      var message = inputs.Get<DeviceRegistration>();
      var context = inputs.GetOrNew<DeviceStatusContext>();

      AddSummary("Mapping message to Context");

      MapMessageToDeviceServiceContext(message, context);

      context.DeviceAsset = MapExistingDeviceToDeviceStatusContext(message.IBKey);

      return Success();
    }

    private void MapMessageToDeviceServiceContext(DeviceRegistration message, DeviceStatusContext context)
    {
      AddSummary("Mapping message to DeviceAssetContext");

      context.IBKey = message.IBKey;
      AddSummary("IBKey: {0}", context.IBKey);

      context.ActionUTC = Convert.ToDateTime(message.ActionUTC);
      AddSummary("ActionUTC: {0}", context.ActionUTC);

      context.Status = message.Status;
      AddSummary("Status: {0}", context.Status);
    }

    private DeviceAssetDto MapExistingDeviceToDeviceStatusContext(string ibKey)
    {
      var existingDeviceDto = Bss.Services.Devices().GetDeviceByIbKey(ibKey);

      if (existingDeviceDto == null || !existingDeviceDto.Exists)
      {
        AddSummary("Device not found for IBKey: {0}", ibKey);
        return new DeviceAssetDto();
      }

      AddSummary("Found device for IBKey: {0}.", ibKey);

      var deviceAsset = new DeviceAssetDto
      {
        AssetId = existingDeviceDto.AssetId,
        Name = existingDeviceDto.Asset.Name,
        DeviceId = existingDeviceDto.Id,
        GpsDeviceId = existingDeviceDto.GpsDeviceId,
        IbKey = existingDeviceDto.IbKey,
        Type = existingDeviceDto.Type,
        OwnerBSSID = string.IsNullOrWhiteSpace(existingDeviceDto.Owner.BssId) ? string.Empty : existingDeviceDto.Owner.BssId,
        InsertUTC = existingDeviceDto.Asset.InsertUtc,
        DeviceState = existingDeviceDto.DeviceState
      };

      AddSummary(deviceAsset.PropertiesAndValues().ToNewLineTabbedString());

      return deviceAsset;
    }
  }
}
