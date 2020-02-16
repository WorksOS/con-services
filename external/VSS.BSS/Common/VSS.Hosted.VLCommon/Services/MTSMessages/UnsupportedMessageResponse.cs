using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class UnsupportedMessageResponse : BaseMessage
  {
    public static new readonly int kPacketID = 0x2C;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public byte[] UnknownMessageData;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);
      serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref UnknownMessageData);
    }
  }
}
