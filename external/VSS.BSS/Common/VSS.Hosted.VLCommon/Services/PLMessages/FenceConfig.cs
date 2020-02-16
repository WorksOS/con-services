using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon.PLMessages
{
  public class FenceConfig : PLBaseMessage
  {
    private static readonly uint runForever = 0xFFFFFFFF;
    private static readonly uint startImmediate = 0x00000000;

    public static new readonly int kPacketID = 0x02;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public InclusiveConfig inclusive = null;
    public ExclusiveConfig exclusive = null;
    public TimeBasedConfig timeBased = null;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      bool includeInclusive = inclusive != null;
      bool includeExclusive = exclusive != null;
      bool includeTimeBased = timeBased != null;

      serializer(action, raw, ref bitPosition, 1, ref includeInclusive);
      serializer(action, raw, ref bitPosition, 1, ref includeExclusive);
      serializer(action, raw, ref bitPosition, 1, ref includeTimeBased);
      filler(ref bitPosition, 5);
      if (includeInclusive)
      {
        if (action == SerializationAction.Hydrate)
          inclusive = new InclusiveConfig();
        serializer(action, raw, ref bitPosition, 8, ref inclusive.inclusiveMessageID);
        BigEndianSerializer(action, raw, ref bitPosition, 3, ref inclusive.latitude);
        BigEndianSerializer(action, raw, ref bitPosition, 3, ref inclusive.longitude);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref inclusive.radius);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref inclusive.startTime);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref inclusive.endTime);
      }
      if (includeExclusive)
      {
        if (action == SerializationAction.Hydrate)
          exclusive = new ExclusiveConfig();
        serializer(action, raw, ref bitPosition, 8, ref exclusive.exclusiveMessageID);
        byte counter = (byte)exclusive.exclusiveLatitude.Count();
        serializer(action, raw, ref bitPosition, 8, ref counter);
        for (int i = 0; i < counter; i++)
        {
          if (action == SerializationAction.Hydrate)
          {
            uint lat = 0;
            uint lon = 0;
            ushort rad = 0;
            BigEndianSerializer(action, raw, ref bitPosition, 3, ref lat);
            exclusive.exclusiveLatitude.Add((decimal)ConversionFactors.ConvertLatGeodesicToDegrees(lat));
            BigEndianSerializer(action, raw, ref bitPosition, 3, ref lon);
            exclusive.exclusiveLongitude.Add((decimal)ConversionFactors.ConvertLonGeodesicToDegrees(lon));
            BigEndianSerializer(action, raw, ref bitPosition, 2, ref rad);
            exclusive.exclusiveRadius.Add(rad);
          }
          else
          {
            uint lat = ConversionFactors.ConvertLatDegreesToGeodesic(exclusive.exclusiveLatitude[i]);
            uint lon = ConversionFactors.ConvertLonDegreesToGeodesic(exclusive.exclusiveLongitude[i]);
            ushort rad = (ushort)exclusive.exclusiveRadius[i];
            BigEndianSerializer(action, raw, ref bitPosition, 3, ref lat);
            BigEndianSerializer(action, raw, ref bitPosition, 3, ref lon);
            BigEndianSerializer(action, raw, ref bitPosition, 2, ref rad);
          }
        }
      }
      if (includeTimeBased)
      {
        if (action == SerializationAction.Hydrate)
          timeBased = new TimeBasedConfig();
        serializer(action, raw, ref bitPosition, 8, ref timeBased.timeMessageID);
        
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workSun);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workSat);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workFri);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workThur);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workWed);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workTue);
        serializer(action, raw, ref bitPosition, 1, ref timeBased.workMon);
        filler(ref bitPosition, 1);
        serializer(action, raw, ref bitPosition, 8, ref timeBased.startHourUTC);
        serializer(action, raw, ref bitPosition, 8, ref timeBased.endHourUTC);
        
      }
    }
    public class InclusiveConfig
    {
      public byte inclusiveMessageID;
      public decimal InclusiveLatitude
      {
        get { return (decimal)ConversionFactors.ConvertLatGeodesicToDegrees(latitude); }
        set { latitude = ConversionFactors.ConvertLatDegreesToGeodesic((decimal)value); }
      }
      internal uint latitude;
      public decimal inclusiveLongitude
      {
        get { return (decimal)ConversionFactors.ConvertLonGeodesicToDegrees(longitude); }
        set { longitude = ConversionFactors.ConvertLonDegreesToGeodesic((decimal)value); }
      }
      internal uint longitude;
      public decimal inclusiveRadius
      {
        get { return radius; }
        set { radius = (short)Math.Round(value, MidpointRounding.AwayFromZero); }
      }
      internal short radius;
      public DateTime? startTimeUTC
      {
        get
        {
          if (startTime == startImmediate)
            return null;
          return epoch.AddSeconds(startTime);
        }
        set
        {
          if (!value.HasValue)
            startTime = startImmediate;
          else
            startTime = (uint)value.Value.Subtract(epoch).TotalSeconds;
        }
      }
      internal uint startTime;
      public DateTime? endTimeUTC
      {
        get
        {
          if (endTime == runForever)
            return null;
          return epoch.AddSeconds(endTime);
        }
        set
        {
          if (!value.HasValue)
            endTime = runForever;
          else
            endTime = (uint)value.Value.Subtract(epoch).TotalSeconds;
        }
      }
      internal uint endTime;
    }

    public class ExclusiveConfig
    {
      public byte exclusiveMessageID;
      public List<decimal> exclusiveLatitude = new List<decimal>();
      public List<decimal> exclusiveLongitude = new List<decimal>();
      public List<decimal> exclusiveRadius = new List<decimal>();
    }

    public class TimeBasedConfig
    {
      public bool timeBasedProductWatch;
      public byte timeMessageID;

      public bool workSun;
      public bool workMon;
      public bool workTue;
      public bool workWed;
      public bool workThur;
      public bool workFri;
      public bool workSat;
      public byte startHourUTC;
      public byte endHourUTC;
    }
  }
}
