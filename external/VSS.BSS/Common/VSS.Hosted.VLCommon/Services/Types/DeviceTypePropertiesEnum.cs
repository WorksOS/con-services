using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace VSS.Hosted.VLCommon
{
  public enum DeviceTypeProperties
  {
    SwitchCount = 0,
    SensorCount = 1,
    TamperSwitch = 2,
    PollTimeOut = 3,
    DailyReportTime = 4,
    FuelMessageType = 5,
    OnRoadThreshold = 6,
    OnRoadDuration = 7,
    OffRoadThreshold = 8,
    OffRoadDuration = 9,
    MinimumReportingFrequency = 10,
    HomeSiteExitSpeed = 11,
    MovingRadius = 12,
    SpeedingThreshold = 13,
    SpeedingDuration = 14,
    StopThreshold = 15,
    StopDuration = 16,
    ConfigDataType = 17,
    ICDSeries = 18,
    HourmeterUpdateRange = 19,
    OdometerUpdateRange = 20,
    Switch1Description = 21,
    Switch2Description = 22,
    Switch3Description = 23,
    AssetSecurityConfigMessageType = 24, 
    LogicalGroup = 25,
    DeviceConfigGroup = 26
  }

  public enum ConfigDataType
  {
      TTConfigData=0,
      MTSConfigData=1,
      A5N2ConfigData=2
  }

  public enum DeviceConfigGroup
  {
    PL=0,
    MTS=1,
    A5N2=2
  }

  public enum DeviceTypeFamily
  {
    UNKNOWN = -1,
    PL = 0,
    MTS = 1,
    A5N2 = 2
  }

  public enum ResetValues
  {
    None = 0,
    GreaterThanZero = 1,
    GreaterThanCurrent = 2
  }

  public enum ICDSeries
  {
    PLInOut = 0,
    MTSInOut = 1,
    TrimTracInOut = 2,
    DataIn = 3,
    DataOut = 4
  }

  public enum AssetSecurityConfigMessageTypeEnum
{     
    GatewaySecurityConfigMessage = 0,
    RadioSecurityConfigMessage = 1,
}

  public static class EnumExtensions
  {
    public static string ToValString(this Enum enumType)
    {
      int enumVal = Convert.ToInt32(enumType);
      return enumVal.ToString();
    }
		
		public static string GetXmlEnumValue(this Enum value)
		{
			// Get the type
			Type type = value.GetType();

			// Get fieldinfo for this type
			FieldInfo fieldInfo = type.GetField(value.ToString());

            if (fieldInfo == null)
            {
                return null;
            }

			// Get the stringvalue attributes
			XmlEnumAttribute[] attribs = fieldInfo.GetCustomAttributes(
					typeof(XmlEnumAttribute), false) as XmlEnumAttribute[];

			// Return the first if there was a match.
			return attribs.Length > 0 ? attribs[0].Name : null;
		}

  }
}
