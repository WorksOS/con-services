using System;
using System.Collections.Generic;
using System.ServiceModel;

using VSS.Hosted.VLCommon.ServiceContracts;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  [ServiceContract(Namespace = ContractConstants.NHOPNS)]
  public interface IConfigStatus
  {
    [OperationContract(IsOneWay = true)]
    void UpdateFirmwareStatus(string gpsDeviceID, DeviceTypeEnum type, FirmwareUpdateStatusEnum status);

    [OperationContract(IsOneWay = true)]
    void UpdatePersonality(string gpsDeviceID, DeviceTypeEnum type, string firmwareVersions);

    void UpdatePersonality(INHOPDataObject message);

    void UpdateECMInfoThroughDataIn(string gpsDeviceID, DeviceTypeEnum deviceType, List<MTSEcmInfo> ecmInfoList, DatalinkEnum dataLinkEnum, DateTime? timestampUtc);    

    [OperationContract(IsOneWay = true)]
    void UpdatePLDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, MessageStatusEnum status, List<PLConfigData.PLConfigBase> configData);

    [OperationContract(IsOneWay = true)]
    void UpdateECMInfo(string gpsDeviceID, DeviceTypeEnum type, List<MTSEcmInfo> ecmInfo);

    [OperationContract(IsOneWay = true)]
    void ProcessAddressClaim(string ecmID, bool arbitraryAddressCapable, byte industryGroup, byte vehicleSystemInstance,
      byte vehicleSystem, byte function, byte functionInstance, byte ecuInstance, ushort manufacturerCode, int identityNumber);

    [OperationContract(IsOneWay = true)]
    void ProcessPLGlobalGramEnabledFieldAndSatelliteNumberChange(List<GlobalGramSatelliteNumber> globalGramSatelliteNumbers);

    [OperationContract(IsOneWay = true)]
    void UpdateDeviceConfiguration(string gpsDeviceID, DeviceTypeEnum deviceType, DeviceConfigBase config);

    [OperationContract(IsOneWay = true)]
    void UpdatePLConfigurationBulk(List<PLDeviceDetailsConfigInfo> configData);
  }
}
