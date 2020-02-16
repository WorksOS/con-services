using System;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Linq;
using VSS.Hosted.VLCommon;

using System.Reflection;

namespace VSS.Hosted.VLCommon.TrimTracMessages
{
  #region -- Toplevel TT Command, used for WCF --
  [DataContract]
  [XmlType( Namespace = "http://www.w3.org/2001/XMLSchema-instance" )]
  [KnownType(typeof(TT_CTKC))] [XmlIncludeAttribute( typeof( TT_CTKC ) )]
  [KnownType(typeof(TT_CTKE))] [XmlIncludeAttribute( typeof( TT_CTKE ) )]
  [KnownType(typeof(TT_CTKF))] [XmlIncludeAttribute( typeof( TT_CTKF ) )]
  [KnownType(typeof(TT_CTKG))] [XmlIncludeAttribute( typeof( TT_CTKG ) )]
  [KnownType(typeof(TT_CTKJ))] [XmlIncludeAttribute( typeof( TT_CTKJ ) )]
  [KnownType(typeof(TT_CTKK))] [XmlIncludeAttribute( typeof( TT_CTKK ) )]
  [KnownType(typeof(TT_CTKP))] [XmlIncludeAttribute( typeof( TT_CTKP ) )]
  [KnownType(typeof(TT_CTKX))] [XmlIncludeAttribute( typeof( TT_CTKX ) )]
  [KnownType(typeof(TT_CTKY))] [XmlIncludeAttribute( typeof( TT_CTKY ) )]
  [KnownType(typeof(TT_CTKZ))] [XmlIncludeAttribute( typeof( TT_CTKZ ) )]
  [KnownType(typeof(TT_QTK_))] [XmlIncludeAttribute( typeof( TT_QTK_ ) )]
  [KnownType(typeof(TT_QTKD))] [XmlIncludeAttribute( typeof( TT_QTKD ) )]
  [KnownType(typeof(TT_QTKK))] [XmlIncludeAttribute( typeof( TT_QTKK ) )]
  [KnownType(typeof(TT_QTKM))] [XmlIncludeAttribute( typeof( TT_QTKM ) )]
  [KnownType(typeof(TT_QTKR))] [XmlIncludeAttribute( typeof( TT_QTKR ) )]
  [KnownType(typeof(TT_QTKU))] [XmlIncludeAttribute( typeof( TT_QTKU ) )]
  [KnownType(typeof(TT_RTKA))] [XmlIncludeAttribute( typeof( TT_RTKA ) )]
  [KnownType(typeof(TT_RTKF))] [XmlIncludeAttribute( typeof( TT_RTKF ) )]
  [KnownType(typeof(TT_RTKG))] [XmlIncludeAttribute( typeof( TT_RTKG ) )]
  [KnownType(typeof(TT_RTKJ))] [XmlIncludeAttribute( typeof( TT_RTKJ ) )]
  [KnownType(typeof(TT_RTKK))] [XmlIncludeAttribute( typeof( TT_RTKK ) )]
  [KnownType(typeof(TT_RTKL))] [XmlIncludeAttribute( typeof( TT_RTKL ) )]
  [KnownType(typeof(TT_RTKM))] [XmlIncludeAttribute( typeof( TT_RTKM ) )]
  [KnownType(typeof(TT_RTKP))] [XmlIncludeAttribute( typeof( TT_RTKP ) )]
  [KnownType(typeof(TT_RTKR))] [XmlIncludeAttribute( typeof( TT_RTKR ) )]
  [KnownType(typeof(TT_RTKS))] [XmlIncludeAttribute( typeof( TT_RTKS ) )]
  [KnownType(typeof(TT_RTKU))] [XmlIncludeAttribute( typeof( TT_RTKU ) )]
  [KnownType(typeof(TT_RTKV))] [XmlIncludeAttribute( typeof( TT_RTKV ) )]
  [KnownType(typeof(TT_RTKx))] [XmlIncludeAttribute( typeof( TT_RTKx ) )]
  [KnownType(typeof(TT_RTKX))] [XmlIncludeAttribute( typeof( TT_RTKX ) )]
  [KnownType(typeof(TT_RTKY))] [XmlIncludeAttribute( typeof( TT_RTKY ) )]
  [KnownType(typeof(TT_RTKZ))] [XmlIncludeAttribute( typeof( TT_RTKZ ) )]
  [KnownType(typeof(TT_STKA))] [XmlIncludeAttribute( typeof( TT_STKA ) )]
  [KnownType(typeof(TT_STKF))] [XmlIncludeAttribute( typeof( TT_STKF ) )]
  [KnownType(typeof(TT_STKG))] [XmlIncludeAttribute( typeof( TT_STKG ) )]
  [KnownType(typeof(TT_STKJ))] [XmlIncludeAttribute( typeof( TT_STKJ ) )]
  [KnownType(typeof(TT_STKK))] [XmlIncludeAttribute( typeof( TT_STKK ) )]
  [KnownType(typeof(TT_STKL))] [XmlIncludeAttribute( typeof( TT_STKL ) )]
  [KnownType(typeof(TT_STKP))] [XmlIncludeAttribute( typeof( TT_STKP ) )]
  [KnownType(typeof(TT_STKU))] [XmlIncludeAttribute( typeof( TT_STKU ) )]
  [KnownType(typeof(TT_STKX))] [XmlIncludeAttribute( typeof( TT_STKX ) )]
  [KnownType(typeof(TT_STKY))] [XmlIncludeAttribute( typeof( TT_STKY ) )]
  [KnownType(typeof(TT_STKZ))] [XmlIncludeAttribute( typeof( TT_STKZ ) )]
  [KnownType(typeof(TT_QTKN))] [XmlIncludeAttribute( typeof( TT_QTKN ) )]
  [KnownType(typeof(TT_RTKN))] [XmlIncludeAttribute( typeof( TT_RTKN ) )]
  [KnownType(typeof(TT_STKI))] [XmlIncludeAttribute( typeof( TT_STKI ) )]
  [KnownType(typeof(TT_RTKI))] [XmlIncludeAttribute( typeof( TT_RTKI ) )]
  [KnownType(typeof(TT_STKT))] [XmlIncludeAttribute( typeof( TT_STKT ) )]
  [KnownType(typeof(TT_RTKT))] [XmlIncludeAttribute( typeof( TT_RTKT ) )]
  public partial class TT 
  {
    public const char Query         = 'Q';
    public const char Configuration = 'S';
    public const char Response      = 'R';

    /// <summary>
    /// This original raw data string used to populate
    /// the properties of this instance.
    /// 
    /// Note that this property is only populated during parsing
    /// and any edits that occur afterward are not captured.
    /// Furthermore, this property will be null if an instance 
    /// is created by means other than parsing.
    /// </summary>
    protected string _OriginalParseData = null;
    public string OriginalParseData
    {
      get { return _OriginalParseData; }
      set { _OriginalParseData = value; }
    }

    public string OriginalCommandID
    {
      get { return CommandID(OriginalParseData); }
    }

    /// <summary>
    /// The command specifier from the original parse data.
    /// </summary>
    public char? OriginalCommandIdentifier
    {
      get { return CommandIdentifer(OriginalParseData); }
    }

    /// <summary>
    /// The command Qualifier from the original parse data.
    /// </summary>
    public char? OriginalCommandQualifier
    {
      get { return CommandQualifier(OriginalParseData); }
    }

    /// <summary>
    /// The first 4 characters past the initial '>'
    /// </summary>
    /// <param name="ttData">A Trim Trac formatted string</param>
    /// <returns>Returns the characters that indicate the command (e.g. QTKR)</returns>
    public static String CommandID(string ttData)
    {
      return (!String.IsNullOrEmpty(ttData) && ttData.Length>5) ? ttData.Substring(1, 4) : null;
    }

    /// <summary>
    /// The character representing whether this command is a query, report or configuration command.
    /// This is the 2nd character of a TTFormatted string
    /// </summary>
    /// <param name="ttData">The TT formatted string</param>
    /// <returns>The single character representing query, response of configuration command.</returns>
    public static char? CommandQualifier(string ttData)
    {
      return (!String.IsNullOrEmpty(ttData) && ttData.Length > 5) ? ttData[1] : (char?)null;
    }

    /// <summary>
    /// The character that maps to the command.  This is the 5th character of a TT formatted string
    /// </summary>
    /// <param name="ttData">A Trim Trac formatted string</param>
    /// <returns>The single character that represents that action.  For example, 'R' would be returned from a QTKR string or a RTKR response</returns>
    public static char? CommandIdentifer(string ttData)
    {
      return (ttData != null && ttData.Length > 5) ? ttData[4] : (char?)null;
    }

    public string Describe()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("{0, -34} : {1}", "Command ID", OriginalCommandID);

      var propertiesToDescribe = from p in this.GetType().GetProperties()
              where p.IsDefined(typeof(DataMemberAttribute), true)
              select p as MemberInfo;

      var fieldsToDescribe = from f in this.GetType().GetFields()
                                 where f.IsDefined(typeof(DataMemberAttribute), true)
                                 select f as MemberInfo;

      foreach (FieldInfo field in fieldsToDescribe.Union(propertiesToDescribe))
      {
        RecursiveDescribe(sb, field, this);
      }

      return sb.ToString();
    }

    static Type[] primitiveTypes = new Type[] { typeof(int), typeof(long), typeof(string), typeof(byte), typeof(float), typeof(double), typeof(short), typeof(bool),
                                                typeof(uint), typeof(ulong), typeof(char), typeof(ushort)};

    private void RecursiveDescribe(StringBuilder sb, MemberInfo mi, object ofObject)
    {
      FieldInfo fieldInfo = mi as FieldInfo;
      PropertyInfo propertyInfo = mi as PropertyInfo;

      Type dataType = null;
      object value = null;

      if (fieldInfo != null)
      {
        dataType = fieldInfo.FieldType;
        value = fieldInfo.GetValue(ofObject);
      }

      if (propertyInfo != null)
      {
        dataType = propertyInfo.PropertyType;
        value = propertyInfo.GetValue(ofObject, BindingFlags.Public | BindingFlags.GetProperty, null, null, null);
      }

      if (primitiveTypes.Any(f => f == dataType) || dataType.IsEnum)
      {
        sb.AppendFormat("{0}{1, -34} : {2}", System.Environment.NewLine, mi.Name, value);
        return;        
      }

      var propertiesToDescribe = from p in value.GetType().GetProperties()
                                 where p.IsDefined(typeof(DataMemberAttribute), true)
                                 select p as MemberInfo;

      var fieldsToDescribe = from f in value.GetType().GetFields()
                             where f.IsDefined(typeof(DataMemberAttribute), true)
                             select f as MemberInfo;

      foreach (MemberInfo field in fieldsToDescribe.Union(propertiesToDescribe))
      {
        RecursiveDescribe(sb, field, value);
      }
    }
  }

  #region -- Enums --
  public enum EEraseRestoreMode
  {
    RestoreDefaults = 1,
    EraseMessageLog = 2,
    EraseLogRestoreDefaults = 3,
  }

  public enum EGPRSTransportProtocol
  {
    UDP = 0,
    TCP = 1,
  }

  public enum EGPRSSessionProtocol
  {
    None = 0,
    TrimTracSessionProtocol = 1 // (See Separate ICD Document, v1.0)
  }

  public enum EGeofenceEnable
  {
    Disabled = 0,
    Enforced = 1,
    LPAActivated = 2, // (See Scheduled Hours Mode)
    LPACenteredAndActivated = 3,
  }

  public enum EGeofenceEnforcement
  {
    Always = 0,
    AfterHOOOnly = 1,
    DuringHOOOnly = 2,
  }

  public enum EScheduledReportMode
  {
    None = 0,
    Daily = 1,
    Weekly = 2
  }

  public enum EInMotionPolling
  {
    Disabled = 0,
    OnDemandWhileInMotion = 1
  }

  public enum EAnytimePolling
  {
    Disabled = 0,
    DutyCycledWhileInIDLEState = 1,
    AnytimeOnDemand = 2
  }

  public enum EFlag
  {
    Disabled = 0,
    Enabled = 1,
    Unknown = -1
  }

  public enum EMotionSensorOverride
  {
    Normal = 0,
    MotionAlways = 1,
    MotionNever = 2
  }

  public enum EOperationMode
  {
    Enabled = 0,
    AlertIgnored = 1,
    EnabledWithNetworkAck = 2,
    EnabledForMonitorOnly = 3,
    ChangeOfStateReporting = 4,
  }

  public enum EMode_Y
  {
    Automatic = 0,
    Disabled = 1,
    NetworkAcknowledgement = 2,
    MonitorOnly = 3
  }

  public enum EMode_Z
  {
    Disabled = 0,
    Enabled = 1,
  }

  public enum EGeofenceType
  {
    BoundaryCrossing = 0,
    Inclusive = 1,
    Exclusive = 2
  }

  public enum ESpeedingReportMode
  {
    ReportAllViolations = 0,
    ReportInitialViolationsOnly = 1
  }

  public enum EMotionReportFlag
  {
    None = 0,
    StartMotionReport = 1,
    StopMotionReport = 2,
    StartAndStopMotionReport = 3,
  }

  public enum EReportDelayFlag
  {
    TxAllMessages = 0,
    TxExceptionReports_QuerySetResponsesOnly = 1,
    TxMotionRelated_Exceptions_Responses = 2,
    TxIDLETimeout_T1_StatusMsgs_Exceptions_Responses = 3,
    TxStartStop_IDLEStatus_Exceptions_Responses = 4,
    TxEvery6thMotionTriggeredMessage_Exceptions_Respones = 5,
    TxEvery11thMotionTriggeredMessage_Exceptions_Respones = 6,
    TxStartStop_Exception_Responses_NoIDLEStatus = 7,
  }

  public enum EDiagnosticsMode
  {
    None = 0,
    LED = 1
  }

  public enum ECommunicationMode
  {
    SMS = 0,
    GPRS = 1,
    AutoSelect = 2,
  }

  public enum EPinState
  {
    OutputLow = 0,
    OutputHigh = 1,
    NoChange = 2,
    NoModuleAttached = 3,
  }

  public enum ETTAlertState
  {
    Normal = 0,
    Activated = 1,
    Sent = 2,
    Acknowledged = 3,
    MonitorActivated = 4,
  }

  //cast each one to a string
  public static class TTAlertResponse
  {
    public const string DontCare      = "X";
    public const string Acknowledged  = "3";
    public const string Clear         = "0";
  }

  public enum EFilter
  {
    All,
    UnsentOnly,
    PositionOnly,
    StatusOnly,
    AlertOnly,
    Unknown,
  }

  public enum ETimeRange
  {
    Unused,
    Newest,
    Oldest,
  }

  public enum EStopRESP_QUERY_LOG
  {
    DoNotSend,
    Send,
  }

  public enum ELastMessage
  {
    AutomaticallyIncludeTheLastMessageInTheLogRegardlessOfTheFilterSettings = 1,
    DonTAutomaticallyIncludeTheLastMessageInTheLog = 0,
  }

  public enum ETriggerType
  {
    IDLETimeout = 0,
    MotionDetected = 1,
    ExceptionReportAlert = 2,
    Query = 3,
    ScheduledReport = 4,
    RuntimeMeterReport = 5,
    StartStopReport = 6,
    FirmwareUpdate = 7,
  }

  public enum EGPSStatusCode
  {
    _3DGPSFix = 0,
    _2DGPSFix = 1,
    FixTimeout_0SVs = 2,
    FixTimeout_1SV = 3,
    FixTimeout_2SVs = 4,
    FixTimeout_3SVs = 5,
    GPSError = 6,
    NoFixAttempted = 7,
    FixAbortedDueToInvalidCAC = 8,
  }

  public enum EGSMStatusCode
  {
    NetworkAvailable = 0,
    MessageLogged_ie_ReportDelayFlagSet1 = 1,
    NetworkTimeout = 2,
    SIMError_NoSIM = 3,
    SIMPINError = 4,
    PreTXlog_LowBattery = 5,
    ModemInitializationFailure = 6,
    GPRSOpeningFailure = 7,
    TCPConnectionFailure = 8,
    SessionProtocolFailure = 9,
  }

  public enum EPositionAge
  {
    Current_noMotionSinceLastPosition = 0,
    Aged_motionSinceLastPosition = 1,
  }

  public enum EExternalPower
  {
    Bad_Below5VDC = 0,
    Good_5VDCOrHigher = 1,
  }

  public enum EGeofenceStatus
  {
    Normal = 0,
    Violation = 1,
    GeofenceReCenteredByLPAInput = 5,
  }

  public enum EExtendedGPSStatusCode
  {
    InternalAntenna_AlmanacComplete = 0,
    ExternalAntenna_AlmanacComplete = 1,
    IntAntenna_AlmanacIncomplete = 2,
    ExtAntenna_AlmanacIncomplete = 3,
    // Note: Always 0 if GPS Status Code set to 7
  }

  public enum ESpeedingStatus
  {
    Normal = 0,
    Violation = 1,
  }

  public enum EReportFlag
  {
    ReportOnly_NoReset = 0,
    ReportWithReset = 1,
    Disabled = 2,
  }

  public enum EAutomaticMessageLogDump
  {
    Disabled = 0,
    Enabled = 1,
    EnabledWithQueueMode_FIFO = 2
  }

  public enum EEngineIdleReportEnabled
  {
    NoEngineIdleReportSent = 0,
    EngineIdleReportSentAtEndOfIdling = 1,
    EngineIdleReportSentAtEndOfIdlingAndIdleCounterReset = 2,
    EngineIdleReportSentAtThresholdAndEnd = 3,
    EngineIdleReportSentAtThresholdAndAtEnd_AndResetAtEnd = 4,
  }

  public enum EGPSFixRate
  {
    GPSOperationDuringFIXStateOnly = 0,
    _1HzGPSOperationExceptWhile = 1,
    _1HzFixesPlusAlwaysCreateRTKP_ifPossible = 2,
    _1HzFixesPlusAlwaysCreateRTKPPlusUseGPSSpeedAsMotionIndication = 3,
  }

  public enum EVAMOutput
  {
    OutputLow = 0,
    OutputHigh = 1,
    NoChange = 2,
  }

  public enum EMeterFlag
  {
    ReportWithoutReset = 0,
    ReportWithReset = 1,
    NotEnabled = 2,
  }

  public enum EOTAFirmwareUpgradeState
  {
    Idle = 0x0,
    StartingOTA = 0x1,
    DownloadError = 0x5, // (TFTP transfer error)
    StorageError = 0x6, // (OTA image could not be stored in flash)
    ChecksumError = 0x7, // (OTA image failed checksum)
    ImageIntegrityError = 0x8, // (Reflash agent found Checksum/CRC error during reflashing)
    ImageUncompressError = 0x9, // (ReFlash agent failed to successfully uncompress the application image)
    OTAReflashWasSuccessful = 0xA,
    IncompatibleFlashDevice = 0xB, // (ReFlash agent and flash device are incompatible)
    ReflashError = 0xC, // (New image could not be flashed)
  }
  #endregion

  [DataContract]
  public partial class TT_ID_CMD : TT
  {
    [DataMember]
    public string ID = "00000000";

    public string ID_CMD { get { return string.IsNullOrEmpty(ID) || ID == "00000000" ? "" : string.Format(";ID={0}", ID); } }

    public TT_ID_CMD() { }
    internal TT_ID_CMD(string unitID)
    {
      this.ID = unitID;
    }
  }

  [DataContract]
  public partial class TT_OTA_CMD : TT_ID_CMD
  {
    [DataMember]
    public string PW = "00000000";

    public string PW_CMD { get { return string.IsNullOrEmpty(PW) ? "" : string.Format(";PW={0}", PW); } }

    public TT_OTA_CMD() { }
    internal TT_OTA_CMD(string unitID, string pwd)
      : base(unitID)
    {
      this.PW = pwd;
    }
  }

  [DataContract]
  public partial class TT_OTA_RSP : TT_ID_CMD { }
  #endregion

  #region -- Common TT DataContracts --
  [DataContract]
  public partial class APP_CONFIG
  {
    /// <summary>
    /// IDLE Timeout, T1
    /// In seconds (10 – 999990)
    /// Default 43200
    /// </summary>
    [DataMember]
    public int IDLETimeout_T1 { get { return _IDLETimeout_T1 ?? 43200; } set { _IDLETimeout_T1 = value; } }
    public int? _IDLETimeout_T1;

    /// <summary>
    /// FIX Timeout, T2
    /// In seconds (10 – 3600)
    /// Default 300
    /// </summary>
    [DataMember]
    public int FIXTimeout_T2 { get { return _FIXTimeout_T2 ?? 300; } set { _FIXTimeout_T2 = value; } }
    public int? _FIXTimeout_T2;

    /// <summary>
    /// TRANSMIT Timeout, T3
    /// In seconds (10 – 3600) Less than 240 not recommended
    /// Default 300
    /// </summary>
    [DataMember]
    public int TRANSMITTimeout_T3 { get { return _TRANSMITTimeout_T3 ?? 300; } set { _TRANSMITTimeout_T3 = value; } }
    public int? _TRANSMITTimeout_T3;

    /// <summary>
    /// DELAY Timeout, T4
    /// In seconds (10 – 86400)
    /// Default 900 
    /// </summary>
    [DataMember]
    public int DELAYTimeout_T4 { get { return _DELAYTimeout_T4 ?? 900; } set { _DELAYTimeout_T4 = value; } }
    public int? _DELAYTimeout_T4;

    /// <summary>
    /// QUERY Timeout, T5
    /// In seconds (10 – 3600)
    /// Default 60 
    /// </summary>
    [DataMember]
    public int QUERYTimeout_T5 { get { return _QUERYTimeout_T5 ?? 60; } set { _QUERYTimeout_T5 = value; } }
    public int? _QUERYTimeout_T5;

    /// <summary>
    /// Almanac Timeout, T6
    /// In hours (10 – 990)
    /// Default 168 
    /// </summary>
    [DataMember]
    public int AlmanacTimeout_T6 { get { return _AlmanacTimeout_T6 ?? 168; } set { _AlmanacTimeout_T6 = value; } }
    public int? _AlmanacTimeout_T6;

    /// <summary>
    /// Static Motion Filter Timeout, T7
    /// In seconds (0 – 90)
    /// Note: T7 is used exclusively while in the IDLE state; however, it is superseded by the 
    /// Dynamic Motion Filter Timeout, T21, whenever Anytime Polling is:  
    /// A.  Set “2=Anytime OnDemand”; OR 
    /// B.  Set “1-Duty-Cycled” and Polling Duty-Cycle On-Time, T20, is running. 
    /// Always set Static Motion Filter Timeout, T7, greater 
    /// than or equal to Static Motion Filter Counter, N3.
    /// Default 20 
    /// </summary>
    [DataMember]
    public int StaticMotionFilterTimeout_T7 { get { return _StaticMotionFilterTimeout_T7 ?? 20; } set { _StaticMotionFilterTimeout_T7 = value; } }
    public int? _StaticMotionFilterTimeout_T7;

    /// <summary>
    /// Motion Report Flag
    /// Default 0
    /// </summary>
    [DataMember]
    public EMotionReportFlag MotionReportFlag { get { return _MotionReportFlag ?? EMotionReportFlag.None; } set { _MotionReportFlag = value; } }
    public EMotionReportFlag? _MotionReportFlag;

    /// <summary>
    /// Report Delay Flag
    /// Default 0
    /// </summary>
    [DataMember]
    public EReportDelayFlag ReportDelayFlag { get { return _ReportDelayFlag ?? EReportDelayFlag.TxAllMessages; } set { _ReportDelayFlag = value; } }
    public EReportDelayFlag? _ReportDelayFlag;

    /// <summary>
    /// Diagnostics Mode
    /// Default 1
    /// </summary>
    [DataMember]
    public EDiagnosticsMode DiagnosticsMode { get { return _DiagnosticsMode ?? EDiagnosticsMode.LED; } set { _DiagnosticsMode = value; } }
    public EDiagnosticsMode? _DiagnosticsMode;

    /// <summary>
    /// Communication Mode
    /// Default 0
    /// </summary>
    [DataMember]
    public ECommunicationMode CommunicationMode { get { return _CommunicationMode ?? ECommunicationMode.SMS; } set { _CommunicationMode = value; } }
    public ECommunicationMode? _CommunicationMode;
  }

  [DataContract]
  public partial class ALERT_STATE
  {
    /// <summary>
    /// HPA Status: ‘X’ = Do not care, ‘3’ = Ack, ‘0’ = Clear
    /// </summary>
    [DataMember]
    public string HPAStatus = "0";

    /// <summary>
    /// MPA Status: ‘X’ = Do not care, ‘3’ = Ack, ‘0’ = Clear
    /// </summary>
    [DataMember]
    public string MPAStatus = "0";

    /// <summary>
    /// LPA Status: ‘X’ = Do not care, ‘3’ = Ack, ‘0’ = Clear
    /// </summary>
    [DataMember]
    public string LPAStatus = "0";
  }

  [DataContract]
  public partial class EXT_APP_CONFIG
  {
    /// <summary>
    /// Scheduled Report Mode
    /// 0=None; 1=Daily; 2=Weekly
    /// Default 0
    /// </summary>
    [DataMember]
    public EScheduledReportMode ScheduledReportMode = EScheduledReportMode.None;

    /// <summary>
    /// Scheduled ReportTime, T18
    /// In Seconds (0-604799).
    /// Note:  This is either the seconds into the day or into the week 
    /// for the device to schedule transmission of a STATUS_MESSAGE plus the most 
    /// recently logged POSITION_REP, if any, when this feature 
    /// is enabled.  If Scheduled Report Mode is set “0=None”, then enter 0.
    /// Default 0 
    /// </summary>
    [DataMember]
    public int ScheduledReportTime_T18 = 0;

    /// <summary>
    /// In-Motion Polling
    /// 0=Disabled; 1=OnDemand while in Motion. Note:  If set to 
    /// “1=OnDemand while in Motion”, the unit is able to receive 
    /// and process data messages ONLY while in motion regardless 
    /// of current state.
    /// Default 0 
    /// </summary>
    [DataMember]
    public EInMotionPolling InMotionPolling = EInMotionPolling.Disabled;

    /// <summary>
    /// Anytime Polling
    /// 0=Disabled; 
    /// 1=Duty-Cycled while in IDLE State; 
    /// 2=Anytime OnDemand.
    /// See “Polling” section on page 38.  
    /// Note: If set to “2=Anytime OnDemand”, the unit is able to receive and 
    /// process data messages regardless of motion, current state or how In-Motion 
    /// Polling is set.  If set “1=Duty-Cycled”, unit is able to receive and 
    /// process data messages while in QUERY or whenever 
    /// while Polling Duty-Cycle On-Time, T20, is running.. 
    /// Default 0 
    /// </summary>
    [DataMember]
    public EAnytimePolling AnytimePolling = EAnytimePolling.Disabled;

    /// <summary>
    /// Polling Duty-Cycle Frequency, T19
    /// In Seconds (10-999990).
    /// Note:  Determines how frequently the Polling Duty-Cycle On-Time, T20, 
    /// timer is started if and to “1=Duty-Cycled while in IDLE State” only 
    /// if the Anytime Polling is set and device is NOT in motion. 
    /// Default 3600 
    /// </summary>
    [DataMember]
    public int PollingDutyCycleFrequency_T19 = 3600;

    /// <summary>
    /// Polling Duty-Cycle On-Time, T20
    /// In Seconds (0,10-3600).
    /// Note: Determines how long after expiration of Polling Duty-Cycle 
    /// Frequency, T19, timer that the unit is able to receive and process 
    /// data messages.  A value of zero causes the QUERY Timeout, T5, value 
    /// to be used.
    /// Default 60 
    /// </summary>
    [DataMember]
    public int PollingDutyCycleOnTime_T20 = 60;

    /// <summary>
    /// Query Hold Flag
    /// 0=Disabled; 1=Enabled. Note: Unit attempts to reconnect to 
    /// the GSM network if the connection is lost during the
    /// QUERY state prior to the expiration of QUERY Timeout, T5
    /// Default 0 
    /// </summary>
    [DataMember]
    public EFlag QueryHoldFlag = EFlag.Disabled;

    /// <summary>
    /// Reserved
    /// 0=Formerly “Extended Motion Detection” in TrimTrac 1.0
    /// Not used in TrimTrac Pro.
    /// Default 0 
    /// </summary>
    [DataMember]
    public int Reserved = 0;

    /// <summary>
    /// Position Report Transmit Attempts, N1
    /// 0=Until T3 Expires; 1-255=Number of Tx Attempts.
    /// Default 1
    /// </summary>
    [DataMember]
    public int PositionReportTransmitAttempts_N1 = 1;

    /// <summary>
    /// Status Message Transmit Attempts, N2
    /// 0=Until T3 Expires; 1-255=Number of Tx Attempts.
    /// Set to 1 if Communication Mode set “1=GPRS-only”
    /// Default 1 
    /// </summary>
    [DataMember]
    public int StatusMessageTransmitAttempts_N2 = 1;

    /// <summary>
    /// Static Motion Filter Counter, N3
    /// In Seconds (1-90).
    /// Note: This parameter is used only while Static Motion Filter Timeout, T7, 
    /// is running in the IDLE State.  It is superseded by the Dynamic Motion Filter
    /// Counter, N4 whenever: 
    /// A.  Anytime Polling is set “2=Anytime OnDemand”; OR 
    /// B.  Polling Duty-Cycle On-Time, T20, is running due to 
    /// Anytime Polling being set “1=Duty-Cycled”.
    /// Always set Static Motion Filter Counter, N3, equal to or 
    /// less than Static Motion Filter Timeout, T7.
    /// Default 10 
    /// </summary>
    [DataMember]
    public int StaticMotionFilterCounter_N3 = 10;

    /// <summary>
    /// Dynamic Motion Filter Timeout, T21
    /// 
    /// In Seconds (1-90).
    /// Note:  T21 applies in TRANSMIT,QUERY and DELAY States.  
    /// Also applies in IDLE if:
    /// A.  Anytime Polling is set “2=Anytime OnDemand”; OR 
    /// B.  Polling Duty-Cycle On-Time, T20, is running due to Anytime Polling 
    /// being set “1=Duty-Cycled”.
    /// WARNING: Dynamic Motion Filter Timeout, T21, must always be set equal 
    /// to or greater than Dynamic  Motion Filter Counter, N4; else the unit 
    /// may not exit QUERY state.
    /// Default 20 
    /// </summary>
    [DataMember]
    public int DynamicMotionFilterTimeout_T21 = 20;

    /// <summary>
    /// Dynamic Motion Filter Counter, N4
    /// In Seconds (1-90).
    /// Note:  This parameter is enabled only when Dynamic Motion Filter Timeout, 
    /// T21, is active. 
    /// WARNING: Dynamic Motion Filter Counter, N4, must always be set equal to 
    /// or less than the setting for Dynamic Motion Filter Timeout, T21; 
    /// else the unit may not exit QUERY state.
    /// Default 10 
    /// </summary>
    [DataMember]
    public int DynamicMotionFilterCounter_N4 = 10;

    /// <summary>
    /// Motion Sensor Override
    /// 0=Normal; 1=Motion Always; 2=Motion Never.
    /// Note: Overrides the motion sensor if set to a value other than “0=Normal”.
    /// Default 0 
    /// </summary>
    [DataMember]
    public EMotionSensorOverride MotionSensorOverride = EMotionSensorOverride.Normal;
  }

  [DataContract]
  public partial class VAM_APP_CONFIG
  {
    // BBBBBB	HPA Idle Timeout. Parameter T11. Units of seconds. 
    [DataMember]
    public int HPAIdleTimeout_T11;

    // CCCCCC	MPA Idle Timeout. Parameter T12. Units of seconds. 
    [DataMember]
    public int MPAIdleTimeout_T12;

    // DDDDDD	HPA Delay Timeout. Parameter T13. Units of seconds
    [DataMember]
    public int HPADelayTimeout_T13;

    // EEEEEE	MPA Delay Timeout. Parameter T14. Units of seconds
    [DataMember]
    public int MPADelayTimeout_T14;

    // FFFFFF	HPA Transmit Timeout. Parameter T15. Units of seconds, 0 is infinity.
    [DataMember]
    public int HPATransmitTimeout_T15;

    // GGGGGG	MPA Transmit Timeout. Parameter T16. Units of seconds, 0 is infinity.
    [DataMember]
    public int MPATransmitTimeout_T16;

    // HHHHHH	HPA Query Timeout. Parameter T17. Units of seconds
    [DataMember]
    public int HPAQueryTimeout_T17;

    // III	Number of attempts to transmit HPA REPORT_POS messages in Transmit state. Parameter N5. 0 means until timeout. The default is 0.
    [DataMember]
    public int HPATransmitionAttempts_N5;

    // JJJ	Number of attempts to transmit MPA REPORT_POS messages in Transmit state. Parameter N6. 0 means until timeout. The default is 0.
    [DataMember]
    public int MPATransmitionAttempts_N6;

    // KKK	Number of attempts to transmit LPA REPORT_POS messages in Transmit state. Parameter N7. 0 means until timeout. The default is 0.
    [DataMember]
    public int LPATransmitionAttempts_N7;

    // L	HPA Operation Mode. ‘0’ = Enabled, ‘1’ = Alert Ignored, ‘2’ = Enabled with Network Ack, ‘3’ = Enabled for Monitor Only, ‘4’ = Change of State Reporting.
    [DataMember]
    public EOperationMode HPAOperationMode;

    // M	MPA Operation Mode. ‘0’ = Enabled, ‘1’ = Alert Ignored, ‘2’ = Enabled with Network Ack, ‘3’ = Enabled for Monitor Only, ‘4’ = Change of State Reporting.
    [DataMember]
    public EOperationMode MPAOperationMode;

    // N	LPA Operation Mode. ‘0’ = Enabled, ‘1’ = Alert Ignored, ‘2’ = Enabled with Network Ack, ‘3’ = Enabled for Monitor Only, ‘4’ = Change of State Reporting.
    [DataMember]
    public EOperationMode LPAOperationMode;
  }

  [DataContract]
  public partial class VAM_OUTPUT
  {
    // B	VAM Output 1: Desired state: ‘0’ – Output Low, ‘1’ – Output High, ‘2’ – No Change. (Note: Output 1 corresponds to pin CT14 of the spring connector. It is labeled as EXT_RELAY_OUT in the schematic)
    [DataMember]
    public EVAMOutput VAMOutput1;

    // C	VAM Output 2: Desired state: ‘0’ – Output Low, ‘1’ – Output High, ‘2’ – No Change. (Note: Output 2 corresponds to pin CT12 of the spring connector. It is labeled as VAM_OUT in the schematic)
    [DataMember]
    public EVAMOutput VAMOutput2;

    // DDDDDDDDDDDDDDDD	Reserved for future use.  Should be sent as ‘0000000000000000’.
    [DataMember]
    public int Reserved0;
  }

  [DataContract]
  public partial class EXT2_APP_CONFIG
  {
    /// <summary>
    /// <Motion Counter Threshold>
    /// Counter (1-2000)
    /// Default: 10 
    /// </summary>
    [DataMember]
    public int MotionCounterThreshold = 10;

    /// <summary>
    /// <Scheduled Hours Mode>
    /// 0=Disabled; 1=Enabled
    /// Default: 0 
    /// </summary>
    [DataMember]
    public EMode_Z ScheduledHoursMode = EMode_Z.Disabled;

    /// <summary>
    /// In Seconds (0 – 86399) after 12:00AM UTC.
    /// Default:  0 
    /// </summary>
    [DataMember]
    public int ScheduledHoursDailyStartTime_T27 = 0;

    /// <summary>
    /// In Seconds (0 – 86400)
    /// Default: 43200 
    /// </summary>
    [DataMember]
    public int ScheduledHoursWorkDayLength_T28 = 43200;

    /// <summary>
    /// 0=Sunday; 1=Monday; 2=Tuesday; 3=Wednesday; 4=Thursday; 5=Friday; 6=Saturday 
    /// (All relative to UTC).
    /// Default: 1 
    /// </summary>
    [DataMember]
    public DayOfWeek ScheduledHoursFirstWeeklyWorkDay = DayOfWeek.Monday;

    /// <summary>
    /// 1= One Day; 2=Two Days; 3=Three Days; 4=Four Days; 
    /// 5=Five Days; 6=Six Days; 7=Seven Days.  
    /// Default: 5 
    /// </summary>
    [DataMember]
    public int ScheduledHoursWorkDaysPerWeek = 5;

    /// <summary>
    /// 0=Disabled; 1=Enabled.
    /// Default: 0 
    /// </summary>
    [DataMember]
    public EMode_Z RuntimeMotionBased = EMode_Z.Disabled;

    /// <summary>
    /// 0=Disabled; 1=Enabled.
    /// Default: 0 
    /// </summary>
    [DataMember]
    public EMode_Z RuntimeLPABased = EMode_Z.Disabled;

    /// <summary>
    /// In Hours (0=No Countdown, 1-990)
    /// Default: 0
    /// </summary>
    [DataMember]
    public int RuntimeMotionBasedCountdown_T29 = 0;

    /// <summary>
    /// In Hours (0=No Countdown, 1-990)
    /// Default: 0
    /// </summary>
    [DataMember]
    public int RuntimeLPABasedCountdown_T30 = 0;

    /// <summary>
    /// 0=Disabled; 1=Enabled, 2=Enabled with queue mode (FIFO).
    /// Default:  0
    /// </summary>
    [DataMember]
    public EAutomaticMessageLogDump AutomaticMessageLogDump = EAutomaticMessageLogDump.Disabled;

    /// <summary>
    /// 0=GPS operation during FIX State only
    /// 1=1Hz GPS operation except while
    /// 2=1Hz fixes plus always create RTKP if possible
    /// 3=1Hz fixes plus always create RTKP plus use GPS Speed as motion indication
    /// stationary.
    /// Default:  0 
    /// </summary>
    [DataMember]
    public EGPSFixRate GPSFixRate = 0;

    /// <summary>
    /// In Seconds (0-990)
    /// Default: 0
    /// </summary>
    [DataMember]
    public int LPASpeedingReportInputArmingDelay_T31 = 0;

    /// <summary>
    /// 0=Boundary Crossing; 1=Inclusive; 2=Exclusive.
    /// Default 1
    /// </summary>
    [DataMember]
    public EGeofenceType GeofenceType = EGeofenceType.Inclusive;

    /// <summary>
    /// 0=No Enforcement; 1-990 = Limit in MPH.
    /// Default: 0. 
    /// </summary>
    [DataMember]
    public int SpeedEnforcement = 0;

    /// <summary>
    /// 0=Report All Violations; 1=Report Initial Violations Only.
    /// Default:  0
    /// </summary>
    [DataMember]
    public ESpeedingReportMode SpeedingReportMode = ESpeedingReportMode.ReportAllViolations;

    /// <summary>
    /// In Seconds (0-99990).
    /// Default: 0 
    /// </summary>
    [DataMember]
    public int SpeedingCountdownTimer = 0;

    // TTTT	Reporting Options: Range from ‘0’ to ‘FFFF’.
    [DataMember]
    public int ReportingOptions;

    // U	Ignition Sense Override: Range from ‘0’ to ‘3’.
    [DataMember]
    public int IgnitionSenseOverride;

    // VVVV	Engine Idle Threshold: 0 = No Engine Idle Time recorded, 1-9990 = Engine Idle Time recorded only after VVVV seconds of Engine Idle.  (Requires 1Hz GPS and IGN).
    [DataMember]
    public int EngineIdleThreshold;

    // W	Engine Idle Report Enable: 0 = No Engine Idle Report sent, 1 = Engine Idle Report sent at end of idling, 2 = Engine Idle Report sent at end of idling and Idle Counter Reset, 3 = Engine Idle Report sent at threshold and end, 4 Engine Idle Report sent at threshold and at end, and Reset at end.
    [DataMember]
    public EEngineIdleReportEnabled EngineIdleReportEnabled;

    // XXXX	Speed Threshold Time: 0-9990 seconds: Number of seconds to initially filter speed. 0 = use built-in default of 10 seconds.
    [DataMember]
    public int SpeedThresholdTime;

    // YY	Moving Speed: 0-90 mph
    [DataMember]
    public int MovingSpeed;

    // ZZZZ	Early Stop Timer: 0-9990 seconds.  Stop timer to override normal DELAY time.
    [DataMember]
    public int EarlyStopTimer;

    // aaaa TBD Chars for future development. Must be 0.
    [DataMember]
    public int Reserved6;
  }

  [DataContract]
  public partial class GEOFENCE_CONFIG
  {
    /// <summary>
    /// <Geofence ID>
    /// Unique geofence identifier 1, ... 10
    /// Default: 1 
    /// </summary>
    [DataMember]
    public int GeofenceID = 1;

    /// <summary>
    /// </summary>
    [DataMember]
    public EGeofenceEnable GeofenceEnable = EGeofenceEnable.Disabled;

    /// <summary>
    /// </summary>
    [DataMember]
    public EGeofenceEnforcement GeofenceEnforcement = EGeofenceEnforcement.Always;

    /// <summary>
    /// <Geofence Delta X>
    /// 100s of meters (1 – 10000)
    /// Note:  East-West length of rectangular Geofence area or, if circular, the 
    /// diameter. 1=100 Meters
    /// Default:  1 
    /// </summary>
    [DataMember]
    public int GeofenceDeltaX = 1;

    /// <summary>
    /// <Geofence Delta Y>
    /// 100s of meters (0, 1 – 10000)
    /// Note: If set to “0”, then circular Geofence area; else rectangular 
    /// Geofence area. 1=100 Meters
    /// Default:  1 
    /// </summary>
    [DataMember]
    public int GeofenceDeltaY = 1;

    /// <summary>
    /// <Geofence Center Latitude>
    /// WGS-84 Coordinates.  Units of Degrees to 7 decimal places 
    /// plus ‘+’ sign = North and ‘-’ = South
    /// (-90.0000000 to +90.0000000)
    /// Must be in quotation marks
    /// Default: “+0.0000000”
    /// </summary>
    [DataMember]
    public double GeofenceCenterLatitude = 0;

    /// <summary>
    /// <Geofence Center Longitude>
    /// WGS-84 Coordinates. Units of Degrees to 7 decimal places
    /// plus ‘+’ sign = East and ‘-‘ = West
    /// (-180.0000000 to +180.0000000) 
    /// Must be in quotation marks
    /// Default:  “+0.0000000” 
    /// </summary>
    [DataMember]
    public double GeofenceCenterLongitude = 0;
  }

  [DataContract]
  public partial class GPRS_CONNECT_CONFIG
  {
    /// <summary>
    /// <GPRS Transport Protocol>
    /// 0=UDP (TrimTrac Session Protocol recommended); 1=TCP.
    /// Default:  1 
    /// </summary>
    [DataMember]
    public EGPRSTransportProtocol GPRSTransportProtocol = EGPRSTransportProtocol.TCP;

    /// <summary>
    /// <GPRS Session Protocol>
    /// </summary>
    [DataMember]
    public EGPRSSessionProtocol GPRSSessionProtocol = EGPRSSessionProtocol.None;

    /// <summary>
    /// <GPRS Session Keep-alive Timeout, T25> 
    /// In Seconds (0=Never; 1 –43200)
    /// Default:  300 
    /// </summary>
    [DataMember]
    public int GPRSSessionKeepAliveTimeout_T25 = 300;

    /// <summary>
    /// <GPRS Session Timeout, T26>
    /// GPRS Session Timeout, T26
    /// Default: 0
    /// </summary>
    [DataMember]
    public int GPRSSessionTimeout_T26 = 0;

    /// <summary>
    /// <GPRS Destination Address>
    /// 
    /// GPRS destination address in the following format: 
    /// 111.222.333.444:12345.  This represents an IP Address 
    /// and Port Number pair, maximum number string up to 21
    /// characters.  Must use quotation marks “_”. 
    /// Default: 0.0.0.0:0 
    /// </summary>
    [DataMember]
    public string GPRSDestinationAddress = "0.0.0.0:0";
  }

  [DataContract]
  public partial class GPRS_SETUP_CONFIG
  {
    /// <summary>
    /// <GPRS APN>
    /// 
    /// Access Point Name (APN) Web address up to 40 case sensitive alphanumeric 
    /// characters as assigned by GPRS network operator.  Example: 
    /// “apn.trimble.com”.  Must use quotation marks “_”.
    /// 
    /// Default: <Empty> 
    /// </summary>
    [DataMember]
    public string GPRSAPN;

    /// <summary>
    /// <GPRS Username>
    /// 
    /// Up to 40 case sensitive alphanumeric characters.  
    /// Must use quotation marks “_”.
    /// 
    /// Default: <Empty> 
    /// </summary>
    [DataMember]
    public string GPRSUsername;

    /// <summary>
    /// <GPRS Password>
    /// 
    /// Up to 20 case sensitive alphanumeric characters.
    /// Must use quotation marks “_”.
    /// 
    /// Default: <Empty> 
    /// </summary>
    [DataMember]
    public string GPRSPassword;
  }

  [DataContract]
  public partial class GPS_CONFIG
  {
    /// <summary>
    /// <GPS Elevation Mask>
    /// In degrees (0-30)
    /// Default 5
    /// </summary>
    [DataMember]
    public int GPSElevationMask = 5;

    /// <summary>
    /// <GPS PDOP Mask>
    /// In tenths of PDOP (60 – 200)
    /// Default 120
    /// </summary>
    [DataMember]
    public int GPSPDOPMask = 120;

    /// <summary>
    /// <GPS PDOP Switch>
    /// In tenths of PDOP (40 – 120)
    /// Default 60
    /// </summary>
    [DataMember]
    public int GPSPDOPSwitch = 60;

    /// <summary>
    /// <GPS Signal Mask>
    /// In tenths of AMUs (10 – 80)
    /// Default 14
    /// </summary>
    [DataMember]
    public int GPSSignalMask = 14;

    /// <summary>
    /// <GPS Dynamics Mode>
    /// (Do Not Change)
    /// Default 5
    /// </summary>
    [DataMember]
    public int GPSDynamicsMode = 5;
  }

  [DataContract]
  public partial class MODULE_APP_CONFIG
  {
    /// <summary>
    /// <HPA Idle Timeout, T11>
    /// In Seconds (10-999990).
    /// Default 10 
    /// </summary>
    [DataMember]
    public int HPAIdleTimeout_T11 = 10;

    /// <summary>
    /// <MPA Idle Timeout, T12>
    /// In Seconds (10-999990).
    /// Default 10
    /// </summary>
    [DataMember]
    public int MPAIdleTimeout_T12 = 10;

    /// <summary>
    /// <HPA Delay Timeout, T13>
    /// In Seconds (10-86400).
    /// Default  10
    /// </summary>
    [DataMember]
    public int HPADelayTimeout_T13 = 10;

    /// <summary>
    /// <MPA Delay Timeout, T14> 
    /// In Seconds (10-86400)
    /// Default 10
    /// </summary>
    [DataMember]
    public int MPADelayTimeout_T14 = 10;

    /// <summary>
    /// <HPA Transmit Timeout, T15>
    /// 0=Infinity; Else in Seconds (10-999990)
    /// Default 0
    /// </summary>
    [DataMember]
    public int HPATransmitTimeout_T15 = 0;

    /// <summary>
    /// <MPA Transmit Timeout, T16>
    /// 0=Infinity; Else in Seconds (10-999990)
    /// If not set 0-Infinity, should be set no lower than 240 Seconds.
    /// Default 300
    /// </summary>
    [DataMember]
    public int MPATransmitTimeout_T16 = 300;

    /// <summary>
    /// <HPA Query Timeout, T17>
    /// In Seconds (10-3600)
    /// Default 60
    /// </summary>
    [DataMember]
    public int HPAQueryTimeout_T17 = 60;

    /// <summary>
    /// <HPA Transmit Attempts, N5>
    /// 0=Until T15 Expires; 1-255=Number of Tx Attempts before T15 expires
    /// Default 0
    /// </summary>
    [DataMember]
    public int HPATransmitAttempts_N5 = 0;

    /// <summary>
    /// <MPA Transmit Attempts, N6>
    /// 0=Until T16 Expires; 1-255=Number of Tx Attempts before T16 expires
    /// Default 0 
    /// </summary>
    [DataMember]
    public int MPATransmitAttempts_N6 = 0;

    /// <summary>
    /// <LPA Transmit Attempts, N7>
    /// 0=Until T3 Expires; 1-255=Number of Tx Attempts before T3 expires.
    /// Default 0 
    /// </summary>
    [DataMember]
    public int LPATransmitAttempts_N7 = 0;

    /// <summary>
    /// <HPA Mode>
    /// Default 0
    /// </summary>
    [DataMember]
    public EMode_Y HPAMode = EMode_Y.Automatic;

    /// <summary>
    /// <MPA Mode>
    /// WARNING: Before inserting a Vehicle Adapter or Control Module: 
    /// A)  Connect the red (+) and black (-) wires to a 9-32 VDC power source 
    /// and install a 100k Ohm resistor across the yellow wires; OR 
    /// B)  Return MPA Mode to its default setting “1=Disabled”.
    /// Default 1. 
    /// </summary>
    [DataMember]
    public EMode_Y MPAMode = EMode_Y.Disabled;

    /// <summary>
    /// <LPA Mode>
    /// Default 0
    /// </summary>
    [DataMember]
    public EMode_Y LPAMode = EMode_Y.Automatic;
  }

  [DataContract]
  public partial class PROV_CONFIG
  {
    /// <summary>
    /// <SMS Destination Address>
    /// Maximum 24 characters including optional international 
    /// dialing “+” sign and country code for SMS messages.
    /// Must be in quotation marks.
    /// Default <Empty>
    /// </summary>
    [DataMember]
    public string SMSDestinationAddress = "";
  }

  [DataContract]
  public partial class CONTROL_OUTPUT
  {
    /// <summary>
    /// Output 1
    ///   0=Output Low
    ///   1=Output High
    ///   2=No Change
    /// </summary>
    [DataMember]
    public EPinState Output1 = EPinState.NoChange;

    /// <summary>
    /// Output 2
    ///   0=Output Low
    ///   1=Output High
    ///   2=No Change
    /// </summary>
    [DataMember]
    public EPinState Output2 = EPinState.NoChange;

    /// <summary>
    /// Reserved for future use
    /// 16 digits
    /// Must be sent as ‘0000000000000000’. 
    /// </summary>
    [DataMember]
    public int Reserved = 0;
  }

  [DataContract]
  public partial class REPORT_POS
  {
    [DataMember]
    public double Latitude;

    [DataMember]
    public double Longitude;

    [DataMember]
    public int Altitude;

    [DataMember]
    public int Speed;

    [DataMember]
    public int Heading;
  }

  [DataContract]
  public partial class STATUS_MSG
  {
    /// <summary>
    /// Protocol Sequence Number
    /// 16-bit Hex (0000-FFFF)
    /// incrementing by 1 for each logged report. 
    /// </summary>
    [DataMember]
    public int ProtocolSequenceNumber;

    [DataMember]
    public ETriggerType TriggerType;

    /// <summary>
    /// Battery Level
    /// 0-100%, 999%=Low Voltage Cut-out
    /// </summary>
    [DataMember]
    public int BatteryLevel;

    [DataMember]
    public bool BatteryChangedFlag;

    [DataMember]
    public int GPSTimeWeek;

    [DataMember]
    public int GPSTimeSeconds;

    [DataMember]
    public EGPSStatusCode GPSStatusCode;

    [DataMember]
    public EGSMStatusCode GSMStatusCode;

    [DataMember]
    public EPositionAge PositionAge;

    [DataMember]
    public ETTAlertState HPAStatus;

    [DataMember]
    public ETTAlertState MPAStatus;

    [DataMember]
    public ETTAlertState LPAStatus;

    [DataMember]
    public EExternalPower ExternalPower;

    [DataMember]
    public EGeofenceStatus GeofenceStatus;

    [DataMember]
    public EExtendedGPSStatusCode ExtendedGPSStatusCode;

    [DataMember]
    public ESpeedingStatus SpeedingStatus;

    [DataMember]
    public ESpeedingStatus ScheduledHoursFlag;

    [DataMember]
    public int Reserved;
  }

  [DataContract]
  public partial class IDENT
  {
    // BBBBBBBB	Device ID: The new Device ID will be used in the response to this message.
    [DataMember]
    public string DeviceID;

    // CCCCCCCC	Security Password: The new security password is only accepted is the old values match.
    [DataMember]
    public string SecurityPassword;
  }

  [DataContract]
  public partial class OTA_FIRWMARE
  {
    // FFFFFFFF	OTA File Size
    [DataMember]
    public int OTAFileSize;

    // CCCCCCCC	OTA File Checksum
    [DataMember]
    public uint OTAFileChecksum;

    // D…D (15 chars)	OTA TFTP IP Address. The quote (“) symbol must be used to terminate this text field.  It may also be used to terminate this field early.
    [DataMember]
    public string OTATFTPIPAddress;

    // E…E (40 chars)	OTA TFTP Filename. The quote (“) symbol must be used to terminate this text field.  It may also be used to terminate this field early.
    [DataMember]
    public string OTATFTPFilename;
  }
  #endregion

  #region -- AT+CTK* --
  internal interface ITT_ToOTA
  {
    TT ToOTA(string unitID, string pwd);
  }

  #region -- AT+CTKC Application Configuration --
  /// <summary>
  /// AT+CTKC Application Configuration (v 1.1, page 109)
  /// 
  /// The AT+CTKC command is used to configure the Basic Read/Write parameters
  /// of the TrimTrac Pro.  It can query the current settings or change them to new
  /// values.
  /// 
  /// AT+CTKC=<IDLE Timeout, T1>,<FIX Timeout, T2>,<TRANSMIT Timeout, T3>,
  /// <DELAY Timeout, T4>,<QUERY Timeout, T5>,<Almanac Timeout, T6>,
  /// <Static Motion Filter Timeout, T7>,<Motion Report Flag>,
  /// <Report Delay Flag>,<Diagnostics Mode>,<Communication Mode> 
  /// 
  /// Default: AT+CTKC=43200,300,300,900,60,168,20,0,0,1,0
  /// </summary>
  [DataContract]
  public partial class TT_CTKC : TT, ITT_ToOTA
  {
    [DataMember]
    public APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKA(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKE Factory Default & Log Erase Command --
  /// <summary>
  /// AT+CTKE Factory Default & Log Erase Command (v 1.1, page 112)
  /// 
  /// The AT+CTKE command restores configuration parameters back to factory 
  /// default values and erase the message log from the TrimTrac Pro device.  After a 
  /// slight pause, the RDY response will appear.  Wait for RDY to appear before 
  /// executing additional commands. 
  /// </summary>
  [DataContract]
  public partial class TT_CTKE : TT
  {
    [DataMember]
    public EEraseRestoreMode EraseRestoreMode;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- AT+CTKF GPRS Connection Provisioning --
  /// <summary>
  /// AT+CTKF GPRS Connection Provisioning (v 1.1, page 112)
  /// 
  /// The AT+CTKF command configures the GPRS connection provisioning 
  /// parameters.  The GPRS Destination Address text field must be enclosed with
  /// quotations  
  /// 
  /// AT+CTKF=<GPRS Transport Protocol>,<GPRS Session Protocol>,<GPRS Session Keep-alive Timeout, T25>,
  /// <GPRS Session Timeout, T26>,<GPRS Destination Address> 
  /// 
  /// Default AT+CTKF=1,0,300, 0,”0.0.0.0:0” 
  /// </summary>
  [DataContract]
  public partial class TT_CTKF : TT, ITT_ToOTA
  {
    [DataMember]
    public GPRS_CONNECT_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKF(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKJ GPRS Setup Provisioning --
  /// <summary>
  /// AT+CTKJ GPRS Setup Provisioning (v 1.1, page 114)
  /// 
  /// The AT+CTKJ command configures the GPRS setup provisioning parameters.
  /// All fields must be enclosed with quotations .  All GPRS setup provisioning 
  /// parameters are provided by the GPRS service provider. 
  /// 
  /// AT+CTKJ=<GPRS APN>,<GPRS Username>,<GPRS Password>
  /// 
  /// Default: AT+CTKJ=””,””,””
  /// </summary>
  [DataContract]
  public partial class TT_CTKJ : TT, ITT_ToOTA
  {
    [DataMember]
    public GPRS_SETUP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKJ(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKG GPS Configure --
  /// <summary>
  /// AT+CTKG GPS Configure (v 1.1, page 115)
  /// 
  /// The AT+CTKG command configures the GPS parameters.  This command
  /// mirrors the functionality of the over-the-air GPS_CONFIG message.  It can query 
  /// the current settings or change them to new values.  Unless you are very familiar 
  /// with the types of parameters listed below and how changes will effect overall 
  /// GPS performance in any given application environment, it is recommended that 
  /// you leave these parameters at their factory default settings. 
  /// 
  /// AT+CTKG=<GPS Elevation Mask>,<GPS PDOP Mask>,<GPS PDOP Switch>,
  /// <GPS Signal Mask>,<GPS Dynamics Mode> 
  /// 
  /// Default:  AT+CTKG=5,120,60,14,5
  /// </summary>
  [DataContract]
  public partial class TT_CTKG : TT, ITT_ToOTA
  {
    [DataMember]
    public GPS_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKG(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKK Geofence Configuration --
  /// <summary>
  /// AT+CTKK Geofence Configuration (v 1.1, page 116)
  /// 
  /// The AT+CTKK command configures the Geofence parameters.  All Geofences 
  /// must be the same type as defined in AT+CTKZ or SET_EXT2_APP_CONFIG.
  /// 
  /// AT+CTKK=<Geofence ID>,<Geofence Enforcement>,<Reserved>,<Geofence Delta X>,
  /// <Geofence Delta Y>,<Geofence Center Latitude>,<Geofence Center Longitude>
  /// 
  /// Default:  AT+CTKK=1,0,0,1,1,”+0.0000000”,”+0.0000000”
  /// </summary>
  [DataContract]
  public partial class TT_CTKK : TT, ITT_ToOTA
  {
    [DataMember]
    public GEOFENCE_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKK(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKP Provisioning --
  /// <summary>
  /// AT+CTKP Provisioning (v 1.1, page 118)
  /// 
  /// Prior to the TrimTrac Pro being used for the first time, the application must be 
  /// provisioned using this command.  The AT+CTKP command configures the SMS 
  /// Communication and Security parameters.  This command is the only method to 
  /// change the Unit ID, SIM PIN and Security Password.  It can query the current 
  /// settings or change them to new values.  See AT+CTKF and AT+CTKJ for GPRS 
  /// related settings. 
  /// 
  /// AT+CTKP=<Unit ID>,<SMS Destination Address>,<SIM PIN>,<Security Password> 
  /// 
  /// Default:  AT+CTKP=“00000000”,””,””,“00000000”
  /// </summary>
  [DataContract]
  public partial class TT_CTKP : TT, ITT_ToOTA
  {
    [DataMember]
    public PROV_CONFIG Data;

    /// <summary>
    /// <Unit ID>
    /// Always 8 alphanumeric characters, UPPER CASE ONLY.  
    /// Must use quotation marks “_”
    /// Default “00000000”
    /// </summary>
    [DataMember]
    public string UnitID = "00000000";

    /// <summary>
    /// <SIM PIN>
    /// If used, must be between 4 and 8 digits. Must use quotation marks “_”  
    /// Default <Empty>
    /// </summary>
    [DataMember]
    public string SIMPIN = "";

    /// <summary>
    /// <Security Password>
    /// Always 8 alphanumeric characters, UPPER CASE ONLY.
    /// Must use quotation marks “_”
    /// Default “00000000”
    /// </summary>
    [DataMember]
    public string SecurityPassword = "00000000";

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKP(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKX Extended Application Configuration --
  /// <summary>
  /// AT+CTKX Extended Application Configuration (v 1.1, page 120)
  /// 
  /// The AT+CTKX command is used to configure the enhanced Basic Read/Write 
  /// parameters of the TrimTrac Pro.  It can query the current settings or change them 
  /// to new values. 
  /// 
  /// AT+CTKX=<Scheduled Report Mode>,<Scheduled Report Time, T18>,<In-Motion Polling>,<Anytime Polling>,
  /// <Polling Duty-Cycle Frequency, T19>,<Polling Duty-Cycle On-Time, T20>,<Query Hold Flag>,<Reserved>,
  /// <Position Report Transmit Attempts, N1>,<Status Message Transmit Attempts, N2>,
  /// <Static Motion Filter Counter, N3>,<Dynamic Motion Filter Timeout, T21>,<Dynamic Motion Filter Counter, N4>,
  /// <Motion Sensor Override> 
  /// 
  /// Default:  AT+CTKX=0,0,0,0,3600,60,0,0,1,1,10,20,10,0
  /// </summary>
  [DataContract]
  public partial class TT_CTKX : TT, ITT_ToOTA
  {
    [DataMember]
    public EXT_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKX(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKY Module-only Application Configuration --
  /// <summary>
  /// AT+CTKY Module-only Application Configuration (v 1.1, page 124)
  /// 
  /// The AT+CTKY command configures the application parameters specific to the 
  /// Vehicle Adapter or Control Module.  
  /// 
  /// AT+CTKY=<HPA Idle Timeout, T11>,<MPA Idle Timeout, T12>,<HPA Delay Timeout, T13>,
  /// <MPA Delay Timeout, T14>,<HPA Transmit Timeout, T15>,<MPA Transmit Timeout, T16>,
  /// <HPA Query Timeout, T17>,<HPA Transmit Attempts, N5>,<MPA Transmit Attempts, N6>,
  /// <LPA Transmit Attempts, N7>,<HPA Mode>,<MPA Mode>,<LPA Mode> 
  /// 
  /// Default:  AT+CTKY=10,10,10,10,0,300,60,0,0,0,0,1,0
  /// </summary>
  [DataContract]
  public partial class TT_CTKY : TT, ITT_ToOTA
  {
    [DataMember]
    public MODULE_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKY(unitID, pwd, Data); }
    #endregion
  }
  #endregion

  #region -- AT+CTKZ Daily Hours, Runtime & Other Extended Settings --
  /// <summary>
  /// AT+CTKZ Daily Hours, Runtime & Other Extended Settings (v 1.1, page 127)
  /// 
  /// The AT+CTKZ command configures the Scheduled Hours Mode, Runtime Meter 
  /// and other operation parameters as defined below. 
  /// 
  /// AT+CTKZ=<Motion Counter Threshold>,<Scheduled Hours Mode>,<Scheduled Hours Daily Start Time, T27>,
  /// <Scheduled Hours Work Day Length, T28>,<Scheduled Hours First Weekly Work Day>,
  /// <Scheduled Hours Work Days per Week>,<Runtime Motion-based>,<Runtime LPA-based>,
  /// <Runtime Motion-based Countdown, T29>,<Runtime LPA-based Countdown, T30>,<Automatic Message Log Dump>,
  /// <GPS Fix Rate>,<LPA Speeding Report Input Arming Delay, T31>,<Geofence Type>,<Speed Enforcement>,
  /// <Mode>,<Speeding Countdown Timer>,<Reserved>,<Reserved>,<Reserved>,<Reserved>,<Reserved>,<Reserved>,<Reserved>
  /// 
  /// Default: AT+CTKZ=10,0,0,43200,1,5,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0
  /// </summary>
  [DataContract]
  public partial class TT_CTKZ : TT, ITT_ToOTA
  {
    [DataMember]
    public EXT2_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    #region ITT_ToOTA Members
    public TT ToOTA(string unitID, string pwd) { return new TT_STKZ(unitID, pwd, Data); }
    #endregion
  }
  #endregion
  #endregion

  // A wildcard character ‘?’ will now be supported for some of the fields in the 
  // SET_*_CONFIG message types.  When this character is encountered, the ‘?’ value 
  // is replaced by the current value of that field.  The following messages can 
  // have wildcards in the configuration data fileds.

  #region -- STK* --
  #region -- SET_ALERT_STATE (STKL) --
  /// <summary>
  /// SET_ALERT_STATE (STKL) (v 1.1, page 140)
  /// 
  /// This message is sent to the TrimTrac Pro to acknowledge or clear alert states; 
  /// provided, however, that the TrimTrac Pro is connected to a Vehicle Adapter or 
  /// Control Module.  When received the TrimTrac Pro will send a RESP_ALERT_STATE 
  /// (page 162) message in response, UNLESS: 
  /// 1.  the SET_ALERT_STATE messages attempts to Clear an alert before all 
  ///     activated inputs have been returned to their normal non-alert conditions, in 
  ///     which case, no RESP_ALERT_STATE message will be sent.  Instead, a 
  ///     new alert message will be generated. 
  /// 2.  If battery powered, sending the device a SET_ALERT_STATE message 
  ///     will have no effect and not cause a RESP_ALERT_STATE message to be 
  ///     returned. 
  /// 
  /// FORMAT: >STKABCD;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// 
  /// Sample SET_ALERT_STATE Message to ACKNOWLEDGE alerts: 
  /// >STKL333;PW=00000000;ID=00000000;*16<
  /// 
  /// Sample SET_ALERT_STATE Message to CLEAR all alerts: 
  /// >STKL000;PW=00000000;ID=00000000;*15< 
  /// 
  /// NOTE:  Sending a Clear command before returning all conditions to their normal, non-
  /// alert states will re-initiate the alert message sequence.  If you wish to silence an activated 
  /// alert before the monitored device or condition have been restored to their normal non-
  /// alert state, then send an acknowledgement SET_ALERT_STATE message.  This will not 
  /// clear the alert, but it will stop re-transmissions until the condition is cleared. 
  /// </summary>
  [DataContract]
  public partial class TT_STKL : TT_OTA_CMD
  {
    [DataMember]
    public ALERT_STATE Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKL() { }
    internal TT_STKL(string unitID, string pwd, ALERT_STATE data) : base(unitID, pwd) { this.Data = data; }

    internal static TT_STKL Acknowledge(TT_RTKS status, String password)
    {
      TT_STKL ack = null;

      bool monitorActivated = (
        status.Data.HPAStatus == ETTAlertState.MonitorActivated &&
        status.Data.MPAStatus == ETTAlertState.MonitorActivated &&
        status.Data.LPAStatus == ETTAlertState.MonitorActivated
      );

      if (!monitorActivated && status.Data.TriggerType == ETriggerType.ExceptionReportAlert)
      {
        ack = new TT_STKL()
        {
          ID = status.ID,
          PW = password,
          Data = new ALERT_STATE()
          {
            HPAStatus = AlertResponse(status.Data.HPAStatus),
            MPAStatus = AlertResponse(status.Data.MPAStatus),
            LPAStatus = AlertResponse(status.Data.LPAStatus),
          }
        };
      }

      return ack;
    }

    private static string AlertResponse(ETTAlertState alertState)
    {
      return (
        (
          alertState == ETTAlertState.Activated || 
          alertState == ETTAlertState.Sent
        ) ? TTAlertResponse.Acknowledged : TTAlertResponse.DontCare
      );
    }

    private static string ClearResponse(ETTAlertState alertState)
    {
      return (
        (
          alertState == ETTAlertState.Acknowledged
        ) ? TTAlertResponse.Clear : TTAlertResponse.DontCare
      );
    }

    //Created method to clear 
    internal static TT_STKL Clear(TT_RTKS pos, String password)
    {
      TT_STKL clear = null;

      if (pos.Data.ExternalPower == EExternalPower.Good_5VDCOrHigher && pos.Data.MPAStatus == ETTAlertState.Acknowledged)
      {
        clear = new TT_STKL()
        {
          ID = pos.ID,
          PW = password,
          Data = new ALERT_STATE()
          {
            HPAStatus = ClearResponse(pos.Data.HPAStatus),
            MPAStatus = ClearResponse(pos.Data.MPAStatus),
            LPAStatus = ClearResponse(pos.Data.LPAStatus),
          }
        };
      }

      return clear;
    }
  }
  #endregion

  #region -- SET_APP_CONFIG (STKA) --
  /// <summary>
  /// SET_APP_CONFIG (STKA) (v 1.1, page 141)
  /// 
  /// The SET_APP_CONFIG message is used by the server application to set the 
  /// primary state machine parameters in the TrimTrac Pro.  When received the 
  /// TrimTrac Pro will send a RESP_APP_CONFIG (page 163) message in response. 
  /// 
  /// FORMAT: >STKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGTTHIJK;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_STKA : TT_OTA_CMD
  {
    [DataMember]
    public APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKA() { }
    internal TT_STKA(string unitID, string pwd, APP_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_CONTROL_OUTPUT (STKU) --
  /// <summary>
  /// SET_CONTROL_OUTPUT (STKU) (v 1.1, page 142)
  /// 
  /// This message is sent by the server to set the Control Module Output pins to the 
  /// desired state.  When received the device will send a RESP_CONTROL_OUTPUT 
  /// (page 174) message. 
  /// 
  /// FORMAT: >STKABCDDDDDDDDDDDDDDDD;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_STKU : TT_OTA_CMD
  {
    [DataMember]
    public CONTROL_OUTPUT Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKU() { }
    internal TT_STKU(string unitID, string pwd, CONTROL_OUTPUT data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_EXT_APP_CONFIG (STKX) --
  /// <summary>
  /// SET_EXT_APP_CONFIG (STKX) (v 1.1, page 143)
  /// 
  /// This message is used to set the Extended Application parameters in the TrimTrac 
  /// Pro.  When received the TrimTrac Pro will send a RESP_EXT_APP_CONFIG 
  /// (page 164) message in response. 
  /// 
  /// FORMAT: >STKABCCCCCCDEFFFFFFGGGGGGHIJJJKKKLLMMNNO;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_STKX : TT_OTA_CMD
  {
    [DataMember]
    public EXT_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKX() { }
    internal TT_STKX(string unitID, string pwd, EXT_APP_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_EXT2_APP_CONFIG (STKZ) --
  /// <summary>
  /// SET_EXT2_APP_CONFIG (STKZ) (v1.1 page 145)
  /// 
  /// This message is used to set the Extended 2 Application parameters in the 
  /// TrimTrac Pro.  When received the TrimTrac Pro will send a 
  /// RESP_EXT2_APP_CONFIG (page 166) message in response. 
  /// 
  /// >STKABBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQUVVVVVVXXXXXXXXXXXXXXXXXXXXXXX;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_STKZ : TT_OTA_CMD
  {
    [DataMember]
    public EXT2_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKZ() { }
    internal TT_STKZ(string unitID, string pwd, EXT2_APP_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_GEOFENCE_CONFIG (STKK) --
  /// <summary>
  /// SET_GEOFENCE_CONFIG (STKK) (v 1.1, page 147)
  /// 
  /// This message is used to set up a Geofence in the TrimTrac1.5 device.  When 
  /// received the TrimTrac Pro device will send a RESP_GEOFENCE_CONFIG 
  /// (page 168) message in response.  Please note that all geofences must be the same 
  /// type (Boundary Crossing, Exclusive, Inclusive) as determined by either 
  /// SET_EXT2_APP_CONFIG (page 145) or AT+CTKZ (page 127). 
  /// 
  /// FORMAT: >STKABBCDEEEEEFFFFFGGGHHHHHHHIIIIJJJJJJJ;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_STKK : TT_OTA_CMD
  {
    [DataMember]
    public GEOFENCE_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKK() { }
    internal TT_STKK(string unitID, string pwd, GEOFENCE_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_GPRS_CONNECT_CONFIG (STKF) --
  /// <summary>
  /// SET_GPRS_CONNECT_CONFIG (STKF) (v 1.1, page 148)
  /// 
  /// This message is used to set the GPRS Connection Configuration Values in the 
  /// TrimTrac Pro device.  When received the TrimTrac Pro device will send a 
  /// RESP_GPRS_CONNECT_CONFIG (page 170) message in response. 
  /// 
  /// FORMAT: >STKABCDDDDDEEEEEFFFFFFFFFFFFFFFFFFFFF”;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_STKF : TT_OTA_CMD
  {
    [DataMember]
    public GPRS_CONNECT_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKF() { }
    internal TT_STKF(string unitID, string pwd, GPRS_CONNECT_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_GPRS_SETUP_CONFIG (STKJ) --
  /// <summary>
  /// SET_GPRS_SETUP_CONFIG (STKJ) (v 1.1, page 149)
  /// 
  /// This message is used to set the GPRS Setup Configuration Values in the 
  /// TrimTrac Pro device.   All GPRS setup provisioning parameters are provided by 
  /// the GPRS service provider.  When received the TrimTrac Pro device will send a 
  /// RESP_GPRS_SETUP_CONFIG (page 170) message in response. 
  /// 
  /// FORMAT: >STKABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB”CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC”DDDDDDDDDDDDDDDDDDDD”;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_STKJ : TT_OTA_CMD
  {
    [DataMember]
    public GPRS_SETUP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKJ() { }
    internal TT_STKJ(string unitID, string pwd, GPRS_SETUP_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_GPS_CONFIG (STKG) --
  /// <summary>
  /// SET_GPS_CONFIG (STKG) (v 1.1, page 150)
  /// 
  /// The SET_GPS_CONFIG message is used by the server application to set the 
  /// TrimTrac GPS parameter values in the TrimTrac Pro.  When received the 
  /// TrimTrac Pro will send a RESP_GPS_CONFIG (page 150) message in response.  
  /// Please note that these GPS configuration parameters will seldom need to be 
  /// changed from default values.  It is NOT recommended that they be changed 
  /// without first consulting with your Trimble representative. 
  /// 
  /// FORMAT: >STKABBCCCDDDEEEF;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_STKG : TT_OTA_CMD
  {
    [DataMember]
    public GPS_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKG() { }
    internal TT_STKG(string unitID, string pwd, GPS_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_MODULE_APP_CONFIG (STKY) --
  /// <summary>
  /// SET_MODULE_APP_CONFIG (STKY) (v 1.1, page 150)
  /// 
  /// This message is used by the server to set the Application Parameter Values 
  /// associated with the Vehicle Adapter and Control Modules.  When received the 
  /// TrimTrac Pro will send a RESP_MODULE_APP_CONFIG (page 173) message 
  /// in response. 
  /// 
  /// >STKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGGGGHHHHHHIIIJJJKKKLMN;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_STKY : TT_OTA_CMD
  {
    [DataMember]
    public MODULE_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKY() { }
    internal TT_STKY(string unitID, string pwd, MODULE_APP_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion

  #region -- SET_PROV_CONFIG (STKV) --
  /// <summary>
  /// SET_PROV_CONFIG (STKV) (v 1.1, page 151)
  /// 
  /// The SET_PROV_CONFIG message is used by the server application to set the 
  /// SMS Communication values in the TrimTrac Pro.  When received by the 
  /// TrimTrac Pro, the unit will send a RESP_PROV_CONFIG (page 171) message in
  /// response. 
  /// 
  /// >STKABBBBBBBBBBBBBBBBBBBBBBBB;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_STKP : TT_OTA_CMD
  {
    [DataMember]
    public PROV_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }

    public TT_STKP() { }
    internal TT_STKP(string unitID, string pwd, PROV_CONFIG data) : base(unitID, pwd) { this.Data = data; }
  }
  #endregion
  #endregion

  #region -- QRK* --
  #region -- QUERY_CONFIG (QTK[A|F|G|I|J|T|V|U|X|Y|Z]) --
  /// <summary>
  /// QUERY_CONFIG (QTK[A|F|G|I|J|V|T|U|X|Y|Z]) (v1.1, page 154)
  /// 
  /// This QUERY_CONFIG message is used by the server application to request the 
  /// TrimTrac Pro send its TrimTrac Application, GPS or Communication parameters 
  /// in the appropriate response message as indicated below. 
  /// 
  /// >QTKA;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTK_ : TT_OTA_CMD
  {
    public enum QUERY_CONFIG
    {
      ApplicationParameters, // (RTKA page 163) 
      GPRSConnectionParameters, // (RTKF page 169) 
      GPSParameters, // (RTKG page170) 
      GPRSSetupParameters, // (RTKJ page 170) 
      ProvisioningParameters, // (RTKP page 171) 
      ExtendedApplicationPara, // (RTKX page 163) 
      ModuleApplicationPara, // (RTKY page 173) 
      Extended2ApplicationPara, // (RTKZ page 166) 
      VAMOutput, // (RTKU)
      DeviceIdentification, // (RTKI)
      Firwmare, // (RTKT)
    }

    [DataMember]
    public QUERY_CONFIG Config;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- QUERY_CONTROL_OUTPUT (QTKU) --
  /// <summary>
  /// QUERY_CONTROL_OUTPUT (QTKU) (v1.1, page 154)
  /// 
  /// This message is sent by the server to request the current state of the Control 
  /// Module Outputs.  TrimTrac Pro responds by sending 
  /// RESP_CONTROL_OUTPUT (page 174) message. 
  /// 
  /// >QTKA;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTKU : TT_OTA_CMD
  {
    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- QUERY_GEOFENCE_CONFIG (QTKK) --
  /// <summary>
  /// QUERY_GEOFENCE_CONFIG (QTKK) (v1.1, page 155)
  /// 
  /// This QUERY_GEOFENCE_CONFIG message is used by the server application 
  /// to request the TrimTrac Pro send its geofence parameters in the appropriate 
  /// RESP_GEOFENCE_CONFIG (page 168) message. 
  /// 
  /// >QTKABB;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTKK : TT_OTA_CMD
  {
    [DataMember]
    public int GeofenceID;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- QUERY_LOG (QTKR) --
  /// <summary>
  /// QUERY_LOG (QTKR) (v1.1, page 156)
  /// 
  /// This message is used by the server application to request logged REPORT_POS 
  /// or STATUS_MSG messages that may not have been received at the server 
  /// application.  The TrimTrac Pro will send the corresponding messages to the 
  /// server application in response to this message at the rate of one REPORT_POS or 
  /// STATUS_MSG per message.  At the end of the messages a RESP_QUERY_LOG 
  /// or RESP_QUERY_AGGR is sent.  The TrimTrac Pro log contains 1,024 of the 
  /// most recent messages.  Care should be taken when structuring the QUERY_LOG 
  /// message.  Querying the entire log, for instance, will result in the transmission of 
  /// all 1,024 logged messages. 
  /// 
  /// >QTKABBBBCCCC[DE[FGHIJJJK[LLLLMMMMMM[NNNNOOOOOO]]]];PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTKR : TT_OTA_CMD
  {

    /// <summary>
    /// Beginning Protocol Sequence Number
    /// 16-bit Hex (0000-FFFF) 
    /// incrementing by 1 for each logged report
    /// </summary>
    [DataMember]
    public int BeginningProtocolSequenceNumber;

    /// <summary>
    /// Ending Protocol Sequence Number
    /// 16-bit Hex (0000-FFFF) 
    /// incrementing by 1 for each logged report
    /// </summary>
    [DataMember]
    public int EndingProtocolSequenceNumber;

    /// <summary>
    /// Aggregate Log Reporting Flag: 
    ///     ‘T’ = enabled, ‘F’ = disabled.  
    /// When enabled, aggregate reports may be returned.
    /// </summary>
    [DataMember]
    public EFlag? AggregateLogReportingFlag;

    /// <summary>
    /// Stop RESP_QUERY_LOG message from being sent: ‘T’ = do not send, ‘F’ = send.
    /// Used to stop the unit from fulfilling previous QUERY_LOG requests. 
    /// </summary>
    [DataMember]
    public EStopRESP_QUERY_LOG? StopRESP_QUERY_LOG;

    /// <summary>
    /// Filter 1: 'Z' = All, 'U' = Unsent only.  
    /// </summary>
    [DataMember]
    public EFilter? Filter1;

    /// <summary>
    /// Filter 2: 'Z' = All, 'P' = Position only, 'S' = Status only. 
    /// </summary>
    [DataMember]
    public EFilter? Filter2;

    /// <summary>
    /// Filter 3: 'Z' = All, 'A' = Alert only 
    /// </summary>
    [DataMember]
    public EFilter? Filter3;

    /// <summary>
    /// Time range: 'Z' = Unused, 'N' = Newest, 'O' = Oldest 
    /// </summary>
    [DataMember]
    public ETimeRange? TimeRange;

    /// <summary>
    /// Maximum Number of Messages to be sent in response to the current 
    /// QUERY_LOG message. If non-zero then this limits the total number of 
    /// message that can be sent from the log.  If zero then the number of messages 
    /// sent from the log limit is 1,024 messages.
    /// Applicable if Time range is not ‘Z’.
    /// </summary>
    [DataMember]
    public int? MaximumNumberOfMessages;

    /// <summary>
    /// Last Message: ‘1’ = automatically include the last message in 
    /// the log regardless of the filter settings, ‘0’ = don’t automatically 
    /// include the last message in the log. 
    /// </summary>
    [DataMember]
    public ELastMessage? LastMessage;

    /// <summary>
    /// GPS week number of starting date 
    /// </summary>
    [DataMember]
    public int? GPSStartingDateWeek;

    /// <summary>
    /// GPS seconds into week of starting date. 
    /// </summary>
    [DataMember]
    public int? GPSStartingDateSeconds;

    /// <summary>
    /// GPS week number of ending date. 'Most recent' if not present. 
    /// </summary>
    [DataMember]
    public int? GPSEndingDateWeek;

    /// <summary>
    /// GPS seconds into week of ending date. 'Most recent' if not 
    /// present. 
    /// </summary>
    [DataMember]
    public int? GPSEndingDateSeconds;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- QUERY_METERS (QTKM) --
  /// <summary>
  /// QUERY_METERS (QTKM) (v1.1, page 157)
  /// 
  /// This message is sent by the server to request the current state of the runtime 
  /// meters (See SET_EXT2_APP_CONFIG for runtime meter setup instructions).
  /// The meters can also be individually cleared via this message.  TrimTrac Pro 
  /// responds by sending a RESP_METERS (page 171) message. 
  /// 
  /// >QTKABC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTKM : TT_OTA_CMD
  {
    [DataMember]
    public EReportFlag RuntimeMotionBasedQuery = EReportFlag.ReportOnly_NoReset;

    [DataMember]
    public EReportFlag RuntimeLPABasedQuery = EReportFlag.ReportOnly_NoReset;

    public override string ToString() { return TTParser.ToString(this); }
  }

  // QUERY_METERS2 – This message is sent by the server to request the current state 
  // of the second set of runtime meters.  The meters can also be individually cleared 
  // via this message.
  // >QTKNBC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_QTKN : TT_OTA_CMD
  {
    // B	Reset Odometer Flag: ‘0’ – No Reset, ‘1’ – Reset Odometer.
    [DataMember]
    public EReportFlag ResetOdometerFlag;

    // C	Reset Engine Idle Runtime Meter Flag: ‘0’ – No Reset, ‘1’ – Reset Engine Idle Runtime Meter.
    [DataMember]
    public EReportFlag ResetEngineIdleRuntimeMeterFlag;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- QUERY_POSITION (QTKD) --
  /// <summary>
  /// QUERY_POSITION (QTKD) (v1.1, page 157)
  /// 
  /// This message is used to command the TrimTrac Pro to report either its current or 
  /// its most recently logged position depending upon how the Position Query Mode is 
  /// set in the QUERY_POSITION message as shown in Figure 24 on page 158. 
  /// More specifically, the unit will compute and report a NEW position fix when 
  /// queried if and only if the Position Query Mode is set: 
  /// 1.  “P=Compute New Position Fix”, OR  
  /// 2.  “S=Compute if Position Aged” AND there has been motion since the 
  /// device last exited the FIX State (i.e. Position Age has become set 
  /// “1=Aged”).  
  /// If either of the foregoing settings and conditions are true and the device was able 
  /// to compute a new position fix as requested, then the unit will respond to the 
  /// QUERY_POSITION message by computing a new position fix and sending it in a 
  /// new REPORT_POS message (with TriggerType set to “3=Query”).   
  /// If  the TrimTrac Pro was unable to compute a new position fix prior to expiration 
  /// of the GPS Fix Timeout specified in the QUERY_POSITION message, then a 
  /// new STATUS_MSG will be sent in response.  This new STATUS_MSG will 
  /// have the TriggerType set “3=Query”, GPS Status Code set to some value other 
  /// than "0=3D Fix, 1=2D Fix or 7=No Fix Attempted" and the Position Age flag will 
  /// be set “1=Aged”.  No other position information will be provided with the 
  /// response. 
  /// If either of the foregoing settings and conditions are true, or if Position Query 
  /// Mode is set “L=Status Report with Last Logged Position”, the unit will send a  
  /// new STATUS_MSG and the most recently logged REPORT_POS (in a single 
  /// SMS message if Communication Mode is set “0=SMS”) if:  
  /// 1. QUERY_POSITION has the Position Query Mode set “L=Status 
  ///    Report with Last Logged Position”; OR 
  /// 2. QUERY_POSITION has the Position Query Mode set “S= Compute if 
  ///    Position Aged”, but there has been no motion since the last logged 
  ///    position (i.e. Position Aged is set “0=Current”); OR 
  /// 3. No GPS fix was achieved prior to expiration of the GPS Fix Timeout 
  ///    specified in the QUERY_POSITION message and: 
  ///    a.  QUERY_POSITION has the Position Query Mode set “S= 
  ///        Compute if Position Aged” and there has been motion since the 
  ///        last logged position (i.e. Position Aged is set “1=Aged”); OR 
  ///    b.  QUERY_POSITION Position Query Mode is set “P=Compute 
  ///        New Position Fix”. 
  /// The new STATUS_MSG will have the TriggerType set to “3=Query” and the 
  /// Position Aged flag will be set ‘1’=Aged if there has been motion since the last 
  /// logged position; else the Position Aged flag will be set “1=Current”. 
  /// 
  /// >QTKABCCCC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_QTKD : TT_OTA_CMD
  {
    public enum EPositionQueryMode
    {
      ComputeIfPositionAged, // 'S'
      ComputeNewPositionFix, // 'P'
      StatusReportWithLastLoggedPosition, // 'L' 
    }

    [DataMember]
    public EPositionQueryMode PositionQueryMode;

    /// <summary>
    /// Position Query Fix Timeout
    /// In Seconds (10-3600)
    /// Note: Temporarily overrides current FIX Timeout, T2, if Position
    /// Query Mode set to “S=Compute” and report current position.
    /// Settings longer than 600 minutes are generally not 
    /// recommended.
    /// No default value.
    /// </summary>
    [DataMember]
    public int PositionQueryFixTimeout;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion
  #endregion

  #region -- RTK* --
  #region -- RESP_ALERT_STATE (RTKL) --
  /// <summary>
  /// RESP_ALERT_STATE (RTKL) (v1.1 page 162)
  /// 
  /// The TrimTrac Pro sends this message after a SET_ALERT_STATE (page 140) 
  /// request is received and processed; provided, however, that the TrimTrac Pro is 
  /// connected to a Vehicle Adapter or Control Module.  If battery powered, then this 
  /// message will not be sent upon receipt of a SET_ALERT_STATE request. 
  /// 
  /// FORMAT: >RTKABCD;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKL : TT_OTA_RSP
  {
    [DataMember]
    public ALERT_STATE Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_APP_CONFIG (RTKA) --
  /// <summary>
  /// RESP_APP_CONFIG (RTKA) (v 1.1, page 163)
  /// 
  /// This is the response message to the SET_APP_CONFIG (page 141) and 
  /// QUERY_CONFIG (page 154) messages containing the Application Parameter 
  /// values.  
  /// 
  /// FORMAT: >RTKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGTTHIJKLLLLMMMMMMNPP;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_RTKA : TT_OTA_RSP
  {
    [DataMember]
    public APP_CONFIG Data;

    /// <summary>
    /// Battery Change Week: GPS Week Number, always 4 digits
    /// </summary>
    [DataMember]
    public int BatteryChangeWeek;

    /// <summary>
    /// Battery Change Time: Seconds into GPS Week, always 6 digits
    /// </summary>
    [DataMember]
    public int BatteryChangeTime;

    /// <summary>
    /// Firmware Version:  Numeric String
    /// </summary>
    [DataMember]
    public Version FirmwareVersion; 

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_EXT_APP_CONFIG (RTKX) --
  /// <summary>
  /// RESP_EXT_APP_CONFIG (RTKX) (v1.1 page 164)
  /// 
  /// This is the response message to the SET_EXT_APP_CONFIG (page 143) and 
  /// QUERY_CONFIG (page 154) message containing the Extended Application 
  /// Parameter values. 
  /// 
  /// >RTKABCCCCCCDEFFFFFFGGGGGGHIJJJKKKLLMMNNO;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKX : TT_OTA_RSP
  {
    [DataMember]
    public EXT_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_EXT2_APP_CONFIG (RTKZ) --
  /// <summary>
  /// RESP_EXT2_APP_CONFIG (RTKZ) (v1.1 page 166)
  /// 
  /// This is the response message to the SET_EXT2_APP_CONFIG (page 145) and 
  /// QUERY_CONFIG (page 154) message containing the Extended 2 Application 
  /// Parameter values. 
  /// 
  /// >RTKABBBBCDDDDDEEEEEFGHIJJJKKKLMNNNOQQQUVVVVVVXXXXXXXXXXXXXXXXXXXXXXX;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_RTKZ : TT_OTA_RSP
  {
    [DataMember]
    public EXT2_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_GEOFENCE_CONFIG (RTKK) --
  /// <summary>
  /// RESP_GEOFENCE_CONFIG (RTKK) (v 1.1, page 168)
  /// 
  /// This is the response message to the SET_GEOFENCE _CONFIG (page 147) and 
  /// QUERY_GEOFENCE_CONFIG (page 155) message containing the Geofence 
  /// Parameter values.  It is also sent with any REPORT_POS and STATUS_MSG 
  /// having  TriggerType set “2=Exception Report Alert” and  Geofence Status set 
  /// “1=Violation” or “5=New LPA-based Geofence”, except when some other new 
  /// event, such as a Speed Violation, has triggered the REPORT_POS OR 
  /// STATUS_MSG.  The RESP_GEOFENCE_CONFIG message sent with either a 
  /// REPORT_POS OR STATUS_MSG will be for the geofence most recently 
  /// violated or boundary most recently crossed. 
  /// 
  /// FORMAT: >RTKABBCDEEEEEFFFFFGGGHHHHHHHIIIIJJJJJJJ;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKK : TT_OTA_RSP
  {
    [DataMember]
    public GEOFENCE_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_GPRS_CONNECT_CONFIG (RTKF) --
  /// <summary>
  /// RESP_GPRS_CONNECT_CONFIG (RTKF) (v 1.1, page 169)
  /// 
  /// This is the response message to the SET_GPRS_CONNECT _CONFIG (page 
  /// 148) and QUERY_CONFIG (page 169) message containing the GPRS 
  /// Connection Parameter values. 
  /// 
  /// FORMAT: >RTKABCDDDDDEEEEEFFFFFFFFFFFFFFFFFFFFF”;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKF : TT_OTA_RSP
  {
    [DataMember]
    public GPRS_CONNECT_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_GPRS_SETUP_CONFIG (RTKJ) --
  /// <summary>
  /// RESP_GPRS_SETUP_CONFIG (RTKJ) (v 1.1, page 170)
  /// 
  /// This is the response message to the SET_GPRS_SETUP _CONFIG (page 149) 
  /// and QUERY_CONFIG (page 170) message containing the GPRS Setup 
  /// Parameter values. 
  /// 
  /// FORMAT: >RTKABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB”CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC”DDDDDDDDDDDDDDDDDDDD”;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKJ : TT_OTA_RSP
  {
    [DataMember]
    public GPRS_SETUP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_GPS_CONFIG (RTKG) --
  /// <summary>
  /// RESP_GPS_CONFIG (RTKG) (v 1.1, page 170)
  /// 
  /// This is the response message to the SET_GPS_CONFIG (page 150) and 
  /// QUERY_CONFIG (page 170) message containing the GPS Configuration. 
  /// 
  /// FORMAT: >RTKABBCCCDDDEEEF;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_RTKG : TT_OTA_RSP
  {
    [DataMember]
    public GPS_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_PROV_CONFIG (RTKV) --
  /// <summary>
  /// RESP_PROV_CONFIG (RTKV) (v 1.1, page 171)
  /// 
  /// This is the response message to the SET_PROV_CONFIG (page 151) and
  /// QUERY_CONFIG (page 154) message containing the provisioning information.
  /// 
  /// >RTKABBBBBBBBBBBBBBBBBBBBBBBB;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKV : TT_OTA_RSP
  {
    [DataMember]
    public PROV_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_MODULE_APP (RTKY) --
  /// <summary>
  /// RESP_MODULE_APP (RTKY) (v 1.1, page 173)
  /// 
  /// This is the by the TrimTrac Pro in response to the 
  /// SET_MODULE_APP_CONFIG (page 150) and QUERY_CONFIG (page 154) 
  /// message. 
  /// 
  /// >RTKABBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGGGGHHHHHHIIIJJJKKKLMN;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKY : TT_OTA_RSP
  {
    [DataMember]
    public MODULE_APP_CONFIG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_CONTROL_OUTPUT (RTKU) --
  /// <summary>
  /// RESP_CONTROL_OUTPUT (RTKU) (v 1.1, page 174)
  /// 
  /// This is the by the TrimTrac Pro in response to the SET_CONTROL_OUTPUT 
  /// (page 142)or QUERY_CONTROL_OUTPUT (page 154) messages.  
  /// 
  /// FORMAT: >RTKABCDDDDDDDDDDDDDDDD;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKU : TT_OTA_RSP
  {
    [DataMember]
    public CONTROL_OUTPUT Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- REPORT_POS and STATUS_MSG (RTK[S|P]) --
  /// <summary>
  /// REPORT_POS and STATUS_MSG (RTK[S|P]) (v1.1, page 160)
  /// 
  /// These two messages are the most common.  The difference between the 
  /// REPORT_POS and STATUS_MSG is that the REPORT_POS message contains 
  /// position information. STATUS_MSG does not contain any position. 
  /// 
  /// >RTKABBBBCDDDEFFFFGGGGGGSTVOPQRWXabU[HHHIIIIIIIJJJJKKKKKKKLLLLLLMMMNNN];ID=YYYYYYYY;*ZZ< 
  /// 
  /// Each new outbound REPORT_POS and STATUS_MSG message is given a 
  /// Report Sequence number.  The sequence number is 16 bits and increments by one 
  /// with each message created and rolls over to 0000 once the maximum 16-bit value 
  /// of FFFF is reached.  The TrimTrac Application saves the message in non-volatile 
  /// memory each time one is created; this is called the Message Log.  The Message 
  /// Log is a FIFO log of the last 1,024 REPORT_POS or STATUS_MSG messages 
  /// (whether transmitted or only logged) such that when it is full the oldest one is 
  /// deleted.  The Message Log can be queried with the QUERY_LOG message from 
  /// the server application. 
  /// </summary>
  [DataContract]
  [KnownType(typeof(TT_RTKP))]
  public partial class TT_RTKS : TT_OTA_RSP
  {
    [DataMember]
    public STATUS_MSG Data;

    public override string ToString() { return TTParser.ToString(this); }
  }

  [DataContract]
  public partial class TT_RTKP : TT_RTKS
  {
    [DataMember]
    public REPORT_POS PositionData;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_METERS (RTKM) --
  /// <summary>
  /// RESP_METERS (RTKM) (v1.1, page 171)
  /// 
  /// This is the response message to the QUERY_METERS (page 157) message.  This 
  /// message is also sent whenever either Runtime meter reaches it automatic report 
  /// threshold, if so enabled.  (See SET_EXT2_APP_CONFIG page 145 for runtime 
  /// meter setup instructions). 
  /// 
  /// >RTKABCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKM : TT_OTA_RSP
  {
    [DataMember]
    public EReportFlag RuntimeMotionBasedResetConfirmation;

    [DataMember]
    public EReportFlag RuntimeLPABasedResetConfirmation;

    /// <summary>
    /// Runtime Motion-based Reading: Accumulated seconds since last reset
    /// always 10 digits 
    /// </summary>
    [DataMember]
    public int RuntimeMotionBasedReading;

    /// <summary>
    /// Runtime LPA-based Reading: Accumulated seconds since last reset
    /// always 10 digits 
    /// </summary>
    [DataMember]
    public int RuntimeLPABasedReading;

    public override string ToString() { return TTParser.ToString(this); }
  }

  /// <summary>
  /// >RTKABCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ<
  /// </summary>
  [DataContract]
  public partial class TT_RTKN : TT_OTA_RSP
  {
    // B	Odometer Meter Flag: ‘0’ – Report without Reset, ‘1’ – Report with Reset, ‘2’ – Odometer not enabled.
    [DataMember]
    public EMeterFlag OdometerMeterFlag;

    // C	Engine Idle Runtime Meter Flag: ‘0’ – Report without Reset, ‘1’ – Report with Reset, ‘2’ – Engine Idle Runtime Meter not enabled.
    [DataMember]
    public EMeterFlag EngineIdleRuntimeMeterFlag;

    // DDDDDDDDDD	Current Odometer value (1/10th mile).
    [DataMember]
    public int CurrentOdometer;

    // EEEEEEEEEE	Current Engine Idle Runtime Meter value (seconds).
    [DataMember]
    public int CurrentEngineIdleRuntimeMeter;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_QUERY_AGGR (RTK[1|2|3|4]) --
  /// <summary>
  /// RESP_QUERY_AGGR (RTK[1|2|3|4]) (v1.1, page 172)
  /// 
  /// The TrimTrac Pro sends this message after a QUERY_LOG (page 156) request is 
  /// received and processed, and if Aggregate Log Reporting Flag is ‘enabled’. It will 
  /// contain messages in aggregated format as shown below. The entire message can 
  /// be no more than 160-byte SMS message. 
  /// 
  /// >RTKA{B};ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKx : TT_OTA_RSP
  {
    [DataMember]
    public int ReportIndex;

    [DataMember]
    public string Message;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- RESP_QUERY_LOG (RTKR) --
  /// <summary>
  /// RESP_QUERY_LOG (RTKR) (v1.1, page 172)
  /// 
  /// This message is sent after a QUERY_LOG (page 156) request is received and 
  /// processed.  It will contain the number of messages actually retrieved from the 
  /// Message Log and sent to the server application. 
  /// 
  /// >RTKABBBB;ID=YYYYYYYY;*ZZ< 
  /// </summary>
  [DataContract]
  public partial class TT_RTKR : TT_OTA_RSP
  {
    /// <summary>
    /// Number of messages sent from the log up to a maximum of 1,024 messages. 
    /// </summary>
    [DataMember]
    public int NumberOfMessangesSent;

    public override string ToString() { return TTParser.ToString(this); }
  }

  #endregion
  #endregion

#if false
  #region -- VAM --
  // SET_VAM_APP_CONFIG – This message will be used by the server to set the VAM-only Application Parameter Values in the TrimTrac-1.5 device.  When received the TrimTrac-1.5 device will send a RESP_VAM_APP_CONFIG message in response. The message is ignored on non-VAM TrimTrac-1.5 device.
  // >STKYBBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGGGGHHHHHHIIIJJJKKKLMN;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_STKY : TT_OTA_CMD
  {
    [DataMember]
    public VAM_APP_CONFIG Data;

    public override string ToString() { return Parser.ToString(this); }
  }

  // RESP_VAM_APP_CONFIG – This is the response message to the SET_VAM_APP_CONFIG and QUERY_CONFIG message containing the VAM-only Application Parameter values.
  // >RTKYBBBBBBCCCCCCDDDDDDEEEEEEFFFFFFGGGGGGHHHHHHIIIJJJKKKLMN;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_RTKY : TT_OTA_RSP
  {
    [DataMember]
    public VAM_APP_CONFIG Data;

    public override string ToString() { return Parser.ToString(this); }
  }

  // SET_VAM_OUTPUT – This message is sent by the server to set the VAM Output pins to the desired state.  When received the device will send a RESP_VAM_OUTPUT message.
  // >STKUBCDDDDDDDDDDDDDDDD;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_STKU : TT_OTA_CMD
  {
    [DataMember]
    public VAM_OUTPUT Data;

    public override string ToString() { return Parser.ToString(this); }
  }

  // RESP_VAM_OUTPUT – This message is sent in response to the SET_VAM_OUTPUT message.
  // >RTKUBCDDDDDDDDDDDDDDDD;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_RTKU : TT_OTA_RSP
  {
    [DataMember]
    public VAM_OUTPUT Data;

    public override string ToString() { return Parser.ToString(this); }
  }
  #endregion
#endif

  #region -- IDENT --
  // SET_IDENT – This message is sent by the server to set the current value of the Device Identification parameters.  A successful change of the Device ID will cause a GPRS session to be restarted.
  // >STKIBBBBBBBBCCCCCCCC;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_STKI : TT_OTA_CMD
  {
    [DataMember]
    public IDENT Data;

    public override string ToString() { return TTParser.ToString(this); }
  }

  // RESP_IDENT – This message is sent in response to the QUERY_IDENT message.
  // >RTKIBCDDDDDDDDDDEEEEEEEEEE;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_RTKI : TT_OTA_RSP
  {
    [DataMember]
    public IDENT Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion

  #region -- OTA_FIRWMARE --
  // >STKTBFFFFFFFFCCCCCCCCDDDDDDDDDDDDDDD”EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE”;PW=PPPPPPPP;ID=YYYYYYYY;*ZZ<
  [DataContract]
  public partial class TT_STKT : TT_OTA_CMD
  {
    // B	OTA Firmware Upgrade Command: 0 = Stop, 1 = Start OTA
    [DataMember]
    public int OTAFirmwareUpgradeCommand;

    [DataMember]
    public OTA_FIRWMARE Data;

    public override string ToString() { return TTParser.ToString(this); }
  }

  // >RTKTBFFFFFFFFCCCCCCCCCCDDDDDDDDDDDDDDD”EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE”;ID=YYYYYYYY;*ZZ<
  [DataContract, KnownType(typeof(EOTAFirmwareUpgradeState))]
  public partial class TT_RTKT : TT_OTA_RSP
  {
    // B	OTA Firmware Upgrade State (in hex):
    [DataMember]
    public int OTAFirmwareUpgradeState;

    [DataMember]
    public OTA_FIRWMARE Data;

    public override string ToString() { return TTParser.ToString(this); }
  }
  #endregion
}
