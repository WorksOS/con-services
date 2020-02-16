using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  public enum SiteTypeEnum
  {
    Reserved0 = 0,
    HomeSite = 1,
    WorkSite = 2,
    Reserved3 = 3,
  }
  public class ConfigurePolygonMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                                   INeedsAcknowledgement
  {
    public static new readonly int kPacketID = 0x14;
    private readonly DateTime beginTimeUTC = new DateTime(2009, 01, 01);

    public override int PacketID
    {
      get { return kPacketID; }
    }
    private uint MessageSequenceIDRaw;
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
    public SiteTypeEnum SiteType
    {
      get
      {
        return (SiteTypeEnum)SiteTypeRaw;
      }
      set
      {
        SiteTypeRaw = (byte)value;
      }
    }
    public Int64 SiteID = 0;
    public ushort SlotNumber;
    public TimeSpan TimeToLiveHours
    {
      get
      {
        return TimeSpan.FromHours(TimeToLiveRaw);
      }
      set
      {
        TimeToLiveRaw = (ushort)value.TotalHours;
      }
    }
    public string ApplicationData;
    public ushort NumberOfVertices;
    public List<PolygonMessagePoint> Points;
    public string SiteName;
    public string SiteText;

    private uint SendUTCSecondsRaw;
    private byte SiteTypeRaw;
    private ushort TimeToLiveRaw;

    #region Implementation of IBaseMessageSequenceID

    public long BaseMessageSequenceID
    {
      get { return MessageSequenceIDRaw; }
      set { MessageSequenceIDRaw = (uint)value; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
      serializer(action, raw, ref bitPosition, 32, ref SendUTCSecondsRaw);
      serializer(action, raw, ref bitPosition, 8, ref SiteTypeRaw);
      serializer(action, raw, ref bitPosition, 32, ref SiteID);
      serializer(action, raw, ref bitPosition, 16, ref SlotNumber);
      serializer(action, raw, ref bitPosition, 16, ref TimeToLiveRaw);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref ApplicationData);
      NumberOfVertices = (ushort)(Points == null ? 0 : Points.Count());
      serializer(action, raw, ref bitPosition, 16, ref NumberOfVertices);
      double oldLat = 0;
      double oldLong = 0;
      for (int i = 0; i < NumberOfVertices; i++)
      {
        if (action == SerializationAction.Hydrate)
        {
          if (Points == null)
            Points = new List<PolygonMessagePoint>();
          PolygonMessagePoint point = new PolygonMessagePoint();
          if (i == 0)
          {
            serializer(action, raw, ref bitPosition, 32, ref point.LatitudeRaw);
            serializer(action, raw, ref bitPosition, 32, ref point.LongitudeRaw);
            oldLat = point.Latitude;
            oldLong = point.Longitude;
          }
          else
          {
            PolygonMessagePoint deltaPoint = new PolygonMessagePoint();
            serializer(action, raw, ref bitPosition, 24, ref deltaPoint.LatitudeRaw);
            serializer(action, raw, ref bitPosition, 24, ref deltaPoint.LongitudeRaw);
            point.Latitude = deltaPoint.Latitude + oldLat;
            point.Longitude = deltaPoint.Longitude + oldLong;
            oldLat = point.Latitude;
            oldLong = point.Longitude;
          }

          Points.Add(point);
        }
        else
        {
          if (i == 0)
          {
            oldLat = Points[i].Latitude;
            oldLong = Points[i].Longitude;
            serializer(action, raw, ref bitPosition, 32, ref Points[i].LatitudeRaw);
            serializer(action, raw, ref bitPosition, 32, ref Points[i].LongitudeRaw);
          }
          else
          {
            PolygonMessagePoint deltaPoint = new PolygonMessagePoint();
            deltaPoint.Latitude = Points[i].Latitude - oldLat;
            deltaPoint.Longitude = Points[i].Longitude - oldLong;
            serializer(action, raw, ref bitPosition, 24, ref deltaPoint.LatitudeRaw);
            serializer(action, raw, ref bitPosition, 24, ref deltaPoint.LongitudeRaw);
            oldLat = Points[i].Latitude;
            oldLong = Points[i].Longitude;
          }
        }
      }
      serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SiteName);
      serializeLengthPrefixedString(action, raw, ref bitPosition, 16, ref SiteText);
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

    public class PolygonMessagePoint
    {
      public double Latitude
      {
        get { return (double)LatitudeRaw * Constants.LatLongConversionMultiplier; }
        set { LatitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
      }
      public double Longitude
      {
        get { return (double)LongitudeRaw * Constants.LatLongConversionMultiplier; }
        set { LongitudeRaw = (int)(value / Constants.LatLongConversionMultiplier); }
      }

      internal int LatitudeRaw;
      internal int LongitudeRaw;

      public override string ToString()
      {
        StringBuilder builder = new StringBuilder("PolygonMessagePoint");
        builder.AppendFormat("\nLatitude: {0}", Latitude);
        builder.AppendFormat("\nLongitude: {0}", Longitude);
        return builder.ToString();
      }
    }
  }
}
