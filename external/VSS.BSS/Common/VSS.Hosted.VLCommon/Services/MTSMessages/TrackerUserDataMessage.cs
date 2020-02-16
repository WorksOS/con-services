using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using VSS.Hosted.VLCommon;
using System.Collections.Generic;
using Microsoft.Web.Script.Serialization;

namespace VSS.Hosted.VLCommon.MTSMessages
{
    // ------------------------------------------------------------------------------------------


    public class IgnitionOnOffUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x07;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get { return On ? UDEventID.IgnitionOn : UDEventID.IgnitionOff; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 15, ref RunTimeCounterHours);
      serializer(action, raw, ref bitPosition, 1, ref On);

      if (action == SerializationAction.Hydrate && hydrationFinalBitPosition > bitPosition)
      {
        // Still room for the sneaky fields.

        AbsoluteRunTimeSpecified = true;
      }

      if (AbsoluteRunTimeSpecified)
      {
        serializer(action, raw, ref bitPosition, 32, ref AbsoluteRunTimeRaw);
      }
    }

    public UInt16 RunTimeCounterHours;
    public bool On;

    public bool AbsoluteRunTimeSpecified;
    private UInt32 AbsoluteRunTimeRaw;

    public double AbsoluteRunTime
    {
      get { return AbsoluteRunTimeRaw / 10.0; }
      set { AbsoluteRunTimeRaw = (UInt32)(value * 10.0); AbsoluteRunTimeSpecified = true; }
    }

    #region Other IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      xw.WriteElementString("StartFlag", PlatformEvents.Constants.RealtimeNamespace, ToTrueFalse(On, version));
      xw.WriteElementString("RTC", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(RunTimeCounterHours));

      if (AbsoluteRunTimeSpecified)
      {
        xw.WriteElementString("AbsoluteRTC", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(AbsoluteRunTime));
      }
    }

    public string CsvValues
    {
      get
      {
        return JoinAsCsv(new Object[]{
                                                    On,
                                                    //HACK: they want to mess up this JBUS information to make reporting work
                                                    (AbsoluteRunTimeSpecified ? AbsoluteRunTime : RunTimeCounterHours),
                                                    AbsoluteRunTime
                                                 });
      }
    }

    #endregion
  }


  public class DiscreteInputUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x09;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get
      {
        // I rely on the order of the UDEventIDs for discrete inputs, but these are fixed in stone.
        // Order is Input1On = 53, Input1Off = 54, Input2On = 55, Input2Off = 56, Input3On = 57, Input3Off = 58.

        return (UDEventID)(UDEventID.Input1On +
                                   ((DiscreteSelector - 1) * 2) +
                                   (DiscreteOn ? 0 : 1));
      }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 3, ref DiscreteSelectorRaw);
      filler(ref bitPosition, 5);
      serializer(action, raw, ref bitPosition, 1, ref DiscreteOn);
      filler(ref bitPosition, 7);
    }

    private byte DiscreteSelectorRaw;
    public bool DiscreteOn;

    public byte DiscreteSelector // A 1-based number
    {
      get
      {
        return (byte)(LowestBitIndexSet(DiscreteSelectorRaw) + 1);  // It is a 1-based number.
      }
      set { DiscreteSelectorRaw = (byte)(1 << (value - 1)); }
    }

    #region Other IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      // DiscreteInput messages are handled by the main wrapper client since the device doesn't have
      // enough information to produce the message. There's an alarm element whose value comes from
      // the CSV values pushed to the database plus the definition of whether the input is actually
      // on or not comes from the CSV values, too. This is mostly because this message is very
      // screwed up. Okay, it doesn't necessarily know if an alarm is associated with the input so
      // that's forgivable, but the 'DiscreteOn' bit actually means "input went high", which is an
      // on event for an active-high device, off otherwise. They could have returned true 'on', but
      // chose not to.

      // Anyway, since the wrapper generates our message, we really don't need to do anything here.
    }

    public string CsvValues
    {
      get
      {
        // The CSV values are pretty hacked up in the database since they take the message values
        // and churn them through the sensor installation table. We'll have to revisit this later
        // with the new platform to see what we really need.  The following is just a guess.

        return JoinAsCsv(new Object[]{
                                                    DiscreteSelector,
                                                    DiscreteOn
                                                 });
      }
    }

    #endregion
  }

    public class SpeedingIndicationUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x0d;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get { return BeginSpeeding ? UDEventID.SpeedingStarted : UDEventID.SpeedingEnded; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 16, ref DurationSeconds);
      serializer(action, raw, ref bitPosition, 16, ref DistanceTraveledMilesRaw);
      serializer(action, raw, ref bitPosition, 8, ref BeginSpeeding);
      serializer(action, raw, ref bitPosition, 8, ref MaximumSpeedMph);
    }

    public UInt16 DurationSeconds;
    private UInt16 DistanceTraveledMilesRaw;
    public bool BeginSpeeding;
    public byte MaximumSpeedMph;

    public double DistanceTraveledMiles
    {
      get { return DistanceTraveledMilesRaw * Constants.DistanceTraveledInTenthsConversionMultiplier; }
      set { DistanceTraveledMilesRaw = (UInt16)(value / Constants.DistanceTraveledInTenthsConversionMultiplier); }
    }

    #region Other IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      xw.WriteElementString("Duration", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(DurationSeconds));
      xw.WriteElementString("Distance", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(DistanceTraveledMiles));
      xw.WriteElementString("StartFlag", PlatformEvents.Constants.RealtimeNamespace, ToTrueFalse(BeginSpeeding, version));
      xw.WriteElementString("MaxSpeed", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(MaximumSpeedMph));
    }

    public string CsvValues
    {
      get
      {
        return JoinAsCsv(new Object[]{
                                                    DurationSeconds,
                                                    DistanceTraveledMiles,
                                                    BeginSpeeding,
                                                    MaximumSpeedMph
                                                 });
      }
    }

    #endregion
  }

  public class StoppedNotificationUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x0e;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {

      get { return StoppedMoving ? UDEventID.StoppedNotificationStopped : UDEventID.StoppedNotificationStarted; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 1, ref StoppedMoving);
      serializer(action, raw, ref bitPosition, 1, ref SuspiciousMove);
      filler(ref bitPosition, 6);
    }

    public bool StoppedMoving;
    public bool SuspiciousMove;

    #region Other IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      xw.WriteElementString("StartFlag", PlatformEvents.Constants.RealtimeNamespace, ToTrueFalse(!StoppedMoving, version));
    }

    public string CsvValues
    {
      get
      {
        return JoinAsCsv(new Object[]{
                                                    !StoppedMoving
                                                 });
      }
    }

    #endregion
  }

    //this has been tested but is no used by the crosscheck dimmessages are acked using the MessageResponse

    public class SiteStatusUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues, TrackerMessage.ISiteID
  {
    public static new readonly int kPacketID = 0x18;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      //get { return Departure ? UDEventID.SiteStatusDeparture : UDEventID.SiteStatusArrival; }

      get
      {
        return (UDEventID)
           ((SiteType == DeviceSiteType.Home) ? UDEventID.SsEventIdArriveHome :
            (SiteType == DeviceSiteType.CustomerDefined) ? UDEventID.SsEventIdArriveCustomerDefined :
            (SiteType == DeviceSiteType.Job) ? UDEventID.SsEventIdArriveJob :
                                                           UDEventID.SsEventIdArriveInvalid) +
           (Departure ? 1 : 0);
      }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 2, ref SiteTypeRaw);
      serializer(action, raw, ref bitPosition, 1, ref Departure);
      serializer(action, raw, ref bitPosition, 1, ref AutomaticSource);
      serializer(action, raw, ref bitPosition, 1, ref UserSource);
      filler(ref bitPosition, 3);
      serializer(action, raw, ref bitPosition, 32, ref SiteIDRaw);
    }

    private byte SiteTypeRaw;
    public bool Departure;
    public bool AutomaticSource;
    public bool UserSource;
    private Int64 SiteIDRaw;

    #region Implementation of ISiteID

    public Int64 SiteID
    {
      get { return SiteIDRaw; }
      set { SiteIDRaw = value; }
    }

    #endregion

    public DeviceSiteType SiteType
    {
      get { return (DeviceSiteType)SiteTypeRaw; }
      set { SiteTypeRaw = (byte)value; }
    }

    #region Other IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      string statusSource = (AutomaticSource && UserSource) ? "GPS and User" :
                             UserSource ? "User" :
                             AutomaticSource ? "GPS" : "INVALID";

      xw.WriteElementString("SiteType", PlatformEvents.Constants.RealtimeNamespace,
         (SiteType == DeviceSiteType.Home ? "Home" :
          SiteType == DeviceSiteType.Job ? "Job" : "Unknown"));

      xw.WriteElementString("SiteStatus", PlatformEvents.Constants.RealtimeNamespace, (Departure ? "Depart" : "Arrive"));
      xw.WriteElementString("StatusSource", PlatformEvents.Constants.RealtimeNamespace, statusSource);
    }

    public string CsvValues
    {
      get
      {
        return JoinAsCsv(new Object[]{
                                                    AutomaticSource,
                                                    UserSource,
                                                    SiteID
                                                 });
      }
    }

    #endregion
  }

    [XmlInclude(typeof(JbusDataUserDataMessage.AlertReportItem))]
  [XmlInclude(typeof(JbusDataUserDataMessage.FaultReportItem))]
  public class JbusDataUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x1C;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get
      {
        switch (SubType)
        {
          case QuerySubType.AdHocQueryResponse:
            return (UDEventID)((int)UDEventID.JbusQueryComplete + StatusRaw);

          case QuerySubType.AlertReport:
            return UDEventID.JBusAlertReport;

          case QuerySubType.FaultReport:
            return UDEventID.JBusFaultReport;

          case QuerySubType.ActiveMidReport:
          case QuerySubType.ActivePidReport:
          default:
            return UDEventID.Unparsed;

          case QuerySubType.XmduBlock:
            return UDEventID.XmduJbusQueryComplete;
        }
      }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 8, ref SubTypeRaw);

      if (SubType == QuerySubType.AdHocQueryResponse)
      {
        serializer(action, raw, ref bitPosition, 8, ref Source.MIDRaw);
        serializer(action, raw, ref bitPosition, 8, ref Source.PIDRaw);
        serializer(action, raw, ref bitPosition, 8, ref Source.ExtendedPIDRaw);
        serializer(action, raw, ref bitPosition, 8, ref StatusRaw);

        uint lenInBytes = 0;

        if (action != SerializationAction.Hydrate)
        {
          // When we serialize, we need to know the data size first. Ranges of PIDs have specific sizes
          // while the rest are variable sized. For those that are variably sized, we need specific code
          // to set the size; i want to eliminate that as much as possible, but this works correctly for
          // now since we never serialize these messages except in a test application, which could merit
          // an update here.

          if (Source.PIDRaw > 0 && Source.PIDRaw <= 127)
          {
            lenInBytes = 1;
          }
          else if (Source.PIDRaw >= 128 && Source.PIDRaw <= 191)
          {
            lenInBytes = 2;
          }
          else
          {
            // Otherwise, these are variable length

            if ((BusMid)Source.MID == BusMid.Engine1)
            {
              switch ((EnginePid)Source.PID)
              {
                case EnginePid.TotalIdleFuelUsed:
                case EnginePid.TotalFuelUsed:
                case EnginePid.TotalPtoHours:
                  lenInBytes = 4;
                  break;
              }
            }
          }

          if (lenInBytes == 0)
          {
            hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;
          }
        }

        serializer(action, raw, ref bitPosition, 8, ref lenInBytes);
        serializer(action, raw, ref bitPosition, 8 * lenInBytes, ref DataValue);
      }
      else if (SubType == QuerySubType.FaultReport)
      {
        serializer(action, raw, ref bitPosition, 16, ref FaultTransientCount);

        ReportItems = (ReportItem[])
           serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ReportItems, typeof(FaultReportItem));
      }
      else if (SubType == QuerySubType.AlertReport)
      {
        // Pull in the alert items blocks.
        // The less than pretty notation is necessary since array covariance doesn't extend to ref parameters.

        ReportItems = (ReportItem[])
           serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, ReportItems, typeof(AlertReportItem));
      }
      else if (SubType == QuerySubType.ActiveMidReport || SubType == QuerySubType.ActivePidReport)
      {
        // Suck all the remaining bytes into the active bitmap.

        uint realDataLength = (action == SerializationAction.Hydrate) ? bytesLeftInMessage(bitPosition) :
                                                                       ((ActiveMap == null) ? 0u : (uint)ActiveMap.Length);

        serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref ActiveMap);
      }
      else if (SubType == QuerySubType.XmduBlock)
      {
        serializeFixedLengthString(action, raw, ref bitPosition, 17, ref XmduSources.VIN);

        serializer(action, raw, ref bitPosition, 32, ref XmduSources.OdometerRaw);
        serializer(action, raw, ref bitPosition, 32, ref XmduSources.EngineHoursRaw);
        serializer(action, raw, ref bitPosition, 8, ref XmduSources.SpeedMphRaw);
        serializer(action, raw, ref bitPosition, 16, ref XmduSources.RpmRaw);
        serializer(action, raw, ref bitPosition, 16, ref XmduSources.AverageFuelEconomyMpgRaw);
        serializer(action, raw, ref bitPosition, 8, ref XmduSources.EngineOilPressureLbfIn2Raw);
        serializer(action, raw, ref bitPosition, 8, ref XmduSources.EngineCoolantTempFRaw);
        serializer(action, raw, ref bitPosition, 16, ref XmduSources.BatteryVoltageVRaw);
      }
    }


    #region Scaled Property Accessors

    public Double ScaledValue
    {
      get { return DataValue * Source.PidScaling; }
      set { DataValue = (UInt32)(value / Source.PidScaling); }
    }

    public Double AverageFuelEconomyMpg
    {
      get { return DataValue / 256.0; }
      set { DataValue = (UInt32)(value * 256.0); }
    }

    public Double TotalIdleFuelUsedGal
    {
      get { return DataValue / 8.0; }
      set { DataValue = (UInt32)(value * 8.0); }
    }

    public Double TotalFuelUsedGal
    {
      get { return DataValue / 8.0; }
      set { DataValue = (UInt32)(value * 8.0); }
    }

    public Double EngineOilPressureLbfIn2
    {
      get { return DataValue / 2.0; }
      set { DataValue = (UInt32)(value * 2.0); }
    }

    public Double EngineCoolantTemperatureF
    {
      get { return DataValue; }
      set { DataValue = (UInt32)value; }
    }

    public Double BatteryPotentialV
    {
      get { return DataValue / 20.0; }
      set { DataValue = (UInt32)(value * 20.0); }
    }

    public PtoStatus PowerTakeOffStatus
    {
      get { return (PtoStatus)DataValue; }
      set { DataValue = (UInt32)value; }
    }

    public Double TotalPtoHours
    {
      get { return DataValue / 20.0; }
      set { DataValue = (UInt32)(value * 20.0); }
    }

    #endregion

    #region Enumerations

    [Flags]
    public enum PtoStatus
    {
      PtoMode = (1 << 7),
      ClutchSwitch = (1 << 6),
      BrakeSwitch = (1 << 5),
      AcceleratorSwitch = (1 << 4),
      ResumeSwitch = (1 << 3),
      CoastSwitch = (1 << 2),
      SetSwitch = (1 << 1),
      PtoControlSwitch = (1 << 0),

      None = 0
    }

    public enum BusMid
    {
      Engine1 = 128
    }

    public enum EnginePid
    {
      PowerTakeOffStatus = 89,
      EngineOilPressure = 100,
      EngineCoolantTemperature = 110,
      BatteryPotential = 168,
      AverageFuelEconomy = 185,
      TotalIdleFuelUsed = 236,
      TotalPtoHours = 248,
      TotalFuelUsed = 250,

      // Extensions for XMDU

      Speed = 84,
      RPM = 160,
      VIN = 237,
      Odometer = 245,
      TotalEngineHours = 247
    }

    public enum QuerySubType
    {
      AdHocQueryResponse,
      FaultReport,
      AlertReport,
      ActiveMidReport,
      ActivePidReport,
      XmduBlock
    }

    public enum QueryStatus
    {
      QueryComplete,
      QueryTimeout,
      CommunicationsError,
      BusNotAvailable,
      InvalidParameters,
      QueryBufferFull,
      QueryInProgress,
      UnknownValue7,
      JbusStale
    }

    private static string[] fmiDescriptions = new string[]{
                                                                 "Above normal operational range",
                                                                 "Below normal operational range",
                                                                 "Data erratic, intermittent, or incorrect",
                                                                 "Voltage above normal or shorted high",
                                                                 "Voltage below normal or shorted low",
                                                                 "Current below normal or open circuit",
                                                                 "Current above normal or grounded circuit",
                                                                 "Mechanical system not responding properly",
                                                                 "Abnormal frequency, pulse width, or period",
                                                                 "Abnormal update rate",
                                                                 "Abnormal rate of change",
                                                                 "Failure mode not identifiable",
                                                                 "Bad intelligent device or component",
                                                                 "Out of Calibration",
                                                                 "Special Instructions",
                                                                 "Reserved"
                                                              };

    #endregion

    #region JBusSource

    public struct JBusSource
    {
      internal byte MIDRaw;
      internal byte PIDRaw;
      internal byte ExtendedPIDRaw;

      public int MID
      {
        get { return MIDRaw; }
        set { MIDRaw = (byte)value; }
      }

      public int PID
      {
        get { return (PIDRaw < 255) ? PIDRaw : 256 + ExtendedPIDRaw; }
        set
        {
          int val = (int)value;

          if (val < 255)
          {
            PIDRaw = (byte)val;
          }
          else
          {
            PIDRaw = 255;
            ExtendedPIDRaw = (byte)(val - 256);
          }
        }
      }

      // The SID is the PID when considered in different semantic context; that context is outside of this unit.

      public int SID
      {
        get { return PID; }
        set { PID = value; }
      }

      public string MidText { get { return midText(MID); } }
      public string PidText { get { return pidText(PID); } }
      public string PidUnits { get { return pidUnits(PID); } }
      public Double PidScaling { get { return pidScaling(PID); } }
      public string SidText { get { return sidText(MID, PID); } }

      public void GenerateXml(XmlWriter xw, bool useSid)
      {
        xw.WriteStartElement("JBusSource", PlatformEvents.Constants.RealtimeNamespace);
        xw.WriteStartElement("MID", PlatformEvents.Constants.RealtimeNamespace);
        xw.WriteAttributeString("ID", String.Empty, MID.ToString());
        xw.WriteString(MidText);
        xw.WriteEndElement();

        if (useSid)
        {
          // This is really a SID

          xw.WriteStartElement("SID", PlatformEvents.Constants.RealtimeNamespace);
          xw.WriteAttributeString("ID", String.Empty, SID.ToString());
          xw.WriteString(SidText);
          xw.WriteEndElement();
        }
        else
        {
          xw.WriteStartElement("PID", PlatformEvents.Constants.RealtimeNamespace);
          xw.WriteAttributeString("ID", String.Empty, PID.ToString());
          xw.WriteAttributeString("Units", String.Empty, PidUnits);

          xw.WriteString(PidText);
          xw.WriteEndElement();
        }

        xw.WriteEndElement();
      }

      #region MID & PID to text mappings

      private static string midText(int mid)
      {
        string txt = midMappings[mid] as string;

        return (txt == null) ? "Unknown MID#" + mid.ToString() : txt;
      }

      private static string pidText(int pid)
      {
        pidMapping map = pidMappings[pid] as pidMapping;

        return (map == null) ? "Unknown PID#" + pid.ToString() : map.Name;
      }

      private static double pidScaling(int pid)
      {
        pidMapping map = pidMappings[pid] as pidMapping;

        return (map == null) ? 1.0 : map.ScalingFactor;
      }

      private static string pidUnits(int pid)
      {
        pidMapping map = pidMappings[pid] as pidMapping;

        return (map == null) ? String.Empty : map.Units;
      }

      private static string sidText(int mid, int sid)
      {
        // SIDs are scoped by their MID, but there is a common list of SIDs for those MIDs that don't have
        // a specific list. This is how it's done in automotive engineering, as well as counting bits from 1.

        Hashtable specifics = sidMappings[mid] as Hashtable;

        if (specifics == null)
        {
          // Get the common list.

          specifics = sidMappings[0] as Hashtable;
        }

        string txt = specifics[sid] as string;

        return (txt == null) ? "Unknown SID#" + sid.ToString() : txt;
      }

      private static Hashtable midMappings;
      private static Hashtable pidMappings;
      private static Hashtable sidMappings;

      private class pidMapping
      {
        public pidMapping(string name)
        {
          Name = name;
        }

        public pidMapping(string name, double scale, string units)
        {
          Name = name;
          ScalingFactor = scale;
          Units = units;
        }

        public string Name;
        public double ScalingFactor = 1.0;
        public string Units = String.Empty;
      }

      private static void addMidMapping(int mid, string name)
      {
        midMappings.Add(mid, name);
      }

      private static void addPidMapping(int pid, string name)
      {
        pidMappings.Add(pid, new pidMapping(name));
      }

      private static void addPidMapping(int pid, string name, double scale, string units)
      {
        pidMappings.Add(pid, new pidMapping(name, scale, units));
      }

      static JBusSource()
      {
        midMappings = new Hashtable();
        pidMappings = new Hashtable();
        sidMappings = new Hashtable();

        #region MIDs

        addMidMapping(128, "Engine #1");
        addMidMapping(129, "Turbocharger");
        addMidMapping(130, "Transmission");
        addMidMapping(131, "Power Takeoff");
        addMidMapping(132, "Axle Power Unit");
        addMidMapping(133, "Axle Trailer #1");
        addMidMapping(134, "Axle Trailer #2");
        addMidMapping(135, "Axle Trailer #3");
        addMidMapping(136, "Brakes Power Unit");
        addMidMapping(137, "Brakes Trailer #1");
        addMidMapping(138, "Brakes Trailer #2");
        addMidMapping(139, "Brakes Trailer #3");
        addMidMapping(140, "Instrument Cluster");
        addMidMapping(141, "Trip Recorder");
        addMidMapping(142, "Vehicle Management System");
        addMidMapping(143, "Fuel System");
        addMidMapping(144, "Cruise Control");
        addMidMapping(145, "Road Speed Indicator");
        addMidMapping(146, "Cab Climate Control");
        addMidMapping(147, "Cargo Refrigeration/Heating Trailer #1");
        addMidMapping(148, "Cargo Refrigeration/Heating Trailer #2");
        addMidMapping(149, "Cargo Refrigeration/Heating Trailer #3");
        addMidMapping(150, "Suspension Power Unit");
        addMidMapping(151, "Suspension Trailer #1");
        addMidMapping(152, "Suspension Trailer #2");
        addMidMapping(153, "Suspension Trailer #3");
        addMidMapping(154, "Diagnostic Systems Power Unit");
        addMidMapping(155, "Diagnostic Systems Trailer #1");
        addMidMapping(156, "Diagnostic Systems Trailer #2");
        addMidMapping(157, "Diagnostic Systems Trailer #3");
        addMidMapping(158, "Electrical Charging System");
        addMidMapping(159, "Proximity Detector Front");
        addMidMapping(160, "Proximity Detector Rear");
        addMidMapping(161, "Aerodynamic Control Unit");
        addMidMapping(162, "Vehicle Navigation Unit");
        addMidMapping(163, "Vehicle Security");
        addMidMapping(164, "Multiplex");
        addMidMapping(165, "Communication Unit—Ground");
        addMidMapping(166, "Tires Power Unit");
        addMidMapping(167, "Tires Trailer #1");
        addMidMapping(168, "Tires Trailer #2");
        addMidMapping(169, "Tires Trailer #3");
        addMidMapping(170, "Electrical");
        addMidMapping(171, "Driver Information Center");
        addMidMapping(172, "Off-board Diagnostics #1");
        addMidMapping(173, "Engine Retarder");
        addMidMapping(174, "Cranking/Starting System");
        addMidMapping(175, "Engine #2");
        addMidMapping(176, "Transmission Additional");
        addMidMapping(177, "Particulate Trap System");
        addMidMapping(178, "Vehicle Sensors to Data Converter");
        addMidMapping(179, "Data Logging Computer");
        addMidMapping(180, "Off-board Diagnostics #2");
        addMidMapping(181, "Communication Unit—Satellite");
        addMidMapping(182, "Off-board Programming Station");
        addMidMapping(183, "Engine #3");
        addMidMapping(184, "Engine #4");
        addMidMapping(185, "Engine #5");
        addMidMapping(186, "Engine #6");
        addMidMapping(187, "Vehicle Control Head Unit/Vehicle Management System #2");
        addMidMapping(188, "Vehicle Logic Control Unit/Vehicle Management System #3");
        addMidMapping(189, "Vehicle Head Signs");
        addMidMapping(190, "Refrigerant Management Protection and Diagnostics");
        addMidMapping(191, "Vehicle Location Unit—Differential Correction");
        addMidMapping(192, "Front Door Status Unit");
        addMidMapping(193, "Middle Door Status Unit");
        addMidMapping(194, "Rear Door Status Unit");
        addMidMapping(195, "Annunciator Unit");
        addMidMapping(196, "Fare Collection Unit");
        addMidMapping(197, "Passenger Counter Unit #1");
        addMidMapping(198, "Schedule Adherence Unit");
        addMidMapping(199, "Route Adherence Unit");
        addMidMapping(200, "Environment Monitor Unit/Auxiliary Cab Climate Control");
        addMidMapping(201, "Vehicle Status Points Monitor Unit");
        addMidMapping(202, "High Speed Communications Unit");
        addMidMapping(203, "Mobile Data Terminal Unit");
        addMidMapping(204, "Vehicle Proximity Right Side");
        addMidMapping(205, "Vehicle Proximity Left Side");
        addMidMapping(206, "Base Unit (Radio Gateway to Fixed End)");
        addMidMapping(207, "Bridge from SAE J1708 Drivetrain Link");
        addMidMapping(208, "Maintenance Printer");
        addMidMapping(209, "Vehicle Turntable");
        addMidMapping(210, "Bus Chassis Identification Unit");
        addMidMapping(211, "Smart Card Terminal");
        addMidMapping(212, "Mobile Data Terminal");
        addMidMapping(213, "Vehicle Control Head Touch Screen");
        addMidMapping(214, "Silent Alarm Unit");
        addMidMapping(215, "Surveillance Microphone");
        addMidMapping(216, "Lighting Control Administrator Unit");
        addMidMapping(217, "Tractor/Trailer Bridge Tractor Mounted");
        addMidMapping(218, "Tractor/Trailer Bridge Trailer Mounted");
        addMidMapping(219, "Collision Avoidance Systems");
        addMidMapping(220, "Tachograph");
        addMidMapping(221, "Driver Information Center #2");
        addMidMapping(222, "Driveline Retarder");
        addMidMapping(223, "Transmission Shift Console—Primary");
        addMidMapping(224, "Parking Heater");
        addMidMapping(225, "Weighing System Axle Group #1/Vehicle");
        addMidMapping(226, "Weighing System Axle Group #2");
        addMidMapping(227, "Weighing System Axle Group #3");
        addMidMapping(228, "Weighing System Axle Group #4");
        addMidMapping(229, "Weighing System Axle Group #5");
        addMidMapping(230, "Weighing System Axle Group #6");
        addMidMapping(231, "Communication Unit—Cellular");
        addMidMapping(232, "Safety Restraint System");
        addMidMapping(233, "Intersection Preemption Emitter");
        addMidMapping(234, "Instrument Cluster #2");
        addMidMapping(235, "Engine Oil Control System");
        addMidMapping(236, "Entry Assist Control #1");
        addMidMapping(237, "Entry Assist Control #2");
        addMidMapping(238, "Idle Adjust System");
        addMidMapping(239, "Passenger Counter Unit #2");
        addMidMapping(240, "Passenger Counter Unit #3");
        addMidMapping(241, "Fuel Tank Monitor");
        addMidMapping(242, "Axles Trailer #4");
        addMidMapping(243, "Axles Trailer #5");
        addMidMapping(244, "Diagnostic Systems Trailer #4");
        addMidMapping(245, "Diagnostic Systems Trailer #5");
        addMidMapping(246, "Brakes Trailer #4");
        addMidMapping(247, "Brakes Trailer #5");
        addMidMapping(248, "Forward Road Image Processor");
        addMidMapping(249, "Body Controller");
        addMidMapping(250, "Steering Column Unit");

        #endregion

        #region PIDs

        addPidMapping(0, "Request Parameter");
        addPidMapping(1, "Invalid Data Parameter");
        addPidMapping(2, "Transmitter System Status");
        addPidMapping(3, "Transmitter System Diagnostic");
        addPidMapping(5, "Underrange Warning Condition");
        addPidMapping(6, "Overrange Warning Condition");
        addPidMapping(7, "Axle #2 Lift Air Pressure", 0.6, "lbf/in2");
        addPidMapping(8, "Brake System Air Pressure Low Warning Switch Status");
        addPidMapping(9, "Axle Lift Status");
        addPidMapping(10, "Axle Slider Status");
        addPidMapping(11, "Cargo Securement");
        addPidMapping(12, "Brake Stroke Status");
        addPidMapping(13, "Entry Assist Position/Deployment");
        addPidMapping(14, "Entry Assist Motor Current", 0.04, "A");
        addPidMapping(15, "Fuel Supply Pump Inlet Pressure", 0.25, "lbf/in2");
        addPidMapping(16, "Suction Side Fuel Filter Differential Pressure", 0.25, "lbf/in2");
        addPidMapping(17, "Engine Oil Level Remote Reservoir");
        addPidMapping(18, "Extended Range Fuel Pressure", 0.58, "lbf/in2");
        addPidMapping(19, "Extended Range Engine Oil Pressure", 0.58, "lbf/in2");
        addPidMapping(20, "Extended Range Engine Coolant Pressure", 0.29, "lbf/in2");
        addPidMapping(21, "Engine ECU Temperature", 2.5, "F");
        addPidMapping(22, "Extended Engine Crankcase Blow-by Pressure", 0.004245, "lbf/in2");
        addPidMapping(23, "Generator Oil Pressure", 0.5, "lbf/in2");
        addPidMapping(24, "Generator Coolant Temperature", 1.0, "F");
        addPidMapping(25, "Air Conditioner System Status #2");
        addPidMapping(26, "Estimated Percent Fan Speed");
        addPidMapping(27, "Percent Exhaust Gas Recirculation Valve #1 Position");
        addPidMapping(28, "Percent Accelerator Position #3");
        addPidMapping(29, "Percent Accelerator Position #2");
        addPidMapping(30, "Crankcase Blow-by Pressure", 0.125, "lbf/in2");
        addPidMapping(31, "Transmission Range Position");
        addPidMapping(32, "Transmission Splitter Position");
        addPidMapping(33, "Clutch Cylinder Position");
        addPidMapping(34, "Clutch Cylinder Actuator Status");
        addPidMapping(35, "Shift Finger Actuator Status #2");
        addPidMapping(36, "Clutch Plates Wear Condition");
        addPidMapping(37, "Transmission Tank Air Pressure", 1.0, "lbf/in2");
        addPidMapping(38, "Second Fuel Level (Right Side)");
        addPidMapping(39, "Tire Pressure Check Interval", 1.0, "min");
        addPidMapping(40, "Engine Retarder Switches Status");
        addPidMapping(41, "Cruise Control Switches Status");
        addPidMapping(42, "Pressure Switch Status");
        addPidMapping(43, "Ignition Switch Status");
        addPidMapping(44, "Attention/Warning Indicator Lamps Status");
        addPidMapping(45, "Inlet Air Heater Status");
        addPidMapping(46, "Vehicle Wet Tank Pressure", 1.0, "lbf/in2");
        addPidMapping(47, "Retarder Status");
        addPidMapping(48, "Extended Range Barometric Pressure", 0.087, "lbf/in2");
        addPidMapping(49, "ABS Control Status");
        addPidMapping(50, "Air Conditioner System Clutch Status/Command #1");
        addPidMapping(51, "Throttle Position");
        addPidMapping(52, "Engine Intercooler Temperature", 1.0, "F");
        addPidMapping(53, "Transmission Synchronizer Clutch Value");
        addPidMapping(54, "Transmission Synchronizer Brake Value");
        addPidMapping(55, "Shift Finger Positional Status");
        addPidMapping(56, "Transmission Range Switch Status");
        addPidMapping(57, "Transmission Actuator Status #2");
        addPidMapping(58, "Shift Finger Actuator Status");
        addPidMapping(59, "Shift Finger Gear Position");
        addPidMapping(60, "Shift Finger Rail Position");
        addPidMapping(61, "Parking Brake Actuator Status");
        addPidMapping(62, "Retarder Inhibit Status");
        addPidMapping(63, "Transmission Actuator Status #1");
        addPidMapping(64, "Direction Switch Status");
        addPidMapping(65, "Service Brake Switch Status");
        addPidMapping(66, "Vehicle Enabling Component Status");
        addPidMapping(67, "Shift Request Switch Status");
        addPidMapping(68, "Torque Limiting Factor");
        addPidMapping(69, "Two Speed Axle Switch Status");
        addPidMapping(70, "Parking Brake Switch Status");
        addPidMapping(71, "Idle Shutdown Timer Status");
        addPidMapping(72, "Blower Bypass Value Position");
        addPidMapping(73, "Auxiliary Water Pump Pressure", 2.0, "lbf/in2");
        addPidMapping(74, "Maximum Road Speed Limit", 0.805, "km/h");
        addPidMapping(75, "Steering Axle Temperature", 1.2, "F");
        addPidMapping(76, "Axle #1 Lift Air Pressure", 0.6, "lbf/in2");
        addPidMapping(77, "Forward Rear Drive Axle Temperature", 1.2, "F");
        addPidMapping(78, "Rear Rear-Drive Axle Temperature", 1.2, "F");
        addPidMapping(79, "Road Surface Temperature", 2.5, "F");
        addPidMapping(80, "Washer Fluid Level");
        addPidMapping(81, "Particulate Trap Inlet Pressure", 0.05, "in Hg");
        addPidMapping(82, "Air Start Pressure", 0.6, "lbf/in2");
        addPidMapping(83, "Road Speed Limit Status");
        addPidMapping(84, "Road Speed", 0.5, "mph");
        addPidMapping(85, "Cruise Control Status");
        addPidMapping(86, "Cruise Control Set Speed", 0.5, "mph");
        addPidMapping(87, "Cruise Control High-Set Limit Speed", 0.5, "mph");
        addPidMapping(88, "Cruise Control Low-Set Limit Speed", 0.5, "mph");
        addPidMapping(89, "Power Takeoff Status");
        addPidMapping(90, "PTO Oil Temperature", 1.2, "F");
        addPidMapping(91, "Percent Accelerator Pedal Position");
        addPidMapping(92, "Percent Engine Load");
        addPidMapping(93, "Output Torque", 20, "lbf-ft");
        addPidMapping(94, "Fuel Delivery Pressure", 0.5, "lbf/in2");
        addPidMapping(95, "Fuel Filter Differential Pressure", 0.25, "lbf/in2");
        addPidMapping(96, "Fuel Level");
        addPidMapping(97, "Water in Fuel Indicator");
        addPidMapping(98, "Engine Oil Level");
        addPidMapping(99, "Engine Oil Filter Differential Pressure", 0.0625, "lbf/in2");
        addPidMapping(100, "Engine Oil Pressure", 0.5, "lbf/in2");
        addPidMapping(101, "Crankcase Pressure", 0.125, "lbf/in2");
        addPidMapping(102, "Boost Pressure", 0.125, "lbf/in2");
        addPidMapping(103, "Turbo Speed", 500, "rpm");
        addPidMapping(104, "Turbo Oil Pressure", 0.6, "lbf/in2");
        addPidMapping(105, "Intake Manifold Temperature", 1.0, "F");
        addPidMapping(106, "Air Inlet Pressure", 0.25, "lbf/in2");
        addPidMapping(107, "Air Filter Differential Pressure", 0.2, "in H2O");
        addPidMapping(108, "Barometric Pressure", 0.0625, "lbf/in2");
        addPidMapping(109, "Coolant Pressure", 0.125, "lbf/in2");
        addPidMapping(110, "Engine Coolant Temperature", 1.0, "F");
        addPidMapping(111, "Coolant Level");
        addPidMapping(112, "Coolant Filter Differential Pressure", 0.0625, "lbf/in2");
        addPidMapping(113, "Governor Droop", 2.0, "rpm");
        addPidMapping(114, "Net Battery Current", 1.2, "A");
        addPidMapping(115, "Alternator Current", 1.2, "A");
        addPidMapping(116, "Brake Application Pressure", 0.6, "lbf/in2");
        addPidMapping(117, "Brake Primary Pressure", 0.6, "lbf/in2");
        addPidMapping(118, "Brake Secondary Pressure", 0.6, "lbf/in2");
        addPidMapping(119, "Hydraulic Retarder Pressure", 0.6, "lbf/in2");
        addPidMapping(120, "Hydraulic Retarder Oil Temperature", 2.0, "F");
        addPidMapping(121, "Engine Retarder Status");
        addPidMapping(122, "Engine Retarder Percent");
        addPidMapping(123, "Clutch Pressure", 2.0, "lbf/in2");
        addPidMapping(124, "Transmission Oil Level");
        addPidMapping(125, "Transmission Oil Level High/Low", 1.0, "pt");
        addPidMapping(126, "Transmission Filter Differential Pressure", 0.25, "lbf/in2");
        addPidMapping(127, "Transmission Oil Pressure", 2.0, "lbf/in2");

        // Double Data Character Length Parameters

        addPidMapping(128, "Component-specific request");
        addPidMapping(129, "Injector Metering Rail #2 Pressure", 0.1, "lbf/in2");
        addPidMapping(130, "Power Specific Fuel Economy", 0.01, "hp-h/gal");
        addPidMapping(131, "Exhaust Back Pressure");
        addPidMapping(132, "Mass Air Flow", 0.2756, "lb/min");
        addPidMapping(133, "Average Fuel Rate", 1 / 64.0, "gal/h");
        addPidMapping(134, "Wheel Speed Sensor Status");
        addPidMapping(135, "Extended Range Fuel Delivery Pressure (Absolute)", 0.1, "lbf/in2");
        addPidMapping(136, "Auxiliary Vacuum Pressure Reading", 0.1, "lbf/in2");
        addPidMapping(137, "Auxiliary Gage Pressure Reading #1", 0.1, "lbf/in2");
        addPidMapping(138, "Auxiliary Absolute Pressure Reading", 0.1, "lbf/in2");
        addPidMapping(139, "Tire Pressure Control System Channel Functional Mode");
        addPidMapping(140, "Tire Pressure Control System Solenoid Status");
        addPidMapping(141, "Trailer #1 Tag #1 or Push Channel #1 Tire Pressure Target", 0.1, "lbf/in2");
        addPidMapping(142, "Drive Channel Tire Pressure Target", 0.1, "lbf/in2");
        addPidMapping(143, "Steer Channel Tire Pressure Target", 0.1, "lbf/in2");
        addPidMapping(144, "Trailer #1 Tag #1 or Push Channel #1 Tire Pressure", 0.1, "lbf/in2");
        addPidMapping(145, "Drive Channel Tire Pressure", 0.1, "lbf/in2");
        addPidMapping(146, "Steer Channel Tire Pressure", 0.1, "lbf/in2");
        addPidMapping(147, "Average Fuel Economy (Natural Gas)");
        addPidMapping(148, "Instantaneous Fuel Economy (Natural Gas)");
        addPidMapping(149, "Fuel Mass Flow Rate (Natural Gas)", 0.275, "lb/h");
        addPidMapping(150, "PTO Engagement Control Status");
        addPidMapping(151, "ATC Control Status");
        addPidMapping(152, "Number of ECU Resets");
        addPidMapping(153, "Crankcase Pressure", 1.133e-3, "lbf/in2");
        addPidMapping(154, "Auxiliary Input and Output Status #2");
        addPidMapping(155, "Auxiliary Input and Output Status #1");
        addPidMapping(156, "Injector Timing Rail Pressure", 0.1, "lbf/in2");
        addPidMapping(157, "Injector Metering Rail Pressure", 0.1, "lbf/in2");
        addPidMapping(158, "Battery Potential-Switched", 0.05, "V");
        addPidMapping(159, "Gas Supply Pressure", 0.05, "lbf/in2");
        addPidMapping(160, "Main Shaft Speed", 0.25, "rpm");
        addPidMapping(161, "Input Shaft Speed", 0.25, "rpm");
        addPidMapping(162, "Transmission Range Selected");
        addPidMapping(163, "Transmission Range Attained");
        addPidMapping(164, "Injection Control Pressure");
        addPidMapping(165, "Compass Bearing", 0.01, "deg");
        addPidMapping(166, "Rated Engine Power", 1.0, "hp");
        addPidMapping(167, "Alternator Potential", 0.05, "V");
        addPidMapping(168, "Battery Potential", 0.05, "V");
        addPidMapping(169, "Cargo Ambient Temperature", 0.25, "F");
        addPidMapping(170, "Cab Interior Temperature", 0.25, "F");
        addPidMapping(171, "Ambient Air Temperature", 0.25, "F");
        addPidMapping(172, "Air Inlet Temperature", 0.25, "F");
        addPidMapping(173, "Exhaust Gas Temperature", 0.25, "F");
        addPidMapping(174, "Fuel Temperature", 0.25, "F");
        addPidMapping(175, "Engine Oil Temperature", 0.25, "F");
        addPidMapping(176, "Turbo Oil Temperature", 0.25, "F");
        addPidMapping(177, "Transmission #1 Oil Temperature", 0.25, "F");
        addPidMapping(178, "Front Axle Weight", 1.0, "lbf");
        addPidMapping(179, "Rear Axle Weight", 1.0, "lbf");
        addPidMapping(180, "Trailer Weight", 4.0, "lbf");
        addPidMapping(181, "Cargo Weight", 4.0, "lbf");
        addPidMapping(182, "Trip Fuel", 0.125, "gal");
        addPidMapping(183, "Fuel Rate (Instantaneous)", 1 / 64.0, "gal/h");
        addPidMapping(184, "Instantaneous Fuel Economy");
        addPidMapping(185, "Average Fuel Economy", 1 / 256.0, "mpg");
        addPidMapping(186, "Power Takeoff Speed", 0.25, "rpm");
        addPidMapping(187, "Power Takeoff Set Speed", 0.25, "rpm");
        addPidMapping(188, "Idle Engine Speed", 0.25, "rpm");
        addPidMapping(189, "Rated Engine Speed", 0.25, "rpm");
        addPidMapping(190, "Engine Speed", 0.25, "rpm");
        addPidMapping(191, "Transmission Output Shaft Speed", 0.25, "rpm");

        // Variable and Long Data Character Length Parameters

        addPidMapping(192, "Multisection Parameter");
        addPidMapping(193, "Transmitter System Diagnostic Table");
        addPidMapping(194, "Transmitter System Diagnostic Code and Occurrence Count Table");
        addPidMapping(195, "Diagnostic Data Request/Clear Count");
        addPidMapping(196, "Diagnostic Data/Count Clear Response");
        addPidMapping(197, "Connection Management");
        addPidMapping(198, "Connection Mode Data Transfer");
        addPidMapping(199, "Traction Control Disable State");
        addPidMapping(209, "ABS Control Status Trailer");
        addPidMapping(210, "Tire Temperature (By Sequence Number)", 2.5, "F");
        addPidMapping(211, "Tire Pressure (By Sequence Number)", 0.6, "psi");
        addPidMapping(212, "Tire Pressure Target (By Sequence Number)", 0.6, "psi");
        addPidMapping(213, "Wheel End Assembly Vibration Level", 1.0, "g");
        addPidMapping(214, "Vehicle Wheel Speeds", 0.5, "mph");
        addPidMapping(215, "Brake Temperature");
        addPidMapping(216, "Wheel Bearing Temperature");
        addPidMapping(217, "Fuel Tank/Nozzle Identification");
        addPidMapping(218, "State Line Crossing");
        addPidMapping(219, "Current State and Country");
        addPidMapping(220, "Engine Torque History");
        addPidMapping(221, "Anti-theft Request");
        addPidMapping(222, "Anti-theft Status");
        addPidMapping(223, "Auxiliary A/D Counts");
        addPidMapping(224, "Immobilizer Security Code");
        addPidMapping(225, "Reserved for Text Message Acknowledged");
        addPidMapping(226, "Reserved for Text Message to Display");
        addPidMapping(227, "Reserved for Text Message Display Type");
        addPidMapping(228, "Speed Sensor Calibration", 1, "pulse per mile");
        addPidMapping(229, "Total Fuel Used (Natural Gas)", 1.10, "lb");
        addPidMapping(230, "Total Idle Fuel Used (Natural Gas)", 1.10, "lb");
        addPidMapping(231, "Trip Fuel (Natural Gas)", 1.10, "lb");
        addPidMapping(232, "DGPS Differential Correction");
        addPidMapping(233, "Unit Number (Power Unit)");
        addPidMapping(234, "Software Identification");
        addPidMapping(235, "Total Idle Hours", 0.05, "h");
        addPidMapping(236, "Total Idle Fuel Used", 0.125, "gal");
        addPidMapping(237, "Vehicle Identification Number");
        addPidMapping(238, "Velocity Vector", 0.5, "mph");
        addPidMapping(239, "Vehicle Position");
        addPidMapping(240, "Change Reference Number");
        addPidMapping(241, "Tire Pressure by Position");
        addPidMapping(242, "Tire Temperature by Position");
        addPidMapping(243, "Component Identification");
        addPidMapping(244, "Trip Distance", 0.1, "mi");
        addPidMapping(245, "Total Vehicle Distance", 0.1, "mi");
        addPidMapping(246, "Total Vehicle Hours", 0.05, "h");
        addPidMapping(247, "Total Engine Hours", 0.05, "h");
        addPidMapping(248, "Total PTO Hours", 0.05, "h");
        addPidMapping(249, "Total Engine Revolutions", 1000, "r");
        addPidMapping(250, "Total Fuel Used", 0.125, "gal");
        addPidMapping(251, "Clock");
        addPidMapping(252, "Date");
        addPidMapping(253, "Elapsed Time");

        // Single Data Character Length Parameters (modulo 256 value identified in parentheses)

        addPidMapping(256, "Request Parameter");
        addPidMapping(257, "Cold Restart of Specific Component");
        addPidMapping(258, "Warm Restart of Specific Component");
        addPidMapping(259, "Component Restart Response");
        addPidMapping(362, "Percent Exhaust Gas Recirculation Valve #2 Position");
        addPidMapping(363, "Hydraulic Retarder Control Air Pressure", 0.6, "lbf/in2");
        addPidMapping(364, "HVAC Unit Discharge Temperature", 2.5, "F");
        addPidMapping(365, "Weighing System Status Command");
        addPidMapping(366, "Engine Oil Level High/Low", 1.0, "pt");
        addPidMapping(367, "Lane Tracking System Status");
        addPidMapping(368, "Lane Departure Indication");
        addPidMapping(369, "Distance to Rear Object (Reverse)", 0.328, "ft");
        addPidMapping(370, "Trailer Pneumatic Brake Control Line Pressure", 0.6, "lbf/in2");
        addPidMapping(371, "Trailer Pneumatic Supply Line Pressure", 0.6, "lbf/in2");
        addPidMapping(372, "Remote Accelerator");
        addPidMapping(373, "Center Rear Drive Axle Temperature", 1.2, "F");
        addPidMapping(374, "Alternator AC Voltage", 0.125, "V");
        addPidMapping(375, "Fuel Return Pressure", 0.5, "psi");
        addPidMapping(376, "Fuel Pump Inlet Vacuum", 0.2, "in Hg");
        addPidMapping(377, "Compression Unbalance");
        addPidMapping(378, "Fare Collection Unit Status");
        addPidMapping(379, "Door Status");
        addPidMapping(380, "Articulation Angle", 1, "deg");
        addPidMapping(381, "Vehicle Use Status");
        addPidMapping(382, "Transit Silent Alarm Status");
        addPidMapping(383, "Vehicle Acceleration", 0.2, "mph/s");

        // Double Data Character Length Parameters

        addPidMapping(384, "Component-specific request");
        addPidMapping(406, "HVAC Blower Motor Speed", 0.25, "rpm");
        addPidMapping(407, "Axle Group Full Weight Calibration", 4.0, "lbf");
        addPidMapping(408, "Axle Group Empty Weight Calibration", 4.0, "lbf");
        addPidMapping(409, "Axle Group Weight", 4.0, "lbf");
        addPidMapping(410, "Extended Range Road Surface Temperature", 0.25, "F");
        addPidMapping(411, "Recirculated Engine Exhaust Gas Differential Pressure");
        addPidMapping(412, "Recirculated Engine Exhaust Gas Temperature", 0.25, "F");
        addPidMapping(413, "Net Vehicle Weight Change", 4.0, "lbs");
        addPidMapping(414, "Air Conditioner Refrigerant Low Side Pressure", 0.20, "lbf/in2");
        addPidMapping(415, "Air Conditioner Refrigerant High Side Pressure", 0.20, "lbf/in2");
        addPidMapping(416, "Evaporator Temperature", 0.25, "F");
        addPidMapping(417, "Gross Vehicle Weight", 4.0, "lbf");
        addPidMapping(418, "Transmission # 2 Oil Temperature", 0.25, "F");
        addPidMapping(419, "Starter Circuit Resistance", 0.25, "milli-ohm");
        addPidMapping(420, "Starter Current (Average)", 0.125, "A");
        addPidMapping(421, "Alternator/Generator Negative Cable Voltage", 0.0001, "V");
        addPidMapping(422, "Auxiliary Current", 0.125, "A");
        addPidMapping(423, "Extended Range Net Battery Current", 0.125, "A");
        addPidMapping(424, "DC Voltage", 0.05, "V");
        addPidMapping(425, "Auxiliary Frequency", 0.1, "Hz");
        addPidMapping(426, "Alternator/Generator Field Voltage", 0.05, "V");
        addPidMapping(427, "Battery Resistance Change", 0.25, "mill-ohm/s");
        addPidMapping(428, "Battery Internal Resistance", 0.25, "mill-ohm");
        addPidMapping(429, "Starter Current Peak", 0.125, "A");
        addPidMapping(430, "Starter Solenoid Voltage", 0.05, "V");
        addPidMapping(431, "Starter Negative Cable Voltage", 0.0001, "V");
        addPidMapping(432, "Starter Motor Voltage", 0.05, "V");
        addPidMapping(433, "Fuel Shutoff Solenoid Voltage", 0.05, "V");
        addPidMapping(434, "AC Voltage", 0.125, "V");
        addPidMapping(435, "Cargo Ambient Temperature (By location)");
        addPidMapping(436, "Trip Sudden Decelerations");
        addPidMapping(437, "Trailer #2, Tag #2, or Push Channel #2 Tire Pressure Target", 0.1, "lbf/in2");
        addPidMapping(438, "Trailer #2, Tag #2, or Push Channel #2 Tire Pressure", 0.1, "lbf/in2");
        addPidMapping(439, "Extended Range Boost Pressure #1", 0.018, "lbf/in2");
        addPidMapping(440, "Extended Range Boost Pressure #2", 0.018, "lbf/in2");
        addPidMapping(441, "Auxiliary Temperature #1", 0.1, "F");
        addPidMapping(442, "Auxiliary Temperature #2", 0.1, "F");
        addPidMapping(443, "Auxiliary Gage Pressure Reading #2", 0.1, "lbf/in2");
        addPidMapping(444, "Battery #2 Potential", 0.05, "V");
        addPidMapping(445, "Cylinder Head Temperature Bank B (right bank)", 0.25, "F");
        addPidMapping(446, "Cylinder Head Temperature Bank A (left bank)", 0.25, "F");
        addPidMapping(447, "Passenger Counter");

        // Variable and Long Data Character Length Parameters

        addPidMapping(449, "Reporting Interval Request");
        addPidMapping(450, "Bridge Filter Control");
        addPidMapping(498, "Send Keypress Command");
        addPidMapping(499, "Driver Interface Unit (DIU) Object/Form Command");
        addPidMapping(500, "Intersection Preemption Status and Configuration");
        addPidMapping(501, "Signage Message");
        addPidMapping(502, "Fare Collection Unit—Point of Sale");
        addPidMapping(503, "Fare Collection Unit—Service Detail");
        addPidMapping(504, "Annunciator Voice Message");
        addPidMapping(505, "Vehicle Control Head Keyboard Message");
        addPidMapping(506, "Vehicle Control Head Display Message");
        addPidMapping(507, "Driver Identification");
        addPidMapping(508, "Transit Route Identification");
        addPidMapping(509, "Mile Post Identification");

        #endregion

        // Build the various SID mapping tables -sigh- this will be in a database after this release; for now,
        // I have to limit the update unit to the PlatformMessages assembly so I can't change the database
        // interface assembly (or get tables added to the db).

        Hashtable sidMapping = new Hashtable();

        #region SIDs

        // Common mappings

        sidMappings[0] = sidMapping;

        sidMapping.Add(151, "System Diagnostic Code #1");
        sidMapping.Add(152, "System Diagnostic Code #2");
        sidMapping.Add(153, "System Diagnostic Code #3");
        sidMapping.Add(154, "System Diagnostic Code #4");
        sidMapping.Add(155, "System Diagnostic Code #5");
        sidMapping.Add(207, "Battery #1 Temperature");
        sidMapping.Add(208, "Battery #2 Temperature");
        sidMapping.Add(209, "Start Enable Device #2");
        sidMapping.Add(210, "Oil Temperature Sensor");
        sidMapping.Add(211, "Sensor Supply Voltage #2 (+5V DC)");
        sidMapping.Add(212, "Sensor Supply Voltage #1 (+5V DC)");
        sidMapping.Add(213, "PLC Data Link");
        sidMapping.Add(214, "ECU Backup Battery");
        sidMapping.Add(215, "Cab Interior Temperature Thermostat");
        sidMapping.Add(216, "Other ECUs Have Reported Fault Codes Affecting Operation");
        sidMapping.Add(217, "Anti-theft Start Inhibit (Password Valid Indicator)");
        sidMapping.Add(218, "ECM Main Relay");
        sidMapping.Add(219, "Start Signal Indicator");
        sidMapping.Add(220, "Electronic Tractor/Trailer Interface (ISO 11992)");
        sidMapping.Add(221, "Internal Sensor Voltage Supply");
        sidMapping.Add(222, "Protect Lamp");
        sidMapping.Add(223, "Ambient Light Sensor");
        sidMapping.Add(224, "Audible Alarm");
        sidMapping.Add(225, "Green Lamp");
        sidMapping.Add(226, "Transmission Neutral Switch");
        sidMapping.Add(227, "Auxiliary Analog Input #1");
        sidMapping.Add(228, "High Side Refrigerant Pressure Switch");
        sidMapping.Add(229, "Kickdown Switch");
        sidMapping.Add(230, "Idle Validation Switch");
        sidMapping.Add(231, "SAE J1939 Data Link");
        sidMapping.Add(232, "5 Volts DC Supply");
        sidMapping.Add(233, "Controller #2");
        sidMapping.Add(234, "Parking Brake On Actuator");
        sidMapping.Add(235, "Parking Brake Off Actuator");
        sidMapping.Add(236, "Power Connect Device");
        sidMapping.Add(237, "Start Enable Device");
        sidMapping.Add(238, "Diagnostic Lamp—Red");
        sidMapping.Add(239, "Diagnostic Light—Amber");
        sidMapping.Add(240, "Program Memory");
        sidMapping.Add(241, "Systems Diagnostics");
        sidMapping.Add(242, "Cruise Control Resume Switch");
        sidMapping.Add(243, "Cruise Control Set Switch");
        sidMapping.Add(244, "Cruise Control Enable Switch");
        sidMapping.Add(245, "Clutch Pedal Switch #1");
        sidMapping.Add(246, "Brake Pedal Switch #1");
        sidMapping.Add(247, "Brake Pedal Switch #2");
        sidMapping.Add(248, "Proprietary Data Link");
        sidMapping.Add(249, "SAE J1922 Data Link");
        sidMapping.Add(250, "SAE J1708 (J1587) Data Link");
        sidMapping.Add(251, "Power Supply");
        sidMapping.Add(252, "Calibration Module");
        sidMapping.Add(253, "Calibration Memory");
        sidMapping.Add(254, "Controller #1");

        // Engine SIDs (MID = 128, 175, 183, 184, 185, 186)

        sidMapping = new Hashtable();

        sidMappings[128] = sidMapping;
        sidMappings[175] = sidMapping;
        sidMappings[183] = sidMapping;
        sidMappings[184] = sidMapping;
        sidMappings[185] = sidMapping;
        sidMappings[186] = sidMapping;

        sidMapping.Add(1, "Injector Cylinder #1");
        sidMapping.Add(2, "Injector Cylinder #2");
        sidMapping.Add(3, "Injector Cylinder #3");
        sidMapping.Add(4, "Injector Cylinder #4");
        sidMapping.Add(5, "Injector Cylinder #5");
        sidMapping.Add(6, "Injector Cylinder #6");
        sidMapping.Add(7, "Injector Cylinder #7");
        sidMapping.Add(8, "Injector Cylinder #8");
        sidMapping.Add(9, "Injector Cylinder #9");
        sidMapping.Add(10, "Injector Cylinder #10");
        sidMapping.Add(11, "Injector Cylinder #11");
        sidMapping.Add(12, "Injector Cylinder #12");
        sidMapping.Add(13, "Injector Cylinder #13");
        sidMapping.Add(14, "Injector Cylinder #14");
        sidMapping.Add(15, "Injector Cylinder #15");
        sidMapping.Add(16, "Injector Cylinder #16");
        sidMapping.Add(17, "Fuel Shutoff Valve");
        sidMapping.Add(18, "Fuel Control Valve");
        sidMapping.Add(19, "Throttle Bypass Valve");
        sidMapping.Add(20, "Timing Actuator");
        sidMapping.Add(21, "Engine Position Sensor");
        sidMapping.Add(22, "Timing Sensor");
        sidMapping.Add(23, "Rack Actuator");
        sidMapping.Add(24, "Rack Position Sensor");
        sidMapping.Add(25, "External Engine Protection Input");
        sidMapping.Add(26, "Auxiliary Output Device Driver #1");
        sidMapping.Add(27, "Variable Geometry Turbocharger Actuator #1");
        sidMapping.Add(28, "Variable Geometry Turbocharger Actuator #2");
        sidMapping.Add(29, "External Fuel Command Input");
        sidMapping.Add(30, "External Speed Command Input");
        sidMapping.Add(31, "Tachometer Signal Output");
        sidMapping.Add(32, "Turbocharger #1 Wastegate Drive");
        sidMapping.Add(33, "Fan Clutch Output Device Driver");
        sidMapping.Add(34, "Exhaust Back Pressure Sensor");
        sidMapping.Add(35, "Exhaust Back Pressure Regulator Solenoid");
        sidMapping.Add(36, "Glow Plug Lamp");
        sidMapping.Add(37, "Electronic Drive Unit Power Relay");
        sidMapping.Add(38, "Glow Plug Relay");
        sidMapping.Add(39, "Engine Starter Motor Relay");
        sidMapping.Add(40, "Auxiliary Output Device Driver #2");
        sidMapping.Add(41, "ECM 8 Volts DC Supply");
        sidMapping.Add(42, "Injection Control Pressure Regulator");
        sidMapping.Add(43, "Autoshift High Gear Actuator");
        sidMapping.Add(44, "Autoshift Low Gear Actuator");
        sidMapping.Add(45, "Autoshift Neutral Actuator");
        sidMapping.Add(46, "Autoshift Common Low Side (Return)");
        sidMapping.Add(47, "Injector Cylinder #17");
        sidMapping.Add(48, "Injector Cylinder #18");
        sidMapping.Add(49, "Injector Cylinder #19");
        sidMapping.Add(50, "Injector Cylinder #20");
        sidMapping.Add(51, "Auxiliary Output Device Driver #3");
        sidMapping.Add(52, "Auxiliary Output Device Driver #4");
        sidMapping.Add(53, "Auxiliary Output Device Driver #5");
        sidMapping.Add(54, "Auxiliary Output Device Driver #6");
        sidMapping.Add(55, "Auxiliary Output Device Driver #7");
        sidMapping.Add(56, "Auxiliary Output Device Driver #8");
        sidMapping.Add(57, "Auxiliary PWM Driver #1");
        sidMapping.Add(58, "Auxiliary PWM Driver #2");
        sidMapping.Add(59, "Auxiliary PWM Driver #3");
        sidMapping.Add(60, "Auxiliary PWM Driver #4");
        sidMapping.Add(61, "Variable Swirl System Valve");
        sidMapping.Add(62, "Prestroke Sensor");
        sidMapping.Add(63, "Prestroke Actuator");
        sidMapping.Add(64, "Engine Speed Sensor #2");
        sidMapping.Add(65, "Heated Oxygen Sensor");
        sidMapping.Add(66, "Ignition Control Mode Signal");
        sidMapping.Add(67, "Ignition Control Timing Signal");
        sidMapping.Add(68, "Secondary Turbo Inlet Pressure");
        sidMapping.Add(69, "After Cooler-Oil Cooler Coolant Temperature");
        sidMapping.Add(70, "Inlet Air Heater Driver #1");
        sidMapping.Add(71, "Inlet Air Heater Driver #2");
        sidMapping.Add(72, "Injector Cylinder #21");
        sidMapping.Add(73, "Injector Cylinder #22");
        sidMapping.Add(74, "Injector Cylinder #23");
        sidMapping.Add(75, "Injector Cylinder #24");
        sidMapping.Add(76, "Knock Sensor");
        sidMapping.Add(77, "Gas Metering Valve");
        sidMapping.Add(78, "Fuel Supply Pump Actuator");
        sidMapping.Add(79, "Engine (Compression) Brake Output #1");
        sidMapping.Add(80, "Engine (Compression) Brake Output #2");
        sidMapping.Add(81, "Engine (Exhaust) Brake Output");
        sidMapping.Add(82, "Engine (Compression) Brake Output #3");
        sidMapping.Add(83, "Fuel Control Valve #2");
        sidMapping.Add(84, "Timing Actuator #2");
        sidMapping.Add(85, "Engine Oil Burn Valve");
        sidMapping.Add(86, "Engine Oil Replacement Valve");
        sidMapping.Add(87, "Idle Shutdown Vehicle Accessories Relay Driver");
        sidMapping.Add(88, "Turbocharger #2 Wastegate Drive");
        sidMapping.Add(89, "Air Compressor Actuator Circuit");
        sidMapping.Add(90, "Engine Cylinder #1 Knock Sensor");
        sidMapping.Add(91, "Engine Cylinder #2 Knock Sensor");
        sidMapping.Add(92, "Engine Cylinder #3 Knock Sensor");
        sidMapping.Add(93, "Engine Cylinder #4 Knock Sensor");
        sidMapping.Add(94, "Engine Cylinder #5 Knock Sensor");
        sidMapping.Add(95, "Engine Cylinder #6 Knock Sensor");
        sidMapping.Add(96, "Engine Cylinder #7 Knock Sensor");
        sidMapping.Add(97, "Engine Cylinder #8 Knock Sensor");
        sidMapping.Add(98, "Engine Cylinder #9 Knock Sensor");
        sidMapping.Add(99, "Engine Cylinder #10 Knock Sensor");
        sidMapping.Add(100, "Engine Cylinder #11 Knock Sensor");
        sidMapping.Add(101, "Engine Cylinder #12 Knock Sensor");
        sidMapping.Add(102, "Engine Cylinder #13 Knock Sensor");
        sidMapping.Add(103, "Engine Cylinder #14 Knock Sensor");
        sidMapping.Add(104, "Engine Cylinder #15 Knock Sensor");
        sidMapping.Add(105, "Engine Cylinder #16 Knock Sensor");
        sidMapping.Add(106, "Engine Cylinder #17 Knock Sensor");
        sidMapping.Add(107, "Engine Cylinder #18 Knock Sensor");
        sidMapping.Add(108, "Engine Cylinder #19 Knock Sensor");
        sidMapping.Add(109, "Engine Cylinder #20 Knock Sensor");
        sidMapping.Add(110, "Engine Cylinder #21 Knock Sensor");
        sidMapping.Add(111, "Engine Cylinder #22 Knock Sensor");
        sidMapping.Add(112, "Engine Cylinder #23 Knock Sensor");
        sidMapping.Add(113, "Engine Cylinder #24 Knock Sensor");
        sidMapping.Add(114, "Multiple Unit Synchronization Switch");
        sidMapping.Add(115, "Engine Oil Change Interval");
        sidMapping.Add(116, "Engine was Shut Down Hot");
        sidMapping.Add(117, "Engine has been Shut Down from Data Link Information");
        sidMapping.Add(118, "Injector Needle Lift Sensor #1");
        sidMapping.Add(119, "Injector Needle Lift Sensor #2");
        sidMapping.Add(120, "Coolant System Thermostat");
        sidMapping.Add(121, "Engine Automatic Start Alarm");
        sidMapping.Add(122, "Engine Automatic Start Lamp");
        sidMapping.Add(123, "Engine Automatic Start Safety Interlock Circuit");
        sidMapping.Add(124, "Engine Automatic Start Failed (Engine)");
        sidMapping.Add(125, "Fuel Heater Driver Signal");
        sidMapping.Add(126, "Fuel Pump Pressurizing Assembly #1");
        sidMapping.Add(127, "Fuel Pump Pressurizing Assembly #2");
        sidMapping.Add(128, "Starter Solenoid Lockout Relay Driver Circuit");
        sidMapping.Add(129, "Cylinder #1 Exhaust Gas Port Temperature");
        sidMapping.Add(130, "Cylinder #2 Exhaust Gas Port Temperature");
        sidMapping.Add(131, "Cylinder #3 Exhaust Gas Port Temperature");
        sidMapping.Add(132, "Cylinder #4 Exhaust Gas Port Temperature");
        sidMapping.Add(133, "Cylinder #5 Exhaust Gas Port Temperature");
        sidMapping.Add(134, "Cylinder #6 Exhaust Gas Port Temperature");
        sidMapping.Add(135, "Cylinder #7 Exhaust Gas Port Temperature");
        sidMapping.Add(136, "Cylinder #8 Exhaust Gas Port Temperature");
        sidMapping.Add(137, "Cylinder #9 Exhaust Gas Port Temperature");
        sidMapping.Add(138, "Cylinder #10 Exhaust Gas Port Temperature");
        sidMapping.Add(139, "Cylinder #11 Exhaust Gas Port Temperature");
        sidMapping.Add(140, "Cylinder #12 Exhaust Gas Port Temperature");
        sidMapping.Add(141, "Cylinder #13 Exhaust Gas Port Temperature");
        sidMapping.Add(142, "Cylinder #14 Exhaust Gas Port Temperature");
        sidMapping.Add(143, "Cylinder #15 Exhaust Gas Port Temperature");
        sidMapping.Add(144, "Cylinder #16 Exhaust Gas Port Temperature");
        sidMapping.Add(145, "Adaptive Cruise Control Mode");
        sidMapping.Add(146, "Exhaust Gas Re-Circulation (EGR) Valve Mechanism");
        sidMapping.Add(147, "Variable Nozzle Turbocharger (VNT) Mechanism");
        sidMapping.Add(148, "Engine (Compression) Brake Output #4");
        sidMapping.Add(149, "Engine (Compression) Brake Output #5");
        sidMapping.Add(150, "Engine (Compression) Brake Output #6");

        // Transmission SIDs (MID = 130)

        sidMapping = new Hashtable();

        sidMappings[130] = sidMapping;

        sidMapping.Add(1, "C1 Solenoid Valve");
        sidMapping.Add(2, "C2 Solenoid Valve");
        sidMapping.Add(3, "C3 Solenoid Valve");
        sidMapping.Add(4, "C4 Solenoid Valve");
        sidMapping.Add(5, "C5 Solenoid Valve");
        sidMapping.Add(6, "C6 Solenoid Valve");
        sidMapping.Add(7, "Lockup Solenoid Valve");
        sidMapping.Add(8, "Forward Solenoid Valve");
        sidMapping.Add(9, "Low Signal Solenoid Valve");
        sidMapping.Add(10, "Retarder Enable Solenoid Valve");
        sidMapping.Add(11, "Retarder Modulation Solenoid Valve");
        sidMapping.Add(12, "Retarder Response Solenoid Valve");
        sidMapping.Add(13, "Differential Lock Solenoid Valve");
        sidMapping.Add(14, "Engine/Transmission Match");
        sidMapping.Add(15, "Retarder Modulation Request Sensor");
        sidMapping.Add(16, "Neutral Start Output");
        sidMapping.Add(17, "Turbine Speed Sensor");
        sidMapping.Add(18, "Primary Shift Selector");
        sidMapping.Add(19, "Secondary Shift Selector");
        sidMapping.Add(20, "Special Function Inputs");
        sidMapping.Add(21, "C1 Clutch Pressure Indicator");
        sidMapping.Add(22, "C2 Clutch Pressure Indicator");
        sidMapping.Add(23, "C3 Clutch Pressure Indicator");
        sidMapping.Add(24, "C4 Clutch Pressure Indicator");
        sidMapping.Add(25, "C5 Clutch Pressure Indicator");
        sidMapping.Add(26, "C6 Clutch Pressure Indicator");
        sidMapping.Add(27, "Lockup Clutch Pressure Indicator");
        sidMapping.Add(28, "Forward Range Pressure Indicator");
        sidMapping.Add(29, "Neutral Range Pressure Indicator");
        sidMapping.Add(30, "Reverse Range Pressure Indicator");
        sidMapping.Add(31, "Retarder Response System Pressure Indicator");
        sidMapping.Add(32, "Differential Lock Clutch Pressure Indicator");
        sidMapping.Add(33, "Multiple Pressure Indicators");
        sidMapping.Add(34, "Reverse Switch");
        sidMapping.Add(35, "Range High Actuator");
        sidMapping.Add(36, "Range Low Actuator");
        sidMapping.Add(37, "Splitter Direct Actuator");
        sidMapping.Add(38, "Splitter Indirect Actuator");
        sidMapping.Add(39, "Shift Finger Rail Actuator 1");
        sidMapping.Add(40, "Shift Finger Gear Actuator 1");
        sidMapping.Add(41, "Upshift Request Switch");
        sidMapping.Add(42, "Downshift Request Switch");
        sidMapping.Add(43, "Torque Converter Interrupt Actuator");
        sidMapping.Add(44, "Torque Converter Lockup Actuator");
        sidMapping.Add(45, "Range High Indicator");
        sidMapping.Add(46, "Range Low Indicator");
        sidMapping.Add(47, "Shift Finger Neutral Indicator");
        sidMapping.Add(48, "Shift Finger Engagement Indicator");
        sidMapping.Add(49, "Shift Finger Center Rail Indicator");
        sidMapping.Add(50, "Shift Finger Rail Actuator 2");
        sidMapping.Add(51, "Shift Finger Gear Actuator 2");
        sidMapping.Add(52, "Hydraulic System");
        sidMapping.Add(53, "Defuel Actuator");
        sidMapping.Add(54, "Inertia Brake Actuator");
        sidMapping.Add(55, "Clutch Actuator");
        sidMapping.Add(56, "Auxiliary Range Mechanical System");
        sidMapping.Add(57, "Shift Console Data Link");
        sidMapping.Add(58, "Main Box Shift Engagement System");
        sidMapping.Add(59, "Main Box Rail Selection System");
        sidMapping.Add(60, "Main Box Shift Neutralization System");
        sidMapping.Add(61, "Auxiliary Splitter Mechanical System");
        sidMapping.Add(62, "Transmission Controller Power Relay");
        sidMapping.Add(63, "Output Shaft Speed Sensor");
        sidMapping.Add(64, "Throttle Position Device");

        // Brake SIDs (MID = 136, 137, 138, 139, 246, 247)

        sidMapping = new Hashtable();

        sidMappings[136] = sidMapping;
        sidMappings[137] = sidMapping;
        sidMappings[138] = sidMapping;
        sidMappings[139] = sidMapping;
        sidMappings[246] = sidMapping;
        sidMappings[247] = sidMapping;

        sidMapping.Add(1, "Wheel Sensor ABS Axle 1 Left");
        sidMapping.Add(2, "ABS Axle 1 Right");
        sidMapping.Add(3, "ABS Axle 2 Left");
        sidMapping.Add(4, "ABS Axle 2 Right");
        sidMapping.Add(5, "ABS Axle 3 Left");
        sidMapping.Add(6, "ABS Axle 3 Right");
        sidMapping.Add(7, "Pressure Modulation Valve ABS Axle 1 Left");
        sidMapping.Add(8, "ABS Axle 1 Right");
        sidMapping.Add(9, "ABS Axle 2 Left");
        sidMapping.Add(10, "ABS Axle 2 Right");
        sidMapping.Add(11, "ABS Axle 3 Left");
        sidMapping.Add(12, "ABS Axle 3 Right");
        sidMapping.Add(13, "Retarder Control Relay");
        sidMapping.Add(14, "Relay Diagonal 1");
        sidMapping.Add(15, "Relay Diagonal 2");
        sidMapping.Add(16, "Mode Switch ABS");
        sidMapping.Add(17, "Mode Switch ASR");
        sidMapping.Add(18, "DIF 1—ASR Valve");
        sidMapping.Add(19, "DIF 2—ASR Valve");
        sidMapping.Add(20, "Pneumatic Engine Control");
        sidMapping.Add(21, "Electronic Engine Control (Servomotor)");
        sidMapping.Add(22, "Speed Signal Input");
        sidMapping.Add(23, "Tractor ABS Warning Light Bulb");
        sidMapping.Add(24, "ASR Light Bulb");
        sidMapping.Add(25, "Wheel Sensor, ABS Axle 1 Average");
        sidMapping.Add(26, "Wheel Sensor, ABS Axle 2 Average");
        sidMapping.Add(27, "Wheel Sensor, ABS Axle 3 Average");
        sidMapping.Add(28, "Pressure Modulator, Drive Axle Relay Valve");
        sidMapping.Add(29, "Pressure Transducer, Drive Axle Relay Valve");
        sidMapping.Add(30, "Master Control Relay");
        sidMapping.Add(31, "Trailer Brake Slack Out of Adjustment Forward Axle Left");
        sidMapping.Add(32, "Forward axle Right");
        sidMapping.Add(33, "Rear Axle Left");
        sidMapping.Add(34, "Rear Axle Right");
        sidMapping.Add(35, "Tractor Brake Slack Out of Adjustment Axle 1 Left");
        sidMapping.Add(36, "Axle 1 Right");
        sidMapping.Add(37, "Axle 2 Left");
        sidMapping.Add(38, "Axle 2 Right");
        sidMapping.Add(39, "Axle 3 Left");
        sidMapping.Add(40, "Axle 3 Right");
        sidMapping.Add(41, "Ride Height Relay");
        sidMapping.Add(42, "Hold Modulator Valve Solenoid Axle 1 Left");
        sidMapping.Add(43, "Axle 1 Right");
        sidMapping.Add(44, "Axle 2 Left");
        sidMapping.Add(45, "Axle 2 Right");
        sidMapping.Add(46, "Axle 3 Left");
        sidMapping.Add(47, "Axle 3 Right");
        sidMapping.Add(48, "Dump Modulator Valve Solenoid Axle 1 Left");
        sidMapping.Add(49, "Axle 1 Right");
        sidMapping.Add(50, "Axle 2 Left");
        sidMapping.Add(51, "Axle 2 Right");
        sidMapping.Add(52, "Axle 3 Left");
        sidMapping.Add(53, "Axle 3 Right");
        sidMapping.Add(54, "Hydraulic Pump Motor");
        sidMapping.Add(55, "Brake Light Switch 1");
        sidMapping.Add(56, "Brake Light Switch 2");
        sidMapping.Add(57, "Electronic Pressure Control, Axle 1");
        sidMapping.Add(58, "Pneumatic Back-up Pressure Control, Axle 1");
        sidMapping.Add(59, "Brake Pressure Sensing, Axle 1");
        sidMapping.Add(60, "Electronic Pressure Control, Axle 2");
        sidMapping.Add(61, "Pneumatic Back-up Pressure Control, Axle 2");
        sidMapping.Add(62, "Brake Pressure Sensing, Axle 2");
        sidMapping.Add(63, "Electronic Pressure Control, Axle 3");
        sidMapping.Add(64, "Pneumatic Back-up Pressure Control, Axle 3");
        sidMapping.Add(65, "Brake Pressure Sensing, Axle 3");
        sidMapping.Add(66, "Electronic Pressure Control, Trailer Control");
        sidMapping.Add(67, "Pneumatic Back-up Pressure Control, Trailer Control");
        sidMapping.Add(68, "Brake Pressure Sensing, Trailer Control");
        sidMapping.Add(69, "Axle Load Sensor");
        sidMapping.Add(70, "Lining Wear Sensor, Axle 1 Left");
        sidMapping.Add(71, "Lining Wear Sensor, Axle 1 Right");
        sidMapping.Add(72, "Lining Wear Sensor, Axle 2 Left");
        sidMapping.Add(73, "Lining Wear Sensor, Axle 2 Right");
        sidMapping.Add(74, "Lining Wear Sensor, Axle 3 Left");
        sidMapping.Add(75, "Lining Wear Sensor, Axle 3 Right");
        sidMapping.Add(76, "Brake Signal Transmitter");
        sidMapping.Add(77, "Brake Signal Sensor 1");
        sidMapping.Add(78, "Brake Signal Sensor 2");
        sidMapping.Add(79, "Tire Dimension Supervision");
        sidMapping.Add(80, "Vehicle Deceleration Control");
        sidMapping.Add(81, "Trailer ABS Warning Light Bulb");
        sidMapping.Add(82, "Brake Torque Output Axle 1 Left");
        sidMapping.Add(83, "Brake Torque Output Axle 1 Right");
        sidMapping.Add(84, "Brake Torque Output Axle 2 Left");
        sidMapping.Add(85, "Brake Torque Output Axle 2 Right");
        sidMapping.Add(86, "Brake Torque Output Axle 3 Left");
        sidMapping.Add(87, "Brake Torque Output Axle 3 Right");
        sidMapping.Add(88, "Vehicle Dynamic Stability Control System (VDC)");
        sidMapping.Add(89, "Steering Angle Sensor");
        sidMapping.Add(90, "Voltage Supply for Stability Control System");
        sidMapping.Add(91, "Brake Lining Display");
        sidMapping.Add(92, "Pressure Limitation Valve");
        sidMapping.Add(93, "Auxiliary Valve");
        sidMapping.Add(94, "Hill holder System");
        sidMapping.Add(95, "Voltage Supply, Lining Wear Sensors, Axle 1");
        sidMapping.Add(96, "Voltage Supply, Lining Wear Sensors, Axle 2");
        sidMapping.Add(97, "Voltage Supply, Lining Wear Sensors, Axle 3");

        // Instrument Panel SIDs (MID = 140,234)

        sidMapping = new Hashtable();

        sidMappings[140] = sidMapping;
        sidMappings[234] = sidMapping;

        sidMapping.Add(1, "Left Fuel Level Sensor");
        sidMapping.Add(2, "Right Fuel Level Sensor");
        sidMapping.Add(3, "Fuel Feed Rate Sensor");
        sidMapping.Add(4, "Fuel Return Rate Sensor");
        sidMapping.Add(5, "Tachometer Gauge Coil");
        sidMapping.Add(6, "Speedometer Gauge Coil");
        sidMapping.Add(7, "Turbocharger Air Pressure Gauge Coil");
        sidMapping.Add(8, "Fuel Pressure Gauge Coil");
        sidMapping.Add(9, "Fuel Level Gauge Coil");
        sidMapping.Add(10, "Second Fuel Level Gauge Coil");
        sidMapping.Add(11, "Engine Oil Pressure Gauge Coil");
        sidMapping.Add(12, "Engine Oil Temperature Gauge Coil");
        sidMapping.Add(13, "Engine Coolant Temperature Gauge Coil");
        sidMapping.Add(14, "Pyrometer Gauge Coil");
        sidMapping.Add(16, "Transmission Oil Pressure Gauge Coil");
        sidMapping.Add(15, "Transmission Oil Temperature Gauge Coil");
        sidMapping.Add(17, "Forward Rear Axle Temperature Gauge Coil");
        sidMapping.Add(18, "Rear Rear Axle Temperature Gauge Coil");
        sidMapping.Add(19, "Voltmeter Gauge Coil");
        sidMapping.Add(20, "Primary Air Pressure Gauge Coil");
        sidMapping.Add(21, "Secondary Air Pressure Gauge Coil");
        sidMapping.Add(22, "Ammeter Gauge Coil");
        sidMapping.Add(23, "Air Application Gauge Coil");
        sidMapping.Add(24, "Air Restriction Gauge Coil");

        // Vehicle Management System SIDs (MID = 142)

        sidMapping = new Hashtable();

        sidMappings[142] = sidMapping;

        sidMapping.Add(1, "Timing Sensor");
        sidMapping.Add(2, "Timing Actuator");
        sidMapping.Add(3, "Fuel Rack Position Sensor");
        sidMapping.Add(4, "Fuel Rack Actuator");
        sidMapping.Add(5, "Oil Level Indicator Output");
        sidMapping.Add(6, "Tachometer Drive Output");
        sidMapping.Add(7, "Speedometer Drive Output");
        sidMapping.Add(8, "PWM Input (ABS/ASR)");
        sidMapping.Add(9, "PWM Output");
        sidMapping.Add(10, "Auxiliary Output #1");
        sidMapping.Add(11, "Auxiliary Output #2");
        sidMapping.Add(12, "Auxiliary Output #3");

        // Fuel System SIDs (MID = 143)

        sidMapping = new Hashtable();

        sidMappings[143] = sidMapping;

        sidMapping.Add(1, "Injector Cylinder #1");
        sidMapping.Add(2, "Injector Cylinder #2");
        sidMapping.Add(3, "Injector Cylinder #3");
        sidMapping.Add(4, "Injector Cylinder #4");
        sidMapping.Add(5, "Injector Cylinder #5");
        sidMapping.Add(6, "Injector Cylinder #6");
        sidMapping.Add(7, "Injector Cylinder #7");
        sidMapping.Add(8, "Injector Cylinder #8");
        sidMapping.Add(9, "Injector Cylinder #9");
        sidMapping.Add(10, "Injector Cylinder #10");
        sidMapping.Add(11, "Injector Cylinder #11");
        sidMapping.Add(12, "Injector Cylinder #12");
        sidMapping.Add(13, "Injector Cylinder #13");
        sidMapping.Add(14, "Injector Cylinder #14");
        sidMapping.Add(15, "Injector Cylinder #15");
        sidMapping.Add(16, "Injector Cylinder #16");
        sidMapping.Add(17, "Fuel Shutoff Valve");
        sidMapping.Add(18, "Fuel Control Valve");
        sidMapping.Add(19, "Throttle Bypass Valve");
        sidMapping.Add(20, "Timing Actuator");
        sidMapping.Add(21, "Engine Position Sensor");
        sidMapping.Add(22, "Timing Sensor");
        sidMapping.Add(23, "Rack Actuator");
        sidMapping.Add(24, "Rack Position Sensor");
        sidMapping.Add(25, "External Engine Protection Input");
        sidMapping.Add(26, "Auxiliary Output Device Driver");
        sidMapping.Add(27, "Cooling Fan Drive Output");
        sidMapping.Add(28, "Engine (Compression) Brake Output #1");
        sidMapping.Add(29, "Engine (Compression) Brake Output #2");
        sidMapping.Add(30, "Engine (Exhaust) Brake Output");
        sidMapping.Add(31, "Pressure Control Valve #1");
        sidMapping.Add(32, "Pressure Control Valve #2");

        // Cab Climate Control SIDs (MID = 146, 200)

        sidMapping = new Hashtable();

        sidMappings[146] = sidMapping;
        sidMappings[200] = sidMapping;

        sidMapping.Add(1, "HVAC Unit Discharge Temperature Sensor");
        sidMapping.Add(2, "Evaporator Temperature Sensor");
        sidMapping.Add(3, "Solar Load Sensor #1");
        sidMapping.Add(4, "Solar Load Sensor #2");
        sidMapping.Add(5, "Fresh/Recirculation Air Intake Door Actuator");
        sidMapping.Add(6, "Mode Door #1 Actuator");
        sidMapping.Add(7, "Mode Door #2 Actuator");
        sidMapping.Add(8, "Mode Door #3 Actuator");
        sidMapping.Add(9, "Blend Door Actuator");
        sidMapping.Add(10, "Blower Motor");
        sidMapping.Add(11, "A/C Clutch Relay");
        sidMapping.Add(12, "Water Valve");
        sidMapping.Add(13, "Heater Exchanger Temperature Sensor");
        sidMapping.Add(14, "In Cabin Temperature Sensor Blower");
        sidMapping.Add(15, "Blower Clutch");
        sidMapping.Add(16, "Stepper Motor Phase 1");
        sidMapping.Add(17, "Stepper Motor Phase 2");
        sidMapping.Add(18, "Stepper Motor Phase 3");
        sidMapping.Add(19, "Stepper Motor Phase 4");
        sidMapping.Add(20, "Refrigerant Evaporator Inlet Temperature Sensor");
        sidMapping.Add(21, "Refrigerant Evaporator Outlet Temperature Sensor");
        sidMapping.Add(22, "Refrigerant Evaporator Inlet Pressure Sensor");
        sidMapping.Add(23, "Refrigerant Evaporator Outlet Pressure Sensor");
        sidMapping.Add(24, "Refrigerant Compressor Inlet Temperature Sensor");
        sidMapping.Add(25, "Refrigerant Compressor Outlet Temperature Sensor");
        sidMapping.Add(26, "Refrigerant Compressor Inlet Pressure Sensor");
        sidMapping.Add(27, "Refrigerant Compressor Outlet Pressure Sensor");
        sidMapping.Add(28, "Refrigerant Condenser Outlet Temperature Sensor");
        sidMapping.Add(29, "Refrigerant Condenser Outlet Pressure Sensor");

        // Suspension SIDs (MID = 150, 151, 152, 153)

        sidMapping = new Hashtable();

        sidMappings[150] = sidMapping;
        sidMappings[151] = sidMapping;
        sidMappings[152] = sidMapping;
        sidMappings[153] = sidMapping;

        sidMapping.Add(1, "Solenoid Valve Axle 1 Right");
        sidMapping.Add(2, "Axle 1 Left");
        sidMapping.Add(3, "Axle 2 Right");
        sidMapping.Add(4, "Axle 2 Left");
        sidMapping.Add(5, "Axle 3 Right");
        sidMapping.Add(6, "Axle 3 Left");
        sidMapping.Add(7, "Central (Lowering/Lifting Control)");
        sidMapping.Add(8, "Solenoid Valve for Lifting the Lifting/Trailing Axle");
        sidMapping.Add(9, "Solenoid Valve for Lowering the Lifting/Trailing Axle");
        sidMapping.Add(10, "Solenoid Valve for Control of the Lift Bellow");
        sidMapping.Add(11, "Solenoid Valve for Starting Lock");
        sidMapping.Add(12, "Solenoid Valve for Door Release");
        sidMapping.Add(13, "Solenoid Valve for Mainflow Throttle");
        sidMapping.Add(14, "Solenoid Valve for Transverse Lock/Throttle");
        sidMapping.Add(15, "Solenoid Valve for Automatic Load-Dependent Brake-Power Balance");
        sidMapping.Add(16, "Height Sensor Axle 1 Right");
        sidMapping.Add(17, "Axle 1 Left");
        sidMapping.Add(18, "Axle 2 Right");
        sidMapping.Add(19, "Axle 2 Left");
        sidMapping.Add(20, "Axle 3 Right");
        sidMapping.Add(21, "Axle 3 Left");
        sidMapping.Add(22, "Pressure Sensor Axle 1 Right");
        sidMapping.Add(23, "Axle 1 Left");
        sidMapping.Add(24, "Axle 2 Right");
        sidMapping.Add(25, "Axle 2 Left");
        sidMapping.Add(26, "Axle 3 Right");
        sidMapping.Add(27, "Axle 3 Left");
        sidMapping.Add(28, "Lift Bellow");
        sidMapping.Add(29, "Sidewalk Detector Sensor");
        sidMapping.Add(30, "Switch for Maximum Permanent Permissible Pressure");
        sidMapping.Add(31, "Switch for Maximum Temporary Permissible Pressure");
        sidMapping.Add(32, "Speed Signal Input");
        sidMapping.Add(33, "Remote Control Unit #1");
        sidMapping.Add(34, "Central Valve Relay");
        sidMapping.Add(35, "Auxiliary Tank Control");
        sidMapping.Add(36, "Exterior Kneel (warning lamp & audible alarm)");
        sidMapping.Add(37, "Wheel Chair Lift Inhibit");
        sidMapping.Add(38, "Checksum ECU Specific Data");
        sidMapping.Add(39, "Checksum Parameter Data");
        sidMapping.Add(40, "Checksum Calibration Data Level Sensors");
        sidMapping.Add(41, "Checksum Calibration Data Pressure Sensors");
        sidMapping.Add(42, "Checksum Maximum Axle Load Data");
        sidMapping.Add(43, "Central 3/2 Solenoid Valve Axle 3");
        sidMapping.Add(44, "Central 3/2 Solenoid Valve Front Axle");
        sidMapping.Add(45, "Pressure Sensor Brake Pressure");
        sidMapping.Add(46, "Power Supply for Pressure Sensors");
        sidMapping.Add(47, "Power Supply for Remote Controls");
        sidMapping.Add(48, "Remote Control #1 Data Line");
        sidMapping.Add(49, "Remote Control #1 Clock Line");
        sidMapping.Add(50, "Remote Control #2 Data Line");
        sidMapping.Add(51, "Remote Control #2 Clock Line");
        sidMapping.Add(52, "Remote Control Unit #2");
        sidMapping.Add(53, "Power Supply for Solenoid Valves");
        sidMapping.Add(54, "Proportional Valve Front Axle Left");
        sidMapping.Add(55, "Proportional Valve Front Axle Right");
        sidMapping.Add(56, "Proportional Valve Drive Axle Left");
        sidMapping.Add(57, "Proportional Valve Drive Axle Right");
        sidMapping.Add(58, "Proportional Valve Axle 3 Left");
        sidMapping.Add(59, "Proportional Valve Axle 3 Right");

        // Vehicle Navigation SIDs (MID = 162, 191)

        sidMapping = new Hashtable();

        sidMappings[162] = sidMapping;
        sidMappings[191] = sidMapping;

        sidMapping.Add(1, "Dead Reckoning Unit");
        sidMapping.Add(2, "Loran Receiver");
        sidMapping.Add(3, "Global Positioning System (GPS)");
        sidMapping.Add(4, "Integrated Navigation Unit");

        // Vehicle Security SIDs (MID = 163)

        sidMapping = new Hashtable();

        sidMappings[163] = sidMapping;

        sidMapping.Add(1, "Transceiver Antenna");
        sidMapping.Add(2, "Security Transponder");

        // Tire SIDs (MID = 166, 167, 168, 169)

        sidMapping = new Hashtable();

        sidMappings[166] = sidMapping;
        sidMappings[167] = sidMapping;
        sidMappings[168] = sidMapping;
        sidMappings[169] = sidMapping;

        sidMapping.Add(1, "Operator Control Panel (OCP)");
        sidMapping.Add(2, "Pneumatic Control Unit (PCU)");
        sidMapping.Add(3, "PCU Steer Solenoid");
        sidMapping.Add(4, "PCU Drive Solenoid");
        sidMapping.Add(5, "PCU Solenoid Trailer #1, Tag #1, or Push #1");
        sidMapping.Add(6, "PCU Supply Solenoid");
        sidMapping.Add(7, "PCU Control Solenoid");
        sidMapping.Add(8, "PCU Deflate Solenoid");
        sidMapping.Add(9, "Pneumatic—Steer Channel");
        sidMapping.Add(10, "Pneumatic—Drive Channel");
        sidMapping.Add(11, "Pneumatic—Trailer #1, Tag #1, or Push #1 Channel");
        sidMapping.Add(12, "Drive Axle Manifold Deflation Solenoid");
        sidMapping.Add(13, "Steer Axle Manifold Deflation Solenoid");
        sidMapping.Add(14, "PCU Solenoid Trailer #2, Tag #2, or Push #2");
        sidMapping.Add(15, "Brake Priority Pressure Switch");
        sidMapping.Add(16, "Pneumatic-Trailer #2, Tag #2, or Push #2 Channel");
        sidMapping.Add(17, "Wiring Harness");
        sidMapping.Add(18, "Tire Pressure Sensor - # 1");
        sidMapping.Add(19, "Tire Pressure Sensor - # 2");
        sidMapping.Add(20, "Tire Pressure Sensor - # 3");
        sidMapping.Add(21, "Tire Pressure Sensor - # 4");
        sidMapping.Add(22, "Tire Pressure Sensor - # 5");
        sidMapping.Add(23, "Tire Pressure Sensor - # 6");
        sidMapping.Add(24, "Tire Pressure Sensor - # 7");
        sidMapping.Add(25, "Tire Pressure Sensor - # 8");
        sidMapping.Add(26, "Tire Pressure Sensor - # 9");
        sidMapping.Add(27, "Tire Pressure Sensor - # 10");
        sidMapping.Add(28, "Tire Pressure Sensor - # 11");
        sidMapping.Add(29, "Tire Pressure Sensor - # 12");
        sidMapping.Add(30, "Tire Pressure Sensor - # 13");
        sidMapping.Add(31, "Tire Pressure Sensor - # 14");
        sidMapping.Add(32, "Tire Pressure Sensor - # 15");
        sidMapping.Add(33, "Tire Pressure Sensor - # 16");
        sidMapping.Add(34, "Tire Temperature Sensor - # 1");
        sidMapping.Add(35, "Tire Temperature Sensor - # 2");
        sidMapping.Add(36, "Tire Temperature Sensor - # 3");
        sidMapping.Add(37, "Tire Temperature Sensor - # 4");
        sidMapping.Add(38, "Tire Temperature Sensor - # 5");
        sidMapping.Add(39, "Tire Temperature Sensor - # 6");
        sidMapping.Add(40, "Tire Temperature Sensor - # 7");
        sidMapping.Add(41, "Tire Temperature Sensor - # 8");
        sidMapping.Add(42, "Tire Temperature Sensor - # 9");
        sidMapping.Add(43, "Tire Temperature Sensor - # 10");
        sidMapping.Add(44, "Tire Temperature Sensor - # 11");
        sidMapping.Add(45, "Tire Temperature Sensor - # 12");
        sidMapping.Add(46, "Tire Temperature Sensor - # 13");
        sidMapping.Add(47, "Tire Temperature Sensor - # 14");
        sidMapping.Add(48, "Tire Temperature Sensor - # 15");
        sidMapping.Add(49, "Tire Temperature Sensor - # 16");
        sidMapping.Add(50, "Tire Sensor Voltage - # 1");
        sidMapping.Add(51, "Tire Sensor Voltage - # 2");
        sidMapping.Add(52, "Tire Sensor Voltage - # 3");
        sidMapping.Add(53, "Tire Sensor Voltage - # 4");
        sidMapping.Add(54, "Tire Sensor Voltage - # 5");
        sidMapping.Add(55, "Tire Sensor Voltage - # 6");
        sidMapping.Add(56, "Tire Sensor Voltage - # 7");
        sidMapping.Add(57, "Tire Sensor Voltage - # 8");
        sidMapping.Add(58, "Tire Sensor Voltage - # 9");
        sidMapping.Add(59, "Tire Sensor Voltage - # 10");
        sidMapping.Add(60, "Tire Sensor Voltage - # 11");
        sidMapping.Add(61, "Tire Sensor Voltage - # 12");
        sidMapping.Add(62, "Tire Sensor Voltage - # 13");
        sidMapping.Add(63, "Tire Sensor Voltage - # 14");
        sidMapping.Add(64, "Tire Sensor Voltage - # 15");
        sidMapping.Add(65, "Tire Sensor Voltage - # 16");

        // Particulate Trap System SIDs (MID = 177)

        sidMapping = new Hashtable();

        sidMappings[177] = sidMapping;

        sidMapping.Add(1, "Heater Circuit #1");
        sidMapping.Add(2, "Heater Circuit #2");
        sidMapping.Add(3, "Heater Circuit #3");
        sidMapping.Add(4, "Heater Circuit #4");
        sidMapping.Add(5, "Heater Circuit #5");
        sidMapping.Add(6, "Heater Circuit #6");
        sidMapping.Add(7, "Heater Circuit #7");
        sidMapping.Add(8, "Heater Circuit #8");
        sidMapping.Add(9, "Heater Circuit #9");
        sidMapping.Add(10, "Heater Circuit #10");
        sidMapping.Add(11, "Heater Circuit #11");
        sidMapping.Add(12, "Heater Circuit #12");
        sidMapping.Add(13, "Heater Circuit #13");
        sidMapping.Add(14, "Heater Circuit #14");
        sidMapping.Add(15, "Heater Circuit #15");
        sidMapping.Add(16, "Heater Circuit #16");
        sidMapping.Add(17, "Heater Regeneration System");

        // Refrigerant Management Systems SIDs (MID = 190)

        sidMapping = new Hashtable();

        sidMappings[190] = sidMapping;

        sidMapping.Add(1, "Refrigerant Charge");
        sidMapping.Add(2, "Refrigerant Moisture Level");
        sidMapping.Add(3, "Non-condensable Gas in Refrigerant");
        sidMapping.Add(4, "Refrigerant Flow Control Solenoid");
        sidMapping.Add(5, "Low Side Refrigerant Pressure Switch");
        sidMapping.Add(6, "Compressor Clutch Circuit");
        sidMapping.Add(7, "Evaporator Thermostat Circuit");
        sidMapping.Add(8, "Refrigerant Flow");

        // Tractor/Trailer Bridge SIDs (MIDS = 217, 218)

        sidMapping = new Hashtable();

        sidMappings[217] = sidMapping;
        sidMappings[218] = sidMapping;

        sidMapping.Add(1, "Auxiliary input #1");
        sidMapping.Add(2, "Auxiliary input #2");
        sidMapping.Add(3, "Auxiliary input #3");
        sidMapping.Add(4, "Auxiliary input #4");
        sidMapping.Add(5, "Auxiliary input #5");
        sidMapping.Add(6, "Auxiliary input #6");
        sidMapping.Add(7, "Auxiliary input #7");
        sidMapping.Add(8, "Auxiliary input #8");
        sidMapping.Add(9, "Clearance, side marker, identification lamp circuit (Black)");
        sidMapping.Add(10, "Left turn lamp circuit (Yellow)");
        sidMapping.Add(11, "Stop lamp circuit (Red)");
        sidMapping.Add(12, "Right turn lamp circuit (Green)");
        sidMapping.Add(13, "Tail lamp/license plate lamp circuit (Brown)");
        sidMapping.Add(14, "Auxiliary lamp circuit (Blue)");
        sidMapping.Add(15, "Tractor mounted rear axle slider control unit");
        sidMapping.Add(16, "Trailer mounted rear axle slider control unit");

        // Collision Avoidance Radar SIDs (MIDS = 219)

        sidMapping = new Hashtable();

        sidMappings[219] = sidMapping;

        sidMapping.Add(1, "Forward Antenna");
        sidMapping.Add(2, "Antenna Electronics");
        sidMapping.Add(3, "Brake Input Monitor");
        sidMapping.Add(4, "Speaker Monitor");
        sidMapping.Add(5, "Steering Sensor Monitor");
        sidMapping.Add(6, "Speedometer Monitor");
        sidMapping.Add(7, "Right Turn Signal Monitor");
        sidMapping.Add(8, "Left Turn Signal Monitor");
        sidMapping.Add(9, "Control Display Unit");
        sidMapping.Add(10, "Right Side Sensor");
        sidMapping.Add(11, "Left Side Sensor");
        sidMapping.Add(12, "Rear Sensor");

        // Driveline Retarder SIDs (MID = 222)

        sidMapping = new Hashtable();

        sidMappings[222] = sidMapping;

        sidMapping.Add(1, "Retarder Enable Solenoid Valve");
        sidMapping.Add(2, "Retarder Modulation Solenoid Valve");
        sidMapping.Add(3, "Retarder Response Solenoid Valve");
        sidMapping.Add(4, "Retarder Modulation Request Sensor");
        sidMapping.Add(5, "Retarder Response System Pressure Indicator");

        // Vehicle Sensors to Data Converter SIDs (MID = 178)

        sidMapping = new Hashtable();

        sidMappings[178] = sidMapping;

        sidMapping.Add(1, "Battery Positive Input");
        sidMapping.Add(2, "Battery Negative Input");
        sidMapping.Add(3, "Current Shunt (-) Input");
        sidMapping.Add(4, "Current Shunt (+) Input");
        sidMapping.Add(5, "Starter Negative Input");
        sidMapping.Add(6, "Alternator Negative Input");
        sidMapping.Add(7, "Transducer +5V Excitation");
        sidMapping.Add(8, "Starter Positive Input");
        sidMapping.Add(9, "Starter Solenoid Input");
        sidMapping.Add(10, "Alternator Positive Input");
        sidMapping.Add(11, "Alternator Field Input");
        sidMapping.Add(12, "Fuel Solenoid Positive Input");
        sidMapping.Add(13, "User Probe Input");
        sidMapping.Add(14, "Fuel Supply Sender Input");
        sidMapping.Add(15, "Air Cleaner Delta P Sender Input");
        sidMapping.Add(16, "Fuel Filter Delta P Sender Input");
        sidMapping.Add(17, "Oil Filter Inlet Sender Input");
        sidMapping.Add(18, "Fuel Return Sender Input");
        sidMapping.Add(19, "Oil Filter Outlet Sender Input");
        sidMapping.Add(20, "Fuel Vacuum Sender Input");
        sidMapping.Add(21, "Battery Negative Input Circuit");
        sidMapping.Add(22, "Battery Positive Input Circuit");
        sidMapping.Add(23, "Starter Positive Input Circuit");
        sidMapping.Add(24, "Starter Negative Input Circuit");
        sidMapping.Add(25, "Starter Solenoid Input Circuit");
        sidMapping.Add(26, "Alternator Field Input Circuit");
        sidMapping.Add(27, "Alternator Positive Input Circuit");
        sidMapping.Add(28, "Alternator Negative Input Circuit");
        sidMapping.Add(29, "Current Sensor Discharge Circuit");
        sidMapping.Add(30, "Current Sensor Charge Circuit");

        // Safety Restraint System SIDs (MID = 232)

        sidMapping = new Hashtable();

        sidMappings[232] = sidMapping;

        sidMapping.Add(1, "Driver Air Bag Ignitor Loop");
        sidMapping.Add(2, "Passenger Air Bag Ignitor Loop");
        sidMapping.Add(3, "Left Belt Tensioner Ignitor Loop");
        sidMapping.Add(4, "Right Belt Tensioner Ignitor Loop");
        sidMapping.Add(5, "Safety Restraint System (SRS) Lamp—directly controlled by the ECU");
        sidMapping.Add(6, "Automotive Seat Occupancy Sensor (AOS)—Passenger Side");
        sidMapping.Add(7, "Side Collision Detector (SDC)—Left");
        sidMapping.Add(8, "Side Bag Ignitor Loop 1—Left");
        sidMapping.Add(9, "Side Bag Ignitor Loop 2—Left");
        sidMapping.Add(10, "Side Collision Detector—Right");
        sidMapping.Add(11, "Side Bag Ignitor Loop 1—Right");
        sidMapping.Add(12, "Side Bag Ignitor Loop 2—Right");
        sidMapping.Add(13, "Rollover Sensor");
        sidMapping.Add(14, "Driver Air Bag Stage 2 Igniter Loop");
        sidMapping.Add(15, "Passenger Air Bag Stage 2 Igniter Loop");

        // Forward Road Image Processor SIDs (MID = 248)

        sidMapping = new Hashtable();

        sidMappings[248] = sidMapping;

        sidMapping.Add(1, "Forward View Imager System");

        #endregion
      }

      #endregion
    }

    #endregion

    #region XmduSource

    public struct XmduSource
    {
      public string VIN;
      internal Int32 OdometerRaw;
      internal Int32 EngineHoursRaw;
      internal byte SpeedMphRaw;
      internal Int16 RpmRaw;
      internal Int16 AverageFuelEconomyMpgRaw;
      internal byte EngineOilPressureLbfIn2Raw;
      internal byte EngineCoolantTempFRaw;
      internal Int16 BatteryVoltageVRaw;

      // The following are only here to support PMV... You can use them, but I don't in the code

      public Double Odometer
      {
        get { return OdometerRaw * 0.1; }
        set { OdometerRaw = (Int32)(value / 0.1); }
      }

      public Double EngineHours
      {
        get { return EngineHoursRaw * 0.05; }
        set { EngineHoursRaw = (Int32)(value / 0.05); }
      }

      public Double SpeedMph
      {
        get { return SpeedMphRaw * 0.5; }
        set { SpeedMphRaw = (byte)(value / 0.5); }
      }

      public Double Rpm
      {
        get { return RpmRaw * 0.25; }
        set { RpmRaw = (Int16)(value / 0.25); }
      }

      public Double AverageFuelEconomyMpg
      {
        get { return AverageFuelEconomyMpgRaw / 256.0; }
        set { AverageFuelEconomyMpgRaw = (Int16)(value * 256); }
      }

      public Double EngineOilPressureLbfIn2
      {
        get { return EngineOilPressureLbfIn2Raw * 0.5; }
        set { EngineOilPressureLbfIn2Raw = (byte)(value / 0.5); }
      }

      public Double EngineCoolantTempF
      {
        get { return EngineCoolantTempFRaw; }
        set { EngineCoolantTempFRaw = (byte)(value); }
      }

      public Double BatteryVoltageV
      {
        get { return BatteryVoltageVRaw * 0.05; }
        set { BatteryVoltageVRaw = (Int16)(value / 0.05); }
      }
    }

    #endregion

    private byte SubTypeRaw;
    public JBusSource Source;
    public XmduSource XmduSources;
    private byte StatusRaw;
    public UInt32 DataValue;

    public int FaultTransientCount;

    [XmlArrayItem(typeof(FaultReportItem)), XmlArrayItem(typeof(AlertReportItem))]
    public ReportItem[] ReportItems;

    public byte[] ActiveMap;

    public class FaultReportItem : ReportItem
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        base.Serialize(action, raw, ref bitPosition);

        serializer(action, raw, ref bitPosition, 8, ref FaultModeIndicator);
      }

      public byte FaultModeIndicator;
    }

    public class AlertReportItem : ReportItem
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        base.Serialize(action, raw, ref bitPosition);

        serializer(action, raw, ref bitPosition, 16, ref DurationSeconds);
      }

      public int DurationSeconds;
    }

    public class ReportItem : NestedMessage
    {
      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
      {
        // ReportItem packets do not call the base serializer.  There is no 'packet ID' to
        // pick up since the type is implied by context.

        serializer(action, raw, ref bitPosition, 8, ref Source.MIDRaw);
        serializer(action, raw, ref bitPosition, 8, ref Source.PIDRaw);
        serializer(action, raw, ref bitPosition, 8, ref Source.ExtendedPIDRaw);
      }

      public JBusSource Source;
    }

    public QuerySubType SubType
    {
      get { return (QuerySubType)SubTypeRaw; }
      set { SubTypeRaw = (byte)value; }
    }

    public QueryStatus Status
    {
      get { return (QueryStatus)StatusRaw; }
      set { StatusRaw = (byte)value; }
    }

    #region IXmlEventFragment Members

    private void XmlReponse(XmlWriter xw)
    {
      xw.WriteStartElement("Response", PlatformEvents.Constants.RealtimeNamespace);
      if (Source.MID == (int)BusMid.Engine1 && Source.PID == (int)EnginePid.PowerTakeOffStatus)
      {
        xw.WriteAttributeString("Flags", String.Empty, ((PtoStatus)DataValue).ToString());
      }

      Source.GenerateXml(xw, false);

      xw.WriteString(XmlConvert.ToString(ScaledValue));
      xw.WriteEndElement();
    }

    public void XmlEvent(XmlWriter xw, int version)
    {
      if (SubType == QuerySubType.AdHocQueryResponse)
      {
        XmlReponse(xw);
      }
      else if (SubType == QuerySubType.FaultReport)
      {
        xw.WriteStartElement("Faults", PlatformEvents.Constants.RealtimeNamespace);
        xw.WriteAttributeString("TransientCount", String.Empty, FaultTransientCount.ToString());

        if (ReportItems != null)
        {
          foreach (FaultReportItem fri in ReportItems)
          {
            if (fri != null)
            {
              int relevantFmi = (fri.FaultModeIndicator & 0x0f);

              xw.WriteStartElement("Fault", PlatformEvents.Constants.RealtimeNamespace);
              xw.WriteAttributeString("FMI", String.Empty, XmlConvert.ToString(relevantFmi));

              fri.Source.GenerateXml(xw, (fri.FaultModeIndicator & (1 << 4)) != 0);

              xw.WriteString(fmiDescriptions[relevantFmi]);
              xw.WriteEndElement();
            }
          }
        }

        xw.WriteEndElement();
      }
      else if (SubType == QuerySubType.AlertReport)
      {
        xw.WriteStartElement("Alerts", PlatformEvents.Constants.RealtimeNamespace);

        if (ReportItems != null)
        {
          foreach (AlertReportItem ari in ReportItems)
          {
            if (ari != null)
            {
              xw.WriteStartElement("Alert", PlatformEvents.Constants.RealtimeNamespace);
              ari.Source.GenerateXml(xw, false);

              xw.WriteString(XmlConvert.ToString(ari.DurationSeconds));
              xw.WriteEndElement();
            }
          }
        }

        xw.WriteEndElement();
      }
      else if (SubType == QuerySubType.XmduBlock)
      {
        // This sucks....

        Source.MID = (int)BusMid.Engine1;

        if (XmduSources.VIN != null && XmduSources.VIN.Length > 0)
        {
          xw.WriteStartElement("Response", PlatformEvents.Constants.RealtimeNamespace);

          Source.PID = (int)EnginePid.VIN;

          Source.GenerateXml(xw, false);

          xw.WriteString(XmduSources.VIN);
          xw.WriteEndElement();
        }

        // Now for each numeric value, see if it is real or a placeholder. If real, stuff the Source and generate the response.

        if (XmduSources.OdometerRaw >= 0)
        {
          Source.PID = (int)EnginePid.Odometer;
          DataValue = (UInt32)XmduSources.OdometerRaw;
          XmlReponse(xw);
        }

        if (XmduSources.EngineHoursRaw >= 0)
        {
          Source.PID = (int)EnginePid.TotalEngineHours;
          DataValue = (UInt32)XmduSources.EngineHoursRaw;
          XmlReponse(xw);
        }

        if (XmduSources.SpeedMphRaw != 255)
        {
          Source.PID = (int)EnginePid.Speed;
          DataValue = (UInt32)XmduSources.SpeedMphRaw;
          XmlReponse(xw);
        }

        if (XmduSources.RpmRaw >= 0)
        {
          Source.PID = (int)EnginePid.RPM;
          DataValue = (UInt32)XmduSources.RpmRaw;
          XmlReponse(xw);
        }

        if (XmduSources.AverageFuelEconomyMpgRaw >= 0)
        {
          Source.PID = (int)EnginePid.AverageFuelEconomy;
          DataValue = (UInt32)XmduSources.AverageFuelEconomyMpgRaw;
          XmlReponse(xw);
        }

        if (XmduSources.EngineOilPressureLbfIn2Raw != 255)
        {
          Source.PID = (int)EnginePid.EngineOilPressure;
          DataValue = (UInt32)XmduSources.EngineOilPressureLbfIn2Raw;
          XmlReponse(xw);
        }

        if (XmduSources.EngineCoolantTempFRaw != 255)
        {
          Source.PID = (int)EnginePid.EngineCoolantTemperature;
          DataValue = (UInt32)XmduSources.EngineCoolantTempFRaw;
          XmlReponse(xw);
        }

        if (XmduSources.BatteryVoltageVRaw >= 0)
        {
          Source.PID = (int)EnginePid.BatteryPotential;
          DataValue = (UInt32)XmduSources.BatteryVoltageVRaw;
          XmlReponse(xw);
        }
      }
    }

    public string CsvValues
    {
      get
      {
        switch (SubType)
        {
          default:
            return String.Empty;

          case QuerySubType.AdHocQueryResponse:
            return String.Format("{0},{1},{2},{3},{4},{5}{6}",
               Source.MID, Source.MidText,
               Source.PID, Source.PidText,
               ScaledValue, Source.PidUnits,

               ((Source.MID == (int)BusMid.Engine1 && Source.PID == (int)EnginePid.PowerTakeOffStatus) ?
               "," + ((PtoStatus)DataValue).ToString() : String.Empty));

          case QuerySubType.AlertReport:
            {
              StringBuilder result = new StringBuilder();

              if (ReportItems != null)
              {
                foreach (AlertReportItem ari in ReportItems)
                {
                  if (ari != null)
                  {
                    if (result.Length > 0)
                    {
                      result.Append(",");
                    }

                    result.AppendFormat("{0},{1},{2},{3},{4}",
                       ari.Source.MID, ari.Source.MidText,
                       ari.Source.PID, ari.Source.PidText,
                       ari.DurationSeconds);
                  }
                }
              }

              return (result.Length == 0) ? String.Empty : result.ToString();
            }

          case QuerySubType.FaultReport:
            {
              StringBuilder result = new StringBuilder();

              if (ReportItems != null)
              {
                foreach (FaultReportItem fri in ReportItems)
                {
                  if (fri != null)
                  {
                    if (result.Length > 0)
                    {
                      result.Append(",");
                    }

                    string pidSidText = null;

                    if ((fri.FaultModeIndicator & (1 << 4)) != 0)
                    {
                      // This is really a SID

                      pidSidText = fri.Source.SidText;
                    }
                    else
                    {
                      // Otherwise, it is a PID

                      pidSidText = fri.Source.PidText;
                    }

                    result.AppendFormat("{0},{1},{2},{3},{4}",
                       fri.Source.MID, fri.Source.MidText,
                       fri.Source.PID, pidSidText,
                       fri.FaultModeIndicator & 0x0f);
                  }
                }
              }

              return (result.Length == 0) ? String.Empty : result.ToString();
            }
        }
      }
    }

    public override bool EventIsPublished
    {
      get
      {
        // We always publish alert and fault reports. Ad-hoc results are only published if they were
        // successful.

        return (Status == QueryStatus.QueryComplete && (SubType == QuerySubType.AdHocQueryResponse || SubType == QuerySubType.XmduBlock)) ||
           SubType == QuerySubType.AlertReport ||
           SubType == QuerySubType.FaultReport;
      }
    }

    #endregion
  }


    public class PortBasedUserDataMessage : TrackerUserDataMessage, IXmlEventFragment, ICsvValues
  {
    public static new readonly int kPacketID = 0x80;

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get { return UDEventID.Unparsed; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      serializer(action, raw, ref bitPosition, 16, ref PortNumber);
      serializer(action, raw, ref bitPosition, 1, ref SaveInDatabase);
      filler(ref bitPosition, 7);

        // Hack: The tracker occasionally reports a packet length longer than the remaining packet
        // size.  Since I now have the hydrationContext available with the final bit position of this
        // message, I use that to calculate the length and ignore the length reported by the device
        // (except to set the ReportedSizeOfPayloadDiscrepencyBytes value).

        uint reportedDataLength = 0;

        lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);
        if (action == SerializationAction.Hydrate)
        {
          bitPosition -= 16;
          serializer(action, raw, ref bitPosition, 16, ref reportedDataLength);
        }
        uint realDataLength = reportedDataLength;

        if (action == SerializationAction.Hydrate)
        {
          realDataLength = bytesLeftInMessage(bitPosition);

          ReportedSizeOfPayloadDiscrepencyBytes = (int)reportedDataLength - (int)realDataLength;

          if (ReportedSizeOfPayloadDiscrepencyBytes != 0)
          {
            hydrationErrors |= MessageHydrationErrors.EmbeddedMessageMalformed;
          }
        }
        else if (Data != null)
        {
          realDataLength = (uint)Data.Length;
        }

        if (PortNumber == 1001)
        {
          if(action == SerializationAction.Hydrate)
            chiron = new ChironDiagnostic();
          SerializeChironDiagnostic(action, raw, ref bitPosition);
        }
        else if (PortNumber == 1002)
        {
          if (action == SerializationAction.Hydrate)
            qosMetrics = new QOSMetrics();
          SerializeQOSMetricsDiagnostic(action, raw, ref bitPosition);
        }
        else if (PortNumber == 1022)
        {
          if (action == SerializationAction.Hydrate)
            skylineFirmwareStatusReporting = new SkylineFirmwareStatusReporting();
          SerializeSkylingFirmwareStatusReportingDiagnostic(action, raw, ref bitPosition);
        }
        else
        {
          serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref Data);
        }
      if(action != SerializationAction.Hydrate)
        configBlockLength.Backfill(bitPosition);
    }

    private void SerializeChironDiagnostic(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref chiron.Subtype);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateDiscreteInput0);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateDiscreteInput1);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateDiscreteInput2);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateDiscreteInput3);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateOutput1);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateOutput2);
      serializer(action, raw, ref bitPosition, 1, ref chiron.IOStateOutput3);
      serializer(action, raw, ref bitPosition, 1, ref chiron.Ignition);
      serializer(action, raw, ref bitPosition, 16, ref chiron.MessageCode);
      filler(ref bitPosition, 8);
      byte heading = 0;
      serializer(action, raw, ref bitPosition, 8, ref heading);
      chiron.Heading = heading * 2.8125;
      serializer(action, raw, ref bitPosition, 8, ref chiron.GPSSpeed);
      serializer(action, raw, ref bitPosition, 8, ref chiron.SpeedSensorSpeed);
      serializer(action, raw, ref bitPosition, 16, ref chiron.SpeedSensorCalibrationFactor);
      filler(ref bitPosition, 16);
      serializer(action, raw, ref bitPosition, 32, ref chiron.SpeedSensorClibrationValue);
      serializer(action, raw, ref bitPosition, 32, ref chiron.BootCount);
      serializer(action, raw, ref bitPosition, 16, ref chiron.RSSI);
      filler(ref bitPosition, 48);
      uint ipAddress = 0;
      serializer(action, raw, ref bitPosition, 32, ref ipAddress);
      chiron.SetIPAddress(ipAddress);
      serializer(action, raw, ref bitPosition, 32, ref chiron.OperatorAndNetworkID);
      if (chiron.Subtype == 7)
      {
        serializer(action, raw, ref bitPosition, 32, ref chiron.ConsecutiveCount);
        serializer(action, raw, ref bitPosition, 32, ref chiron.HorizontalSpeed);
        serializer(action, raw, ref bitPosition, 32, ref chiron.VerticalSpeed);
        serializer(action, raw, ref bitPosition, 32, ref chiron.OscillatorDrift);
        serializer(action, raw, ref bitPosition, 32, ref chiron.HorizontalAcceleration);
      }
      else if (chiron.Subtype == 3)
      {
        serializeFixedLengthString(action, raw, ref bitPosition, 15, ref chiron.CommStateText);
      }
    }

    private void SerializeQOSMetricsDiagnostic(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.type);
      if (qosMetrics.type == 0x00)
      {
        serializeTCPUDPMetrics(action, raw, ref bitPosition);
      }
      else if (qosMetrics.type == 0x01)
      {
        serializeNetworkMetrics(action, raw, ref bitPosition);
      }
      else if (qosMetrics.type == 0x02)
      {
        serializeGPSMetrics(action, raw, ref bitPosition);
      }
      else if (qosMetrics.type == 0x03)
      {
        //serialize ErrorLog message
        serializeNulTerminatedString(action, raw, ref bitPosition, ref qosMetrics.errorLogMsg);
      }
      else
      {
        serializeFixedLengthBytes(action, raw, ref bitPosition, bytesLeftInMessage(bitPosition), ref Data);
      }
    }

    private void serializeNetworkMetrics(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.flashFileVersion);
      if (qosMetrics.flashFileVersion == 4)
      {
        filler(ref bitPosition, 8);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.unexpectedModuleResets);
      }
      else
      {
        filler(ref bitPosition, 24);
      }
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.accumulatedRuntime);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.bootCount);
      if (qosMetrics.flashFileVersion != 1)
      {
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.sumOfDisconnectTimes);
        if (action == SerializationAction.Hydrate)
        {
          uint standardDev = 0;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref standardDev);
          qosMetrics.standardDeviation = standardDev;
        }
        else
        {
          uint standardDev = (uint)qosMetrics.standardDeviation;
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref standardDev);
        }
        
      }
      else
      {
        BigEndianSerializer(action, raw, ref bitPosition, 8, ref qosMetrics.standardDeviation);
      }
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.minDisconnectDuration);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.maxDisconnectDuration);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeNoCommsLessThanEqualToNegative100);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeNoCommsGreaterThanEqualToNegative100);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeNonRoaming);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeRoaming);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInUnkown);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInAttachedState);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInActivatedState);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInGetIPState);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInOpenSocketState);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeInNetEntryState);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeConnectedToServerOnUnknownNetwork);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeSearchingOnUnknownNetwork);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.pdpActivateAttempts);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.pdpContextFailures);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.attachAttempts);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.commsDiconnects);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.disconnectSamplesForStdDev);
      BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.simFailures);
      serializeFixedLengthString(action, raw, ref bitPosition, 32, ref qosMetrics.apnString);
      serializeFixedLengthString(action, raw, ref bitPosition, 26, ref qosMetrics.moduleVersionString);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.operatorID1);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeSearchingOnOperator1);
      if (qosMetrics.flashFileVersion == 4)
      {
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeConnectedOnOperator1);
      }
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.operatorID2);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeSearchingOnOperator2);
      if (qosMetrics.flashFileVersion == 4)
      {
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeConnectedOnOperator2);
      }
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.operatorID3);
      BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeSearchingOnOperator2);
      if (qosMetrics.flashFileVersion == 4)
      {
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.timeConnectedOnOperator3);
      }
      if (qosMetrics.flashFileVersion == 4 || qosMetrics.flashFileVersion == 3)
      {
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.connectRssiMinInHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.connectRssiMaxInHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.sumRssiInHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.rssiSamplesInHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.connectRssiMinOutsideHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.connectRssiMaxOutsideHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.sumRssiOutsideHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.rssiSamplesOutsideHomeSite);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.errorCode1);
        filler(ref bitPosition, 16);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.errorCode1Count);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.errorCode2);
        filler(ref bitPosition, 16);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.errorCode2Count);
        BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.errorCode3);
        filler(ref bitPosition, 16);
        BigEndianSerializer(action, raw, ref bitPosition, 4, ref qosMetrics.errorCode3Count);
        if (qosMetrics.flashFileVersion == 4)
        {
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref qosMetrics.networkScansRequested);
        }
      }
    }

    private void serializeTCPUDPMetrics(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.flashFileVersion);
      byte protocol = 0;
      serializer(action, raw, ref bitPosition, 8, ref protocol);
      if (protocol == 1)
      {
        qosMetrics.protocol = "TCP";
      }
      else if (protocol == 2)
      {
        qosMetrics.protocol = "UDP";
      }
      filler(ref bitPosition, 16);

      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.accumulatedRuntime);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.logStart);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.applicationBytesRecieved);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.applicationBytesSent);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.serialIOBytesRecieved);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.serialIOBytesSent);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.tcpSegmentsRecieved);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.tcpSegmentsSent);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.tcpPacketsRecievedWithErrors);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.tcpRetransmissions);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.moduleHardResets);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.udpDatagramsReceived);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.udpDatagramsSent);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.udpDatagramsRecievedWithErrors);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.udpPortErrors);
      filler(ref bitPosition, 192);
    }

    private void serializeGPSMetrics(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.flashFileVersion);
      filler(ref bitPosition, 24);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.accumulatedRuntime);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.invalidNAVUpdates);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.validNAVUpdates);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.oldNAVUpdates);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.positionFilterResets);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.gpsAntennaFaults);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.gpsFatalErrors);
      BigEndianHydrator(raw, ref bitPosition, 4, out qosMetrics.cocomErrors);
      BigEndianHydrator(raw, ref bitPosition, 2, out qosMetrics.maxInvalidDuration);
      BigEndianHydrator(raw, ref bitPosition, 2, out qosMetrics.invalidAndOldNavUpdates);
      ushort minHDOP = 0;
      BigEndianHydrator(raw, ref bitPosition, 2, out minHDOP);
      qosMetrics.minHDOP = minHDOP / 10.0;
      ushort maxHDOP = 0;
      BigEndianHydrator(raw, ref bitPosition, 2, out maxHDOP);
      qosMetrics.maxHDOP = maxHDOP / 10.0;
      BigEndianHydrator(raw, ref bitPosition, 2, out qosMetrics.maxSpeed);
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.avgNumberSvTracked);
      byte lastBootStatus = 0;
      serializer(action, raw, ref bitPosition, 8, ref lastBootStatus);
      qosMetrics.lastBootStatus = (lastBootStatus == 0) ? "COLD" : "WARM";
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.fiveOrLessSvTrackedPercent);
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.minSNR);
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.maxSNR);
      serializer(action, raw, ref bitPosition, 8, ref qosMetrics.avgSNR);
    }

    private void SerializeSkylingFirmwareStatusReportingDiagnostic(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.subtype);
      if (skylineFirmwareStatusReporting.subtype == 0)
      {
        serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwUpdateStatus);
        serializeNulTerminatedString(action, raw, ref bitPosition, ref skylineFirmwareStatusReporting.fwFilename);
      }
      else if (skylineFirmwareStatusReporting.subtype == 1)
      {
        serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.numberOfFWVersions);
        skylineFirmwareStatusReporting.fwVersion = new SkylineFirmwareVersion[skylineFirmwareStatusReporting.numberOfFWVersions];
        for (int i = 0; i < skylineFirmwareStatusReporting.numberOfFWVersions; i++)
        {
          skylineFirmwareStatusReporting.fwVersion[i] = new SkylineFirmwareVersion();
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].fwTarget);
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].fwMajorVersion);
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].fwMinorVersion);
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].fwBuildType);
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].hwMajorVersion);
          serializer(action, raw, ref bitPosition, 8, ref skylineFirmwareStatusReporting.fwVersion[i].hwMinorVersion);
        }
      }
    }

    public class ChironDiagnostic
    {
      public byte Subtype;
      public bool IOStateDiscreteInput0;
      public bool IOStateDiscreteInput1;
      public bool IOStateDiscreteInput2;
      public bool IOStateDiscreteInput3;
      public bool IOStateOutput1;
      public bool IOStateOutput2;
      public bool IOStateOutput3;
      public bool Ignition;
      public short MessageCode;
      public double Heading;
      public byte GPSSpeed;
      public byte SpeedSensorSpeed;
      public short SpeedSensorCalibrationFactor;
      public int SpeedSensorClibrationValue;
      public int BootCount;
      public short RSSI;
      public string IPAddress;
      public int OperatorAndNetworkID;
      public int ConsecutiveCount;
      public int HorizontalSpeed;
      public int VerticalSpeed;
      public int OscillatorDrift;
      public int HorizontalAcceleration;
      public string CommStateText;

      public void SetIPAddress(uint ipAddress)
      {
        if (ipAddress <= 0)
        {
          IPAddress = "0.0.0.0";
          return;
        }

        string xHex = ipAddress.ToString("x");

        byte[] hexBytes = HexDump.HexStringToBytes(xHex);
        if (hexBytes.Length == 4)
        {
          IPAddress = hexBytes[0] + "." + hexBytes[1] + "." + hexBytes[2] + "." + hexBytes[3];
        }
        else
        {
          IPAddress = "0.0.0.0";
        }
      }

      public override string ToString()
      {
        return GetChironMessageString();
      }
      public string GetChironMessageString()
      {
        try
        {
          StringBuilder builder = new StringBuilder(this.GetType().Name);
          FieldInfo[] messageFields = this.GetType().GetFields();
          PropertyInfo[] messageProperties = this.GetType().GetProperties();

          foreach (PropertyInfo property in messageProperties)
          {
            object prop = property.GetValue(this, null);
            if (property.PropertyType != typeof(byte[]))
            {
              if (prop != null)
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), prop.ToString());
              }
            }
          }

          foreach (FieldInfo field in messageFields)
          {
            object f = field.GetValue(this);
            if (f != null && !string.IsNullOrEmpty(f.ToString()))
            {
              builder.Append("\n");
              builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, ":"), f.ToString());
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

    public class QOSMetrics
    {
      public byte type;
      public byte flashFileVersion;
      public string protocol;
      public uint accumulatedRuntime;
      public uint logStart;
      public uint applicationBytesRecieved;
      public uint applicationBytesSent;
      public uint serialIOBytesRecieved;
      public uint serialIOBytesSent;
      public uint tcpSegmentsRecieved;
      public uint tcpSegmentsSent;
      public uint tcpPacketsRecievedWithErrors;
      public uint tcpRetransmissions;
      public uint moduleHardResets;
      public uint udpDatagramsReceived;
      public uint udpDatagramsSent;
      public uint udpDatagramsRecievedWithErrors;
      public uint udpPortErrors;

      public uint invalidNAVUpdates;
      public uint validNAVUpdates;
      public uint oldNAVUpdates;
      public uint positionFilterResets;
      public uint gpsAntennaFaults;
      public uint gpsFatalErrors;
      public uint cocomErrors;
      public ushort maxInvalidDuration;
      public ushort invalidAndOldNavUpdates;
      public double minHDOP;
      public double maxHDOP;
      public ushort maxSpeed;
      public byte avgNumberSvTracked;
      public string lastBootStatus;
      public byte fiveOrLessSvTrackedPercent;
      public byte minSNR;
      public byte maxSNR;
      public byte avgSNR;

      public ushort unexpectedModuleResets;
      public uint bootCount;
      public uint sumOfDisconnectTimes;
      public long standardDeviation;
      public uint minDisconnectDuration;
      public uint maxDisconnectDuration;
      public uint timeNoCommsLessThanEqualToNegative100;
      public uint timeNoCommsGreaterThanEqualToNegative100;
      public uint timeNonRoaming;
      public uint timeRoaming;
      public uint timeInUnkown;
      public uint timeInAttachedState;
      public uint timeInActivatedState;
      public uint timeInGetIPState;
      public uint timeInOpenSocketState;
      public uint timeInNetEntryState;
      public uint timeConnectedToServerOnUnknownNetwork;
      public uint timeSearchingOnUnknownNetwork;
      public uint pdpActivateAttempts;
      public uint pdpContextFailures;
      public uint attachAttempts;
      public ushort commsDiconnects;
      public ushort disconnectSamplesForStdDev;
      public ushort simFailures;
      public string apnString;
      public string moduleVersionString;
      public uint operatorID1;
      public uint timeSearchingOnOperator1;
      public uint timeConnectedOnOperator1;
      public uint operatorID2;
      public uint timeSearchingOnOperator2;
      public uint timeConnectedOnOperator2;
      public uint operatorID3;
      public uint timeSearchingOnOperator3;
      public uint timeConnectedOnOperator3;
      public short connectRssiMinInHomeSite;
      public short connectRssiMaxInHomeSite;
      public int sumRssiInHomeSite;
      public int rssiSamplesInHomeSite;
      public short connectRssiMinOutsideHomeSite;
      public short connectRssiMaxOutsideHomeSite;
      public int sumRssiOutsideHomeSite;
      public int rssiSamplesOutsideHomeSite;
      public ushort errorCode1;
      public uint errorCode1Count;
      public ushort errorCode2;
      public uint errorCode2Count;
      public ushort errorCode3;
      public uint errorCode3Count;
      public ushort networkScansRequested;

      public string errorLogMsg;

      public override string ToString()
      {
        return GetQOSMessageString();
      }

      public string GetQOSMessageString()
      {
        try
        {
          StringBuilder builder = new StringBuilder(this.GetType().Name);
          FieldInfo[] messageFields = this.GetType().GetFields();
          PropertyInfo[] messageProperties = this.GetType().GetProperties();

          foreach (PropertyInfo property in messageProperties)
          {
            object prop = property.GetValue(this, null);
            if (property.PropertyType != typeof(byte[]))
            {
              if (prop != null)
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), prop.ToString());
              }
            }
          }

          foreach (FieldInfo field in messageFields)
          {
            object f = field.GetValue(this);
            if (f != null && !string.IsNullOrEmpty(f.ToString()))
            {
              builder.Append("\n");
              builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, ":"), f.ToString());
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

    public class SkylineFirmwareStatusReporting
    {
      public byte subtype;
      public byte fwUpdateStatus;
      public string fwFilename;

      public byte numberOfFWVersions;
      public SkylineFirmwareVersion[] fwVersion;

      public override string ToString()
      {
        return GetSkylineFirmwareStatusReportingString();
      }

      public string GetSkylineFirmwareStatusReportingString()
      {
        try
        {
          StringBuilder builder = new StringBuilder(this.GetType().Name);
          FieldInfo[] messageFields = this.GetType().GetFields();
          PropertyInfo[] messageProperties = this.GetType().GetProperties();

          foreach (PropertyInfo property in messageProperties)
          {
            object prop = property.GetValue(this, null);
            if (property.PropertyType != typeof(byte[]))
            {
              if (prop != null && property.PropertyType != typeof(PlatformMessage) && property.PropertyType != typeof(MessageCategory) && property.DeclaringType != typeof(PlatformMessage))
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), prop.ToString());
              }
            }
            else if (prop != null && property.PropertyType != typeof(byte[]))
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
              if (field.FieldType != typeof(byte[]))
              {
                var array = f as IList;

                builder.Append("\n");
                builder.AppendFormat("{0} Count: {1}", field.Name, array.Count);

                foreach (var arrayItem in array)
                {
                  builder.Append("\n");
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

    public class SkylineFirmwareVersion {
      public byte fwTarget;
      public byte fwMajorVersion;
      public byte fwMinorVersion;
      public byte fwBuildType;
      public byte hwMajorVersion;
      public byte hwMinorVersion;

      public override string ToString()
      {
        return GetSkylineFirmwareVersionString();
      }

      public string GetSkylineFirmwareVersionString()
      {
        try
        {
          StringBuilder builder = new StringBuilder(this.GetType().Name);
          FieldInfo[] messageFields = this.GetType().GetFields();
          PropertyInfo[] messageProperties = this.GetType().GetProperties();

          foreach (PropertyInfo property in messageProperties)
          {
            object prop = property.GetValue(this, null);
            if (property.PropertyType != typeof(byte[]))
            {
              if (prop != null)
              {
                builder.Append("\n");
                builder.AppendFormat("{0,-26}  {1}", DecorateString(property.Name, ":"), prop.ToString());
              }
            }
          }

          foreach (FieldInfo field in messageFields)
          {
            object f = field.GetValue(this);
            if (f != null && !string.IsNullOrEmpty(f.ToString()))
            {
              builder.Append("\n");
              builder.AppendFormat("{0,-26}  {1}", DecorateString(field.Name, ":"), f.ToString());
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

    public UInt16 PortNumber;
    public bool SaveInDatabase;
    public ChironDiagnostic chiron;
    public QOSMetrics qosMetrics;
    public SkylineFirmwareStatusReporting skylineFirmwareStatusReporting;
    public byte[] Data;

    public int ReportedSizeOfPayloadDiscrepencyBytes;

    #region IXmlEventFragment Members

    public void XmlEvent(XmlWriter xw, int version)
    {
      xw.WriteStartElement("UserData", PlatformEvents.Constants.RealtimeNamespace);
      if (Data != null)
      {
        xw.WriteBase64(Data, 0, Data.Length);
      }
      xw.WriteEndElement();

      xw.WriteElementString("Port", PlatformEvents.Constants.RealtimeNamespace, XmlConvert.ToString(PortNumber));
    }

    public string CsvValues
    {
      get
      {
        // HACK: in the database, we store a comma because we put the base-64 encoded port-based data
        // before the data_value string.  However, I need this to look like this because this is how
        // it is saved to the database.  -sigh-

        return String.Format(",{0}", PortNumber);
      }
    }

    #endregion
  }

  public class UnknownTrackerUserDataMessage : TrackerUserDataMessage
  {
    public static new readonly int kPacketID = 0x81;      // This packet does not exist in our wireless format.

    public override int PacketID
    {
      get { return kPacketID; }
    }

    public override UDEventID EventID
    {
      get { return UDEventID.Unparsed; }
    }

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      if (action == SerializationAction.Hydrate)
      {
        uint realDataLength = bytesLeftInMessage(bitPosition);

        serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref Data);
      }
      else
      {
        serializeFixedLengthBytes(action, raw, ref bitPosition, (uint)(Data == null ? 0 : Data.Length), ref Data);
      }
    }
    public byte[] Data;
  }

 
}

