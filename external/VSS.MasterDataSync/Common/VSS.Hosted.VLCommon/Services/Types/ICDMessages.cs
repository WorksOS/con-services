using System;
using System.Configuration;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using VSS.Hosted.VLCommon.ExtensionMethodStringScrub;

namespace VSS.Hosted.VLCommon.ICD
{

  public static class Constants
  {
    public const string OEMDATAFEEDNS = "http://www.trimble.com/MachineData";
  }

  /// <summary>
  /// Each device message will be encapsulated into a data packet as shown in Table .  
  /// This packet starts with a control character and the length of the Device Packet.   
  /// Next is the Device packet followed by a CRC to verify the packet data.
  /// </summary>
  public class DeviceMessageWrapper
  {
    /// <summary>
    /// This value should always be 0x02
    /// </summary>
    public byte StartControlByte { get; set; }
    public ushort PacketLength { get; set; }
    public DevicePacket DevicePacket { get; set; }
    public byte CRC { get; set; }
  }




  /// <summary>
  /// General type found in DeviceMessageWrapper
  /// Implemented by ...
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof(MachineEvent))]
  [XmlInclude(typeof(MachineEvent))]
  [KnownType(typeof(ServiceOutageReport))]
  [XmlInclude(typeof(ServiceOutageReport))]
  [KnownType(typeof(PersonalityReport))]
  [XmlInclude(typeof(PersonalityReport))]
  public class DevicePacket
  {}

  #region MachineEvent
  /// <summary>
  /// 0x07 Machine Event
  /// This message provides a mechanism to pass location, SMU, and general telematics data, to the back office. 
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class MachineEvent : DevicePacket
  {
    /// <summary>
    /// Packet ID: 0x07
    /// </summary>
    [DataMember(Order = 0)]
    public byte ID { get; set; }

    [DataMember(Order = 1)]
    public byte SeqID { get; set; }

    /// <summary>
    /// UTC Date Time, LSB = 1 sec, Seconds elapsed since 1 Jan 2009
    /// </summary>
    [DataMember(Order = 2)]
    public uint DateTimeUTC { get; set; }

    //DateTimeUTC Converted to Date Time Based On MTS ICD
    public DateTime EventUTC
    {
      get { return new DateTime(2009, 01, 01, 0, 0, 0, DateTimeKind.Utc).AddSeconds(DateTimeUTC); }
    }
    /// <summary>
    /// Service Meter Hours (SMH). LSB = 0.1 hours.  
    /// Special Value “FFFFFFFF” = “SMH Unavailable”. 
    /// (SMH Unavailable is a status indication, and the corresponding 
    /// decimal value is not intended for back office utilization calculations)
    /// </summary>
    [DataMember(Order = 3)]
    public uint SMH { get; set; }

    public double? ServiceMeterHours
    {
      get
      {
        if (SMH == uint.MaxValue)
          return null;
        return (double)(SMH / 10.0);
      }
    }

    private int latitude;
    /// <summary>
    /// Latitude (1 degree = 2 base 16)
    /// </summary>
    [DataMember(Order = 4, EmitDefaultValue=false)]
    public int? Lat
    {
      get
      {
        return ICDMessagesHelper.ShouldEmitValue() ? latitude : (int?)null;
      }
      set { latitude = value ?? 0; }
    }

    public double? Latitude
    {
      get
      {
        return (double)latitude * Math.Pow(2.0, -16.0);
      }
    }

    private int longitude;
    /// <summary>
    /// Longitude (1 degree = 2 base 16)
    /// </summary>
    [DataMember(Order = 5, EmitDefaultValue = false)]
    public int? Lon 
    {
      get
      {
        return ICDMessagesHelper.ShouldEmitValue() ? longitude : (int?)null;
      }
      set { longitude = value ?? 0; }
    }

    public double? Longitude
    {
      get { return (double)longitude * Math.Pow(2.0, -16.0); }
    }

    private string address;
    /// <summary>
    /// Address
    /// </summary>
    [DataMember(Order = 6, EmitDefaultValue = false)]
    public string Address
    {
      get
      {
        return ICDMessagesHelper.ShouldEmitValue() ? null : address ?? "-";
      }
      set { address = value; }
    }

    /// <summary>
    /// Distance Traveled (LSB = 1 hectometer = 100m = 0.062137119 miles)
    /// <para>This field may be filled with GPS-derived distance 
    /// traveled or with vehicle odometer from the vehicle bus. 
    /// The default is GPS-derived distance traveled.</para>
    /// <para>This can be changed over-the-air using configuration 
    /// message Sub-Type 0x23 “Configure Machine Event Header”.</para>
    /// <para>Note that if this is configured to be odometer from the 
    /// vehicle bus, GPS-derived distance will not be a fallback 
    /// if the bus-sourced odometer fails.</para>
    /// <para>If the source fails to provide distance/mileage for 
    /// any reason, “unknown” will be returned in this field 
    /// (signified by setting the value to FFFFFFh).</para>
    /// <para>Note that it is not required, nor is it actually even 
    /// possible using the current ICD messages, to indicate 
    /// to the server what the data source for this field is.</para>
    /// </summary>
    [DataMember(Order = 7)]
    public uint Distance { get; set; }

    public double? DistanceInMiles
    {
      get
      {
        if (Distance == 0xFFFFFF)
          return null;

        return (double)Distance * 0.062137119; 
      }
    }
    /// <summary>
    /// Speed (LSB = 0.5 m/s = 1.118468146 mph)
    /// </summary>
    [DataMember(Order = 8)]
    public byte Speed { get; set; }

    public double SpeedMPH
    {
      get { return (double)Speed * 1.118468151; }
    }
    /// <summary>
    /// Track LSB = 36 degrees x 2 base -7 = 2.8125 degrees
    /// </summary>
    [DataMember(Order = 9)]
    public sbyte Track { get; set; }

    public double TrackInDegrees
    {
      get { return (double)Track * (360.0 / Math.Pow(2.0, 7.0)); }
    }
    /// <summary>
    /// Location Age Unit (0 = secs, 1 = mins, 2 = hrs, 3 = days)
    /// </summary>
    [DataMember(Order = 10)]
    public LocationAgeUnit LocAgeUnit { get; set; }

    /// <summary>
    /// Location Age (value 0xFF is “invalid” position location)
    /// </summary>
    [DataMember(Order = 11)]
    public byte LocAge { get; set; }

    /// <summary>
    /// Location Uncertainty Unit (0 = cms, 1 = meters)
    /// </summary>
    [DataMember(Order = 12)]
    public LocationUncertaintyUnit LocUncertaintyUnit { get; set; }

    /// <summary>
    /// Location Uncertainty (range 0 .. 255). 
    /// 0 = Not available. 
    /// 255 = >= 255.
    /// </summary>
    [DataMember(Order = 13)]
    public byte LocUncertainty { get; set; }

    /// <summary>
    /// Event Block Count (0..255)
    /// </summary>
    [DataMember(Order = 14)]
    public byte Count { get; set; }

    /// <summary>
    /// Array of MachineEventBlocks
    /// </summary>
    [DataMember(Order = 15)]
    public MachineEventBlock[] Blocks { get; set; }

    /// <summary>
    /// True is LocAge is not 0xFF (255)
    /// </summary>
    public bool LocAgeValid
    {
      get { return LocAge != byte.MaxValue; }
    }

    /// <summary>
    /// True if SMH is not FFFFFFFF (?)
    /// </summary>
    public bool SMHAvailable
    {
      get { return (SMH != uint.MaxValue); }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum LocationAgeUnit : byte
  {
    [EnumMember]
    Seconds = 0,

    [EnumMember]
    Minutes = 1,

    [EnumMember]
    Hours = 2,

    [EnumMember]
    Days = 3,

    [EnumMember]
    Months = 4
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum LocationUncertaintyUnit : byte
  {
    [EnumMember]
    Cms = 0,

    [EnumMember]
    Meters = 1
  }

  /// <summary>
  /// Found in MachineEvent
  /// Encapsulates EventBlockData information
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class MachineEventBlock
  {
    /// <summary>
    /// UTC Delta (delta from the UTC in the header) -32768..32768
    /// </summary>
    [DataMember(Order = 0)]
    public short DeltaUTC { get; set; }

    /// <summary>
    /// Source (0 = Gateway, 1 = LiteVIMS, 2 = Radio, 3 = Vehicle Bus)
    /// </summary>
    [DataMember(Order = 1)]
    public MachineEventSource Source { get; set; }

    /// <summary>
    /// Length (0 .. M)
    /// </summary>
    [DataMember(Order = 2)]
    public ushort Length { get; set; }

    /// <summary>
    /// Data (variable length binary data)
    /// </summary>
    [DataMember(Order = 3)]
    public EventBlockData Data { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineEventSource : byte
  {
    [EnumMember]
    Gateway = 0,

    [EnumMember]
    LiteVIMS = 1,

    [EnumMember]
    Radio = 2,

    [EnumMember]
    VehicleBus = 3
  }

  /// <summary>
  /// General type found in MachineEventBlock
  /// Implemented by ...
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof(PositionEventBlockData))]
  [XmlInclude(typeof(PositionEventBlockData))]
  [KnownType(typeof(EngineStartStopEventBlockData))]
  [XmlInclude(typeof(EngineStartStopEventBlockData))]
  [KnownType(typeof(IgnitionOnOffEventBlockData))]
  [XmlInclude(typeof(IgnitionOnOffEventBlockData))]
  [KnownType(typeof(DiscreteInputEventBlockData))]
  [XmlInclude(typeof(DiscreteInputEventBlockData))]
  [KnownType(typeof(SpeedingIndicationEventBlockData))]
  [XmlInclude(typeof(SpeedingIndicationEventBlockData))]
  [KnownType(typeof(StoppedNotificationEventBlockData))]
  [XmlInclude(typeof(StoppedNotificationEventBlockData))]
  [KnownType(typeof(GeofenceEntryExitEventBlockData))]
  [XmlInclude(typeof(GeofenceEntryExitEventBlockData))]
  [KnownType(typeof(GeneratorOperatingStateEventBlockData))]
  [XmlInclude(typeof(GeneratorOperatingStateEventBlockData))]
  [KnownType(typeof(DeviceSpecialReportingStatusEventBlockData))]
  [XmlInclude(typeof(DeviceSpecialReportingStatusEventBlockData))]
  [KnownType(typeof(ECMInformationGatewayEventBlockData))]
  [XmlInclude(typeof(ECMInformationGatewayEventBlockData))]
  [KnownType(typeof(GatewayAdminEventBlockData))]
  [XmlInclude(typeof(GatewayAdminEventBlockData))]
  [KnownType(typeof(FaultCodeReportEventBlockData))]
  [XmlInclude(typeof(FaultCodeReportEventBlockData))]
  [KnownType(typeof(DiagnosticReportGatewayEventBlockData))]
  [XmlInclude(typeof(DiagnosticReportGatewayEventBlockData))]
  [KnownType(typeof(FuelEngineReportGatewayEventBlockData))]
  [XmlInclude(typeof(FuelEngineReportGatewayEventBlockData))]
  [KnownType(typeof(MachineActivityEventBlockData))]
  [XmlInclude(typeof(MachineActivityEventBlockData))]
  [KnownType(typeof(SMHAdjustmentEventBlockData))]
  [XmlInclude(typeof(SMHAdjustmentEventBlockData))]
  [KnownType(typeof(ECMAddressClaimEventBlockData))]
  [XmlInclude(typeof(ECMAddressClaimEventBlockData))]
  [KnownType(typeof(ECMInformationVehicleBusEventBlockData))]
  [XmlInclude(typeof(ECMInformationVehicleBusEventBlockData))]
  [KnownType(typeof(FuelEngineReportVehicleBusEventBlockData))]
  [XmlInclude(typeof(FuelEngineReportVehicleBusEventBlockData))]
  [KnownType(typeof(DiagnosticReportVehicleBusEventBlockData))]
  [XmlInclude(typeof(DiagnosticReportVehicleBusEventBlockData))]
  [KnownType(typeof(J1939ParamsReportEventBlockData))]
  [XmlInclude(typeof(J1939ParamsReportEventBlockData))]
  [KnownType(typeof(StatisticsReportEventBlockData))]
  [XmlInclude(typeof(StatisticsReportEventBlockData))]
  [KnownType(typeof(VehicleBusBinaryEventBlockData))]
  [XmlInclude(typeof(VehicleBusBinaryEventBlockData))]
  [KnownType(typeof(TPMSReportVehicleBusEventBlockData))]
  [XmlInclude(typeof(TPMSReportVehicleBusEventBlockData))]
  [KnownType(typeof(PassiveRFIDData))]
  [XmlInclude(typeof(PassiveRFIDData))]
  [KnownType(typeof(RFIDDeviceStatusData))]
  [XmlInclude(typeof(RFIDDeviceStatusData))]
  [KnownType(typeof(PayloadReportVehicleBusEventBlockData))]
  [XmlInclude(typeof(PayloadReportVehicleBusEventBlockData))]
  [KnownType(typeof(RadioDeviceMachineSecurityEventBlockData))]
  [XmlInclude(typeof(RadioDeviceMachineSecurityEventBlockData))]
  [KnownType(typeof(TMSInformationMessageEventBlockdata))]
  [XmlInclude(typeof(TMSInformationMessageEventBlockdata))]
  [KnownType(typeof(J1939DiagnosticReportGatewayEventBlockData))]
  [XmlInclude(typeof(J1939DiagnosticReportGatewayEventBlockData))]
  public class EventBlockData {}

  #region Event Block Data, Source == Radio

  /// <summary>
  /// Block 0x00 Position Block
  /// This block is used to bundle delta positions into the 
  /// Machine Event header. It is used in preference to the 
  /// Position Bundle II message if Uncertainty and Age are
  /// required in the position bundle.
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PositionEventBlockData : EventBlockData
  {
    /// <summary>
    /// Block ID (0x00)
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    private int deltaLat;
    /// <summary>
    /// Delta Latitude (signed, 1 degree = 216, range = +/- 1 degree)
    /// </summary>
    [DataMember(Order = 1, EmitDefaultValue = false)]
    public int? DeltaLat 
    {
      get { return ICDMessagesHelper.ShouldEmitValue() ? deltaLat : (int?)null; }
      set { deltaLat = value ?? 0; } 
    }

    private int deltaLon;
    /// <summary>
    /// Delta Longitude (signed, 1 degree = 216, range = +/- 1 degree)
    /// </summary>
    [DataMember(Order = 2, EmitDefaultValue = false)]
    public int? DeltaLon
    {
      get { return ICDMessagesHelper.ShouldEmitValue() ? deltaLon : (int?)null; }
      set { deltaLon = value ?? 0; }
    }    

    /// <summary>
    /// Delta-distance (unsigned, 
    /// LSB = 1 hectometer = 100m = 0.062137119 miles, 
    /// range = 0 - 102.3 km, 63.56… miles)
    /// </summary>
    [DataMember(Order = 3)]
    public ushort DeltaDistance { get; set; }
    /// <summary>
    /// Speed (LSB = 0.5 m/s = 1.118468146 mph)
    /// Same as in MachineEvent
    /// </summary>
    [DataMember(Order = 4)]
    public byte Speed { get; set; }
    /// <summary>
    /// Track (unsigned, LSB = 360 degrees * 2-7 = 2.8125)
    /// Same as in MachineEvent
    /// </summary>
    [DataMember(Order = 5)]
    public byte Track { get; set; }
    /// <summary>
    /// (0 = cms, 1 = meters) 
    /// Same as in MachineEvent 
    /// </summary>
    [DataMember(Order = 6)]
    public LocationUncertaintyUnit LocUncertaintyUnit { get; set; }
    /// <summary>
    /// range 0 .. 255
    /// Same as in MachineEvent 
    /// </summary>
    [DataMember(Order = 6)]
    public byte LocUncertainty { get; set; }
    /// <summary>
    /// (0 = valid, 1 = invalid)
    /// </summary>
    [DataMember(Order = 7)]
    public bool IsInvalid { get; set; }
  }

  /// <summary>
  /// Block 0x01 Engine Start/Stop
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class EngineStartStopEventBlockData : EventBlockData
  {
    /// <summary>
    /// Block ID (0x01)
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }
    /// <summary>
    /// 1 = Engine Start, 0 = Engine Stop
    /// </summary>
    [DataMember(Order = 1)]
    public bool IsEngineStart { get; set; }
  }

  /// <summary>
  /// Block 0x04 Ignition On/Off Message
  /// 
  /// <para>The ignition on/off message is sent by the device 
  /// whenever the vehicle ignition switch is turned on or off.  
  /// By default the device will report ignition on/off events.  
  /// Reporting can be enabled or disabled with Sub-Type 0x06
  /// Ignition Reporting Configuration Message.</para>
  /// 
  /// <para>The message contains the current value of the engine run 
  /// time counter. This counter logs number of hours that the 
  /// ignition has been on.  The counter will wrap back to 
  /// zero once it has reached its maximum value of 32,767 
  /// hours (over 6 years if the vehicle is operated 16 
  /// hours/day, 6 days/week). The run time counter can be 
  /// set using the Set Device Mileage and Run Time Counters 
  /// Message described in Section 3.1.1.</para>
  /// 
  /// <para>A 30 second delay is used for both on and off events to 
  /// filter out events caused by rapidly cycling the ignition 
  /// as might be seen when trying to start a vehicle.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class IgnitionOnOffEventBlockData : EventBlockData
  {
    /// <summary>
    /// Block ID (0x04)
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// 0 = off, 1 = on
    /// </summary>
    [DataMember(Order = 1)]
    public bool IsOn { get; set; }

    /// <summary>
    /// Runtime Counter Hours
    /// </summary>
    [DataMember(Order = 2)]
    public ushort RuntimeCounter { get; set; }
  }

  /// <summary>
  /// Block 0x05 Discrete Input Message
  /// 
  /// <para>The Discrete Input message indicates the state of any 
  /// one of the three discrete inputs to the device when 
  /// the state of the input changes.  This message indicates
  /// a discrete input event has occurred by setting the 
  /// corresponding discrete bit to 1 and setting the bit of 
  /// the discrete state to the input state.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DiscreteInputEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x05
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// Discrete 1, 2, or 3
    /// </summary>
    [DataMember(Order = 1)]
    public byte DiscreteInput { get; set; }

    [DataMember(Order = 2)]
    public bool DiscreteState { get; set; }
  }

  /// <summary>
  /// Block 0x06 Speeding Indication Message
  /// 
  /// <para>The Speeding Indication message is sent when the device 
  /// detects the beginning and ending of a speeding event.  
  /// The definition of a speeding event and whether or not 
  /// speeding events are reported are controlled by the 
  /// Speeding Reporting Configuration message described in 
  /// Section 3.1.3.2.</para>
  /// 
  /// <para>When the vehicle exceeds the specified speed threshold 
  /// continuously for the specified duration, the Speeding 
  /// Indication message is sent with the begin/end flag set.  
  /// When the vehicle speed drops below the speed threshold 
  /// after a speeding indication has been sent, the Speeding 
  /// Indication message is sent with the begin/end flag off.  
  /// The distance and duration fields indicate the distance 
  /// traveled and time elapsed since the vehicle’s speed 
  /// first exceeded the velocity threshold.  This is the 
  /// case for both the start of speeding and end of speeding 
  /// indications.  The maximum speed field indicates the 
  /// maximum speed achieved in the given duration.</para>
  /// 
  /// <para>Note: Start and End speeding indications must always be 
  /// reported in pairs.</para>
  ///
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class SpeedingIndicationEventBlockData : EventBlockData
  {
    /// <summary>
    /// Block ID (0x06)
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// In Seconds
    /// </summary>
    [DataMember(Order = 1)]
    public ushort Duration { get; set; }

    /// <summary>
    /// Tenths of Miles
    /// </summary>
    [DataMember(Order = 2)]
    public ushort Distance { get; set; }

    /// <summary>
    /// 1-Speeding Started, 0-Speeding Stopped
    /// </summary>
    [DataMember(Order = 3)]
    public bool IsSpeedingStart { get; set; }

    /// <summary>
    /// MPH
    /// </summary>
    [DataMember(Order = 4)]
    public byte MaxSpeed { get; set; }
  }

  /// <summary>
  /// Block 0x07 Stopped Notification Message
  /// 
  /// <para>The Stopped Notification message is sent when the device 
  /// detects the beginning and ending of a stop event.  The 
  /// definition of a stop event and whether or not stop events 
  /// are reported are controlled by the Stopped Notification 
  /// Configuration message described in Section 3.1.3.4 and 
  /// the Moving Configuration message described in Section 
  /// 3.1.3.9.</para>
  /// 
  /// <para>The Stopped Notification message is sent with the 
  /// Stopped/Started flag set when the vehicle speed is below 
  /// the specified threshold continuously for the specified 
  /// duration threshold.</para>
  /// 
  /// <para>The message is sent again with the Stopped/Started flag 
  /// cleared when the vehicle has been determined to be moving 
  /// again.  The definition of moving is determined by the 
  /// Moving Configuration message. Devices use the distance 
  /// based moving detection by default.  In this case, 
  /// movement after the initial stopped event beyond the radius 
  /// specified in the Moving Configuration message is required 
  /// to indicate moving.  If the GPS solution is invalid when 
  /// the stop event occurs, then moving will be reported once 
  /// the solution becomes valid.</para>
  /// 
  /// <para>If speed is used, then the behavior depends on whether or 
  /// not a speed sensor is connected to the device.  If a speed 
  /// sensor is connected, then any detected movement of the 
  /// wheels will indicate started moving.  If a speed sensor 
  /// is not connected, the speed indicated by GPS above the 
  /// speed threshold indicated in the Stopped Notification 
  /// Configuration message will indicate moving. If a speed 
  /// sensor is connected and the vehicle is driven below the 
  /// stop speed threshold but continues moving after the stop 
  /// duration threshold expires, it will report moving 
  /// immediately after reporting a stop event.</para>
  /// 
  /// <para>The Suspicion Moving Alert flag is shown in BIT1 of the 
  /// message type in the table below. When this bit is set 
  /// to “1”, it means that: 
  /// a) the machine location has moved out of predefined range 
  ///    when it compares to the previous reported GPS location; or 
  /// b) the motion sensor has triggered.</para>
  /// 
  /// <para>When suspicion motion is detected, the device must send 
  /// a notification message to the back office.  If this bit 
  /// is set to “0”, it indicates that the machine is either 
  /// stationary or remains in a static location within a 
  /// predefined range.  The device doesn’t need to send the 
  /// alert notification to the back office for this status.</para>
  /// 
  /// <para>Note: Stopped and Started events must always be reported in pairs.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class StoppedNotificationEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x07
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// 0-Started Moving, 1-Stopped Moving
    /// </summary>
    [DataMember(Order = 1)]
    public bool IsStoppedMoving { get; set; }

    /// <summary>
    /// 0- Stationary, 1- Suspicion Moving
    /// </summary>
    [DataMember(Order = 2)]
    public bool IsSuspicousMove { get; set; }
  }

  /// <summary>
  /// Block 0x08 Geofence Entry/Exit Message
  /// This message is used by a device to indicate geofence 
  /// entry and exit, using geofences dispatched to the 
  /// device via the 0x14 Configure Polygon Message.  
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class GeofenceEntryExitEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x08
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// Not used
    /// </summary>
    [DataMember(Order = 1)]
    public byte Type { get; set; }

    /// <summary>
    /// 0 – Arrival, 1 – Departure
    /// </summary>
    [DataMember(Order = 2)]
    public bool IsDeparture { get; set; }

    [DataMember(Order = 3)]
    public uint GeofenceID { get; set; }
  }

  /// <summary>
  /// Block 0x09 Generator “Ready-to-Run” Operating State
  /// This message is used to display the engine status of 
  /// the EPD/Gensets application in the PL42x programs, It 
  /// allows user to view of if the engine at “Read-to-Run” 
  /// state.  
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class GeneratorOperatingStateEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x09
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// 0 = Not Ready-to-Run, 1 = Ready-to-Run
    /// </summary>
    [DataMember(Order = 1)]
    public byte IsReadyToRun { get; set; }

    /// <summary>
    /// Value of SPN3543 (see J1939-71 spec) in Enum
    /// 0: Engine stopped
    /// 4: Running
    /// 5: Cool down
    /// 6: Engine stopping
    /// 15: Not available
    /// Other value are reserved for future 
    /// </summary>
    [DataMember(Order = 2)]
    public GensetOperatingState OperatingState { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum GensetOperatingState : byte
  {
    [EnumMember]
    EngineStopped = 0,

    [EnumMember]
    Running = 4,

    [EnumMember]
    CoolDown = 5,

    [EnumMember]
    EngineStopping = 6,

    [EnumMember]
    NotAvailable = 15
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DeviceSpecialReportingStatusEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x10
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// Total length of block 
    /// (including block ID; allows for future expansion)
    /// </summary>
    [DataMember(Order = 1)]
    public byte Length { get; set; }

    /// <summary>
    /// See User Data message 
    /// Sub-Type 0x26 Set Device Special Reporting Mode
    /// 0x00 = No Reporting (Opt Out Mode)
    /// 0x01 = Subscribed (Normal Mode)
    /// 0x02 = OEM Tracking Mode
    /// 0x03-0xFF = Reserved
    /// </summary>
    [DataMember(Order = 2)]
    public byte Mode { get; set; }

    /// <summary>
    /// 0 = Mode request accepted
    /// 1 = Mode request rejected – Tamper enabled
    /// 2 = Mode request rejected – duplicate request.
    /// 3-255 Reserved
    /// </summary>
    [DataMember(Order = 3)]
    public byte ModeStatus { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PassiveRFIDData : EventBlockData
  {
    private string _epcCode;

    /// <summary>
    /// BlockID 0x15
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    /// <summary>
    /// EPC Code string 
    /// </summary>
    [DataMember(Order = 1)]
    public string EPCCode
    {
      get { return _epcCode.ToString(true); }
      set { _epcCode = value; }
    }

    private int latitude;
    /// <summary>
    /// Latitude (1 degree = 2 base 16)
    /// </summary>
    [DataMember(Order = 2, EmitDefaultValue = false)] 
    public int? Latitude
    {
      get { return ICDMessagesHelper.ShouldEmitValue() ? latitude : (int?)null; }
      set { latitude = value ?? 0; }
    }

    public double? LatitudeInDegrees
    {
      get
      {
        return ICDMessagesHelper.ShouldEmitValue() ? (double)latitude * Math.Pow(2.0, -16.0) : (double?)null;
      }
    }

    private int longitude;
    /// <summary>
    /// Longitude (1 degree = 2 base 16)
    /// </summary>
    [DataMember(Order = 3, EmitDefaultValue = false)] 
    public int? Longitude
    {
      get { return ICDMessagesHelper.ShouldEmitValue() ? longitude : (int?)null; }
      set { longitude = value ?? 0; }
    }

    public double? LongitudeInDegrees
    {
      get
      {
        return ICDMessagesHelper.ShouldEmitValue() ? (double)longitude * Math.Pow(2.0, -16.0) : (double?)null;
      }
    }
  
    /// <summary>
    /// PositionAge 0xFF = invalid
    /// </summary>
    [DataMember(Order = 4)]
    public byte PositionAge;

    /// <summary>
    /// Location Age Unit (0 = secs, 1 = mins, 2 = hrs, 3 = days, 4 = months)
    /// </summary>
    [DataMember(Order = 5)]
    public byte PositionAgeUnit;

    /// <summary>
    /// Antenna 0 = NA, 1 = Antenna 1, 2 = Antenna 2
    /// </summary>
    [DataMember(Order = 6)]
    public byte AntennaSource;

    /// <summary>
    /// AntennaSignalStrength -128 to 127
    /// </summary>
    [DataMember(Order = 7)]
    public short AntennaSignalStrength;

    [DataMember(Order = 8)]
    public byte TagType;
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class RFIDDeviceStatusData : EventBlockData
  {
    /// <summary>
    /// BlockID 0x16
    /// </summary>
    [DataMember(Order = 0)]
    public byte BlockID;

    /// <summary>
    /// Length
    /// Total length of block (Excluding the block ID and excluding the length of this Byte Count field), this allows for future expansion
    /// </summary>
    [DataMember(Order = 1)] 
    public byte Length;
    /// <summary>
    /// ReportingMode
    ///1 = RFID product subcomponent status
    ///2 = configuration status for Vega M5e RFID device
    ///3 = Native fault code from Vega M5e RFID device
    ///4 = configuration status for Vega M6e RFID device
    ///5 = Native fault code from M6e Device
    /// </summary>
    [DataMember(Order = 2)]
    public byte ReportingMode;

    /// <summary>
    /// CommLinkDown
    /// false communication link between the Telematics device and RFID Reader is ok
    /// true Telematics device can't get response from the RFID Reader now
    /// </summary>
    [DataMember(Order = 3)]
    public bool? CommLinkDown;

    /// <summary>
    /// TelematicsDeviceEnabledReadFunction
    /// true Telematics device enabled the RFID read function in the RFID Reader
    /// false Telematics device disabled the RFID read function in the RFID Reader
    /// </summary>
    [DataMember(Order = 4)]
    public bool? TelematicsDeviceEnabledReadFunction;

    /// <summary>
    /// RegionalSettingInconsistent
    /// true RFID Regional setting is inconsistent with the features available in the RFID hardware module
    /// false RFID Regional setting is consistent with the features available in the RFID hardware module
    /// </summary>
    [DataMember(Order = 5)]
    public bool? RegionalSettingInconsistent;

    /// <summary>
    /// RFIDReadEnabled
    /// Report M5e RFID device configuration status:
    /// Bit 0: RFID Reader enabled or disabled
    /// Bit #0=0: disabled
    /// Bit #0=1: enabled 
    /// </summary>
    [DataMember(Order = 6)]
    public byte? RFIDReadEnabled;

    /// <summary>
    /// TriggerSource
    /// Bit 1-3: Trigger sources of RFID Read
    /// </summary>
    [DataMember(Order = 7)]
    public byte? TriggerSource;

    /// <summary>
    /// Bit 4: Antenna setting status
    /// Bit #4=0: dynamic switch
    /// Bit #4=1: equal time switching
    /// </summary>
    [DataMember(Order = 8)]
    public byte? AntennaSwitchingStatus;

    /// <summary>
    /// Bit 5-6: Link rate
    /// Bit #5-6=00: "250kHz"
    /// Bit #5-6=01: "640kHz"
    /// </summary>
    [DataMember(Order = 9)]
    public byte? LinkRate;

    /// <summary>
    /// Bit 7-8: Tari
    /// Bit #7-8=00: "25us"
    /// Bit #7-8=01: "12.5us"
    /// Bit #7-8=10: "6.25us"
    /// </summary>
    [DataMember(Order = 10)]
    public byte? Tari;

    /// <summary>
    /// Bit 9-10: Miller Value setting status
    /// Bit #9-10=00: FM0
    /// Bit #9-10=01: M2
    /// Bit #9-10=10: M4
    /// Bit #9-10=11: M8
    /// </summary>
    [DataMember(Order = 11)] 
    public byte? MillerValueSettingStatus;

    /// <summary>
    /// Bit 11-12: Session setting status
    /// Bit #11-12=00: S0
    /// Bit #11-12=01: S1
    /// Bit #11-12=10: S2
    /// Bit #11-12=11: S3
    /// </summary>
    [DataMember(Order = 12)]
    public byte? SessionSettingStatus;

    /// <summary>
    /// Bit 13-14: Target setting status
    /// Bit #13-14=00: "A"
    /// Bit #13-14=01: "B"
    /// Bit #13-14=10: "AB"
    /// Bit #13-14=11: "BA"
    /// </summary>
    [DataMember(Order = 13, IsRequired = false)]
    public byte? TargetSettingStatus;

    /// <summary>
    /// Bit 15: Gen2Q setting status
    /// Bit #15=0: dynamic Q
    /// Bit #15=1: fixed Q
    /// </summary>
    [DataMember(Order = 14)]
    public bool? Gen2QIsFixedQ;

    /// <summary>
    /// Bit 16-18: Baud rate
    /// Bit #16-18=000: 9600
    /// Bit #16-18=001: 19200
    /// Bit #16-18=010: 38400 
    /// Bit #16-18=011: 57600
    /// Bit #16-18=100: 115200
    /// Bit #16-18=101: 230400
    /// </summary>
    [DataMember(Order = 15)]
    public byte? BaudRate;

    /// <summary>
    /// Reader Operation Region:
    /// Bit 19-22: Reader Operating region
    /// Bit #19-22=0000: NA  (North America)
    /// Bit #19-22=0001: EU (Europe Union)
    /// Bit #19-22=0010: AU (Australia)
    /// Bit #19-22=0011: KR (Korea)
    /// Bit #19-22=0100: IN  (India)
    /// </summary>
    [DataMember(Order = 16)]
    public byte? ReaderOperationRegion;

    /// <summary>
    /// For Reporting Mode#3
    /// Pass on the native M5e RFID device fault codes one at the time:
    /// For Reporting Mode#5
    /// Pass on the native M6e RFID device fault codes one at the time:
    /// </summary>
    [DataMember(Order = 17)] 
    public uint? FaultCode;
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class RadioDeviceMachineSecurityEventBlockData : EventBlockData
  {
      /// <summary>
      /// 0x17
      /// </summary>
      [DataMember(Order = 0)]
      public byte BlockID { get; set; }

      /// <summary>
      /// 
      /// </summary>
      [DataMember(Order = 1)]
      public byte ByteCount { get; set; }

      /// <summary>
      /// 0x00= Normal Operation with Machine Security feature disabled (default)
      /// 0x01= Normal Operation with Machine Security enabled
      /// 0x02= Machine is in remote disable mode
      /// 0x03= Machine is in remote de-rate mode
      /// </summary>
      [DataMember(Order = 2)]
      public MachineSecurityModeSetting LatestMachineSecurityModeconfiguration { get; set; }

      /// <summary>
      /// 0x00= Normal Operation with Machine Security feature disabled (default)
      /// 0x01= Normal Operation with Machine Security enabled
      /// 0x02= Machine is in remote disable mode
      /// 0x03= Machine is in remote de-rate mode
      /// 0x04= Machine is in remote disable mode but security may be tampered or bypass
      /// 0x05= Machine is in remote disable mode but main power has been cut, the device will lost control of the relay output, the relay will be malfunction during the power cut. (Note: after the power is applied back to the device, normally, it will take ~17s for the device to regain control of the relay and re-activates the relay.
      /// </summary>    
      [DataMember(Order = 3)]
      public MachineSecurityModeSetting CurrentMachineSecurityModeconfiguration { get; set; }

      /// <summary>
      /// 0x00 = Off
      /// 0x01 = Tamper Resistance Level 1
      /// 0x02 = Tamper Resistance Level 2
      /// 0x03 = Tamper Resistance Level 3
      /// </summary>  
      [DataMember(Order = 4)]
      public TamperResistanceMode TamperResistanceMode { get; set; }

      /// <summary>
      /// 0x00 = Mode change request received, implementation pending
      /// 0x01 = Mode change request implemented
      /// 0x02 = Mode change rejected – remote Disable or De-rate or Tamper already enabled
      /// 0x03 = Mode request rejected – duplicate request
      /// 0x04 = Mode status report generated by the device due to the changes of state (may be due to lost of configuration in power cut, file read error or file corruption) 
      /// </summary>  
      [DataMember(Order = 5)]
      public DeviceSecurityModeReceivingStatus DeviceSecurityModeReceivingStatus { get; set; }

      /// <summary>
      /// 0x00= Security mode setting is using default setting in the firmware ( default)
      /// 0x01= Security mode setting is coming from VL back office
      /// 0x02= security mode setting is coming from Trimble service tool (not supported now)
      /// 0x03=security mode setting is coming from CAT ET (not supported now)
      /// </summary>
      [DataMember(Order = 6)]
      public SourceSecurityModeConfiguration SourceSecurityModeConfiguration { get; set; }
      
  }
  #endregion

  #region Event Block Data, Source == Gateway

  /// <summary>
  /// 0x51 ECM Information Message
  /// 
  /// <para>ECM Information Messages are infrequent and are sent 
  /// after installation and whenever there is a change to 
  /// the ECMs on the machine data links such as an ECM 
  /// software update, removal/addition of an ECM, etc.  The 
  /// ECM Information will send at Key Off if there is a 
  /// change in the information.</para>
  /// 
  /// <para> The purpose of this transaction is to provide VisionLink 
  /// with an updated list of ECMs installed on the machine.  
  /// This message informs the end user of detailed information 
  /// such as software part number for each particular ECM.  
  /// The information and rows for Device ID 2 are only 
  /// created and available if two data links are present.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (Most Significant Byte (MSB) first) unless otherwise 
  /// stated in the table.</para>
  /// </summary>
  [KnownType(typeof(string[]))]
  [XmlInclude(typeof(string[]))]
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMInformationGatewayEventBlockData : EventBlockData
  {

    private string[] _EngineSNs;
    private string[] _TransmissionSNs;

    /// <summary>
    /// 0x51
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x02
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x01
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Number of Engines detected on the data link.
    /// </summary>
    [DataMember(Order = 3)]
    public byte EngineCount { get; set; }

    /// <summary>
    /// Number of Transmissions detected on the data link.
    /// </summary>
    [DataMember(Order = 4)]
    public byte TransmissionCount { get; set; }

    /// <summary>
    /// Serial numbers of engines
    /// </summary>
    [DataMember(Order = 5, EmitDefaultValue = false, Name = "EngineSNs")]
    public string[] EngineSNs
    {
      get
      {
        if (_EngineSNs != null)
        {
          for (int i = 0; i <= _EngineSNs.Length - 1; i++)
          {
            _EngineSNs[i] = _EngineSNs[i].ToString(true);
          }
        }
        return _EngineSNs;
      }
      set
      {
        _EngineSNs = value;
      }
    }

    /// <summary>
    /// Serial numbers of transmissions 
    /// </summary>
    [DataMember(Order = 6, EmitDefaultValue = false)] 
    public string[] TransmissionSNs
    {
      get
      {
        if (_TransmissionSNs != null)
        {
          for (int i = 0; i <= _TransmissionSNs.Length - 1; i++)
          {
            _TransmissionSNs[i] = _TransmissionSNs[i].ToString(true);
          }
        }
        return _TransmissionSNs;
      }

      set 
      { _TransmissionSNs = value;  }
    }

     /// <summary>
    /// Number of ECMs included in transaction 
    /// </summary>
    [DataMember(Order = 7)]
    public byte BlockCount { get; set; }

    /// <summary>
    /// Information about ECMs
    /// </summary>
    [DataMember(Order = 8)]
    public ECMGatewayBlock[] Blocks { get; set; }
  }

  /// <summary>
  /// Exists in <seealso>
  ///             <cref>ECMInformationEventBlockData</cref>
  ///           </seealso>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMGatewayBlock
  {
    private string _softwarePartNumber;
    private string _serialNumber;
    private string _hardwarePartNumber;

    /// <summary>
    /// Only CDL
    /// Only J1939
    /// CDL and J1939    
    /// </summary>
    [DataMember(Order = 0)]
    public DataLinkType DataLinkType { get; set; }

    /// <summary>
    /// Acting Master ECM (Sync Clock)
    /// </summary>
    [DataMember(Order = 1)]
    public bool IsMasterECM { get; set; }

    /// <summary>
    /// Synchronized SMU Clock Strategy Supported
    /// </summary>
    [DataMember(Order = 2)]
    public bool CanSyncSMUClock { get; set; }

    /// <summary>
    /// Event Protocol Version
    /// </summary>
    [DataMember(Order = 3)]
    public byte EventVersion { get; set; }

    /// <summary>
    /// Diagnostic Protocol Version
    /// </summary>
    [DataMember(Order = 4)]
    public byte DiagnosticVersion { get; set; }

    /// <summary>
    /// CDL MID if both data links are present, LSB first 
    /// </summary>
    [DataMember(Order = 5)]
    public ushort MID1 { get; set; }

    /// <summary>
    /// Service Tool Support Change Level #1, LSB first  
    /// </summary>
    [DataMember(Order = 6)]
    public ushort ModuleSvcToolLevel1 { get; set; }

    /// <summary>
    /// Application Level #1, LSB First 
    /// </summary>
    [DataMember(Order = 7)]
    public ushort ModuleAppLevel1 { get; set; }

    /// <summary>
    /// J1939 MID if both data links are present, LSB First 
    /// </summary>
    [DataMember(Order = 8)]
    public ushort MID2 { get; set; }
    
    /// <summary>
    /// Service Tool Support Change Level, LSB first 
    /// </summary>
    [DataMember(Order = 9)]
    public ushort ModuleSvcToolLevel2 { get; set; }

    /// <summary>
    /// Application Level, LSB first 
    /// </summary>
    [DataMember(Order = 10)]
    public ushort ModuleAppLevel2 { get; set; }

    /// <summary>
    ///   Dynamic identifier for ach ECM on data link.
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 11)]
    public ushort SourceAddress { get; set; }

    /// <summary>
    ///   Modules software part number (ex. “2568986-00”).
    ///   Fill with #’s if parameter is unavailable for an ECM.
    /// </summary>
    [DataMember(Order = 12)]
    public string SoftwarePartNumber
    {
      get { return _softwarePartNumber.ToString(true); }
      set { _softwarePartNumber = value; }
    }

    /// <summary>
    ///   Modules ECM Serial Number (ex. “1914B009LQ”).
    ///   Fill with #’s if parameter is unavailable for an ECM.
    /// </summary>
    [DataMember(Order = 13)]
    public string SerialNumber
    {
      get { return _serialNumber.ToString(true); }
      set { _serialNumber = value; }
    }

    /// <summary>
    ///   Modules ECM Hardware Part Number (ex. “2347956-00”).
    ///   Fill with #’s if parameter is unavailable for an ECM.
    /// </summary>
    [DataMember(Order = 14)]
    public string HardwarePartNumber
    {
      get { return _hardwarePartNumber.ToString(true); }
      set { _hardwarePartNumber = value; }
    }

    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 15)]
    public bool ArbitraryAddressCapable { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 16)]
    public byte IndustryGroup { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 17)]
    public byte VehicleSysInstance { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 18)]
    public byte VehicleSystem { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 19)]
    public byte Function { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 20)]
    public byte FunctionInstance { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 21)]
    public byte ECUInstance { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 22)]
    public ushort ManufacturerCode { get; set; }
    /// <summary>
    /// Available only for Version2 
    ///   Included only if either public or proprietary J1939 is supported.
    /// </summary>
    [DataMember(Order = 23)]
    public int IDNumber { get; set; }
  }

  /// <summary>
  /// Unknown = 0,
  /// OnlyCDL = 1,
  /// OnlyJ1939 = 2,
  /// CDLAndJ1939 = 3,
  /// SAEJI939 = 4,
  /// SAEJI939AndCDL = 5,
  /// SAEJI939AndJ1939 = 6,
  /// All = 7
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum DataLinkType : byte
  {
    [EnumMember]
    Unknown = 0,

    [EnumMember]
    OnlyCDL = 1,

    [EnumMember]
    OnlyJ1939 = 2,

    [EnumMember]
    CDLAndJ1939 = 3,

    [EnumMember]
    SAEJI939 = 4,

    [EnumMember]
    SAEJI939AndCDL = 5,

    [EnumMember]
    SAEJI939AndJ1939 = 6,

    [EnumMember]
    All = 7
  }

  /// <summary>
  /// TT 0x53 Gateway Administration Message
  /// <para>Gateway Administration Message is an infrequent 
  /// transaction that occurs at installation time, and when 
  /// configuration parameters change.</para> 
  /// 
  /// <para>The purpose of the Gateway Administration Message 
  /// is to provide VisionLink with the updated configuration 
  /// information pertaining to the Product Link Embedded 
  /// Gateway.</para> 
  /// 
  /// <para>When a configuration fails, an Administration 
  /// Delivery Message will be sent with failure code and the 
  /// same sequence number reference as the OTA Configuration 
  /// Outbound Message.</para>
  /// 
  /// <para>The Engineering Administration Message (TT:53FE) 
  /// is available for Engineering purposes only and will not 
  /// be necessary for VisionLink application.  This message 
  /// will include parameters allowed for configuring and 
  /// troubleshooting the Embedded Gateway board.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof(DigitalInputsAdminEventBlockData))]
  [XmlInclude(typeof(DigitalInputsAdminEventBlockData))]
  [KnownType(typeof(MaintenanceAdminEventDataBlock))]
  [XmlInclude(typeof(MaintenanceAdminEventDataBlock))]
  [KnownType(typeof(TamperSecurityAdminEventDataBlock))]
  [XmlInclude(typeof(TamperSecurityAdminEventDataBlock))]
  [KnownType(typeof(EngineeringTamperTimersEventBlockData))]
  [XmlInclude(typeof(EngineeringTamperTimersEventBlockData))]
  [KnownType(typeof(EngineeringAdminEventBlockData))]
  [XmlInclude(typeof(EngineeringAdminEventBlockData))]
  [KnownType(typeof(AdminFailedDeliveryEventBlockData))]
  [XmlInclude(typeof(AdminFailedDeliveryEventBlockData))]
  public class GatewayAdminEventBlockData : EventBlockData { }

  /// <summary>
  /// TT 0x53 ST 0x00 Digital Inputs Administration Information
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0x00</para>
  /// <para>Version: 0x00</para>
  /// <para>Tech Note:  Configuration information for each 
  /// digital input will be included only if they have been 
  /// configured.  In other words, if digital inputs 2 and 4 
  /// have been configured and digital inputs 1 and 3 have 
  /// not been configured, then the registration message will 
  /// only contain digital input configuration information 
  /// for digital inputs 2 and 4.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DigitalInputsAdminEventBlockData : GatewayAdminEventBlockData
  {
    private string _SensorDescription1;
    private string _SensorDescription2;
    private string _SensorDescription3;
    private string _SensorDescription4;


    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// A counter to establish how many digital inputs are 
    /// configured for a particular registration.  Valid values 
    /// are 0 – 15 in which the bit denotes which sensors are 
    /// present. 
    /// 0 = no inputs, 1 = input #1, 2 = input #2, 3 = input #1 and #2, …
    /// </summary>
    [DataMember(Order = 3)]
    public byte Counter { get; set; }

    /// <summary>
    /// Not Installed
    /// Not configured
    /// Normally Open
    /// Normally Closed
    /// </summary>
    [DataMember(Order = 4)]
    public DigitalInputConfiguration? Config1 { get; set; }

    /// <summary>
    /// Debounce time in 100millisecond increments
    /// </summary>
    [DataMember(Order = 5)]
    public ushort? Delay1 { get; set; }

    /// <summary>
    /// All Conditions 
    /// Key On, Engine Off
    /// Key On, Engine On
    /// </summary>
    [DataMember(Order = 6)]
    public DigitalInputMonitoringCondition? MonitoringCondition1 { get; set; }

    /// <summary>
    /// Text Description of Digital Input #1
    /// </summary>
    [DataMember(Order = 7)]
    public string SensorDescription1
    {
      get { return _SensorDescription1.ToString(true); }
      set { _SensorDescription1 = value; }
    }

    [DataMember(Order = 8)]
    public DigitalInputConfiguration? Config2 { get; set; }
    
    [DataMember(Order = 9)]
    public ushort? Delay2 { get; set; }
    
    [DataMember(Order = 10)]
    public DigitalInputMonitoringCondition? MonitoringCondition2 { get; set; }
    
    [DataMember(Order = 11)]
    public string SensorDescription2
    {
      get { return _SensorDescription2.ToString(true); }
      set { _SensorDescription2 = value; }
    }

    [DataMember(Order = 12)]
    public DigitalInputConfiguration? Config3 { get; set; }

    [DataMember(Order = 13)]
    public ushort? Delay3 { get; set; }

    [DataMember(Order = 14)]
    public DigitalInputMonitoringCondition? MonitoringCondition3 { get; set; }

    [DataMember(Order = 15)]
    public string SensorDescription3
    {
      get { return _SensorDescription3.ToString(true); }
      set { _SensorDescription3 = value; }
    }

    [DataMember(Order = 16)]
    public DigitalInputConfiguration? Config4 { get; set; }

    [DataMember(Order = 17)]
    public ushort? Delay4 { get; set; }

    [DataMember(Order = 18)]
    public DigitalInputMonitoringCondition? MonitoringCondition4 { get; set; }

    [DataMember(Order = 19)]
    public string SensorDescription4
    {
      get { return _SensorDescription4.ToString(true); }
      set { _SensorDescription4 = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum DigitalInputMonitoringCondition
  {
    [EnumMember]
    AllConditions = 0x028C,

    [EnumMember]
    KeyOnEngineOff = 0x028E,

    [EnumMember]
    KeyOnEngineOn = 0x028F,
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum DigitalInputConfiguration
  {
    [EnumMember]
    NotInstalled = 0x11,

    [EnumMember]
    NotConfigured = 0x2C,

    [EnumMember]
    NormallyOpen = 0x57,

    [EnumMember]
    NormallyClosed = 0x58,
  }

  /// <summary>
  /// TT 0x53 ST 0x01 Maintenance Administration Information
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0x01</para>
  /// <para>Version: 0x00</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class MaintenanceAdminEventDataBlock : GatewayAdminEventBlockData
  {
    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x01
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    [DataMember(Order = 3)]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Duration to keep Maintenance Mode enabled, after 
    /// which it will automatically be disabled. Units in hours.
    /// </summary>
    [DataMember(Order = 4)]
    public byte Duration { get; set; }
  }

  /// <summary>
  /// TT 0x53 ST 0x02 Tamper Security Administration Information
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0x02</para>
  /// <para>Version: 0x00</para>
  /// <para>This message will be sent any time the Machine 
  /// Start Mode or Tamper Resistance Mode is configured 
  /// by a user.  When the machine’s actual start status 
  /// changes (at a key switch on), the Gateway will send 
  /// a TT 0x4606 Tamper Security Status Report with the 
  /// trigger.</para>
  /// 
  /// <para>Tech Note:  MSS Key Security could also be 
  /// utilized on the Machine as well.  Machine Security 
  /// Mode will help Off-Board determine if the dealer is 
  /// using Key Security in parallel with Tamper and if any 
  /// other issues arise during install/uninstall process.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TamperSecurityAdminEventDataBlock : GatewayAdminEventBlockData
  {
    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x01
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// 0x00 = Normal Operation
    /// 0x01 = Derated
    /// 0x02 = Disabled
    /// </summary>    
    [DataMember(Order = 3)]
    public MachineStartMode MachineStartMode { get; set; }

    /// <summary>
    /// 0x00 = Never Configured
    /// 0x01 = Off-Board Office System (VL)
    /// 0x02 = Cat Electronic Technician
    /// </summary>  
    [DataMember(Order = 4)]
    public MachineStartModeConfigSource MachineStartModeConfigSource { get; set; }

    /// <summary>
    /// 0x00 = Off
    /// 0x01 = Tamper Resistance Level 1
    /// 0x02 = Tamper Resistance Level 2
    /// 0x03 = Tamper Resistance Level 3
    /// </summary>  
    [DataMember(Order = 5)]
    public TamperResistanceMode TamperResistanceMode { get; set; }

    /// <summary>
    /// 0x00 = Never Configured
    /// 0x01 = Off-Board Office System (VL)
    /// 0x02 = Cat Electronic Technician
    /// </summary>
    [DataMember(Order = 6)]
    public TamperResistanceModeConfigSource TamperResistanceModeConfigSource { get; set; }

    /// <summary>
    /// 0x00 = Not Installed
    /// 0x01 = Installed (Tamper Resistance Only)
    /// 0x02 = Installed (Tamper Resistance and MSS Key)
    /// 0x03 = Installed (MSS Key Only)
    /// 0x04 = Not Installed – Another MSS Master on Data Link
    /// 0x05 = Installed – Immobilizer could not be uninstalled
    /// 0x06 = Not Installed – Only Legacy Immobilizer detected
    /// 0x07 = Not Installed – Immobilizer Restriction with Power Down Required
    /// 0xFF = Previous State – Unknown Error
    /// </summary>
    [DataMember(Order = 7)]
    public MachineSecurityMode MachineSecurityMode { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineSecurityMode
  {
    [EnumMember]
    NotInstalled = 0x00,

    [EnumMember]
    TamperResistanceOnly = 0x01,

    [EnumMember]
    TamperResistanceAndMSSKey = 0x02,

    [EnumMember]
    MSSKeyOnly = 0x03,

    [EnumMember]
    NotInstalledAnotherMSSMasterOnDataLink = 0x04,

    [EnumMember]
    ImmobilizerCouldNotBeUninstalled = 0x05,

    [EnumMember]
    NotInstalledOnlyLegacyImmobilizerDetected = 0x06,

    [EnumMember]
    NotInstalledImmobilizerRestrictionWithPowerDownRequired = 0x07,

    [EnumMember]
    PreviousStateUnknownError = 0xFF
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum TamperResistanceModeConfigSource
  {
    [EnumMember]
    NeverConfigured = 0x00,

    [EnumMember]
    OffBoardOfficeSystemVl = 0x01,

    [EnumMember]
    CatElectronicTechnician = 0x02    
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum TamperResistanceMode
  {
    [EnumMember]
    Invalid = -0x01,

    [EnumMember]
    Off = 0x00,

    [EnumMember]
    Level1 = 0x01,

    [EnumMember]
    Level2 = 0x02,

    [EnumMember]
    Level3 = 0x03
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineStartModeConfigSource
  {
    [EnumMember]
    NeverConfigured = 0x00,

    [EnumMember]
    OffBoardOfficeSystemVl = 0x01,

    [EnumMember]
    CatElectronicTechnician = 0x02  
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineStartMode
  {
    [EnumMember]
    NormalOperation = 0x00,

    [EnumMember]
    Derated = 0x01,

    [EnumMember]
    Disabled = 0x02,
    // Possible Future States

    [EnumMember]
    NormalOperationPending = 0x10,

    [EnumMember]
    DeratedPending = 0x11,

    [EnumMember]
    DisabledPending = 0x12
  }

  /// <summary>
  /// TT 0x53 ST 0xFD Engineering Tamper Timers Administration Information
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0xFD</para>
  /// <para>Version: 0x00</para>
  /// Engineering purposes only
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class EngineeringTamperTimersEventBlockData : GatewayAdminEventBlockData
  {
    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0xFD
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Timer counter since GPS antenna has been removed (seconds)
    /// </summary>
    [DataMember(Order = 3)]
    public uint GPSAntennaDisconnectionTimerSeconds { get; set; }

    /// <summary>
    /// Timer counter since receiving either a GPS fix or 
    /// connection to the VisionLink Off-Board (seconds)
    /// </summary>
    [DataMember(Order = 4)]
    public uint GSMGPSSignalQualityTimerSeconds { get; set; }

    /// <summary>
    /// Timer counter since connecting to the VisionLink Off-Board (seconds)
    /// </summary>
    [DataMember(Order = 5)]
    public uint GSMSignalQualityTimerSeconds { get; set; }

    /// <summary>
    /// Timer counter since SIM Card has been removed (seconds)
    /// </summary>
    [DataMember(Order = 6)]
    public uint GSMSIMCardRemovalTimeSeconds { get; set; }
  }

  /// <summary>
  /// TT 0x53 ST 0xFE Engineering Administration Information
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0xFE</para>
  /// <para>Version: 0x00</para>
  /// **Engineering purposes only**
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class EngineeringAdminEventBlockData : GatewayAdminEventBlockData
  {
    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0xFE
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Number of service meter hours to wait before transmitting 
    /// same events/diagnostics fault code (0-65535 hours)
    /// </summary>
    [DataMember(Order = 3)]
    public ushort EventInterval { get; set; }
  }

  /// <summary>
  /// TT 0x53 ST 0xFF Administration Failed Delivery Message
  /// Type: 0x53 <seealso cref="GatewayAdminEventBlockData"/>
  /// <para>SubType: 0xFF</para>
  /// <para>Version: 0x00</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class AdminFailedDeliveryEventBlockData : GatewayAdminEventBlockData
  {
    /// <summary>
    /// 0x53
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0xFE
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Message Sequence number provided in the OTA Configuration
    /// </summary>
    [DataMember(Order = 3)]
    public byte MessageSequenceNumber { get; set; }

    /// <summary>
    /// Reason for not updating Gateway configuration parameters
    /// 0x01 = Unrecognizable parameter
    /// 0x02 = Incorrect parameter value
    /// </summary>
    [DataMember(Order = 4)]
    public DeliveryFailureReason DeliveryFailureReason { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum DeliveryFailureReason : byte
  {
    [EnumMember]
    UnrecognizableParameter = 0x01,

    [EnumMember]
    IncorrectParameterValue = 0x02
  }

  /// <summary> 
  /// TT 0x21 (Fault Code) Event Reporting
  /// <para>Event fault codes are sent immediately when 
  /// triggered.  When an event fault code is triggered, the 
  /// Embedded Gateway will begin a 1 hour timer (event interval) 
  /// for that particular code to prevent sending the same 
  /// fault code identifier if triggered again.  After the 
  /// 1 hour timer, the Embedded Gateway will send another 
  /// update to the Off-Board if the fault code is triggered.  
  /// The event interval is configurable for engineering 
  /// purposes and utilizes the TT:1102 Gateway Configuration 
  /// Message.</para>
  /// 
  /// <para>The purpose of this transaction is simply to 
  /// deliver ECM event data to VisionLink. Each event is 
  /// represented by five pieces of data: the event level, 
  /// the module ID (MID) reporting the event, the event 
  /// identifier, and the event occurrence count.  This 
  /// transaction includes two-byte MID to allow flexibility 
  /// with machines containing EDDT.</para>
  /// 
  /// <para>There is one event per diagnostic message (Transaction 
  /// Type 0x2100).</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class FaultCodeReportEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x21
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x01
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// The Event Level (0, 1, 2, 3)
    /// </summary>
    [DataMember(Order = 3)]
    public byte EventLevel { get; set; }

    /// <summary>
    /// The Electronic Control Module (ECM) Identifier from 
    /// which the event was generated
    /// </summary>
    [DataMember(Order = 4)]
    public ushort EventMID { get; set; }

    /// <summary>
    /// The Event Identifier (EID)
    /// </summary>
    [DataMember(Order = 5)]
    public ushort EventCode { get; set; }

    /// <summary>
    /// The number of times the event has occurred since last ECM reset. 
    /// </summary>
    [DataMember(Order = 6)]
    public byte EventOccuranceCount { get; set; }
  }

  /// <summary>
  /// TT 0x22 Diagnostic Reporting
  /// <para>Diagnostic fault codes are sent immediately when 
  /// triggered.  When a diagnostic fault code is triggered, 
  /// the Embedded Gateway will begin a 1 hour timer 
  /// (event interval) for that particular code to prevent 
  /// sending the same fault code identifier if triggered 
  /// again.  After the 1 hour timer, the Embedded Gateway 
  /// will send another update to the Off-Board if the fault 
  /// code is triggered.  The event interval is configurable 
  /// for engineering purposes and utilizes the TT:1102 
  /// Gateway Configuration Message.</para>
  /// 
  /// <para>Each diagnostic is represented by six pieces of 
  /// data: the diagnostic level, the module ID (MID) 
  /// reporting the diagnostic, the diagnostic identifier, 
  /// the diagnostic failure mode, and the diagnostic occurrence 
  /// count.  This transaction includes two-byte MID to allow 
  /// flexibility with machines containing EDDT.</para>
  /// 
  /// <para>There is one diagnostic per diagnostic message 
  /// (Transaction Type 0x2200).</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DiagnosticReportGatewayEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x22
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x01
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// The Diagnostic Level (0, 1, 2, 3)
    /// </summary>
    [DataMember(Order = 3)]
    public byte DiagnosticLevel { get; set; }

    /// <summary>
    /// The Electronic Control Module (ECM) Identifier from which 
    /// the Diagnostic was generated
    /// </summary>
    [DataMember(Order = 4)]
    public ushort DiagnosticMID { get; set; }

    /// <summary>
    /// The Diagnostic Component Identifier (CID)
    /// </summary>
    [DataMember(Order = 5)]
    public ushort DiagnosticPrimaryCode { get; set; }

    /// <summary>
    /// The Diagnostic Failure Mode Identifier (FMI)
    /// </summary>
    [DataMember(Order = 6)]
    public byte DiagnosticSecondaryCode { get; set; }

    /// <summary>
    /// The number of times the Diagnostic has occurred since last ECM reset. 
    /// </summary>
    [DataMember(Order = 7)]
    public byte DiagnosticOccurancesCount { get; set; }
  }
  
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class J1939DiagnosticReportGatewayEventBlockData : EventBlockData
  {
    /// <summary>
    ///   0x23
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    ///   0x01
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    ///   0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    ///   The DataLink Type
    /// </summary>
    [DataMember(Order = 3)]
    public byte DataLinkType { get; set; }

    /// <summary>
    ///   The Electronic Control Module (ECM) Identifier from which
    ///   the Diagnostic was generated
    /// </summary>
    [DataMember(Order = 4)]
    public ushort ECMIdentifier { get; set; }

    /// <summary>
    ///   The Failure Mode Identifier (FMI)
    /// </summary>
    [DataMember(Order = 5)]
    public byte FailureModeIdentifier { get; set; }

    /// <summary>
    ///   The number of times the Diagnostic has occurred since last ECM reset.
    /// </summary>
    [DataMember(Order = 6)]
    public byte DiagnosticOccurancesCount { get; set; }

    /// <summary>
    ///   The Significant Part Number (SPN)
    /// </summary>
    [DataMember(Order = 7)]
    public int SignificantPartNumber { get; set; }

    /// <summary>
    ///   Protect Lamp Status
    /// </summary>
    [DataMember(Order = 8)]
    public bool ProtectLampStatus { get; set; }

    /// <summary>
    ///   Amber Warning Lamp Status
    /// </summary>
    [DataMember(Order = 9)]
    public bool AmberWarningLampStatus { get; set; }

    /// <summary>
    ///   Red Stop Lamp Status
    /// </summary>
    [DataMember(Order = 10)]
    public bool RedStopLampStatus { get; set; }

    /// <summary>
    ///   Malfunction Indicator LampStatus
    /// </summary>
    [DataMember(Order = 11)]
    public bool MalfunctionIndicatorLampStatus { get; set; }
  }

  /// <summary>
  /// TT 0x45 Fuel/Engine and Payload/Cycle Count Report
  /// <para>The purpose of this transaction is to provide 
  /// VisionLink a status update of what is occurring on the 
  /// machine.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof (FuelEngineReportGatewayEventBlockData))]
  [XmlInclude(typeof (FuelEngineReportGatewayEventBlockData))]
  [KnownType(typeof (PayloadAndCycleCountReportGatewayEventBlockData))]
  [XmlInclude(typeof (PayloadAndCycleCountReportGatewayEventBlockData))]
  public class FuelEnginePayloadCycleCountGatewayEventBlockData : EventBlockData {}

  // Note: There are 2 versions of this, 
  // in V1 0 notes an invalid value, 
  // in V2 MaxValue is the invalid value.
  // in V3 the element wil not be there  if it is invalid value
  /// <summary>
  /// TT 0x4500 Fuel/Engine Reporting
  /// <para>The purpose of this transaction is to deliver the 
  /// machine’s daily totals involving fuel and engine parameters.  
  /// This message supports multiple engines for all parameters 
  /// linked with Module Identifications.  If a MID contains 
  /// some parameters as a zero value, Off-Board Office should 
  /// assume this parameters as ‘Not Available’.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para> 
  /// 
  /// <para>Version 01 of this message utilized the value of 
  /// zero for unavailable.</para>
  /// 
  /// <para>Version 02 (V2) of this message will use max values 
  /// (all 0xFF’s) for unavailable since it is more likely 
  /// to have a zero value than the max value.</para>
  /// <para>Version 03 (V3) of this message will not have any value
  /// if the value is  unavailable since both Max value and zero are  
  /// valid values.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class FuelEngineReportGatewayEventBlockData : FuelEnginePayloadCycleCountGatewayEventBlockData
  {
    /// <summary>
    /// 0x45
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x01 or 0x02 or 0X03
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Number of ECMs reporting
    /// </summary>
    [DataMember(Order = 3)]
    public byte ECMCount { get; set; }

    /// <summary>
    /// Information about Fuel from ECMs version 1 & 2
    /// </summary>
    [DataMember(Order = 4)]
    public FuelEngineReportECM[] Blocks { get; set; }  
  }

  /// <summary>
  ///   Exists in <seealso cref="FuelEngineReportGatewayEventBlockData" />
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class FuelEngineReportECM
  {
    /// <summary>
    ///   The Electronic Control Module (ECM) Identifier
    /// </summary>
    [DataMember(Order = 0)]
    public ushort ECMID { get; set; }

    /// <summary>
    ///   Total fuel consumption (in 1/8 gallons).
    ///   V2, V3: Range: 0 to 536,870,907 gallons
    /// </summary>
    [DataMember(Order = 1)]
    public uint? FuelConsumptionTotal { get; set; }

    /// <summary>
    ///   Percentage of equipment’s fuel remaining
    ///   V2, V3: Range: 0 to 100%
    /// </summary>
    [DataMember(Order = 2)]
    public byte? FuelRemainingPercent { get; set; }

    /// <summary>
    ///   Total amount of engine idle time (0.05 hours/bit)
    ///   V2, V3: Range: 0 to 214,748,363 hours
    /// </summary>
    [DataMember(Order = 3)]
    public uint? EngineIdleTimeTotal { get; set; }

    /// <summary>
    ///   Total maximum fuel (in 1/8 gallons).
    ///   V2, V3: Range:  0 to 536,870,907 gallons
    /// </summary>
    [DataMember(Order = 4)]
    public uint? MaximumFuelTotal { get; set; }

    /// <summary>
    ///   Total number of engine starts
    ///   V2, V3: Range: 0 to 65535 starts
    /// </summary>
    [DataMember(Order = 5)]
    public ushort? EngineStartsTotal { get; set; }

    /// <summary>
    ///   Total number of engine revolutions (4 revolutions/bit)
    ///   V2, V3: Range: 0 to 1.717987E10 revolutions
    /// </summary>
    [DataMember(Order = 6)]
    public uint? EngineRevolutionsTotal { get; set; }

    /// <summary>
    ///   Total fuel used at idle time (in 1/8 gallons)
    ///   V2, V3: Range: 0 to 536,870,907 gallons
    /// </summary>
    [DataMember(Order = 7)]
    public uint? IdleFuelTotal { get; set; }

    /// <summary>
    ///   Total amount of machine idle time (0.05 hours bit)
    ///   V2, V3: Range: 0 to 214,748,363 hours
    /// </summary>
    [DataMember(Order = 8)]
    public uint? MachineIdleTimeTotal { get; set; }

    /// <summary>
    ///   Total fuel used at machine idle time (in 1/8 gallons)
    ///   Note *This field might send zero value until a later version of software
    /// </summary>
    [DataMember(Order = 9)]
    public uint? MachineIdleFuelTotal { get; set; }

    /// <summary>
    /// Total Fuel consumed after the machine stops running (in 1/8 gallons)
    /// Only for V3: Range: 0 to 214,748,363 hours
    /// </summary>
    [DataMember(Order = 10)]
    public uint? AfterTreatment1HistoricalInfo { get; set; }

    /// <summary>
    ///  Diesel Exhaust in 0.4% per bit
    /// Only for V3
    /// </summary>
    [DataMember(Order = 11)]
    public byte? AfterTreatment1DieselExhaustFluidTankInfo { get; set; }
  }    

  /// <summary>
  /// TT 0x4503 Payload and Cycle Counts Reporting
  /// <para>The purpose of this transaction is to deliver the 
  /// machine’s totals involving lifetime payload weight
  /// and total machine operation cycle count.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian.</para> 
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PayloadAndCycleCountReportGatewayEventBlockData : FuelEnginePayloadCycleCountGatewayEventBlockData
  {
    /// <summary>
    /// 0x45
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x03
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Number of ECMs reporting
    /// </summary>
    [DataMember(Order = 3)]
    public byte ECMCount { get; set; }

    /// <summary>
    /// Information about Fuel from ECMs
    /// </summary>
    [DataMember(Order = 4)]
    public PayloadAndCycleCountReportECM[] Blocks { get; set; }
  }

  /// <summary>
  /// Exists in <seealso cref="PayloadAndCycleCountReportGatewayEventBlockData"/>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PayloadAndCycleCountReportECM
  {
    /// <summary>
    /// The Electronic Control Module (ECM) Identifier 
    /// </summary>
    [DataMember(Order = 0)]
    public ushort ECMID { get; set; }

    /// <summary>
    /// Lifetime Total Payload Weight (1.0 tonne/bit).
    /// </summary>
    [DataMember(Order = 1)]
    public uint TotalPayload { get; set; }

    /// <summary>
    /// Total Machine Operation Cycle Count (1 cycle/bit).
    /// </summary>
    [DataMember(Order = 2)]
    public uint TotalCycles { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof(TMSInformationMessageEventBlockdata))]
  [XmlInclude(typeof(TMSInformationMessageEventBlockdata))]
  [KnownType(typeof(TMSReportMessageEventBlockData))]
  [XmlInclude(typeof(TMSReportMessageEventBlockData))]
  public class TMSInfoAndReportMessageBlockData : EventBlockData
  {
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TMSInformationMessageEventBlockdata : TMSInfoAndReportMessageBlockData
  {
    /// <summary>
    ///   0x44
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    ///   0x02  
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    ///   0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }
    /// <summary>
    ///  16 - Installed
    ///   17 - Not Installed
    /// </summary>
    [DataMember(Order = 3)]
    public ushort TMSInstallationStatus { get; set; }

    /// <summary>
    /// MID value
    /// </summary>
    [DataMember(Order = 4)]
    public ushort MID { get; set; }
    /// <summary>
    /// No. Of Tire Info Message to follow
    /// </summary>
    [DataMember(Order = 5)]
    public ushort RecordsCount { get; set; }

    /// <summary>
    /// Information about the Tire
    /// </summary>
    [DataMember(Order = 6)]
    public TireInfoData[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TireInfoData
  {
    private string _sensorID;

    [DataMember(Order = 0)]
    public byte AxlePosition { get; set; }

    [DataMember(Order = 1)]
    public byte TirePosition { get; set; }

    [DataMember(Order = 2)]
    public string SensorID
    {
      get { return _sensorID.ToString(true); }
      set { _sensorID = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TMSReportMessageEventBlockData : TMSInfoAndReportMessageBlockData
  {
    /// <summary>
    ///   0x44
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    ///   0x01  
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    ///   0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }
    /// <summary>
    /// No. Of Tire Info Message to follow
    /// </summary>
    [DataMember(Order = 5)]
    public ushort RecordsCount { get; set; }

    /// <summary>
    /// Information about the Tire
    /// </summary>
    [DataMember(Order = 6)]
    public TireReportData[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TireReportData
  {
    [DataMember(Order = 0)]
    public byte AxlePosition { get; set; }

    [DataMember(Order = 1)]
    public byte TirePosition { get; set; }

    /// <summary>
    /// Pressure in 0.1 KPa, Invalid value -4294967295
    /// </summary>
    [DataMember(Order = 2)]
    public ushort Pressure { get; set; }
    /// <summary>
    /// Valid values 0 and 65535
    /// </summary>
    [DataMember(Order = 3)]
    public ushort TemperatureIndicator { get; set; }
    /// <summary>
    /// In Degree Celsius, Range -32736 to +32767
    /// Value is valid when TemperatureIndicator is 0, else invalid
    /// </summary>
    [DataMember(Order = 4)]
    public short Temperature { get; set; }
    /// <summary>
    /// 0x0000 = No alerts
    ///Bit 0 = Over Pressure Level 1
    ///Bit 1 = Over Pressure Level 2
    ///Bit 2 = Under Pressure Level 1
    ///Bit 3 = Under Pressure Level 2
    ///Bit 4 = Over Temperature Level 1
    ///Bit 5 = Over Temperature Level 2
    ///Bit 6 = Low sensor battery
    ///Bit 7 = Sensor Not Communicating
    ///0xFFFF = Tire status unavailable
    /// </summary>
    [DataMember(Order = 4)]
    public ushort AlertStatus { get; set; }
    /// <summary>
    ///  16 - Installed
    ///   17 - Not Installed
    /// </summary>
    [DataMember(Order = 5)]
    public ushort SensorInstallationStatus { get; set; }
  }

  /// <summary>
  /// TT 0x46 Machine Activity Events
  /// <para>If configured to send, these events will be sent 
  /// for every transition from Start/Stop, Stop/Start.</para>
  /// 
  /// <para>The purpose of this transaction is to provide 
  /// VisionLink a status update of what is occurring on the 
  /// machine.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  [KnownType(typeof(MSSKeyIDReportEventBlockData))]
  [XmlInclude(typeof(MSSKeyIDReportEventBlockData))]
  [KnownType(typeof(TamperSecurityStatusReportEventBlockData))]
  [XmlInclude(typeof(TamperSecurityStatusReportEventBlockData))]
  public class MachineActivityEventBlockData : EventBlockData { }

  /// <summary>
  /// TT 0x4605 MSS Key ID Report
  /// <para>Type: 0x46 <seealso cref="MachineActivityEventBlockData"/></para>
  /// <para>SubType: 0x05</para>
  /// <para>Version: 0x00</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class MSSKeyIDReportEventBlockData : MachineActivityEventBlockData
  {
    /// <summary>
    /// 0x46
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x05
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// Operator MSS Key ID
    /// </summary>
    [DataMember(Order = 3)]
    public long MSSKeyID { get; set; }
  }

  /// <summary>
  /// TT 0x4606 Tamper Security Status Report
  /// <para>Type: 0x46 <seealso cref="MachineActivityEventBlockData"/></para>
  /// <para>SubType: 0x06</para>
  /// <para>Version: 0x00</para>
  /// <para>Provides an update every time the Machine Start 
  /// Status changes and what caused the change.  This is to 
  /// inform the Off-Board the actual current state of the 
  /// machine and not just the configuration.  Updates to 
  /// configuration will be handled by TT 0x5302 Tamper Security 
  /// Administration Information.  There is a 30 second debounce 
  /// between Key Off and Key On for a transition between 
  /// Machine Start Status (for example, Normal Operation will 
  /// not transition to Derated if Key is turned On after 15 
  /// seconds of the Key Off).</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TamperSecurityStatusReportEventBlockData : MachineActivityEventBlockData
  {
    /// <summary>
    /// 0x46
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x06
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// 0x00 = Normal Operation
    /// 0x01 = Derated
    /// 0x02 = Disabled
    /// Possible future states
    /// 0x10 = Normal Operation Pending
    /// 0x11 = Derated Pending
    /// 0x12 = Disabled Pending
    /// </summary>
    [DataMember(Order = 3)]
    public MachineStartMode MachineStartStatus { get; set; }

    /// <summary>
    /// 0x00 = Unknown/Not Configured/Not Triggered
    /// 0x01 = OTA command
    /// 0x02 = On-Board Service Tool command
    /// 0x03 = SIM Card Removed Timer Expired
    /// 0x04 = GPS Antenna Disconnected Timer Expired
    /// 0x05 = GPS/GSM Loss of Signal Timer Expired
    /// 0x06 = GSM Loss of Signal Timer Expired
    /// 0x07 = Invalid MSS Key
    /// 0x08 = Tamper Resolved - Connection to Off-Board
    /// 0x09 = Tamper Uninstalled
    /// 0x0A = Valid MSS Key
    /// </summary>
    [DataMember(Order = 4)]
    public MachineStartStatusTrigger MachineStartStatusTrigger { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineStartStatusTrigger
  {
    [EnumMember]
    Unknown = 0x00,
    [EnumMember]
    OTACommand = 0x01,
    [EnumMember]
    OnBoardServiceToolCommand = 0x02,
    [EnumMember]
    SIMCardRemovedTimerExpired = 0x03,
    [EnumMember]
    GPSAntennaDisconnectedTimerExpired = 0x04,
    [EnumMember]
    GPSGSMLossofSignalTimerExpired = 0x05,
    [EnumMember]
    GSMLossofSignalTimerExpired = 0x06,
    [EnumMember]
    InvalidMSSKey = 0x07,
    [EnumMember]
    TamperResolvedConnectiontoOffBoard = 0x08,
    [EnumMember]
    TamperUninstalled = 0x09,
    [EnumMember]
    ValidMSSKey = 0x0A
  }

  /// <summary>
  /// TT 0x3A SMH Adjustment Message
  /// <para>The Service Meter Adjustment transaction is only 
  /// sent when the user changes the SMU on the device via 
  /// the service tool or a back-office configuration command.</para>
  /// 
  /// <para>The purpose of this transaction is simple – it 
  /// contains the old SMH that Product Link is tracking 
  /// presently and it contains the new SMU that Product Link 
  /// will start tracking at the time the message is generated. 
  /// This data is needed by VisionLink to accurately track 
  /// equipment usage.  OTA Adjustments should only occur if 
  /// there are no Sync Clock ECMs on the machine.</para>
  /// 
  /// <para>Byte Order:  All multiple byte data is Big Endian 
  /// (MSB first) unless otherwise stated in the table.</para>
  /// </summary>
  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class SMHAdjustmentEventBlockData : EventBlockData
  {
    /// <summary>
    /// 0x3A
    /// </summary>
    [DataMember(Order = 0)]
    public byte Type { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 1)]
    public byte SubType { get; set; }

    /// <summary>
    /// 0x00
    /// </summary>
    [DataMember(Order = 2)]
    public byte Version { get; set; }

    /// <summary>
    /// The Service Meter Hour reading before the alteration (0.1 hours/bit)
    /// </summary>
    [DataMember(Order = 3)]
    public uint SMHBefore { get; set; }

    /// <summary>
    /// The Service Meter Hour reading after the alteration (0.1 hours/bit)
    /// </summary>
    [DataMember(Order = 4)]
    public uint SMHAfter { get; set; }
  }

  #endregion

  #region Event Block Data, Source == VehicleBus

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class VehicleBusBinaryEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public VehicleBusProtocol? Protocol { get; set; }

    [DataMember(Order = 1)]
    public byte[] VehicalBusBinary { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMAddressClaimEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 3)]
    public ECMAddressClaim[] ECMAddressClaims { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMAddressClaim
  {
    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 2)]
    public bool ArbitraryAddressCapable { get; set; }

    [DataMember(Order = 3)]
    public byte IndustryGroup { get; set; }

    [DataMember(Order = 4)]
    public byte VehicleSysInstance { get; set; }

    [DataMember(Order = 5)]
    public byte VehicleSys { get; set; }

    [DataMember(Order = 6)]
    public byte Function { get; set; }

    [DataMember(Order = 7)]
    public byte FunctionInstance { get; set; }

    [DataMember(Order = 8)]
    public byte ECUInstance { get; set; }

    [DataMember(Order = 9)]
    public ushort ManufacturerCode { get; set; }

    [DataMember(Order = 10)]
    public int IDNumber { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMInformationVehicleBusEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public byte EngineCount { get; set; }

    [DataMember(Order = 3)]
    public byte TransmissionCount { get; set; }

    [DataMember(Order = 4)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 5)]
    public ECMVehicleBusBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMVehicleBusBlock
  {
    private string _SoftwarePartNumber;
    private string _SoftwareDescription;
    private string _SoftwareReleaseDate;
    private string _PartNumber;
    private string _SerialNumber;


    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 2)]
    public string SoftwarePartNumber
    {
      get { return _SoftwarePartNumber.ToString(true); }
      set { _SoftwarePartNumber = value; }
    }

    [DataMember(Order = 3)]
    public string SoftwareDescription
    {
      get { return _SoftwareDescription.ToString(true); }
      set { _SoftwareDescription = value; }
    }

    [DataMember(Order = 4)]
    public string SoftwareReleaseDate
    {
      get { return _SoftwareReleaseDate.ToString(true); }
      set { _SoftwareReleaseDate = value; }
    }

    [DataMember(Order = 5)]
    public string PartNumber
    {
      get { return _PartNumber.ToString(true); }
      set { _PartNumber = value; }
    }

    [DataMember(Order = 6)]
    public string SerialNumber
    {
      get { return _SerialNumber.ToString(true); }
      set { _SerialNumber = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class FuelEngineReportVehicleBusEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 3)]
    public ECMFuelEngineReportBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ECMFuelEngineReportBlock
  {
    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 2)]
    public uint FuelConsumptionTotal { get; set; }

    [DataMember(Order = 3)]
    public byte FuelLevelPercentage { get; set; }

    [DataMember(Order = 4)]
    public uint EngineIdleTimeTotal { get; set; }

    [DataMember(Order = 5)]
    public ushort EngineStartsTotal { get; set; }

    [DataMember(Order = 6)]
    public uint EngineRevolutionsTotal { get; set; }

    [DataMember(Order = 7)]
    public uint IdleFuelTotal { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DiagnosticReportVehicleBusEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 3)]
    public DiagnosticReportBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class DiagnosticReportBlock
  {
    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 2)]
    public byte ProtectLampStatus { get; set; }

    [DataMember(Order = 3)]
    public byte AmberWarningLampStatus { get; set; }

    [DataMember(Order = 4)]
    public byte RedStopLampStatus { get; set; }

    [DataMember(Order = 5)]
    public byte MalfunctionLampStatus { get; set; }

    [DataMember(Order = 6)]
    public uint SuspectParameterNumber { get; set; }

    [DataMember(Order = 7)]
    public byte FailureModeID { get; set; }

    [DataMember(Order = 8)]
    public byte OccurencesCount { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TPMSReportVehicleBusEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }

    [DataMember(Order = 2)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 3)]
    public TPMSReportBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TPMSReportBlock
  {
    private string _ecmDescription;

    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte ECMSourceAddress { get; set; }

    [DataMember(Order = 2)]
    public byte ECMDescriptionLength { get; set; }

    [DataMember(Order = 3)]
    public string ECMDescription
    {
      get { return _ecmDescription.ToString(true); }
      set { _ecmDescription = value; }
    }

    [DataMember(Order = 4)]
    public byte TireCount { get; set; }

    [DataMember(Order = 5)]
    public TPMSTireReportBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class TPMSTireReportBlock
  {
    [DataMember(Order = 0)]
    public int TirePosition { get; set; }

    [DataMember(Order = 1)]
    public int AxlePosition { get; set; }

    [DataMember(Order = 2)]
    public uint TirePressure { get; set; }

    [DataMember(Order = 3)]
    public uint TireTemperature { get; set; }

    [DataMember(Order = 4)]
    public ushort AlertStatus { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PayloadReportVehicleBusEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }

    [DataMember(Order = 2)]
    public byte ECMCount { get; set; }

    [DataMember(Order = 3)]
    public PayloadReportBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PayloadReportBlock
  {
    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte ECMSourceAddress { get; set; }

    [DataMember(Order = 2)]
    public uint TotalPayload { get; set; }

    [DataMember(Order = 3)]
    public uint TotalCycleCount { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class J1939ParamsReportEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public J1939Report J1939Report { get; set; }

    [DataMember(Order = 3)]
    public ushort BlockCount { get; set; }

    [DataMember(Order = 4)]
    public J1939ParamBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class J1939ParamBlock
  {
    private string _Payload;

    [DataMember(Order = 0)]
    public byte CANBusInstance { get; set; }

    [DataMember(Order = 1)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 2)]
    public byte ProtectLampStatus { get; set; }

    [DataMember(Order = 3)]
    public byte AmberWarningLampStatus { get; set; }

    [DataMember(Order = 4)]
    public byte RedStopLampStatus { get; set; }

    [DataMember(Order = 5)]
    public byte MalfunctionLampStatus { get; set; }

    [DataMember(Order = 6)]
    public ushort ParamGroupNumber { get; set; }

    [DataMember(Order = 7)]
    public uint SuspectParamNumber { get; set; }


    public byte Size { get; set; }

    public string Payload
    {
      get { return _Payload.ToString(true); }
      set { _Payload = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class StatisticsReportEventBlockData : EventBlockData
  {
    public VehicleBusControl? Control { get; set; }

    [DataMember(Order = 0)]
    public byte BlockID { get; set; }

    [DataMember(Order = 1)]
    public VehicleBusProtocol? Protocol { get; set; }
    
    [DataMember(Order = 2)]
    public ushort BlockCount { get; set; }

    [DataMember(Order = 3)]
    public StatisticsBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class StatisticsBlock
  {
    [DataMember(Order = 0)]
    public byte SourceAddress { get; set; }

    [DataMember(Order = 1)]
    public ushort ParamGroupNumber { get; set; }

    [DataMember(Order = 2)]
    public uint SuspectParamNumber { get; set; }

    [DataMember(Order = 3)]
    public short DeltaUTC { get; set; }

    [DataMember(Order = 4)]
    public short DeltaSMU { get; set; }

    [DataMember(Order = 5)]
    public int Minimum { get; set; }

    [DataMember(Order = 6)]
    public int Maximum { get; set; }

    [DataMember(Order = 7)]
    public int Average { get; set; }

    [DataMember(Order = 8)]
    public int StdDeviation { get; set; }

    [DataMember(Order = 9)]
    public byte ScaleFactor { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum J1939Report
  {
    [EnumMember]
    Fault = 0,

    [EnumMember]
    Periodic = 1,

    [EnumMember]
    Requested = 2
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum VehicleBusProtocol
  {
    [EnumMember]
    J1939 = 0,

    [EnumMember]
    J1939CAT = 1,

    [EnumMember]
    J1939CNH = 2,

    [EnumMember]
    ISO11783 = 3,

    [EnumMember]
    ISO11783CNH = 4
    // Future values reserved
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum VehicleBusControl
  {
    [EnumMember]
    Raw = 0,

    [EnumMember]
    TrimbleProprietary = 1
    // Future values reserved
  }

  #endregion

  #endregion // MachineEvent

  #region Service Outage Report

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class ServiceOutageReport : DevicePacket
  {
    private string _Text;

    [DataMember(Order = 0)]
    public byte ID { get; set; }

    [DataMember(Order = 1)]
    public byte SeqID { get; set; }

    [DataMember(Order = 2)]
    public uint DateTimeUTC { get; set; }

    [DataMember(Order = 3)]
    public ServiceOutageCategory Category { get; set; }

    [DataMember(Order = 4)]
    public ServiceOutageLevel Level { get; set; }

    [DataMember(Order = 5)]
    public byte FormatCode { get; set; }

    [DataMember(Order = 6)]
    public ushort Length { get; set; }

    [DataMember(Order = 7)]
    public string Text
    {
      get { return _Text.ToString(true); }
      set { _Text = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum ServiceOutageLevel
  {
    [EnumMember]
    Info = 1,

    [EnumMember]
    Warn = 2,

    [EnumMember]
    Error = 3,

    [EnumMember]
    Fatal = 4
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum ServiceOutageCategory
  {
    [EnumMember]
    MSPProcessor = 1,

    [EnumMember]
    GPSModule = 2,

    [EnumMember]
    MasterBoard = 3,

    [EnumMember]
    SkylineFirmwareApp = 4,

    [EnumMember]
    CATGatewayBoard = 5,

    [EnumMember]
    FirmwareUpdate = 6,

    [EnumMember]
    PeripheralConnection = 7,

    [EnumMember]
    WifiInformationalMessage = 8
  }

  #endregion

  #region Personality Report

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PersonalityReport : DevicePacket
  {
    [DataMember(Order = 0)]
    public byte ID { get; set; }

    [DataMember(Order = 1)]
    public byte Count { get; set; }

    [DataMember(Order = 2)]
    public PersonalityBlock[] Blocks { get; set; }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public class PersonalityBlock
  {
    private string _Text;


    [DataMember(Order = 0)]
    public ReportType Type { get; set; }

    [DataMember(Order = 1)]
    public byte Length { get; set; }

    [DataMember(Order = 2)]
    public string Text
    {
      get { return _Text.ToString(true); }
      set { _Text = value; }
    }
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum ReportType
  {
    [EnumMember]
    UBoot = 0x00,

    [EnumMember]
    Kernel = 0x01,

    [EnumMember]
    RFS = 0x02,

    [EnumMember]
    MSP = 0x03,

    [EnumMember]
    GPS = 0x04,

    [EnumMember]
    ICCID = 0x05,

    [EnumMember]
    GSN = 0x06,

    [EnumMember]
    GM = 0x07,

    [EnumMember]
    SerialNumber = 0x08,

    [EnumMember]
    Gateway = 0x09,

    [EnumMember]
    Hardware = 0x0A,

    [EnumMember]
    Software = 0x0B,

    [EnumMember]
    VIN = 0x0C
  }

  #endregion

  #region Helper
  
  public static class ICDMessagesHelper
  {
    public static bool ShouldEmitValue()
    {
      return (ConfigurationManager.AppSettings["VLEnvironment"] == null || ConfigurationManager.AppSettings["VLEnvironment"] != "CN");
    }
  }

  #endregion
}
