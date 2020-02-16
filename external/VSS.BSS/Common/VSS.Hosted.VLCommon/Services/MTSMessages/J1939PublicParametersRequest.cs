using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class J1939PublicParametersRequest : BaseMessage
  {
    public static new readonly int kPacketID = 0x1E;
    public uint sequenceID;
    public J1939ParameterID[] parameterBlock;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 32, ref sequenceID);
      parameterBlock = (J1939ParameterID[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, parameterBlock, typeof(J1939ParameterID));
    }
  }
}
