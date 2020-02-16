using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class PersonalityReportMessage : TrackerMessage, INeedsAcknowledgement
  {
    public static new readonly int kPacketID = 0x0F;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      filler(ref bitPosition, 4);

      uint count = (PersonalityBlocks == null) ? 0 : (uint)PersonalityBlocks.Length;

      serializer(action, raw, ref bitPosition, 8, ref count);

      if (action == SerializationAction.Hydrate)
      {
        PersonalityBlocks = new PersonalityReportBlock[count];
      }

      PersonalityBlocks = (PersonalityReportBlock[])
         serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 0, PersonalityBlocks, typeof(PersonalityReportBlock));
    }

    public PersonalityReportBlock[] PersonalityBlocks;

    #region Implementation of INeedsAcknowledgement

    public Type AcknowledgementType
    {
      get { return typeof(PersonalityReportAck); }
    }

    public ProtocolMessage GenerateAcknowledgement()
    {
      PersonalityReportAck ack = new PersonalityReportAck();

      return ack;
    }

    public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
    {
      PersonalityReportAck ack = ackMessage as PersonalityReportAck;

      return ack != null;
    }

    #endregion

  }

  public class PersonalityReportBlock : NestedMessage
  {
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref reportType);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref Description);
    }

    public ReportType VersionReportType
    {
      get
      {
        return (ReportType)reportType;
      }
      set 
      {
        reportType = (byte)value;
      }
    }

    public string Description = string.Empty;

    private byte reportType;

    public enum ReportType
    {
      U_Boot = 0x00,
      Kernel = 0x01,
      RFS = 0x02,
      MSP = 0x03,
      GPS = 0x04,
      SCID = 0x05,
      GSN = 0x06,
      GM = 0x07,
      SerialNumber = 0x08,
      Gateway = 0x09,
      Hardware = 0x0A,
      Software = 0x0B,
      VIN = 0x0C,
    }
  }
}
