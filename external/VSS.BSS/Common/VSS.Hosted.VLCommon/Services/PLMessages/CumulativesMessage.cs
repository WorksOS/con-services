using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon;


namespace VSS.Hosted.VLCommon.PLMessages
{
  public class CumulativesMessage : PLTrackerMessage
  {
    public static new readonly int kPacketID = 0x42;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte SubType;
    public byte Version;
    public DateTime Timestamp
    {
      get { return epoch.AddSeconds(timestamp); }
      set { timestamp = (uint)value.Subtract(epoch).TotalSeconds; }
    }
    public double Latitude
    {
      get { return ConversionFactors.ConvertLatGeodesicToDegrees(latitude); }
      set { latitude = ConversionFactors.ConvertLatDegreesToGeodesic((decimal)value); }
    }
    public double Longitude
    {
      get { return ConversionFactors.ConvertLonGeodesicToDegrees(longitude); }
      set { longitude = ConversionFactors.ConvertLonDegreesToGeodesic((decimal)value); }
    }
    public PLLocationTypeEnum LocationType
    {
      get { return (PLLocationTypeEnum)locationType; }
      set { locationType = (byte)value; }
    }
    public int ServiceMeterHours;

    public bool TimeBasedProductWatchOrFencingActive;
    public bool ExclusiveProductWatchOrGeographicFencingActive;
    public bool InclusiveProductWatchOrGeographicFencingActive;
    public bool TimeBasedProductWatchOrFencingAlarm;
    public bool ExclusiveProductWatchOrGeographicFencingAlarm;
    public bool InclusiveProductWatchOrGeographicFencingAlarm;
    public byte InclusiveProductWatchOrGeographicFencingID;
    public byte ExclusiveProductWatchOrGeographicFencingID;
    public byte TimeBasedProductWatchOrFencingID;

    public bool SatelliteBlockage;
    public bool PowerLossDisconnectSwitchUsed;
    public byte PowerMode;

    public bool EventDiagnosticPending;
    public bool ClockManualAlignmentRequiredOrBatteryFailed;
    public bool DigitalSwitch1Active;
    public bool DigitalSwitch2Active;
    public bool DigitalSwitch3Active;
    public bool DigitalSwitch4Active;

    public decimal FuelConsumption
    {
      get { return (decimal)fuelConsumption / 8.0M; }
      set { fuelConsumption = ((uint)(value * 8.0M)); }
    }
    public byte FuelLevelPercentage;
    public double TotalIdleTime
    {
      get { return (double)totalIdleTime / 20.0; }
      set { totalIdleTime = (uint)(value * 20); }
    }
    public decimal TotalMaximumFuel
    {
      get { return (decimal)totalMaximumFuel / 8.0M; }
      set { totalMaximumFuel = ((uint)(value * 8.0M)); }
    }
    public ushort NumberOfEngineStarts;
    public uint TotalEngineRevolutions
    {
      get { return totalEngineRevolutions * 4; }
      set { totalEngineRevolutions = value / 4; }
    }
    public decimal TotalIdleFuel
    {
      get { return (decimal)totalIdleFuel / 8.0M; }
      set { totalIdleFuel = ((uint)(value * 8.0M)); }
    }

    private byte locationType;
    private uint timestamp;
    private uint latitude;
    private uint longitude;
    private uint fuelConsumption;
    private uint totalIdleTime;
    private uint totalMaximumFuel;
    private uint totalIdleFuel;
    private uint totalEngineRevolutions;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubType);
      serializer(action, raw, ref bitPosition, 8, ref Version);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref timestamp);
      BigEndianSerializer(action, raw, ref bitPosition, 3, ref latitude);
      BigEndianSerializer(action, raw, ref bitPosition, 3, ref longitude);
      serializer(action, raw, ref bitPosition, 8, ref locationType);
      BigEndianSerializer(action, raw, ref bitPosition, 3, ref ServiceMeterHours);
      serializer(action, raw, ref bitPosition, 1, ref TimeBasedProductWatchOrFencingActive);
      serializer(action, raw, ref bitPosition, 1, ref ExclusiveProductWatchOrGeographicFencingActive);
      serializer(action, raw, ref bitPosition, 1, ref InclusiveProductWatchOrGeographicFencingActive);
      serializer(action, raw, ref bitPosition, 1, ref TimeBasedProductWatchOrFencingAlarm);
      serializer(action, raw, ref bitPosition, 1, ref ExclusiveProductWatchOrGeographicFencingAlarm);
      serializer(action, raw, ref bitPosition, 1, ref InclusiveProductWatchOrGeographicFencingAlarm);
      serializer(action, raw, ref bitPosition, 1, ref SatelliteBlockage);
      serializer(action, raw, ref bitPosition, 1, ref PowerLossDisconnectSwitchUsed);
      serializer(action, raw, ref bitPosition, 2, ref PowerMode);
      serializer(action, raw, ref bitPosition, 1, ref EventDiagnosticPending);
      serializer(action, raw, ref bitPosition, 1, ref ClockManualAlignmentRequiredOrBatteryFailed);
      serializer(action, raw, ref bitPosition, 1, ref DigitalSwitch1Active);
      serializer(action, raw, ref bitPosition, 1, ref DigitalSwitch2Active);
      serializer(action, raw, ref bitPosition, 1, ref DigitalSwitch3Active);
      serializer(action, raw, ref bitPosition, 1, ref DigitalSwitch4Active);
      serializer(action, raw, ref bitPosition, 8, ref InclusiveProductWatchOrGeographicFencingID);
      serializer(action, raw, ref bitPosition, 8, ref ExclusiveProductWatchOrGeographicFencingID);
      serializer(action, raw, ref bitPosition, 8, ref TimeBasedProductWatchOrFencingID);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref fuelConsumption);
      serializer(action, raw, ref bitPosition, 8, ref FuelLevelPercentage);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalIdleTime);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalMaximumFuel);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref NumberOfEngineStarts);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalEngineRevolutions);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref totalIdleFuel);
    }
  }
}
