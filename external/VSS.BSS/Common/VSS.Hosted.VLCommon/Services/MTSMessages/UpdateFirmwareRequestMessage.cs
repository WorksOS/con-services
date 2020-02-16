using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class UpdateFirmwareRequestMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID, INeedsAcknowledgement
  {
    public static new readonly int kPacketID = 0x17;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      // Now for my members

      serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref Directory);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref Host);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref UserName);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref base64PasswordString);
    }
     
     

    public string Directory;
    public string Host;
    public string UserName;
    public string Password
    {
      get
      {
        byte [] base64 = Convert.FromBase64String(base64PasswordString);
        return Encoding.ASCII.GetString(base64);
      }
      set
      {
        byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(value);
        base64PasswordString = Convert.ToBase64String(toEncodeAsBytes);
      }
    }

    public string base64PasswordString;

    private Int64 MessageSequenceIDRaw;

    #region Implementation of IBaseMessageSequenceID

    public Int64 BaseMessageSequenceID
    {
      get { return MessageSequenceIDRaw; }
      set { MessageSequenceIDRaw = value; }
    }

    #endregion

    #region Implementation of INeedsAcknowledgement

    public Type AcknowledgementType
    {
      get { return typeof(MessageResponseTrackerMessage); }
    }

    public ProtocolMessage GenerateAcknowledgement()
    {
      MessageResponseTrackerMessage ack = new MessageResponseTrackerMessage();

      ack.BaseMessageSequenceID = BaseMessageSequenceID;

      return ack;
    }

    public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
    {
      MessageResponseTrackerMessage ack = ackMessage as MessageResponseTrackerMessage;

      return ack != null && ack.BaseMessageSequenceID == BaseMessageSequenceID;
    }

    #endregion
  }
}
