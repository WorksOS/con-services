using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Hosted.VLCommon.PLMessages
{
  public class ProductWatchActivation : PLBaseMessage
  {
    public static new readonly int kPacketID = 0x50;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public bool? inclusiveWatchActivate = null;
    public bool? exclusiveWatchActivate = null;
    public bool? timeBasedWatchActivate = null;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      if (action == SerializationAction.Hydrate)
      {
        base.Serialize(action, raw, ref bitPosition);
        filler(ref bitPosition, 8);
      }
      else
      {
        string prodWatch = "P ";
        serializeFixedLengthString(action, raw, ref bitPosition, 2, ref prodWatch);
      }

      byte inclusive = (byte)(!inclusiveWatchActivate.HasValue ? 0 : inclusiveWatchActivate.Value ? 1 : 2);
      serializer(action, raw, ref bitPosition, 2, ref inclusive);
      if (action == SerializationAction.Hydrate)
      {
        if (inclusive == 0)
          inclusiveWatchActivate = null;
        else if (inclusive == 1)
          inclusiveWatchActivate = true;
        else
          inclusiveWatchActivate = false;
      }
      byte exclusive = (byte)(!exclusiveWatchActivate.HasValue ? 0 : exclusiveWatchActivate.Value ? 1 : 2);
      serializer(action, raw, ref bitPosition, 2, ref exclusive);
      if (action == SerializationAction.Hydrate)
      {
        if (exclusive == 0)
          exclusiveWatchActivate = null;
        else if (exclusive == 1)
          exclusiveWatchActivate = true;
        else
          exclusiveWatchActivate = false;
      }
      byte timeBased = (byte)(!timeBasedWatchActivate.HasValue ? 0 : timeBasedWatchActivate.Value ? 1 : 2);
      serializer(action, raw, ref bitPosition, 2, ref timeBased);
      if (action == SerializationAction.Hydrate)
      {
        if (timeBased == 0)
          exclusiveWatchActivate = null;
        else if (timeBased == 1)
          timeBasedWatchActivate = true;
        else
          timeBasedWatchActivate = false;
      }

      filler(ref bitPosition, 2);

      string fill = "0";
      serializeFixedLengthString(action, raw, ref bitPosition, 1, ref fill);
      
      byte sequenceNumber = 0;
      serializer(action, raw, ref bitPosition, 8, ref sequenceNumber);
    }
  }
}
