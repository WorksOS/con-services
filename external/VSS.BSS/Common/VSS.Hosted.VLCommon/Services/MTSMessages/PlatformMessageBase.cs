using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using VSS.Hosted.VLCommon;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.PLMessages;

namespace VSS.Hosted.VLCommon.MTSMessages
{
   /// <summary>
   /// Summary description for PlatformMessages.
   /// </summary>
   public abstract class PlatformMessage : ProtocolMessage
   {
      #region Constants and Enumerations

      public enum MessageCategory 
      {
         /// <summary>Category for all base messages (i.e., to the device).</summary>
         Base,
         /// <summary>Category for all tracker messages (i.e., from the device).</summary>
         Tracker,
         /// <summary>Category for the payload of any of the 3 tracker user-data messages.</summary>
         TrackerUserDataPayload,
         /// <summary>Category for the payload of base (platform) user-data message.</summary>
         BaseUserDataPayload,
         /// <summary>Category for locally defined nested arrays of messages like CondensedDeviceState.</summary>
         Nested,
         /// <summary>Category for embedded messages in the BitConfigurationTrackerMessage message.</summary>
         BitConfigurationBlockPayload,

         /// <summary>Category for supporting this class hierarchy, such as UnknownPlatformMessage.</summary>
         InternalSupport,

         /// <summary>Category for embedded messages in the MachineEventHeader message.</summary>
         MachineEventBlock,

         /// <summary>Category for embedded messages in the MachineEventBlock message.</summary>
         MachineEventBlockPayload,

         MachineEventBlockRadioPayload,

         MachineEventBlockGatewayPayload,
         
         MachineEventBlockVehicleBusPayload,
         /// <summary>Category for PL Device messages.</summary>
         PLTrackerMessage,

         /// <summary>Category for PL Base messages.</summary>
         PLBaseMessage,

         /// <summary>Numerical value indicates the number of enumerators in the enumeration excluding itself.</summary>
         MessageCategoryCount
      }

      #region Common IDs used across multiple messages

      // The following enumerations are used in numerous messages so they were collected here.

      public enum AggRMTimeGeoStamping
      {
         Now                                = 0,
         WeightStartedIncreasing            = 1,
         WeightIncreaseStopped              = 2,
         WeightReachedEmpty                 = 3,
         DrumChangedToCharge                = 4,
         DrumChangedToFastCharge            = 5,
         DrumFastChargeStopped              = 6,
         DrumFastDischargeStopped           = 7,
         DrumChangedToFastDischarge         = 8,
         DrumTurnedJustEnoughToStartPouring = 9,
         WashOutStarted                     = 10,
         VehicleSpeedContinuouslyAbove5Mph  = 11,
         WeightDecreaseStopped              = 12,
         VehicleEnteredAJobSite             = 13,
         VehicleLeftAJobSite                = 14,
         ContainerClosed                    = 15,
         ContainerOpened                    = 16,
         EnteredALoadingZone                = 17,
         LeftALoadingZone                   = 18,
         SiteStatus                         = 19,
         TimeOfLift                         = 20,
         AirBrakeActivated                  = 21,
         VehicleStop                        = 22,
         WashingStop                        = 23,
         SlumperStatusUpdate                = 24,
         SlumperDrumNotEmpty                = 25,
         SlumperDrumEmpty                   = 26,
         OperatorKeypressOnMdt              = 27 // part of PBS (Push Button Statusing)
      }

      public enum AggRMEventReason
      {
         ReasonUnknown              = 0,
         WeightIncreaseOrDecrease   = 1,
         WeightRateOfChange         = 2,
         WeightReachedEmpty         = 3,
         DrumSpeedAndOrDirection    = 4,
         Geographic                 = 5,
         MultipleReasons            = 6,
         WashoutSensor              = 7,
         VehicleSpeed               = 8,
         Ignition                   = 9,
         JobRelatedEventOccurred    = 10,
         ContainerOpenedOrClosed    = 11,
         LiftDetected               = 12,
         EnteredOrLeftALoadingZone  = 13,
         SlumperReportsStatusChange = 14,
         ManualOperatorEntry        = 15  // part of PBS (Push Button Statusing)
      }

      public enum DeviceSiteType 
      {
         Invalid,    // CustomerDefined,
         Home,
         Job,
         CustomerDefined
      }

      public enum DeviceUncertaintySource
      {
         aPriori,
         SensorBased
      }

      public enum DeviceIndicatorScaling 
      {
         Meters,
         Decameters,
         Hectometers,
         Kilometers
      }

      public enum DeviceDriverIDType
      {
         PlatformDriverID,
         MdtDriverID,
         RfidIButton
      }

      public enum DeviceLocationQuality 
      {
         Nothing,
         SpeedOnly,
         SpeedHeadingLocation
      }

      public enum DeviceAgeUncertainty
      {
         NotIncluded,
         LocationCurrent,  // Implies age/uncertainty aren't included
         LocationOldAgeIncluded,  // Location is old; age is returned
         LocationOldAgeUncertaintyIncluded   // Location is old; both age and uncertainty are returned
      }

      public enum DeviceDriverIDAction 
      {
         Validate       = 0x00,
         Invalidate     = 0x01,
         AckOnly        = 0x02,
         InvalidateAll  = 0x03
      }

      public enum DeviceTarget 
      {
         DeviceCPU,
         MDT,
         Application // Preliminary; Greg K is working on it now [PMSO:6-Oct-2005]
      }

      #endregion

      public class Constants 
      {
         // These multipliers are applied to the raw value from the device to convert to the client data type.

         public static readonly double  LatLongConversionMultiplier = Math.Pow(2.0, -16.0);
         public static readonly double  SiteDispatchLatLongConversionMultiplier = 180.0*Math.Pow(2.0, -23.0);
         public static readonly double  HeadingConversionMultiplier = 360.0/Math.Pow(2.0, 7.0);
         public static readonly double  SpeedConversionMultiplier   = 1.118468151; // Converts from raw to mph (use 0.5 for m/s)
         public static readonly double  MileageConversionMultiplier = 10.0;
         public static readonly double  EUDDistanceTraveledConversionMultiplier = 0.062137119;
         public static readonly double  DistanceTraveledInTenthsConversionMultiplier = 0.1;
         public static readonly double  BatteryVoltageConversionMultiplier = 150.0;
         public static readonly double  HdopConversionMultiplier = 0.1;
         public static readonly double  RemainingMaterialMultiplier = 0.01;
         public static readonly double  ConcretePresentMultiplier = 0.25;
         public static readonly double  ReadyMixWeightMultiplier = 10.0;
         public static readonly double  AggregateEventWeightMultiplier = 4.0;
         public static readonly double  OrderBackMaterialMultiplier = 0.01;
         public static readonly double  FuelOilAddedMultiplier = 0.1;
         public static readonly double  TirePressureMultiplier = 1;
         public static readonly double  TireTemperatureMultiplier = 0.03125;
         public static readonly double TireTemperatureOffSet = - 273;
      }

      public static readonly string[] kAggRMTimeGeoStampingText = new string[] {
                                                                                  "Now",      
                                                                                  "Weight started increasing",
                                                                                  "Weight increase stopped",
                                                                                  "Weight reached \"empty\"",
                                                                                  "Drum changed to charge",
                                                                                  "Drum changed to Fast Charge",
                                                                                  "Drum Fast Charge stopped",  
                                                                                  "Drum Fast Discharge stopped",
                                                                                  "Drum changed to Fast Discharge",
                                                                                  "Drum turned just enough to start pouring",
                                                                                  "Wash Out started",
                                                                                  "Vehicle speed continuously above 5 MPH",
                                                                                  "Weight decrease stopped",
                                                                                  "Vehicle entered a Job Site",
                                                                                  "Vehicle left a Job Site",
                                                                                  "Container Closed",
                                                                                  "Container Opened",
                                                                                  "Entered a Loading Zone",
                                                                                  "Left a Loading Zone",
                                                                                  "Site Status",
                                                                                  "Time of lift",
                                                                                  "Air Brake Activated",
                                                                                  "Vehicle Stop",
                                                                                  "Washing Stop",
                                                                                  "First Slump reported by autoslumper",
                                                                                  "Drum Not Empty reported by autoslumper",
                                                                                  "Drum Empty reported by autoslumper",
                                                                                  "Operator Keypress on MDT"
                                                                               };

      public static readonly string[] kAggRMEventReasonText = new string[] {
                                                                              "Reason unknown or undefined",
                                                                              "Weight increase or decrease",
                                                                              "Weight rate of change",
                                                                              "Weight reached \"empty\"",
                                                                              "Drum speed and/or direction",
                                                                              "Geographic",
                                                                              "Multiple reasons",
                                                                              "Washout sensor",
                                                                              "Vehicle speed",
                                                                              "Ignition",
                                                                              "A Job related event occurred",
                                                                              "Container opened or closed",
                                                                              "Lift Detected",
                                                                              "Entered or left a Loading Zone",
                                                                              "Slumper Reports Status Change",
                                                                              "Manual Operator Entry"
                                                                           };

      #endregion

      public abstract MessageCategory Category 
      {
         get;
      }

      public virtual int PacketID 
      {
         get { return kPacketID; }     // Boilerplate implementation; but you need this in your class.
      }

      public MessageHydrationErrors  HydrationErrors 
      {
         get { return hydrationErrors; }
      }

      public uint ReportedMessageLength
      {
         get { return reportedMessageLength; }
      }

      #region Public Serialize Methods

      public virtual void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         // All device packets start with the packet type ID.  That's all we store in this base
         // Serialize implementation.  We have a special implementation here since the packet
         // type begets the type of this class.  If the action is to serialize, we simply store
         // the packet type ID to the raw array.  If we are hydrating, we pull the value into
         // a temporary variable and compare it against the packet type ID of this class instance.
         // If they do not match, throw an exception.

         if (PacketID < 0) 
         {
            // The packet ID is -1. This means this packet type does not have a packet ID in the
            // serialized format.  Simply return.

            return;
         }

         uint packetID = (uint) PacketID;
         uint packetIDBitLength = (Category == MessageCategory.Tracker) ? 4u : 8u;

         if (action == SerializationAction.Serialize) 
         {
            // Note that tracker messages put the packet ID just in the first nibble.  Base messages and
            // user-data messages put them in the first complete byte.

            serializer(action, raw, ref bitPosition, packetIDBitLength, ref packetID);
         } 
         else if (action == SerializationAction.CalculateLength) 
         {

            bitPosition += packetIDBitLength;
         }
         else 
         {
            // Otherwise, verify it is the same.

            serializer(SerializationAction.Hydrate, raw, ref bitPosition, packetIDBitLength, ref packetID);

            if (packetID != (uint) PacketID) 
            {
               throw new ApplicationException(
                  String.Format("An instance of type {0} attempted to hydrate a packet of type ID {1} (expected {2})",
                  this.GetType().ToString(), packetID, PacketID));
            }
         }
      }


      public static byte[] SerializeNestedMessages(NestedMessage[] msgs)
      {
         // I use this to serialize the nested arrays of nested messages (e.g., condensed status, and several
         // in the BIT messages).

         if (msgs == null) 
         {
            return null;
         }

         uint bitPosition = 0;

         foreach (NestedMessage nm in msgs) 
         {
            nm.Serialize(SerializationAction.CalculateLength, null, ref bitPosition);
         }

         byte[] raw = new byte[(bitPosition+7)/8];

         bitPosition = 0;

         foreach (NestedMessage nm in msgs) 
         {
            nm.Serialize(SerializationAction.Serialize, raw, ref bitPosition);
         }

         return raw;
      }

      public static byte[] SerializePlatformMessage(PlatformMessage msg)
      {
         uint bitPosition = 0;

         return SerializePlatformMessage(msg, null, ref bitPosition, true);
      }

      public static byte[] SerializePlatformMessage(PlatformMessage msg, byte[] raw, ref uint bitPosition, bool includeEnvelope)
      {
         // If raw is null, we create the byte buffer for the caller based on the size of the serialized message.

         if ((bitPosition % 8) != 0) 
         {
            throw new ApplicationException("Messages must be serialized on byte boundaries");
         }

         uint sizeOfPayloadInBytes = 0;

         if (raw == null  ||  includeEnvelope) 
         {
            // In either case, we need to calculate the size of the message.

            uint payloadBitPosition = 0;

            msg.Serialize(SerializationAction.CalculateLength, null, ref payloadBitPosition);

            sizeOfPayloadInBytes = (payloadBitPosition+7u)/8u;
         }

         if (raw == null) 
         {
            uint sizeOfRawblockInBytes = sizeOfPayloadInBytes;

            if (includeEnvelope) 
            {
               sizeOfRawblockInBytes++;      // 0x02 control byte
               sizeOfRawblockInBytes += 2u;  // 16-bit length
               // ... msg goes here ...
               sizeOfRawblockInBytes++;      // 8-bit CRC
            }
            
            raw = new byte[sizeOfRawblockInBytes];
         }

         if (includeEnvelope) 
         {
            ulongSerializer(raw, ref bitPosition,  8, 0x02);                  // 0x02 control byte
            ulongSerializer(raw, ref bitPosition, 16, sizeOfPayloadInBytes);  // 16-bit length
         }

         uint startOfPayloadIndex = bitPosition / 8;

         msg.Serialize(SerializationAction.Serialize, raw, ref bitPosition);

         if (includeEnvelope) 
         {
            snapToByteBoundary(ref bitPosition);

            ulongSerializer(raw, ref bitPosition, 8, Crc.CalculateCrc8(raw, (int) startOfPayloadIndex, (int) sizeOfPayloadInBytes)); // CRC
         }

         return raw;
      }

      #endregion

      #region Public Hydration Methods

      // The HydratePlatformMessage public methods hydrate tracker or base messages.  The protected version (hydratePlatformMessage)
      // can hydrate userdata payload messages as well, but is controlled through indirect means.

      public static PlatformMessage HydratePlatformMessage(byte[] raw, bool includesEnvelope, bool isTrackerMessage) 
      {
         uint bitPosition = 0;
         MessageCategory category = isTrackerMessage ? MessageCategory.Tracker : MessageCategory.Base;

         return hydratePlatformMessage(raw, ref bitPosition, includesEnvelope, category);
      }

      public static PlatformMessage HydratePlatformMessage(byte[] raw, ref uint bitPosition, bool includesEnvelope, bool isTrackerMessage) 
      {
         MessageCategory category = isTrackerMessage ? MessageCategory.Tracker : MessageCategory.Base;

         return hydratePlatformMessage(raw, ref bitPosition, includesEnvelope, category);
      }

      /// <summary>
      /// Initializes the instance from the user-data binary payload.  It normalizes the binary
      /// payload (CDPD and GPRS currently store the packets differently, duh) then uses the UserDataMessage
      /// hydrator.
      /// </summary>
      /// <param name="userDataBinary">The user-data binary payload.</param>
      public static TrackerUserDataMessage HydrateTrackerUserDataMessageFromUserDataBinary(byte[] userDataBinary)
      {
         uint bitPosition = 0;

         return (TrackerUserDataMessage) hydratePlatformMessage(userDataBinary, ref bitPosition, false, MessageCategory.TrackerUserDataPayload, (uint) userDataBinary.Length);
      }

      public static BitConfigurationMessage HydrateBITConfigurationMessageFromBITBinary(byte[] bitConfigBinary)
      {
        uint bitPosition = 0;

        return (BitConfigurationMessage)hydratePlatformMessage(bitConfigBinary, ref bitPosition, false, MessageCategory.BitConfigurationBlockPayload, (uint)bitConfigBinary.Length);
      }

      public static BitConfigurationTrackerMessage HydrateBITMessageFromBITBinary(byte[] bitBinary)
      {
        uint bitPosition = 0;

        return (BitConfigurationTrackerMessage)hydratePlatformMessage(bitBinary, ref bitPosition, false, MessageCategory.Tracker, (uint)bitBinary.Length);
      }

      /// <summary>
      /// Initializes the instance from the user-data binary payload.  It normalizes the binary
      /// payload (CDPD and GPRS currently store the packets differently, duh) then uses the UserDataMessage
      /// hydrator.
      /// </summary>
      /// <param name="userDataBinary">The user-data binary payload.</param>
      public static BaseUserDataMessage HydrateBaseUserDataMessageFromUserDataBinary(byte[] userDataBinary)
      {
         uint bitPosition = 0;

         return (BaseUserDataMessage) hydratePlatformMessage(userDataBinary, ref bitPosition, false, MessageCategory.BaseUserDataPayload, (uint) userDataBinary.Length);
      }

      #endregion

      #region Protected Methods

      protected static PlatformMessage hydratePlatformMessage(byte[] raw, ref uint bitPosition, bool includesEnvelope, MessageCategory category) 
      {
         return hydratePlatformMessage(raw, ref bitPosition, includesEnvelope, category, 0);
      }

      protected static PlatformMessage hydratePlatformMessage(byte[] raw, ref uint bitPosition, bool includesEnvelope, MessageCategory category, uint dataLength) 
      {
         // category indicates how to consider the packet ID found in the raw stream. The packet ID numbers
         // use the same range on both the base and tracker messages, but they are different messages.

         // Packets from the device include the envelope.  Those from the 'base' (platform) do not.  We use the
         // includesEnvelope parameter to know the difference.

         int categoryIndex = (int) category;

         MessageHydrationErrors hydrationErrors = MessageHydrationErrors.NoErrors;   // Used to gather the resulting errors.

         if (includesEnvelope) 
         {
            // The first byte of the packet is a start control byte (0x02) followed by a 16-bit payload length,
            // then the payload, and finally an 8-bit CRC of the payload.

            if (ulongHydrator(raw, ref bitPosition, 8) != 0x02) 
            {
               throw new ApplicationException("PlatformMessage does not start with the 0x02 start control byte");
            }

            // Extract the data length.

            dataLength = (UInt16) ulongHydrator(raw, ref bitPosition, 16);
         }

         // Examine the first byte of the raw message to get the packet ID.  Use that to construct
         // the instance and hydrate its values.
         // Only the lower nibble contains the packet ID for tracker messages (others use a byte).

         uint packetIDLength = (category == MessageCategory.Tracker) ? 4u : 8u;
         int  packetID       = (int) ulongHydrator(raw, ref bitPosition, packetIDLength);

         // The serializers start at the beginning of the payload. We peeked at the first byte of the payload
         // to see which packet to hydrate.  So, we have skipped over a 1 byte control byte and 2 bytes of length
         // so 3 bytes or 24 bits.

         bitPosition -= packetIDLength;   // Backup since the hydrator advanced the bitPosition.

         uint payloadStartByteIndex = bitPosition/8;  // Remember this for error checking.

         PlatformMessage msg = null;

         if (packetID >= registeredPacketTypes[categoryIndex].Length  ||
            registeredPacketTypes[categoryIndex][packetID] == null) 
         {
            // Here, we create an UnknownPlatform message with the contents of the unknown message.
            // If this message was framed, we know the expected data length.

            hydrationErrors |= MessageHydrationErrors.MessageParserNotAvailable;

            UnknownPlatformMessage unkMsg = new UnknownPlatformMessage();

            // We have to help the UnknownPlatformMessage out a bit.

            unkMsg.UnknownPacketID = packetID;
            unkMsg.DataLength      = (dataLength == 0) ? (UInt16) raw.Length-payloadStartByteIndex : dataLength;

            msg = unkMsg;
         } 
         else 
         {
            msg = (PlatformMessage) Activator.CreateInstance(registeredPacketTypes[categoryIndex][packetID]);
         }

         // Remember where this message should end.  Especially useful for whack messages that have
         // fields hidden after the documented size.  We store the bitPosition of the first bit beyond the message.

         if (dataLength > 0) 
         {
            msg.hydrationFinalBitPosition = bitPosition+(dataLength*8);
         }

         // Remember the reported (or calculated) length of the message for the caller.

         msg.reportedMessageLength = dataLength;

         // The serializer could cause an indexing error while reading raw since the envelope could have a payload
         // of, say, 5 bytes while the message is truly 10 bytes long.  I catch that here and launder it.

         try 
         {
             msg.Serialize(SerializationAction.Hydrate, raw, ref bitPosition);
         } 
         catch (IndexOutOfRangeException) 
         {
            hydrationErrors |= MessageHydrationErrors.MessageLongerThanEnvelope;
         }

         if (includesEnvelope) 
         {
            // Verify the 8-bit CRC at the end.

            snapToByteBoundary(ref bitPosition);

            // Verify that the message wasn't longer than the payload.

            uint expectedCrcIndex = payloadStartByteIndex+dataLength;
            uint foundCrcIndex    = bitPosition / 8;

            if (foundCrcIndex != (expectedCrcIndex)) 
            {
               if (foundCrcIndex < expectedCrcIndex) 
               {
                  hydrationErrors |= MessageHydrationErrors.MessageShorterThanEnvelope;
               } 
               else 
               {
                  hydrationErrors |= MessageHydrationErrors.MessageLongerThanEnvelope;
               }
            } 
            else 
            {
               byte packetCrc     = (byte) ulongHydrator(raw, ref bitPosition, 8);
               byte calculatedCrc = Crc.CalculateCrc8(raw, (int) payloadStartByteIndex, (int) dataLength);

               if (packetCrc != calculatedCrc) 
               {
                  hydrationErrors |= MessageHydrationErrors.CrcFailedMessageCorrupt;
               }
            }
         }

         // Assign the list of errors in processing the message, if any.  Note that I OR them into the flags
         // list since the serializer may have set some bits along the way for more details.

         msg.hydrationErrors |= hydrationErrors;

         return msg;
      }

      
      protected static string ToTrueFalse(bool value, int version) 
      {
         // In version 1.0 of the realtime feed, we produce non-standard XML using True and False instead of true and false in the stream.
         // For 2.0, we follow XML.

         if (version == 1) 
         {
            return value ? "True" : "False";
         }

         return XmlConvert.ToString(value);
      }

      /// <summary>
      /// Returns the 0-based index of the lowest bit set in the value.  Returns -1 if
      /// none of the bits are set.
      /// </summary>
      /// <param name="value"></param>
      /// <returns></returns>
      protected int LowestBitIndexSet(UInt32 value)
      {
        int bitIndex = 0;
        int bitMask = 1;

        while ((value & bitMask) == 0 && bitIndex < 32)
        {
          bitIndex++;
          bitMask = bitMask << 1;
        }

        return (bitIndex == 32) ? -1 : bitIndex;
      }

      protected readonly char[] kCharactersNeedingQuotesInCsv = new char[] { ',', '"' };

      protected string CsvQuote(string value)
      {
        // If the part contains a comma or quote, we have to quote it.

        if (value != null && value.IndexOfAny(kCharactersNeedingQuotesInCsv, 0) >= 0)
        {
          // Double the quotes

          value = value.Replace("\"", "\"\"");

          // Now quote it

          return "\"" + value + "\"";
        }

        // Otherwise, no change needed.

        return value;
      }

      protected string JoinAsCsv(IEnumerable stringList)
      {
        StringBuilder result = new StringBuilder();

        IEnumerator strings = stringList.GetEnumerator();

        while (strings.MoveNext())
        {
          if (result.Length > 0)
          {
            result.Append(",");
          }

          string part = (strings.Current == null) ? String.Empty : strings.Current.ToString();

          result.Append(CsvQuote(part));
        }

        return result.ToString();
      }

      #endregion

      #region Bit Twiddlin'

      // Returns a value with the specified bit set.
      protected static ulong bit(uint n) 
      {
         return 1UL << (int) n;
      }

      // Returns a bit mask of bitLength 1's in the low-order bits.
      protected static ulong mask(uint bitLength)
      {
         return bit(bitLength)-1;
      }

      protected static long signExtend(ulong value, uint bitLength)
      {
         ulong signBit = bit(bitLength-1);   // Topmost bit.
         long  result  = (long) value;

         if ((value & signBit) != 0) 
         {
            result = result - (long) bit(bitLength);
         }

         return result;
      }

      #endregion

      #region serializer Overloads

      protected static void snapToByteBoundary(ref uint bitPosition)
      {
         bitPosition = (bitPosition+7u) & ~7u;
      }

      protected static void filler(ref uint bitPosition, uint bitLength) 
      {
         bitPosition += bitLength;
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref bool value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, value ? 1UL : 0UL);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else
         {
            value = ulongHydrator(raw, ref bitPosition, bitLength) != 0;
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref sbyte value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (sbyte) signExtend(ulongHydrator(raw, ref bitPosition, bitLength), bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref byte value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (byte) ulongHydrator(raw, ref bitPosition, bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref Int16 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (Int16) signExtend(ulongHydrator(raw, ref bitPosition, bitLength), bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref UInt16 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (UInt16) ulongHydrator(raw, ref bitPosition, bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref Int32 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (Int32) signExtend(ulongHydrator(raw, ref bitPosition, bitLength), bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref UInt32 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            value = (UInt32) ulongHydrator(raw, ref bitPosition, bitLength);
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref Int64 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else 
         {
            // I have decided that anything getting expanded into an Int64 should not be sign extended since Int64s
            // are IDs and appear nowhere in our protocols. If we had an Int64 in the protocol, sign-extension
            // would be useless anyway. So I leave it unsigned.

            //value = (Int64) signExtend(ulongHydrator(raw, ref bitPosition, bitLength), bitLength);
            value = unchecked((Int64) ulongHydrator(raw, ref bitPosition, bitLength));
         }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref UInt64 value)
      {
        if (action == SerializationAction.Serialize)
        {
          ulongSerializer(raw, ref bitPosition, bitLength, (ulong)value);
        }
        else if (action == SerializationAction.CalculateLength)
        {
          bitPosition += bitLength;
        }
        else
        {
          value = (UInt64)ulongHydrator(raw, ref bitPosition, bitLength);
        }
      }

      protected static void serializer(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref VSS.Hosted.VLCommon.BitVector32 value) 
      {
         if (action == SerializationAction.Serialize) 
         {
            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) value.Data);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;
         }
         else
         {
            value = new VSS.Hosted.VLCommon.BitVector32((int) ulongHydrator(raw, ref bitPosition, bitLength));
         }
      }

      protected static void serializeByteLengthPrefixedNumber(SerializationAction action, byte[] raw,
                                                              ref uint bitPosition, uint bitLength, ref Int32 value) 
      {

         if (action != SerializationAction.Hydrate) 
         {
            // Serialize or Calculate Length case.
            // Get the number of bytes to contain the number

            int  t    = Math.Abs(value);
            uint size = (t < 0x80) ? 1u : (t < 0x8000) ? 2u : (t < 0x800000) ? 3u : 4u;

            // First put out a length prefix of the number of bytes the number will take

            serializer(action, raw, ref bitPosition, bitLength, ref size);

            // Now the number

            serializer(action, raw, ref bitPosition, size*8u, ref value);
         }
         else 
         {
            // Hydrator

            uint byteSize = 0;

            serializer(action, raw, ref bitPosition, bitLength, ref byteSize);
            serializer(action, raw, ref bitPosition, byteSize*8u, ref value);
         }
      }

      protected static void serializeLengthPrefixedBytes(SerializationAction action, byte[] raw,
                                                         ref uint bitPosition, uint bitLength, ref byte[] value) 
      {
         int  localValueLength = 0;

         if (action == SerializationAction.Serialize) 
         {
            localValueLength = (value == null) ? 0 : value.Length;

            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) localValueLength);

            serializeFixedLengthBytes(action, raw, ref bitPosition, (uint) localValueLength, ref value);
         } 
         else if (action == SerializationAction.CalculateLength) 
         {
            bitPosition += bitLength;

            if (value != null) 
            {
               bitPosition += (uint)(value.Length*8);
            }
         } 
         else 
         {
            // Hydrator

            localValueLength = (int) ulongHydrator(raw, ref bitPosition, bitLength);
            serializeFixedLengthBytes(action, raw, ref bitPosition, (uint) localValueLength, ref value);
         }
      }

      protected static void serializeFixedLengthBytes(SerializationAction action, byte[] raw,
                                                      ref uint bitPosition,
                                                      uint byteCount, ref byte[] value) 
      {
         int  byteIndex = 0;

         if (action == SerializationAction.Serialize) 
         {
            // Copy the bytes into the raw array.

            if (byteCount != 0  &&  value != null) 
            {
               uint copyBitPosition = bitPosition;
               uint realByteCount   = (uint) Math.Min(value.Length, byteCount);

               while (realByteCount-- > 0) 
               {
                  ulongSerializer(raw, ref copyBitPosition, 8, value[byteIndex++]);
               }

               // Regardless of the length, we'll bump by the serialization size.
               // If the byte array isn't as long as the room alloted to it, the rest are NULs.
            }

            bitPosition += (uint) (byteCount*8);
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition = (uint)(bitPosition+byteCount*8);
         }
         else 
         {
            if (byteCount == 0) 
            {
               value = null;
            } 
            else 
            {
               value = new byte[byteCount];

               while (byteCount-- > 0) 
               {
                  value[byteIndex++] = (byte) ulongHydrator(raw, ref bitPosition, 8);
               }
            }
         }
      }

      protected void serializeSingleEmbeddedMessage(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref BaseUserDataMessage value) 
      {
         // If we are hydrating messages, I want to catch exceptions due to improper message
         // format and turn the message into an UnknownTrackerUserDataMessage.
         // Note that this does not change exception handling for calculating lengths
         // or serializing.

         uint bitPositionOfEmbeddedMessage = bitPosition;

         try 
         {
            value = (BaseUserDataMessage) serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
               value, MessageCategory.BaseUserDataPayload);
         } 
         catch
         {
            if (action != SerializationAction.Hydrate) 
            {
               // Rethrow the exception as it isn't our special case.

               throw;
            }

            // Turn it into a special message.

            hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

            bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

            UnknownBaseUserDataMessage unknownMsg = new UnknownBaseUserDataMessage();

            serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
               bitLength, ref unknownMsg.Data);

            value = unknownMsg;
         }
      }

      protected void serializeSingleEmbeddedMessage(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref TrackerUserDataMessage value) 
      {
         // If we are hydrating messages, I want to catch exceptions due to improper message
         // format and turn the message into an UnknownTrackerUserDataMessage.
         // Note that this does not change exception handling for calculating lengths
         // or serializing.

         uint bitPositionOfEmbeddedMessage = bitPosition;

         try 
         {
            value = (TrackerUserDataMessage) serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
               value, MessageCategory.TrackerUserDataPayload);
         } 
         catch
         {
            if (action != SerializationAction.Hydrate) 
            {
               // Rethrow the exception as it isn't our special case.

               throw;
            }

            // Turn it into a special message.

            hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

            bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

            UnknownTrackerUserDataMessage unknownMsg = new UnknownTrackerUserDataMessage();

            serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
               bitLength, ref unknownMsg.Data);

            value = unknownMsg;
         }
      }

      protected void serializeSingleEmbeddedMachineEventPayloadMessage(SerializationAction action, byte[] raw,
          ref uint bitPosition, uint bitLength, ref MachineEventBlockPayload value)
      {
        // If we are hydrating messages, I want to catch exceptions due to improper message
        // format and turn the message into an UnknownTrackerUserDataMessage.
        // Note that this does not change exception handling for calculating lengths
        // or serializing.

        uint bitPositionOfEmbeddedMessage = bitPosition;

        try
        {
          value = (MachineEventBlockPayload)serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
             value, MessageCategory.MachineEventBlockPayload);
        }
        catch
        {
          if (action != SerializationAction.Hydrate)
          {
            // Rethrow the exception as it isn't our special case.

            throw;
          }

          // Turn it into a special message.

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

          bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

          UnkownMachineEventData unknownMsg = new UnkownMachineEventData();

          serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
             bitLength, ref unknownMsg.Data);

          value = unknownMsg;
        }
      }

      protected void serializeSingleEmbeddedMachineEventPayloadRadioMessage(SerializationAction action, byte[] raw,
          ref uint bitPosition, uint bitLength, ref MachineEventBlockRadioPayload value)
      {
        // If we are hydrating messages, I want to catch exceptions due to improper message
        // format and turn the message into an UnknownTrackerUserDataMessage.
        // Note that this does not change exception handling for calculating lengths
        // or serializing.

        uint bitPositionOfEmbeddedMessage = bitPosition;

        try
        {
          value = (MachineEventBlockRadioPayload)serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
             value, MessageCategory.MachineEventBlockRadioPayload);
        }
        catch
        {
          if (action != SerializationAction.Hydrate)
          {
            // Rethrow the exception as it isn't our special case.

            throw;
          }

          // Turn it into a special message.

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

          bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

          UnkownMachineEventRadioData unknownMsg = new UnkownMachineEventRadioData();

          serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
             bitLength, ref unknownMsg.Data);

          value = unknownMsg;
        }
      }

      protected void serializeSingleEmbeddedMachineEventPayloadGatewayMessage(SerializationAction action, byte[] raw,
           ref uint bitPosition, uint bitLength, ref MachineEventBlockGatewayPayload value)
      {
        // If we are hydrating messages, I want to catch exceptions due to improper message
        // format and turn the message into an UnknownTrackerUserDataMessage.
        // Note that this does not change exception handling for calculating lengths
        // or serializing.

        uint bitPositionOfEmbeddedMessage = bitPosition;

        try
        {
          value = (MachineEventBlockGatewayPayload)serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
             value, MessageCategory.MachineEventBlockGatewayPayload);
        }
        catch
        {
          if (action != SerializationAction.Hydrate)
          {
            // Rethrow the exception as it isn't our special case.

            throw;
          }

          // Turn it into a special message.

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

          bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

          UnkownMachineEventGatewayData unknownMsg = new UnkownMachineEventGatewayData();

          serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
             bitLength, ref unknownMsg.Data);

          value = unknownMsg;
        }
      }

      protected void serializeSingleEmbeddedMachineEventPayloadVehicleBusMessage(SerializationAction action, byte[] raw,
            ref uint bitPosition, uint bitLength, ref MachineEventBlockVehicleBusPayload value)
      {
        // If we are hydrating messages, I want to catch exceptions due to improper message
        // format and turn the message into an UnknownTrackerUserDataMessage.
        // Note that this does not change exception handling for calculating lengths
        // or serializing.

        uint bitPositionOfEmbeddedMessage = bitPosition;

        try
        {
          //value = (MachineEventBlockVehicleBusPayload)serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength,
          //   value, MessageCategory.MachineEventBlockVehicleBusPayload);

          if (action == SerializationAction.Hydrate)
            value = (MachineEventBlockVehicleBusPayload)hydratePlatformMessage(raw, ref bitPosition, false, MessageCategory.MachineEventBlockVehicleBusPayload);

          else
            value.Serialize(action, raw, ref bitPosition);
        }
        catch
        {
          if (action != SerializationAction.Hydrate)
          {
            // Rethrow the exception as it isn't our special case.

            throw;
          }

          // Turn it into a special message.

          hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;

          bitPosition = bitPositionOfEmbeddedMessage;  // Back up to where we started

          UnkownMachineEventVehicleBusData unknownMsg = new UnkownMachineEventVehicleBusData();

          serializeLengthPrefixedBytes(SerializationAction.Hydrate, raw, ref bitPosition,
             bitLength, ref unknownMsg.Data);

          value = unknownMsg;
        }
      }

      protected void serializeSingleEmbeddedMessage(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, ref NestedMessage value) 
      {
         value = (NestedMessage) serializeSingleEmbeddedMessage(action, raw, ref bitPosition, bitLength, value, MessageCategory.Nested);
      }

      protected PlatformMessage serializeSingleEmbeddedMessage(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, PlatformMessage value, MessageCategory category) 
      {
         // The bitLength refers to the size prefix before the actual nested message.

         uint localValueLength = 0;

         if (action == SerializationAction.Serialize) 
         {
            // First get the length of the embedded message.

            if (value != null) 
            {
               uint embeddedBitPosition = 0;

               value.Serialize(SerializationAction.CalculateLength, null, ref embeddedBitPosition);

               // Round to the nearest byte into the length.

               localValueLength = (embeddedBitPosition+7)/8;
            }

            ulongSerializer(raw, ref bitPosition, bitLength, localValueLength);

            if (localValueLength != 0) 
            {
               value.Serialize(SerializationAction.Serialize, raw, ref bitPosition);
            }
         }
         else if (action == SerializationAction.CalculateLength) 
         {
            // Account for the length prefix

            bitPosition += bitLength;

            if (value != null) 
            {
               // Calculate the length of the embedded message.

               value.Serialize(SerializationAction.CalculateLength, raw, ref bitPosition);
            }
         } 
         else
         {
            // Hydrate the message.

            localValueLength = (uint) ulongHydrator(raw, ref bitPosition, bitLength);

            uint startingBitPosition = bitPosition;

            value = hydratePlatformMessage(raw, ref bitPosition, false, category, localValueLength);

            // Verify the length matches what we found.

            uint hydratedLength = (bitPosition+7-startingBitPosition)/8;

            if (hydratedLength != localValueLength) 
            {
               if (hydratedLength < localValueLength) 
               {
                  throw new ApplicationException("Embedded message was shorter than stated length");
               } 
               else 
               {
                  throw new ApplicationException("Embedded message was longer than stated length");
               }
            }
         }

         return value;
      }

      protected Object[] serializeHeterogeneousRunLengthArray(SerializationAction action, byte[] raw,
         ref uint bitPosition, uint bitLength, Type elementType, Object[] value, MessageCategory category)
      {
         // Array covariance doesn't extend to ref parameters so we need to return the out value, which makes
         // this method less than perfect.

         int  localValueLength = 0;

         if (action == SerializationAction.Serialize  ||  action == SerializationAction.CalculateLength) 
         {
            localValueLength = (value == null) ? 0 : value.Length;

            if (action == SerializationAction.Serialize) 
            {
               ulongSerializer(raw, ref bitPosition, bitLength, (ulong) localValueLength);
            } 
            else 
            {
               bitPosition += bitLength;
            }

            // Now serialize or calculate the length each of the array members.

            if (value != null) 
            {
               foreach (Object msg in value) 
               {
                  NestedMessage nestedMsg = (NestedMessage) msg;

                  // Check for a null, which would indicate a hydration error.

                  if (nestedMsg != null) 
                  {
                     nestedMsg.Serialize(action, raw, ref bitPosition);
                  }
               }
            }
         } 
         else 
         {
            localValueLength = (int) ulongHydrator(raw, ref bitPosition, bitLength);

            if (localValueLength == 0) 
            {
               value = null;
            } 
            else 
            {
               value = (Object[]) Array.CreateInstance(elementType, localValueLength);

               // Populate the array with each message found in the raw stream.

               for (int i=0; i<localValueLength; i++) 
               {
                  uint dataLength = 0;

                  if (hydrationFinalBitPosition > 0) 
                  {
                     // We know the final bit of the parent packet so use that to limit the length of the nested message
                     // that's snapped (especially if it is an unknown message as can often happen with bit packets).

                     dataLength = bytesLeftInMessage(bitPosition);
                  }

                  NestedMessage newMsg = hydratePlatformMessage(raw, ref bitPosition, false, category) as NestedMessage;

                  // If there was a hydration error, we'll get an UnknownPlatformMessage instance, which is not
                  // a NestedMessage so newMsg will be null. In that case, we can't set the parent.  It is up to
                  // the caller to know that a null indicates a bad message.

                  if (newMsg != null) 
                  {
                     newMsg.Parent = this;
                  }

                  value[i] = newMsg;
               }
            }
         }

         return value;
      }

      protected NestedMessage[] serializeHomogeneousRunLengthArray(SerializationAction action, byte[] raw,
            ref uint bitPosition, uint bitLength, NestedMessage[] value, Type nestedType)
      {
         // Array covariance doesn't extend to ref parameters so we need to return the out value, which makes
         // this method less than perfect.

         int  localValueLength = 0;

         if (action == SerializationAction.Serialize  ||  action == SerializationAction.CalculateLength) 
         {
            localValueLength = (value == null) ? 0 : value.Length;

            if (action == SerializationAction.Serialize) 
            {
               ulongSerializer(raw, ref bitPosition, bitLength, (ulong) localValueLength);
            } 
            else 
            {
               bitPosition += bitLength;
            }

            // Now serialize or calculate the length each of the array members.

            if (value != null) 
            {
               foreach (NestedMessage msg in value) 
               {
                  msg.Serialize(action, raw, ref bitPosition);
               }
            }
         } 
         else 
         {
            if (bitLength > 0)
            {
               localValueLength = (int) ulongHydrator(raw, ref bitPosition, bitLength);

               if (localValueLength == 0) 
               {
                  value = null;
               } 
               else 
               {
                  value = (NestedMessage[]) Array.CreateInstance(nestedType, localValueLength);
               }
            } 
            else 
            {
               localValueLength = (value == null) ? 0 : value.Length;
            }

            if (value != null)
            {
               // Populate the array with each message found in the raw stream.

               for (int i=0; i<localValueLength; i++) 
               {
                  value[i] = (NestedMessage) Activator.CreateInstance(nestedType);

                  value[i].Serialize(action, raw, ref bitPosition);

                  value[i].Parent = this;
               }
            }
         }

         return value;
      }

      protected static void serializeDriverID(SerializationAction action, byte[] raw, ref uint bitPosition, TrackerMessage.IDriverID driverID)
      {
         // The length field has an ID type field between it and the variable length data so I have
         // to do flips here to get this to work.  Had the length field preceded the data as it
         // does throughout the wireless interface, I could have used a single call to get it.

         UInt16   localLength          = 0;
         byte     driverIDTypeRaw      = 0;
         Int64    platformDriverIDRaw  = 0;
         string   mdtDriverIDRaw       = null;
         bool     driverPresentRaw     = false;

         if (action == SerializationAction.Hydrate) 
         {
            serializer(SerializationAction.Hydrate, raw, ref bitPosition,  5, ref localLength);
            serializer(SerializationAction.Hydrate, raw, ref bitPosition,  2, ref driverIDTypeRaw);
            serializer(SerializationAction.Hydrate, raw, ref bitPosition,  1, ref driverPresentRaw);
            
            driverID.DriverIDType = (DeviceDriverIDType) driverIDTypeRaw;
            driverID.IsDriverPresent = driverPresentRaw;

            switch (driverID.DriverIDType) 
            {
               case DeviceDriverIDType.PlatformDriverID:
                  // Length must be 4 for this.

                  serializer(SerializationAction.Hydrate, raw, ref bitPosition, 32, ref platformDriverIDRaw);
                  driverID.PlatformDriverID = platformDriverIDRaw;
                  break;

               case DeviceDriverIDType.MdtDriverID:
                  mdtDriverIDRaw = driverID.MdtDriverID;
                  serializeFixedLengthString(SerializationAction.Hydrate, raw, ref bitPosition, localLength, ref mdtDriverIDRaw);
                  driverID.MdtDriverID = mdtDriverIDRaw;
                  break;
            }
         } 
         else 
         {
            // This handles the 'calculateLength' and 'serialize' cases.

            switch (driverID.DriverIDType) 
            {
               case DeviceDriverIDType.PlatformDriverID:
                  // Length must be 4 for this.

                  localLength = 4;
                  break;

               case DeviceDriverIDType.MdtDriverID:
                  localLength = (UInt16) ((driverID.MdtDriverID == null) ? 0 : driverID.MdtDriverID.Length);
                  break;
            }

            driverIDTypeRaw = (byte) driverID.DriverIDType;
            driverPresentRaw = driverID.IsDriverPresent;

            serializer(action, raw, ref bitPosition,  5, ref localLength);
            serializer(action, raw, ref bitPosition,  2, ref driverIDTypeRaw);
            serializer(action, raw, ref bitPosition,  1, ref driverPresentRaw);

            switch (driverID.DriverIDType) 
            {
               case DeviceDriverIDType.PlatformDriverID:
                  platformDriverIDRaw = driverID.PlatformDriverID;
                  serializer(action, raw, ref bitPosition, 32, ref platformDriverIDRaw);
                  break;

               case DeviceDriverIDType.MdtDriverID:
                  mdtDriverIDRaw = driverID.MdtDriverID;
                  serializeFixedLengthString(action, raw, ref bitPosition, (uint) mdtDriverIDRaw.Length, ref mdtDriverIDRaw);
                  break;
            }
         }
      }

      protected static void serializeDriverIDWithAction(SerializationAction action, byte[] raw, ref uint bitPosition, ref byte driverIDAction, TrackerMessage.IDriverID driverID)
      {
         serializer(action, raw, ref bitPosition,  8, ref driverIDAction);

         switch ((DeviceDriverIDAction) driverIDAction) 
         {
            case DeviceDriverIDAction.Validate:
               Int64   PlatformDriverIDRaw   = driverID.PlatformDriverID;
               string  MdtDriverIDRaw        = driverID.MdtDriverID;
               string  DriverDisplayNameRaw  = driverID.DriverDisplayName;
               byte    RfidIbuttonCount      = 0; 

               serializer(action, raw, ref bitPosition, 32, ref PlatformDriverIDRaw);
               
               serializeLengthPrefixedString(action, raw, ref bitPosition,  8, ref MdtDriverIDRaw);
               serializeLengthPrefixedString(action, raw, ref bitPosition,  8, ref DriverDisplayNameRaw);

               serializer(action, raw, ref bitPosition,  8, ref RfidIbuttonCount);

               if (action == SerializationAction.Hydrate) 
               {
                  // We hydrated into the raw members so assign them to the properties.  This is such a hassle to simulate
                  // implementation inheritance.

                  driverID.PlatformDriverID  = PlatformDriverIDRaw;
                  driverID.MdtDriverID       = MdtDriverIDRaw;
                  driverID.DriverDisplayName = DriverDisplayNameRaw;
               }
               
               break;

            case DeviceDriverIDAction.Invalidate:
               serializeDriverID(action, raw, ref bitPosition, driverID);
               break;

            case DeviceDriverIDAction.AckOnly:
               // There are no data fields specifically for an ackonly.
               break;

            case DeviceDriverIDAction.InvalidateAll:
               // There are no data fields specifically for an invalidate-all.
               break;
         }
      }

      protected static void serializeFixedLengthString(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref string value, bool isSRDiagnostic = false)
      {
         Encoding encoder = Encoding.Default;
         byte[] encodedBytes;

         if (action == SerializationAction.Serialize) 
         {
           if (isSRDiagnostic)
             encodedBytes = new byte[1] { Convert.ToByte(Convert.ToInt32(value)) };
           else
             encodedBytes = encoder.GetBytes(value);

            //Array.Copy(encodedBytes, 0, raw, bitPosition/8, Math.Min(encodedBytes.Length, byteCount));

            if ((bitPosition & 0x7) == 0) 
            {
               // On an even bit boundary

               Buffer.BlockCopy(encodedBytes, 0, raw, (int) bitPosition/8, (int) Math.Min(encodedBytes.Length, byteCount));
               bitPosition += byteCount*8;
            } 
            else 
            {
               // Rats, need to use the serialize code since sometimes we're on a bit boundary
               // To make it easy, we are going to save off a value of what bitPostion should
               // be at the end of the serialization.  Then after we've serialized the string
               // set the value for bitPosition.

               uint finalBitPosition = bitPosition + byteCount*8;

               for (int i=0; i<encodedBytes.Length && i<byteCount; i++) 
               {
                  serializer(action, raw, ref bitPosition, 8, ref encodedBytes[i]);
               }

               bitPosition = finalBitPosition;
            }
         } 
         else if (action == SerializationAction.Hydrate) 
         {
            // We don't want to get any trailing NULs so scan the array for the first NUL and set the length
            // accordingly.
            if (isSRDiagnostic)
                 value = Convert.ToString( raw[(int)bitPosition / 8]);

            if ((bitPosition & 0x07) == 0) 
            {
               // On an even bit boundary

               int activeLength = 0;
               int index = (int) bitPosition/8;

               while (activeLength < byteCount && raw[index++] != 0) 
               {
                  activeLength++;
               }

               if (!isSRDiagnostic)                
                 value = encoder.GetString(raw, (int) bitPosition/8, activeLength);

               bitPosition += byteCount*8;
            } 
            else 
            {
               // Otherwise, pull it into an array

               byte[] alignedBytes = new byte[byteCount];

               for (int i=0; i<alignedBytes.Length; i++) 
               {
                  serializer(action, raw, ref bitPosition, 8, ref alignedBytes[i]);
               }

               int activeLength = 0;

               for (int i=0; i<alignedBytes.Length; i++) 
               {
                  if (alignedBytes[i] == 0) 
                  {
                     break;
                  }

                  activeLength = i+1;
               }
               if (!isSRDiagnostic) 
                 value = encoder.GetString(alignedBytes, 0, activeLength);
            }
         }
         else 
         {
            // calculate the length

            bitPosition += byteCount*8;
         }
      }

      protected static void serializeASCIIFixedLengthString(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref string value)
      {
        Encoding encoder = Encoding.ASCII;

        if (action == SerializationAction.Serialize)
        {
          byte[] encodedBytes = encoder.GetBytes(value);

          //Array.Copy(encodedBytes, 0, raw, bitPosition/8, Math.Min(encodedBytes.Length, byteCount));

          if ((bitPosition & 0x7) == 0)
          {
            // On an even bit boundary

            Buffer.BlockCopy(encodedBytes, 0, raw, (int)bitPosition / 8, (int)Math.Min(encodedBytes.Length, byteCount));
            bitPosition += byteCount * 8;
          }
          else
          {
            // Rats, need to use the serialize code since sometimes we're on a bit boundary
            // To make it easy, we are going to save off a value of what bitPostion should
            // be at the end of the serialization.  Then after we've serialized the string
            // set the value for bitPosition.

            uint finalBitPosition = bitPosition + byteCount * 8;

            for (int i = 0; i < encodedBytes.Length && i < byteCount; i++)
            {
              serializer(action, raw, ref bitPosition, 8, ref encodedBytes[i]);
            }

            bitPosition = finalBitPosition;
          }
        }
        else if (action == SerializationAction.Hydrate)
        {
          // We don't want to get any trailing NULs so scan the array for the first NUL and set the length
          // accordingly.

          if ((bitPosition & 0x07) == 0)
          {
            // On an even bit boundary

            int activeLength = 0;
            int index = (int)bitPosition / 8;

            while (activeLength < byteCount && raw[index++] != 0)
            {
              activeLength++;
            }

            value = encoder.GetString(raw, (int)bitPosition / 8, activeLength);

            bitPosition += byteCount * 8;
          }
          else
          {
            // Otherwise, pull it into an array

            byte[] alignedBytes = new byte[byteCount];

            for (int i = 0; i < alignedBytes.Length; i++)
            {
              serializer(action, raw, ref bitPosition, 8, ref alignedBytes[i]);
            }

            int activeLength = 0;

            for (int i = 0; i < alignedBytes.Length; i++)
            {
              if (alignedBytes[i] == 0)
              {
                break;
              }

              activeLength = i + 1;
            }

            value = encoder.GetString(alignedBytes, 0, activeLength);
          }
        }
        else
        {
          // calculate the length

          bitPosition += byteCount * 8;
        }
      }

      protected static void serializeLengthPrefixedString(SerializationAction action, byte[] raw,
                                                          ref uint bitPosition, uint bitLength, ref string value) 
      {
         // For strings, the bitLength refers to the length prefix.

         Encoding codec            = Encoding.Default;
         int      localValueLength = 0;
         uint     byteIndex        = 0;

         if (action == SerializationAction.Serialize) 
         {
            string localValue = value;

            if (localValue == null) 
            {
               localValue = String.Empty;
            }

            localValueLength = codec.GetByteCount(localValue);

            ulongSerializer(raw, ref bitPosition, bitLength, (ulong) localValueLength);

            // Copy the bytes into the raw array.  Note that we require the bitPosition to be byte aligned
            // right now.

            if ((bitPosition & 0x07) != 0) 
            {
               throw new ApplicationException(String.Format("Attempt to serialize string at {0}; require byte boundary", bitPosition));
            }

            byteIndex = bitPosition / 8;

            int bytesEncoded = codec.GetBytes(localValue, 0, localValue.Length, raw, (int) byteIndex);

            byteIndex += (uint) bytesEncoded;

            bitPosition = byteIndex * 8;
         } 
         else if (action == SerializationAction.CalculateLength)
         {
            bitPosition += bitLength;  // Account for the length prefix.

            if (!String.IsNullOrEmpty(value)) 
            {
               bitPosition = (uint) (bitPosition+codec.GetByteCount(value)*8);
            }
         }
         else 
         {
            localValueLength = (int) ulongHydrator(raw, ref bitPosition, bitLength);

            // Extract the string from the byte stream.

            byte[] toConvert;

            if ((bitPosition & 0x07) != 0) 
            {
               // Rats, not byte aligned so we have extra work

               toConvert = new byte[localValueLength];

               for (int i=0; i<localValueLength; i++) 
               {
                  serializer(action, raw, ref bitPosition, 8, ref toConvert[i]);
               }

               byteIndex = 0;
            } 
            else 
            {
               toConvert = raw;
               byteIndex = bitPosition / 8;
               bitPosition += (uint)(localValueLength * 8);
            }

            value = codec.GetString(toConvert, (int) byteIndex, (int) localValueLength);
         }
      }

      protected static void serializeNulTerminatedString(SerializationAction action, byte[] raw, ref uint bitPosition, ref string value)
      {
         Encoding encoder = Encoding.Default;

         if (action != SerializationAction.Hydrate) 
         {
            byte[] encodedBytes = encoder.GetBytes(value);

            if (action == SerializationAction.Serialize) 
            {
               // Note that we don't explicitly copy a NUL to the byte after the string, but the buffer starts out
               // NUL filled so it is there.

               //Array.Copy(encodedBytes, 0, raw, bitPosition/8, encodedBytes.Length);
               Buffer.BlockCopy(encodedBytes, 0, raw, (int) bitPosition/8, encodedBytes.Length);
            }

            bitPosition += (uint)((encodedBytes.Length+1)*8);

         } 
         else if (action == SerializationAction.Hydrate) 
         {
            // Look for a NUL in the byte array.  That is the first byte beyond the end of the string. Convert the
            // bytes before that to a string.  Note that this assumes an ANSI encoding and none of that fancy
            // shift-JIS or UTF-8 multi-byte encodings.

            int startIndex = (int) (bitPosition/8);
            int endIndex   = startIndex;

            while (endIndex < raw.Length  &&  raw[endIndex] != 0) 
            {
               endIndex++;
            }

            // endIndex points to the NUL.

            value = encoder.GetString(raw, startIndex, endIndex-startIndex);

            bitPosition = (uint)((endIndex+1)*8);
         }
      }

      protected static void serializeAsteriskTerminatedString(SerializationAction action, byte[] raw, ref uint bitPosition, ref string value)
      {
        Encoding encoder = Encoding.Default;

        if (action != SerializationAction.Hydrate)
        {
          value = string.Format("{0}*", value);
          byte[] encodedBytes = encoder.GetBytes(value);

          if (action == SerializationAction.Serialize)
          {
            //Array.Copy(encodedBytes, 0, raw, bitPosition/8, encodedBytes.Length);
            Buffer.BlockCopy(encodedBytes, 0, raw, (int)bitPosition / 8, encodedBytes.Length);
          }

          bitPosition += (uint)((encodedBytes.Length) * 8);

        }
        else if (action == SerializationAction.Hydrate)
        {
          // Look for a * in the byte array.  That is the first byte beyond the end of the string. Convert the
          // bytes before that to a string.  Note that this assumes an ANSI encoding and none of that fancy
          // shift-JIS or UTF-8 multi-byte encodings.

          int startIndex = (int)(bitPosition / 8);
          int endIndex = startIndex;

          while (endIndex < raw.Length && Convert.ToChar(raw[endIndex]) != '*')
          {
            endIndex++;
          }

          // endIndex points to the NUL.

          value = encoder.GetString(raw, startIndex, endIndex - startIndex);

          bitPosition = (uint)((endIndex + 1) * 8);
        }
      }

      #endregion

      #region ulongSerializer & ulongHydrator

      protected static void ulongSerializer(byte[] raw, ref uint bitPosition, uint bitLength, ulong value) 
      {
         // We assume little endian into the raw stream

         uint byteIndex   = bitPosition / 8;
         uint bitsToShift = bitPosition % 8;

         while (bitLength > 0) 
         {
            byte slice       = (byte) value;
            uint bitsInSlice = Math.Min((8-bitsToShift), bitLength);

            // If the slice includes too many bits, mask them.

            if (bitsInSlice < 8) 
            {
               slice &= (byte) mask(bitsInSlice);
            }

            // Now shift the remaining bits to their spot.

            slice = (byte)(slice << (int) bitsToShift);

            raw[byteIndex] |= slice;

            // Update the loop control variables.
            // After the first iteration, everything lines up with a byte boundary.

            value      >>= (int) bitsInSlice;
            bitLength   -= bitsInSlice;
            bitPosition += bitsInSlice;

            bitsToShift  = 0;
            byteIndex++;
         }
      }
      
      protected static ulong ulongHydrator(byte[] raw, ref uint bitPosition, uint bitLength) 
      {
         ulong returnValue   = 0;
         uint  valueBitIndex = 0;     // Used to tile the bits into value.

         // We assume little endian in the raw stream; this hydrator, though, is endian-agnostic.

         uint byteIndex   = bitPosition / 8;
         uint bitsToShift = bitPosition % 8;

         while (bitLength > 0) 
         {
            // grab a chunk

            byte slice = (byte) (raw[byteIndex] >> (int) bitsToShift);   // 'right justify' the part to include.
               
            // Mask off any noise

            uint bitsInSlice = Math.Min(bitLength, 8-bitsToShift);

            if (bitsInSlice < 8) 
            {
               slice &= (byte) mask(bitsInSlice);
            }

            // Plunk the bits into the output value.

            returnValue |= ((ulong) slice) << (int) valueBitIndex;

            // Adjust all looping parameters.

            valueBitIndex += bitsInSlice;
            bitLength     -= bitsInSlice;
            bitPosition   += bitsInSlice;

            // After the first loop, we never need to shift since we'll be at the start of a byte.

            bitsToShift = 0;
            byteIndex++;
         }

         // Now the resulting value is in 'returnValue'.  We leave it unsigned.  Overloads handle extension.

         return returnValue;
      }

      protected static void BigEndianSerializer(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref uint val)
      {
        if (action == SerializationAction.Hydrate)
        {
          BigEndianHydrator(raw, ref bitPosition, byteCount, out val);
        }
        else
        {
          uint bitPositionReverse = 0;
          uint bitCount = byteCount * 8;
          byte[] bytes = new byte[byteCount];
          serializer(action, bytes, ref bitPositionReverse, bitCount, ref val);
          List<byte> byteList = new List<byte>(bytes);
          byteList.Reverse();
          byte[] reversedBytes = byteList.ToArray();
          serializeFixedLengthBytes(action, raw, ref bitPosition, byteCount, ref reversedBytes);
        }
      }
      protected static void BigEndianSerializer(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref int val)
      {
        if (action == SerializationAction.Hydrate)
        {
          BigEndianHydrator(raw, ref bitPosition, byteCount, out val);
        }
        else
        {
          uint bitPositionReverse = 0;
          uint bitCount = byteCount * 8;
          byte[] bytes = new byte[byteCount];
          serializer(action, bytes, ref bitPositionReverse, bitCount, ref val);
          List<byte> byteList = new List<byte>(bytes);
          byteList.Reverse();
          byte[] reversedBytes = byteList.ToArray();
          serializeFixedLengthBytes(action, raw, ref bitPosition, byteCount, ref reversedBytes);
        }
      }

      protected static void BigEndianSerializer(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref short val)
      {
        if (action == SerializationAction.Hydrate)
        {
          BigEndianHydrator(raw, ref bitPosition, byteCount, out val);
        }
        else
        {
          uint bitPositionReverse = 0;
          uint bitCount = byteCount * 8;
          byte[] bytes = new byte[byteCount];
          serializer(action, bytes, ref bitPositionReverse, bitCount, ref val);
          List<byte> byteList = new List<byte>(bytes);
          byteList.Reverse();
          byte[] reversedBytes = byteList.ToArray();
          serializeFixedLengthBytes(action, raw, ref bitPosition, byteCount, ref reversedBytes);
        }
      }
      protected static void BigEndianSerializer(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref ushort val)
      {
        if (action == SerializationAction.Hydrate)
        {
          BigEndianHydrator(raw, ref bitPosition, byteCount, out val);
        }
        else
        {
          uint bitPositionReverse = 0;
          uint bitCount = byteCount * 8;
          byte[] bytes = new byte[byteCount];
          serializer(action, bytes, ref bitPositionReverse, bitCount, ref val);
          List<byte> byteList = new List<byte>(bytes);
          byteList.Reverse();
          byte[] reversedBytes = byteList.ToArray();
          serializeFixedLengthBytes(action, raw, ref bitPosition, byteCount, ref reversedBytes);
        }
      }

      protected static void BigEndianSerializer(SerializationAction action, byte[] raw, ref uint bitPosition, uint byteCount, ref long val)
      {
        if (action == SerializationAction.Hydrate)
        {
          BigEndianHydrator(raw, ref bitPosition, byteCount, out val);
        }
        else
        {
          uint bitPositionReverse = 0;
          uint bitCount = byteCount * 8;
          byte[] bytes = new byte[byteCount];
          serializer(action, bytes, ref bitPositionReverse, bitCount, ref val);
          List<byte> byteList = new List<byte>(bytes);
          byteList.Reverse();
          byte[] reversedBytes = byteList.ToArray();
          serializeFixedLengthBytes(action, raw, ref bitPosition, byteCount, ref reversedBytes);
        }
      }

      protected static void BigEndianHydrator(byte[] raw, ref uint bitPosition, uint byteCount, out uint val)
      {
         byte[] bytes = new byte[4];
         serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, byteCount, ref bytes);
         List<byte> byteList = new List<byte>();
         if (bytes.Length < 4)
         {
           while (byteList.Count + bytes.Length < 4)
           {
             byteList.Add(0);
           }
         }
         byteList.AddRange(bytes);
         byteList.Reverse();
         val = BitConverter.ToUInt32(byteList.ToArray(), 0);
      }
      protected static void BigEndianHydrator(byte[] raw, ref uint bitPosition, uint byteCount, out int val)
      {
        byte[] bytes = new byte[4];
        serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, byteCount, ref bytes);
        List<byte> byteList = new List<byte>();
        if (bytes.Length < 4)
        {
          while (byteList.Count + bytes.Length < 4)
          {
            byteList.Add(0);
          }
        }
        byteList.AddRange(bytes);
        byteList.Reverse();
        val = BitConverter.ToInt32(byteList.ToArray(), 0);
      }

      protected static void BigEndianHydrator(byte[] raw, ref uint bitPosition, uint byteCount, out short val)
      {
        byte[] bytes = new byte[2];
        serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, byteCount, ref bytes);
        List<byte> byteList = new List<byte>();
        if (bytes.Length < 2)
        {
          while (byteList.Count + bytes.Length < 2)
          {
            byteList.Add(0);
          }
        }
        byteList.AddRange(bytes);
        byteList.Reverse();
        val = BitConverter.ToInt16(byteList.ToArray(), 0);
      }

      protected static void BigEndianHydrator(byte[] raw, ref uint bitPosition, uint byteCount, out ushort val)
      {
        byte[] bytes = new byte[2];
        serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, byteCount, ref bytes);
        List<byte> byteList = new List<byte>();
        if (bytes.Length < 2)
        {
          while (byteList.Count + bytes.Length < 2)
          {
            byteList.Add(0);
          }
        }
        byteList.AddRange(bytes);
        byteList.Reverse();
        val = BitConverter.ToUInt16(byteList.ToArray(), 0);
      }
      protected static void BigEndianHydrator(byte[] raw, ref uint bitPosition, uint byteCount, out long val)
      {
        byte[] bytes = new byte[8];
        serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, byteCount, ref bytes);
        List<byte> byteList = new List<byte>();
        if (bytes.Length < 8)
        {
          while (byteList.Count + bytes.Length < 8)
          {
            byteList.Add(0);
          }
        }
        byteList.AddRange(bytes);
        byteList.Reverse();

        val = BitConverter.ToInt64(byteList.ToArray(), 0);
      }
      #endregion

      #region lengthBackfill Definition

      protected struct lengthBackfill 
      {
         public uint messageSize;
         private SerializationAction action;
         private uint   bitLength;
         private uint   bitPosition;
         private UInt32 hydrationLength;
         private byte[] raw;

         public static lengthBackfill Mark(SerializationAction action, byte[] raw, ref uint bitPosition, uint bitLength) 
         {
            lengthBackfill mark = new lengthBackfill();

            mark.action      = action;
            mark.raw         = raw;
            mark.bitPosition = bitPosition;
            mark.bitLength   = bitLength;

            if (action == SerializationAction.Hydrate) 
            {
               mark.hydrationLength = (uint) ulongHydrator(raw, ref bitPosition, bitLength);
            } 
            else 
            {
               PlatformMessage.filler(ref bitPosition, bitLength);   // Skip over where we'll backfill the value.
            }

            return mark;
         }

         public uint  BitsRemaining(uint currentBitPosition) 
         {
           if(hydrationLength > 0)
            return hydrationLength*8-(currentBitPosition-(bitPosition+bitLength));

           return 0;
         }

         public uint BytesRemaining(uint currentBitPosition)
         {
           return BitsRemaining(currentBitPosition) / 8;
         }

         public void Backfill(uint endBitPosition) 
         {
            // We only have work to do if the action is serializing.

            if (action == SerializationAction.Serialize) 
            {
               uint byteLength = (endBitPosition-(bitPosition+bitLength))/8;
               messageSize = byteLength;
               PlatformMessage.serializer(action, raw, ref bitPosition, bitLength, ref byteLength);
            }
            else if (action == SerializationAction.Hydrate) 
            {
               // Verify the length of these blocks individually so we can better see where the error is.

               UInt32 totalBitLength = endBitPosition-(bitPosition+bitLength);

               if (totalBitLength != hydrationLength*8) 
               {
                  throw new IndexOutOfRangeException("Sub block length does not match the data");
               }
            }
         }
      }

      #endregion

      static PlatformMessage() 
      {
         // Verify we are on a little endian system

         if (!BitConverter.IsLittleEndian) 
         {
            throw new ApplicationException("The PlatformMessage serialization code requires a little endian host platform.");
         }

         // Now register all the packet types.

         registerPacketTypes();
      }

      #region Device Packet Type Registration

       private static void registerPacketTypes()
      {
         // All platform messages (both base and device) must be defined in the current assembly.
         // Using reflection, find the public classes that derive from PlatformMessage and
         // register each.

         Type[] exportedTypes = Assembly.GetExecutingAssembly().GetExportedTypes();

         foreach (Type packetType in exportedTypes) 
         {
            if (!packetType.IsSubclassOf(typeof(PlatformMessage)) || // We only register platform messages
                packetType.IsAbstract)                               // ...unless they are abstract
            {
               continue;
            }

            int  packetID = 0;
            MessageCategory category = 
               packetType.IsSubclassOf(typeof(TrackerMessage))          ? MessageCategory.Tracker :
               packetType.IsSubclassOf(typeof(BaseUserDataMessage))     ? MessageCategory.BaseUserDataPayload :
               packetType.IsSubclassOf(typeof(TrackerUserDataMessage))  ? MessageCategory.TrackerUserDataPayload :
               packetType.IsSubclassOf(typeof(BaseMessage))             ? MessageCategory.Base :
               packetType.IsSubclassOf(typeof(BitConfigurationMessage)) ? MessageCategory.BitConfigurationBlockPayload :
               packetType.IsSubclassOf(typeof(NestedMessage))           ? MessageCategory.Nested :
               packetType.IsSubclassOf(typeof(MachineEventBlockRadioPayload)) ? MessageCategory.MachineEventBlockRadioPayload :
               packetType.IsSubclassOf(typeof(MachineEventBlockGatewayPayload)) ? MessageCategory.MachineEventBlockGatewayPayload :
               packetType.IsSubclassOf(typeof(MachineEventBlockVehicleBusPayload)) ? MessageCategory.MachineEventBlockVehicleBusPayload :
               packetType.IsSubclassOf(typeof(MachineEventBlockPayload)) ? MessageCategory.MachineEventBlockPayload :
               packetType.IsSubclassOf(typeof(PLBaseMessage)) ? MessageCategory.PLBaseMessage :
               packetType.IsSubclassOf(typeof(PLTrackerMessage)) ? MessageCategory.PLTrackerMessage :
               packetType.Equals(typeof(UnknownPlatformMessage))         ? MessageCategory.InternalSupport :
                                                                          MessageCategory.MessageCategoryCount;

            if (category == MessageCategory.MessageCategoryCount) 
            {
               throw new ApplicationException("You defined a new category of message but did not update PlatformMessage.registerPacketTypes");
            }

            // Nested messages do not have a kPacketID field since their type is known from context (i.e., there
            // is never an ID serialized to identify the type).

            if (category != MessageCategory.Nested) 
            {
               // Get the ID from the class.

               FieldInfo packetIDConstant = packetType.GetField("kPacketID");

               packetID = (int) packetIDConstant.GetValue(null);

               if (packetID != -1) 
               {
                  // Packet IDs of -1 are nested serialized packets (e.g., CondensedDeviceState).

                  registerDevicePacketType(category, packetID, packetType);
               }
            }
         }
      }

      public static void DumpPacketRegistrations(TextWriter output) 
      {
         // Dump the registrations

         for (MessageCategory cat = (MessageCategory) 0; cat < MessageCategory.MessageCategoryCount; cat++) 
         {
            int i = (int) cat;

            if (registeredPacketTypes[i] != null) 
            {
               output.WriteLine("Known {0} Messages:", cat);

               for (int id=0; id < registeredPacketTypes[i].Length; id++) 
               {
                  if (registeredPacketTypes[i][id] != null) 
                  {
                     output.WriteLine(" ID {0:X2}: {1}", id, registeredPacketTypes[i][id].Name);
                  }
               }
            }
         }
         output.WriteLine("---[EndOfRegistrations]---");
      }

      private static void registerDevicePacketType(MessageCategory category, int packetID, Type devicePacketType) 
      {
         int registeredPacketTypesLength = 0;
         int categoryIndex = (int) category;

         if (registeredPacketTypes[categoryIndex] != null) 
         {
            registeredPacketTypesLength = registeredPacketTypes[categoryIndex].Length;
         }

         if (registeredPacketTypesLength <= packetID)
         {
            Type[] newArray = new Type[packetID+1];

            if (registeredPacketTypes[categoryIndex] != null) 
            {
               // Copy the array over.

               Array.Copy(registeredPacketTypes[categoryIndex], newArray, registeredPacketTypes[categoryIndex].Length);
            }

            registeredPacketTypes[categoryIndex] = newArray;
         }

         // Now that the registeredPacketTypes array is ready, store the association for speed.

         if (registeredPacketTypes[categoryIndex][packetID] != null) 
         {
            throw new ApplicationException(String.Format("{0} packet ID {1} registered twice", devicePacketType.Name, packetID));
         }

         registeredPacketTypes[categoryIndex][packetID] = devicePacketType;
      }

      private static Type[][] registeredPacketTypes = new Type[(int)MessageCategory.MessageCategoryCount][];

      #endregion

      #region Instance and Class Data Members

      public static readonly int kPacketID = -1;  // This is never used; derived classes have a new version of it.

      protected uint   bytesLeftInMessage(uint bitPosition) 
      {
         return (hydrationFinalBitPosition > 0) ? (hydrationFinalBitPosition-bitPosition)/8 : 0;
      }

      protected MessageHydrationErrors    hydrationErrors;
      protected uint                      hydrationFinalBitPosition;
      protected uint                      reportedMessageLength;

      #endregion

      #region Reflective Property/Field Access

      protected Object getFieldOrPropertyValue(string name) 
      {
         Type myType = this.GetType();

         FieldInfo fi = myType.GetField(name);

         if (fi != null) 
         {
            return fi.GetValue(this);
         }

         PropertyInfo pi = myType.GetProperty(name);

         if (pi != null) 
         {
            return pi.GetValue(this, null);
         }

         return null;
      }

      public string GetStringProperty(string name, string defaultValue) 
      {
         Object stringObj = getFieldOrPropertyValue(name);

         if (stringObj != null) 
         {
            try 
            {
               return Convert.ToString(stringObj);
            } 
            catch { /* mute */ }
         }

         return defaultValue;
      }

      public int GetIntProperty(string name, int defaultValue) 
      {
         Object intObj = getFieldOrPropertyValue(name);

         if (intObj != null) 
         {
            try 
            {
               return Convert.ToInt32(intObj);
            } 
            catch { /* mute */ }
         }

         return defaultValue;
      }

      public bool GetBoolProperty(string name, bool defaultValue) 
      {
         Object boolObj = getFieldOrPropertyValue(name);

         if (boolObj != null) 
         {
            try 
            {
               return Convert.ToBoolean(boolObj);
            } 
            catch { /* mute */ }
         }

         return defaultValue;
      }

      public double GetDoubleProperty(string name, double defaultValue) 
      {
         Object doubleObj = getFieldOrPropertyValue(name);

         if (doubleObj != null) 
         {
            try 
            {
               return Convert.ToDouble(doubleObj);
            } 
            catch { /* mute */ }
         }

         return defaultValue;
      }

      public Object GetGeneralProperty(string name, Object defaultValue) 
      {
         Object obj = GetObjectProperty(name, defaultValue);

         if (obj != defaultValue  &&  obj != null) 
         {
            try 
            {
               return Convert.ChangeType(obj, defaultValue.GetType());
            } 
            catch { /* mute */ }
         }

         return defaultValue;
      }

      public Object GetObjectProperty(string name, Object defaultValue) 
      {
         Object obj = getFieldOrPropertyValue(name);

         return (obj != null) ? obj : defaultValue;
      }

      #endregion

      public override string ToString()
      {
        return this.GetMessageString();
      }

      private string GetMessageString()
      {
        try
        {
          StringBuilder builder = new StringBuilder(this.GetType().Name);
          FieldInfo[] messageFields = this.GetType().GetFields();
          PropertyInfo[] messageProperties = this.GetType().GetProperties();
          if (hydrationErrors != MessageHydrationErrors.NoErrors)
            builder.AppendFormat("\nInvalid Message:  {0}", hydrationErrors.ToString());
          foreach (PropertyInfo property in messageProperties)
          {
            object prop = property.GetValue(this, null);
            if (property.PropertyType != typeof(byte[]))
            {
              if (prop != null && property.PropertyType != typeof(PlatformMessage) && property.PropertyType != typeof(MessageCategory) 
                && property.PropertyType != typeof(Type)
                && property.DeclaringType != typeof(PlatformMessage) 
                && property.Name != "CsvValues"
                && property.Name != "EventIsPublished" && !property.PropertyType.IsArray && !(prop is IList))
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), prop.ToString());
              }
              else if (property.PropertyType.IsArray || (prop is IList))
              {
                var array = prop as IList;

                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, " Count:"), array.Count);

                foreach (var arrayItem in array)
                {
                  builder.Append("\n");
                  builder.Append(arrayItem.ToString());
                }
              }
            }
            else if (prop != null && property.PropertyType == typeof(byte[]))
            {
              builder.Append("\n");
              byte[] bytes = (prop as byte[]);
              builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), HexDump.BytesToHexString(bytes));
            }
          }

          foreach (FieldInfo field in messageFields)
          {
            object f = field.GetValue(this);
            if (f != null && !field.IsInitOnly && !field.FieldType.IsArray 
              && !field.FieldType.IsGenericType && !string.IsNullOrEmpty(f.ToString()))
            {
              builder.Append("\n");
              builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, ":"), f.ToString());
            }
            else if (f != null && (field.FieldType.IsArray || field.FieldType.IsGenericType))
            {
              if (field.FieldType.IsGenericType && !(f is IList))
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, " Count:"), f.ToNullString());
              }
              else if (field.FieldType != typeof(byte[]))
              {
                var array = f as IList;

                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, " Count:"), array.Count);

                foreach (var arrayItem in array)
                {
                  builder.Append("\n");
                  if (arrayItem != null)
                    builder.Append(arrayItem.ToString());
                }
              }
              else if (f != null && field.FieldType == typeof(byte[]))
              {
                builder.Append("\n");
                byte[] bytes = (field.GetValue(this) as byte[]);
                builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, ":"), HexDump.BytesToHexString(bytes));
              }
            }
          }
          return builder.ToString();
        }
        catch (Exception)
        {
          return base.ToString();
        }
      }

     private string DecorateString(string s, string d)
      {
        s = (s == null) ? string.Empty : s;
        d = (d == null) ? string.Empty : d;
        return new StringBuilder(s).Append(d).ToString();
      }
   }

   public class UnknownPlatformMessage : PlatformMessage, INeedsAcknowledgement
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.InternalSupport; }
      }

      public static new readonly int kPacketID = 0;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         // Nobody should try to create this message.  If they are trying to do anything but hydrate,
         // ensure the DataLength is correct.

         if (action != SerializationAction.Hydrate) 
         {
            DataLength = (UnknownMessageData == null) ? 0 : (uint) UnknownMessageData.Length;
         }

         serializeFixedLengthBytes(SerializationAction.Hydrate, raw, ref bitPosition, DataLength, ref UnknownMessageData);
      }

      public int     UnknownPacketID;
      [XmlIgnore]
      public uint    DataLength;
      [XmlIgnore]
      public byte[]  UnknownMessageData;

      #region Implementation of INeedsAcknowledgement

      public Type AcknowledgementType
      {
         get { return typeof(UnsupportedMessageResponse); }
      }

      public ProtocolMessage GenerateAcknowledgement()
      {
        UnsupportedMessageResponse ack = new UnsupportedMessageResponse();
        ack.UnknownMessageData = UnknownMessageData;

        return ack;
      }

      public bool IsMessageForAcknowledgement(ProtocolMessage ackMessage)
      {
         return false;
      }

      #endregion
   }

   #region NestedMessage (Used for messages containing arrays of objects)

   public abstract class NestedMessage : PlatformMessage
   {
      public override MessageCategory Category 
      {
         get { return MessageCategory.Nested; }
      }

      public override int PacketID
      {
         // Nested messages have a -1 packet ID since the packet type is implied.
         get { return -1; }
      }

      [XmlIgnore]
      public PlatformMessage Parent 
      {
         get { return parent; }
         set { parent = value; }
      }

      protected PlatformMessage  parent;
   }

   #endregion
}
