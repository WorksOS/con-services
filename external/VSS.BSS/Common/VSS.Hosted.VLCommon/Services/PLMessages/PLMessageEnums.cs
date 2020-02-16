using System.Runtime.Serialization;
using VSS.Hosted.VLCommon.ICD;

namespace VSS.Hosted.VLCommon
{
  public enum PLTransactionTypeEnum
  {
    Unknown = 0x00,
    OTAConfigMessages = 0x01,
    FenceConfig = 0x02,
    RegistrationMessage0x31 = 0x31,
    FaultEvent = 0x34,
    FaultDiagnostic = 0x35,
    CumulativesMessage = 0x42,
    RegistrationMessage = 0x50,
    Administration = 0x52,
    Status0x60 = 0x60,
    MachineSecurity = 0x53
  }

  [DataContract]
  public enum EventFrequency
  {
    [EnumMember]
    Unknown = 0,
    [EnumMember]
    Immediately = 1,
    [EnumMember]
    Next = 2,
    [EnumMember]
    Never = 3,
  }

  [DataContract]
  public enum InputConfig
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

  [DataContract]
  public enum SMUFuelReporting
  {
    [EnumMember]
    Off = 0x00,
    [EnumMember]
    Fuel = 0x01,
    [EnumMember]
    SMU = 0x02,
    [EnumMember]
    SMUFUEL = 0x03,
    [EnumMember]
    PL321VIMSFuel = 0x40,
    [EnumMember]
    PL321VIMSSMU = 0x2F,
  }

  [DataContract]
  public enum PL321VIMSSMUFuelStatus
  {
    [EnumMember]
    Enable = 0x01,
    [EnumMember]
    Disable = 0x00,
  }

  public enum DigitalInputMonitoringConditions
  {
    Always = 0x028C,
    KeyOffEngineOff = 0x028D,
    KeyOnEngineOff = 0x028E,
    KeyOnEngineOn = 0x028F,
  }

  [DataContract]
  public enum FieldID
  {
    [EnumMember]
    EventInterval = 0x0B,
    [EnumMember]
    Level1TransmissionFrequency = 0x0C,
    [EnumMember]
    Level2TransmissionFrequency = 0x0D,
    [EnumMember]
    Level3TransmissionFrequency = 0x0E,
    [EnumMember]
    NextMessageInterval = 0x0F,
    [EnumMember]
    GlobalGramEnable = 0x14,
    [EnumMember]
    ReportStartTime = 0x16,
    [EnumMember]
    DigitalInput1Config = 0x18,
    [EnumMember]
    DigitalInput1DelayTime = 0x19,
    [EnumMember]
    DigitalInput1Description = 0x1B,
    [EnumMember]
    DigitalInput2Config = 0x1C,
    [EnumMember]
    DigitalInput2DelayTime = 0x1D,
    [EnumMember]
    DigitalInput2Description = 0x1F,
    [EnumMember]
    DigitalInput3Config = 0x20,
    [EnumMember]
    DigitalInput3DelayTime = 0x21,
    [EnumMember]
    DigitalInput3Description = 0x23,
    [EnumMember]
    DigitalInput4Config = 0x24,
    [EnumMember]
    DigitalInput4DelayTime = 0x25,
    [EnumMember]
    DigitalInput4Description = 0x27,
    [EnumMember]
    DiagnosticTransmissionFrequency = 0x28,
    [EnumMember]
    SMU = 0X29,
    [EnumMember]
    PL121PositionReporting = 0x2E,
    [EnumMember]
    PL121SMUReporting = 0x2F,
    [EnumMember]
    SMUFuelReporting = 0x2D,
    [EnumMember]
    PL321VIMSGatewaySMUFuelReporting = 0x40,
    [EnumMember]
    StartStopConfiguration = 0x30,
    [EnumMember]
    PositionReportConfiguration = 0x31,
    [EnumMember]
    DigitalInput1MonitoringCondition = 0x32,
    [EnumMember]
    DigitalInput2MonitoringCondition = 0x33,
    [EnumMember]
    DigitalInput3MonitoringCondition = 0x34,
    [EnumMember]
    DigitalInput4MonitoringCondition = 0x35,
    [EnumMember]
    MaintenanceMode = 0x36,
    [EnumMember]
    MaintenanceModeDurationTimer = 0x37,
    [EnumMember]
    MachineStartMode = 0x41,
    [EnumMember]
    TamperResistanceMode = 0x42,
    [EnumMember]
    NotUsed = 0xFF
  }

  public enum PLLocationTypeEnum
  {
    GPS = 1,
    Invalid = 5,
  }

  [DataContract]
  public enum ModuleTypeEnum
  {
    [EnumMember]
    Orbcomm201 = 1,
    [EnumMember]
    Orbcomm151 = 4,
    [EnumMember]
    PL121 = 5,
    [EnumMember]
    PL300 = 7,
  }

  public enum DataLinkConfig
  {
    CDLOnly = 0,
    CDLAndBDT = 1,
    ATAOnly = 2,
    ATAAndBDT = 3,
  }

  public enum MachineStartStatus
  {
    NotConfigured = -0x02,
    NoPending = -0x01,
    NormalOperation = 0x00,
    Derated = 0x01,
    Disabled = 0x02,
    NormalOperationPending = 0x10,
    DeratedPending = 0x11,
    DisabledPending = 0x12
  }

  public enum TamperResistanceStatus
  {
    NoPending = -0x01,
    Off = 0x00,
    TamperResistanceLevel1 = 0x01,
    TamperResistanceLevel2 = 0x02,
    TamperResistanceLevel3 = 0x03
  }

  public enum LeasedOwnershipType
  {
    NotSet = -1,
    Claim = 4,
    Released = 5
  }

  public enum ConfigurationSource
  {
    VisionLink = 0x00,
    ET = 0x01,
    PSPS = 0x02,
    WebServiceTool = 0x03
  }

  public enum MachineStartModeConfigurationSource
  {
    NeverConfigured = 0x00,
    OffBoardOfficeSystemVL = 0x01,
    CatElectronicsTechnician = 0x02
  }

  public enum TamperResistanceModeConfigurationSource
  {
    NeverConfigured = 0x00,
    OffBoardOfficeSystemVL = 0x01,
    CatElectronicsTechnician = 0x02    
  }

  public enum MachineSecurityMode
  {
    NotInstalled = 0x00,
    TamperResistanceOnly = 0x01,
    TamperResistanceAndMSSKey = 0x02,
    MSSKeyOnly = 0x03,
    AnotherMSSMasteronDataLink = 0x04,
    ImmobilizerCouldNotBeUninstalled = 0x05,
    OnlyLegacyImmobilizerDetected = 0x06,
    ImmobilizerRestrictionWithPowerDownRequired = 0x07,
    PreviousState = 0xFF
  }

  public enum MachineStartStatusTrigger
  {
    Unknown = 0x00,
    OTACommand = 0x01,
    OnBoardServiceToolCommand = 0x02,
    SimCardRemovedTimerExpired = 0x03,
    GPSAntennaDisconnectedTimerExpired = 0x04,
    GPSGSMLossofSignalTimerExpired = 0x05,
    GSMLossofSignalTimerExpired = 0x06,
    InvalidMSSKey = 0x07,
    TamperResolvedConnectiontoOffBoard = 0x08,
    TamperUninstalled = 0x09,
    ValidMSSKey = 0x0A
  }

  public enum SwitchState
  {
    NotInstalled = 0x00,
    NotConfigured = 0x01,
    NormallyOpen = 0x02,
    NormallyClosed = 0x04
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum MachineSecurityModeSetting
  {
    [EnumMember]
    NotConfigured = -0x02,
    [EnumMember]
    NoPending = -0x01,
    [EnumMember]
    NormalOperationWithMachineSecurityFeatureDisabled = 0x00,
    [EnumMember]
    NormalOperationWithMachineSecurityFeatureEnabled = 0x01,
    [EnumMember]
    Disabled = 0x02,
    [EnumMember]
    Derated = 0x03,
    [EnumMember]
    MachineInDisableModebutsecuritytamperedorbypass = 0x04,
    [EnumMember]
    MachineInDisableModebutPowercut = 0x05
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum DeviceSecurityModeReceivingStatus
  {
    [EnumMember]
    ModechangeRequestReceivedImplementationPending = 0x00,
    [EnumMember]
    ModechangeRequestImplemented = 0x01,
    [EnumMember]
    ModechangeRejectedAlreadyEnabled = 0x02,
    [EnumMember]
    ModechangeRejectedDuplicateRequest = 0x03,
    [EnumMember]
    ModestatusReportGenerated = 0x04
  }

  [DataContract(Namespace = Constants.OEMDATAFEEDNS)]
  public enum SourceSecurityModeConfiguration
  {
    [EnumMember]
    DefaultFirmware = 0x00,
    [EnumMember]
    VisionLink = 0x01,
    [EnumMember]
    TrimbleServiceTool = 0x02,
    [EnumMember]
    CATET = 0x03
  }

  public enum ReportingFrequencyInterval
  {
    Minute = 1,
    Hour = 2,
    Day = 3,
    Week = 4,
    Month = 5
  }
}
