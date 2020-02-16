﻿ 
 
 
 
 
 
 
 
 


//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VSS.Hosted.VLCommon
{
  public enum DimAlertSubTypeEnum
  {
    UnknownProject=-2,
    UnknownCell=-1,
    NoMatchingProjectDate=1,
    NoMatchingProjectArea=2,
    MultipleProjects=3,
    InvalidSeedPosition=4,
    InvalidOnGroundFlag=5,
    InvalidPosition=6
  }
  public enum DimAssetWorkingStateEnum
  {
    NoDeviceState=-1,
    AwaitingFirstReport=0,
    AssetOn=1,
    Idling=2,
    AssetOff=3,
    Reporting=4,
    NotReporting=5,
    OnSite=6,
    Working=8,
    Stopped=9,
    Running=10,
    CoolDown=11,
    EngineStopping=12,
    NotReadytoRun=13,
    NotAvailable=14
  }
  public enum DimEmulatedEventTypeEnum
  {
    Unknown=-1,
    Actual,notemulated=0,
    BySite=1,
    ByProximity_LoadEE=2,
    ByProximity_LoadER=3,
    BySiteProximity_ScraperIdleSameSite=4,
    ByAverage=5,
    BySiteToSite=6,
    ByProximity_ScraperToScraper=7,
    ByManualEmulation=8
  }
  public enum DimEventTypeEnum
  {
    IgnitionOn=10,
    StartWorking=15,
    StartMoving=20,
    SiteEntry=22,
    SiteExit=23,
    StopMoving=25,
    StopWorking=30,
    Ignitionoff=35,
    EngineStart=50,
    EngineStop=55,
    InclusiveZoneAlarm=60,
    ExclusiveZoneAlarm=65,
    SpeedingStart=70,
    SpeedingStop=71,
    SensorOn=72,
    SensorOff=73,
    StartCycle=76,
    StopCycle=77,
    PowerOn=78,
    PowerOff=79,
    IdleTimeOut=80
  }
  public enum DimFaultTypeEnum
  {
    None=0,
    Event=1,
    Diagnostic=2,
    Component=3
  }
  public enum DimLoadIncompleteTypeEnum
  {
    None=0,
    Unknown=1,
    NoStopMoving=2,
    NoStopMovingInSite=3,
    MissingSwitchEvent=4
  }
  public enum DimLoadQualityTypeEnum
  {
    Unknown=0,
    SwitchtoSwitch=1,
    ProximitytoSwitch=2,
    SwitchtoSiteorSitetoSwitch=3,
    AveragedSitetoSwitch=4,
    SitetoSite=5,
    LoadERproximityLoadOnly=6,
    Reconciled=7
  }
  public enum DimMassHaulTypeEnum
  {
    Unknown=-1,
    MassHaul=0,
    CrossHaul=1,
    NoHaul=2,
    BackHaul=3,
    UnplannedHaul=4,
    UnplannedCrossHaul=5,
    UnplannedNoHaul=6
  }
  public enum DimReportTypeEnum
  {
    None=0,
    FleetUsage=1,
    FuelUtilization=2,
    AssetHealth=3,
    AssetHistory=4,
    AssetUsageSingleAsset=5,
    FleetStatus=6,
    LoadCount=7,
    AssetFuelSingleAsset=8,
    FluidAnalysis=9,
    ServiceDue=10,
    CycleTime=11,
    SharedAssetViewReport=12,
    MaintenanceHistoryReport=13,
    AssetUsageSummaryReport=14,
    SiteRuntime=15,
    PayloadReport=16,
    FleetUtilization=17,
    BackhoeLoaderOperation=18,
    EngineIdle=19,
    FleetEventCountReport=20,
    ExcavatorUsageReport=21,
    MachineParametersReport=22,
    AssetSecurityLastActivityReport=23,
    AssetSpeedingReport=24,
    AssetSecurityUserActivityReport=25,
    StateMileageReport=26,
    ProjectTablePieChart=101,
    ProjectProfile=102,
    ProjectMap=103,
    SummaryVolumesProfile=104,
    SummaryVolumesTable=105,
    ProjectMonitoringTable=106,
    StationOffset=201,
    GridReport=202,
    SummaryData=203,
    GoogleALKUsageReport=204,
    WebPerformanceMetrics=1001,
    ClientSvcMetrics=1002,
    MessageLatency=1003,
    CustomerDatafeedUsage=1004,
    DatafeedPerformance=1005,
    ReportingDevicesWithoutSubscriptions=1006,
    PermissionsReport=1007,
    UserAccountsReport=2000,
    UserPrivilegesReport=2001,
    VisionLinkOperationsReport=2002,
    FleetSummaryCustomReport=3000
  }
  public enum DimSensorAspectsEnum
  {
    NotAvailable=-1,
    NoAlert=0,
    OverPressure=1,
    UnderPressure=2,
    OverTemperature=3,
    LowSensorBattery=4,
    NormalSensorBattery=5,
    HighSensorBattery=6,
    OverPressureLevel1=7,
    OverPressureLevel2=8,
    LowPressureLevel1=9,
    LowPressureLevel2=10,
    OverTemperatureLevel1=11,
    OverTemperatureLevel2=12,
    SensorNotCommunicating=13,
    SensorNotInstalled=14
  }
  public enum DimSensorTypeEnum
  {
    NotAvailable=0,
    Pressure=1,
    Temperature=2,
    Battery=3
  }
  public enum DimSeverityLevelEnum
  {
    Unknown=0,
    Low=1,
    Medium=2,
    High=3
  }
  public enum DimUtilizationCalloutTypeEnum
  {
    None=0,
    MissingMeterValue=1,
    MultipleDayDelta=2,
    Spike=3,
    NotApplicable=4,
    NegativeValue=5,
    NoData=6,
    MissingTotalFuelData=7
  }
  public enum DimUtilizationTypeEnum
  {
    PayloadWeight=1,
    CycleCount=2,
    PayloadWeightMeter=3,
    CycleCountMeter=4,
    PayloadPerCycle=5
  }
  public enum DimWarningLevelEnum
  {
    Unset=0,
    Unknown=1,
    Normal=2,
    Warning=3,
    Hazardous=4
  }
}
