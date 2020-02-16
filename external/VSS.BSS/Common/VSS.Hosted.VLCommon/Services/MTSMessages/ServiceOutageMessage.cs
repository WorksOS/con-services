using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class ServiceOutageMessage : TrackerMessage, INeedsAcknowledgement, TrackerMessage.IDeviceSequenceID
  {

    public static new readonly int kPacketID = 0x0E;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    private readonly DateTime beginTimeUTC = new DateTime(2009, 01, 01);

    public DateTime UtcDateTime
    {
      get
      {
        return beginTimeUTC.AddSeconds(SecondsElapsed);
      }
      set
      {
        SecondsElapsed = (uint)value.Subtract(beginTimeUTC).TotalSeconds;
      }
    }
    public OutageCategory ServiceOutageCatergory
    {
      get { return (OutageCategory)category; }
      set { category = (byte)value; }
    }
    public OutageLevel ServiceOutageLevel
    {
      get { return (OutageLevel)level; }
      set { level = (byte)value; }
    }

    public byte OutageDescriptionCode;
    public string OutageDescription;

    private byte DevicePacketSequenceIDRaw;
    private uint SecondsElapsed;
    private byte category;
    private byte level;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      filler(ref bitPosition, 4);
      serializer(action, raw, ref bitPosition, 8, ref DevicePacketSequenceIDRaw);
      serializer(action, raw, ref bitPosition, 32, ref SecondsElapsed);
      serializer(action, raw, ref bitPosition, 8, ref category);
      serializer(action, raw, ref bitPosition, 8, ref level);
      serializer(action, raw, ref bitPosition, 8, ref OutageDescriptionCode);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 16, ref OutageDescription);
    }

    #region Implementation of IDeviceSequenceID

    public byte DevicePacketSequenceID
    {
      get { return DevicePacketSequenceIDRaw; }
      set { DevicePacketSequenceIDRaw = value; }
    }

    #endregion

    #region Implementation of INeedsAcknowledgement

    public Type AcknowledgementType
    {
      get { return typeof(ServiceOutageAck); }
    }

    public ProtocolMessage GenerateAcknowledgement()
    {
      ServiceOutageAck ack = new ServiceOutageAck();

      ack.DevicePacketSequenceID = DevicePacketSequenceID;

      return ack;
    }

    public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
    {
      ServiceOutageAck ack = ackMessage as ServiceOutageAck;

      return ack != null && ack.DevicePacketSequenceID == DevicePacketSequenceID;
    }

    #endregion

    public enum OutageCategory
    {
      MSPProcessor = 1,
      GPS = 2,
      MasterBoard = 3,
      SkylineFirmwareApplication = 4,
      CATGatewayBoard = 5,
      FirmwareUpdate = 6,
      PeripheralConnection = 7,
    }
    public enum OutageLevel
    {
      Debug = 0,
      Info = 1,
      Warn = 2,
      Error = 3,
      Fatal = 4,
    }

    #region "Unconverted value properties for OEM Data Feed"
    public uint UtcDateTimeUnConverted
    {
      get { return SecondsElapsed; }
    }
    #endregion
  }

  public class ServiceOutageAck : BaseMessage, TrackerMessage.IDeviceSequenceID
  {
    public static new readonly int kPacketID = 0x1B;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      // Now for my members

      serializer(action, raw, ref bitPosition, 8, ref ServiceOutageSequenceIDRaw);
    }

    public byte ServiceOutageSequenceIDRaw;

    #region Implementation of IDeviceSequenceID

    public byte DevicePacketSequenceID
    {
      get { return ServiceOutageSequenceIDRaw; }
      set { ServiceOutageSequenceIDRaw = value; }
    }

    #endregion
  }
}
