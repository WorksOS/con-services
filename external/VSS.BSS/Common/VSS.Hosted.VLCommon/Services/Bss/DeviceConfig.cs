using log4net;
using MassTransit;
using MassTransit.Log4NetIntegration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon.Events;
using VSS.Hosted.VLCommon.MTSMessages;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Factories;
using VSS.Nighthawk.DeviceCapabilityService.Interfaces.Implementations.Query;
using BE = VSS.BaseEvents;
using ED= VSS.Nighthawk.ExternalDataTypes.Enumerations;
using VSS.Hosted.VLCommon.Services.Bss.Interfaces;
using VSS.Hosted.VLCommon.Services.Bss.Helpers;
using VssJsonMessageSerializer = VSS.Nighthawk.MassTransit.JsonMessageSerializer;
namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// This class includes functions for configuring devices, based on the Service Plan purchased by the user.
  /// This includes enabling/disabling digital switches, and adjusting the position reporting rate.
  /// </summary>
  public static class DeviceConfig
  {
    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly bool isSiteDispatchEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["SiteDispatchEnable"]);
    public static IServiceBus ServiceBus { get; set; }
    private static bool? _isProductionBss;
    public static bool ConfigureDeviceForServicePlan(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType, 
      bool isAdded, ServiceTypeEnum modifiedPlan, List<ServicePlanIDs> currentPlanIDs )
    {
      try
      {
        Log.IfDebugFormat("DeviceConfig.ConfigureDeviceForServicePlan: gpsDeviceID={0}, deviceType={1}, isAdded={2}, modifiedPlan={3}, currentPlanIDs={4}", gpsDeviceID, deviceType, isAdded, modifiedPlan, GetCurrentPlanIDs(currentPlanIDs));
        bool hasOneMinPlan = (from pp in currentPlanIDs where pp.PlanID == (int)ServiceTypeEnum.e1minuteUpdateRateUpgrade select 1).Any();
        bool hasUtilPlan = (from pp in currentPlanIDs where pp.PlanID == (int)ServiceTypeEnum.CATUtilization || pp.PlanID == (int)ServiceTypeEnum.StandardUtilization select 1).Any();
        bool hasCore = (from pp in currentPlanIDs where pp.PlanID != (int)modifiedPlan && pp.IsCore select 1).Any();
        bool hasCatDailyPlan = (from pp in currentPlanIDs where pp.PlanID == (int)ServiceTypeEnum.CATDaily select 1).Any();
        bool hasVisionLinkDailyPlan = (from pp in currentPlanIDs where pp.PlanID == (int)ServiceTypeEnum.VisionLinkDaily select 1).Any();

        bool addingCorePlan = (from pp in currentPlanIDs where pp.PlanID == (int)modifiedPlan select pp.IsCore).FirstOrDefault();
        Log.IfDebugFormat("DeviceConfig.ConfigureDeviceForServicePlan: hasOneMinPlan={0}, hasUtilPlan={1}, hasCore={2}, addingCorePlan={3},hasCatDailyPlan={4},hasVisionLinkDailyPlan={5} ", hasOneMinPlan, hasUtilPlan, hasCore, addingCorePlan,hasCatDailyPlan, hasVisionLinkDailyPlan);

        if (isAdded && !hasCore && addingCorePlan)
        {
          ConfigureDeviceToDefaults(opCtx, gpsDeviceID, deviceType, currentPlanIDs);
        }

                // Set the location reporting frequency
                if (hasCatDailyPlan || hasVisionLinkDailyPlan)
                {
                    if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.PLInOut.ToValString())
                        PLOnceDailyConfig(opCtx, gpsDeviceID, deviceType);
                    else if(DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
                        MTSLeastReportingConfig(opCtx, gpsDeviceID, deviceType);
                }
                else if (hasOneMinPlan)
                {
                    OneMinuteReportingConfig(opCtx, gpsDeviceID, deviceType, isAdded, modifiedPlan);
                }
                else if (deviceType == DeviceTypeEnum.PL440 && !isAdded && modifiedPlan == ServiceTypeEnum.e1minuteUpdateRateUpgrade)
                {
                    OneMinuteReportingConfig(opCtx, gpsDeviceID, deviceType, isAdded, modifiedPlan);
                }
                else if (hasUtilPlan)
                    OneHourReportingConfig(opCtx, gpsDeviceID, deviceType);
                else
                    CoreReportingConfig(opCtx, gpsDeviceID, deviceType);

        var featureSetId = DeviceTypeList.GetAppFeatureSetId((int)deviceType);
        //Sent RFID only for supported devices-- as of now only for SNM451
        if (AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.RFID))
        {
          var gpsDeviceIDarray = new string[1];
          gpsDeviceIDarray[0] = gpsDeviceID;

          if (modifiedPlan == (ServiceTypeEnum.VisionLinkRFID))
          {
            if (isAdded)
            {
              API.MTSOutbound.SetAssetBasedFirmwareVersion(opCtx, gpsDeviceIDarray, deviceType, isAdded);
            }

            API.MTSOutbound.SendRFIDConfiguration(opCtx, gpsDeviceIDarray,
            DeviceConfigurationBaseUserDataMessage.RFIDReaderType.TMVegaM5e, //0 = TM Vega M5e (default)
            (isAdded ? DeviceConfigurationBaseUserDataMessage.RFIDReaderStatusType.EnableRFIDReader : DeviceConfigurationBaseUserDataMessage.RFIDReaderStatusType.DisableRFIDReader), //0: RFID read is enabled only by “ignition on” event and RFID read is only disabled by “ignition off” event (default)
            DeviceConfigurationBaseUserDataMessage.RFIDTriggerSourceType.EnabledByIgnition,  //0: RFID read is enabled only by “ignition on” event and RFID read is only disabled by “ignition off” event (default)
            3000,  //TX RF power (Centi-dBm) 0x0000= 0 or 0dbm or 1mW  ( it can’t be  set to 0mW, for M5e, the lowest value is 500 ceti-dBm) 0x0BB8=3000 ceti-dBm  NOTE: for TM Vega M5e, the Max and default setting is 3000, which is 30dBm.
            375, //Default setting 375ms
            0, //0ms (default setting for continue read)
            DeviceConfigurationBaseUserDataMessage.AntennaSwitchingMethodType.Dynamic, //0: Dynamic (default)
            DeviceConfigurationBaseUserDataMessage.LinkRateType.KHz250, //250: "250kHz",  (default)
            DeviceConfigurationBaseUserDataMessage.TariType.Us25, //0: "25us",  (default)
            DeviceConfigurationBaseUserDataMessage.MillerValueType.M2, //1: "M2", (default)
            DeviceConfigurationBaseUserDataMessage.SessionForRfidConfigurationType.S2, //2: "S2", (default)
            DeviceConfigurationBaseUserDataMessage.TargetForRfidConfigurationType.AB, //0: "AB" (per bug 23403),  (default)
            false, //Gen 2Q: Dynamic (default); 
            0,
            DeviceConfigurationBaseUserDataMessage.BaudRateForRfidConfigurationType.BaudRate115200bps, //5: 115200 (bps) (default)
            DeviceConfigurationBaseUserDataMessage.ReaderOperationRegionForRfidConfigurationType.NA,  //0: "NA" (default)
            deviceType   //is DeviceTypeEnum passedIn to Method
            );

            //When unsubscribing from RFID subscription the order of config changes is important: We must Disable the RFID before we update the Firmware version (revert back to 420VocationalTruck config)
            if (!isAdded)
            {
                API.MTSOutbound.SetAssetBasedFirmwareVersion(opCtx, gpsDeviceIDarray, deviceType, isAdded);
            }
          }
        }
        // Not enable/disable config based on the service plan addition/cancellation that is being processed.
        switch (modifiedPlan)
        {
          case ServiceTypeEnum.Essentials:
            if (currentPlanIDs.Count == 0 && isAdded && 
                //Purge sites for devices capable of sending sites. XCheck is an exception as we do SSSD but we still want to purge any leftover sites from EM stack
                (AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.SiteDispatch) ||
                 deviceType == DeviceTypeEnum.CrossCheck))
            {
              //Purge All Sites
                if (isSiteDispatchEnabled)
                {
                    var asset = (from a in opCtx.AssetReadOnly
                                 where a.Device.GpsDeviceID == gpsDeviceID && a.Device.fk_DeviceTypeID == (int)deviceType
                                 select a).FirstOrDefault();
                    var purgeEvent = new
                    {
                        AssetUid = asset.AssetUID,
                        DeviceId = gpsDeviceID,
                        DeviceType = deviceType,
                        DeviceUid = asset.Device.DeviceUID
                    };
                    API.GeofenceService.PurgePublish(purgeEvent);
                    //foreach (string gpsDeviceID in gpsDeviceIDs)
                    //    API.Site.Purge(opCtx, gpsDeviceID, deviceType);
                }
                else
                {
                    //Must be cancelling core so no active service plans left - need to purge sites
                    API.Site.Purge(opCtx, gpsDeviceID, deviceType);
                }
            }
            else if (currentPlanIDs.Count == 0 && !isAdded)
            {
              return false;
            }
            break;
          default:
            // There are other plans, but they do not have implications on the device's on-board configuration
            break;
        }

        return true;
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "Unable to configure device {0} for Service Plan {1}", gpsDeviceID, modifiedPlan);
        return false;
      }
    }

    public static bool ConfigureDeviceToDefaults(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType, List<ServicePlanIDs> currentPlanIDs)
    {
      try
      {
        Log.IfDebugFormat("DeviceConfig.ConfigureDeviceToDefaults: gpsDeviceId={0}, deviceType={1}", gpsDeviceID, deviceType);
        string[] gpsDeviceIDs = { gpsDeviceID };

        //Determine which configuration message to send based on the out communication channel
        if(DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
          MTSDefaultConfig(opCtx, gpsDeviceIDs, deviceType, currentPlanIDs);
        else if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.TrimTracInOut.ToValString())
            TrimTracDefaultConfig(opCtx, gpsDeviceID);
        else if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.PLInOut.ToValString())
            PLDefaultConfig(opCtx, gpsDeviceID, deviceType);

        ConfigureDigitalSwitches(opCtx, gpsDeviceID, deviceType);

        return true;
      }
      catch (Exception e)
      {
        Log.IfErrorFormat(e, "Unable to configure device {0} with defaults ", gpsDeviceID);
        return false;
      }
    }

    public static bool UpdateDeviceState(INH_OP opCtx, string gpsDeviceID, DeviceTypeEnum deviceType, DeviceStateEnum deviceState)
    {
      bool updatedOK = true;

      try
      {
        if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.TrimTracInOut.ToValString())
          API.Device.UpdateTTDeviceState(opCtx, gpsDeviceID, deviceState);
        else
          API.Device.UpdateOpDeviceState(opCtx, gpsDeviceID, deviceState, (int)deviceType);

      }
      catch (Exception ex)
      {
        updatedOK = false;
        Log.IfWarnFormat("Update Service failed: {0}", ex.Message);
      }
      return updatedOK;
    }

    public static void ResetEnvironmentFlag()
    {
      _isProductionBss = null;
      IsEnvironmentProd();
    }

    public static bool IsEnvironmentProd()
    {
      if (!_isProductionBss.HasValue)
      {
        bool flag;
        string isProductionBssConfig = ConfigurationManager.AppSettings["IsProductionBSS"];
        if (Boolean.TryParse(isProductionBssConfig, out flag))
        {
          _isProductionBss = flag;
          Log.IfInfoFormat("DeviceConfig.IsEnvironmentProd: IsProductionBSS is {0}.", _isProductionBss);
        }
        else
        {
          _isProductionBss = false;
          Log.IfInfo("DeviceConfig.IsEnvironmentProd: Could not parse IsProductionBSS. Value set to default of false.");
        }
      }

      Log.IfDebugFormat("DeviceConfig.IsEnvironmentProd: Returning {0}", _isProductionBss.Value);
      return _isProductionBss.Value;
    }

    #region Implementation
    private static void ConfigureDigitalSwitches(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      string[] gpsDeviceIDs = { gpsDeviceID };

      switch (deviceType)
      {
        case DeviceTypeEnum.Series522:
        case DeviceTypeEnum.Series523:
            API.MTSOutbound.ConfigureSensors(opCtx1, gpsDeviceIDs, true, true, 4, false, true, false, 4, false, true, false, 4, false, deviceType);
            API.MTSOutbound.SendOTAConfiguration(opCtx1, gpsDeviceIDs, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), null, InputConfig.NotConfigured, 
              new TimeSpan(0, 0, 0, 5), null, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), null, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), 
              null, null, false, new TimeSpan(0, 12, 0, 00), DigitalInputMonitoringConditions.Always, DigitalInputMonitoringConditions.Always,
              DigitalInputMonitoringConditions.Always, DigitalInputMonitoringConditions.Always, deviceType);
          break;
        case DeviceTypeEnum.Series521:
        case DeviceTypeEnum.SNM940:
        case DeviceTypeEnum.SNM941:
        case DeviceTypeEnum.SNM451:
        case DeviceTypeEnum.CrossCheck:
          API.MTSOutbound.ConfigureSensors(opCtx1, gpsDeviceIDs, true, true, 4, false, true, false, 4, false, true, false, 4, false, deviceType);
          break;
        case DeviceTypeEnum.PL420:
        case DeviceTypeEnum.PL421:
        case DeviceTypeEnum.PL431:
          // Don't enable sensor 2 (tamper switch)
          API.MTSOutbound.ConfigureSensors(opCtx1, gpsDeviceIDs, true, true, 4, false, false, false, 4, false, true, false, 4, false, deviceType);
          break;
        case DeviceTypeEnum.PL321:
          API.PLOutbound.SendDigitalInputConfig(opCtx1, gpsDeviceID, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), DigitalInputMonitoringConditions.Always,
              null, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), DigitalInputMonitoringConditions.Always, null, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), 
              DigitalInputMonitoringConditions.Always, null, InputConfig.NotConfigured, new TimeSpan(0, 0, 0, 5), DigitalInputMonitoringConditions.Always, null);
          break; 
      }
    }

    private static void OneMinuteReportingConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType, bool isAdded, ServiceTypeEnum modifiedPlan)
    {
      if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
      {
        if(deviceType==DeviceTypeEnum.CrossCheck)
            UpdateMTSSamplingIntervals(opCtx1, gpsDeviceID, ServiceType.OneMinuteSamplingInterval, ServiceType.OneMinuteReportingInterval, deviceType);
        else
            UpdateMTSSamplingIntervals(opCtx1, gpsDeviceID, ServiceType.OneMinuteSamplingInterval, ServiceType.TenMinuteReportingInterval, deviceType);
      }
      else if (deviceType == DeviceTypeEnum.PL440 && modifiedPlan==ServiceTypeEnum.e1minuteUpdateRateUpgrade)
      {
        //Enable Rapid reporting 
        if (isAdded)
        EnableRapidReporting(gpsDeviceID, (ED.DeviceTypeEnum)deviceType); //ideally device type enum should be moved out of VL common
        else
        DisableRapidReporting(gpsDeviceID, (ED.DeviceTypeEnum)deviceType); 
        
      }

    }

    private static void OneHourReportingConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
          UpdateMTSSamplingIntervals(opCtx1, gpsDeviceID, ServiceType.PerformanceSamplingInterval, ServiceType.PerformanceReportingInterval, deviceType);      
    }

    private static void MTSLeastReportingConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
        if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
            UpdateMTSSamplingIntervals(opCtx1, gpsDeviceID, ServiceType.LeastSamplingInterval, ServiceType.LeastReportingInterval, deviceType);
    }

    private static void CoreReportingConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      if (DeviceTypeFeatureMap.DeviceTypePropertyValue(deviceType, DeviceTypeProperties.ICDSeries) == ICDSeries.MTSInOut.ToValString())
          UpdateMTSSamplingIntervals(opCtx1, gpsDeviceID, ServiceType.DefaultSamplingInterval, ServiceType.DefaultReportingInterval, deviceType);
      
    }

    private static void PLOnceDailyConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
        Log.IfDebugFormat("DeviceConfig.PLOnceDailyConfig: enter");

        if (deviceType == DeviceTypeEnum.PL321)
        {
            API.PLOutbound.SendReportIntervalsConfig(opCtx1, gpsDeviceID, deviceType, null, EventFrequency.Next, EventFrequency.Next,
                EventFrequency.Next, TimeSpan.FromHours(24), false, null, EventFrequency.Next, SMUFuelReporting.SMUFUEL, false, 1);
            API.PLOutbound.SendProductWatchActivation(opCtx1, gpsDeviceID, true, true, false);
        }
        else if (deviceType == DeviceTypeEnum.PL121)
            API.PLOutbound.SendReportIntervalsConfig(opCtx1, gpsDeviceID, deviceType, null, null, null, null, null, false, null, null, null, null, 1);
        Log.IfDebugFormat("DeviceConfig.PLOnceDailyConfig: exit");
    }

        private static void PLDefaultConfig(INH_OP opCtx1, string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      Log.IfDebugFormat("DeviceConfig.PLDefaultConfig: enter");
      
      if (deviceType == DeviceTypeEnum.PL321)
      {
        API.PLOutbound.SendReportIntervalsConfig(opCtx1, gpsDeviceID, deviceType, null, EventFrequency.Immediately, EventFrequency.Immediately, 
          EventFrequency.Immediately, null, false, null, EventFrequency.Immediately, SMUFuelReporting.SMUFUEL, true, 4);
        API.PLOutbound.SendProductWatchActivation(opCtx1, gpsDeviceID, true, true, false);
      }
      else if (deviceType == DeviceTypeEnum.PL121)
        API.PLOutbound.SendReportIntervalsConfig(opCtx1, gpsDeviceID, deviceType, null, null, null, null, null, false, null, null, null, null, 4);
      Log.IfDebugFormat("DeviceConfig.PLDefaultConfig: exit");
    }

    private static void MTSDefaultConfig(INH_OP opCtx, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<ServicePlanIDs> currentPlanIDs)
    {
      Log.IfDebugFormat("DeviceConfig.MTSDefaultConfig: enter");
      //Set general config info
      ushort deviceShutdownDelaySeconds = 900;
      ushort mdtShutdownDelaySeconds = 60;
      bool alwaysOnDevice = false;
      API.MTSOutbound.SetGeneralDeviceConfig(opCtx, gpsDeviceIDs, deviceShutdownDelaySeconds, mdtShutdownDelaySeconds, alwaysOnDevice, deviceType);

      //Enable ignition reporting
      bool ignitionReportingEnabled = true;
      API.MTSOutbound.SetIgnitionReportingConfiguration(opCtx, gpsDeviceIDs, ignitionReportingEnabled, deviceType);

      //reset device mileage and runtime to 0
      API.MTSOutbound.SetRuntimeMileage(opCtx, gpsDeviceIDs, 0.0, 0, deviceType);

      //Purge All Sites
      if (isSiteDispatchEnabled)
      {
        var asset = (from a in opCtx.AssetReadOnly
                       where a.Device.GpsDeviceID == gpsDeviceIDs.FirstOrDefault() && a.Device.fk_DeviceTypeID == (int)deviceType
                       select a).FirstOrDefault();
        var purgeEvent = new
        {
          AssetUid = asset.AssetUID,
          DeviceId = gpsDeviceIDs.FirstOrDefault(),
          DeviceType = deviceType, 
          DeviceUid= asset.Device.DeviceUID
        };
        API.GeofenceService.PurgePublish(purgeEvent);
      }
      else
      {
          foreach (string gpsDeviceID in gpsDeviceIDs)
            API.Site.Purge(opCtx, gpsDeviceID, deviceType);
      }
      
      //Set site entry/exit config
      byte entryHomeSiteSpeedMPH = 99;
      byte exitHomeSiteSpeedMPH = 0;
      byte hysteresisHomeSiteSeconds = 1;
      if (deviceType == DeviceTypeEnum.SNM940 || deviceType == DeviceTypeEnum.SNM941 || deviceType == DeviceTypeEnum.DCM300)
      {
        exitHomeSiteSpeedMPH = 1;
      }

      if (deviceType != DeviceTypeEnum.CrossCheck)
      {
          API.MTSOutbound.SetZoneLogicConfig(opCtx, gpsDeviceIDs, entryHomeSiteSpeedMPH, exitHomeSiteSpeedMPH, hysteresisHomeSiteSeconds, deviceType);
      }

      //Set moving config
      ushort movingRadius = 30;  //Default radius for off-road
      if (deviceType == DeviceTypeEnum.PL420 || deviceType == DeviceTypeEnum.CrossCheck)
      {
        movingRadius = 300;
      }
      API.MTSOutbound.SetMovingConfiguration(opCtx, gpsDeviceIDs, movingRadius, deviceType);

      //Set speeding threshold config
      double speedingThreshold = 150;
      long speedingDuration = 3600;
      bool speedingEnabled = false;
      if (deviceType == DeviceTypeEnum.SNM940 ||
         deviceType == DeviceTypeEnum.SNM941 ||
          deviceType == DeviceTypeEnum.PL420 ||
          deviceType == DeviceTypeEnum.PL421 ||
          deviceType == DeviceTypeEnum.SNM451 ||
          deviceType == DeviceTypeEnum.PL431 || 
          deviceType == DeviceTypeEnum.DCM300)
      {
        speedingThreshold = 10;
        speedingDuration = 4;
      }
      if (deviceType == DeviceTypeEnum.CrossCheck)
      {
        speedingThreshold = 65;
        speedingDuration = 120;
      }

      API.MTSOutbound.SetSpeedingThreshold(opCtx, gpsDeviceIDs, speedingThreshold, speedingDuration, speedingEnabled, deviceType);

      //Set stopped threshold config
      double stopThreshold = 0.2;
      long stopDuration = 30;
      bool stopEnabled = true;

      if (deviceType == DeviceTypeEnum.SNM940 || deviceType == DeviceTypeEnum.SNM941 || deviceType == DeviceTypeEnum.DCM300)
      {
        stopThreshold = 1;
      }
      if (deviceType == DeviceTypeEnum.PL420)
      {
        stopThreshold = .1;
        stopDuration = 120;
      }
      if (deviceType == DeviceTypeEnum.CrossCheck)
      {
        stopThreshold = 0;
        stopDuration = 120;
      }
      API.MTSOutbound.SetStoppedThreshold(opCtx, gpsDeviceIDs, stopThreshold, stopDuration, stopEnabled, deviceType);

      var featureSetId = DeviceTypeList.GetAppFeatureSetId((int)deviceType);
      if (AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.GatewayRequestMessage))
      {
          API.MTSOutbound.SendGatewayRequest(opCtx, gpsDeviceIDs, deviceType, new List<GatewayMessageType>
        {GatewayMessageType.ECMInfo, 
          GatewayMessageType.DigitalInputInfo, GatewayMessageType.MaintenanceModeInfo});
      }

      if (AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.BusRequestMessage))
      {
          API.MTSOutbound.SendVehicleBusRequest(opCtx, gpsDeviceIDs, deviceType, new List<VehicleBusMessageType> { VehicleBusMessageType.ECMInfo });
      }

      if (AppFeatureMap.DoesFeatureSetSupportsFeature(featureSetId, AppFeatureEnum.PublicJ1939))
      {
        //Enable J1939 SMH and Odometer by default for PL420 and PL421
          API.MTSOutbound.SetMachineEventHeaderConfiguration(opCtx, gpsDeviceIDs, PrimaryDataSourceEnum.J1939, deviceType);

        //Set Main Power Loss Reporting
        bool mainPowerLossReportingEnabled = true;
        API.MTSOutbound.SetMainPowerLossReporting(opCtx, gpsDeviceIDs, mainPowerLossReportingEnabled, deviceType);

        //Set Suspicious Move Reporting
        bool suspiciousMoveReportingEnabled = true;
        API.MTSOutbound.SetSuspiciousMove(opCtx, gpsDeviceIDs, suspiciousMoveReportingEnabled, deviceType);


        API.MTSOutbound.SetConfigureJ1939Reporting(opCtx, gpsDeviceIDs, true, DeviceConfigurationBaseUserDataMessage.ReportType.Fault, new List<J1939ParameterID>(), deviceType, false);
        List<J1939ParameterID> periodicParameters = new List<J1939ParameterID>();
        J1939ParameterID engineTotalPTO = new J1939ParameterID();
        engineTotalPTO.PGN = 65255;
        engineTotalPTO.SPN = 248;
        engineTotalPTO.SourceAddress = 0;
        periodicParameters.Add(engineTotalPTO);
        
        J1939ParameterID transmissionTotalPTO = new J1939ParameterID();
        transmissionTotalPTO.PGN = 65255;
        transmissionTotalPTO.SPN = 248;
        transmissionTotalPTO.SourceAddress = 3;
        periodicParameters.Add(transmissionTotalPTO);
       
        if (deviceType == DeviceTypeEnum.PL421)
        {           
          J1939ParameterID batteryVoltageParameter = new J1939ParameterID();
          batteryVoltageParameter.PGN = 65271;
          batteryVoltageParameter.SPN = 168;
          batteryVoltageParameter.SourceAddress = 0;
          periodicParameters.Add(batteryVoltageParameter);

          J1939ParameterID kilowattHoursParameter = new J1939ParameterID();
          kilowattHoursParameter.PGN = 65018;
          kilowattHoursParameter.SPN = 2468;
          kilowattHoursParameter.SourceAddress = 234;
          periodicParameters.Add(kilowattHoursParameter);
        }

        API.MTSOutbound.SetConfigureJ1939Reporting(opCtx, gpsDeviceIDs, true, DeviceConfigurationBaseUserDataMessage.ReportType.Periodic, periodicParameters, deviceType, false);

        //Set firmware version based on the asset that the device is mounted on.
        //If it has an RFID plan, we have already sent the firmware configuration, so don't send it again.
        bool hasRFIDPlan = (currentPlanIDs != null) && (from pp in currentPlanIDs where pp.PlanID == (int)ServiceTypeEnum.VisionLinkRFID select 1).Any();
        if (!hasRFIDPlan)
            API.MTSOutbound.SetAssetBasedFirmwareVersion(opCtx, gpsDeviceIDs, deviceType);
      }

      API.MTSOutbound.SendPersonalityRequest(opCtx, gpsDeviceIDs, deviceType);
      Log.IfDebugFormat("DeviceConfig.MTSDefaultConfig: exit");
    }

    private static void TrimTracDefaultConfig(INH_OP opCtx1, string gpsDeviceID)
    {
      Log.IfDebugFormat("DeviceConfig.TrimTracDefaultConfig: enter");
      string[] gpsDeviceIDs = { gpsDeviceID };

      //reset device mileage and runtime to 0
      API.TTOutbound.ResetRuntimeHour(opCtx1, gpsDeviceIDs, 0);

      //Set Reporting Configuration
      int delayTimeout = 1710;//30 mins-90 for latency
      API.TTOutbound.SendDailyReportConfig(opCtx1, gpsDeviceIDs, delayTimeout);
      Log.IfDebugFormat("DeviceConfig.TrimTracDefaultConfig: exit");
    }

    private static bool UpdateMTSSamplingIntervals(INH_OP opCtx1, string gpsDeviceID, TimeSpan samplingInterval, TimeSpan reportingInterval, DeviceTypeEnum deviceType)
    {
      bool success = true;

      if ((reportingInterval.TotalSeconds < 1 && reportingInterval.TotalSeconds > short.MaxValue) || 
          (samplingInterval.TotalSeconds < 1 && samplingInterval.TotalSeconds > short.MaxValue))
      {
        throw new InvalidOperationException(string.Format("Invalid sampling interval; intervals must be between 1 and {0}", short.MaxValue));
      }

      bool hasChanged = false;
      int deviceTypeID = (int) deviceType;

          MTSDevice device = (from d in opCtx1.MTSDevice
                            where d.SerialNumber == gpsDeviceID
                            && d.DeviceType == deviceTypeID
                            select d).FirstOrDefault<MTSDevice>();
        if (device != null)
        {
          if (device.SampleRate != (int)samplingInterval.TotalSeconds)
          {
            device.SampleRate = (int)samplingInterval.TotalSeconds;
            device.UpdateUTC = DateTime.UtcNow;
            hasChanged = true;
          }

          if (device.UpdateRate != (int)reportingInterval.TotalSeconds)
          {
            device.UpdateRate = (int)reportingInterval.TotalSeconds;
            device.UpdateUTC = DateTime.UtcNow;
            hasChanged = true;
          }
        }
        else
        {
          throw new InvalidOperationException("Device does not exist");
        }

        if (hasChanged)
        {
          int result = opCtx1.SaveChanges();

          if (result <= 0)
            throw new InvalidOperationException("Failed to save Sampling Intervals");
        }
      return success;
    }

    private static object GetCurrentPlanIDs(List<ServicePlanIDs> currentPlanIDs)
    {
      if ((currentPlanIDs == null) || (currentPlanIDs.Count == 0))
      {
        return "none";
      }

      StringBuilder buf = new StringBuilder();
      foreach (ServicePlanIDs id in currentPlanIDs)
      {
        buf.Append(id).Append(" ");
      }
      return buf.ToString();
    }

    private static bool EnableRapidReporting(string gpsDeviceId,ED.DeviceTypeEnum deviceType)
    {
      //getting asset start mode
      Log.IfDebugFormat("Publishing Command to DataOut to enable rapid reporting");
      try
      {
        DeviceConfigFactory configFactory = new DeviceConfigFactory();

        DeviceQuery deviceQuery = new DeviceQuery() { GPSDeviceID = gpsDeviceId, DeviceType = deviceType };
        BE.IEnableRapidReportingEvent enableRapidReportingEvent = configFactory.BuildEnableRapidReportingEventForDevice(deviceQuery);
        enableRapidReportingEvent.DeviceId = gpsDeviceId;
        enableRapidReportingEvent.DeviceType = deviceType;
        enableRapidReportingEvent.TimestampUtc = DateTime.UtcNow;
        Log.IfInfoFormat("Publishing IEnableRapidReportingEvent {0} ", JsonConvert.SerializeObject(enableRapidReportingEvent));
        if (ServiceBus == null)
          InstantiateServiceBus();
        ServiceBus.PublishSpecificOf(enableRapidReportingEvent);
      }
      catch (Exception ex)
      {
        Log.IfErrorFormat("Publishing Enable Rapid Reporting event failed for gpsDeviceId {0}: {1}", gpsDeviceId, ex);
        throw;
      }

      Log.IfInfoFormat("Finished publishing Enable Rapid reporting event");
      return true;
    }

    private static bool DisableRapidReporting(string gpsDeviceId, ED.DeviceTypeEnum deviceType)
    {
      //getting asset start mode
      Log.IfDebugFormat("Publishing Command to DataOut to disable rapid reporting");
      try
      {
        DeviceConfigFactory configFactory = new DeviceConfigFactory();
        DeviceQuery deviceQuery = new DeviceQuery() { GPSDeviceID = gpsDeviceId, DeviceType = deviceType };
        BE.IDisableRapidReportingEvent disableRapidReportingEvent = configFactory.BuildDisableRapidReportingEventForDevice(deviceQuery);
        disableRapidReportingEvent.DeviceId = gpsDeviceId;
        disableRapidReportingEvent.DeviceType = deviceType;
        disableRapidReportingEvent.TimestampUtc = DateTime.UtcNow;
        Log.IfInfoFormat("Publishing IDisableRapidReportingEvent {0} ", JsonConvert.SerializeObject(disableRapidReportingEvent));
        if (ServiceBus == null)
          InstantiateServiceBus();
        ServiceBus.PublishSpecificOf(disableRapidReportingEvent);

      }
      catch (Exception ex)
      {
        Log.IfErrorFormat("Publishing Disable Rapid Reporting event failed for gpsDeviceId {0}: {1}", gpsDeviceId, ex);
        throw;
      }

      Log.IfInfoFormat("Finished publishing Disable Rapid reporting event");
      return true;
    }

    private static IServiceBus InstantiateServiceBus()
    {
      IConnectionConfig rabbitMqConnectionConfig = new RabbitMqConnectionConfig();
      int maxProcessorThreads = 5;
      int maxProcessorThreadsOut;

      if (int.TryParse(ConfigurationManager.AppSettings["MaxProcessorThreads"], out maxProcessorThreadsOut))
      {
        maxProcessorThreads = maxProcessorThreadsOut;
      }
      string comGateway = rabbitMqConnectionConfig.ConnectionString(csName: "NH_RABBITMQv2",
                                                           keyVirtualHost: "RabbitMqVirtualHost",
                                                           keyQName: "FlexGatewayRabbitMqName");
      ServiceBus = ServiceBusFactory.New(sbc =>
      {
        sbc.UseRabbitMq(cnfg => cnfg.ConfigureHost(new Uri(comGateway),
          configurator =>
          {
            configurator.
              SetUsername(
                rabbitMqConnectionConfig
                  .GetUserName(
                    keyUser:
                      "FlexGatewayRabbitMqUser"));
            configurator.
              SetPassword(
                rabbitMqConnectionConfig
                  .GetPassword(
                    keyPassword
                      :
                      "FlexGatewayRabbitMqPassword"));
            configurator.SetRequestedHeartbeat(rabbitMqConnectionConfig.GetHeartbeatSeconds());
          }));
        sbc.ReceiveFrom(comGateway);
        sbc.UseControlBus();
        sbc.SetConcurrentConsumerLimit(maxProcessorThreads);
        sbc.SetDefaultSerializer<VssJsonMessageSerializer>();
        sbc.UseLog4Net();
      });

      return ServiceBus;
    }

    #endregion

    public class ServicePlanIDs
    {
      public long PlanID;
      public bool IsCore;

      public override string ToString() {
        return string.Format("PlanID={0}/IsCore={1}", PlanID, IsCore);
      }
    }
  }
}
