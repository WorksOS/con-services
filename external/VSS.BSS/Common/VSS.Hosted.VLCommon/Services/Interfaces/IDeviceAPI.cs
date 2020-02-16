using System;
using System.Collections.Generic;
using ED= VSS.Nighthawk.ExternalDataTypes.Enumerations;
namespace VSS.Hosted.VLCommon
{
  public interface IDeviceAPI
  {
    Device CreateDevice(INH_OP opCtx, string ibKey, string ownerBSSID, string gpsDeviceID, DeviceTypeEnum deviceType,
                        TimeSpan? samplingInterval, TimeSpan? reportingInterval, TimeSpan? lowPowerInterval, TimeSpan? bitReportInterval, bool isReadOnly);

    Device CreateDevice(INH_OP opCtx, string ibKey, string ownerBSSID, string gpsDeviceID, DeviceTypeEnum deviceType);

    bool ActivateDevice(INH_OP opCtx, string ibKey, DeviceStateEnum deviceState);
    bool DeActivateDevice(INH_OP opCtx, string ibKey, DeviceStateEnum deviceState);

    bool CreateTTDevice(INH_OP opCtx, string unitId, string imei, DeviceStateEnum deviceState, out TTDevice dev);
    void UpdateTTDeviceState(INH_OP opCtx, string gpsDeviceID, DeviceStateEnum deviceState);
    string IMEI2UnitID(string IMEI);

    MTSDevice CreateMTSDevice(INH_OP opCtx, string gpsDeviceID, TimeSpan bitPacketInterval, TimeSpan lowPowerInterval,
                              TimeSpan samplingInterval, TimeSpan reportingInterval, DeviceTypeEnum deviceType);
    void UpdateOpDeviceState(INH_OP opCtx, string gpsDeviceID, DeviceStateEnum deviceState, int deviceType);

    PLDevice CreatePLDevice(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType, bool isReadOnly);
    bool IsProductLinkDevice(DeviceTypeEnum deviceType);

    IList<DevicePersonality> CreateDevicePersonality(INH_OP opContext, long deviceID, string firmwareVersionID, string simSerialNumber, 
          string partNumber, string cellularModemIMEA, string gpsDeviceID, DeviceTypeEnum deviceType);

    void UpdateDeviceState(long deviceId, DeviceStateEnum deviceState, INH_OP nhOpCtx = null);

    void UpdateDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config, ObjectContextTransactionParams<Action> tansactionParams = null, INH_OP nhOpCtx = null);
 
    void DeregisterDeviceState(long deviceId, DeviceStateEnum deviceState, DateTime deregesteredUTC);

    bool UpdateDailyReportIsUserCustomized(INH_OP ctx, string gpsDeviceID, bool isUserCustomized,
                                           DateTime? dailyReportUTC, int deviceType);

    string GetDeviceTypeDescription(int deviceTypeID, string locale);

    bool UpdateOwnerBSSID(long deviceId, Guid organizationIdentifier, INH_OP nhOpCtx);

    bool CancelOwnerBSSID(long deviceId, Guid organizationIdentifier, INH_OP nhOpCtx);
  }
}
