using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Hosted.VLCommon;


namespace VSS.Hosted.VLCommon.PLMessages
{
  public class Diagnostic : NestedMessage
  {
    public byte Level;
    public byte MID;
    public ushort CID;
    public byte FMI;
    public byte Occurrences;
    public DateTime? Timestamp;
    public uint? ServiceMeterHour = null;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref Level);
      serializer(action, raw, ref bitPosition, 8, ref MID);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref CID);
      serializer(action, raw, ref bitPosition, 8, ref FMI);
      serializer(action, raw, ref bitPosition, 8, ref Occurrences);
      if (action == SerializationAction.Hydrate)
      {
        if (raw[bitPosition / 8] == 0x00)
        {
          uint tempSMH = 0;
          filler(ref bitPosition, 8);
          BigEndianSerializer(action, raw, ref bitPosition, 3, ref tempSMH);
          Timestamp = null;
          ServiceMeterHour = tempSMH;
        }
        else
        {
          uint tempTime = 0;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref tempTime);
          DateTime epoch = new DateTime(1980, 01, 06);
          Timestamp = epoch.AddSeconds(tempTime);
          ServiceMeterHour = null;
        }
      }
      else
      {
        if (ServiceMeterHour.HasValue)
        {
          uint tempSMH = ServiceMeterHour.Value;
          filler(ref bitPosition, 8);
          BigEndianSerializer(action, raw, ref bitPosition, 3, ref tempSMH);
        }
        else
        {
          DateTime epoch = new DateTime(1980, 01, 06);
          uint tempTime = (uint)Timestamp.Value.Subtract(epoch).TotalSeconds;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref tempTime);
        }
      }
    }
  }

  public class FaultDiagnostic : PLTrackerMessage
  {
    public static new readonly int kPacketID = 0x35;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public DateTime Timestamp
    {
      get { return epoch.AddSeconds(timestamp); }
    }
    private uint timestamp;
    public double Latitude
    {
      get { return ConversionFactors.ConvertLatGeodesicToDegrees(latitude); }
      set { latitude = ConversionFactors.ConvertLatDegreesToGeodesic((decimal)value); }
    }
    private uint latitude;
    public double Longitude
    {
      get { return ConversionFactors.ConvertLonGeodesicToDegrees(longitude); }
      set { longitude = ConversionFactors.ConvertLonDegreesToGeodesic((decimal)value); }
    }
    private uint longitude;
    public PLLocationTypeEnum LocationType
    {
      get { return (PLLocationTypeEnum)locationType; }
      set { locationType = (byte)value; }
    }
    private byte locationType;
    public Diagnostic[] Diagnostics;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref timestamp);
      serializer(action, raw, ref bitPosition, 24, ref latitude);
      serializer(action, raw, ref bitPosition, 24, ref longitude);
      serializer(action, raw, ref bitPosition, 8, ref locationType);
      int count = Diagnostics == null ? 0 : Diagnostics.Length;
      serializer(action, raw, ref bitPosition, 8, ref count);
      if (action == SerializationAction.Hydrate)
        Diagnostics = new Diagnostic[count];

      Diagnostics = (Diagnostic[])
            serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, Diagnostics, typeof(Diagnostic));
    }
  }
}
