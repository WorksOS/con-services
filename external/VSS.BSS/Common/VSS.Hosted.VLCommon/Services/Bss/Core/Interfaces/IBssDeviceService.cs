using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.Bss
{
  public interface IBssDeviceService  
  {
    ExistingDeviceDto GetDeviceByIbKey(string ibKey);
    Device GetDeviceByGpsDeviceId(string gpsDeviceId, DeviceTypeEnum? type);
    DeviceTypeEnum? GetDeviceTypeByPartNumber(string partNumber);

    Device CreateDevice(DeviceDto ibDevice);

    IList<DevicePersonality> CreateDevicePersonality(AssetDeviceContext context);

    bool TransferOwnership(long deviceId, string newOwnerBssId);

    void RegisterDevice(long deviceID);
    bool IsDeviceReadOnly(DeviceTypeEnum deviceType, string ibKey);
    void UpdateDeviceState(long deviceId, DeviceStateEnum deviceState);
    void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC);
    void ReconfigureDevice(long oldDeviceID, string oldGPSDeviceID, DeviceTypeEnum oldDeviceType, long newDeviceID, string newGPSDeviceID, DeviceTypeEnum newDeviceType, DateTime actionUtc);
    void UpdateDeviceOwnerBssIds(string oldBssId, string newBssId);
  }
}
