using System;
using System.Collections.Generic;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  public class BssDeviceServiceFake : IBssDeviceService
  {
    private readonly bool _booleanToReturn;
    private readonly ExistingDeviceDto _existingDeviceToReturn;
    private readonly List<DevicePersonality> _personalitiesToReturn;
    private readonly DeviceTypeEnum? _deviceTypeToReturn;
    private readonly Device _deviceToReturn;
    private readonly AssetDeviceHistory _assetDeviceHistory;

    public bool WasExecuted { get; set; }
    public long DeviceIdArg { get; set; }
    public string OwnerBssIdArg { get; set; }

    #region CTOR

    public BssDeviceServiceFake() { }

    public BssDeviceServiceFake(DeviceTypeEnum? deviceTypeToReturn)
    {
      _deviceTypeToReturn = deviceTypeToReturn;
    }

    public BssDeviceServiceFake(Device deviceToReturn)
    {
      _deviceToReturn = deviceToReturn;
    }

    public BssDeviceServiceFake(ExistingDeviceDto existingDeviceToReturn)
    {
      _existingDeviceToReturn = existingDeviceToReturn;
    }

    public BssDeviceServiceFake(List<DevicePersonality> personalitiesToReturn)
    {
      _personalitiesToReturn = personalitiesToReturn;
    }

    public BssDeviceServiceFake(AssetDeviceHistory assetDeviceHistory)
    {
      _assetDeviceHistory = assetDeviceHistory;
    }

    public BssDeviceServiceFake(bool booleanToReturn)
    {
      _booleanToReturn = booleanToReturn;
    }

    #endregion

    public ExistingDeviceDto GetDeviceByIbKey(string ibKey)
    {
      WasExecuted = true;
      return _existingDeviceToReturn;
    }

    public Device GetDeviceByGpsDeviceId(string gpsDeviceId, DeviceTypeEnum? type)
    {
      WasExecuted = true;
      return _deviceToReturn;
    }

    public DeviceTypeEnum? GetDeviceTypeByPartNumber(string partNumber)
    {
      WasExecuted = true;
      return _deviceTypeToReturn;
    }

    public Device CreateDevice(DeviceDto ibDevice)
    {
      WasExecuted = true;
      return _deviceToReturn;
    }

    public IList<DevicePersonality> CreateDevicePersonality(VSS.Hosted.VLCommon.Bss.AssetDeviceContext contextTests)
    {
      WasExecuted = true;
      return _personalitiesToReturn;
    }

    public bool TransferOwnership(long deviceId, string newOwnerBssId)
    {
      DeviceIdArg = deviceId;
      OwnerBssIdArg = newOwnerBssId;
      WasExecuted = true;
      return _booleanToReturn;
    }

    public bool IsDeviceReadOnly(DeviceTypeEnum deviceType, string ibKey)
    {
      WasExecuted = true;
      return _booleanToReturn;
    }

    public void RegisterDevice(long deviceID)
    {
      WasExecuted = true;
    }

    public void UpdateDeviceState(long deviceId, DeviceStateEnum deviceState)
    {
      WasExecuted = true;
    }
    public void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC)
    {
      WasExecuted = true;
    }


    public void ReconfigureDevice(long oldDeviceID, string oldGPSDeviceID, DeviceTypeEnum oldDeviceType, long newDeviceID, string newGPSDeviceID, DeviceTypeEnum newDeviceType, DateTime actionUtc)
    {
      WasExecuted = true;
    }

    public void UpdateDeviceOwnerBssIds(string oldBssId, string newBssId)
    {
      throw new NotImplementedException();
    }
  }
}
