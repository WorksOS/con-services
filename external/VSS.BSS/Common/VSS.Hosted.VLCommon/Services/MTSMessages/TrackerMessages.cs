using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using VSS.Hosted.VLCommon;
using Microsoft.Web.Script.Serialization;
using System.Collections.Generic;
using log4net;

namespace VSS.Hosted.VLCommon.MTSMessages
{
   public abstract class TrackerMessage : PlatformMessage
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.Tracker; }
      }

      public interface IDeviceSequenceID
      {
         byte    DevicePacketSequenceID { get; set; }
      }

      public interface ISiteID
      {
         Int64   SiteID   { get; set; }
      }

      public interface IDriverID
      {
         DeviceDriverIDType   DriverIDType      { get; set; }
         Int64                PlatformDriverID  { get; set; }
         string               MdtDriverID       { get; set; }
         string               DriverDisplayName { get; set; }
         bool                 IsDriverPresent   { get; set; }
      }

      public interface IUserDataContainer 
      {
         TrackerUserDataMessage     UserDataMessage { get; set; }
      }
   }

   public class NetEntryTrackerMessage : TrackerMessage, INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x00;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition, 30, ref DeviceID);
      }

      public Int64  DeviceID;
      
      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(SetIntervalsBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         return new SetIntervalsBaseMessage();
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         return ackMessage is SetIntervalsBaseMessage;
      }

      #endregion
   }

   public class NetEntryVersion2TrackerMessage : TrackerMessage, INeedsAcknowledgement
   {
     //This logger is only needed to track the firmware serialnumber encoding bug
     private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

     public static new readonly int kPacketID = 0x0D;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);
              
       List<char> charSet = new List<char> {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 
         'I', 'J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
         '0','1','2','3','4','5','6','7','8','9'};
       if (action == SerializationAction.Hydrate)
       {
         byte numBytes = 0;
         //The Legacy part of this message where the characters are 6 bits each is error prone with certain device ID lengths 
         //so it was obsoleted and replaced with a null terminated string and indicated by a 0 in the length field
         serializer(action, raw, ref bitPosition, 4, ref numBytes);
         if (numBytes > 0)
         {
           use6BitEncoding = true;
           StringBuilder stringBuilder = new StringBuilder();
           int numCharacters = (numBytes * 8) / 6;
           for (int i = 0; i < numCharacters; i++)
           {
             byte charecterNum = 0;
             serializer(action, raw, ref bitPosition, 6, ref charecterNum);
             stringBuilder.Append(charSet[charecterNum]);
           }

           deviceIDRaw = stringBuilder.ToString();
           if (bitPosition % 8 != 0)
             filler(ref bitPosition, 8 - (uint)((bitPosition) % 8));
         }
         else
         {
           use6BitEncoding = false;
           serializeNulTerminatedString(action, raw, ref bitPosition, ref deviceIDRaw);
         }
       }
       else
       {
         if (use6BitEncoding)
         {
           int numCharacters = deviceIDRaw.Length;
           byte numBytes = (byte)(((numCharacters * 6) + 8) / 8);

           serializer(action, raw, ref bitPosition, 4, ref numBytes);
           for (int i = 0; i < numCharacters; i++)
           {
             byte character = (byte)charSet.FindIndex(e => e == deviceIDRaw[i]);
             serializer(action, raw, ref bitPosition, 6, ref character);
           }

           if (bitPosition % 8 != 0)
             filler(ref bitPosition, 8 - (uint)((bitPosition) % 8));
         }
         else
         {
           byte numBytes = 0;
           serializer(action, raw, ref bitPosition, 4, ref numBytes);
           serializeNulTerminatedString(action, raw, ref bitPosition, ref deviceIDRaw);
         }
       }
     }

     private static readonly char noDeviceIDincluded = Convert.ToChar(2);

     public string DeviceID
     {
       get 
       { 
         //Due to Firmware bug there may by 0 padding on the end of the serial number wich is decoded as a set of 'A's. 
         //This code corrects for that it is temporary until it is fixed on the firmware side.
         if (deviceIDRaw.EndsWith("AAA"))
         {
           log.IfWarnFormat("Correcting Encoding Fault, Removing trailing A's from '{0}'", deviceIDRaw);

           int lastAAA = deviceIDRaw.LastIndexOf("AAA");
           string temp = deviceIDRaw.Substring(0, lastAAA);
           return temp;
         }
         else if (deviceIDRaw.Length == 1 && deviceIDRaw == noDeviceIDincluded.ToString())
         {
           return null;
         }

         return deviceIDRaw;
       }
       set { deviceIDRaw = value; }
     }

     private string deviceIDRaw;
     private bool use6BitEncoding = false;

     #region Implementation of INeedsAcknowledgement

     public Type AcknowledgementType
     {
       get { return typeof(SetIntervalsBaseMessage); }
     }

     public ProtocolMessage GenerateAcknowledgement()
     {
       return new SetIntervalsBaseMessage();
     }

     public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
     {
       return ackMessage is SetIntervalsBaseMessage;
     }

     #endregion
   }

   public class StateStatusTrackerMessage : TrackerMessage, ICoordinate,
                                                            TrackerMessage.IDeviceSequenceID,
                                                            IUtcTime,
                                                            INeedsAcknowledgement
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

         serializer(action, raw, ref bitPosition,  8, ref DevicePacketSequenceIDRaw);
         serializer(action, raw, ref bitPosition,  4, ref NetworkStatusCodeRaw);
         serializer(action, raw, ref bitPosition, 24, ref UtcSeconds.Seconds);
         serializer(action, raw, ref bitPosition, 24, ref LatitudeRaw);
         serializer(action, raw, ref bitPosition, 25, ref LongitudeRaw);
         serializer(action, raw, ref bitPosition,  7, ref SpeedRaw);
         serializer(action, raw, ref bitPosition,  8, ref HeadingRaw);

         // Pull in the condensed state & status blocks.
         // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

         CondensedDeviceStates = (CondensedDeviceState[])
            serializeHomogeneousRunLengthArray(action, raw, ref bitPosition,  8, CondensedDeviceStates, typeof(CondensedDeviceState));
      }

      public class CondensedDeviceState : NestedMessage, ISpeedHeading
      {
         public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
         {
            // CondensedDeviceState packets do not call the base serializer.  There is no 'packet ID' to
            // pick up since the type is implied by context.

            serializer(action, raw, ref bitPosition, 16, ref TimeSinceLastStateBlockSec);
            serializer(action, raw, ref bitPosition, 20, ref LatitudeDeltaRaw);
            serializer(action, raw, ref bitPosition, 20, ref LongitudeDeltaRaw);
            serializer(action, raw, ref bitPosition,  8, ref SpeedRaw);
            serializer(action, raw, ref bitPosition,  8, ref HeadingRaw);
         }

         public    UInt16  TimeSinceLastStateBlockSec;
         private Int32 LatitudeDeltaRaw;
         public Int32 LongitudeDeltaRaw;
         private byte SpeedRaw;
         private sbyte HeadingRaw;

         public Double  LatitudeDelta
         {
            get { return (double) LatitudeDeltaRaw*Constants.LatLongConversionMultiplier; }
            set { LatitudeDeltaRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
         }

         public Double  LongitudeDelta
         {
            get { return (double) LongitudeDeltaRaw*Constants.LatLongConversionMultiplier; }
            set { LongitudeDeltaRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
         }

         #region Implementation of ISpeedHeading
   
         public Double  Speed
         {
            get { return (double) SpeedRaw*Constants.SpeedConversionMultiplier; }
            set { SpeedRaw = (byte)(value/Constants.SpeedConversionMultiplier); }
         }

         public Double Heading 
         {
            get { return (double) HeadingRaw*Constants.HeadingConversionMultiplier; }
            set { HeadingRaw = (sbyte)(value/Constants.HeadingConversionMultiplier); }
         }

         #endregion
      }

      private    byte    DevicePacketSequenceIDRaw;
      private    byte    NetworkStatusCodeRaw;
      public    UtcSeconds  UtcSeconds;
      private Int32 LatitudeRaw;
      private Int32 LongitudeRaw;
      private byte SpeedRaw;
      private sbyte HeadingRaw;

      public CondensedDeviceState[] CondensedDeviceStates;

      public enum DeviceNetworkStatusCode 
      {
         NoStatus,
         NetworkExitRequest,
         LowPowerEntry,
         LowPowerExitRequest
      }

      public DeviceNetworkStatusCode NetworkStatusCode 
      {
         get { return (DeviceNetworkStatusCode) NetworkStatusCodeRaw; }
         set { NetworkStatusCodeRaw = (byte) value; }
      }

      #region Implementation of ICoordinate

      public Double  Latitude 
      {
         get { return (double) LatitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LatitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      public Double  Longitude
      {
         get { return (double) LongitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LongitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      #endregion
   
      #region Implementation of ISpeedHeading

      public Double  Speed
      {
         get { return (double) SpeedRaw*Constants.SpeedConversionMultiplier; }
         set { SpeedRaw = (byte)(value/Constants.SpeedConversionMultiplier); }
      }

      public Double Heading 
      {
         get { return (double) HeadingRaw*Constants.HeadingConversionMultiplier; }
         set { HeadingRaw = (sbyte)(value/Constants.HeadingConversionMultiplier); }
      }

      #endregion

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DevicePacketSequenceIDRaw; }
         set { DevicePacketSequenceIDRaw = value; }
      }

      #endregion

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(DeviceStateStatusAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         DeviceStateStatusAckBaseMessage ack = new DeviceStateStatusAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         DeviceStateStatusAckBaseMessage ack = ackMessage as DeviceStateStatusAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
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

   public class UserDataWithLocationTrackerMessage : TrackerMessage, ICoordinate,
                                                                     TrackerMessage.ISiteID,
                                                                     IMileage,
                                                                     TrackerMessage.IDeviceSequenceID,
                                                                     TrackerMessage.IUserDataContainer,
                                                                     IUtcTime,
                                                                     INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x02;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  8, ref DevicePacketSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
         serializer(action, raw, ref bitPosition, 24, ref LatitudeRaw);
         serializer(action, raw, ref bitPosition, 25, ref LongitudeRaw);
         serializer(action, raw, ref bitPosition, 20, ref UtcSeconds.Seconds);
         serializer(action, raw, ref bitPosition, 24, ref MileageRaw);

         serializeSingleEmbeddedMessage(action, raw, ref bitPosition, 16, ref Message);
      }

      private    byte    DevicePacketSequenceIDRaw;
      private Int64 SiteIDRaw;
      private Int32 LatitudeRaw;
      private Int32 LongitudeRaw;
      public    UtcSeconds  UtcSeconds;
      private UInt32 MileageRaw;
      public    TrackerUserDataMessage  Message;

      #region Implementation of ICoordinate

      public Double  Latitude 
      {
         get { return (double) LatitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LatitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      public Double  Longitude
      {
         get { return (double) LongitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LongitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      #endregion

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DevicePacketSequenceIDRaw; }
         set { DevicePacketSequenceIDRaw = value; }
      }

      #endregion

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDRaw; }
         set { SiteIDRaw = value; }
      }

      #endregion

      #region Implementation of IMileage

      public Double  Mileage
      {
         get { return ((double) MileageRaw)/Constants.MileageConversionMultiplier; }
         set { MileageRaw = (UInt32)(value*Constants.MileageConversionMultiplier); }
      }

      #endregion

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(UserDataAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         UserDataAckBaseMessage ack = new UserDataAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         UserDataAckBaseMessage ack = ackMessage as UserDataAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
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

      #region IUserDataContainer Members

      [XmlIgnore]
      public TrackerUserDataMessage UserDataMessage
      {
         get { return Message; }
         set { Message = value; }
      }

      #endregion
   }

   public class UserDataTrackerMessage : TrackerMessage, TrackerMessage.IUserDataContainer
   {
      public static new readonly int kPacketID = 0x03;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializeSingleEmbeddedMessage(action, raw, ref bitPosition, 16, ref Message);
      }

      #region IUserDataContainer Members

      [XmlIgnore]
      public TrackerUserDataMessage UserDataMessage
      {
         get { return Message; }
         set { Message = value; }
      }

      #endregion

      public TrackerUserDataMessage  Message;
   }

   public class MessageResponseTrackerMessage : TrackerMessage,
                                                IUtcTime,
                                                BaseMessage.IBaseMessageSequenceID,
                                                INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x04;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  1, ref ResponseFlag);
         serializer(action, raw, ref bitPosition,  3, ref ResponseKey);
         serializer(action, raw, ref bitPosition, 32, ref MessageSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 20, ref MessageResponseUtcSeconds.Seconds);
      }

      public    bool                ResponseFlag;
      public    UInt16              ResponseKey;
      private Int64 MessageSequenceIDRaw;
      public    UtcSeconds  MessageResponseUtcSeconds;

      #region Implementation of IBaseMessageSequenceID

      public Int64 BaseMessageSequenceID
      {
         get { return MessageSequenceIDRaw; }
         set { MessageSequenceIDRaw = value; }
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return MessageResponseUtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return MessageResponseUtcSeconds.Date; }
         set { MessageResponseUtcSeconds.Date = value; }
      }

      #endregion

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return ResponseFlag ? typeof(MessageResponseAckBaseMessage) : null; }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         if (!ResponseFlag) 
         {
            return null;
         }

         MessageResponseAckBaseMessage ack = new MessageResponseAckBaseMessage();

         ack.ResponseKeyID         = ResponseKey;
         ack.BaseMessageSequenceID = BaseMessageSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         if (!ResponseFlag) 
         {
            return false;
         }

         MessageResponseAckBaseMessage ack = ackMessage as MessageResponseAckBaseMessage;

         return ack != null  &&  ack.BaseMessageSequenceID == BaseMessageSequenceID;
      }

      #endregion
   }

   public class SiteStatusTrackerMessage : TrackerMessage, ICoordinate,
                                                           TrackerMessage.IDeviceSequenceID,
                                                           TrackerMessage.ISiteID,
                                                           IMileage,
                                                           IUtcTime,
                                                           INeedsAcknowledgement,
                                                           IDeprecated
   {
      public static new readonly int kPacketID = 0x05;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  2, ref SiteTypeRaw);
         serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
         serializer(action, raw, ref bitPosition, 24, ref LatitudeRaw);
         serializer(action, raw, ref bitPosition, 25, ref LongitudeRaw);
         serializer(action, raw, ref bitPosition, 20, ref EventTimeUtcSeconds.Seconds);
         serializer(action, raw, ref bitPosition, 24, ref MileageRaw);
         serializer(action, raw, ref bitPosition,  1, ref StatusRaw);
         serializer(action, raw, ref bitPosition,  1, ref AutomaticSourceFlag);
         serializer(action, raw, ref bitPosition,  1, ref UserSourceFlag);
         serializer(action, raw, ref bitPosition,  8, ref DevicePacketSequenceIDRaw);
      }

      private    UInt16  SiteTypeRaw;
      private    Int64   SiteIDRaw;
      private    Int32   LatitudeRaw;
      private    Int32   LongitudeRaw;
      public    UtcSeconds  EventTimeUtcSeconds;
      private UInt32 MileageRaw;
      private UInt16 StatusRaw;
      public    bool    AutomaticSourceFlag;
      public    bool    UserSourceFlag;
      private    byte    DevicePacketSequenceIDRaw;

      public DeviceSiteType SiteType 
      {
         get { return (DeviceSiteType) SiteTypeRaw; }
         set { SiteTypeRaw = (UInt16) value; }
      }

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDRaw; }
         set { SiteIDRaw = value; }
      }

      #endregion

      #region Implementation of ICoordinate

      public Double  Latitude 
      {
         get { return (double) LatitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LatitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      public Double  Longitude
      {
         get { return (double) LongitudeRaw*Constants.LatLongConversionMultiplier; }
         set { LongitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
      }

      #endregion

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DevicePacketSequenceIDRaw; }
         set { DevicePacketSequenceIDRaw = value; }
      }

      #endregion

      #region Implementation of IMileage

      public Double  Mileage
      {
         get { return ((double) MileageRaw)/Constants.MileageConversionMultiplier; }
         set { MileageRaw = (UInt32)(value*Constants.MileageConversionMultiplier); }
      }

      #endregion

      public enum DeviceStatus 
      {
         Arrival,
         Departure
      }

      public DeviceStatus Status 
      {
         get { return (DeviceStatus) StatusRaw; }
         set { StatusRaw = (UInt16) value; }
      }

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(SiteStatusAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         SiteStatusAckBaseMessage ack = new SiteStatusAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         SiteStatusAckBaseMessage ack = ackMessage as SiteStatusAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
      }

      #endregion

      #region Implementation of IDeprecated

      public string DeprecationIndication
      {
         get { return "Deprecated as of March 3, 2004 per Greg Kremer"; }
      }

      #endregion

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return EventTimeUtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return EventTimeUtcSeconds.Date; }
         set { EventTimeUtcSeconds.Date = value; }
      }

      #endregion
   }

   public class RFProgrammingAcknowledgementTrackerMessage : TrackerMessage 
   {
      public static new readonly int kPacketID = 0x08;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  4, ref SubTypeRaw);
         serializer(action, raw, ref bitPosition, 32, ref ProgrammingEventID);

         if (SubType == AckType.ProgrammingAcknowledgement) 
         {
            serializer(action, raw, ref bitPosition,  3, ref AcknowledgementValueRaw);
            serializer(action, raw, ref bitPosition,  1, ref AcknowledgementOfRaw);
         } 
         else if (SubType == AckType.ProgrammingNakMap) 
         {
            serializer(action, raw, ref bitPosition,  8, ref ModuleNumber);
            serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref RecordNak.Data);
         }
      }

      public enum AckType 
      {
         ProgrammingAcknowledgement,
         ProgrammingNakMap
      }

      public enum AckValue
      {
         ACK,
         NAK,
         Cancel,
         CancelBankInUse,
         CancelBankNotErased
      }

      public enum AckOf 
      {
         ProgrammingSchedule,
         ProgrammingStart
      }

      private byte SubTypeRaw;
      public   Int64    ProgrammingEventID;

      // For ProgrammingAcknowledgement

      private byte AcknowledgementValueRaw;
      private byte AcknowledgementOfRaw;

      // For ProgrammingNakMap

      public   byte           ModuleNumber;
      public BitArray RecordNak;

      public   AckType  SubType 
      {
         get { return (AckType) SubTypeRaw; }
         set { SubTypeRaw = (byte) value; }
      }

      public   AckValue AcknowledgementValue
      {
         get { return (AckValue) AcknowledgementValueRaw; }
         set { AcknowledgementValueRaw = (byte) value; }
      }

      public   AckOf    AcknowledgementOf
      {
         get { return (AckOf) AcknowledgementOfRaw; }
         set { AcknowledgementOfRaw = (byte) value; }
      }
   }

   public class KeepAliveTrackerMessage : TrackerMessage 
   {
      public static new readonly int kPacketID = 0x09;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);
      }
   }

   public abstract class EnhancedEventHeaderTrackerMessage : TrackerMessage, ICoordinate,
                                                                             TrackerMessage.IDriverID,
                                                                             TrackerMessage.IDeviceSequenceID,
                                                                             IMileage,
                                                                             TrackerMessage.ISiteID,
                                                                             IUtcTime
   {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  1, ref SiteIDIncluded);
         serializer(action, raw, ref bitPosition,  1, ref DriverIDIncluded);
         filler    (             ref bitPosition,  2);
         serializer(action, raw, ref bitPosition,  8, ref DevicePacketSequenceIDRaw);
         serializer(action, raw, ref bitPosition, 24, ref DistanceTraveledRaw);
         serializer(action, raw, ref bitPosition, 20, ref EventTimeUtcSeconds.Seconds);
         serializer(action, raw, ref bitPosition,  2, ref LocationSpeedHeadingIncludedRaw);
         serializer(action, raw, ref bitPosition,  2, ref LocationAgeUncertaintyIncludedRaw);

         if (LocationSpeedHeadingIncluded != DeviceLocationQuality.Nothing) 
         {
            serializer(action, raw, ref bitPosition,  8, ref SpeedRaw);
         }

         if (LocationSpeedHeadingIncluded == DeviceLocationQuality.SpeedHeadingLocation) 
         {
            serializer(action, raw, ref bitPosition, 24, ref LatitudeRaw);
            serializer(action, raw, ref bitPosition, 25, ref LongitudeRaw);
            serializer(action, raw, ref bitPosition,  7, ref HeadingRaw);
         }

         if (LocationAgeUncertaintyIncluded >= DeviceAgeUncertainty.LocationOldAgeIncluded)
         {
            serializer(action, raw, ref bitPosition, 16, ref LocationAgeIndicatorRaw);
         }
         
         if (LocationAgeUncertaintyIncluded >= DeviceAgeUncertainty.LocationOldAgeUncertaintyIncluded)
         {
            serializer(action, raw, ref bitPosition, 12, ref LocationUncertaintyIndicatorRaw);
            serializer(action, raw, ref bitPosition,  2, ref LocationUncertaintyIndicatorScalingRaw);
            serializer(action, raw, ref bitPosition,  2, ref LocationUncertaintyIndicatorSourceRaw);
         }

         if (SiteIDIncluded) 
         {
            serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
         }

         if (DriverIDIncluded) 
         {
            serializeDriverID(action, raw, ref bitPosition, this);
         }
      }

      private    byte    DevicePacketSequenceIDRaw;

      public    bool    SiteIDIncluded;
      private Int64 SiteIDRaw;

      public    bool    DriverIDIncluded;
      private byte DriverIDTypeRaw;
      private Int64 PlatformDriverIDRaw;
      private string MdtDriverIDRaw;
      public    bool    DriverPresent;

      private UInt32 DistanceTraveledRaw;
      public    UtcSeconds  EventTimeUtcSeconds;
      private byte LocationSpeedHeadingIncludedRaw;
      private byte LocationAgeUncertaintyIncludedRaw;

      private byte SpeedRaw;
      private sbyte HeadingRaw;

      private Int32 LatitudeRaw;
      private Int32 LongitudeRaw;

      private Int16 LocationAgeIndicatorRaw;
      private UInt16 LocationUncertaintyIndicatorRaw;
      private byte LocationUncertaintyIndicatorScalingRaw;
      private byte LocationUncertaintyIndicatorSourceRaw;

      #region Implementation of IDeviceSequenceID

      public byte DevicePacketSequenceID
      {
         get { return DevicePacketSequenceIDRaw; }
         set { DevicePacketSequenceIDRaw = value; }
      }

      #endregion

      #region Implementation of ISiteID

      public Int64 SiteID 
      {
         get { return SiteIDIncluded ? SiteIDRaw : unchecked((Int64)(-1)); }
         set { SiteIDIncluded = true; SiteIDRaw = value; }
      }

      #endregion

      #region Implementation of IDriverID

      public DeviceDriverIDType DriverIDType
      {
         get { return (DeviceDriverIDType) DriverIDTypeRaw; }
         set { DriverIDIncluded = true; DriverIDTypeRaw = (byte) value; }
      }

      public Int64  PlatformDriverID 
      {
         get { return PlatformDriverIDRaw; }
         set { DriverIDIncluded = true; DriverIDType = DeviceDriverIDType.PlatformDriverID; PlatformDriverIDRaw = value; }
      }

      public string  MdtDriverID 
      {
         get { return MdtDriverIDRaw; }
         set { DriverIDIncluded = true; DriverIDType = DeviceDriverIDType.MdtDriverID; MdtDriverIDRaw = value; }
      }

      public string DriverDisplayName
      {
         get { return null; }
         set { throw new ApplicationException("EnhancedEventHeader does not have a DriverDisplayName"); }
      }

      public bool IsDriverPresent
      {
         get { return DriverPresent; }
         set { DriverPresent = value;}
      }

      #endregion

      public Double  DistanceTraveled 
      {
         get { return (double) DistanceTraveledRaw*Constants.EUDDistanceTraveledConversionMultiplier; }
         set { DistanceTraveledRaw = (UInt32)(value/Constants.EUDDistanceTraveledConversionMultiplier); }
      }

      #region Implementation of IMileage

      public Double  Mileage
      {
         get { return DistanceTraveled; }
         set { DistanceTraveled = value; }
      }

      #endregion

      #region Implementation of ISpeedHeading

      public Double  Speed
      {
         get { return (double) SpeedRaw*Constants.SpeedConversionMultiplier; }
         set 
         {
            if (LocationSpeedHeadingIncluded == DeviceLocationQuality.Nothing) 
            {
               // For the speed to be included, LocationSpeedHeadingIncluded must be at least .SpeedOnly
               LocationSpeedHeadingIncluded = DeviceLocationQuality.SpeedOnly;
            }
            SpeedRaw = (byte)(value/Constants.SpeedConversionMultiplier);
         }
      }

      public Double Heading 
      {
         get { return (double) HeadingRaw*Constants.HeadingConversionMultiplier; }
         set
         {
            LocationSpeedHeadingIncluded = DeviceLocationQuality.SpeedHeadingLocation;
            HeadingRaw = (sbyte)(value/Constants.HeadingConversionMultiplier);
         }
      }

      #endregion

      #region Implementation of ICoordinate

      public Double  Latitude 
      {
         get { return (double) LatitudeRaw*Constants.LatLongConversionMultiplier; }
         set 
         {
            LocationSpeedHeadingIncluded = DeviceLocationQuality.SpeedHeadingLocation;
            LatitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier);
         }
      }

      public Double  Longitude
      {
         get { return (double) LongitudeRaw*Constants.LatLongConversionMultiplier; }
         set 
         {
            LocationSpeedHeadingIncluded = DeviceLocationQuality.SpeedHeadingLocation;
            LongitudeRaw = (Int32)(value/Constants.LatLongConversionMultiplier);
         }
      }

      #endregion

      public DeviceUncertaintySource LocationUncertaintyIndicatorSource 
      {
         get { return (DeviceUncertaintySource) LocationUncertaintyIndicatorSourceRaw; }
         set
         {
            LocationAgeUncertaintyIncluded = DeviceAgeUncertainty.LocationOldAgeUncertaintyIncluded;
            LocationUncertaintyIndicatorSourceRaw = (byte) value;
         }
      }

      public    Int16  LocationAgeIndicator
      {
         get { return LocationAgeIndicatorRaw; }
         set 
         {
            if (LocationAgeUncertaintyIncluded < DeviceAgeUncertainty.LocationOldAgeIncluded) 
            {
               LocationAgeUncertaintyIncluded = DeviceAgeUncertainty.LocationOldAgeIncluded;
            }
            LocationAgeIndicatorRaw = value;
         }
      }

      public    UInt16  LocationUncertaintyIndicator
      {
         get { return LocationUncertaintyIndicatorRaw; }
         set
         {
            LocationAgeUncertaintyIncluded = DeviceAgeUncertainty.LocationOldAgeUncertaintyIncluded;
            LocationUncertaintyIndicatorRaw = value;
         }
      }

      public    DeviceIndicatorScaling    LocationUncertaintyIndicatorScaling
      {
         get { return (DeviceIndicatorScaling) LocationUncertaintyIndicatorScalingRaw; }
         set
         {
            LocationAgeUncertaintyIncluded = DeviceAgeUncertainty.LocationOldAgeUncertaintyIncluded;
            LocationUncertaintyIndicatorScalingRaw = (byte) value;
         }
      }

      public DeviceLocationQuality LocationSpeedHeadingIncluded
      {
         get { return (DeviceLocationQuality) LocationSpeedHeadingIncludedRaw; }
         set { LocationSpeedHeadingIncludedRaw = (byte) value; }
      }

      public DeviceAgeUncertainty LocationAgeUncertaintyIncluded
      {
         get { return (DeviceAgeUncertainty) LocationAgeUncertaintyIncludedRaw; }
         set { LocationAgeUncertaintyIncludedRaw = (byte) value; }
      }

      #region Implementation of IUtcTime

      public DateTime  DateRelativeToReceiveTime(DateTime receiveTime)
      {
         return EventTimeUtcSeconds.GetDateRelativeToAnother(receiveTime);
      }

      public DateTime    UtcDateTime 
      {
         get { return EventTimeUtcSeconds.Date; }
         set { EventTimeUtcSeconds.Date = value; }
      }

      #endregion
   }

   public class EnhancedUserDataTrackerMessage : EnhancedEventHeaderTrackerMessage,
                                                 TrackerMessage.IUserDataContainer,
                                                 INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x0A;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         // Recall that we are derived off EnhancedEventHeaderTrackerMessage whose Serialize() does
         // the enhanced event header.

         base.Serialize(action, raw, ref bitPosition);

         serializeSingleEmbeddedMessage(action, raw, ref bitPosition, 16, ref InnerMessage);
      }

      public TrackerUserDataMessage  InnerMessage;

      #region IUserDataContainer Members

      [XmlIgnore]
      public TrackerUserDataMessage UserDataMessage
      {
         get { return InnerMessage; }
         set { InnerMessage = value; }
      }

      #endregion

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(UserDataAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         UserDataAckBaseMessage ack = new UserDataAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         UserDataAckBaseMessage ack = ackMessage as UserDataAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
      }

      #endregion
   }

   public class DriverStatusTrackerMessage : EnhancedEventHeaderTrackerMessage,
                                             INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x0B;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         // Recall that we are derived off EnhancedEventHeaderTrackerMessage whose Serialize() does
         // the enhanced event header.

         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  1, ref LoginAction);
         filler    (             ref bitPosition,  7);
      }

      public bool LoginAction;

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(DriverStatusAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         DriverStatusAckBaseMessage ack = new DriverStatusAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         DriverStatusAckBaseMessage ack = ackMessage as DriverStatusAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
      }

      #endregion
   }

   public class Position2TrackerMessage : EnhancedEventHeaderTrackerMessage,
                                          INeedsAcknowledgement
   {
      public static new readonly int kPacketID = 0x0C;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         // You know, I'm really really tired of the wireless guys not being able to be consistent about things no
         // matter how much I lament.  So naturally the length of the embedded position array has padding -after-
         // it. I want the stupid count right before the item.  Make for consistent parsing.  Whatever.

         uint count = (CondensedDeviceStates == null) ? 0 : (uint) CondensedDeviceStates.Length;

         serializer(action, raw, ref bitPosition,  6,  ref count);
         filler    (             ref bitPosition,  2);

         // Pull in the condensed state & status blocks.
         // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

         if (action == SerializationAction.Hydrate) 
         {
            CondensedDeviceStates = new CondensedDeviceState2[count];
         }

         CondensedDeviceStates = (CondensedDeviceState2[])
            serializeHomogeneousRunLengthArray(action, raw, ref bitPosition,  0, CondensedDeviceStates, typeof(CondensedDeviceState2));
      }

      public class CondensedDeviceState2 : NestedMessage, ISpeedHeading
      {
         public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
         {
            // CondensedDeviceState packets do not call the base serializer.  There is no 'packet ID' to
            // pick up since the type is implied by context.

            serializer(action, raw, ref bitPosition, 12, ref TimeSinceLastStateBlockSec);
            serializer(action, raw, ref bitPosition, 17, ref LatitudeDeltaRaw);
            serializer(action, raw, ref bitPosition, 17, ref LongitudeDeltaRaw);
            serializer(action, raw, ref bitPosition, 10, ref DistanceDeltaRaw);
            serializer(action, raw, ref bitPosition,  8, ref SpeedRaw);
            serializer(action, raw, ref bitPosition,  7, ref HeadingRaw);
            filler    (             ref bitPosition,  1);
         }

         public    UInt16  TimeSinceLastStateBlockSec;
         private    Int32   LatitudeDeltaRaw;
         private Int32 LongitudeDeltaRaw;
         private UInt16 DistanceDeltaRaw;
         private byte SpeedRaw;
         private sbyte HeadingRaw;

         public Double  LatitudeDelta
         {
            get { return (double) LatitudeDeltaRaw*Constants.LatLongConversionMultiplier; }
            set { LatitudeDeltaRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
         }

         public Double  LongitudeDelta
         {
            get { return (double) LongitudeDeltaRaw*Constants.LatLongConversionMultiplier; }
            set { LongitudeDeltaRaw = (Int32)(value/Constants.LatLongConversionMultiplier); }
         }

         public Double  MileageDelta
         {
            get { return (double) DistanceDeltaRaw*Constants.EUDDistanceTraveledConversionMultiplier; }
            set { DistanceDeltaRaw = (UInt16)(value/Constants.EUDDistanceTraveledConversionMultiplier); }
         }

         #region Implementation of ISpeedHeading
   
         public Double  Speed
         {
            get { return (double) SpeedRaw*Constants.SpeedConversionMultiplier; }
            set { SpeedRaw = (byte)(value/Constants.SpeedConversionMultiplier); }
         }

         public Double Heading 
         {
            get { return (double) HeadingRaw*Constants.HeadingConversionMultiplier; }
            set { HeadingRaw = (sbyte)(value/Constants.HeadingConversionMultiplier); }
         }

         #endregion
      }

      public CondensedDeviceState2[] CondensedDeviceStates;

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(DeviceStateStatusAckBaseMessage); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
         DeviceStateStatusAckBaseMessage ack = new DeviceStateStatusAckBaseMessage();

         ack.DevicePacketSequenceID = DevicePacketSequenceID;

         return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         DeviceStateStatusAckBaseMessage ack = ackMessage as DeviceStateStatusAckBaseMessage;

         return ack != null  &&  ack.DevicePacketSequenceID == DevicePacketSequenceID;
      }

      #endregion
      
   }
}
