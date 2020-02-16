using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  [DataContract]
  public enum ConfigTypeIDEnum
  {
    [EnumMember]
    SiteDispatch = 0,
    [EnumMember]
    SitePurge = 1,
    [EnumMember]
    ConfigureSensors = 2,
    [EnumMember]
    SpeedingThreshold = 3,
    [EnumMember]
    StoppedThreshold = 4,
    [EnumMember]
    RuntimeMileage = 5,
    [EnumMember]
    TextMessage = 6,
    [EnumMember]
    ZoneLogic = 7,
    [EnumMember]
    GeneralDeviceConfig = 8,
    [EnumMember]
    PrimaryIPAddressConfig = 9,
    [EnumMember]
    MovingConfig = 10,
    [EnumMember]
    HomeSitePositionReportingConfig = 11,
    [EnumMember]
    DevicePortConfig = 12,
    [EnumMember]
    IgnitionEventsEnabled = 13,
    [EnumMember]
    DailyReportConfig = 14,
    [EnumMember]
    NetworkInterfaceConfig = 15
  }
}
