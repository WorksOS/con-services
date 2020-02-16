using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class FirmwareUpdateResponse : BitConfigurationMessage
  {
    public static new readonly int kPacketID = (int)BitConfigurationTrackerMessage.BitConfigurationID.FirmwareUpdateStatus; // 0x1B

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override string Abbreviation { get { return "FrmUpdteSts"; } }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
      serializer(action, raw, ref bitPosition, 32, ref DateTimeSeconds);
      serializer(action, raw, ref bitPosition, 8, ref StatusNum);
    }

    public uint MessageSequenceID
    {
      get
      {
        return MessageSequenceIDRaw;
      }
      set
      {
        MessageSequenceIDRaw = value;
      }
    }

    public DateTime UTCDateTime
    {
      get
      {
        return new DateTime(2009, 01, 01).AddSeconds((double)DateTimeSeconds);
      }
      set
      {
        DateTimeSeconds = (uint)value.Subtract(new DateTime(2009, 01, 01)).TotalSeconds;
      }
    }

    public FirmwareUpdateStatusDescription FirmwareUpdaterStatus
    {
      get
      {
        return (FirmwareUpdateStatusDescription)StatusNum;
      }
      set
      {
        StatusNum = (byte)value;
      }
    }

    private uint MessageSequenceIDRaw;
    private uint DateTimeSeconds;
    private byte StatusNum;

    public enum FirmwareUpdateStatusDescription
    {
      Started = 0,
      Successful = 1,
      Rejected = 2,
      DownloadFailed = 3,
      UpgradeFailed = 4,
      DownloadCompleted = 5,
      Busy = 6,
    }

    #region "Unconverted value properties for OEM Data Feed"
    public uint UtcDateTimeUnConverted
    {
      get { return DateTimeSeconds; }
    }
    #endregion
  }
}
