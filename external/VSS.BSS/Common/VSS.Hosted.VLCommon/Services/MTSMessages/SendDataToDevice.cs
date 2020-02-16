using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class SendDataToDevice : BaseMessage
  {
    public static new readonly int kPacketID = 0x1D;
    public uint sequenceID;
    public ControlType controlType;
    public Destination destination;
    public byte[] data;

    private readonly DateTime beginTimeUTC = new DateTime(2009, 01, 01);
    public uint SendUTCSecondsRaw;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public DateTime SendUTC
    {
      get
      {
        return beginTimeUTC.AddSeconds(SendUTCSecondsRaw);
      }
      set
      {
        SendUTCSecondsRaw = (uint)value.Subtract(beginTimeUTC).TotalSeconds;
      }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 32, ref sequenceID);

      serializer(action, raw, ref bitPosition, 32, ref SendUTCSecondsRaw);
      
      // Setup temp byte to pass to serializer and restore to enum in case it changes
      byte temp = (byte)controlType;
      serializer(action, raw, ref bitPosition, 8, ref temp);
      controlType = (ControlType)temp;

      // Setup temp byte to pass to serializer and restore to enum in case it changes
      temp = (byte)destination;
      serializer(action, raw, ref bitPosition, 8, ref temp);
      destination = (Destination)temp;

      serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref data);
    }

    public enum ControlType
    {
      RawJ1939Data = 0x00,
      AsciiText = 0x01,
      UnicodeText = 0x02
    }

    public enum Destination
    {
      CanBusInstance1 = 0x00,
      CanBusInstance2 = 0x01
    }

  }
}
