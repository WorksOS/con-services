using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;


namespace VSS.Hosted.VLCommon.MTSMessages
{
    public class SetDeviceMileageRunTimeCountersBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x01;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition, 24, ref MileageRaw);
         serializer(action, raw, ref bitPosition, 15, ref RunTimeCounterHours);
      }

      public Double  Mileage
      {
         get { return ((double) MileageRaw)/(Constants.MileageConversionMultiplier); }
         set { MileageRaw = (UInt32)(value*(Constants.MileageConversionMultiplier)); }
      }

      private UInt32  MileageRaw;
      public UInt16  RunTimeCounterHours;

      public MTSConfigData.MileageRuntimeConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
      {
        MTSConfigData.MileageRuntimeConfig config = new MTSConfigData.MileageRuntimeConfig();
        config.MessageSourceID = messageID;
        config.SentUTC = sentUTC;
        config.Status = status;

        config.Mileage = Mileage;
        config.RuntimeHours = RunTimeCounterHours;

        return config;
      }
   }

   public class PasscodeBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x1B;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         byte PasscodeLength = 0;
         if (action != SerializationAction.Hydrate && Passcode != null)
         {
            PasscodeLength = (byte)Passcode.Length;
         }
         serializer(action, raw, ref bitPosition, 8, ref PasscodeLength);
         serializeFixedLengthString(action, raw, ref bitPosition, PasscodeLength, ref Passcode);
      }

      public string Passcode;

      public MTSConfigData.PasscodeConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
      {
        MTSConfigData.PasscodeConfig config = new MTSConfigData.PasscodeConfig();
        config.MessageSourceID = messageID;
        config.SentUTC = sentUTC;
        config.Status = status;

        config.Passcode = Passcode;

        return config;
      }
   }
  
   public class ConfigureDiscreteInputsBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x02;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  1, ref EnableDiscreteInput1);
         serializer(action, raw, ref bitPosition,  1, ref EnableDiscreteInput2);
         serializer(action, raw, ref bitPosition,  1, ref EnableDiscreteInput3);
         filler    (             ref bitPosition,  5);
         serializer(action, raw, ref bitPosition,  1, ref DiscreteInput1HighOne);
         serializer(action, raw, ref bitPosition,  1, ref DiscreteInput2HighOne);
         serializer(action, raw, ref bitPosition,  1, ref DiscreteInput3HighOne);
         filler    (             ref bitPosition,  5);
         serializer(action, raw, ref bitPosition, 16, ref DiscreteInput1HysteresisRaw);
         serializer(action, raw, ref bitPosition, 16, ref DiscreteInput2HysteresisRaw);
         serializer(action, raw, ref bitPosition, 16, ref DiscreteInput3HysteresisRaw);
         serializer(action, raw, ref bitPosition,  1, ref IgnitionRequiredInput1);
         serializer(action, raw, ref bitPosition,  1, ref IgnitionRequiredInput2);
         serializer(action, raw, ref bitPosition,  1, ref IgnitionRequiredInput3);
         filler    (             ref bitPosition,  5);
         filler    (             ref bitPosition, 16);
      }

      public MTSConfigData.DiscreteInputConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
      {
        MTSConfigData.DiscreteInputConfig config = new MTSConfigData.DiscreteInputConfig();
        config.MessageSourceID = messageID;
        config.SentUTC = sentUTC;
        config.Status = status;

        config.IO1Enabled = EnableDiscreteInput1;
        config.IO2Enabled = EnableDiscreteInput2;
        config.IO3Enabled = EnableDiscreteInput3;
        config.IO1HysteresisHalfSeconds = DiscreteInput1HysteresisHalfSeconds;
        config.IO2HysteresisHalfSeconds = DiscreteInput2HysteresisHalfSeconds;
        config.IO3HysteresisHalfSeconds = DiscreteInput3HysteresisHalfSeconds;
        config.IO1IgnRequired = IgnitionRequiredInput1;
        config.IO2IgnRequired = IgnitionRequiredInput2;
        config.IO3IgnRequired = IgnitionRequiredInput3;
        config.IO1PolarityIsHigh = DiscreteInput1HighOne;
        config.IO2PolarityIsHigh = DiscreteInput2HighOne;
        config.IO3PolarityIsHigh = DiscreteInput3HighOne;

        return config;
      }

      public Double DiscreteInput1HysteresisHalfSeconds
      {
         get { return (double) DiscreteInput1HysteresisRaw; }
         set { DiscreteInput1HysteresisRaw = (UInt16)(value); }
      }

      public Double DiscreteInput2HysteresisHalfSeconds
      {
         get { return (double) DiscreteInput2HysteresisRaw; }
         set { DiscreteInput2HysteresisRaw = (UInt16)(value); }
      }

      public Double DiscreteInput3HysteresisHalfSeconds
      {
         get { return (double) DiscreteInput3HysteresisRaw; }
         set { DiscreteInput3HysteresisRaw = (UInt16)(value); }
      }

      public bool    EnableDiscreteInput1;
      public bool    EnableDiscreteInput2;
      public bool    EnableDiscreteInput3;

      public bool    DiscreteInput1HighOne;
      public bool    DiscreteInput2HighOne;
      public bool    DiscreteInput3HighOne;

      private UInt16  DiscreteInput1HysteresisRaw;
      private UInt16  DiscreteInput2HysteresisRaw;
      private UInt16  DiscreteInput3HysteresisRaw;

      public bool    IgnitionRequiredInput1;
      public bool    IgnitionRequiredInput2;
      public bool    IgnitionRequiredInput3;
   }

    public class DeviceConfigurationBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x15;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public DeviceConfigurationBaseUserDataMessage()
      {
        ZoneLogic.inMessage = false;
        SpeedingReporting.inMessage = false;
        StoreForwardConfig.inMessage = false;
        MessageAlertConfig.inMessage = false;
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  8, ref SubTypeRaw);

         if (SubType == ConfigType.NetworkInterfaceConfiguration) 
         {
            // Network interface message

            lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

            serializeNulTerminatedString(action, raw, ref bitPosition, ref NetworkInterface.StackConfigString1);
            serializeNulTerminatedString(action, raw, ref bitPosition, ref NetworkInterface.StackConfigString2);
            serializeNulTerminatedString(action, raw, ref bitPosition, ref NetworkInterface.StackConfigString3);
            serializeNulTerminatedString(action, raw, ref bitPosition, ref NetworkInterface.StackConfigString4);
            serializeNulTerminatedString(action, raw, ref bitPosition, ref NetworkInterface.ApplicationConfigString);

            configBlockLength.Backfill(bitPosition);
         } 
         else if (SubType == ConfigType.IPAddressConfiguration) 
         {
           IPAddress.inMessage = true;
            serializer(action, raw, ref bitPosition,  8, ref IPAddress.DestinationRaw);
            serializer(action, raw, ref bitPosition, 32, ref IPAddress.AddressRaw);
            serializer(action, raw, ref bitPosition, 16, ref IPAddress.Port);
         }
         else if (SubType == ConfigType.IPFilterConfiguration) 
         {
            IPFilter.AllowedAddresses = (IPFilterConfigurationBlock.AllowedAddress[])
               serializeHomogeneousRunLengthArray(action, raw, ref bitPosition,  8, IPFilter.AllowedAddresses, typeof(IPFilterConfigurationBlock.AllowedAddress));
         }
         else if (SubType == ConfigType.ApplicationLogicConfiguration)
         {
           if (action == SerializationAction.Hydrate)
           {
             uint realDataLength = bytesLeftInMessage(bitPosition);
             serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref UnknownMessageData);
           }
           else
           {
             serializeFixedLengthBytes(action, raw, ref bitPosition, (uint)UnknownMessageData.Length, ref UnknownMessageData);
           }
         }
         else if (SubType == ConfigType.ZoneLogicConfiguration)
         {
           ZoneLogic.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.SitePersistenceFlag);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeZoneEntrySpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeSiteEntrySpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.JobSiteEntrySpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.ReservedEntrySpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeZoneExitSpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeSiteExitSpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.JobSiteExitSpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.ReservedExitSpeed);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeZoneHysteresisSeconds);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.HomeSiteHysteresisSeconds);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.JobSiteHysteresisSeconds);
           serializer(action, raw, ref bitPosition, 8, ref ZoneLogic.ReservedHysteresisSeconds);
         }
         else if (SubType == ConfigType.SpeedingReportingConfiguration || SubType == ConfigType.StoppedNotificationConfiguration)
         {
           SpeedingReporting.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref SpeedingReporting.ConfigurationFlag);
           serializer(action, raw, ref bitPosition, 8, ref SpeedingReporting.SpeedThreshold);
           serializer(action, raw, ref bitPosition, 16, ref SpeedingReporting.DurationThreshold);
         }
         else if (SubType == ConfigType.GeneralDeviceConfiguration)
         {
           GeneralDevice.inMessage = true;
           serializer(action, raw, ref bitPosition, 16, ref GeneralDevice.DeviceLogicType);
           serializer(action, raw, ref bitPosition, 16, ref GeneralDevice.DeviceShutdownDelay);
           serializer(action, raw, ref bitPosition, 16, ref GeneralDevice.MDTShutdownDelay);
           serializer(action, raw, ref bitPosition, 8, ref GeneralDevice.AlwaysOnDevice);
         }
         else if (SubType == ConfigType.MovingConfiguration)
         {
           MovingConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 16, ref MovingConfig.Radius);
           serializer(action, raw, ref bitPosition, 16, ref MovingConfig.SpareWord1);
           serializer(action, raw, ref bitPosition, 16, ref MovingConfig.SpareWord2);
         }
         else if (SubType == ConfigType.HomeSitePositionReportingConfiguration)
         {
           HomeSitePosition.inMessage = true;
           serializer(action, raw, ref bitPosition, 16, ref HomeSitePosition.Radius);
           serializer(action, raw, ref bitPosition, 8, ref HomeSitePosition.DurationThresholdSeconds);
           serializer(action, raw, ref bitPosition, 16, ref HomeSitePosition.SpareWord1);
           serializer(action, raw, ref bitPosition, 16, ref HomeSitePosition.SpareWord2);
         }
         else if (SubType == ConfigType.DevicePortConfiguration)
         {
           serializer(action, raw, ref bitPosition, 8, ref PortNumberRaw);
           serializer(action, raw, ref bitPosition, 8, ref ServiceTypeRaw);
         }
         else if (SubType == ConfigType.StoreForwardConfiguration)
         {
           StoreForwardConfig.inMessage = true; 
           serializer(action, raw, ref bitPosition, 1, ref StoreForwardConfig.PositionForwardingEnabled);
           serializer(action, raw, ref bitPosition, 1, ref StoreForwardConfig.OutOfNetworkPositionSavingEnabled);
           serializer(action, raw, ref bitPosition, 1, ref StoreForwardConfig.InNetworkPositionSavingEnabled);
           filler(ref bitPosition, 5);
           serializer(action, raw, ref bitPosition, 4, ref StoreForwardConfig.UpdateIntervals);
           serializer(action, raw, ref bitPosition, 4, ref StoreForwardConfig.UpdateIntervals);
         }
         else if (SubType == ConfigType.MessageAlertConfiguration)
         {
           MessageAlertConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref MessageAlertConfig.AlertCount);
           serializer(action, raw, ref bitPosition, 8, ref MessageAlertConfig.AlertDelay);
           serializer(action, raw, ref bitPosition, 16, ref MovingConfig.SpareWord1);
           serializer(action, raw, ref bitPosition, 16, ref MovingConfig.SpareWord2);
         }
         else if (SubType == ConfigType.IgnitionReportingConfiguration)
         {
           IgnitionReportingConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 1, ref IgnitionReportingConfig.IgnitionReportingEnabled);
           filler(ref bitPosition, 7);
         }
         else if (SubType == ConfigType.ConfigureMachineEvent)
         {
           
           byte count = MachineEventConfigs == null ? (byte)0 : (byte)MachineEventConfigs.Count;
           serializer(action, raw, ref bitPosition, 8, ref count);
           if (MachineEventConfigs == null)
             MachineEventConfigs = new List<MachineEventConfig>();
           for (int i = 0; i < count; i++)
           {
             if (action == SerializationAction.Hydrate)
             {
               MachineEventConfig config = new MachineEventConfig();
               serializer(action, raw, ref bitPosition, 8, ref config.mode);
               serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref config.TriggerMessageType);
               serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref config.ReponseMessageList);
               MachineEventConfigs.Add(config);
             }
             else
             {
               MachineEventConfig config = MachineEventConfigs[i];
               serializer(action, raw, ref bitPosition, 8, ref config.mode);
               serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref config.TriggerMessageType);
               serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref config.ReponseMessageList);
             }
           }
         }
         else if (SubType == ConfigType.ConfigureDailyReport)
         {
           DailyReportConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref DailyReportConfig.Enabled);
           serializer(action, raw, ref bitPosition, 8, ref DailyReportConfig.DailyReportHour);
           serializer(action, raw, ref bitPosition, 8, ref DailyReportConfig.DailyReportMinute);
           serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref DailyReportConfig.TimeZoneName);
         }
         else if (SubType == ConfigType.MainPowerLossReporting)
         {
           MainPowerLossReportingConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref MainPowerLossReportingConfig.isEnabled);
         }
         else if (SubType == ConfigType.ConfigureJ1939Reporting)
         {
           J1939Reporting.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref J1939Reporting.isEnabled);
           serializer(action, raw, ref bitPosition, 8, ref J1939Reporting._reportType);
           serializer(action, raw, ref bitPosition, 8, ref J1939Reporting.includeSupportingParameters);

           J1939Reporting.parameter = (J1939ParameterID[])serializeHomogeneousRunLengthArray(action, raw, ref bitPosition, 8, J1939Reporting.parameter, typeof(J1939ParameterID));
         }
         else if (SubType == ConfigType.ConfigureMachineEventHeader)
         {
           machineEventHeader.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref machineEventHeader._source);
         }
         else if (SubType == ConfigType.TorchAddonsConfiguration)
         {
             torchAddOn.inMessage = true;
             serializer(action, raw, ref bitPosition, 8, ref torchAddOn._addonFeatureCode);
             serializer(action, raw, ref bitPosition, 8, ref torchAddOn.isEnabled);
         }
         else if (SubType == ConfigType.RadioTransmitterDisableControl)
         {
           radioTransmitter.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref radioTransmitter.isEnabled);
         }
         else if (SubType == ConfigType.SuspiciousMoveReporting)
         {
           SuspiciousMoveReportingConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref SuspiciousMoveReportingConfig.isEnabled);
         }
         else if (SubType == ConfigType.AssetBasedFirmwareVersionConfig)
         {
           firmwareVersionConfig.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref firmwareVersionConfig._config);
         }
         else if (SubType == ConfigType.RFIDConfiguration)
         {
           RfidConfiguration.inMessage = true;
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._readerType);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._rfidReaderStatus);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._triggerSource);
           serializer(action, raw, ref bitPosition, 16, ref RfidConfiguration.TXRFPower);
           serializer(action, raw, ref bitPosition, 16, ref RfidConfiguration.AsynOnTime);
           serializer(action, raw, ref bitPosition, 16, ref RfidConfiguration.AsynOffTime);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._antennaSwitchingMethod);
           serializer(action, raw, ref bitPosition, 16, ref RfidConfiguration._linkRate);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._tari);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._millerValue);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._session);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._target);
           serializer(action, raw, ref bitPosition, 1, ref RfidConfiguration.Gen2QIsFixedQ);
           serializer(action, raw, ref bitPosition, 7, ref RfidConfiguration.Gen2QFixedQValue);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._baudRate);
           serializer(action, raw, ref bitPosition, 8, ref RfidConfiguration._readerOperationRegion);
         }
         else if (SubType == ConfigType.SetRadioMachineSecuritymode)
         {
             setRadioMachineSecuritymode.inMessage = true;
             serializer(action, raw, ref bitPosition, 8, ref setRadioMachineSecuritymode._machineSecurityFeature);
             serializer(action, raw, ref bitPosition, 8, ref setRadioMachineSecuritymode._remoteControlInterface);
             serializer(action, raw, ref bitPosition, 8, ref setRadioMachineSecuritymode._machineStartmode);
             serializer(action, raw, ref bitPosition, 8, ref setRadioMachineSecuritymode._tamperResistanceMode);
             serializer(action, raw, ref bitPosition, 8, ref setRadioMachineSecuritymode._relayoutputssetting);
         }
         else
         {
             // Otherwise, I have no idea what this is so suck it all into a data block.

             serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref UnknownMessageData);
         }
      }

      public enum ConfigType
      {
        StoreForwardConfiguration = 0x00,
        SpeedingReportingConfiguration = 0x02,
        ZoneLogicConfiguration = 0x03,
        StoppedNotificationConfiguration = 0x04,
        GeneralDeviceConfiguration = 0x05,
        IgnitionReportingConfiguration = 0x06,
        IPAddressConfiguration = 0x07,
        MixerSensorConfiguration = 0x08,
        NetworkInterfaceConfiguration = 0x09,
        MovingConfiguration = 0x10,
        ApplicationLogicConfiguration = 0x12,
        MessageAlertConfiguration = 0x13,
        DiagnosticReportConfiguration = 0x14,
        HomeSitePositionReportingConfiguration = 0x15,
        CDPDModuleConfiguration = 0x16,
        DevicePortConfiguration = 0x17,
        MetricsConfiguration = 0x18,
        GenericGpioConfiguration = 0x19,
        MainPowerLossReporting = 0x21,
        SBCConfiguration = 0x1A,
        IPFilterConfiguration = 0x1B,
        ConfigureMachineEvent = 0x1C,
        ConfigureDailyReport = 0x1D,
        ConfigureJ1939Reporting = 0x1F,
        SuspiciousMoveReporting = 0x20,
        RadioTransmitterDisableControl = 0x22,
        ConfigureMachineEventHeader = 0x23,
        TorchAddonsConfiguration = 0x24,
        SIMPOSOutputConfiguration = 0xFF,
        AssetBasedFirmwareVersionConfig = 0x25,
        RFIDConfiguration = 0x30,
        SetRadioMachineSecuritymode = 0x31
      }

      private string GetPortNumberString()
      {
        switch (PortNumberRaw)
        {
          case 1:
            return "SIO1 (DB9, 38400 baud max)";
          case 2:
            return "SIO2 (RJ45, 19200 baud max)";
          case 3:
            return "JBX11 (JBX – Serial Port 1,.19200 baud max)";
          case 4:
            return "JBX21 (JBX – Serial Port 2, 19200 baud max)";
          default:
            return null;
        }
      }

      private byte GetPortNumberInt(string portNum)
      {
        if (portNum == "SIO1 (DB9, 38400 baud max)")
          return 1;
        else if (portNum == "SIO2 (RJ45, 19200 baud max)")
          return 2;
        else if (portNum == "JBX11 (JBX – Serial Port 1,.19200 baud max)")
          return 3;
        else if (portNum == "JBX21 (JBX – Serial Port 2, 19200 baud max)")
          return 4;
        else
          return 5;        
      }

      public string PortNum
      {
        get { return GetPortNumberString(); }
        set { PortNumberRaw = GetPortNumberInt(value); }
      }

      private byte PortNumberRaw = 5;

      private string GetServiceTypeString()
      {
        switch (ServiceTypeRaw)
        {
          case 0:
            return "Diagnostics (CONIO), 19200";
          case 1:
            return "MDT, 9600";
          case 2:
            return "User Data Service, 9600";
          case 3:
            return "Sensor PNP, 4800";
          case 4:
            return "Sensor PNP, 9600";
          case 5:
            return "NMEA, 4800";
          case 6:
            return "JBX Module2";
          case 7:
            return "WAN Access3";
          case 8:
            return "RSS Slumper, 9600";
          case 9:
            return "WAN Access, 19200";
          case 10:
            return "Garmin FMI, 9600";
          default:
            return null;
        }
      }
      private byte GetServiceTypeInt(string serviceType)
      {
        if (serviceType == "Diagnostics (CONIO), 19200")
          return 0;
        else if (serviceType == "MDT, 9600")
          return 1;
        else if (serviceType == "User Data Service, 9600")
          return 2;
        else if (serviceType == "Sensor PNP, 4800")
          return 3;
        else if (serviceType == "Sensor PNP, 9600")
          return 4;
        else if (serviceType == "NMEA, 4800")
          return 5;
        else if (serviceType == "JBX Module2")
          return 6;
        else if (serviceType == "WAN Access3")
          return 7;
        else if (serviceType == "RSS Slumper, 9600")
          return 8;
        else if (serviceType == "WAN Access, 19200")
          return 9;
        else if (serviceType == "Garmin FMI, 9600")
          return 10;
        else
          return 11;
      }
      public string PortConfigType
      {
        get { return GetServiceTypeString(); }
        set { ServiceTypeRaw = GetServiceTypeInt(value); }
      }

      private int ServiceTypeRaw = 11;
      
      public ConfigType SubType 
      {
         get { return (ConfigType) SubTypeRaw; }
        set { SubTypeRaw = (int)value; }
      }

      public int        SubTypeRaw;

      public struct NetworkInterfaceConfiguration 
      {
         public string     StackConfigString1;
         public string     StackConfigString2;
         public string     StackConfigString3;
         public string     StackConfigString4;
         public string     ApplicationConfigString;

         public override string ToString()
         {
           var builder = new StringBuilder("NetworkInterfaceConfiguration");
           if(!string.IsNullOrEmpty(StackConfigString1))
             builder.AppendFormat("\nStackConfigString1:         {0}", StackConfigString1);
           if (!string.IsNullOrEmpty(StackConfigString2))
             builder.AppendFormat("\nStackConfigString2:         {0}", StackConfigString2);
           if (!string.IsNullOrEmpty(StackConfigString3))
             builder.AppendFormat("\nStackConfigString3:         {0}", StackConfigString3);
           if (!string.IsNullOrEmpty(StackConfigString4))
             builder.AppendFormat("\nStackConfigString4:         {0}", StackConfigString4);
           if (!string.IsNullOrEmpty(ApplicationConfigString))
             builder.AppendFormat("\nApplicationConfigString:    {0}", ApplicationConfigString);

           return builder.ToString();
         }
      };

      public struct StoreForwardConfiguration
      {
        internal bool inMessage;
        public bool PositionForwardingEnabled;
        public bool OutOfNetworkPositionSavingEnabled;
        public bool InNetworkPositionSavingEnabled;
        public byte UpdateIntervals;
        public StoreForwardUpdateInterval UpdateInterval
        {
          get { return (StoreForwardUpdateInterval)UpdateIntervals; }
          set { UpdateIntervals = (byte)value; }
        }

        public override string ToString()
        {

          if (!inMessage)
          {
            return null;
          }
          else
          {
            StringBuilder builder = new StringBuilder("StoreForwardConfiguration");
            builder.AppendFormat("\nPositionForwardingEnabled:  {0}", PositionForwardingEnabled);
            builder.AppendFormat("\nOutOfNetworkPositionSavingEnabled:  {0}", OutOfNetworkPositionSavingEnabled);
            builder.AppendFormat("\nInNetworkPositionSavingEnabled:  {0}", InNetworkPositionSavingEnabled);
            builder.AppendFormat("\nUpdateInterval:  {0}", UpdateInterval.ToString());

            return builder.ToString();
          }
          
        }
      };

      public struct MessageAlertConfiguration
      {
        internal bool inMessage;
        public byte AlertCount;
        public byte AlertDelay;
        public ushort SpareWord1;
        public ushort SpareWord2;

        public override string ToString()
        {
          if (!inMessage)
            return null;
          else
          {
            StringBuilder builder = new StringBuilder("MessageAlertConfiguration");
            builder.AppendFormat("\nAlertCount:  {0}", AlertCount);
            builder.AppendFormat("\nAlertDelay:  {0}", AlertDelay);

            return builder.ToString();
          }
        }
      }

      public enum StoreForwardUpdateInterval
      {
        oneMinute = 0x3,
        seconds144 = 0x4,
        seconds225 = 0x5,
        fiveMinutes = 0x6,
        tenMinutes = 0x7,
        fifteenMinutes = 0x8,
        twentyMinutes = 0x9,
        thirtyMinutes = 0xA,
        oneHour = 0xB
      };

      public NetworkInterfaceConfiguration   NetworkInterface;

      public struct IPFilterConfiguration 
      {
         public IPFilterConfigurationBlock.AllowedAddress[]  AllowedAddresses;

         public override string ToString()
         {
           if (AllowedAddresses == null)
             return null;

           return "IPFilterConfiguration";
         }
      }

      public IPFilterConfiguration IPFilter;

      public enum DestinationType 
      {
         Local,
         BurnIn,
         Primary,
         Secondary
      }

      public struct IPAddressConfiguration 
      {
        internal bool inMessage;
         public int              DestinationRaw;
         public UInt32           AddressRaw;
         public Int16            Port;

         public DestinationType  Destination 
         {
            get { return (DestinationType) DestinationRaw; }
            set { DestinationRaw = (int) value; }
         }

         public override string ToString()
         {
           if (!inMessage)
             return null;
           else
           {
             StringBuilder builder = new StringBuilder("IPAddressConfiguration");
             builder.AppendFormat("\nAddressRaw:  {0}", AddressRaw.ToString());
             builder.AppendFormat("\nPort:  {0}", Port.ToString());
             return builder.ToString();
           }
         }
      }

      public struct ZoneLogicConfiguration
      {
        internal bool inMessage;
        public byte SitePersistenceFlag;
        public byte HomeZoneEntrySpeed;
        public byte HomeSiteEntrySpeed;
        public byte JobSiteEntrySpeed;
        public byte ReservedEntrySpeed;
        public byte HomeZoneExitSpeed;
        public byte HomeSiteExitSpeed;
        public byte JobSiteExitSpeed;
        public byte ReservedExitSpeed;
        public byte HomeZoneHysteresisSeconds;
        public byte HomeSiteHysteresisSeconds;
        public byte JobSiteHysteresisSeconds;
        public byte ReservedHysteresisSeconds;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("ZoneLogicConfiguration");
          builder.AppendFormat("\nSitePersistenceFlag:  {0}", SitePersistenceFlag.ToString());
          builder.AppendFormat("\nHomeZoneEntrySpeed:  {0}", HomeZoneEntrySpeed.ToString());
          builder.AppendFormat("\nHomeSiteEntrySpeed:  {0}", HomeSiteEntrySpeed.ToString());
          builder.AppendFormat("\nJobSiteEntrySpeed:  {0}", JobSiteEntrySpeed.ToString());
          builder.AppendFormat("\nReservedEntrySpeed:  {0}", ReservedEntrySpeed.ToString());
          builder.AppendFormat("\nHomeZoneExitSpeed:  {0}", HomeZoneExitSpeed.ToString());
          builder.AppendFormat("\nHomeSiteExitSpeed:  {0}", HomeSiteExitSpeed.ToString());
          builder.AppendFormat("\nJobSiteExitSpeed:  {0}", JobSiteExitSpeed.ToString());
          builder.AppendFormat("\nReservedExitSpeed:  {0}", ReservedExitSpeed.ToString());
          builder.AppendFormat("\nHomeZoneHysteresisSeconds:  {0}", HomeZoneHysteresisSeconds.ToString());
          builder.AppendFormat("\nHomeSiteHysteresisSeconds: {0}", HomeSiteHysteresisSeconds.ToString());
          builder.AppendFormat("\nJobSiteHysteresisSeconds:  {0}", JobSiteHysteresisSeconds.ToString());
          builder.AppendFormat("\nSitePersistenceFlag:  {0}", SitePersistenceFlag.ToString());
          builder.AppendFormat("\nReservedHysteresisSeconds:  {0}", ReservedHysteresisSeconds.ToString());

          return builder.ToString();
        }
      }

      public struct SpeedReportingConfiguration
      {
        internal bool inMessage;

        public bool ConfigurationFlag;
        public byte SpeedThreshold;
        public short DurationThreshold;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("SpeedReportingConfiguration");
          builder.AppendFormat("\nConfigurationFlag:  {0}", ConfigurationFlag.ToString());
          builder.AppendFormat("\nSpeedThreshold:  {0}", SpeedThreshold.ToString());
          builder.AppendFormat("\nDurationThreshold:  {0}", DurationThreshold.ToString());
          return builder.ToString();
        }

        public MTSConfigData.SpeedingConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
        {
          MTSConfigData.SpeedingConfig config = new MTSConfigData.SpeedingConfig();
          config.MessageSourceID = messageID;
          config.SentUTC = sentUTC;
          config.Status = status;

          config.IsEnabled = ConfigurationFlag;
          config.ThresholdMPH = SpeedThreshold;
          config.Duration = TimeSpan.FromSeconds(DurationThreshold);

          return config;
        }

      }

      public struct GeneralDeviceConfiguration
      {
        internal bool inMessage;
        public ushort DeviceLogicType;
        public ushort DeviceShutdownDelay;
        public ushort MDTShutdownDelay;
        public bool AlwaysOnDevice;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("GeneralDeviceConfiguration");
          builder.AppendFormat("\nDeviceLogicType:  {0}", DeviceLogicType.ToString());
          builder.AppendFormat("\nDeviceShutdownDelay:  {0}", DeviceShutdownDelay.ToString());
          builder.AppendFormat("\nMDTShutdownDelay:  {0}", MDTShutdownDelay.ToString());
          builder.AppendFormat("\nAlwaysOnDevice:  {0}", AlwaysOnDevice.ToString());
          return builder.ToString();
        }
      }

      public struct MovingConfiguration
      {
        internal bool inMessage;
        public ushort Radius;
        public ushort SpareWord1;
        public ushort SpareWord2;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("MovingConfiguration");
          builder.AppendFormat("\nRadius:  {0}", Radius.ToString());
          
          return builder.ToString();
        }

        public MTSConfigData.MovingConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
        {
          MTSConfigData.MovingConfig config = new MTSConfigData.MovingConfig();
          config.MessageSourceID = messageID;
          config.SentUTC = sentUTC;
          config.Status = status;

          config.RadiusInFeet = Radius;

          return config;
        }

      }

      public struct HomeSitePositionConfiguration
      {
        internal bool inMessage;
        public ushort Radius;
        public byte DurationThresholdSeconds;
        public ushort SpareWord1;
        public ushort SpareWord2;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("HomeSitePositionConfiguration");
          builder.AppendFormat("\nRadius:  {0}", Radius.ToString());
          builder.AppendFormat("\nDurationThresholdSeconds:  {0}", DurationThresholdSeconds.ToString());

          return builder.ToString();
        }
      }

      public struct IgnitionReportingConfiguration
      {
        internal bool inMessage;
        public bool IgnitionReportingEnabled;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("IgnitionReportingConfiguration");
          builder.AppendFormat("\nRadius:  {0}", IgnitionReportingEnabled.ToString());

          return builder.ToString();
        }
      }

      public struct MachineEventConfig
      {
        public MachineEventDeliveryMode DeliveryMode
        {
          get { return (MachineEventDeliveryMode)mode; }
          set { mode = (byte)value; }
        }

        public string TriggerMessageType;
        public string ReponseMessageList;
        internal byte mode;

        public override string ToString()
        {
          StringBuilder builder = new StringBuilder("MachineEventConfig");
          builder.AppendFormat("\nTriggerMessageType:  {0}", TriggerMessageType.ToString());
          builder.AppendFormat("\nReponseMessageList:  {0}", ReponseMessageList.ToString());
          builder.AppendFormat("\nDeliveryMode:  {0}", DeliveryMode.ToString());

          return builder.ToString();
        }
      }

      public struct ConfigureDailyReport
      {
        internal bool inMessage;
        public bool Enabled;
        public byte DailyReportHour;
        public byte DailyReportMinute;
        public string TimeZoneName;

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("ConfigureDailyReport");
          builder.AppendFormat("\nEnabled:  {0}", Enabled.ToString());
          builder.AppendFormat("\nDailyReportHour:  {0}", DailyReportHour.ToString());
          builder.AppendFormat("\nDailyReportMinute:  {0}", DailyReportMinute.ToString());
          builder.AppendFormat("\nTimeZoneName:  {0}", TimeZoneName.ToString());

          return builder.ToString();
        }

        public MTSConfigData.DailyReportConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
        {
          MTSConfigData.DailyReportConfig config = new MTSConfigData.DailyReportConfig();
          config.MessageSourceID = messageID;
          config.SentUTC = sentUTC;
          config.Status = status;

          config.DailyReportTimeUTC = new TimeSpan(DailyReportHour, DailyReportMinute, 0);

          return config;
        }
      }

      public struct MainPowerLossReporting
      {
        internal bool inMessage;
        public bool isEnabled;

        public override string ToString()
        {
          if (!inMessage)
            return null;
          StringBuilder builder = new StringBuilder("MainPowerLossReporting");
          builder.AppendFormat("\nIsEnabled: {0}", isEnabled.ToString());
          return builder.ToString();
        }
      }

      public struct ConfigureJ1939Reporting
      {
        internal bool inMessage;
        public bool isEnabled;
        public ReportType reportType
        {
          get { return (ReportType)_reportType; }
          set { _reportType = (byte)value; }
        }
        internal byte _reportType;

        public bool includeSupportingParameters;
        public J1939ParameterID[] parameter;

        public override string ToString()
        {
          if (!inMessage)
            return null;
          StringBuilder builder = new StringBuilder("ConfigureJ1939Reporting");
          builder.AppendFormat("\nIsEnabled:  {0}", isEnabled.ToString());
          builder.AppendFormat("\nReportType:  {0}", reportType.ToString());
          builder.AppendFormat("\nIncludeSuportingParameters:  {0}", includeSupportingParameters);
          foreach (J1939ParameterID p in parameter)
          {
            builder.AppendFormat("\n{0}", p.ToString());
          }
          return builder.ToString();
        }
      }

      public enum ReportType
      {
        Fault = 0, 
        Periodic = 1,
        Statistics = 2
      }

     public struct RadioTransmitterDisableControl
     {
       internal bool inMessage;
       public bool isEnabled;

       public override string ToString()
       {
         if (!inMessage)
           return null;

         StringBuilder builder = new StringBuilder("RadioTransmitterDisableControl");
         builder.AppendFormat("\nIsEnabled:  {0}", isEnabled.ToString());

         return builder.ToString();
       }
     }
     public struct ConfigureTorchAddOn
     {
         internal bool inMessage;
         internal byte _addonFeatureCode;
         public bool isEnabled;

         public TorchAddOn addonFeatureCode
         {
             get { return (TorchAddOn)_addonFeatureCode; }
             set { _addonFeatureCode = (byte)value; }
         }

         public override string ToString()
         {
             if (!inMessage)
                 return null;
             
             StringBuilder builder = new StringBuilder("ConfigureTorchAddOn");
             builder.AppendFormat("\n" + ((TorchAddOn)addonFeatureCode).ToString() + ":  {0}", isEnabled.ToString());
             return builder.ToString();
         }
         public MTSConfigData.TMSConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
         {
           MTSConfigData.TMSConfig config = new MTSConfigData.TMSConfig();
           config.MessageSourceID = messageID;
           config.SentUTC = sentUTC;
           config.Status = status;

           config.IsEnabled = isEnabled;

           return config;
         }
     }

     public struct ConfigureMachineEventHeader
     {
        internal bool inMessage;
        internal int _source;

        public PrimaryDataSourceEnum DataSource
        {
          get { return (PrimaryDataSourceEnum)_source; }
          set { _source = (int)value; }
        }

        public override string ToString()
       {
         if (!inMessage)
           return null;

         StringBuilder builder = new StringBuilder("RadioTransmitterDisableControl");
         builder.AppendFormat("\nPrimaryDataSource:  {0}", DataSource.ToString());
         return builder.ToString();
       }

        public MTSConfigData.SMHSourceConfig GetConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
       {
         MTSConfigData.SMHSourceConfig config = new MTSConfigData.SMHSourceConfig();
         config.MessageSourceID = messageID;
         config.SentUTC = sentUTC;
         config.Status = status;

         config.PrimaryDataSource = _source;         

         return config;
       }
     }

      public struct SuspiciousMoveReporting
      {
        internal bool inMessage;
        public bool isEnabled;
        
        public override string ToString()
        {
          if (!inMessage)
            return null;
          StringBuilder builder = new StringBuilder("SuspiciousMoveReporting");
          builder.AppendFormat("\nIsEnabled:  {0}", isEnabled.ToString());
         
          return builder.ToString();
        }
      }

      public struct AssetBasedFirmwareVersionConfig
      {
        internal bool inMessage;
        internal byte _config;
        public AssetBasedFirmwareConfiguration FirmWareConfiguration
        {
          get { return (AssetBasedFirmwareConfiguration)_config; }
          set { _config = (byte)value; }
        }

        public override string ToString()
        {
          if (!inMessage)
            return null;

          StringBuilder builder = new StringBuilder("AssetBasedFirmwareVersionConfig");
          builder.AppendFormat("\n{0,-26}  {1}", "FirmwareFor:", FirmWareConfiguration.ToString());
          return builder.ToString();
        }
      }

     public struct RFIDConfiguration
     {
       internal bool inMessage;

       public RFIDReaderType ReaderType
       {
         get { return (RFIDReaderType)_readerType; }
         set { _readerType = (byte)value; }
       }
       internal byte _readerType;

       public RFIDReaderStatusType RFIDReaderStatus
       {
         get { return (RFIDReaderStatusType)_rfidReaderStatus; }
         set { _rfidReaderStatus = (byte)value; }
       }
       internal byte _rfidReaderStatus;

       public ReaderOperationRegionForRfidConfigurationType ReaderOperationRegion 
       {
         get { return (ReaderOperationRegionForRfidConfigurationType)_readerOperationRegion; }
         set { _readerOperationRegion = (byte)value; }
       }
       internal byte _readerOperationRegion;

       public RFIDTriggerSourceType TriggerSource
       {
         get { return (RFIDTriggerSourceType)_triggerSource; }
         set { _triggerSource = (byte)value; }
       }
       internal byte _triggerSource;

       public UInt16 TXRFPower;
       public UInt16 AsynOnTime;
       public UInt16 AsynOffTime;

       public AntennaSwitchingMethodType AntennaSwitchingMethod
       {
         get { return (AntennaSwitchingMethodType)_antennaSwitchingMethod; }
         set { _antennaSwitchingMethod = (byte)value; }
       }
       internal byte _antennaSwitchingMethod;

       public LinkRateType LinkRate
       {
         get { return (LinkRateType)_linkRate; }
         set { _linkRate = (short)value; }
       }
       internal short _linkRate;

       public TariType Tari
       {
         get { return (TariType)_tari; }
         set { _tari = (byte)value; }
       }
       internal byte _tari;

       public MillerValueType MillerValue
       {
         get { return (MillerValueType)_millerValue; }
         set { _millerValue = (byte)value; }
       }
       internal byte _millerValue;

       public SessionForRfidConfigurationType Session
       {
         get { return (SessionForRfidConfigurationType)_session; }
         set { _session = (byte)value; }
       }
       internal byte _session;

       public TargetForRfidConfigurationType Target
       {
         get { return (TargetForRfidConfigurationType)_target; }
         set { _target = (byte)value; }
       }
       internal byte _target;

       public bool Gen2QIsFixedQ;
       public byte Gen2QFixedQValue;

       public BaudRateForRfidConfigurationType BaudRate
       {
         get { return (BaudRateForRfidConfigurationType)_baudRate; }
         set { _baudRate = (byte)value; }
       }
       internal byte _baudRate;

       public override string ToString()
       {
         if (!inMessage)
           return null;

         StringBuilder builder = new StringBuilder("RFIDConfiguration");
         builder.AppendFormat("\n{0,-26}  {1}", "ReaderType:", ReaderType.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "RFIDReaderStatus:", RFIDReaderStatus.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "TriggerSource:", TriggerSource.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "TXRFPower:", TXRFPower.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "AsynOnTime:", AsynOnTime.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "AsynOffTime:", AsynOffTime.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "AntennaSwitchingMethod:", AntennaSwitchingMethod.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "LinkRate:", LinkRate.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "Tari:", Tari.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "MillerValue:", MillerValue.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "Session:", Session.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "Target:", Target.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "Gen2Q Bit 0:", (Gen2QIsFixedQ) ? "1 (Fixed Q)" : "0 (Dynamic)");
         builder.AppendFormat("\n{0,-26}  {1}", "Gen2Q Bits 1-7 FixedQValue", Gen2QFixedQValue);
         builder.AppendFormat("\n{0,-26}  {1}", "BaudRate:", BaudRate.ToString());
         builder.AppendFormat("\n{0,-26}  {1}", "ReaderOperationRegion:", ReaderOperationRegion.ToString());
         return builder.ToString();
       }
     }

     public struct SetRadioMachineSecuritymode
     {
         internal bool inMessage;

         public MachineSecurityFeatureConfiguration MachineSecurityFeature
         {
             get { return (MachineSecurityFeatureConfiguration)(sbyte)_machineSecurityFeature; }
             set { _machineSecurityFeature = (byte)value; }
         }
         internal byte _machineSecurityFeature;

         public RemoteControlInterface RemoteControlInterface
         {
             get { return (RemoteControlInterface)(sbyte)_remoteControlInterface; }
             set { _remoteControlInterface = (byte)value; }
         }
         internal byte _remoteControlInterface;

         public MachineStartStatus MachineStartmode
         {
             get { return (MachineStartStatus)(sbyte)_machineStartmode; }
             set { _machineStartmode = (byte)value; }
         }
         internal byte _machineStartmode;

         public TamperResistanceStatus TamperResistanceMode
         {
             get { return (TamperResistanceStatus)(sbyte)_tamperResistanceMode; }
             set { _tamperResistanceMode = (byte)value; }
         }

         internal byte _tamperResistanceMode;

         public Relayoutputssetting Relayoutputssetting
         {
             get { return (Relayoutputssetting)(sbyte)_relayoutputssetting; }
             set { _relayoutputssetting = (byte)value; }
         }
         internal byte _relayoutputssetting;

         public override string ToString()
         {
             if (!inMessage)
                 return null;

             StringBuilder builder = new StringBuilder("GeneralDeviceConfiguration");
             builder.AppendFormat("\nMachineSecurityFeature:  {0}", _machineSecurityFeature.ToString());
             builder.AppendFormat("\nRemoteControlInterface:  {0}", _remoteControlInterface.ToString());
             builder.AppendFormat("\nMachineStartmode:  {0}", _machineStartmode.ToString());
             builder.AppendFormat("\nTamperResistanceModes:  {0}", _tamperResistanceMode.ToString());
             builder.AppendFormat("\nRelayoutputssetting:  {0}", _relayoutputssetting.ToString());
             return builder.ToString();
         }
     }

     public enum MachineSecurityFeatureConfiguration 
     {
        DisableMachineSecurityFeature = 0x00,
        EnableMachineSecurityFeature = 0x01
     }

     public enum RemoteControlInterface 
     {
         UsesRelayOutputsdevice = 0x00,
         UseJ1939_CANinterface = 0x01
     }

     public enum Relayoutputssetting
     {
         RelayOutput1ToIn_activeState = 0x00,
         RelayOutput1ToActiveState = 0x01,
         RelayOutput2ToIn_activeState = 0x02,
         RelayOutput2ToActiveState = 0x03,
         NA = 0xFE
     }

     public enum RFIDReaderType : byte
     {
       TMVegaM5e = 0,
       TMM6e = 1,
       TMVegaM5eEU = 2
     }

     public enum RFIDReaderStatusType : byte
     {
       DisableRFIDReader = 0,
       EnableRFIDReader = 1
     }

     public enum ReaderOperationRegionForRfidConfigurationType : byte
     {
       NA = 0,
       EU = 1, 
       AU = 2,
       KR = 3,
       IN = 4
     }

     public enum RFIDTriggerSourceType : byte
     {
       EnabledByIgnition = 0,
       EnabledByEventsOrATCommands = 1,
       EnabledBySiteLogic = 2,
       DigitalOutputFromTelematicsDeviceOnly = 3,
       DigitalInputFromExternalAccessory = 4
     }

     public enum AntennaSwitchingMethodType : byte
     {
       Dynamic = 0,
       EqualTime = 1
     }

     public enum LinkRateType : short
     {
       KHz250 = 250,
       KHz640 = 640
     }

     public enum TariType : byte
     {
       Us25 = 0,
       Us12_5 = 1,
       Us6_25 = 2
     }

     public enum MillerValueType : byte
     {
       FM0 = 0,
       M2 = 1,
       M4 = 2,
       M8 = 3
     }

     public enum SessionForRfidConfigurationType : byte
     {
       S0 = 0,
       S1 = 1,
       S2 = 2,
       S3 = 3
     }

     public enum TargetForRfidConfigurationType : byte
     {
       A = 0,
       B = 1,
       AB = 2,
       BA = 3
     }

     public enum BaudRateForRfidConfigurationType : byte
     {
       BaudRate9600bps = 1,
       BaudRate19200bps = 2,
       BaudRate38400bps = 3,
       BaudRate57600bps = 4,
       BaudRate115200bps = 5,
       BaudRate230400bps = 6
     }

     public enum AssetBasedFirmwareConfiguration : byte
      {
        PL420VocationalTrucks = 1,
        PL421BCP = 2,
        PL420EPD= 3,
        PL420AfterMarket1 = 4,
        PL420AfterMarket2 = 5,
        PL421China = 6,
        PL421Forestry = 7,
        PL421Paving = 8,
        PL4XXRFID = 9,
        PL4XXJ1939ScanMode = 254
      }

      public enum MachineEventDeliveryMode
      {
        Suppress = 0,
        Immediate = 1,
        DailyReport = 2,
      }
      public enum TorchAddOn
      {
        //Enumeration for Feature Code to Configure, name and feature code in parenthesis
        CE   =  1, // Cellular Parameter Modification (CE)
        WF   =  2, // Wifi Enabled (WF)
        FT   =  3, // File Transfer (FT)
        VR   =  4, // VRS Client (VR)
        PR   =  5, // Port Routing (PR)
        IT   =  6, // Internet Routing to Trimble (IT)
        IF   =  7, // Internet Routing Full Access (IF)
        AV   =  8, // Ag Mode Vehicle Bus Config (AV)
        WH   =  9, // WLAN Hotspot (WH)
        G1   = 10, //  Group Code for Ag Full Featured (G1)
        G2   = 11, //  Group Code for HH Full Featured (G2)
        G3   = 12, //  Group Code for Ag Full Featured with Cell Mod (G3)
        G4   = 13, //  Group Code for HH Full Featured with Cell Mod (G4)
        PM   = 14, //  Productivity Monitoring
        TPMS = 15  // TPMS Monitoring
      }      

      public IPAddressConfiguration IPAddress;
      public ZoneLogicConfiguration ZoneLogic;
      public SpeedReportingConfiguration SpeedingReporting;
      public GeneralDeviceConfiguration GeneralDevice;
      public MovingConfiguration MovingConfig;
      public HomeSitePositionConfiguration HomeSitePosition;
      public StoreForwardConfiguration StoreForwardConfig;
      public MessageAlertConfiguration MessageAlertConfig;
      public IgnitionReportingConfiguration IgnitionReportingConfig;
      public List<MachineEventConfig> MachineEventConfigs;
      public ConfigureDailyReport DailyReportConfig;
      public MainPowerLossReporting MainPowerLossReportingConfig;
      public ConfigureJ1939Reporting J1939Reporting;
      public SuspiciousMoveReporting SuspiciousMoveReportingConfig;
      public RadioTransmitterDisableControl radioTransmitter;
      public ConfigureMachineEventHeader machineEventHeader;
      public ConfigureTorchAddOn torchAddOn;
      public AssetBasedFirmwareVersionConfig firmwareVersionConfig;
      public RFIDConfiguration RfidConfiguration;
      public SetRadioMachineSecuritymode setRadioMachineSecuritymode;

      [XmlIgnore]
      public byte[]  UnknownMessageData;
   }


   //public class DimProgrammingStartUserDataMessage : BaseUserDataMessage
   //{
   //  public static new readonly int kPacketID = 0x14;
   //  public static readonly byte kSubType = 0x00;

   //  public override int PacketID
   //  {
   //    get { return kPacketID; }
   //  }

   //  public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
   //  {
   //    uint subType = kSubType;

   //    base.Serialize(action, raw, ref bitPosition);
   //    serializer(action, raw, ref bitPosition, 8, ref subType);   
   //    serializer(action, raw, ref bitPosition, 32, ref DIMListID);
           

   //  }

   //  public uint DIMListID;
       
   //}

   public class DimProgrammingUserDataMessage : BaseUserDataMessage
   {
     public static new readonly int kPacketID = 0x14;

     public override int PacketID
     {
       get { return kPacketID; }
     }

     public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
     {
       base.Serialize(action, raw, ref bitPosition);

       if (action == SerializationAction.Hydrate)
       {
         // Peek to see what nested class we need to create if we are hydrating.

         byte subType = 0;
         uint subTypeBitPosition = bitPosition;

         serializer(action, raw, ref subTypeBitPosition, 8, ref subType);

         // Create the subtype and fall through for common code.

         switch (subType)
         {
           case 0x00:
             ProgrammingStart start = new ProgrammingStart();
             Message = start;
             break;
           case 0x01:
             ProgrammingMessage msg = new ProgrammingMessage();
             Message = msg;
             break;
           default:
             hydrationErrors |= MessageHydrationErrors.EmbeddedMessageUnknown;
             break;
         }
       }

       if (Message != null)
       {
         Message.Parent = this;
         Message.Serialize(action, raw, ref bitPosition);
       }
     }
        
      public class ProgrammingStart : NestedMessage
      {
        public static readonly byte kSubType = 0x00;

        public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
        {
          uint subType = kSubType;
          serializer(action, raw, ref bitPosition, 8, ref subType);
          serializer(action, raw, ref bitPosition, 32, ref DIMListID);
          serializer(action, raw, ref bitPosition, 8, ref NumberOfDIMMessages);
          serializer(action, raw, ref bitPosition, 8, ref NumberOfDIMQuestions);
          serializer(action, raw, ref bitPosition, 8, ref NumberOfDIMDefaults); 
        }

        public uint DIMListID;
        public byte NumberOfDIMMessages = 0;
        public byte NumberOfDIMQuestions = 0;
        public byte NumberOfDIMDefaults = 0;   
      }

      public class ProgrammingMessage : NestedMessage
      {
        public static readonly byte kSubType = 0x01;
        public static readonly byte kDIM2 = 0x0F;
        public static readonly byte kDIM2WithParams = 0x1E;

        public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
        {
          uint subType = kSubType;

          serializer(action, raw, ref bitPosition, 8, ref subType);
          serializer(action, raw, ref bitPosition, 32, ref DIMListID);

          serializer(action, raw, ref bitPosition, 8, ref MessageNumber);
          serializeLengthPrefixedString(action, raw, ref bitPosition, 8, ref Message);

          byte numberOfQuestions;
          byte userDataType;

          if (Questions != null && Questions.Length > 0 && Defaults != null && Defaults.Length > 0)
          {
            numberOfQuestions = (byte)Questions.Length;
            userDataType = (byte)kDIM2WithParams;

            serializer(action, raw, ref bitPosition, 8, ref numberOfQuestions);
            if (action == SerializationAction.Hydrate)
            {
              Questions = new short[numberOfQuestions];
              Defaults = new short[numberOfQuestions];
            }

            for (int i = 0; i < Questions.Length; i++)
            {
              serializer(action, raw, ref bitPosition, 16, ref Questions[i]);
              serializer(action, raw, ref bitPosition, 16, ref Defaults[i]);
            }
          }
          else
          {
            numberOfQuestions = 0;
            userDataType = (byte)kDIM2;

            serializer(action, raw, ref bitPosition, 8, ref numberOfQuestions);
          }

          serializer(action, raw, ref bitPosition, 8, ref userDataType);

          byte deviceAction = 0;
          serializer(action, raw, ref bitPosition, 8, ref deviceAction);
        }

        public uint DIMListID;
        public byte MessageNumber;
        public string Message;
        public short[] Questions;
        public short[] Defaults;
      }

      public NestedMessage Message;
   }

   public class DeviceConfigurationQueryBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x17;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition,  8, ref CommandRaw);
      }

      private byte CommandRaw;

      public enum QueryCommand 
      {
         OnceDailyReports,
         ApplicationBITReports,
         PeriodicBITReports
      }

      public QueryCommand Command 
      {
         get { return (QueryCommand) CommandRaw; }
         set { CommandRaw = (byte) value; }
      }
   }

    public class OutboundPortBasedDataBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x80;

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         serializer(action, raw, ref bitPosition, 16, ref PortNumber);

         serializeLengthPrefixedBytes(action, raw, ref bitPosition, 16, ref Data);

      }

      public UInt16     PortNumber;
      public string     DataString
      {
        get { return Encoding.ASCII.GetString(Data); }
        set { Data = Encoding.ASCII.GetBytes(value); }
      }
      public byte[] Data;
   }

   //@@@@ SEVERAL MORE NEED TO BE IMPLEMENTED FOR THE SAKE OF THE DBWRITER WHEN IT HANDLES STORING OUTBOUND
   // Consider making the unknown messages 'IDeprecated' to generate a message

   public class UnknownBaseUserDataMessage : BaseUserDataMessage
   {
      public static new readonly int kPacketID = 0x81;      // This packet does not exist in our wireless format.

      public override int PacketID
      {
         get { return kPacketID; }
      }

      public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition) 
      {
         base.Serialize(action, raw, ref bitPosition);

         if (action == SerializationAction.Hydrate) 
         {
            uint realDataLength = bytesLeftInMessage(bitPosition);

            serializeFixedLengthBytes(action, raw, ref bitPosition, realDataLength, ref Data);
         } 
         else 
         {
            serializeFixedLengthBytes(action, raw, ref bitPosition, (uint) Data.Length, ref Data);
         }
      }

      public byte[]  Data;
   }
}
