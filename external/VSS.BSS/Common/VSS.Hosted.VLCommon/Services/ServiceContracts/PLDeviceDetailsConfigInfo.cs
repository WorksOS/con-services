using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.ServiceContracts
{
  [DataContract]
  public class PLDeviceDetailsConfigInfo
  {
    // Basic identification
    [DataMember]
    public string ModuleCode;
    [DataMember]
    public DeviceTypeEnum DeviceType;

    // Globalgram and Satellite information, if available
    [DataMember]
    public bool IsGlobalGramSet = false;
    [DataMember]
    public bool? GlobalGramEnabled;
    [DataMember]
    public int? SatelliteNumber;

    // ecmInfo
    [DataMember]
    public bool IsEcmListSet = false;
    [DataMember]
    public List<MTSEcmInfo> EcmList;

    // Firmware
    [DataMember]
    public bool IsFirmwareVersionsSet = false;
    [DataMember]
    public string FirmwareVersions;

    //Device config
    [DataMember]
    public bool IsConfigDataSet = false;
    [DataMember]
    public MessageStatusEnum Status;
    [DataMember]
    public List<PLConfigData.PLConfigBase> ConfigData;
  }

}
