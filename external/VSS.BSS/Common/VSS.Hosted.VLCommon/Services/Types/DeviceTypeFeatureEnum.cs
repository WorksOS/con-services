using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VSS.Hosted.VLCommon
{
  public enum DeviceTypeFeatureEnum
  {
      ECMPartNumber = 0,
      SynchClock = 1,
      SwitchConfig = 2,
      SensorConfig = 3,
      MovingStoppedThreshold = 4,
      MovingStoppedThresholdUpdate = 5,
      MaintenanceConfig = 6,
      WorkDefinitionConfig = 7,
      LoadCountConfig = 8,
      AssetSecurityConfig = 9,
      TPMS = 10,
      SMUSourceConfig = 11,
      RFID = 12,
      ReportOccurenceCount = 13,
      ReportFuel = 14,
      SiteDispatch = 15,
      BusRequestMessage = 16,
      GatewayRequestMessage = 17,
      PublicJ1939 = 18,
      SitePurge = 19,
      DataEngineStartStopAlert = 20,
      FirmwareStatusUpdate = 21,
      OTADeregistration = 22,
      OdometerSupport = 23,
      DailyReportTime = 24,
      DailyReportConfigUpdate = 25,
      HourmeterSupport = 26,
      ReportingConfig = 27,
      EventFrequency = 28,
      NextMessageInterval = 29,
      FaultCodeFilter = 30,
      ECMInfo = 31,
      DevicePartNumber = 32,
      GatewayFirmwarePartNumber = 33,
      DataLinkType = 34,
      DeviceHourMeterUpdate = 35,
      DeviceDeActivation = 36,
      AssetSecurityStartMode = 37,      
      AssetSecurityTamperResistance = 38,
      AssetSecurityLeaseOwnership = 39,
      AssetSecuritySyncEnabled = 40,
      NetworkManagerFirmware = 41,
      CellularRadioFirmware = 42,
      SatelliteRadioFirmware = 43,
      FirmwareVersion = 44,
      CellModemIMEI = 45,
      FuelLevel = 46,  
      FluidAnalysis = 47,
      Diagnostics = 48,
      HoursLocationDelay = 49,
      DeviceConfig = 50
  }
}
