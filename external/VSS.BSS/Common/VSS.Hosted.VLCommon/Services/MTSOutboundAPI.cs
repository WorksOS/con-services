using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.MTSMessages;
using MTS = VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Hosted.VLCommon
{
  public enum DriverIDCharSet
  {
    AlphaOnly = 0x00,
    NumericOnly = 0x01,
    AlphaNumeric = 0x02
  }

  public enum TriggerType
  {
    None,
    Ignition,
    DiscreteInput,
    Speeding,
    Moving,
    Site,
    Engine,
    MSSKeyID,
    Daily
  }

  public enum TriggerResponse
  {
    PositionReport,
    FuelReport,
    ECMInfo,
    GatewayAdmin,
    MaintenanceAdmin,
  }

  public enum GatewayMessageType
  {
    ECMInfo,
    DigitalInputInfo,
    MaintenanceModeInfo,
    FaultCode,
    Diagnostic,
    FuelEngine,
    MSSKeyID,
    SMHAdjustment,
    MachineSecurityInfo,
    MachineSecurityStartModeInfo
  }

  public enum VehicleBusMessageType
  {
    AddressClaim = 1,
    ECMInfo = 2,
    FuelEngine = 3,
    FuelReportSupplemental = 4,
    Diagnostic = 5,
    TireMonitoring = 8
  }

  public class MachineEventConfigBlock
  {
    public DeviceConfigurationBaseUserDataMessage.MachineEventDeliveryMode DeliveryMode;
    public TriggerType Trigger;
    public List<TriggerResponse> Responses;
  }

  public class MTSOutboundAPI : IMTSOutboundAPI
  {
      public bool SendPurgeSites(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatSitePurge(0xFFFFFFFF, sequenceID),
            false, gpsDeviceID, (int)deviceType);
        mtsOut.SequenceID = sequenceID;
        mtsOut.PacketID = SitePurgeBaseMessage.kPacketID;
        msgs.Add(mtsOut);
      }

      AddToMTSOut(opCtx1, msgs, "CC/MTS site assignment messages");

      return success;
    }

    public bool AssignSite(INH_OP opCtx, string[] gpsDeviceIDs, long siteID, bool assign, DeviceTypeEnum deviceType)
    {
      if (siteID <= 0 || gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      Site site = (from s in opCtx.SiteReadOnly
                   where s.ID == siteID
                   select s).FirstOrDefault();

      if (site == null)
        throw new InvalidOperationException("Site is null");

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx, deviceType);
        if (assign)
        {
          if (deviceType == DeviceTypeEnum.CrossCheck)
          {
            MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
              MTSOutboundFormatter.FormatSiteDispatch(site.ID, site.MaxLat, site.MaxLon, site.MinLat, site.MinLon, String.Empty, site.Name, sequenceID),
              false, gpsDeviceID, (int)deviceType);
            mtsOut.SequenceID = sequenceID;
            mtsOut.PacketID = SiteDispatchBaseMessage.kPacketID;
            msgs.Add(mtsOut);
          }
          if (AppFeatureMap.DoesFeatureSetSupportsFeature(DeviceTypeList.GetAppFeatureSetId((int)deviceType), AppFeatureEnum.SiteDispatch))
          {
            MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
             MTSOutboundFormatter.FormatConfigurePolygonMessage(sequenceID, DateTime.UtcNow, MTS.SiteTypeEnum.HomeSite, (uint)site.ID, 0, TimeSpan.FromHours(0), string.Empty, site.PolygonPoints.ToList<Point>(), string.Empty, string.Empty),
             false, gpsDeviceID, (int)deviceType);
            mtsOut.SequenceID = sequenceID;
            mtsOut.PacketID = ConfigurePolygonMessage.kPacketID;
            msgs.Add(mtsOut);
          }
        }
        else
        {
          MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
              MTSOutboundFormatter.FormatSitePurge(site.ID, sequenceID),
              false, gpsDeviceID, (int)deviceType);
          mtsOut.SequenceID = sequenceID;
          mtsOut.PacketID = SitePurgeBaseMessage.kPacketID;
          msgs.Add(mtsOut);
        }
      }

      AddToMTSOut(opCtx, msgs, "CC/MTS site assignment messages");
      return success;
    }

    public bool ConfigureSensors(INH_OP opCtx1, string[] gpsDeviceIDs, bool sensor1Enabled, bool sensor1IgnRequired, double? sensor1HystHalfSec, bool sensor1HasPosPolarity,
      bool sensor2Enabled, bool sensor2IgnRequired, double? sensor2HystHalfSec, bool sensor2HasPosPolarity,
      bool sensor3Enabled, bool sensor3IgnRequired, double? sensor3HystHalfSec, bool sensor3HasPosPolarity, DeviceTypeEnum deviceType)
    {
      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatSensorConfig(sensor1Enabled, sensor1IgnRequired, sensor1HystHalfSec, sensor1HasPosPolarity,
                sensor2Enabled, sensor2IgnRequired, sensor2HystHalfSec, sensor2HasPosPolarity,
                sensor3Enabled, sensor3IgnRequired, sensor3HystHalfSec, sensor3HasPosPolarity, sequenceID),
            false, gpsDeviceID, (int)deviceType);
        mtsOut.SequenceID = sequenceID;
        mtsOut.PacketID = UserDataBaseMessage.kPacketID;
        mtsOut.TypeID = ConfigureDiscreteInputsBaseUserDataMessage.kPacketID;
        msgs.Add(mtsOut);
      }
      AddToMTSOut(opCtx1, msgs, "CC sensor message");
      return success;
    }

    public bool SetRuntimeMileage(INH_OP opCtx1, string[] gpsDeviceIDs, double mileage, long runtime, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);
        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatMileageRuntime(mileage, runtime, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = SetDeviceMileageRunTimeCountersBaseUserDataMessage.kPacketID;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "CC runtime mileage messages");
      return success;
    }

    public bool SetStoppedThreshold(INH_OP opCtx1, string[] gpsDeviceIDs, double threshold, long duration, bool isEnabled, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatSetStoppedThreshold(isEnabled, threshold, duration, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.StoppedNotificationConfiguration;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "CC stopped threshold messages");
      return success;
    }

    public bool SetSpeedingThreshold(INH_OP opCtx1, string[] gpsDeviceIDs, double threshold, long duration, bool isEnabled, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatSetSpeedingThreshold(isEnabled, threshold, duration, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.SpeedingReportingConfiguration;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "CC speeding threshold messages");

      return success;
    }

    public bool SetZoneLogicConfig(INH_OP opCtx1, string[] gpsDeviceIDs,
                 byte entryHomeSiteSpeedMPH, byte exitHomeSiteSpeedMPH, byte hysteresisHomeSiteSeconds, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        byte entryHomeZone = 15;
        byte exitHomeZone = 12;
        byte hysteresisHomeZoneSeconds = 1;


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatZoneLogicConfig(entryHomeZone, entryHomeSiteSpeedMPH, 5,
                                                    exitHomeZone, exitHomeSiteSpeedMPH, 12, hysteresisHomeZoneSeconds,
                                                    hysteresisHomeSiteSeconds, 1, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ZoneLogicConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(opCtx1, msgs, "CC Zone Logic messages");
      return success;
    }

    public bool SetGeneralDeviceConfig(INH_OP opCtx1, string[] gpsDeviceIDs, ushort deviceShutdownDelaySeconds, ushort mdtShutdownDelaySeconds, bool alwaysOnDevice, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      //Only 'Generic' device logic type is supported
      ushort deviceLogicType = 65535;

      List<MTSOut> msgs = new List<MTSOut>(); 
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatGeneralDeviceConfig(deviceLogicType, deviceShutdownDelaySeconds, mdtShutdownDelaySeconds, alwaysOnDevice, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.GeneralDeviceConfiguration;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "CC General Config CCOut");
      return success;
    }


    public bool SetMovingConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, ushort radius, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>(); 
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatMovingConfig(radius, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MovingConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(opCtx1, msgs, "CC Moving Config CCOut");

      return success;
    }

    public bool SetIgnitionReportingConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs,
      bool ignitionReportingEnabled, DeviceTypeEnum deviceType)
    {
      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {

        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatIgnitionReportingConfig(ignitionReportingEnabled, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.IgnitionReportingConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(opCtx1, msgs, "CC Ignition Reporting Config messages");

      return success;
    }

    public bool SendPersonalityRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");


      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
                                                MTSOutboundFormatter.FormatPersonalityRequest(deviceType),
                                                false, gpsDeviceID, (int)deviceType);
        MTSOut.PacketID = RequestPersonalityMessage.kPacketID;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "Firmware Update Command to CCOut");

      return success;
    }

    public bool SendOTAConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs,
      InputConfig? input1Config, TimeSpan? input1Delay, string input1Desc,
      InputConfig? input2Config, TimeSpan? input2Delay, string input2Desc,
      InputConfig? input3Config, TimeSpan? input3Delay, string input3Desc,
      InputConfig? input4Config, TimeSpan? input4Delay, string input4Desc,
      TimeSpan? smu, bool? maintenanceModeEnabled, TimeSpan? maintenanceModeDuration, DigitalInputMonitoringConditions? input1MonitoringCondition,
      DigitalInputMonitoringConditions? input2MonitoringCondition, DigitalInputMonitoringConditions? input3MonitoringCondition,
      DigitalInputMonitoringConditions? input4MonitoringCondition,
      DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
          uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatOTAConfiguration(input1Config, input1Delay, input1Desc,
          input2Config, input2Delay, input2Desc, input3Config,
          input3Delay, input3Desc, input4Config, input4Delay, input4Desc,
          smu, maintenanceModeEnabled, maintenanceModeDuration, input1MonitoringCondition,
          input2MonitoringCondition, input3MonitoringCondition,
          input4MonitoringCondition, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = ConfigureGatewayMessage.kPacketID;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(opCtx1, msgs, "Configure Gateway to raw Out table");

      return success;
    }

    public bool SendDailyReportConfig(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, bool enabled,
      byte dailyReportTimeHour, byte dailyReportTimeMinute, string timezoneName)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

       
          MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatConfigureDailyReport(sequenceID, enabled, dailyReportTimeHour, dailyReportTimeMinute,
            timezoneName),
            false, gpsDeviceID, (int)deviceType);
          MTSOut.SequenceID = sequenceID;
          MTSOut.PacketID = UserDataBaseMessage.kPacketID;
          MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
          MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureDailyReport;
          msgs.Add(MTSOut);
        
      }

      AddToMTSOut(opCtx1, msgs, "Machine Event Config Out Table");
      return success;
    }

    public bool SendGatewayRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<GatewayMessageType> gatewayMessageTypes)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

       
          MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
             MTSOutboundFormatter.FormatGatewayRequest(sequenceID, gatewayMessageTypes),
            false, gpsDeviceID, (int)deviceType);
          MTSOut.SequenceID = sequenceID;
          MTSOut.PacketID = GatewayMessageRequest.kPacketID;
          msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "Machine Event Config Out Table");
      return success;
    }

    public bool SendVehicleBusRequest(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, List<VehicleBusMessageType> gatewayMessageTypes)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
           MTSOutboundFormatter.FormatVehicleBusRequest(sequenceID, gatewayMessageTypes),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = VehicleBusMessageRequest.kPacketID;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "Machine Event Config Out Table");
      return success;
    }

    public bool CalibrateDeviceRuntime(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, double newRuntimeHours)
    {
      return CalibrationHelper.CalibrateDeviceRuntime(opCtx1, gpsDeviceIDs, deviceType, newRuntimeHours);
    }

    private DeviceTypeEnum GetDeviceTypeEnum(INH_OP opCtx, string gps)
    {
      var deviceType = (from d in opCtx.Device
                        where d.GpsDeviceID == gps
                        && (d.fk_DeviceTypeID == (int)DeviceTypeEnum.Series521 
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.Series522 
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.Series523 
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.SNM940
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.SNM941
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL420
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL421
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.PL431
                          || d.fk_DeviceTypeID == (int)DeviceTypeEnum.SNM451)
                        select d.fk_DeviceTypeID).FirstOrDefault();
      return (DeviceTypeEnum)Enum.Parse(typeof(DeviceTypeEnum), deviceType.ToString());
    }

    public bool SendMachineSecuritySystemInformationMessage(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType,
        MachineStartStatus? machineStartStatus, TamperResistanceStatus? tamperResistanceStatus)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatMachineSecuritySystemInformation(deviceType, sequenceID, machineStartStatus, tamperResistanceStatus),
            false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = ConfigureGatewayMessage.kPacketID;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "Configure Gateway to raw Out table");
      return success;
    }

    public void SetMainPowerLossReporting(INH_OP opCtx1, string[] gpsDeviceIDs, bool powerLossReportingEnabled, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatMainPowerLossReporting(sequenceID, powerLossReportingEnabled), false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MainPowerLossReporting;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(opCtx1, msgs, "Main Power Loss Reporting");
    }

    public void SetSuspiciousMove(INH_OP opCtx1, string[] gpsDeviceIDs, bool suspiciousMoveEnabled, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatSuspiciousMoveReporting(sequenceID, suspiciousMoveEnabled),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.SuspiciousMoveReporting;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "Suspicious move reporting");

    }

    public void SetConfigureJ1939Reporting(INH_OP opCtx1, string[] gpsDeviceIDs, bool reportingEnabled, DeviceConfigurationBaseUserDataMessage.ReportType reportType, List<J1939ParameterID> parameters, DeviceTypeEnum deviceType, bool includeSupportingParameters = false)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatConfigureJ1939Reporting(sequenceID, reportingEnabled, reportType, parameters.ToArray(), includeSupportingParameters),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureJ1939Reporting;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "Configure J1939 Reporting");

    }

    public void SetMachineEventHeaderConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs, PrimaryDataSourceEnum primaryDataSource, DeviceTypeEnum deviceType)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatConfigureMachineEventHeader(sequenceID, primaryDataSource),
            false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureMachineEventHeader;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "Configure Machine Event Header Odometer");
    }

    public void SetAssetBasedFirmwareVersion(INH_OP opCtx1, string[] gpsDeviceIDs, DeviceTypeEnum deviceType, bool RFIDServicePlanAdded = false)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration configuration;
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);
        //Configuration is based on the asset type-- by product family        
        //Device device = Services.Devices().GetDeviceByGpsDeviceId(gpsDeviceID, deviceType);
        //if (device == null)
        //  continue;
        //var existingDevice = Services.Devices().GetDeviceByIbKey(device.IBKey);
        //string productFamily = Services.Assets().GetAssetProductFamilyInformation(existingDevice.Asset.MakeCode, existingDevice.Asset.SerialNumber);
        if (deviceType == DeviceTypeEnum.SNM451 && !RFIDServicePlanAdded)
        {
          configuration = DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL420VocationalTrucks;
        }
        else if (deviceType == DeviceTypeEnum.SNM451)
        {
          configuration = DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL4XXRFID;
        }
        else
        {

          string productFamily = null;
          using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
          {
            var assetInfo = (from a in ctx.AssetReadOnly
                             join d in ctx.DeviceReadOnly on a.fk_DeviceID equals d.ID
                             where d.GpsDeviceID == gpsDeviceID
                                   && d.fk_DeviceTypeID == (int) deviceType
                             select new
                                      {
                                        MakeCode = a.fk_MakeCode,
                                        SerialNumber = a.SerialNumberVIN
                                      }).FirstOrDefault();

            var modelInfo = new OemAssetInformationStrategy(ctx).GetAssetModelInformation(assetInfo.MakeCode,
                                                                                          assetInfo.SerialNumber);
            if (modelInfo != null)
            {
              productFamily = modelInfo.ProductFamilyName;
            }
          }

          //set the configuration based on the product family. 
          //hard code for now and then consult with others for the best practice    

          switch (productFamily)
          {
            case "ONHT":
              configuration =
                DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL420VocationalTrucks;
              break;
            case "GEN":
              configuration = DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL420EPD;
              break;
            default:
              configuration = DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration.PL421BCP;
              break;
          }
        }

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatAssetBasedFirmwareVersion(sequenceID, configuration),
            false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.AssetBasedFirmwareVersionConfig;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "Configure Firmware based on the asset type for PL42x");
    }
		

    public void SendRFIDConfiguration(INH_OP opCtx1, string[] gpsDeviceIDs,
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
      DeviceTypeEnum deviceType)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(opCtx1, deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int) MessageStatusEnum.Pending,
                                            MTSOutboundFormatter.FormatRfidConfiguration(sequenceID, rfidReaderType,
                                                                                         rfidReaderStatus, triggerSource,
                                                                                         txRFPower, asynOnTime,
                                                                                         asynOffTime,
                                                                                         antennaSwitchingMethod,
                                                                                         linkRate,
                                                                                         tari,
                                                                                         millerValue,
                                                                                         session,
                                                                                         target,
                                                                                         gen2QHasFixedQValue,
                                                                                         gen2QFixedQValue,
                                                                                         baudRate,
                                                                                         readerOperationRegion),
                                                                                         false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.RFIDConfiguration;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(opCtx1, msgs, "RFID configuration.");
    }
		
    #region unused
		/*
    public bool ConfigureTPMS(string gpsDeviceID, bool isEnabled, DeviceTypeEnum deviceType)
    {
      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

      MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatTPMSConfig(isEnabled,sequenceID),
            false, gpsDeviceID, (int)deviceType);
      mtsOut.SequenceID = sequenceID;
      mtsOut.PacketID = UserDataBaseMessage.kPacketID;
      mtsOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
      mtsOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.TorchAddonsConfiguration;
      msgs.Add(mtsOut);
      AddToMTSOut(msgs, "TPMS config message");
      return success;
    }
		 * */
		/*
    public bool PollPosition(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      bool success = true;


      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
        MTSOutboundFormatter.FormatPositionPoll(deviceType),
        false, gpsDeviceID, (int)deviceType);
      MTSOut.PacketID = UpdateRealTimePositionBaseMessage.kPacketID;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC posn poll");
      }

      return success;
    }*/

		/*
    public bool SendPasscode(string gpsDeviceID, string passcode, DeviceTypeEnum deviceType)
    {
      bool success = true;
      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);
      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
        MTSOutboundFormatter.FormatPasscode(passcode, sequenceID),
        false, gpsDeviceID, (int)deviceType);
      MTSOut.SequenceID = sequenceID;
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.TypeID = PasscodeBaseUserDataMessage.kPacketID;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC passcode");
      }
      
      return success;
    }
		*/
		/*
    public bool SetDevicePortConfig(string[] gpsDeviceIDs,
      string portNumber, string serviceType, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatDevicePortConfig(sequenceID, portNumber, serviceType),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.DevicePortConfiguration;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(msgs, "CC Device Port Config messages");

      return success;
    }
		*/
		/*
    public bool SetPrimaryIPAddressConfiguration(string[] gpsDeviceIDs, string ipAddress, bool isTCP, DeviceTypeEnum deviceType, short? otherPort = null)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      int deviceTypeID = (int)deviceType;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        short port = isTCP ? (short)2189 : !otherPort.HasValue ? (short)2188 : otherPort.Value;

        if(deviceType == DeviceTypeEnum.CrossCheck)
          port = isTCP ? (short)1225 : (short)1120;
        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatIPAddressConfig(DeviceConfigurationBaseUserDataMessage.DestinationType.Primary, ipAddress, port, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.IPAddressConfiguration;
        msgs.Add(MTSOut);

        //Also update TCP/UDP flag if required
        using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
        {
          MTSDevice mtsDevice = (from d in rawCtx.MTSDevice
                                 where d.SerialNumber == gpsDeviceID
                                 && d.DeviceType == deviceTypeID
                                 select d).FirstOrDefault<MTSDevice>();
          if (mtsDevice == null)
            throw new InvalidOperationException(string.Format("Device {0} doesn't exist", gpsDeviceID));
          if (mtsDevice.IsTCP != isTCP)
          {
            mtsDevice.IsTCP = isTCP;
            rawCtx.SaveChanges();
          }
        }
      }
      AddToMTSOut(msgs, "CC Primary IP CCOut");

      return success;
    }*/
		/*
    public bool CancelFirmwareRequestMessage(INH_OP opCtx, string[] gpsDeviceIDs, DateTime? dueUTC)
    {
      //DueUTC is used as a way to only cancel certain firmware updates if it is not null it will only update firmware updates that have
      // a duedate >= dueUTC if it is null it will update all that are still pending or sent

      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      //bool success = false;
      bool recordsFound = false;
      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        foreach (string gpsDeviceID in gpsDeviceIDs)
        {
          int dType = (int)GetDeviceTypeEnum(opCtx, gpsDeviceID);
          int status = (int)MessageStatusEnum.Sent;
          List<MTSOut> MTSOutMsgs = (from p in rawCtx.MTSOut
                                     where p.PacketID == UpdateFirmwareRequestMessage.kPacketID
                                     && p.Status <= status
                                     && p.SerialNumber == gpsDeviceID
                                     && p.DeviceType == dType
                                     select p).ToList<MTSOut>();

          DeviceFirmwareVersion deviceFirmwareVersion = (from df in opCtx.DeviceFirmwareVersion
                                                         join d in opCtx.Device on df.fk_DeviceID equals d.ID
                                                         where d.GpsDeviceID == gpsDeviceID
                                                         && d.fk_DeviceTypeID == dType
                                                         select df).FirstOrDefault<DeviceFirmwareVersion>();
          foreach (MTSOut MTSOut in MTSOutMsgs)
          {
            recordsFound = true;
            if (dueUTC.HasValue && MTSOut.DueUTC.HasValue && MTSOut.DueUTC >= dueUTC.Value)
            {
              MTSOut.Status = status;
              MTSOut.SentCount = byte.MaxValue;
              deviceFirmwareVersion.fk_FirmwareUpdateStatusID = (int) FirmwareUpdateStatusEnum.Cancelled;
              deviceFirmwareVersion.UpdateStatusUTC = DateTime.UtcNow;
            }
            else if (!dueUTC.HasValue)
            {
              MTSOut.Status = status;
              MTSOut.SentCount = byte.MaxValue;
              deviceFirmwareVersion.fk_FirmwareUpdateStatusID = (int)FirmwareUpdateStatusEnum.Cancelled;
              deviceFirmwareVersion.UpdateStatusUTC = DateTime.UtcNow;
            }
          }
        }

        //make sure atleast one record found, and call the save changes. Otherwise it will throw the exception.
        if (recordsFound && rawCtx.SaveChanges() <= 0)
          throw new InvalidOperationException("Failed to Cancel Firmware Update Command to MTSOut");
      }

      //make sure atleast one record found, and call the save changes. Otherwise it will throw the exception.
      if (recordsFound && opCtx.SaveChanges() <= 0)
        throw new InvalidOperationException("Failed to Cancel Firmware Update Command to MTSOut");

      //return true if any firmware update is cancelled ortherwise false
      return recordsFound;
    }
		*/
		/*
    public bool SendPort1033SkylineFirmWareUpdateCommand(string gpsDeviceID, byte fwRequest, byte? target, bool? forceDirectory, bool? versionNumbersIncluded, string ftpHostName,
      string ftpUserName, string ftpPassword, string sourcePath, string destinationPath, byte? fwMajor, byte? fwMinor, byte? fwBuildType, byte? hwMajor, byte? hwMinor, DeviceTypeEnum deviceType)
    {
      bool success = true;
      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
                                          MTSOutboundFormatter.FormatFirmwareUpdateService(fwRequest, target, forceDirectory,
                                                                                           versionNumbersIncluded, ftpHostName,
                                                                                           ftpUserName, ftpPassword, sourcePath,
                                                                                           destinationPath, fwMajor, fwMinor, fwBuildType,
                                                                                           hwMajor, hwMinor, sequenceID),
                                          false, gpsDeviceID, (int)deviceType);
      MTSOut.SequenceID = sequenceID;
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.TypeID = OutboundPortBasedDataBaseUserDataMessage.kPacketID;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save Firmware Update Command to CCOut");
      }
      return success;
    }
		*/
		/*
    public bool SendMachineEventConfig(string[] gpsDeviceIDs, DeviceTypeEnum deviceType,
      List<MachineEventConfigBlock> configBlocks)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatConfigureMachineEvent(sequenceID, configBlocks),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureMachineEvent;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(msgs, "Machine Event Config Out Table");
      return success;
    }
		*/
		/*
    public bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, String newAPN, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      //string proxyAPN = ";proxy;2,4,3,0,31;0,0,0,0,0-;;1500;6000;60000;60000;F;T;F-TCP;0-";
      //string privateAPN = ";tms.trimble.com;2,4,3,0,31;0,0,0,0,0-;;1500;6000;60000;60000;F;T;F-TCP;0-";
      string appConfig = "60";
      
      string[] apn = newAPN.Split('-');
      string stackConfig1 = apn[0];
      string stackConfig2 = apn[1];
      string stackConfig3 = apn[2];
      string stackConfig4 = apn[3];

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
           MTSOutboundFormatter.FormatNetworkInterfaceConfig(stackConfig1, stackConfig2, stackConfig3, stackConfig4, appConfig, sequenceID),
           false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.NetworkInterfaceConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(msgs, "CC Network Config CCOut");

      return true;
    }
		 */
		/*ConfigureTPMS
    public bool SetNetworkInterfaceConfiguration(string[] gpsDeviceIDs, string stackConfig1, string stackConfig2, string stackConfig3, string stackConfig4, string AppConfig, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
           MTSOutboundFormatter.FormatNetworkInterfaceConfig(stackConfig1, stackConfig2, stackConfig3, stackConfig4, AppConfig, sequenceID),
           false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.NetworkInterfaceConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(msgs, "CC Network Config CCOut");

      return success;
    }
		
    public bool QueryBITReport(string[] gpsDeviceIDs,
      DeviceConfigurationQueryBaseUserDataMessage.QueryCommand whichReport, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int) MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatDeviceConfigurationQueryCommand(whichReport, sequenceID),
          false, gpsDeviceID, (int) deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationQueryBaseUserDataMessage.kPacketID;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(msgs, "CC Network Config CCOut");

      return success;
    }
		*/

		/*
    public bool SendRadioMachineSecuritySystemInformationMessage(string[] gpsDeviceIDs, DeviceTypeEnum deviceType,
            MachineStartStatus? machineStartStatus)
    {
        if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
            throw new ArgumentException("Invalid parameters");

        bool success = true;

        List<MTSOut> msgs = new List<MTSOut>();
        foreach (string gpsDeviceID in gpsDeviceIDs)
        {
            uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

            MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
                MTSOutboundFormatter.FormatRadioMachineSecuritySystemInformation(deviceType, sequenceID, machineStartStatus),
                false, gpsDeviceID, (int)deviceType);
            MTSOut.SequenceID = sequenceID;
            MTSOut.PacketID = UserDataBaseMessage.kPacketID;
            MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
            MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.SetRadioMachineSecuritymode;
            msgs.Add(MTSOut);
        }

        AddToMTSOut(msgs, "Configure Machine Security modes: Remote Enable or Remote Disable or De-rate");
        return success;
    }
		*/
		/*
    public bool SetDriverIDConfig(INH_OP opCtx, string[] gpsDeviceIDs, bool driverIDEnabled, bool enableMDTDriverEntry, bool forceEntryAndLogOut,
      DriverIDCharSet charSet, byte mdtIDMax, byte mdtIDMin, byte displayedListSize, byte storedListSize, bool forcedLogon,
      bool autoLogoutInvalid, bool autoLogout, TimeSpan autoLogoutTime, bool expireMRU, TimeSpan mruExpiry, bool expireUnvalidatedMRUs,
      TimeSpan unvalidatedExpiry, bool displayMechanic, string mechanicID, string mechanicDisplayName, bool enableLoggedIn, byte loggedInoutputPolarity, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      int result = -1;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatDriverIDConfig(driverIDEnabled, enableMDTDriverEntry, forceEntryAndLogOut,
                                                             charSet, mdtIDMax, mdtIDMin, displayedListSize, storedListSize, forcedLogon,
                                                             autoLogoutInvalid, autoLogout, autoLogoutTime, expireMRU, mruExpiry,
                                                             expireUnvalidatedMRUs, unvalidatedExpiry, displayMechanic, mechanicID,
                                                             mechanicDisplayName, enableLoggedIn, loggedInoutputPolarity, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;
        msgs.Add(MTSOut);

        //commented out because this is for crosscheck and it will come back into play later
        if (deviceType == DeviceTypeEnum.CrossCheck)
        {
          string serialNumber = gpsDeviceID.ToString();
          int crosscheckType = (int)DeviceTypeEnum.CrossCheck;
          DriverIDConfig driverID = (from driver in opCtx.DriverIDConfig
                                     let d = driver.Device
                                     where d.GpsDeviceID == serialNumber &&
                                     d.fk_DeviceTypeID == crosscheckType
                                     select driver).FirstOrDefault<DriverIDConfig>();
          if (driverID == null)
          {
            driverID = new DriverIDConfig{ DriverIDEnabled=driverIDEnabled, MDTDriverEntryEnabled=enableMDTDriverEntry, ForceEntryAndLogOut=forceEntryAndLogOut,
                                           MDTCharset=(int)charSet, MDTIDMax=mdtIDMax, MDTIDMin=mdtIDMin, DisplayedListSize=displayedListSize, StoredListSize=storedListSize, 
                                           ForceDriverEntry=forcedLogon, AutoLogOutInvalids=autoLogoutInvalid, AutoLogOutEnabled=autoLogout, AutoLogOutMinutes=autoLogoutTime.Minutes, 
                                           ExpireMRUEntries=expireMRU, MRUExpirySeconds=mruExpiry.Seconds, ExpireUnvalidated=expireUnvalidatedMRUs, UnvalidatedExpirySeconds=unvalidatedExpiry.Seconds, 
                                           DisplayMechanic=displayMechanic, EnableLoggedInOutput=enableLoggedIn, LoggedInOutputPolarity=loggedInoutputPolarity};

            driverID.Device = (from d in opCtx.Device
                               where d.GpsDeviceID == serialNumber &&
                               d.fk_DeviceTypeID == crosscheckType
                               select d).FirstOrDefault<Device>();
            driverID.MechanicDisplayName = mechanicDisplayName;
            driverID.MechanicID = mechanicID;
          }
          else
          {
            driverID.DriverIDEnabled = driverIDEnabled;
            driverID.MDTDriverEntryEnabled = enableMDTDriverEntry;
            driverID.ForceEntryAndLogOut = forceEntryAndLogOut;
            driverID.MDTCharset = (int)charSet;
            driverID.MDTIDMax = mdtIDMax;
            driverID.MDTIDMin = mdtIDMin;
            driverID.DisplayedListSize = displayedListSize;
            driverID.StoredListSize = storedListSize;
            driverID.ForceDriverEntry = forcedLogon;
            driverID.AutoLogOutInvalids = autoLogoutInvalid;
            driverID.AutoLogOutEnabled = autoLogout;
            driverID.AutoLogOutMinutes = autoLogoutTime.Minutes;
            driverID.ExpireMRUEntries = expireMRU;
            driverID.MRUExpirySeconds = mruExpiry.Seconds;
            driverID.ExpireUnvalidated = expireUnvalidatedMRUs;
            driverID.UnvalidatedExpirySeconds = unvalidatedExpiry.Seconds;
            driverID.DisplayMechanic = displayMechanic;
            driverID.MechanicID = mechanicID;
            driverID.MechanicDisplayName = mechanicDisplayName;
            driverID.EnableLoggedInOutput = enableLoggedIn;
            driverID.LoggedInOutputPolarity = loggedInoutputPolarity;
          }
        }
      }

      AddToMTSOut(msgs, "CC DriverID Configurations");
      result = opCtx.SaveChanges();
      if (result <= 0)
        throw new InvalidOperationException("Failed to save CC DriverID configurations");
      return success;
    }
		*/
		//public bool SendPredefinedMessageList(string[] gpsDeviceIDs, PredefinedMessageList list, DeviceTypeEnum deviceType)
		//{
		//  if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
		//    throw new ArgumentException("Invalid parameters");

		//  bool success = true;

		//  List<MTSOut> msgs = new List<MTSOut>();
		//  foreach (string gpsDeviceID in gpsDeviceIDs)
		//  {
		//    //Send out a Message Start
		//    uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

		//    MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
		//      MTSOutboundFormatter.FormatDIMStart((uint)list.ListID, (byte)list.PredefinedMessage.Count, 0, 0, sequenceID),
		//      false, gpsDeviceID, (int)deviceType);
		//    MTSOut.SequenceID = sequenceID;
		//    MTSOut.PacketID = UserDataBaseMessage.kPacketID;
		//    MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
		//    msgs.Add(MTSOut);

		//    //Send out messages in the list
		//    foreach (PredefinedMessage msg in list.PredefinedMessage)
		//    {
		//      sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

		//      MTSOut MTSOutDIM = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
		//        MTSOutboundFormatter.FormatDIMMessage((uint)list.ListID, (byte)msg.MessageNumber, msg.MessageText, null, null, sequenceID),
		//        false, gpsDeviceID, (int)deviceType);
		//      MTSOutDIM.SequenceID = sequenceID;
		//      MTSOutDIM.PacketID = UserDataBaseMessage.kPacketID;
		//      MTSOutDIM.TypeID = DimProgrammingUserDataMessage.kPacketID;
		//      msgs.Add(MTSOutDIM);
		//    }
		//  }

		//  AddToMTSOut(msgs, "Predefined Message List");

		//  return success;
		//}
		/*
    public bool SendDeviceConfigurationQueryCommand(string[] gpsDeviceIDs, DeviceConfigurationQueryBaseUserDataMessage.QueryCommand command, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatDeviceConfigurationQueryCommand(command, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationQueryBaseUserDataMessage.kPacketID;
        msgs.Add(MTSOut);

      }

      AddToMTSOut(msgs, "CC Device Configuration Query Command CCOut");

      return success;

    }
		*/
		/*
    public bool SendFirmwareRequestMessage(INH_OP opCtx, string[] gpsDeviceIDs, long mtsFirmwareVersionID, string directory, string host, string username, string password, DateTime dueDateUTC)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        DeviceTypeEnum dType = GetDeviceTypeEnum(opCtx, gpsDeviceID);

        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(dType);
        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
                                              MTSOutboundFormatter.FormatFirmwareUpdateRequestMessage(directory, host, username, password, sequenceID),
                                              false, gpsDeviceID, (int)dType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UpdateFirmwareRequestMessage.kPacketID;
        MTSOut.DueUTC = dueDateUTC;
        msgs.Add(MTSOut);
        int deviceTypeID = (int)dType;
        DeviceFirmwareVersion firmware = (from df in opCtx.DeviceFirmwareVersion
                                          join d in opCtx.Device on df.fk_DeviceID equals d.ID
                                          where d.GpsDeviceID == gpsDeviceID
                                          && d.fk_DeviceTypeID == deviceTypeID
                                          select df).FirstOrDefault<DeviceFirmwareVersion>();
        if (firmware == null)
        {
          long deviceID = (from d in opCtx.Device
                           where d.GpsDeviceID == gpsDeviceID
                           && d.fk_DeviceTypeID == deviceTypeID
                           select d.ID).FirstOrDefault<long>();

          if (deviceID == 0)
            throw new InvalidOperationException("Could Not Find GpsDeviceID");

          firmware = new DeviceFirmwareVersion{ UpdateStatusUTC=dueDateUTC};
          firmware.fk_DeviceID = deviceID;
          opCtx.DeviceFirmwareVersion.AddObject(firmware);
        }

        firmware.fk_MTS500FirmwareVersionIDPending = mtsFirmwareVersionID;
        firmware.fk_FirmwareUpdateStatusID = (int)FirmwareUpdateStatusEnum.Pending;
        //Note: For 'Pending' status only the UpdateStatusUTC is the scheduled date. For all other statuses it is when the status is updated.
        firmware.UpdateStatusUTC = dueDateUTC;
      }

      AddToMTSOut(msgs, "Firmware Update Command to MTSOut");

      int result = opCtx.SaveChanges();
      if (result <= 0)
        throw new InvalidOperationException("Failed to save Firmware Update Command to DeviceFirmwareVersion");
      return success;
    }
		*/

		/*
    public void SetRadioTransmitterDisableControl(string[] gpsDeviceIDs, bool isEnabled, DeviceTypeEnum deviceType)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatRadioTransmitterDisableControl(sequenceID, isEnabled),
            false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.RadioTransmitterDisableControl;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(msgs, "Configure Radio Transmitter Disable Control");
    }
		*/

		/*
		 
    public void SendJ1939PublicParametersRequest(string[] gpsDeviceIDs, List<J1939ParameterID> parameters, DeviceTypeEnum deviceType)
    {
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatJ1939Request(sequenceID, parameters.ToArray()),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = J1939PublicParametersRequest.kPacketID;
        msgs.Add(MTSOut);
      }
      AddToMTSOut(msgs, "J1939 public parameters request.");
    }*/
		/*
    public bool SendDeviceData(string[] gpsDeviceIDs, SendDataToDevice.ControlType controlType, SendDataToDevice.Destination destination, byte[] data, DeviceTypeEnum deviceType)
    {
      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        DateTime sendTime = DateTime.UtcNow;
        MTSOut MTSOut = MTSOut.CreateMTSOut(0, sendTime, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatSendDataToDevice(sequenceID, sendTime, controlType, destination, data),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = SendDataToDevice.kPacketID;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(msgs, "CC Moving Config CCOut");
      return success;
    }
		*/
		/*
    public bool SendTextMessage(string[] gpsDeviceIDs, string message, string[] responseSet, out uint[] sequenceIDs, DeviceTypeEnum deviceType)
    {
      sequenceIDs = null;

      if (string.IsNullOrEmpty(message) || gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<uint> seqIDs = new List<uint>();
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);
        seqIDs.Add(sequenceID);

        MTSOut mtsOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
            MTSOutboundFormatter.FormatSendTextMessage(message, responseSet, sequenceID),
            false, gpsDeviceID, (int)deviceType);
        mtsOut.SequenceID = sequenceID;
        mtsOut.PacketID = TextBaseMessage.kPacketID;
        msgs.Add(mtsOut);
      }

      AddToMTSOut(msgs, "CC/MTS send text messages");
      sequenceIDs = seqIDs.ToArray();
      return success;
    }
		*/
		/*
    public bool RequestTCPUDPStats(string gpsDeviceID, DeviceTypeEnum deviceType)
    {
      bool success = true;

      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
        MTSOutboundFormatter.FormatOutboundPortBasedMessage(sequenceID),
        false, gpsDeviceID, (int)deviceType);
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.SubTypeID = OutboundPortBasedDataBaseUserDataMessage.kPacketID;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC posn poll");
      }
      return success;
    }
		*/
		/*
    public bool SetHomeSitePositionReportingConfiguration(string[] gpsDeviceIDs, ushort homeSiteRadius, byte durationThresholdSeconds, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatHomeSitePositionReportingConfig(homeSiteRadius, durationThresholdSeconds, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MovingConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(msgs, "CC HomeSite Config CCOut");
		SetApplicationGeneralConfiguration
      return success;
    }
		*/
		/*
    public bool SetDiagnosticReportConfiguration(string[] gpsDeviceIDs, bool enableGPSAntenna, bool enableComms, bool enableGPSStatus, bool enableOdometerConfidence, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatDiagnosticReportConfig(enableGPSAntenna, enableComms, enableGPSStatus, enableOdometerConfidence, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.DiagnosticReportConfiguration;
        msgs.Add(MTSOut);

      }

      AddToMTSOut(msgs, "CC Diag Report CCOut");

      return success;

    }
		*/
		/*
    public bool SetApplicationGeneralConfiguration(string[] gpsDeviceIDs, bool manualStatusing, DeviceTypeEnum deviceType)
    {
      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
          MTSOutboundFormatter.FormatApplicationGeneralConfig(manualStatusing, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;
        msgs.Add(MTSOut);

      }
      AddToMTSOut(msgs, "CC Application General Config messages");

      return success;
    }
		*/
		/*
    public bool SetGPSEventConfiguration(string gpsDeviceID, bool gpsAntennaFaultEnabled, DeviceTypeEnum deviceType)
    {
      bool success = true;

      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
        MTSOutboundFormatter.FormatGPSEventConfig(gpsAntennaFaultEnabled, sequenceID),
        false, gpsDeviceID, (int)deviceType);
      MTSOut.SequenceID = sequenceID;
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
      MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC GPS Event Config messages");
      }
      return success;
    }		
    public bool SetMappingAppConfiguration(string gpsDeviceID, bool autoStopEnabled,
  uint arrivalTimeThresholdSeconds, uint arrivalDistanceThresholdMeters, DeviceTypeEnum deviceType)
    {
      bool success = true;

      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
        MTSOutboundFormatter.FormatMappingAppConfig(autoStopEnabled, arrivalTimeThresholdSeconds,
              arrivalDistanceThresholdMeters, sequenceID),
        false, gpsDeviceID, (int)deviceType);
      MTSOut.SequenceID = sequenceID;
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
      MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;

      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC Mapping App Config messages");
      }
      return success;
    }

    public bool SetMessageAlertConfiguration(string gpsDeviceID,
      byte alertCount, byte alertDelay, DeviceTypeEnum deviceType)
    {
      bool success = true;

      uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

      MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
         MTSOutboundFormatter.FormatMessageAlertConfig(alertCount, alertDelay, sequenceID),
         false, gpsDeviceID, (int)deviceType);
      MTSOut.SequenceID = sequenceID;
      MTSOut.PacketID = UserDataBaseMessage.kPacketID;
      MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
      MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MessageAlertConfiguration;
      using (INH_RAW rawCtx = ObjectContextFactory.NewNHContext<INH_RAW>())
      {
        rawCtx.MTSOut.AddObject(MTSOut);
        int result = rawCtx.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save CC Message Alert Config messages");
      }
      return success;
    }

    public bool SendMetricsConfiguration(string[] gpsDeviceIDs, bool enableNetMetricsReports, bool enableTCPMetricsReports, bool enableGpsMetricsReports,
  bool enableErrorLogReports, TimeSpan networkMetricsMinReportingInterval, TimeSpan networkMetricsMaxReportingInterval,
  TimeSpan tcpMetricsMinReportingInterval, TimeSpan tcpMetricsMaxReportingInterval, TimeSpan gpsMetricsMinReportingInterval,
  TimeSpan gpsMetricsMaxReportingInterval, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;

      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);


        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
           MTSOutboundFormatter.FormatMetricsConfiguration(enableNetMetricsReports, enableTCPMetricsReports, enableGpsMetricsReports,
            enableErrorLogReports, networkMetricsMinReportingInterval, networkMetricsMaxReportingInterval, tcpMetricsMinReportingInterval,
            tcpMetricsMaxReportingInterval, gpsMetricsMinReportingInterval, gpsMetricsMaxReportingInterval, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationQueryBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.MetricsConfiguration;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(msgs, "CC Device Configuration Query Command CCOut");

      return success;

    }

    public bool SetStoreForwardConfiguration(string[] gpsDeviceIDs, bool positionForwardingEnabled, bool outOfNetworkPositionSavingEnabled, bool inNetworkPositionSavingEnabled, DeviceConfigurationBaseUserDataMessage.StoreForwardUpdateInterval updateIntervals, DeviceTypeEnum deviceType)
    {
      if (gpsDeviceIDs == null || gpsDeviceIDs.Length == 0)
        throw new ArgumentException("Invalid parameters");

      bool success = true;
      List<MTSOut> msgs = new List<MTSOut>();
      foreach (string gpsDeviceID in gpsDeviceIDs)
      {
        uint sequenceID = MessageSequenceAPI.OutboundSequenceID(deviceType);

        MTSOut MTSOut = MTSOut.CreateMTSOut(0, DateTime.UtcNow, (int)MessageStatusEnum.Pending,
           MTSOutboundFormatter.FormatStoreForwardConfigurationMessage(positionForwardingEnabled, outOfNetworkPositionSavingEnabled, inNetworkPositionSavingEnabled, updateIntervals, sequenceID),
          false, gpsDeviceID, (int)deviceType);
        MTSOut.SequenceID = sequenceID;
        MTSOut.PacketID = UserDataBaseMessage.kPacketID;
        MTSOut.TypeID = DeviceConfigurationBaseUserDataMessage.kPacketID;
        MTSOut.SubTypeID = (int)DeviceConfigurationBaseUserDataMessage.ConfigType.StoreForwardConfiguration;
        msgs.Add(MTSOut);
      }

      AddToMTSOut(msgs, "CC Diag Report CCOut");

      return success;
    }
		*/
    private void AddToMTSOut(INH_OP opCtx1, List<MTSOut> msgs, string errorDescrption)
    {
        foreach (MTSOut msg in msgs)
        {
            opCtx1.MTSOut.AddObject(msg);
        }

        int result = opCtx1.SaveChanges();
        if (result <= 0)
          throw new InvalidOperationException("Failed to save " + errorDescrption);
    }

    #endregion

  }
}
