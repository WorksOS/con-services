using System;
using System.Linq;

namespace VSS.Hosted.VLCommon
{

  public enum SequenceType
  {
    MessageSequenceID = 0,
    DeviceSiteID = 1,
    PredefinedMessageListID = 2,
    DriverID = 3
  }

  public static class MessageSequenceAPI
  {
    public static uint OutboundSequenceID(INH_OP opCtx1, DeviceTypeEnum deviceType)
    {
      uint seqID = 0;
      Sequence sequence;
          sequence = (from msgSequence in opCtx1.Sequence
                             select msgSequence).FirstOrDefault<Sequence>();

        if (sequence == null)
        {
          sequence = new Sequence();
          sequence.CCOutSequenceID = 0;
          sequence.PR3OutSequenceID = 0;
          opCtx1.Sequence.AddObject(sequence);
          opCtx1.SaveChanges();
        }

        switch (deviceType)
        {
          case DeviceTypeEnum.CrossCheck:
            seqID = (uint)(sequence.CCOutSequenceID % uint.MaxValue);
            sequence.CCOutSequenceID = ++seqID;
            break;
          case DeviceTypeEnum.Series521:
          case DeviceTypeEnum.Series522:
          case DeviceTypeEnum.Series523:
          case DeviceTypeEnum.SNM940:
          case DeviceTypeEnum.SNM941:
          case DeviceTypeEnum.PL420:
          case DeviceTypeEnum.PL421:
          case DeviceTypeEnum.PL431:
          case DeviceTypeEnum.SNM451:
            seqID = (uint)(sequence.PR3OutSequenceID % uint.MaxValue);
            sequence.PR3OutSequenceID = ++seqID;
            break;
          default:
            throw new NotImplementedException(deviceType.ToString() + " not yet implemented");
        }

        int result = opCtx1.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save message sequence");
      return seqID;
    }
  }
}
