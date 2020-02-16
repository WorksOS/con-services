using System;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// Contains a map for checking if a device type supports a feature. In the future, this will be moved to the db and fetched on a 1-time basis instead of
  /// hard-coding these values.
  /// This class exists now since we haven't moved the properties into the DB but only device types. 
  /// </summary>
  public static class DeviceTypeFeatureMap
  {
    //For logging
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    //KEY : DeviceTypeID, VALUE : {KEY:FEATUREID, VALUE:BOOLEAN}
    static readonly IDictionary<int, IDictionary<int, bool>> deviceTypeFeatureMap = new Dictionary<int, IDictionary<int, bool>>();

    static readonly IDictionary<int, IDictionary<int, string>> deviceTypePropertyMap = new Dictionary<int, IDictionary<int, string>>();

    static DeviceTypeFeatureMap()
    {
      #region init supported features
      //Explicit intialization is done for all the device types.

      InitDefaultProperties();
      InitCrossCheck();
      InitPL121();
      InitPL321();
      InitPL522();
      InitPL420();
      InitPL421();
      InitSNM940();
      InitSNM941();
      InitTM3000();
      InitTrimTrac();
      InitTAP66();
      InitSNM451();
      InitPL431();
      InitDCM300();
      InitPL641();
      InitPLE641();
      InitPL521();
      InitPL523();
      InitPL631();
      InitPLE631();
      InitPLE641PLUSPL631();
      InitPL241();
      InitPL231();
      InitPL131();
      InitPL141();
      InitPL440();
      InitPL240();
      InitPL161();
      InitPL542();
      InitPLE642();
      InitPLE742();
      InitPL240B();

      #endregion init supported features
    }

    private static void InitPL161()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, false);

      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL161, featureListAvailability);



      //////////////////////////////////////////////////////////////////////////
      /////////////////////////////////////////////////////////////////////////
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL161, devicePropertyValues);

    }



    private static void InitPL240()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);//to do
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL240, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL240, devicePropertyValues);
      #endregion
    }

    private static void InitPL240B()
    {
        IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
        //Explicitly initialize all the Device Features
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);//to do
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);//to do
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);//to do
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);//to do
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
        featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
        //Add to Mapping Dictionary
        deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL240B, featureListAvailability);

        #region init device properties
        IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
        devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
        devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
        devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
        devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
        devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
        devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
        devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
        //Add to properties dictionary
        deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL240B, devicePropertyValues);
        #endregion
    }

    public static List<KeyValuePair<int, List<KeyValuePair<int, bool>>>> GetDeviceTypeFeatureMap()
    {
      Dictionary<int, List<KeyValuePair<int, bool>>> featureMapDictWithList = deviceTypeFeatureMap.ToDictionary(x => x.Key, x => x.Value.ToList());
      return featureMapDictWithList.ToList();
    }

    public static List<KeyValuePair<int, List<KeyValuePair<int, string>>>> GetDeviceTypePropertyMap()
    {
      Dictionary<int, List<KeyValuePair<int, string>>> propertyMapDictWithList = deviceTypePropertyMap.ToDictionary(x => x.Key, x => x.Value.ToList());
      return propertyMapDictWithList.ToList();
    }

    public static IList<int> GetSupportedDeviceTypes(ICDSeries mode)
    {
      IList<int> supportedDevicestypeIDs = new List<int>();
      foreach (int devicetype in deviceTypeFeatureMap.Keys)
      {
        if (DeviceTypePropertyValue((DeviceTypeEnum)devicetype, DeviceTypeProperties.ICDSeries).ToString() == mode.ToValString())
          supportedDevicestypeIDs.Add(devicetype);
      }
      return supportedDevicestypeIDs;
    }

    public static IList<int> SupportedDevicestypeIDs(DeviceTypeFeatureEnum feature)
    {
      IList<int> supportedDevicestypeIDs = new List<int>();
      foreach (int devicetype in deviceTypeFeatureMap.Keys)
      {
        if (DoesDeviceTypeSupportFeature((DeviceTypeEnum)devicetype, feature))
          supportedDevicestypeIDs.Add(devicetype);
      }

      return supportedDevicestypeIDs;
    }

    public static bool DoesDeviceTypeSupportFeature(DeviceTypeEnum deviceType, DeviceTypeFeatureEnum feature)
    {
      IDictionary<int, bool> featureAvailability;
      bool value;

      if (deviceType == DeviceTypeEnum.MANUALDEVICE)
      {
        return false;
      }

      if (!deviceTypeFeatureMap.TryGetValue((int)deviceType, out featureAvailability))
      {
        log.Info("Device type does not exist or is not added to the mapping list");
        return false;
      }

      if (!featureAvailability.TryGetValue((int)feature, out value))
      {
        log.Info("Feature does not exist or is not added to the mapping list");
        return false;
      }

      return value;
    }


    public static string DeviceTypePropertyValue(DeviceTypeEnum deviceType, DeviceTypeProperties property)
    {
      IDictionary<int, string> propertyValue;
      string value;

      if (deviceType == DeviceTypeEnum.MANUALDEVICE)
      {
        return string.Empty;
      }
      //throw exception if device type is not added to the deviceTypeFeatureMap.
      // Dynamic device types will take default property values.
      //      if (DeviceTypeList.GetDeviceType((int)deviceType) == null || (!deviceTypePropertyMap.TryGetValue((int)deviceType, out propertyValue) && !deviceTypePropertyMap.TryGetValue(-1, out propertyValue)))
      if (!deviceTypePropertyMap.TryGetValue((int)deviceType, out propertyValue) && !deviceTypePropertyMap.TryGetValue(-1, out propertyValue))
      {
        log.Info("Device type does not exist or is not added to the mapping list");
        throw new ApplicationException("Device type does not exist or is not added to the mapping list");
      }
      //throw exception if the feature has not been added to list. 
      if (!propertyValue.TryGetValue((int)property, out value))
      {
        log.Info("Device property does not exist for the particular device type");
        return string.Empty;
      }

      return value;
    }

    #region initialization

    private static void InitDefaultProperties()
    {
      #region init default device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add(-1, devicePropertyValues);
      #endregion
    }
    private static void InitCrossCheck()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);


      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.CrossCheck, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());

      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.CrossCheck, devicePropertyValues);
      #endregion
    }

    private static void InitPL121()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL121, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.PLInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.PL.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL121, devicePropertyValues);
      #endregion
    }

    private static void InitPL321()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL321, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.PLInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.PL.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL321, devicePropertyValues);
      #endregion
    }

    private static void InitPL522()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.Series522, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, "4");
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "5");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.AssetSecurityConfigMessageType, AssetSecurityConfigMessageTypeEnum.GatewaySecurityConfigMessage.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.Series522, devicePropertyValues);
      #endregion

    }

    private static void InitPL420()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL420, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "2");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.None.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToBattery");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToBattery");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL420, devicePropertyValues);
      #endregion
    }

    private static void InitPL421()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL421, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "2");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.AssetSecurityConfigMessageType, AssetSecurityConfigMessageTypeEnum.RadioSecurityConfigMessage.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL421, devicePropertyValues);
      #endregion
    }

    private static void InitSNM941()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.SNM941, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToBattery");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.SNM941, devicePropertyValues);
      #endregion
    }


    private static void InitSNM940()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.SNM940, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToBattery");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.SNM940, devicePropertyValues);
      #endregion
    }

    private static void InitTM3000()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.TM3000, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.TM3000, devicePropertyValues);
      #endregion
    }

    private static void InitTrimTrac()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.TrimTrac, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.TTConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.TrimTracInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.TrimTrac, devicePropertyValues);
      #endregion
    }

    private static void InitTAP66()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.TAP66, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.TAP66, devicePropertyValues);
      #endregion
    }

    private static void InitSNM451()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.SNM451, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "2");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.SNM451, devicePropertyValues);
      #endregion
    }

    private static void InitPL431()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL431, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "2");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL431, devicePropertyValues);
      #endregion
    }

    private static void InitDCM300()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, false);// This will be set to true when command out is implemented
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);


      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.DCM300, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, "switchToGround");
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, "switchToBattery");
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.DCM300, devicePropertyValues);
      #endregion
    }

    private static void InitPL641()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL641, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL641, devicePropertyValues);
      #endregion
    }

    private static void InitPLE641()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);

      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PLE641, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PLE641, devicePropertyValues);
      #endregion
    }

    //obsolete devices?
    private static void InitPL521()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.Series521, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "0");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.Series521, devicePropertyValues);
      #endregion

    }

    private static void InitPL523()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);

      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.Series523, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, "4");
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "5");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.MTSConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.MTSInOut.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.MTS.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.Series523, devicePropertyValues);
      #endregion

    }
    private static void InitPL631()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL631, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL631, devicePropertyValues);
      #endregion
    }
    private static void InitPLE631()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PLE631, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PLE631, devicePropertyValues);
      #endregion
    }

    private static void InitPLE641PLUSPL631()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);

      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PLE641PLUSPL631, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PLE641PLUSPL631, devicePropertyValues);
      #endregion
    }

    private static void InitPL241()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL241, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL241, devicePropertyValues);
      #endregion
    }

    private static void InitPL231()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL231, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL231, devicePropertyValues);
      #endregion
    }

    private static void InitPL131()
    {

      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL131, devicePropertyValues);

    }

    private static void InitPL141()
    {

      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL141, devicePropertyValues);

    }


    private static void InitPL440()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL440, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, "2");
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);

      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL440, devicePropertyValues);
      #endregion
    }
    private static void InitPLE642()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);

      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PLE642, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PLE642, devicePropertyValues);
      #endregion
    }
    private static void InitPL542()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PL542, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PL542, devicePropertyValues);
      #endregion
    }

    private static void InitPLE742()
    {
      IDictionary<int, bool> featureListAvailability = new Dictionary<int, bool>();
      //Explicitly initialize all the Device Features
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceDeActivation, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SensorConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SMUSourceConfig, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SwitchConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMPartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.LoadCountConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MaintenanceConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThresholdUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.MovingStoppedThreshold, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceHourMeterUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportFuel, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportOccurenceCount, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SiteDispatch, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SitePurge, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.RFID, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SynchClock, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.TPMS, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.WorkDefinitionConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.BusRequestMessage, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.PublicJ1939, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataEngineStartStopAlert, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareStatusUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OTADeregistration, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportTime, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.OdometerSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HourmeterSupport, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DailyReportConfigUpdate, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ReportingConfig, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.EventFrequency, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NextMessageInterval, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FaultCodeFilter, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DevicePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.GatewayFirmwarePartNumber, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DataLinkType, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.ECMInfo, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityLeaseOwnership, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityTamperResistance, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecurityStartMode, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.AssetSecuritySyncEnabled, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.NetworkManagerFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellularRadioFirmware, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.SatelliteRadioFirmware, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FirmwareVersion, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.CellModemIMEI, false);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FuelLevel, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.FluidAnalysis, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.Diagnostics, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.HoursLocationDelay, true);
      featureListAvailability.Add((int)DeviceTypeFeatureEnum.DeviceConfig, true);
      //Add to Mapping Dictionary
      deviceTypeFeatureMap.Add((int)DeviceTypeEnum.PLE742, featureListAvailability);

      #region init device properties
      IDictionary<int, string> devicePropertyValues = new Dictionary<int, string>();
      devicePropertyValues.Add((int)DeviceTypeProperties.DailyReportTime, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.FuelMessageType, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.MinimumReportingFrequency, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SensorCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.SwitchCount, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OffRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadDuration, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.OnRoadThreshold, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.PollTimeOut, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.TamperSwitch, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ConfigDataType, ConfigDataType.A5N2ConfigData.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.HourmeterUpdateRange, ResetValues.GreaterThanCurrent.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.OdometerUpdateRange, ResetValues.GreaterThanZero.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch1Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch2Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.Switch3Description, string.Empty);
      devicePropertyValues.Add((int)DeviceTypeProperties.ICDSeries, ICDSeries.DataIn.ToValString());
      devicePropertyValues.Add((int)DeviceTypeProperties.LogicalGroup, "A5N2");
      devicePropertyValues.Add((int)DeviceTypeProperties.DeviceConfigGroup, DeviceConfigGroup.A5N2.ToValString());
      //Add to properties dictionary
      deviceTypePropertyMap.Add((int)DeviceTypeEnum.PLE742, devicePropertyValues);
      #endregion
    }

    #endregion initialization
  }
}
