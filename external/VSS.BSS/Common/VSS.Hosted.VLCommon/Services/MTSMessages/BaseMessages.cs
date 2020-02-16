using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml.Serialization;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Script.Serialization;
using System.Collections.Generic;

namespace VSS.Hosted.VLCommon.MTSMessages
{
   public abstract class BaseMessage : PlatformMessage 
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.Base; }
      }

      public interface IBaseMessageSequenceID 
      {
         Int64   BaseMessageSequenceID   { get; set; }
      }
   }

   public class TextBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                               INeedsAcknowledgement,
                                               IUtcTime
   {
      public static new readonly int kPacketID = 0x01;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 20, ref SendTimeGPSSeconds.Seconds);
         serializer(action, raw, ref bitPosition,  3, ref ResponseSetID);
         filler    (             ref bitPosition,  1);
         serializeLengthPrefixedString(action, raw, ref bitPosition, 16, ref MessageRaw.MessageRaw);
      }

      private      Int64                   MessageSequenceIDRaw;
      public      UtcSeconds      SendTimeGPSSeconds;
      public      Int16                   ResponseSetID;
      private      MessageResponseParser   MessageRaw;

      public String  Message 
      {
         get { return MessageRaw.Message; }
         set { MessageRaw.Message = value; }
      }

      public String  ResponseSetText
      {
         get { return MessageRaw.ResponseSetText; }
         set { MessageRaw.ResponseSetText = value; }
      }

      #region MessageResponseParser helper for dealing with whack Message payload

      public struct MessageResponseParser 
      {
         public      String               MessageRaw;
         
         public   String   Message 
         {
            get 
            {
               // Up to the first |.

               if (MessageRaw == null) 
               {
                  return String.Empty;
               }

               int i = MessageRaw.IndexOf('|');

               return (i == -1) ? MessageRaw : MessageRaw.Substring(0, i);
            }

            set 
            {
               // Crop the set value to the first |.

               if (value == null) 
               {
                  value = String.Empty;
               } 
               else 
               {
                  int i = value.IndexOf('|');

                  if (i != -1) 
                  {
                     value = value.Substring(0, i);
                  }
               }

               string responses = ResponseSetText;

               MessageRaw = value + responses;
            }
         }

         public   String   ResponseSetText 
         {
            get 
            {
               // Everything after the first |

               if (MessageRaw == null) 
               {
                  return String.Empty;
               }

               int i = MessageRaw.IndexOf('|');

               return (i == -1) ? String.Empty : MessageRaw.Substring(i);
            }

            set 
            {
               // Extract the message and append the response text to it. Note that
               // the response set must start with | and include at most 4 bars.

               if (value != null  &&  value.Length != 0) 
               {
                  if (value[0] != '|') 
                  {
                     value = '|'+value;
                  }

                  int barsLeft = 5;
                  int lastPos  = 0;
                  int i = value.IndexOf('|');

                  while (barsLeft > 0  &&  i != -1) 
                  {
                     barsLeft--;
                     lastPos = i;
                     i = value.IndexOf('|', lastPos+1);
                  }

                  // if barsLeft == 0 then lastPos points to the 5th '|' so crop the string there.

                  if (barsLeft == 0) 
                  {
                     value = value.Substring(0, lastPos);
                  }

                  MessageRaw = Message+value;
               } 
               else
               {
                  MessageRaw = Message;
               }
            }
         }
      }

      #endregion

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

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return SendTimeGPSSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return SendTimeGPSSeconds.Date; }
         set { SendTimeGPSSeconds.Date = value; }
      }

      #endregion
   }

   public class UserDataBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                                   INeedsAcknowledgement,
                                                   IUtcTime
   {
      public static new readonly int kPacketID = 0x03;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 24, ref SendTimeGPSSeconds.Seconds);

         serializeSingleEmbeddedMessage(action, raw, ref bitPosition, 16, ref Message);
      }

      public    Int64               MessageSequenceIDRaw;
      public    UtcSeconds  SendTimeGPSSeconds;
      public    BaseUserDataMessage Message;

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

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return SendTimeGPSSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return SendTimeGPSSeconds.Date; }
         set { SendTimeGPSSeconds.Date = value; }
      }

      #endregion
   }

   public class SetIntervalsBaseMessage : BaseMessage 
   {
      public static new readonly int kPacketID = 0x04;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 16, ref SamplingInterval);
         serializer(action, raw, ref bitPosition, 16, ref ReportingInterval);
         serializer(action, raw, ref bitPosition, 16, ref LowPowerInterval);
         serializer(action, raw, ref bitPosition, 16, ref BitPacketInterval);
         serializer(action, raw, ref bitPosition, 10, ref GpsDate.GpsWeek);
         serializer(action, raw, ref bitPosition,  3, ref GpsDate.GpsRollOverCount);
         filler    (             ref bitPosition,  1);
         serializer(action, raw, ref bitPosition,  2, ref IntervalUnitsRaw);
         serializer(action, raw, ref bitPosition, 24, ref GpsDate.GpsSecond);

         // If we are hydrating, see if we have enough bytes left for the ConnectionConfiguration
         // since we don't send that to CDPD devices.  If we are serializing, only do so if
         // the ConnectionConfiguration was specified.

         if (action == SerializationAction.Hydrate  &&  hydrationFinalBitPosition > bitPosition)
         {
            ConnectionModeSpecified = true;
         }

         if (ConnectionModeSpecified) 
         {
            serializer(action, raw, ref bitPosition, 16, ref ConnectionMode);
         }
      }

      public static SetIntervalsBaseMessage GenerateShutupMessage() 
      {
         // Construct a new SetIntervalsBaseMessage message; a shut-up message simply as all zeros
         // for the intervals, which is the default on construction.  So this is mostly a documentation
         // aid.

         return new SetIntervalsBaseMessage();
      }

      public static SetIntervalsBaseMessage GenerateDefaultIntervalsMessage(bool forTcp) 
      {
         SetIntervalsBaseMessage msg = new SetIntervalsBaseMessage();

         if (forTcp) 
         {
            msg.ConnectionMode = 0;
            msg.ConnectionModeSpecified = true;
         }

         msg.SamplingInterval = 60;
         msg.ReportingInterval = 600;
         msg.LowPowerInterval = 28800;
         msg.BitPacketInterval = 14400;

         return msg;
      }

      public static SetIntervalsBaseMessage GenerateReenterNetworkMessage() 
      {
         // Construct a new SetIntervalsBaseMessage message; a re-enter network message simply as all zeros
         // for the intervals, except a number of seconds (>0) for the reporting interval.  This message is
         // useful for re-balancing TCP devices that all glommed onto one server when the other started up slowly.
         // The number of seconds is the delay before resending the net-entry.

         SetIntervalsBaseMessage msg = new SetIntervalsBaseMessage();

         msg.ReportingInterval = 1;
         msg.BitPacketInterval = 14400; // Nominal value to keep the device from sending daily packets.

         return msg;
      }

      public bool IsShutupMessage 
      {
         get { return SamplingInterval == 0  &&  ReportingInterval == 0; }
      }

      public bool IsReenterNetworkMessage 
      {
         get { return SamplingInterval == 0  &&  ReportingInterval != 0; }
      }

      public    UInt16   SamplingInterval;
      public    UInt16   ReportingInterval;
      public    UInt16   LowPowerInterval;
      public    UInt16   BitPacketInterval;
      private    UInt16   IntervalUnitsRaw;

      // Following is for GPRS only.
      public    UInt16  ConnectionMode;          // 0x0000 = no keep-alives, 0xffff = new connection per transaction
      public    bool    ConnectionModeSpecified;

      public    GpsDate  GpsDate;

      public enum IntervalUnit 
      {
         Seconds,
         Minutes,
         Hours,
         Days
      }

      public IntervalUnit IntervalUnits
      {
         get { return (IntervalUnit) IntervalUnitsRaw; }
         set { IntervalUnitsRaw = (UInt16) value; }
      }
   }

   public class MessageResponseAckBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID
   {
      public static new readonly int kPacketID = 0x05;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  3, ref ResponseKeyID);
         filler    (             ref bitPosition,  5);
         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
      }

      public    UInt16  ResponseKeyID;
      private    Int64  MessageSequenceIDRaw;

      #region Implementation of IBaseMessageSequenceID

      public Int64 BaseMessageSequenceID
      {
         get { return MessageSequenceIDRaw; }
         set { MessageSequenceIDRaw = value; }
      }

      #endregion
   }

   public class SiteDispatchBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                                       TrackerMessage.ISiteID,
                                                       INeedsAcknowledgement,
                                                       IUtcTime
   {
      public static new readonly int kPacketID = 0x06;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 20, ref UtcSeconds.Seconds);
         serializer(action, raw, ref bitPosition,  3, ref ResponseSet);
         filler    (             ref bitPosition,  1);
         serializer(action, raw, ref bitPosition,  2, ref SiteTypeRaw);
         filler    (             ref bitPosition,  6);
         serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
         serializer(action, raw, ref bitPosition, 24, ref NELatitudeRaw);
         serializer(action, raw, ref bitPosition, 24, ref NELongitudeRaw);
         serializer(action, raw, ref bitPosition, 24, ref SWLatitudeRaw);
         serializer(action, raw, ref bitPosition, 24, ref SWLongitudeRaw);
         serializer(action, raw, ref bitPosition, 16, ref TimeToLiveHours);
         serializeLengthPrefixedString(action, raw, ref bitPosition, 16, ref MessageRaw.MessageRaw);
         serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref SiteName);
      }

      private    Int64   MessageSequenceIDRaw;
      public    UtcSeconds  UtcSeconds;
      public    Int16   ResponseSet;
      private    Int16   SiteTypeRaw;
      private    Int64   SiteIDRaw;
      private    Int32   NELatitudeRaw;
      private    Int32   NELongitudeRaw;
      private    Int32   SWLatitudeRaw;
      private    Int32   SWLongitudeRaw;
      public    UInt16  TimeToLiveHours;
      private    TextBaseMessage.MessageResponseParser   MessageRaw;
      public    String  SiteName;

      public String  Message 
      {
         get { return MessageRaw.Message; }
         set { MessageRaw.Message = value; }
      }

      public String Name
      {
          get { return SiteName; }
          set { SiteName = value; }
      }

      public String  ResponseSetText
      {
         get { return MessageRaw.ResponseSetText; }
         set { MessageRaw.ResponseSetText = value; }
      }

      public DeviceSiteType SiteType 
      {
         get { return (DeviceSiteType) SiteTypeRaw; }
         set { SiteTypeRaw = (Int16) value; }
      }

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDRaw; }
         set { SiteIDRaw = value; }
      }

      #endregion

      public Double  NELatitude 
      {
         get { return (double) NELatitudeRaw*Constants.SiteDispatchLatLongConversionMultiplier; }
         set { NELatitudeRaw = (Int32)(value/Constants.SiteDispatchLatLongConversionMultiplier); }
      }

      public Double  NELongitude
      {
         get { return (double) NELongitudeRaw*Constants.SiteDispatchLatLongConversionMultiplier; }
         set { NELongitudeRaw = (Int32)(value/Constants.SiteDispatchLatLongConversionMultiplier); }
      }

      public Double  SWLatitude 
      {
         get { return (double) SWLatitudeRaw*Constants.SiteDispatchLatLongConversionMultiplier; }
         set { SWLatitudeRaw = (Int32)(value/Constants.SiteDispatchLatLongConversionMultiplier); }
      }

      public Double  SWLongitude
      {
         get { return (double) SWLongitudeRaw*Constants.SiteDispatchLatLongConversionMultiplier; }
         set { SWLongitudeRaw = (Int32)(value/Constants.SiteDispatchLatLongConversionMultiplier); }
      }

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

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return UtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return UtcSeconds.Date; }
         set { UtcSeconds.Date = value; }
      }

      #endregion
   }

   public class UserDataAckBaseMessage : BaseMessage, TrackerMessage.IDeviceSequenceID
   {
      public static new readonly int kPacketID = 0x07;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  8, ref UserDataSequenceIDRaw);
      }

      private    byte   UserDataSequenceIDRaw;

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return UserDataSequenceIDRaw; }
         set { UserDataSequenceIDRaw = value; }
      }

      #endregion
   }

   public class SitePurgeBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                                    INeedsAcknowledgement,
                                                    IUtcTime
   {
      public static new readonly int kPacketID = 0x08;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 20, ref SendTimeUtcSeconds.Seconds);
         filler    (             ref bitPosition,  4);
         serializer(action, raw, ref bitPosition,  2, ref SiteTypeRaw);
         filler    (             ref bitPosition,  6);
         serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);

      }

      private    Int64   MessageSequenceIDRaw;
      public    UtcSeconds  SendTimeUtcSeconds;
      private    Int16   SiteTypeRaw;
      private    Int64   SiteIDRaw;

      public DeviceSiteType SiteType 
      {
         get { return (DeviceSiteType) SiteTypeRaw; }
         set { SiteTypeRaw = (Int16) value; }
      }

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDRaw; }
         set { SiteIDRaw = value; }
      }

      #endregion

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

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return SendTimeUtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return SendTimeUtcSeconds.Date; }
         set { SendTimeUtcSeconds.Date = value; }
      }

      #endregion
   }

   public class SiteStatusAckBaseMessage : BaseMessage, TrackerMessage.IDeviceSequenceID
   {
      public static new readonly int kPacketID = 0x0A;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  2, ref SiteTypeRaw);
         filler    (             ref bitPosition,  6);
         serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
         serializer(action, raw, ref bitPosition,  8, ref SiteSequenceIDRaw);
      }

      private    Int16   SiteTypeRaw;
      private    Int64   SiteIDRaw;
      private    byte    SiteSequenceIDRaw;

      public DeviceSiteType SiteType 
      {
         get { return (DeviceSiteType) SiteTypeRaw; }
         set { SiteTypeRaw = (Int16) value; }
      }

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDRaw; }
         set { SiteIDRaw = value; }
      }

      #endregion

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return SiteSequenceIDRaw; }
         set { SiteSequenceIDRaw = value; }
      }

      #endregion
   }

   public class DeviceStateStatusAckBaseMessage : BaseMessage, TrackerMessage.IDeviceSequenceID
   {
      public static new readonly int kPacketID = 0x0B;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  8, ref DevicePacketSequenceIDRaw);
      }

      private    byte    DevicePacketSequenceIDRaw;

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DevicePacketSequenceIDRaw; }
         set { DevicePacketSequenceIDRaw = value; }
      }

      #endregion
   }

   public class BitBlockAckBaseMessage : BaseMessage, TrackerMessage.IDeviceSequenceID
   {
      public static new readonly int kPacketID = 0x0C;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  8, ref BitPacketSequenceIDRaw);
      }

      private    byte    BitPacketSequenceIDRaw;

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return BitPacketSequenceIDRaw; }
         set { BitPacketSequenceIDRaw = value; }
      }

      #endregion
   }

   public class UpdateRealTimePositionBaseMessage : BaseMessage
   {
      public static new readonly int kPacketID = 0x0D;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);
      }
   }

   public class DriverStatusAckBaseMessage : BaseMessage, TrackerMessage.IDeviceSequenceID, TrackerMessage.IDriverID
   {
      public static new readonly int kPacketID = 0x12;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  8, ref DriverStatusSequenceIDRaw);

         serializeDriverIDWithAction(action, raw, ref bitPosition, ref DriverStatusActionRaw, this);
      }

      private    byte    DriverStatusSequenceIDRaw;
      private    byte    DriverStatusActionRaw;

      private    byte    DriverIDTypeRaw;
      private    Int64   PlatformDriverIDRaw;
      private    string  MdtDriverIDRaw;
      private    string  DriverDisplayNameRaw;
      //private    byte    RfidIbuttonCount;
      private    bool    DriverPresentRaw;

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DriverStatusSequenceIDRaw; }
         set { DriverStatusSequenceIDRaw = value; }
      }

      #endregion

      public DeviceDriverIDAction DriverStatusAction 
      {
         get { return (DeviceDriverIDAction) DriverStatusActionRaw; }
         set { DriverStatusActionRaw = (byte) value; }
      }

      #region Implementation of IDriverID

      public DeviceDriverIDType DriverIDType
      {
         get { return (DeviceDriverIDType) DriverIDTypeRaw; }
         set { DriverIDTypeRaw = (byte) value; }
      }

      public Int64  PlatformDriverID 
      {
         get { return PlatformDriverIDRaw; }
         set { PlatformDriverIDRaw = value; }
      }

      public string  MdtDriverID 
      {
         get { return MdtDriverIDRaw; }
         set { MdtDriverIDRaw = value; }
      }

      public string DriverDisplayName
      {
         get { return DriverDisplayNameRaw; }
         set { DriverDisplayNameRaw = value; }
      }

      public bool IsDriverPresent
      {
         get { return DriverPresentRaw; }
         set { DriverPresentRaw = value;}
      }

      #endregion
   }

   public class DriverValidationUpdateBaseMessage : BaseMessage, BaseMessage.IBaseMessageSequenceID,
                                                                 TrackerMessage.IDriverID,
                                                                 INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x13;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);

         serializeDriverIDWithAction(action, raw, ref bitPosition, ref DriverStatusActionRaw, this);
      }

      private    Int64   MessageSequenceIDRaw;
      private    byte    DriverStatusActionRaw;

      private    byte    DriverIDTypeRaw;
      private    Int64   PlatformDriverIDRaw;
      private    string  MdtDriverIDRaw;
      private    string  DriverDisplayNameRaw;
      private    bool    DriverPresentRaw;

      #region Implementation of IBaseMessageSequenceID

      public Int64 BaseMessageSequenceID
      {
         get { return MessageSequenceIDRaw; }
         set { MessageSequenceIDRaw = value; }
      }

      #endregion

      public DeviceDriverIDAction DriverStatusAction 
      {
         get { return (DeviceDriverIDAction) DriverStatusActionRaw; }
         set { DriverStatusActionRaw = (byte) value; }
      }

      #region Implementation of IDriverID

      public DeviceDriverIDType DriverIDType
      {
         get { return (DeviceDriverIDType) DriverIDTypeRaw; }
         set { DriverIDTypeRaw = (byte) value; }
      }

      public Int64  PlatformDriverID 
      {
         get { return PlatformDriverIDRaw; }
         set { PlatformDriverIDRaw = value; }
      }

      public string  MdtDriverID 
      {
         get { return MdtDriverIDRaw; }
         set { MdtDriverIDRaw = value; }
      }

      public string DriverDisplayName
      {
         get { return DriverDisplayNameRaw; }
         set { DriverDisplayNameRaw = value; }
      }

      public bool IsDriverPresent
      {
         get { return DriverPresentRaw; }
         set { DriverPresentRaw = value;}
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

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion
   }


   #region RF Programming Base Messages

   public class RFProgrammingScheduleBaseMessage : BaseMessage,
                                                   IUtcTime,
                                                   INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x0E;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref ProgrammingEventID);
         serializer(action, raw, ref bitPosition,  2, ref TargetRaw);
         filler    (             ref bitPosition,  6);
         serializer(action, raw, ref bitPosition,  8, ref SWMajorRev);
         serializer(action, raw, ref bitPosition,  8, ref SWMinorRev);
         serializer(action, raw, ref bitPosition,  8, ref SWBuild);
         serializer(action, raw, ref bitPosition,  8, ref HWMajorRev);
         serializer(action, raw, ref bitPosition,  8, ref HWMinorRev);
         serializer(action, raw, ref bitPosition, 24, ref ScheduledProgrammingTimeUtcSeconds.Seconds);
      }

      public    Int64   ProgrammingEventID;
      private    byte    TargetRaw;
      public    byte    SWMajorRev;
      public    byte    SWMinorRev;
      public    byte    SWBuild;
      public    byte    HWMajorRev;
      public    byte    HWMinorRev;
      public    UtcSeconds  ScheduledProgrammingTimeUtcSeconds;

      public DeviceTarget Target 
      {
         get { return (DeviceTarget) TargetRaw; }
         set { TargetRaw = (byte) value; }
      }

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return ScheduledProgrammingTimeUtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return ScheduledProgrammingTimeUtcSeconds.Date; }
         set { ScheduledProgrammingTimeUtcSeconds.Date = value; }
      }

      #endregion

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(RFProgrammingAcknowledgementTrackerMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         RFProgrammingAcknowledgementTrackerMessage ack = new RFProgrammingAcknowledgementTrackerMessage();

         ack.SubType = RFProgrammingAcknowledgementTrackerMessage.AckType.ProgrammingAcknowledgement;
         ack.ProgrammingEventID   = ProgrammingEventID;
         ack.AcknowledgementValue = RFProgrammingAcknowledgementTrackerMessage.AckValue.ACK;
         ack.AcknowledgementOf    = RFProgrammingAcknowledgementTrackerMessage.AckOf.ProgrammingSchedule;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         return false;
      }

      #endregion
   }

   public class RFProgrammingStartBaseMessage : BaseMessage,
                                                INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x0F;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref ProgrammingEventID);
         serializer(action, raw, ref bitPosition,  2, ref TargetRaw);
         filler    (             ref bitPosition,  6);
         serializer(action, raw, ref bitPosition,  8, ref SWMajorRev);
         serializer(action, raw, ref bitPosition,  8, ref SWMinorRev);
         serializer(action, raw, ref bitPosition,  8, ref SWBuild);
         serializer(action, raw, ref bitPosition,  8, ref HWMajorRev);
         serializer(action, raw, ref bitPosition,  8, ref HWMinorRev);

         int numberOfCodeModules = (CodeModules == null) ? 0 : CodeModules.Length;

         serializer(action, raw, ref bitPosition,  8, ref numberOfCodeModules);
         serializer(action, raw, ref bitPosition, 16, ref RecordSize);

         // Pull in the CodeModule blocks.
         // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

         // If we are hydrating, we need to create the array first; this is only because the run-length of the
         // array doesn't appear immediately before the array itself (so the serializer uses the length of the
         // array to populate it since we state the length of the 'run length' is 0 bits).

         if (action == SerializationAction.Hydrate) 
         {
            CodeModules = (numberOfCodeModules > 0) ? new CodeModule[numberOfCodeModules] : null;
         }

         CodeModules = (CodeModule[])
            serializeHomogeneousRunLengthArray(action, raw, ref bitPosition,  0, CodeModules, typeof(CodeModule));
      }

      public class CodeModule : NestedMessage
      {
         public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
         {
            // CodeModule packets do not call the base serializer.  There is no 'packet ID' to
            // pick up since the type is implied by context.

            serializer(action, raw, ref bitPosition, 32, ref StartAddress);
            serializer(action, raw, ref bitPosition, 32, ref EndAddress);
            serializer(action, raw, ref bitPosition, 32, ref Crc);
            serializer(action, raw, ref bitPosition, 16, ref NumberOfRecords);
         }

         public UInt32     StartAddress;
         public UInt32     EndAddress;
         public UInt32     Crc;
         public UInt16     NumberOfRecords;
      }

      public    Int64         ProgrammingEventID;
      private    byte          TargetRaw;
      public    byte          SWMajorRev;
      public    byte          SWMinorRev;
      public    byte          SWBuild;
      public    byte          HWMajorRev;
      public    byte          HWMinorRev;

      public    UInt16        RecordSize;

      public    CodeModule[]  CodeModules;

      public DeviceTarget Target 
      {
         get { return (DeviceTarget) TargetRaw; }
         set { TargetRaw = (byte) value; }
      }

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(RFProgrammingAcknowledgementTrackerMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         RFProgrammingAcknowledgementTrackerMessage ack = new RFProgrammingAcknowledgementTrackerMessage();

         ack.SubType = RFProgrammingAcknowledgementTrackerMessage.AckType.ProgrammingAcknowledgement;
         ack.ProgrammingEventID   = ProgrammingEventID;
         ack.AcknowledgementValue = RFProgrammingAcknowledgementTrackerMessage.AckValue.ACK;
         ack.AcknowledgementOf    = RFProgrammingAcknowledgementTrackerMessage.AckOf.ProgrammingStart;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         return false;
      }

      #endregion
   }

   public class RFProgrammingDataBaseMessage : BaseMessage
   {
      public static new readonly int kPacketID = 0x10;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition,  8, ref ModuleNumber);
         serializer(action, raw, ref bitPosition, 16, ref RecordNumber);

         serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref Data);

         serializer(action, raw, ref bitPosition, 32, ref DataCrc);
      }

      public   byte     ModuleNumber;
      public   UInt16   RecordNumber;
      public   byte[]   Data;
      public   UInt32   DataCrc;
   }

   public class RFProgrammingNakRequestBaseMessage : BaseMessage,
                                                     INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x11;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // Now for my members

         serializer(action, raw, ref bitPosition, 32, ref ProgrammingEventID);
      }

      public    Int64  ProgrammingEventID;

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(RFProgrammingAcknowledgementTrackerMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         RFProgrammingAcknowledgementTrackerMessage ack = new RFProgrammingAcknowledgementTrackerMessage();

         ack.SubType = RFProgrammingAcknowledgementTrackerMessage.AckType.ProgrammingNakMap;
         ack.ProgrammingEventID   = ProgrammingEventID;
         ack.ModuleNumber         = 0;
         ack.RecordNak.Data    = new byte[0];   // Special case indicates a cancel

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         return false;
      }

      #endregion
   }

   public class RequestPersonalityMessage : BaseMessage
   {
     public static new readonly int kPacketID = 0x18;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);
     }
   }

   public class PersonalityReportAck : BaseMessage
   {
     public static new readonly int kPacketID = 0x19;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);
     }
   }


   public class GatewayMessageRequest : BaseMessage,
                                        INeedsAcknowledgement, BaseMessage.IBaseMessageSequenceID
   {
     public static new readonly int kPacketID = 0x1A;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public List<string> MessageTypeRequested;
     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);

       serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);

       byte count = MessageTypeRequested == null ? (byte)0 : (byte)MessageTypeRequested.Count;
       serializer(action, raw, ref bitPosition, 8, ref count);
       if (MessageTypeRequested == null)
         MessageTypeRequested = new List<string>();
       for (int i = 0; i < count; i++)
       {
         if (action == SerializationAction.Hydrate)
         {
           string messageType = string.Empty;
           serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref messageType);
           MessageTypeRequested.Add(messageType);
         }
         else
         {
           string messageType = MessageTypeRequested[i];
           serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref messageType);
         }
       }
     }
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

   public class VehicleBusMessageRequest : BaseMessage,
                                         INeedsAcknowledgement, BaseMessage.IBaseMessageSequenceID
   {
     public static new readonly int kPacketID = 0x1C;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public List<byte> MessageTypeRequested;
     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);

       serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);

       byte count = MessageTypeRequested == null ? (byte)0 : (byte)MessageTypeRequested.Count;
       serializer(action, raw, ref bitPosition, 8, ref count);
       if (MessageTypeRequested == null)
         MessageTypeRequested = new List<byte>();
       for (int i = 0; i < count; i++)
       {
         byte message = action == SerializationAction.Hydrate ? (byte)0 : MessageTypeRequested[i];
         serializer(action, raw, ref bitPosition, 8, ref message);
         if (action == SerializationAction.Hydrate)
         {
           MessageTypeRequested.Add(message);
         }
       }
     }
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
   #endregion
}
