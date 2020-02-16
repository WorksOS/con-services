using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using VSS.Hosted.VLCommon.MTSMessages;
using MTS = VSS.Hosted.VLCommon.MTSMessages;

namespace VSS.Hosted.VLCommon
{
  public static class MTSOutboundFormatter
  {
    public static byte[] FormatSendTextMessage(string message, string[] responseSet, uint sequenceID)
    {
      if (string.IsNullOrEmpty(message))
      {
        throw new ArgumentException("Message is empty");
      }
      if (message.Length > 250)
      {
        throw new ArgumentException("Message exceeds 250 characters");
      }

      TextBaseMessage txtMessage = new TextBaseMessage();
      txtMessage.BaseMessageSequenceID = sequenceID;
      txtMessage.Message = message;
      StringBuilder responseSetBuilder = new StringBuilder();
      foreach (string s in responseSet)
      {
        responseSetBuilder.Append("|");
        responseSetBuilder.Append(s);
      }
      txtMessage.ResponseSetText = responseSetBuilder.ToString();
      if (txtMessage.ResponseSetText.Length > 28)
      {
        throw new ArgumentException("Response set exceeds 28 characters");
      }
      txtMessage.UtcDateTime = DateTime.UtcNow;
      return FormatMessage(txtMessage);
    }


    public static byte[] FormatSiteDispatch(long messageSiteID, double? neLat, double? neLong, double? swLat,
      double? swLong, string message, string siteName, long sequenceID)
    {
      if (!neLat.HasValue || double.IsNaN(neLat.Value))
      {
        throw new ArgumentException("NELatitute is not a number");
      }
      if (!neLong.HasValue || double.IsNaN(neLong.Value))
      {
        throw new ArgumentException("NELongitutde is not a number");
      }
      if (!swLat.HasValue || double.IsNaN(swLat.Value))
      {
        throw new ArgumentException("SWLatitute is not a number");
      }
      if (!swLong.HasValue || double.IsNaN(swLong.Value))
      {
        throw new ArgumentException("SWLongitutde is not a number");
      }

      SiteDispatchBaseMessage dispatchMsg = new SiteDispatchBaseMessage();
      dispatchMsg.BaseMessageSequenceID = sequenceID;
      dispatchMsg.SiteID = messageSiteID;
      dispatchMsg.SiteType = PlatformMessage.DeviceSiteType.Home;
      dispatchMsg.NELatitude = neLat.Value;
      dispatchMsg.NELongitude = neLong.Value;
      dispatchMsg.SWLatitude = swLat.Value;
      dispatchMsg.SWLongitude = swLong.Value;
      dispatchMsg.Message = message;
      dispatchMsg.UtcDateTime = DateTime.UtcNow;
      dispatchMsg.SiteName = siteName;
      return FormatMessage(dispatchMsg);
    }

    public static byte[] FormatDIMStart(uint listID, byte numberOfMessages, byte numberOfQuestions, byte numberOfDefaults, uint sequenceID)
    {      
      DimProgrammingUserDataMessage dimUserData = new DimProgrammingUserDataMessage();
      DimProgrammingUserDataMessage.ProgrammingStart startMsg = new DimProgrammingUserDataMessage.ProgrammingStart();
      startMsg.DIMListID = listID;
      startMsg.NumberOfDIMMessages = numberOfMessages;
      startMsg.NumberOfDIMQuestions = numberOfQuestions;
      startMsg.NumberOfDIMDefaults = numberOfDefaults;
      dimUserData.Message = startMsg;
      
      UserDataBaseMessage dimStart = new UserDataBaseMessage();
      dimStart.BaseMessageSequenceID = sequenceID;
      dimStart.UtcDateTime = DateTime.UtcNow;
      dimStart.Message = dimUserData;
      return FormatMessage(dimStart);  
    }

    public static byte[] FormatDIMMessage(uint listID, byte messageNumber, string message, short[] questions, short[] defaults, uint sequenceID)
    {
      DimProgrammingUserDataMessage dimUserData = new DimProgrammingUserDataMessage();
      DimProgrammingUserDataMessage.ProgrammingMessage dimMsg = new DimProgrammingUserDataMessage.ProgrammingMessage();

      dimMsg.DIMListID = listID;
      dimMsg.MessageNumber = messageNumber;
      dimMsg.Message = message;
      dimMsg.Questions = questions;
      dimMsg.Defaults = defaults;
      dimUserData.Message = dimMsg;

      UserDataBaseMessage dimMessage = new UserDataBaseMessage();
      dimMessage.BaseMessageSequenceID = sequenceID;
      dimMessage.UtcDateTime = DateTime.UtcNow;
      dimMessage.Message = dimUserData;

      return FormatMessage(dimMessage);
    }

    public static byte[] FormatSitePurge(long siteID, uint sequenceID)
    {
      SitePurgeBaseMessage purgeMsg = new SitePurgeBaseMessage();
      purgeMsg.SiteID = siteID;
      purgeMsg.SiteType = PlatformMessage.DeviceSiteType.Home;
      purgeMsg.BaseMessageSequenceID = sequenceID;
      purgeMsg.UtcDateTime = DateTime.UtcNow;
      return FormatMessage(purgeMsg);
    }

    public static byte[] FormatSensorConfig(bool sensor1Enabled, bool sensor1IgnRequired, double? sensor1HystHalfSec, bool sensor1HasPosPolarity,
      bool sensor2Enabled, bool sensor2IgnRequired, double? sensor2HystHalfSec, bool sensor2HasPosPolarity,
      bool sensor3Enabled, bool sensor3IgnRequired, double? sensor3HystHalfSec, bool sensor3HasPosPolarity,uint sequenceID)
    {
     
      ConfigureDiscreteInputsBaseUserDataMessage sensorMsg = new ConfigureDiscreteInputsBaseUserDataMessage();
       
      sensorMsg.EnableDiscreteInput1 = sensor1Enabled;
      sensorMsg.IgnitionRequiredInput1 = sensor1IgnRequired;      
      sensorMsg.DiscreteInput1HysteresisHalfSeconds = sensor1HystHalfSec ?? 0;
      sensorMsg.DiscreteInput1HighOne = sensor1HasPosPolarity;

      sensorMsg.EnableDiscreteInput2 = sensor2Enabled;
      sensorMsg.IgnitionRequiredInput2 = sensor2IgnRequired;
      sensorMsg.DiscreteInput2HysteresisHalfSeconds = sensor2HystHalfSec ?? 0;
      sensorMsg.DiscreteInput2HighOne = sensor2HasPosPolarity;
      
      sensorMsg.EnableDiscreteInput3 = sensor3Enabled;
      sensorMsg.IgnitionRequiredInput3 = sensor3IgnRequired;
      sensorMsg.DiscreteInput3HysteresisHalfSeconds = sensor3HystHalfSec ?? 0;
      sensorMsg.DiscreteInput3HighOne = sensor3HasPosPolarity;
        
      UserDataBaseMessage sensor = new UserDataBaseMessage();
      sensor.BaseMessageSequenceID = sequenceID;
      sensor.UtcDateTime = DateTime.UtcNow;
      sensor.Message = sensorMsg;
      return FormatMessage(sensor);
    }

    public static byte[] FormatTPMSConfig(bool isEnabled, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage tpmsConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      tpmsConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.TorchAddonsConfiguration;

      tpmsConfigMsg.torchAddOn.isEnabled = isEnabled;
      tpmsConfigMsg.torchAddOn._addonFeatureCode = 15;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = tpmsConfigMsg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatPositionPoll(DeviceTypeEnum deviceType)
    {
      UpdateRealTimePositionBaseMessage pollMessage = new UpdateRealTimePositionBaseMessage();
      return FormatMessage(pollMessage);
    }

    public static byte[] FormatSetSpeedingThreshold(bool isEnabled, double speedMPH, long durationSec, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage speedingConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      speedingConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.SpeedingReportingConfiguration;
      speedingConfigMsg.SpeedingReporting.ConfigurationFlag = isEnabled;
      speedingConfigMsg.SpeedingReporting.SpeedThreshold = (byte) (int) (Math.Round(speedMPH, 0));
      speedingConfigMsg.SpeedingReporting.DurationThreshold = (short)durationSec;
      UserDataBaseMessage speedingConfig = new UserDataBaseMessage();
      speedingConfig.BaseMessageSequenceID = sequenceID;      speedingConfig.UtcDateTime = DateTime.UtcNow;
      speedingConfig.Message = speedingConfigMsg;
      return FormatMessage(speedingConfig);
    }

    public static byte[] FormatSetStoppedThreshold(bool isEnabled, double speedMPH, long durationSec, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage stoppedConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      stoppedConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.StoppedNotificationConfiguration;
      stoppedConfigMsg.SpeedingReporting.ConfigurationFlag = isEnabled;
      stoppedConfigMsg.SpeedingReporting.SpeedThreshold = (byte) (int) (Math.Round(speedMPH*10, 0));
      stoppedConfigMsg.SpeedingReporting.DurationThreshold = (short)durationSec;
      UserDataBaseMessage stoppedConfig = new UserDataBaseMessage();
      stoppedConfig.BaseMessageSequenceID = sequenceID;
      stoppedConfig.UtcDateTime = DateTime.UtcNow;
      stoppedConfig.Message = stoppedConfigMsg;
      return FormatMessage(stoppedConfig);
    }

    public static byte[] FormatDevicePortConfig(uint sequenceID, string portNumber, string serviceType)
    {
      DeviceConfigurationBaseUserDataMessage devicePortConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      devicePortConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.DevicePortConfiguration;
      devicePortConfigMsg.PortNum = portNumber;
      devicePortConfigMsg.PortConfigType = serviceType;
      UserDataBaseMessage devicePortConfig = new UserDataBaseMessage();
      devicePortConfig.BaseMessageSequenceID = sequenceID;
      devicePortConfig.UtcDateTime = DateTime.UtcNow;
      devicePortConfig.Message = devicePortConfigMsg;
      return FormatMessage(devicePortConfig);
    }

    public static byte[] FormatGeneralDeviceConfig(ushort deviceLogicType, ushort deviceShutdownDelay, ushort mdtShutdownDelay, bool alwaysOnDevice, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage generalDeviceConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      generalDeviceConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.GeneralDeviceConfiguration;
      generalDeviceConfigMsg.GeneralDevice.DeviceLogicType = deviceLogicType;
      generalDeviceConfigMsg.GeneralDevice.DeviceShutdownDelay = deviceShutdownDelay;
      generalDeviceConfigMsg.GeneralDevice.MDTShutdownDelay = mdtShutdownDelay;
      generalDeviceConfigMsg.GeneralDevice.AlwaysOnDevice = alwaysOnDevice;
      UserDataBaseMessage generalDeviceConfig = new UserDataBaseMessage();
      generalDeviceConfig.BaseMessageSequenceID = sequenceID;
      generalDeviceConfig.UtcDateTime = DateTime.UtcNow;
      generalDeviceConfig.Message = generalDeviceConfigMsg;
      return FormatMessage(generalDeviceConfig);
    }

    public static byte[] FormatIPAddressConfig(DeviceConfigurationBaseUserDataMessage.DestinationType ipAddressDestination, string ipAddress, short portNumber, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage ipAddressConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      ipAddressConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.IPAddressConfiguration;
      ipAddressConfigMsg.IPAddress.Destination = ipAddressDestination;
      string hexIP = HexDump.BytesToHexString(IPAddress.Parse(ipAddress).GetAddressBytes());
      hexIP = hexIP.Replace("0x", string.Empty);
      uint addressRaw = uint.Parse(hexIP, System.Globalization.NumberStyles.AllowHexSpecifier);
      ipAddressConfigMsg.IPAddress.AddressRaw = addressRaw;
      ipAddressConfigMsg.IPAddress.Port = portNumber;
      UserDataBaseMessage ipAddressConfig = new UserDataBaseMessage();
      ipAddressConfig.BaseMessageSequenceID = sequenceID;
      ipAddressConfig.UtcDateTime = DateTime.UtcNow;
      ipAddressConfig.Message = ipAddressConfigMsg;
      return FormatMessage(ipAddressConfig);
    }

    public static byte[] FormatNetworkInterfaceConfig(string stackConfig1, string stackConfig2, string stackConfig3, string stackConfig4, string applicationConfig, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage networkInterfaceConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      networkInterfaceConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.NetworkInterfaceConfiguration;
      networkInterfaceConfigMsg.NetworkInterface.StackConfigString1 = stackConfig1;
      networkInterfaceConfigMsg.NetworkInterface.StackConfigString2 = stackConfig2;
      networkInterfaceConfigMsg.NetworkInterface.StackConfigString3 = stackConfig3;
      networkInterfaceConfigMsg.NetworkInterface.StackConfigString4 = stackConfig4;
      networkInterfaceConfigMsg.NetworkInterface.ApplicationConfigString = applicationConfig;
      UserDataBaseMessage networkInterfaceConfig = new UserDataBaseMessage();
      networkInterfaceConfig.BaseMessageSequenceID = sequenceID;
      networkInterfaceConfig.UtcDateTime = DateTime.UtcNow;
      networkInterfaceConfig.Message = networkInterfaceConfigMsg;
      return FormatMessage(networkInterfaceConfig);
    }

    public static byte[] FormatConfigureMachineEvent(uint sequenceID, List<MachineEventConfigBlock> configBlocks)
    {
      DeviceConfigurationBaseUserDataMessage machineEventConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      machineEventConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureMachineEvent;
      machineEventConfigMsg.MachineEventConfigs = new List<DeviceConfigurationBaseUserDataMessage.MachineEventConfig>();
      foreach (MachineEventConfigBlock block in configBlocks)
      {
        DeviceConfigurationBaseUserDataMessage.MachineEventConfig newConfig = new DeviceConfigurationBaseUserDataMessage.MachineEventConfig();
        newConfig.DeliveryMode = block.DeliveryMode;
        newConfig.TriggerMessageType = GetTriggerMessageType(block.Trigger);
        newConfig.ReponseMessageList = GetTriggerResponseMessageList(block.Responses);
        machineEventConfigMsg.MachineEventConfigs.Add(newConfig);
      }

      UserDataBaseMessage machineEventConfig = new UserDataBaseMessage();
      machineEventConfig.BaseMessageSequenceID = sequenceID;
      machineEventConfig.UtcDateTime = DateTime.UtcNow;
      machineEventConfig.Message = machineEventConfigMsg;

      return FormatMessage(machineEventConfig);
    }

    public static string GetTriggerMessageType(TriggerType triggerType)
    {
      switch (triggerType)
      {
        case TriggerType.Ignition:
          return "RD:0A07";
        case TriggerType.DiscreteInput:
          return "RD:0A09";
        case TriggerType.Engine:
          return "RD:0701";
        case TriggerType.Moving:
          return "RD:0A0E";
        case TriggerType.Site:
          return "RD:0A18";
        case TriggerType.Speeding:
          return "RD:0A0D";
        case TriggerType.MSSKeyID:
          return "GW:4605";
        case TriggerType.Daily:
          return "RS:Daily";
        default:
          return string.Empty;
      }
    }

    private static string GetTriggerResponseMessageList(List<TriggerResponse> triggerResponses)
    {
      StringBuilder responseString = new StringBuilder();
      foreach (TriggerResponse response in triggerResponses)
      {
        if (responseString.Length > 0)
          responseString.Append(",");
        responseString.Append(GetTriggerResponseMessage(response));
      }
      return responseString.ToString();
    }

    public static String GetTriggerResponseMessage(TriggerResponse triggerResponse)
    {
      switch (triggerResponse)
      {
        case TriggerResponse.FuelReport:
          return "GW:4500";
        case TriggerResponse.PositionReport:
          return "RD:0700";
        case TriggerResponse.ECMInfo:
          return "GW:5102";
        case TriggerResponse.GatewayAdmin:
          return "GW:5300";
        case TriggerResponse.MaintenanceAdmin:
          return "GW:5301";
        default:
          return string.Empty;
      }
    }

    public static byte[] FormatGatewayRequest(uint sequenceID, List<GatewayMessageType> gatwayMessageToSend)
    {
      GatewayMessageRequest requestMsg = new GatewayMessageRequest();
      requestMsg.BaseMessageSequenceID = sequenceID;
      requestMsg.MessageTypeRequested = new List<string>();
      foreach (GatewayMessageType type in gatwayMessageToSend)
      {
        switch (type)
        {
          case GatewayMessageType.MaintenanceModeInfo:
            requestMsg.MessageTypeRequested.Add("5300");
            break;
          case GatewayMessageType.Diagnostic:
            requestMsg.MessageTypeRequested.Add("2200");
            break;
          case GatewayMessageType.ECMInfo:
            requestMsg.MessageTypeRequested.Add("5102");
            break;
          case GatewayMessageType.FaultCode:
            requestMsg.MessageTypeRequested.Add("2100");
            break;
          case GatewayMessageType.FuelEngine:
            requestMsg.MessageTypeRequested.Add("4500");
            break;
          case GatewayMessageType.DigitalInputInfo:
            requestMsg.MessageTypeRequested.Add("5301");
            break;
          case GatewayMessageType.MSSKeyID:
            requestMsg.MessageTypeRequested.Add("4605");
            break;
          case GatewayMessageType.SMHAdjustment:
            requestMsg.MessageTypeRequested.Add("3A00");
            break;
          case GatewayMessageType.MachineSecurityInfo:
            requestMsg.MessageTypeRequested.Add("5302");
            break;
          case GatewayMessageType.MachineSecurityStartModeInfo:
            requestMsg.MessageTypeRequested.Add("4606");
            break;
          default:
            break;
        }
      }

      return FormatMessage(requestMsg);

    }

    public static byte[] FormatVehicleBusRequest(uint sequenceID, List<VehicleBusMessageType> gatwayMessageToSend)
    {
      VehicleBusMessageRequest requestMsg = new VehicleBusMessageRequest();
      requestMsg.BaseMessageSequenceID = sequenceID;
      requestMsg.MessageTypeRequested = new List<byte>();
      foreach (VehicleBusMessageType type in gatwayMessageToSend)
      {
        requestMsg.MessageTypeRequested.Add((byte)type);
      }
      return FormatMessage(requestMsg);
    }

    public static byte[] FormatConfigureDailyReport(uint sequenceID, bool enabled, byte dailyReportTimeHour, byte dailyReportTimeMinute, string timezoneName)
    {
      DeviceConfigurationBaseUserDataMessage dailyReportMsg = new DeviceConfigurationBaseUserDataMessage();
      dailyReportMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureDailyReport;
      dailyReportMsg.DailyReportConfig.Enabled = enabled;
      dailyReportMsg.DailyReportConfig.DailyReportHour = dailyReportTimeHour;
      dailyReportMsg.DailyReportConfig.DailyReportMinute = dailyReportTimeMinute;
      dailyReportMsg.DailyReportConfig.TimeZoneName = timezoneName;

      UserDataBaseMessage dailyReporConfig = new UserDataBaseMessage();
      dailyReporConfig.BaseMessageSequenceID = sequenceID;
      dailyReporConfig.UtcDateTime = DateTime.UtcNow;
      dailyReporConfig.Message = dailyReportMsg;

      return FormatMessage(dailyReporConfig);
    }

    public static byte[] FormatMovingConfig(ushort radius, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage movingConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      movingConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.MovingConfiguration;
      movingConfigMsg.MovingConfig.Radius = radius;
      movingConfigMsg.MovingConfig.SpareWord1 = 0;
      movingConfigMsg.MovingConfig.SpareWord2 = 0;
      UserDataBaseMessage movingConfig = new UserDataBaseMessage();
      movingConfig.BaseMessageSequenceID = sequenceID;
      movingConfig.UtcDateTime = DateTime.UtcNow;
      movingConfig.Message = movingConfigMsg;
      return FormatMessage(movingConfig);
    }

    public static byte[] FormatDiagnosticReportConfig(bool enableGPSAntenna, bool enableComms, bool enableGPSStatus, bool enableOdometerConfidence, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage diagnosticReportConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      diagnosticReportConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.DiagnosticReportConfiguration;
      byte[] buffer = new byte[10];
      BitShifter.SetValue(enableGPSAntenna, 0, 1, buffer);
      BitShifter.SetValue(enableComms, 1, 1, buffer);
      BitShifter.SetValue(enableGPSStatus, 2, 1, buffer);
      BitShifter.SetValue(enableOdometerConfidence, 3, 1, buffer);
      diagnosticReportConfigMsg.UnknownMessageData = buffer;
      UserDataBaseMessage diagnosticReportConfig = new UserDataBaseMessage();
      diagnosticReportConfig.BaseMessageSequenceID = sequenceID;
      diagnosticReportConfig.UtcDateTime = DateTime.UtcNow;
      diagnosticReportConfig.Message = diagnosticReportConfigMsg;
      return FormatMessage(diagnosticReportConfig);
    }

    public static byte[] FormatHomeSitePositionReportingConfig(ushort homeSiteRadius, byte durationThresholdSeconds, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage homeSitePositionReportingConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      homeSitePositionReportingConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.HomeSitePositionReportingConfiguration;
      homeSitePositionReportingConfigMsg.HomeSitePosition.Radius = homeSiteRadius;
      homeSitePositionReportingConfigMsg.HomeSitePosition.DurationThresholdSeconds = durationThresholdSeconds;
      homeSitePositionReportingConfigMsg.HomeSitePosition.SpareWord1 = 0;
      homeSitePositionReportingConfigMsg.HomeSitePosition.SpareWord2 = 0;
      UserDataBaseMessage homeSitePositionReportingConfig = new UserDataBaseMessage();
      homeSitePositionReportingConfig.BaseMessageSequenceID = sequenceID;
      homeSitePositionReportingConfig.UtcDateTime = DateTime.UtcNow;
      homeSitePositionReportingConfig.Message = homeSitePositionReportingConfigMsg;
      return FormatMessage(homeSitePositionReportingConfig);
    }

    public static byte[] FormatDeviceConfigurationQueryCommand(DeviceConfigurationQueryBaseUserDataMessage.QueryCommand command, uint sequenceID)
    {
      DeviceConfigurationQueryBaseUserDataMessage deviceConfigurationQueryMsg = new DeviceConfigurationQueryBaseUserDataMessage();
      deviceConfigurationQueryMsg.Command = command;
      UserDataBaseMessage deviceConfigurationQuery = new UserDataBaseMessage();
      deviceConfigurationQuery.BaseMessageSequenceID = sequenceID;
      deviceConfigurationQuery.UtcDateTime = DateTime.UtcNow;
      deviceConfigurationQuery.Message = deviceConfigurationQueryMsg;
      return FormatMessage(deviceConfigurationQuery);
    }

    public static byte[] FormatConfigurePolygonMessage(uint sequenceID, DateTime sendUTCTime, MTS.SiteTypeEnum siteType, uint siteID,
      ushort slotNumber, TimeSpan timeToLive, string applicationData, List<Point> points, string siteName, string siteText)
    {
      ConfigurePolygonMessage polygonMsg = new ConfigurePolygonMessage();
      polygonMsg.BaseMessageSequenceID = sequenceID;
      polygonMsg.SendUTC = sendUTCTime;
      polygonMsg.SiteType = siteType;
      polygonMsg.SiteID = siteID;
      polygonMsg.SlotNumber = slotNumber;
      polygonMsg.TimeToLiveHours = timeToLive;
      polygonMsg.ApplicationData = applicationData;
      polygonMsg.Points = new List<ConfigurePolygonMessage.PolygonMessagePoint>();
      foreach (Point p in points)
      {
        ConfigurePolygonMessage.PolygonMessagePoint newPoint = new ConfigurePolygonMessage.PolygonMessagePoint();

        newPoint.Latitude = p.Latitude;
        newPoint.Longitude = p.Longitude;
        polygonMsg.Points.Add(newPoint);
      }
      polygonMsg.SiteName = siteName;
      polygonMsg.SiteText = siteText;

      return FormatMessage(polygonMsg);
    }

    private enum DriverIDItemID
    {
      DriverIDEnabled = 0x01,
      EnableMDTDriverEntry = 0x10,
      ForceEntryAndLogOut = 0x11,
      CharSet = 0x12,
      MdtIDMax = 0x13,
      MdtIDMin = 0x14,
      DisplayedListSize = 0x15,
      StoredListSize = 0x16,
      ForcedLogon = 0x17,
      AutoLogoutInvalid = 0x18,
      AutoLogout = 0x20,
      AutoLogoutTime = 0x21,
      ExpireMRU = 0x30,
      MruExpiry = 0x31,
      ExpireUnvalidatedMRUs = 0x32,
      UnvalidMRUEntry = 0x33,
      DisplayMechanic = 0x40,
      MechanicID = 0xc1,
      MechanicDisplayName = 0xc2,
      EnableLoggedIn = 0x50,
      LoggedInoutputPolarity = 0x51
    }

    class ConfigGroup
    {
      public byte GroupID;
      public byte ItemID;
      public uint Value;
      public byte[] OptionalData;

      public byte[] Serialize()
      {
        int length = 6;
        if (OptionalData != null)
          length += OptionalData.Length;

        byte[] bytes = new byte[length];
        bytes[0] = GroupID;
        bytes[1] = ItemID;
        int bitIndex = 16;
        BitShifter.SetValue(Value, bitIndex, 32, bytes);
        bitIndex += 32;

        if (null != OptionalData)
        {
          ItemID = (byte)(ItemID ^= 0x80);  // Is supposed to set MSB to 1
          bytes[1] = ItemID;

          foreach (byte b in OptionalData)
          {
            BitShifter.SetValue(b, bitIndex, 8, bytes);
            bitIndex += 8;
          }

        }

        return bytes;
      }
    }

    public static byte[] FormatApplicationGeneralConfig(bool manualStatusingMode, uint sequenceID)
    {
      byte applicationGeneralConfigID = 11;
      byte itemID = 0x01;
      List<byte> configList = new List<byte>();
      configList.Add(1);  //this is the number of configuration Items

      DeviceConfigurationBaseUserDataMessage appGeneralConfig = new DeviceConfigurationBaseUserDataMessage();
      appGeneralConfig.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;
      ConfigGroup group = new ConfigGroup() { GroupID = applicationGeneralConfigID, ItemID = itemID, Value = (byte)(manualStatusingMode ? 1 : 0) };
      configList.AddRange(group.Serialize());
      appGeneralConfig.UnknownMessageData = configList.ToArray<byte>();
      UserDataBaseMessage config = new UserDataBaseMessage();
      config.BaseMessageSequenceID = sequenceID;
      config.UtcDateTime = DateTime.UtcNow;
      config.Message = appGeneralConfig;
      return FormatMessage(config);
    }

    public static byte[] FormatGPSEventConfig(bool gpsAntennaFaultEnabled, uint sequenceID)
    {
      byte gpsEventConfigID = 14;
      byte itemID = 0x01;
      List<byte> configList = new List<byte>();
      configList.Add(1);  //this is the number of configuration Items

      DeviceConfigurationBaseUserDataMessage gpsEventConfig = new DeviceConfigurationBaseUserDataMessage();
      gpsEventConfig.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;
      ConfigGroup group = new ConfigGroup() { GroupID = gpsEventConfigID, ItemID = itemID, Value = (byte)(gpsAntennaFaultEnabled ? 1 : 0) };
      configList.AddRange(group.Serialize());
      gpsEventConfig.UnknownMessageData = configList.ToArray<byte>();
      UserDataBaseMessage config = new UserDataBaseMessage();
      config.BaseMessageSequenceID = sequenceID;
      config.UtcDateTime = DateTime.UtcNow;
      config.Message = gpsEventConfig;
      return FormatMessage(config);
    }

    private enum MappingAppItems
    {
      AutoStopArrivalEnabled = 0x01,
      AutoArrivalTimeThreshold = 0x02,
      AutoArrivalDistanceThreshold = 0x03
    }

    public static byte[] FormatMappingAppConfig(bool autoStopEnabled, uint arrivalTimeThresholdSeconds, uint arrivalDistanceThresholdMeters, uint sequenceID)
    {
      byte mappingAppConfigGroupID = 12;
      DeviceConfigurationBaseUserDataMessage mappingAppConfig = new DeviceConfigurationBaseUserDataMessage();
      mappingAppConfig.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;

      List<byte> configList = new List<byte>();
      configList.Add(3);  //this is the number of configuration Items

      ConfigGroup group = new ConfigGroup() { GroupID = mappingAppConfigGroupID, ItemID = (byte)MappingAppItems.AutoStopArrivalEnabled, Value = (byte)(autoStopEnabled ? 1 : 0) };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = mappingAppConfigGroupID, ItemID = (byte)MappingAppItems.AutoArrivalTimeThreshold, Value = arrivalTimeThresholdSeconds };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = mappingAppConfigGroupID, ItemID = (byte)MappingAppItems.AutoArrivalDistanceThreshold, Value = arrivalDistanceThresholdMeters };
      configList.AddRange(group.Serialize());

      mappingAppConfig.UnknownMessageData = configList.ToArray<byte>();
      UserDataBaseMessage config = new UserDataBaseMessage();
      config.BaseMessageSequenceID = sequenceID;
      config.UtcDateTime = DateTime.UtcNow;
      config.Message = mappingAppConfig;
      return FormatMessage(config);
    }

    public static byte[] FormatDriverIDConfig(bool driverIDEnabled, bool enableMDTDriverEntry, bool forceEntryAndLogOut, DriverIDCharSet charSet, byte mdtIDMax, byte mdtIDMin, byte displayedListSize,
      byte storedListSize, bool forcedLogon, bool autoLogoutInvalid, bool autoLogout, TimeSpan autoLogoutTime, bool expireMRU, TimeSpan mruExpiry,
      bool expireUnvalidatedMRUs, TimeSpan unvalidMRUEntry, bool displayMechanic, string mechanicID, string mechanicDisplayName, bool enableLoggedIn, byte loggedInoutputPolarity, uint sequenceID)
    {
      byte DriverIDConfigGroupID = 8;
      DeviceConfigurationBaseUserDataMessage driverConfig = new DeviceConfigurationBaseUserDataMessage();
      driverConfig.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ApplicationLogicConfiguration;
      
      List<byte> configList = new List<byte>();
      configList.Add(21);  //this is the number of configuration Items

      ConfigGroup group = new ConfigGroup(){GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.DriverIDEnabled, Value=(byte)(driverIDEnabled ? 1 : 0)};
      configList.AddRange( group.Serialize() );
      
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.EnableMDTDriverEntry, Value = (byte)(enableMDTDriverEntry ? 1 : 0) };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.ForceEntryAndLogOut, Value = (byte)(forceEntryAndLogOut ? 1 : 0) };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.CharSet, Value = (byte)charSet };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.MdtIDMax, Value = mdtIDMax };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.MdtIDMin, Value = mdtIDMin };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.DisplayedListSize, Value = displayedListSize };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.StoredListSize, Value = storedListSize };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.ForcedLogon, Value = (byte)(forcedLogon ? 1 : 0)};
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.AutoLogoutInvalid, Value = (byte)(autoLogoutInvalid ? 1 : 0) };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.AutoLogout, Value = (byte)(autoLogout ? 1 : 0) };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.AutoLogoutTime, Value = (uint) autoLogoutTime.TotalMinutes };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.ExpireMRU, Value = (byte)(expireMRU ? 1 : 0) };
      configList.AddRange(group.Serialize());
      List<byte> optionalDataList = BitConverter.GetBytes((long)mruExpiry.TotalSeconds).ToList<byte>();

      uint value = (uint)optionalDataList.Count();
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.MruExpiry, Value = value, OptionalData = optionalDataList.ToArray<byte>() };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.ExpireUnvalidatedMRUs, Value = (byte)(expireUnvalidatedMRUs ? 1 : 0) };
      configList.AddRange(group.Serialize());
      optionalDataList.Clear();
      optionalDataList = BitConverter.GetBytes((long)unvalidMRUEntry.TotalSeconds).ToList<byte>();
      value = (uint)optionalDataList.Count();
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.UnvalidMRUEntry, Value = value, OptionalData = optionalDataList.ToArray<byte>() };
      configList.AddRange(group.Serialize());
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.DisplayMechanic, Value = (byte)(displayMechanic ? 1 : 0) };
      configList.AddRange(group.Serialize());
      optionalDataList.Clear();
      optionalDataList = new ASCIIEncoding().GetBytes(mechanicID).ToList<byte>();
      value = (uint)optionalDataList.Count();
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.MechanicID, Value = value, OptionalData = optionalDataList.ToArray<byte>() };
      configList.AddRange(group.Serialize());

      optionalDataList.Clear();
      optionalDataList = new ASCIIEncoding().GetBytes(mechanicDisplayName).ToList<byte>();
      value = (uint)optionalDataList.Count();
      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.MechanicDisplayName, Value = value, OptionalData = optionalDataList.ToArray<byte>() };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.EnableLoggedIn, Value = (byte)(enableLoggedIn ? 1 : 0) };
      configList.AddRange(group.Serialize());

      group = new ConfigGroup() { GroupID = DriverIDConfigGroupID, ItemID = (byte)DriverIDItemID.LoggedInoutputPolarity, Value = loggedInoutputPolarity };
      configList.AddRange(group.Serialize());

      driverConfig.UnknownMessageData = configList.ToArray<byte>();
      UserDataBaseMessage config = new UserDataBaseMessage();
      config.BaseMessageSequenceID = sequenceID;
      config.UtcDateTime = DateTime.UtcNow;
      config.Message = driverConfig;
      return FormatMessage(config);
    }

    public static byte[] FormatZoneLogicConfig(byte entryHomeZoneSpeedMPH, byte entryHomeSiteSpeedMPH, byte entryJobSiteSpeedMPH,
      byte exitHomeZoneSpeedMPH, byte exitHomeSiteSpeedMPH, byte exitJobSiteSpeedMPH, byte hysteresisHomeZoneSeconds, byte hysteresisHomeSiteSeconds,
      byte hysteresisJobSiteSeconds, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage zoneLogicConfig = new DeviceConfigurationBaseUserDataMessage();
      zoneLogicConfig.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ZoneLogicConfiguration;
      
      zoneLogicConfig.ZoneLogic.SitePersistenceFlag = 2;

      zoneLogicConfig.ZoneLogic.HomeZoneEntrySpeed = entryHomeZoneSpeedMPH;
      zoneLogicConfig.ZoneLogic.HomeSiteEntrySpeed = entryHomeSiteSpeedMPH;
      zoneLogicConfig.ZoneLogic.JobSiteEntrySpeed = entryJobSiteSpeedMPH;
      zoneLogicConfig.ZoneLogic.ReservedEntrySpeed = 0;

      zoneLogicConfig.ZoneLogic.HomeZoneExitSpeed = exitHomeZoneSpeedMPH;
      zoneLogicConfig.ZoneLogic.HomeSiteExitSpeed = exitHomeSiteSpeedMPH;
      zoneLogicConfig.ZoneLogic.JobSiteExitSpeed = exitJobSiteSpeedMPH;
      zoneLogicConfig.ZoneLogic.ReservedExitSpeed = 12;
      
      zoneLogicConfig.ZoneLogic.HomeZoneHysteresisSeconds = hysteresisHomeZoneSeconds;
      zoneLogicConfig.ZoneLogic.HomeSiteHysteresisSeconds = hysteresisHomeSiteSeconds;
      zoneLogicConfig.ZoneLogic.JobSiteHysteresisSeconds = hysteresisJobSiteSeconds;
      zoneLogicConfig.ZoneLogic.ReservedHysteresisSeconds = 1;
      
      UserDataBaseMessage config = new UserDataBaseMessage();
      config.BaseMessageSequenceID = sequenceID;
      config.UtcDateTime = DateTime.UtcNow;
      config.Message = zoneLogicConfig;
      return FormatMessage(config);
    }

    public static byte[] FormatMileageRuntime(double mileage, long runtime, uint sequenceID)
    {
      if (double.IsNaN(mileage))
      {
        throw new ArgumentException("Mileage is not a number");
      }

      SetDeviceMileageRunTimeCountersBaseUserDataMessage mileageRuntimeMsg = new SetDeviceMileageRunTimeCountersBaseUserDataMessage();
      mileageRuntimeMsg.Mileage = mileage;
      mileageRuntimeMsg.RunTimeCounterHours = (ushort)runtime;
      UserDataBaseMessage mileageRuntime = new UserDataBaseMessage();
      mileageRuntime.BaseMessageSequenceID = sequenceID;
      mileageRuntime.UtcDateTime = DateTime.UtcNow;
      mileageRuntime.Message = mileageRuntimeMsg;
      return FormatMessage(mileageRuntime);
    }

    public static byte[] FormatPasscode(string passcode, uint sequenceID)
    {
      PasscodeBaseUserDataMessage passcodeMsg = new PasscodeBaseUserDataMessage();
      passcodeMsg.Passcode = passcode;
      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = passcodeMsg;
      return FormatMessage(baseMsg);
    }

    public static byte[] FormatMetricsConfiguration(bool enableNetMetricsReports, bool enableTCPMetricsReports, bool enableGpsMetricsReports,
      bool enableErrorLogReports, TimeSpan networkMetricsMinReportingInterval, TimeSpan networkMetricsMaxReportingInterval, 
      TimeSpan tcpMetricsMinReportingInterval, TimeSpan tcpMetricsMaxReportingInterval, TimeSpan gpsMetricsMinReportingInterval,
      TimeSpan gpsMetricsMaxReportingInterval, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage metricsConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      metricsConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.MetricsConfiguration;
      byte[] buffer = new byte[44];
      BitShifter.SetValue(enableNetMetricsReports, 0, 1, buffer);
      BitShifter.SetValue(enableTCPMetricsReports, 1, 1, buffer);
      BitShifter.SetValue(enableGpsMetricsReports, 2, 1, buffer);
      BitShifter.SetValue(enableErrorLogReports, 3, 1, buffer);
      BitShifter.SetValue((uint)networkMetricsMinReportingInterval.TotalSeconds, 32, 32, buffer);
      BitShifter.SetValue((uint)networkMetricsMaxReportingInterval.TotalSeconds, 64, 32, buffer);
      BitShifter.SetValue((uint)tcpMetricsMinReportingInterval.TotalSeconds, 96, 32, buffer);
      BitShifter.SetValue((uint)tcpMetricsMaxReportingInterval.TotalSeconds, 128, 32, buffer);
      BitShifter.SetValue((uint)gpsMetricsMinReportingInterval.TotalSeconds, 160, 32, buffer);
      BitShifter.SetValue((uint)gpsMetricsMaxReportingInterval.TotalSeconds, 192, 32, buffer);
      metricsConfigMsg.UnknownMessageData = buffer;
      UserDataBaseMessage metricsConfig = new UserDataBaseMessage();
      metricsConfig.BaseMessageSequenceID = sequenceID;
      metricsConfig.UtcDateTime = DateTime.UtcNow;
      metricsConfig.Message = metricsConfigMsg;
      return FormatMessage(metricsConfig);
    }

    public static byte[] FormatStoreForwardConfigurationMessage(bool positionForwardingEnabled, bool outOfNetworkPositionSavingEnabled,
      bool inNetworkPositionSavingEnabled, DeviceConfigurationBaseUserDataMessage.StoreForwardUpdateInterval updateIntervals, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage storeForwardMsg = new DeviceConfigurationBaseUserDataMessage();
      storeForwardMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.StoreForwardConfiguration;
      storeForwardMsg.StoreForwardConfig.PositionForwardingEnabled = positionForwardingEnabled;
      storeForwardMsg.StoreForwardConfig.OutOfNetworkPositionSavingEnabled = outOfNetworkPositionSavingEnabled;
      storeForwardMsg.StoreForwardConfig.InNetworkPositionSavingEnabled = inNetworkPositionSavingEnabled;
      storeForwardMsg.StoreForwardConfig.UpdateInterval = updateIntervals;
      UserDataBaseMessage storeForwardConfig = new UserDataBaseMessage();
      storeForwardConfig.BaseMessageSequenceID = sequenceID;
      storeForwardConfig.UtcDateTime = DateTime.UtcNow;
      storeForwardConfig.Message = storeForwardMsg;
      return FormatMessage(storeForwardConfig);
    }

    public static byte[] FormatMessageAlertConfig(byte alertCount, byte alertDelay, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage messageAlertConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      messageAlertConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.MessageAlertConfiguration;
      messageAlertConfigMsg.MessageAlertConfig.AlertCount = alertCount;
      messageAlertConfigMsg.MessageAlertConfig.AlertDelay = alertDelay;
      messageAlertConfigMsg.MovingConfig.SpareWord1 = 0;
      messageAlertConfigMsg.MovingConfig.SpareWord2 = 0;
      UserDataBaseMessage messageAlertConfig = new UserDataBaseMessage();
      messageAlertConfig.BaseMessageSequenceID = sequenceID;
      messageAlertConfig.UtcDateTime = DateTime.UtcNow;
      messageAlertConfig.Message = messageAlertConfigMsg;
      return FormatMessage(messageAlertConfig);
    }

    public static byte[] FormatIgnitionReportingConfig(bool ignitionReportingEnabled, uint sequenceID)
    {
      DeviceConfigurationBaseUserDataMessage ignitionReportingConfigMsg = new DeviceConfigurationBaseUserDataMessage();
      ignitionReportingConfigMsg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.IgnitionReportingConfiguration;
      ignitionReportingConfigMsg.IgnitionReportingConfig.IgnitionReportingEnabled = ignitionReportingEnabled;
      UserDataBaseMessage ignitionReportingConfig = new UserDataBaseMessage();
      ignitionReportingConfig.BaseMessageSequenceID = sequenceID;
      ignitionReportingConfig.UtcDateTime = DateTime.UtcNow;
      ignitionReportingConfig.Message = ignitionReportingConfigMsg;
      return FormatMessage(ignitionReportingConfig);
    }

    public static byte[] FormatFirmwareUpdateRequestMessage(string directory, string host, string userName, string password, uint sequenceID)
    {
      UpdateFirmwareRequestMessage firmwareUpdate = new UpdateFirmwareRequestMessage();
      firmwareUpdate.Directory = directory;
      firmwareUpdate.Host = host;
      firmwareUpdate.UserName = userName;
      firmwareUpdate.Password = password;
      firmwareUpdate.BaseMessageSequenceID = sequenceID;

      return FormatMessage(firmwareUpdate);
    }

    public static byte[] FormatFirmwareUpdateService(byte fwRequestType, byte? target, bool? forceDirectory, bool? versionNumbersIncluded, string ftpHostName,
      string ftpUserName, string ftpPassword, string sourcePath, string destinationPath, byte? fwMajor, byte? fwMinor, byte? fwBuildType, byte? hwMajor, byte? hwMinor, uint sequenceID)
    {
      OutboundPortBasedDataBaseUserDataMessage port = new OutboundPortBasedDataBaseUserDataMessage();
      FirmwareUpdateServiceCommand firmwareUpdate = new FirmwareUpdateServiceCommand();
      port.PortNumber = 1033;
      firmwareUpdate.FWRequestType = fwRequestType;
      if (fwRequestType == 0)
      {
        firmwareUpdate.Target = (FWTarget)target.Value;
        firmwareUpdate.ForceDirectoryCreation = forceDirectory.Value;
        firmwareUpdate.VersionNumbersIncluded = versionNumbersIncluded.Value;
        firmwareUpdate.FTPHostName = ftpHostName;
        firmwareUpdate.FTPUserName = ftpUserName;
        firmwareUpdate.FTPPassword = ftpPassword;
        firmwareUpdate.SourcePath = sourcePath;
        firmwareUpdate.DestinationPath = destinationPath;
        if (versionNumbersIncluded.Value)
        {
          firmwareUpdate.FWMajorVersion = fwMajor.Value;
          firmwareUpdate.FWMinorVersion = fwMinor.Value;
          firmwareUpdate.FWBuildType = fwBuildType.Value;
          firmwareUpdate.HWMajorVersion = hwMajor.Value;
          firmwareUpdate.HWMinorVersion = hwMinor.Value;
        }
      }
      port.Data = firmwareUpdate.Serialize();

      UserDataBaseMessage portMsg = new UserDataBaseMessage();
      portMsg.BaseMessageSequenceID = sequenceID;
      portMsg.UtcDateTime = DateTime.UtcNow;
      portMsg.Message = port;

      return FormatMessage(portMsg);
    }

    public static byte[] FormatPersonalityRequest(DeviceTypeEnum deviceType)
    {
      RequestPersonalityMessage msg = new RequestPersonalityMessage();

      return FormatMessage(msg);
    }

    public static byte[] FormatOTAConfiguration(InputConfig? input1Config, TimeSpan? input1Delay, string input1Desc,
      InputConfig? input2Config, TimeSpan? input2Delay, string input2Desc,
      InputConfig? input3Config, TimeSpan? input3Delay, string input3Desc,
      InputConfig? input4Config, TimeSpan? input4Delay, string input4Desc, 
      TimeSpan? smu, bool? maintenanceModeEnabled, TimeSpan? maintenanceModeDuration, 
      DigitalInputMonitoringConditions? monitoringCondition1, DigitalInputMonitoringConditions? monitoringCondition2,
      DigitalInputMonitoringConditions? monitoringCondition3, DigitalInputMonitoringConditions? monitoringCondition4,
      uint sequenceID)
    {
      ConfigureGatewayMessage msg = new ConfigureGatewayMessage();
      msg.TransactionType = 0x11;
      msg.TransactionSubType = 0x02;
      msg.TransactionVersion = 0x01;
      msg.MessageSequenceID = (byte)(sequenceID % 255);
      List<NestedMessage> blockList = new List<NestedMessage>();

      GetConfigBlock(input1Config, FieldID.DigitalInput1Config, blockList);
      GetConfigBlock(input1Delay, FieldID.DigitalInput1DelayTime, blockList);
      GetConfigBlock(string.IsNullOrEmpty(input1Desc) ? null : input1Desc.Substring(0,Math.Min(input1Desc.Length, 24)), FieldID.DigitalInput1Description, blockList);
      GetConfigBlock(input2Config, FieldID.DigitalInput2Config, blockList);
      GetConfigBlock(input2Delay, FieldID.DigitalInput2DelayTime, blockList);
      GetConfigBlock(string.IsNullOrEmpty(input2Desc) ? null : input2Desc.Substring(0, Math.Min(input2Desc.Length, 24)), FieldID.DigitalInput2Description, blockList);
      GetConfigBlock(input3Config, FieldID.DigitalInput3Config, blockList);
      GetConfigBlock(input3Delay, FieldID.DigitalInput3DelayTime, blockList);
      GetConfigBlock(string.IsNullOrEmpty(input3Desc) ? null : input3Desc.Substring(0, Math.Min(input3Desc.Length, 24)), FieldID.DigitalInput3Description, blockList);
      GetConfigBlock(input4Config, FieldID.DigitalInput4Config, blockList);
      GetConfigBlock(input4Delay, FieldID.DigitalInput4DelayTime, blockList);
      GetConfigBlock(string.IsNullOrEmpty(input4Desc) ? null : input4Desc.Substring(0, Math.Min(input4Desc.Length, 24)), FieldID.DigitalInput4Description, blockList);
      GetConfigBlock(monitoringCondition1, FieldID.DigitalInput1MonitoringCondition, blockList);
      GetConfigBlock(monitoringCondition2, FieldID.DigitalInput2MonitoringCondition, blockList);
      GetConfigBlock(monitoringCondition3, FieldID.DigitalInput3MonitoringCondition, blockList);
      GetConfigBlock(monitoringCondition4, FieldID.DigitalInput4MonitoringCondition, blockList);
      GetSMUConfigBlock(smu, blockList);
      GetConfigBlock(maintenanceModeEnabled, FieldID.MaintenanceMode, blockList);
      GetDurationConfigBlock(maintenanceModeDuration, blockList);

      msg.Blocks = blockList;

      UserDataBaseMessage otaMsg = new UserDataBaseMessage();
      otaMsg.BaseMessageSequenceID = sequenceID;
      otaMsg.UtcDateTime = DateTime.UtcNow;
      otaMsg.Message = msg;

      return FormatMessage(otaMsg);
    }

    public static byte[] FormatOutboundPortBasedMessage(uint sequenceID)
    {
      OutboundPortBasedDataBaseUserDataMessage msg = new OutboundPortBasedDataBaseUserDataMessage();
      msg.PortNumber = 1027;
      msg.DataString = "QSTAT";

      UserDataBaseMessage port = new UserDataBaseMessage();
      port.BaseMessageSequenceID = sequenceID;
      port.UtcDateTime = DateTime.UtcNow;
      port.Message = msg;

      return FormatMessage(port);
    }

    private static void GetConfigBlock(DigitalInputMonitoringConditions? monitoringCondition, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (monitoringCondition != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.MonitoringCondition = monitoringCondition.Value;
        blockList.Add(block);
      }
    }

    private static void GetConfigBlock(InputConfig? inputConfig, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (inputConfig != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.Configuration = inputConfig.Value;
        blockList.Add(block);
      }
    }

    private static void GetConfigBlock(TimeSpan? inputDelay, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (inputDelay != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.DelayTime = inputDelay.Value;
        blockList.Add(block);
      }
    }

    private static void GetConfigBlock(string inputDesc, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (!string.IsNullOrEmpty(inputDesc))
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.Description = inputDesc;
        blockList.Add(block);
      }
    }

    private static void GetSMUConfigBlock(TimeSpan? SMU, List<NestedMessage> blockList)
    {
      if (SMU != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = FieldID.SMU;
        block.SMU = SMU.Value;
        blockList.Add(block);
      }
    }
    private static void GetConfigBlock(bool? enabled, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (enabled != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.MaintenanceModeEnabled = enabled.Value;
        blockList.Add(block);
      }
    }
    private static void GetDurationConfigBlock(TimeSpan? duration, List<NestedMessage> blockList)
    {
      if (duration != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = FieldID.MaintenanceModeDurationTimer;
        block.MaintenanceModeDuration = duration.Value;
        blockList.Add(block);
      }
    }

    private static void GetMachineStartStatusBlock(MachineStartStatus? machineStartStatus, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (machineStartStatus != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.StartStatus = machineStartStatus.Value;
        blockList.Add(block);
      }
    }

    private static void GetTamperResistanceStatusBlock(TamperResistanceStatus? tamperResistanceStatus, FieldID fieldID, List<NestedMessage> blockList)
    {
      if (tamperResistanceStatus != null)
      {
        OTAConfigParameters block = new OTAConfigParameters();
        block.FieldIdentifier = fieldID;
        block.ResistanceStatus = tamperResistanceStatus.Value;
        blockList.Add(block);
      }
    }

    private static int GetNumBytes(BaseMessage baseMessage)
    {
      uint numbits = 0;
      baseMessage.Serialize(SerializationAction.CalculateLength, null, ref numbits);
      return (int)((numbits + 7) / 8);
    }

    private static byte[] FormatMessage(BaseMessage baseMessage)
    {
      int numbytes = GetNumBytes(baseMessage);

      byte[] formattedbytes = new byte[numbytes];
      uint bitPosition = 0;
      formattedbytes = PlatformMessage.SerializePlatformMessage(baseMessage, null, ref bitPosition, true);
      return formattedbytes;
    }

    enum FWTarget
    {
      ReservedCrossCheckApp = 0,
      EchoDX = 1,
      ReservedCrossCheckAppFramework = 2,
      FileTransfer = 3,
      ARM9App = 4,
      ARM7App = 5,
      MSPApp = 6,
      ARM9BootLoader = 7,
      Arm7BootLoader = 8
    }

    class FirmwareUpdateServiceCommand
    {
      public byte FWRequestType;
      private byte TargetRaw;
      public bool ForceDirectoryCreation;
      public bool VersionNumbersIncluded;
      public string FTPHostName;
      public string FTPUserName;
      public string FTPPassword;
      public string SourcePath;
      public string DestinationPath;
      public byte FWMajorVersion;
      public byte FWMinorVersion;
      public byte FWBuildType;
      public byte HWMajorVersion;
      public byte HWMinorVersion;

      public FWTarget Target
      {
        get { return (FWTarget)TargetRaw; }
        set { TargetRaw = (byte)value; }
      }

      public byte[] Serialize()
      {
        List<byte> fwUpdate = new List<byte>();
        byte[] buffer = new byte[1];
        BitShifter.SetValue(FWRequestType, 0, 8, buffer);
        fwUpdate.AddRange(buffer);
        if (FWRequestType == 0)
        {
          buffer = new byte[1];
          BitShifter.SetValue(TargetRaw, 0, 8, buffer);
          fwUpdate.AddRange(buffer);
          buffer = new byte[1];
          BitShifter.SetValue(ForceDirectoryCreation, 0, 1, buffer);
          BitShifter.SetValue(VersionNumbersIncluded, 1, 1, buffer);
          fwUpdate.AddRange(buffer);
          fwUpdate.AddRange(new ASCIIEncoding().GetBytes(FTPHostName).ToArray<byte>());
          fwUpdate.Add(0);
          fwUpdate.AddRange(new ASCIIEncoding().GetBytes(FTPUserName).ToArray<byte>());
          fwUpdate.Add(0);
          fwUpdate.AddRange(new ASCIIEncoding().GetBytes(FTPPassword).ToArray<byte>());
          fwUpdate.Add(0);
          fwUpdate.AddRange(new ASCIIEncoding().GetBytes(SourcePath).ToArray<byte>());
          fwUpdate.Add(0);
          fwUpdate.AddRange(new ASCIIEncoding().GetBytes(DestinationPath).ToArray<byte>());
          fwUpdate.Add(0);
          if (VersionNumbersIncluded)
          {
            buffer = new byte[1];
            BitShifter.SetValue(FWMajorVersion, 0, 8, buffer);
            fwUpdate.AddRange(buffer);
            buffer = new byte[1];
            BitShifter.SetValue(FWMinorVersion, 0, 8, buffer);
            fwUpdate.AddRange(buffer);
            buffer = new byte[1];
            BitShifter.SetValue(FWBuildType, 0, 8, buffer);
            fwUpdate.AddRange(buffer);
            buffer = new byte[1];
            BitShifter.SetValue(HWMajorVersion, 0, 8, buffer);
            fwUpdate.AddRange(buffer);
            buffer = new byte[1];
            BitShifter.SetValue(HWMinorVersion, 0, 8, buffer);
            fwUpdate.AddRange(buffer);
          }
        }

        return fwUpdate.ToArray<byte>();
      }
    }

    public static byte GetGroupID(byte[] message, out byte itemCount, out byte itemID)
    {
      byte group;
      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] numItems = new byte[1];
          msgReader.Read(numItems, 0, 1);
          itemCount = numItems[0];

          byte[] groupArray = new byte[1];
          msgReader.Read(groupArray, 0, 1);
          group = groupArray[0];

          byte[] itemIDArray = new byte[1];
          msgReader.Read(itemIDArray, 0, 1);
          itemID = itemIDArray[0];

        }
      }
      return group;
    }

    public static byte[] FormatMachineSecuritySystemInformation(DeviceTypeEnum deviceType, uint sequenceID,
        MachineStartStatus? machineStartStatus, TamperResistanceStatus? tamperResistanceStatus)
    {
      ConfigureGatewayMessage msg = new ConfigureGatewayMessage();
      msg.TransactionType = 0x11;
      msg.TransactionSubType = 0x02;
      msg.TransactionVersion = 0x01;
      msg.MessageSequenceID = (byte)(sequenceID % 255);
      List<NestedMessage> blockList = new List<NestedMessage>();

      if (machineStartStatus != null)
        GetMachineStartStatusBlock(machineStartStatus, FieldID.MachineStartMode, blockList);

      if (tamperResistanceStatus != null)
        GetTamperResistanceStatusBlock(tamperResistanceStatus, FieldID.TamperResistanceMode, blockList);

      msg.Blocks = blockList;

      UserDataBaseMessage otaMsg = new UserDataBaseMessage();
      otaMsg.BaseMessageSequenceID = sequenceID;
      otaMsg.UtcDateTime = DateTime.UtcNow;
      otaMsg.Message = msg;

      return FormatMessage(otaMsg);
    }

    public static byte[] FormatRadioMachineSecuritySystemInformation(DeviceTypeEnum deviceType, uint sequenceID,
            MachineStartStatus? machineStartStatus)
    {
        DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
        msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.SetRadioMachineSecuritymode;
        msg.setRadioMachineSecuritymode.MachineStartmode = machineStartStatus.Value;
        //below are the default values assigned as per ICD message documentation
        msg.setRadioMachineSecuritymode.MachineSecurityFeature = DeviceConfigurationBaseUserDataMessage.MachineSecurityFeatureConfiguration.EnableMachineSecurityFeature;
        msg.setRadioMachineSecuritymode.RemoteControlInterface = DeviceConfigurationBaseUserDataMessage.RemoteControlInterface.UsesRelayOutputsdevice;
        //For phase-1, if Machine start mode is equal to 0x02, tamper resistance must set to 0x01. 
        //If VL sends 0x00 accidently in this case, the firmware shall treat it as 0x01. From ICD message document
        msg.setRadioMachineSecuritymode.TamperResistanceMode = TamperResistanceStatus.Off;

      if(msg.setRadioMachineSecuritymode.MachineStartmode == MachineStartStatus.Disabled)
           msg.setRadioMachineSecuritymode.TamperResistanceMode = TamperResistanceStatus.TamperResistanceLevel1;

        msg.setRadioMachineSecuritymode.Relayoutputssetting = DeviceConfigurationBaseUserDataMessage.Relayoutputssetting.RelayOutput1ToActiveState;
                
        UserDataBaseMessage baseMsg = new UserDataBaseMessage();
        baseMsg.BaseMessageSequenceID = sequenceID;
        baseMsg.UtcDateTime = DateTime.UtcNow;
        baseMsg.Message = msg;

        return FormatMessage(baseMsg);
    }

    public static byte[] FormatMainPowerLossReporting(uint sequenceID, bool isEnabled)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.MainPowerLossReporting;
      msg.MainPowerLossReportingConfig.isEnabled = isEnabled;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatConfigureJ1939Reporting(uint sequenceID, bool isEnabled, DeviceConfigurationBaseUserDataMessage.ReportType reportType, J1939ParameterID[] parameters, bool includeSupportingParameters = false)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureJ1939Reporting;

      msg.J1939Reporting.isEnabled = isEnabled;
      msg.J1939Reporting.reportType = reportType;
      msg.J1939Reporting.parameter = parameters;
      msg.J1939Reporting.includeSupportingParameters = includeSupportingParameters;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatJ1939Request(uint sequenceID, J1939ParameterID[] parameters)
    {
      J1939PublicParametersRequest msg = new J1939PublicParametersRequest();
      msg.parameterBlock = parameters;
      msg.sequenceID = sequenceID;

      return FormatMessage(msg);
    }

    public static byte[] FormatRadioTransmitterDisableControl(uint sequenceID, bool isEnabled)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.RadioTransmitterDisableControl;

      msg.radioTransmitter.isEnabled = isEnabled;
     
      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatConfigureMachineEventHeader(uint sequenceID, PrimaryDataSourceEnum primaryDataSource)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.ConfigureMachineEventHeader;

      msg.machineEventHeader.DataSource = primaryDataSource;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatSendDataToDevice(uint sequenceID, DateTime sendUTCTime, SendDataToDevice.ControlType controlType, SendDataToDevice.Destination destination, byte[] data)
    {
      SendDataToDevice msg = new SendDataToDevice();
      msg.sequenceID = sequenceID;
      msg.SendUTC = sendUTCTime;
      msg.controlType = controlType;
      msg.destination = destination;
      msg.data = data;
      return FormatMessage(msg);
    }

    public static byte[] FormatSuspiciousMoveReporting(uint sequenceID, bool isEnabled)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.SuspiciousMoveReporting;

      msg.SuspiciousMoveReportingConfig.isEnabled = isEnabled;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatAssetBasedFirmwareVersion(uint sequenceID, DeviceConfigurationBaseUserDataMessage.AssetBasedFirmwareConfiguration configuration)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.AssetBasedFirmwareVersionConfig;

      msg.firmwareVersionConfig.FirmWareConfiguration = configuration;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);
    }

    public static byte[] FormatRfidConfiguration(uint sequenceID,
      DeviceConfigurationBaseUserDataMessage.RFIDReaderType rfidReaderType,
      DeviceConfigurationBaseUserDataMessage.RFIDReaderStatusType rfidReaderStatus, DeviceConfigurationBaseUserDataMessage.RFIDTriggerSourceType triggerSource,
      UInt16 txRFPower, UInt16 asynOnTime, UInt16 asynOffTime,
      DeviceConfigurationBaseUserDataMessage.AntennaSwitchingMethodType antennaSwitchingMethod,
      DeviceConfigurationBaseUserDataMessage.LinkRateType linkRate,
      DeviceConfigurationBaseUserDataMessage.TariType tari,
      DeviceConfigurationBaseUserDataMessage.MillerValueType millerValue,
      DeviceConfigurationBaseUserDataMessage.SessionForRfidConfigurationType session,
      DeviceConfigurationBaseUserDataMessage.TargetForRfidConfigurationType target,
      bool gen2QHasFixedQValue, byte gen2QFixedQValue,
      DeviceConfigurationBaseUserDataMessage.BaudRateForRfidConfigurationType baudRate,
      DeviceConfigurationBaseUserDataMessage.ReaderOperationRegionForRfidConfigurationType readerOperationRegion)
    {
      DeviceConfigurationBaseUserDataMessage msg = new DeviceConfigurationBaseUserDataMessage();
      msg.SubType = DeviceConfigurationBaseUserDataMessage.ConfigType.RFIDConfiguration;

      msg.RfidConfiguration.ReaderType = rfidReaderType;
      msg.RfidConfiguration.RFIDReaderStatus = rfidReaderStatus;
      msg.RfidConfiguration.TriggerSource = triggerSource;
      msg.RfidConfiguration.TXRFPower = txRFPower;
      msg.RfidConfiguration.AsynOnTime = asynOnTime;
      msg.RfidConfiguration.AsynOffTime = asynOffTime;
      msg.RfidConfiguration.AntennaSwitchingMethod = antennaSwitchingMethod;
      msg.RfidConfiguration.LinkRate = linkRate;
      msg.RfidConfiguration.Tari = tari;
      msg.RfidConfiguration.MillerValue = millerValue;
      msg.RfidConfiguration.Session = session;
      msg.RfidConfiguration.Target = target;
      msg.RfidConfiguration.Gen2QIsFixedQ = gen2QHasFixedQValue;
      msg.RfidConfiguration.Gen2QFixedQValue = gen2QFixedQValue;
      msg.RfidConfiguration.BaudRate = baudRate;
      msg.RfidConfiguration.ReaderOperationRegion = readerOperationRegion;

      UserDataBaseMessage baseMsg = new UserDataBaseMessage();
      baseMsg.BaseMessageSequenceID = sequenceID;
      baseMsg.UtcDateTime = DateTime.UtcNow;
      baseMsg.Message = msg;

      return FormatMessage(baseMsg);

    }

    #region Helper methods for testing

    public static void HydrateDriverIDConfigMessage(byte[] message, out byte itemCount, out byte group, out byte itemID, out bool isEnabled,
      out bool enableMDTDriverEntry, out bool forceEntryAndLogOut, out DriverIDCharSet charSet, out byte mdtIDMax, out byte mdtIDMin, out byte displayedListSize,
      out byte storedListSize, out bool forcedLogon, out bool autoLogoutInvalid, out bool autoLogout, out TimeSpan autoLogoutTime, out bool expireMRU, out TimeSpan mruExpiry,
      out bool expireUnvalidatedMRUs, out TimeSpan unvalidMRUEntry, out bool displayMechanic, out string mechanicID, out string mechanicDisplayName, out bool enableLoggedIn, 
      out byte LoggedInoutputPolarity)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] numItems = new byte[1];
          msgReader.Read(numItems, 0, 1);
          itemCount = numItems[0];

          byte[] groupArray = new byte[1];
          msgReader.Read(groupArray, 0, 1);
          group = groupArray[0];

          byte[] itemIDArray = new byte[1];
          msgReader.Read(itemIDArray, 0, 1);
          itemID = itemIDArray[0];

          byte[] enabledFlag = new byte[4];
          msgReader.Read(enabledFlag, 0, 4);
          isEnabled = enabledFlag[0] == 1;

          byte[] enableMDTDriverEntryArray = new byte[6];
          msgReader.Read(enableMDTDriverEntryArray, 0, 6);
          enableMDTDriverEntry = enableMDTDriverEntryArray[2] == 0x01;

          byte[] forceEntryAndLogOutArray = new byte[6];
          msgReader.Read(forceEntryAndLogOutArray, 0, 6);
          forceEntryAndLogOut = forceEntryAndLogOutArray[2] == 0x01;

          byte[] charSetArray = new byte[6];
          msgReader.Read(charSetArray, 0, 6);
          charSet = (DriverIDCharSet)charSetArray[2];

          byte[] mdtIDMaxArray = new byte[6];
          msgReader.Read(mdtIDMaxArray, 0, 6);
          mdtIDMax = mdtIDMaxArray[2];

          byte[] mdtIDMinArray = new byte[6];
          msgReader.Read(mdtIDMinArray, 0, 6);
          mdtIDMin = mdtIDMinArray[2];

          byte[] displayedListSizeArray = new byte[6];
          msgReader.Read(displayedListSizeArray, 0, 6);
          displayedListSize = displayedListSizeArray[2];

          byte[] storedListSizeArray = new byte[6];
          msgReader.Read(storedListSizeArray, 0, 6);
          storedListSize = storedListSizeArray[2];

          byte[] forcedLogonArray = new byte[6];
          msgReader.Read(forcedLogonArray, 0, 6);
          forcedLogon = forcedLogonArray[2] == 0x01;

          byte[] autoLogoutInvalidArray = new byte[6];
          msgReader.Read(autoLogoutInvalidArray, 0, 6);
          autoLogoutInvalid = autoLogoutInvalidArray[2] == 0x01;

          byte[] autoLogoutArray = new byte[6];
          msgReader.Read(autoLogoutArray, 0, 6);
          autoLogout = autoLogoutArray[2] == 0x01;

          byte[] autoLogoutTimeArray = new byte[6];
          msgReader.Read(autoLogoutTimeArray, 0, 6);
          autoLogoutTime = TimeSpan.FromMinutes(BitConverter.ToInt32(autoLogoutTimeArray, 2));

          byte[] expireMRUArray = new byte[6];
          msgReader.Read(expireMRUArray, 0, 6);
          expireMRU = expireMRUArray[2] == 0x01;

          byte[] optionalArrayLengthArray = new byte[6];
          msgReader.Read(optionalArrayLengthArray, 0, 6);
          int optionalArrayLength = BitConverter.ToInt32(optionalArrayLengthArray, 2);

          byte[] mruExpiryArray = new byte[optionalArrayLength];
          msgReader.Read(mruExpiryArray, 0, optionalArrayLength);
          mruExpiry = TimeSpan.FromSeconds(BitConverter.ToInt64(mruExpiryArray, 0));

          byte[] expireUnvalidatedMRUsArray = new byte[6];
          msgReader.Read(expireUnvalidatedMRUsArray, 0, 6);
          expireUnvalidatedMRUs = expireUnvalidatedMRUsArray[2] == 0x01;

          msgReader.Read(optionalArrayLengthArray, 0, 6);
          optionalArrayLength = BitConverter.ToInt32(optionalArrayLengthArray, 2);

          byte[] unvalidMRUEntryArray = new byte[optionalArrayLength];
          msgReader.Read(unvalidMRUEntryArray, 0, optionalArrayLength);
          unvalidMRUEntry = TimeSpan.FromSeconds(BitConverter.ToInt64(unvalidMRUEntryArray, 0));

          byte[] displayMechanicArray = new byte[6];
          msgReader.Read(displayMechanicArray, 0, 6);
          displayMechanic = displayMechanicArray[2] == 0x01;

          msgReader.Read(optionalArrayLengthArray, 0, 6);
          optionalArrayLength = BitConverter.ToInt32(optionalArrayLengthArray, 2);
          byte[] mechanicIDArray = new byte[optionalArrayLength];
          msgReader.Read(mechanicIDArray, 0, optionalArrayLength);
          mechanicID = new ASCIIEncoding().GetString(mechanicIDArray);

          msgReader.Read(optionalArrayLengthArray, 0, 6);
          optionalArrayLength = BitConverter.ToInt32(optionalArrayLengthArray, 2);
          byte[] mechanicDisplayNameArray = new byte[optionalArrayLength];
          msgReader.Read(mechanicDisplayNameArray, 0, optionalArrayLength);
          mechanicDisplayName = new ASCIIEncoding().GetString(mechanicDisplayNameArray);

          byte[] enableLoggedInArray = new byte[6];
          msgReader.Read(enableLoggedInArray, 0, 6);
          enableLoggedIn = enableLoggedInArray[2] == 0x01;

          byte[] LoggedInoutputPolarityArray = new byte[6];
          msgReader.Read(LoggedInoutputPolarityArray, 0, 6);
          LoggedInoutputPolarity = LoggedInoutputPolarityArray[2];
        }
      }
    }

    public static void HydrateMetricsConfigMessage(byte[] message, out TimeSpan actualnetworkMetricsMinReportingInterval, 
      out TimeSpan actualnetworkMetricsMaxReportingInterval, out TimeSpan actualTCPMetricsMinReportingInterval, 
      out TimeSpan actualTCPMetricsMaxReportingInterval, out TimeSpan actualGPSMetricsMinReportingInterval, 
      out TimeSpan actualGPSMetricsMaxReportingInterval)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] ConfigItems = new byte[4];
          msgReader.Read(ConfigItems, 0, 4);

          byte[] metricsReportingIntervalArray = new byte[4];
          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualnetworkMetricsMinReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));

          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualnetworkMetricsMaxReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));

          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualTCPMetricsMinReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));

          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualTCPMetricsMaxReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));

          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualGPSMetricsMinReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));

          msgReader.Read(metricsReportingIntervalArray, 0, 4);
          actualGPSMetricsMaxReportingInterval = TimeSpan.FromSeconds(BitConverter.ToUInt32(metricsReportingIntervalArray, 0));
        }
      }
    }

    public static void HydrateAppGeneralConfigMessage(byte[] message, out byte itemCount, out byte group, out byte itemID,
     out bool actualManualStatusingMode)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] numItems = new byte[1];
          msgReader.Read(numItems, 0, 1);
          itemCount = numItems[0];

          byte[] groupArray = new byte[1];
          msgReader.Read(groupArray, 0, 1);
          group = groupArray[0];

          byte[] itemIDArray = new byte[1];
          msgReader.Read(itemIDArray, 0, 1);
          itemID = itemIDArray[0];

          itemIDArray = new byte[1];
          msgReader.Read(itemIDArray, 0, 1);
          actualManualStatusingMode = itemIDArray[0] == 1 ? true : false;
        }
      }
    }

    public static void HydrateMappingAppConfigMessage(byte[] message, out byte itemCount, out byte group, out bool actualAutoStopEnabled, out uint actualArrivalTimeThresholdSeconds, out uint actualArrivalDistanceThresholdMeters)
    {
      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] numItems = new byte[1];
          msgReader.Read(numItems, 0, 1);
          itemCount = numItems[0];

          byte[] groupArray = new byte[1];
          msgReader.Read(groupArray, 0, 1);
          group = groupArray[0];

          byte[] itemIDArray = new byte[1];
          msgReader.Read(itemIDArray, 0, 1);

          itemIDArray = new byte[4];
          msgReader.Read(itemIDArray, 0, 4);
          actualAutoStopEnabled = itemIDArray[0] == 1;

          itemIDArray = new byte[6];
          msgReader.Read(itemIDArray, 0, 6);
          actualArrivalTimeThresholdSeconds = BitConverter.ToUInt32(itemIDArray, 2);

          itemIDArray = new byte[6];
          msgReader.Read(itemIDArray, 0, 6);
          actualArrivalDistanceThresholdMeters = BitConverter.ToUInt32(itemIDArray, 2);
        }
      }
    }

    public static void HydrateFirmwareUpdateService(byte[] message, out byte fwRequestType, out byte? target, out bool? forceDirectory, out bool? versionNumbersIncluded, out string ftpHostName,
      out string ftpUserName, out string ftpPassword, out string sourcePath, out string destinationPath, out byte? fwMajor, out byte? fwMinor, out byte? fwBuildType, out byte? hwMajor, out byte? hwMinor)
    {
      target = null; 
      forceDirectory = null;  
      versionNumbersIncluded = null;  
      ftpHostName = null; 
      ftpUserName = null; 
      ftpPassword = null; 
      sourcePath = null; 
      destinationPath = null; 
      fwMajor = null; 
      fwMinor = null; 
      fwBuildType = null; 
      hwMajor = null;
      hwMinor = null; 

      using (MemoryStream stream = new MemoryStream())
      {
        using (BinaryReader msgReader = new BinaryReader(stream, Encoding.Default))
        {
          stream.Write(message, 0, message.Length);
          stream.Seek(0, SeekOrigin.Begin);

          byte[] requestType = new byte[1];
          msgReader.Read(requestType, 0, 1);
          fwRequestType = requestType[0];

          if (fwRequestType == 0)
          {
            requestType = new byte[1];
            msgReader.Read(requestType, 0, 1);
            target = requestType[0];

            requestType = new byte[1];
            msgReader.Read(requestType, 0, 1);
            byte enableFlags = requestType[0];
            forceDirectory = (enableFlags & 1) != 0;
            versionNumbersIncluded = (enableFlags & 2) != 0;

            if (versionNumbersIncluded.Value)
            {
              requestType = new byte[message.Length - 8];
            }
            else
            {
              requestType = new byte[message.Length - 3];
            }
            msgReader.Read(requestType, 0, requestType.Length);
            string s = System.Text.ASCIIEncoding.ASCII.GetString(requestType);
            string[] tempS = s.Split('\0');
            ftpHostName = tempS[0];
            ftpUserName = tempS[1];
            ftpPassword = tempS[2];
            sourcePath = tempS[3];
            destinationPath = tempS[4];

            if (versionNumbersIncluded.Value)
            {
              requestType = new byte[1];
              msgReader.Read(requestType, 0, 1);
              fwMajor = requestType[0];

              requestType = new byte[1];
              msgReader.Read(requestType, 0, 1);
              fwMinor = requestType[0];

              requestType = new byte[1];
              msgReader.Read(requestType, 0, 1);
              fwBuildType = requestType[0];

              requestType = new byte[1];
              msgReader.Read(requestType, 0, 1);
              hwMajor = requestType[0];

              requestType = new byte[1];
              msgReader.Read(requestType, 0, 1);
              hwMinor = requestType[0];
            }
          }
        }
      }
    }
    #endregion
  }
}
