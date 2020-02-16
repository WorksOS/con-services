using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssDeviceServiceExceptionFake : IBssDeviceService
  {
    public bool WasExecuted { get; set; }

    public ExistingDeviceDto GetDeviceByIbKey(string ibKey)
    {
      throw new NotImplementedException();
    }

    public Device GetDeviceByGpsDeviceId(string gpsDeviceId, DeviceTypeEnum? type)
    {
      throw new NotImplementedException();
    }

    public DeviceTypeEnum? GetDeviceTypeByPartNumber(string partNumber)
    {
      throw new NotImplementedException();
    }
    
    public Device CreateDevice(DeviceDto ibDevice)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public IList<DevicePersonality> CreateDevicePersonality(VSS.Hosted.VLCommon.Bss.AssetDeviceContext context)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool TransferOwnership(long deviceId, string newOwnerBssId)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void RegisterDevice(long deviceID)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool IsDeviceReadOnly(DeviceTypeEnum deviceType, string ibKey)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void UpdateDeviceState(long deviceId, DeviceStateEnum deviceState)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool RegisterDevice(long deviceID, string gpsDeviceID, DeviceTypeEnum deviceType, DeviceStateEnum deviceState)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public bool RegisterDevice(long deviceID, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }


    public void ReconfigureDevice(long oldDeviceID, string oldGPSDeviceID, DeviceTypeEnum oldDeviceType, long newDeviceID, string newGPSDeviceID, DeviceTypeEnum newDeviceType, DateTime actionUtc)
    {
      WasExecuted = true;
      throw new NotImplementedException();
    }

    public void UpdateDeviceOwnerBssIds(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }
  }
}