using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class J1939ParameterID : NestedMessage
  {
      public byte SourceAddress;
      public ushort PGN;
      public int SPN;

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        serializer(action, raw, ref bitPosition, 8, ref SourceAddress);
        serializer(action, raw, ref bitPosition, 16, ref PGN);
        serializer(action, raw, ref bitPosition, 24, ref SPN);
      }
  }
}
