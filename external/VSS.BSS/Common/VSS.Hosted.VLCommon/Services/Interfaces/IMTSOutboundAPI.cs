using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Hosted.VLCommon
{
  public interface IMTSOutboundAPI
  {
      bool SendPurgeSites(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType);
    bool AssignSite(INH_OP opCtx, string[] gpsDeviceIDs, long siteID, bool assign, DeviceTypeEnum deviceType);
    bool ConfigureSensors(INH_OP opCtx1, string[] gpsDeviceIDs, bool sensor1Enabled, bool sensor1IgnRequired, double? sensor1HystHalfSec, bool sensor1HasPosPolarity,
     bool sensor2Enabled, bool sensor2IgnRequired, double? sensor2HystHalfSec, bool sensor2HasPosPolarity,
     bool sensor3Enabled, bool sensor3IgnRequired, double? sensor3HystHalfSec, bool sensor3HasPosPolarity, DeviceTypeEnum deviceType);

    bool SetRuntimeMileage(INH_OP opCtx1, string[] gpsDeviceIDs, double mileage, long runtime, DeviceTypeEnum deviceType);
    bool SetStoppedThreshold(INH_OP opCtx1, string[] gpsDeviceIDs, double threshold, long duration, bool isEnabled, DeviceTypeEnum deviceType);
    bool SetSpeedingThreshold(INH_OP opCtx1, string[] gpsDeviceIDs, double threshold, long duration, bool isEnabled, DeviceTypeEnum deviceType);


    bool SetZoneLogicConfig(INH_OP opCtx1, string[] gpsDeviceIDs, byte entryHomeSiteSpeedMPH, byte exitHomeSiteSpeedMPH, byte hysteresisHomeSiteSeconds, DeviceTypeEnum deviceType);
    bool SetGeneralDeviceConfig(INH_OP opCtx1, string[] gpsDeviceIDs, ushort deviceShutdownDelaySeconds, ushort mdtShutdownDelaySeconds, bool alwaysOnDevice, DeviceTypeEnum deviceType);

    bool SetMovingConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, ushort radius, DeviceTypeEnum deviceType);

    bool SetIgnitionReportingConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, bool ignitionReportingEnabled, DeviceTypeEnum deviceType);

    bool SendPersonalityRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType);

    bool SendGatewayRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<GatewayMessageType> gatewayMessageTypes);
    bool SendVehicleBusRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<VehicleBusMessageType> gatewayMessageTypes);
    bool SendOTAConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs,
      InputConfig? input1Config, TimeSpan? input1Delay, string input1Desc,
      InputConfig? input2Config, TimeSpan? input2Delay, string input2Desc,
      InputConfig? input3Config, TimeSpan? input3Delay, string input3Desc,
      InputConfig? input4Config, TimeSpan? input4Delay, string input4Desc,
      TimeSpan? smu, bool? maintenanceModeEnabled, TimeSpan? maintenanceModeDuration, DigitalInputMonitoringConditions? input1MonitoringCondition,
      DigitalInputMonitoringConditions? input2MonitoringCondition, DigitalInputMonitoringConditions? input3MonitoringCondition,
      DigitalInputMonitoringConditions? input4MonitoringCondition,
      DeviceTypeEnum deviceType);
    bool SendDailyReportConfig(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, bool enabled,
      byte dailyReportTimeHour, byte dailyReportTimeMinute, string timezoneName);
    bool CalibrateDeviceRuntime(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, double newRuntimeHours);

    bool SendMachineSecuritySystemInformationMessage(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, MachineStartStatus? machineStartStatus, TamperResistanceStatus? tamperResistanceStatus);

    void SetMainPowerLossReporting(INH_OP opCtx1, string[] gpsDeviceIDs, bool powerLossReportingEnabled, DeviceTypeEnum deviceType);
    void SetSuspiciousMove(INH_OP opCtx1, string[] gpsDeviceIDs, bool suspiciousMoveEnabled, DeviceTypeEnum deviceType);
    void SetConfigureJ1939Reporting(INH_OP opCtx1, string[] gpsDeviceIDs, bool reportingEnabled, DeviceConfigurationBaseUserDataMessage.ReportType reportType, List<J1939ParameterID> parameters, DeviceTypeEnum deviceType, bool includeSupportingParameters = false);

    void SetMachineEventHeaderConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, PrimaryDataSourceEnum primaryDataSource, DeviceTypeEnum deviceType);
    void SetAssetBasedFirmwareVersion(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, bool RFIDServicePlanAdded = false);

    void SendRFIDConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs,
                               DeviceConfigurationBaseUserDataMessage.RFIDReaderType rfidReaderType,
                               DeviceConfigurationBaseUserDataMessage.RFIDReaderStatusType rfidReaderStatus,
                               DeviceConfigurationBaseUserDataMessage.RFIDTriggerSourceType triggerSource,
                               UInt16 txRFPower, UInt16 asynOnTime, UInt16 asynOffTime,
                               DeviceConfigurationBaseUserDataMessage.AntennaSwitchingMethodType antennaSwitchingMethod,
                               DeviceConfigurationBaseUserDataMessage.LinkRateType linkRate,
                               DeviceConfigurationBaseUserDataMessage.TariType tari,
                               DeviceConfigurationBaseUserDataMessage.MillerValueType millerValue,
                               DeviceConfigurationBaseUserDataMessage.SessionForRfidConfigurationType session,
                               DeviceConfigurationBaseUserDataMessage.TargetForRfidConfigurationType target,
                               bool gen2QHasFixedQValue, byte gen2QFixedQValue,
                               DeviceConfigurationBaseUserDataMessage.BaudRateForRfidConfigurationType baudRate,
                               DeviceConfigurationBaseUserDataMessage.ReaderOperationRegionForRfidConfigurationType readerOperationRegion,
                               DeviceTypeEnum deviceType);
    #region unused
		//bool PollPosition(string gpsDeviceID, DeviceTypeEnum deviceType);
		//bool SendPredefinedMessageList(string[] gpsDeviceIDs, PredefinedMessageList list, DeviceTypeEnum deviceType);
		/*bool SetDriverIDConfig(INH_OP opCtx, string[] gpsDeviceIDs, bool driverIDEnabled, bool enableMDTDriverEntry, bool forceEntryAndLogOut,
			DriverIDCharSet charSet, byte mdtIDMax, byte mdtIDMin, byte displayedListSize, byte storedListSize, bool forcedLogon,
			bool autoLogoutInvalid, bool autoLogout, TimeSpan autoLogoutTime, bool expireMRU, TimeSpan mruExpiry, bool expireUnvalidatedMRUs,
			TimeSpan unvalidatedExpiry, bool displayMechanic, string mechanicID, string mechanicDisplayName, bool enableLoggedIn, byte loggedInoutputPolarity, DeviceTypeEnum deviceType);*/
		//bool SetPrimaryIPAddressConfiguration(string[] gpsDeviceIDs, string ipAddress, bool isTCP, DeviceTypeEnum deviceType, short? otherPort = null);
		//bool SendFirmwareRequestMessage(INH_OP opCtx, string[] gpsDeviceIDs, long mtsFirmwareVersionID, string directory, string host, string username, string password, DateTime dueDateUTC);
		//bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, string stackConfig1, string stackConfig2, string stackConfig3, string stackConfig4, string AppConfig, DeviceTypeEnum deviceType);
		//bool QueryBITReport(string[] gpsDeviceIDs, DeviceConfigurationQueryBaseUserDataMessage.QueryCommand whichReport, DeviceTypeEnum deviceType);
		//bool SendDeviceConfigurationQueryCommand(string[] gpsDeviceIDs, DeviceConfigurationQueryBaseUserDataMessage.QueryCommand command, DeviceTypeEnum deviceType);
		//bool SetDevicePortConfig(string[] gpsDeviceIDs,string portNumber, string serviceType, DeviceTypeEnum deviceType);
		//bool CancelFirmwareRequestMessage(INH_OP opCtx, string[] gpsDeviceIDs, DateTime? dueUTC);
		//bool SendPort1033SkylineFirmWareUpdateCommand(string gpsDeviceID, byte fwRequest, byte? target, bool? forceDirectory, bool? versionNumbersIncluded, string ftpHostName,
			//string ftpUserName, string ftpPassword, string sourcePath, string destinationPath, byte? fwMajor, byte? fwMinor, byte? fwBuildType, byte? hwMajor, byte? hwMinor, DeviceTypeEnum deviceType);
		//bool SendMachineEventConfig(string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<MachineEventConfigBlock> configBlocks);
		//bool SendRadioMachineSecuritySystemInformationMessage(string[] gpsDeviceIDs, DeviceTypeEnum deviceType, MachineStartStatus? machineStartStatus);
		//bool SendPasscode(string gpsDeviceIDs, string passcode, DeviceTypeEnum deviceType);
		//void SendJ1939PublicParametersRequest(string[] gpsDeviceIDs, List<J1939ParameterID> parameters, DeviceTypeEnum deviceType);
		//bool SendDeviceData(string[] gpsDeviceIDs, SendDataToDevice.ControlType controlType, SendDataToDevice.Destination destination, byte[] data, DeviceTypeEnum deviceType);
		//void SetRadioTransmitterDisableControl(string[] gpsDeviceIDs, bool isEnabled, DeviceTypeEnum deviceType);
		//bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, String newAPN, DeviceTypeEnum deviceType);
		//bool ConfigureTPMS(string gpsDeviceID, bool isEnabled, DeviceTypeEnum deviceType);
    //bool SendTextMessage(string[] gpsDeviceIDs, string message, string[] responseSet, out uint[] sequenceIDs, DeviceTypeEnum deviceType);
    //bool RequestTCPUDPStats(string gpsDeviceID, DeviceTypeEnum deviceType);
    //bool SetHomeSitePositionReportingConfiguration(string[] gpsDeviceIDs, ushort homeSiteRadius, byte durationThresholdSeconds, DeviceTypeEnum deviceType);
    //bool SetDiagnosticReportConfiguration(string[] gpsDeviceIDs, bool enableGPSAntenna, bool enableComms, bool enableGPSStatus, bool enableOdometerConfidence, DeviceTypeEnum deviceType);
    //bool SetApplicationGeneralConfiguration(string[] gpsDeviceIDs, bool manualStatusing, DeviceTypeEnum deviceType);
    /*bool SetGPSEventConfiguration(string gpsDeviceID, bool gpsAntennaFaultEnabled, DeviceTypeEnum deviceType);
    bool SetMappingAppConfiguration(string gpsDeviceID, bool autoStopEnabled,
      uint arrivalTimeThresholdSeconds, uint arrivalDistanceThresholdMeters, DeviceTypeEnum deviceType);
    bool SetMessageAlertConfiguration(string gpsDeviceID, byte alertCount, byte alertDelay, DeviceTypeEnum deviceType);
    bool SendMetricsConfiguration(string[] gpsDeviceIDs, bool enableNetMetricsReports, bool enableTCPMetricsReports, bool enableGpsMetricsReports,
      bool enableErrorLogReports, TimeSpan networkMetricsMinReportingInterval, TimeSpan networkMetricsMaxReportingInterval,
      TimeSpan tcpMetricsMinReportingInterval, TimeSpan tcpMetricsMaxReportingInterval, TimeSpan gpsMetricsMinReportingInterval,
      TimeSpan gpsMetricsMaxReportingInterval, DeviceTypeEnum deviceType);
    bool SetStoreForwardConfiguration(string[] gpsDeviceIDs, bool positionForwardingEnabled, bool outOfNetworkPositionSavingEnabled, bool inNetworkPositionSavingEnabled, DeviceConfigurationBaseUserDataMessage.StoreForwardUpdateInterval updateIntervals, DeviceTypeEnum deviceType);*/
    #endregion

    
  }
}
