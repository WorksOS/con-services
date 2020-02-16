using System;
using System.Reflection;
using System.IO;
using System.Xml;
using VSS.Hosted.VLCommon;

/// These are enumerations and constants used by the various message parsers in the system such as the
/// PlatformMessage parsers and the TrimTrac message parsers. By putting the constants here, they are not
/// duplicated in those assemblies and neither message hierarchy has to include the other for convenient constants.
namespace VSS.Hosted.VLCommon.MTSMessages
{
   public enum SerializationAction 
   {
      Serialize,
      CalculateLength,
      Hydrate
   }

   [Flags]
   public enum MessageHydrationErrors 
   {
      NoErrors                      =  0,

      MessageParserNotAvailable     =  1,
      MessageShorterThanEnvelope    =  2,
      MessageLongerThanEnvelope     =  4,
      CrcFailedMessageCorrupt       =  8,
      EmbeddedMessageUnknown        = 16, // Error hydrating one of the nested messages.
      EmbeddedBitCountIncorrect     = 32, // Number of BIT blocks (crossCheck only) is incorrect, but compensated for.
      EmbeddedMessageMalformed      = 64  // Indicates some error was worked around in the embedded message
   }

   #region Event IDs

   /// <summary>
   /// These user-data event IDs originate in the platform for CrossCheck devices. TrimTracs effectively use two ID
   /// numbers assigned to CrossCheck events mostly to make as much common as possible between the two message
   /// hierarchies. The artificial IDs do exist between the two platforms for the sake of a common realtime feed.
   /// </summary>
   public enum UDEventID
   {
      // The following are artificial event numbers used in the PlatformEvents hierarchy.

      Coherency = -7,
      MessageSend = -6,
      MessageAcknowledgement = -5,
      MessageReturnReceipt = -4,
      MessageResponse = -3,
      PositionEvent = -2,

      // Some messages will give Unparsed as an event ID if the data was bad; otherwise, this is port-based user-data.

      Unparsed = -1,

      TrackerInit = 0,
      PredefMsg0 = 1,
      PredefMsg1 = 2,
      PredefMsg2 = 3,
      PredefMsg3 = 4,
      PredefMsg4 = 5,
      PredefMsg5 = 6,
      PredefMsg6 = 7,
      PredefMsg7 = 8,
      PredefMsg8 = 9,
      PredefMsg9 = 10,
      PredefMsg10 = 11,
      PredefMsg11 = 12,
      PredefMsg12 = 13,
      PredefMsg13 = 14,
      PredefMsg14 = 15,
      PredefMsg15 = 16,
      PredefMsg16 = 17,
      PredefMsg17 = 18,
      PredefMsg18 = 19,
      PredefMsg19 = 20,
      PredefMsg20 = 21,
      PredefMsg21 = 22,
      PredefMsg22 = 23,
      PredefMsg23 = 24,
      PredefMsg24 = 25,
      PredefMsg25 = 26,
      PredefMsg26 = 27,
      PredefMsg27 = 28,
      PredefMsg28 = 29,
      PredefMsg29 = 30,
      PredefMsg30 = 31,
      PredefMsg31 = 32,
      PredefMsg32 = 33,
      PredefMsg33 = 34,
      PredefMsg34 = 35,
      PredefMsg35 = 36,
      PredefMsg36 = 37,
      PredefMsg37 = 38,
      PredefMsg38 = 39,
      PredefMsg39 = 40,
      PredefMsg40 = 41,
      MaterialLeftOnReusable = 42,
      MaterialLeftOnNotReusable = 43,
      WaterAdded = 44,
      MixerStatusLoading = 45,
      MixerStatusStartPour = 46,
      MixerStatusEndPour = 47,
      MixerStatusWashOut = 48,
      //TrackerMileage =  49,
      IgnitionOn = 50,
      IgnitionOff = 51,
      WaterAddedFlowMetered = 52,
      Input1On = 53,
      Input1Off = 54,
      Input2On = 55,
      Input2Off = 56,
      Input3On = 57,
      Input3Off = 58,
      EmergencyArriveScene = 59,
      EmergencyArriveHospital = 60,
      EmergencyLightsOn = 61,
      EmergencyLightsOff = 62,
      EngineOverRev = 63,
      StatusHeadTrafficEnroute = 64,
      StatusHeadTrafficStaged = 65,
      StatusHeadTrafficOnScene = 66,
      StatusHeadTrafficDepartScene = 67,
      StatusHeadTrafficArriveAtHospital = 68,
      StatusHeadTrafficAvailable = 69,
      StatusHeadTrafficOutOfService = 70,
      StatusHeadTrafficNumberOfPatients = 71,
      StatusHead999Emergency = 72,
      StatusHeadCallSignBegin = 73,
      StatusHeadCallSignChangeShift = 74,
      StatusHeadCallSignEndShift = 75,
      StatusHeadOutOfService = 76,
      StatusHeadInService = 77,
      StatusHeadChangeCallSign = 78,
      StatusHeadInitialPersonnelChangeData = 79,
      StatusHeadInitialPersonnelChangeDataA = 80,
      StatusHeadInitialPersonnelChangeDataB = 81,
      StatusHeadPersonnelIdChange = 82,
      StatusHeadOutOfServicePeriod = 83,
      StatusHeadTrackerSlotDelayConfirm = 84,
      // The following Ss event IDs are events as a side effect of the SiteStatus message.  When an inbound receives
      // a SiteStatus message, it creates an artificial event with a null user data value, but with one of the following
      // event IDs.  This is used to produce events to the customer feed.
      SsEventIdArriveHome = 85,
      SsEventIdLeaveHome = 86,
      SsEventIdArriveInvalid = 87,
      SsEventIdLeaveInvalid = 88,
      SsEventIdArriveJob = 89,
      SsEventIdLeaveJob = 90,
      SsEventIdArriveCustomerDefined = 91,
      SsEventIdLeaveCustomerDefined = 92,
      StoppedNotificationStarted = 93,
      StoppedNotificationStopped = 94,
      SpeedingStarted = 95,
      SpeedingEnded = 96,
      EmsBeginLoading = 97,
      EmsEndLoading = 98,
      EmsLeavePlant = 99,
      EmsArriveJob = 100,
      EmsBeginPour = 101,
      EmsFinishPour = 102,
      EmsBeginWash = 103,
      EmsLeaveJob = 104,
      EmsArrivePlant = 105,
      EmsWaterAdded = 106,
      EmsWaterAddedAtSite = 107,
      AggregateArrivePlant = 108,
      AggregateBeginLoading = 109,
      AggregateEndLoading = 110,
      AggregateLeavePlant = 111,
      AggregateArriveJob = 112,
      AggregateBeginUnloading = 113,
      AggregateEndUnloading = 114,
      AggregateDepartJob = 115,
      EmsEndWash = 116,
      SecurityService = 117,
      DriverEnteredWeight = 118,
      DriverAbsent = 119,
      DriverPresent = 120,
      DriverOffBreak = 121,
      DriverOnBreak = 122,
      DriverBreakReqAccept = 123,
      DriverBreakReqReject = 124,
      SiteStatusArrival = 125,
      SiteStatusDeparture = 126,
      // The event IDs starting with Ds are artificial events generated from the DriverStatus high-level message.
      // When an inbound receives a DriverStatus message, we create an event in OEM of one of these three IDs.
      // [15-Mar-2004:PMSO]
      DsEventIdDriverLogon = 127,
      DsEventIdDriverLogoff = 128,
      DsEventIdDriverLogonUnknown = 129,
      OrderBack = 130,
      FuelAdded = 131,
      OilAdded  = 132,
      // New events for the JBUS data [2-Sep-2004:PMSO]
      JbusQueryComplete = 133,
      JbusQueryTimeout = 134,
      JbusCommunicationsError = 135,
      JbusBusNotAvailable = 136,
      JbusInvalidParameters = 137,
      JbusQueryBufferFull = 138,
      JbusQueryInProgress = 139,
      // New events for the SBC pilot [2-Sep-2004:PMSO]
      SbcIgnitionOn = 140,
      SbcIgnitionOff = 141,
      SbcRoll = 142,
      SbcDistance = 143,
      SbcSpeed = 144,
      SbcTime = 145,
      SbcIdle = 146,
      SbcPoll = 147,
      EmsFirstSlump = 148,    // Part of adding ReadySlump support to the extended ready-mix status message
      MaterialAddedFlowMetered = 149, // Part of ReadySlump support since we need to distinguish what was added.
      TicketRejected = 150,   // Part of adding ReadySlump
      FreeFormText = 151,     // Currently only used by the XMDU
      XmduJbusQueryComplete = 152,

      JBusAlertReport              = 600,
      JBusFaultReport              = 601,
      MachineEvent                 = 602


      // *****************************************
      // **         R E M E M B E R ! !         **
      // *****************************************
      // Nothing will work unless you add a row to
      // tracker_event_description for each new
      // event added here.  Remember to update the
      // crazy object_serial value for t_e_d.
      // *****************************************
      // **         R E M E M B E R ! !         **
      // *****************************************
   }

   #endregion

   #region Interfaces

   public interface INeedsAcknowledgement 
   {
      Type  AcknowledgementType  { get; }

      ProtocolMessage   GenerateAcknowledgement();
      bool              IsMessageForAcknowledgement(ProtocolMessage ackMessage);
   }

   public interface IDeprecated 
   {
      // Strictly a marker interface used to indicate a message no longer formerly supported.

      string   DeprecationIndication { get; }   // Returns a reason to add to a log file.
   }

   public interface IUtcTime
   {
      DateTime    DateRelativeToReceiveTime(DateTime receiveTime);
      DateTime    UtcDateTime { get; set; }
   }


   public enum Origin 
   {
      OutgoingEventMessage,
      OutgoingStateMessage,
      MessageResponseMessage,
      CoherencyEventAssetMessage,
      CoherencyEventWorksiteMessage,
      CoherencyEventMonitoringUser,
      CoherencyEventDriver
   }

   /// <summary>
   /// Classes implementing IXmlEventFragment are available to the event feed. IHighLevelPlatformEvent is a
   /// superset interface that includes the packet and event times. This interface is mostly used by user-data
   /// messages (duh) to implement what is needed to publish a -portion- of the XML event.  For example, the
   /// user-data tracker messages implement the interface and use XmlEvent to publish the properties of the
   /// user-data high-level message then defer to the user-data message to publish its specific properties.  It
   /// is left to something outside the PlatformMessages hierarchy to generate the event envelope with the event
   /// and packet GMTs (since that isn't available to the messages). The IHighLevelPlatformEvent interface
   /// includes methods for publishing a full event including the times.  Clients to the PlatformEvents classes
   /// receive arrays of IHighLevelPlatformEvent.
   /// </summary>
   public interface IXmlEventFragment 
   {
      /// <summary>
      /// Method creates the XML for the platform's real-time event feed.
      /// </summary>
      /// <param name="xw">The writer used to generate the event XML.</param>
      /// <param name="version">Version of the event XML to create. 1.0 is the legacy realtime
      /// with all its special cases; 2.0 is a format that's easier to deserialize.</param>
      void XmlEvent( XmlWriter xw, int version );

      /// <summary>
      /// Returns true if the event associated with the platform message is published.  Sometimes a platform
      /// message has an associated event, but whether it is published or not depends on values in the
      /// particular instance.  Further, we have in the past needed to mute certain messages based on the
      /// messaging version we want to maintain (which we can do through the config file).
      /// </summary>
      bool EventIsPublished { get; }

      /// <summary>
      /// Returns the platform event ID number for the message.  Note that this ID number is often dependent on
      /// the values in the message.
      /// </summary>
      UDEventID EventID { get; }
   }

   public interface ICsvValues 
   {
      /// <summary>
      /// Returns the CSV (comma-separated values) string containing the parsed event data typically used for
      /// reports. It is stored in NCCDB.Outgoing_Event_Msg.Data_Value for reports to access, however guerrilla
      /// IO needs it as well for backward compatibility.
      /// </summary>
      string CsvValues { get; }
   }

   public interface ICoordinate 
   {
      Double  Latitude  { get; set; }
      Double  Longitude { get; set; }
   }

   public interface ISpeedHeading
   {
      Double   Speed    { get; set; }
      Double   Heading  { get; set; }
   }

   public interface IMileage
   {
      Double   Mileage { get; set; }
   }
     
   public interface IAssetInfo 
   {
      Int64    AssetID    { get; }
      string   AssetName  { get; }
      Int64    OwnerOrgID { get; }
   }

   public class AssetGroupOwnerPair 
   {
      public Int64   AssetGroupID;
      public Int64   OwningOrgID;
      public string  AssetGroupName;
   }

   /// <summary>
   /// A HighLevelPlatformEvent is a wrapper to a realtime event XML fragment. This wrapper includes the items that
   /// don't appear in a single platform message (for example, the message contains the data from the tracker, but
   /// the timestamps associated with it are external to the message as well as the owner ID of who owns the
   /// message.
   /// </summary>
   public interface IHighLevelPlatformEvent : IXmlEventFragment
   {
      MessageCategory Category { get; }

      DateTime   PacketGmt   { get; }
      DateTime   EventGmt    { get; }

      Int64      OwnerOrgID  { get; }
      Int64      SequenceID  { get; }  // A number to assist in sorting; when sorting by date, if there's a tie, we take this in ascending order.
   }

   public enum MessageCategory
   {
      Event,
      Position,
      Message,
      Coherency
   }

   public interface IHighLevelPlatformEventAdditionalDescription
   {
      void  AdditionalDescriptionForViewer(TextWriter tw);
   }

   #endregion

   #region HighLevelPlatformEventComparer

    #endregion

   public abstract class ProtocolMessage 
   {
      public override string ToString()
      {
         // Get the name of our type.  Get the name of our direct base class.  If the current type ends in the
         // base class, remove it (i.e., SiteStatusTrackerMessage, which derives off TrackerMessage becomes
         // SiteStatus).

         Type   ourType  = GetType();
         string ourName  = ourType.Name;
         string baseName = ourType.BaseType.Name;

         if (ourName.EndsWith(baseName)) 
         {
            return ourName.Substring(0, ourName.Length-baseName.Length);
         }

         return ourName;
      }
   }
}
