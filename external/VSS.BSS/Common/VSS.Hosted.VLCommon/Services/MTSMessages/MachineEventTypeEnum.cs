using System;

namespace VSS.Hosted.VLCommon.MTSMessages
{
  [Flags]
  public enum MachineEventTypeEnum : ulong
  {
    Unknown = 0,
    PositionBlock00 = 1,
    EngineStartStopBlock01 = 2,
    ECMInfoBlock51 = 8,
    GatewayAdmin5300 = 16,
    MaintMode5301 = 32,
    AdminFailed53FF = 64,
    EventReporting21 = 128,
    DiagReporting22 = 256,
    FuelEngine4500 = 512,
    VehicleBusDiagnosticMessage = 1024,
    VehicleBusFuelEngineReport = 2048,
    PayloadAndCycleCountReport4503 = 4096,
    OperatorLogin4600 = 8192,
    IdleStartStop4601 = 16384,
    VehicleBusECMInformation = 32768,
    VehicleBusAddressClaim = 65536,
    OccupiedSites = 131072,
    MSSKeyID4605 = 262144,
    SMH3A = 524288,
    IgnitionOnOff = 1048576,
    DeviceInitializationAnnouncement = 2097152,
    J1939ProprietaryCNH = 4194304,
    DiscreteInput = 8388608,
    SpeedingIndication = 16777216,
    StoppedNotification = 33554432,
    SiteEntryExit = 67108864,
    MachineSecurity5302 = 134217728,
    TamperSecurityStatus = 268435456,
    VehicleBusJ1939ParametersReport = 536870912,
    VehicleBusJ1939StatisticsReport = 1073741824,
    GensetStatusBlock = 2147483648,
    VehicleBusTPMSReport = 4294967296,
    PassiveRFIDMetaDataReportingBlock = 8589934592,
    RFIDDeviceStatusFaultCodeReportingMode = 17179869184,
    VehicleBusPayloadReport=34359738368,
    DeviceMachineSecurityReportingStatusMessage = 68719476736,
    GatewayTMSInfoMessage = 137438953472,
    GatewayTMSReportMessage = 274877906944,
    J1939DiagReporting23 = 549755813888
  }
}
