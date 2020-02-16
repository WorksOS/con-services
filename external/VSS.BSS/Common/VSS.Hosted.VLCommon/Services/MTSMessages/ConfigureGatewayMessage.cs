using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSS.Hosted.VLCommon;


namespace VSS.Hosted.VLCommon.MTSMessages
{
  public class ConfigureGatewayMessage : BaseUserDataMessage
  {
    public static new readonly int kPacketID = 0x1A;
    public override int PacketID
    {
      get
      {
        return kPacketID;
      }
    }
    public byte TransactionType;
    public byte TransactionSubType;
    public byte TransactionVersion;
    public byte MessageSequenceID;
    public List<NestedMessage> Blocks;
    
    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      base.Serialize(action, raw, ref bitPosition);

      lengthBackfill configBlockLength = lengthBackfill.Mark(action, raw, ref bitPosition, 16);

      serializer(action, raw, ref bitPosition, 8, ref TransactionType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionSubType);
      serializer(action, raw, ref bitPosition, 8, ref TransactionVersion);
      serializer(action, raw, ref bitPosition, 8, ref MessageSequenceID);
      if (Blocks == null)
        Blocks = new List<NestedMessage>();
      if (TransactionType == 0x11)
      {
        if (action == SerializationAction.Hydrate)
        {
          while (configBlockLength.BitsRemaining(bitPosition) > 0)
          {
            OTAConfigParameters ota = new OTAConfigParameters();
            ota.Serialize(action, raw, ref bitPosition);
            Blocks.Add(ota);
          }
        }
        else 
        {
          foreach (OTAConfigParameters param in Blocks)
          {
            param.Serialize(action, raw, ref bitPosition);
          }
        }
      }

      configBlockLength.Backfill(bitPosition);
    }

    public MTSConfigData.MaintenanceModeConfig GetMaintModeConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
    {
      MTSConfigData.MaintenanceModeConfig maintModeConfig = null;

      foreach (OTAConfigParameters param in Blocks.OfType<OTAConfigParameters>())
      {
        switch (param.FieldIdentifier)
        {
          case FieldID.MaintenanceMode:
            if (null == maintModeConfig)
              maintModeConfig = new MTSConfigData.MaintenanceModeConfig();

            maintModeConfig.MessageSourceID = messageID;
            maintModeConfig.SentUTC = sentUTC;
            maintModeConfig.Status = status;
            maintModeConfig.IsEnabled = param.MaintenanceModeEnabled;
            break;
          case FieldID.MaintenanceModeDurationTimer:
            if (null == maintModeConfig)
              maintModeConfig = new MTSConfigData.MaintenanceModeConfig();

            maintModeConfig.MessageSourceID = messageID;
            maintModeConfig.SentUTC = sentUTC;
            maintModeConfig.Status = status;
            maintModeConfig.Duration = param.MaintenanceModeDuration;
            break;
        }
      }

      return maintModeConfig;
    }

    public MTSConfigData.DigitalSwitchConfig GetDigSwitchConfig(FieldID fieldID, long messageID, DateTime? sentUTC, MessageStatusEnum status)
    {
      MTSConfigData.DigitalSwitchConfig digSwitchConfig = null;

      foreach (OTAConfigParameters param in Blocks.OfType<OTAConfigParameters>())
      {
        switch (param.FieldIdentifier)
        {
          case FieldID.DigitalInput1Config:
          case FieldID.DigitalInput2Config:
          case FieldID.DigitalInput3Config:
          case FieldID.DigitalInput4Config:
            if (param.FieldIdentifier == fieldID)
            {
              if (null == digSwitchConfig)
                digSwitchConfig = new MTSConfigData.DigitalSwitchConfig();

              digSwitchConfig.Config = param.Configuration;
            }
            break;
          case FieldID.DigitalInput1DelayTime:
          case FieldID.DigitalInput2DelayTime:
          case FieldID.DigitalInput3DelayTime:
          case FieldID.DigitalInput4DelayTime:
            if ((param.FieldIdentifier == FieldID.DigitalInput1DelayTime && fieldID == FieldID.DigitalInput1Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput2DelayTime && fieldID == FieldID.DigitalInput2Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput3DelayTime && fieldID == FieldID.DigitalInput3Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput4DelayTime && fieldID == FieldID.DigitalInput4Config))
            {
              if (null == digSwitchConfig)
                digSwitchConfig = new MTSConfigData.DigitalSwitchConfig();

              digSwitchConfig.DelayTime = param.DelayTime;
            }
            break;
          case FieldID.DigitalInput1Description:
          case FieldID.DigitalInput2Description:
          case FieldID.DigitalInput3Description:
          case FieldID.DigitalInput4Description:
            if ((param.FieldIdentifier == FieldID.DigitalInput1Description && fieldID == FieldID.DigitalInput1Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput2Description && fieldID == FieldID.DigitalInput2Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput3Description && fieldID == FieldID.DigitalInput3Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput4Description && fieldID == FieldID.DigitalInput4Config))
            {
              if (null == digSwitchConfig)
                digSwitchConfig = new MTSConfigData.DigitalSwitchConfig();

              digSwitchConfig.Description = param.Description;
            }
            break;
          case FieldID.DigitalInput1MonitoringCondition:
          case FieldID.DigitalInput2MonitoringCondition:
          case FieldID.DigitalInput3MonitoringCondition:
          case FieldID.DigitalInput4MonitoringCondition:
            if ((param.FieldIdentifier == FieldID.DigitalInput1MonitoringCondition && fieldID == FieldID.DigitalInput1Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput2MonitoringCondition && fieldID == FieldID.DigitalInput2Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput3MonitoringCondition && fieldID == FieldID.DigitalInput3Config) ||
              (param.FieldIdentifier == FieldID.DigitalInput4MonitoringCondition && fieldID == FieldID.DigitalInput4Config))
            {
              if (null == digSwitchConfig)
                digSwitchConfig = new MTSConfigData.DigitalSwitchConfig();

              digSwitchConfig.MonitoringCondition = param.MonitoringCondition;
            }
            break;
        }
      }

      if (null != digSwitchConfig)
      {
        digSwitchConfig.Field = fieldID;
        digSwitchConfig.MessageSourceID = messageID;
        digSwitchConfig.SentUTC = sentUTC;
        digSwitchConfig.Status = status;
      }

      return digSwitchConfig;
    }

    public RuntimeAdjConfig GetRuntimeAdjConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
    {
      RuntimeAdjConfig runtime = null;

      foreach (OTAConfigParameters param in Blocks.OfType<OTAConfigParameters>())
      {
        if (param.FieldIdentifier == FieldID.SMU)
        {
          runtime = new RuntimeAdjConfig();
          runtime.Runtime = param.SMU;
          runtime.MessageSourceID = messageID;
          runtime.SentUTC = sentUTC;
          runtime.Status = status;
        }
      }

      return runtime;
    }

    public MTSConfigData.TamperSecurityAdministrationInformationConfig GetMachineSecuritySystemConfig(long messageID, DateTime? sentUTC, MessageStatusEnum status)
    {
      MTSConfigData.TamperSecurityAdministrationInformationConfig machineSecuritySystemInformation = new MTSConfigData.TamperSecurityAdministrationInformationConfig();

      foreach (OTAConfigParameters param in Blocks.OfType<OTAConfigParameters>())
      {
        if (param.FieldIdentifier == FieldID.MachineStartMode)
        {
          machineSecuritySystemInformation.machineStartStatus = param.StartStatus;
          machineSecuritySystemInformation.machineStartStatusField = FieldID.MachineStartMode;
        }
        if (param.FieldIdentifier == FieldID.TamperResistanceMode)
        {
          machineSecuritySystemInformation.tamperResistanceStatus = param.ResistanceStatus;
          machineSecuritySystemInformation.tamperResistanceStatusField = FieldID.TamperResistanceMode;
        }
      }
      if (!machineSecuritySystemInformation.machineStartStatus.HasValue && !machineSecuritySystemInformation.tamperResistanceStatus.HasValue)
      {
        return null;
      }

      machineSecuritySystemInformation.MessageSourceID = messageID;
      machineSecuritySystemInformation.SentUTC = sentUTC;
      machineSecuritySystemInformation.Status = status;
      return machineSecuritySystemInformation;
    }
  }


  public class OTAConfigParameters : NestedMessage
  {
    public FieldID FieldIdentifier
    {
      get { return (FieldID)fieldID; }
      set { fieldID = (byte)value; }
    }
    public InputConfig Configuration
    {
      get 
      {
        if (config != 0x11 && config != 0x2C && config != 0x57 && config != 0x58)
          return InputConfig.NotInstalled;
        return (InputConfig)config; 
      }
      set { config = (byte)value; }
    }
    public TimeSpan DelayTime
    {
      get { return TimeSpan.FromMilliseconds((double)delayTime * 100.0); }
      set { delayTime = (ushort)(value.TotalMilliseconds / 100); }
    }
    public string Description
    {
      get 
      { 
          if(string.IsNullOrEmpty(description))
              return string.Empty;
          
          return description.Trim(); 
      }
      set { description = value.PadRight(24, ' '); }
    }
    private string description;
    public DigitalInputMonitoringConditions MonitoringCondition
    {
      get 
      {
        if (monitoringCondition != 0x028C && monitoringCondition != 0x028D && monitoringCondition != 0x028E && monitoringCondition != 0x028F)
          return DigitalInputMonitoringConditions.Always;
        return (DigitalInputMonitoringConditions)monitoringCondition; 
      }
      set { monitoringCondition = (ushort)value; }
    }
    public bool MaintenanceModeEnabled;
    public TimeSpan MaintenanceModeDuration
    {
      get { return TimeSpan.FromHours(maintenanceModeDurationHours); }
      set { maintenanceModeDurationHours = (byte)value.TotalHours; }
    }
    public TimeSpan SMU
    {
      get { return TimeSpan.FromMinutes(smuMinutes); }
      set { smuMinutes = (uint)value.TotalMinutes; }
    }

    public MachineStartStatus StartStatus
    {
      get
      {
        if (machineStartStatus != 0x00 && machineStartStatus != 0x01 && machineStartStatus != 0x02
          && machineStartStatus != 0x10 && machineStartStatus != 0x11 && machineStartStatus != 0x12)
          return MachineStartStatus.NormalOperation;
        return (MachineStartStatus)machineStartStatus;
      }
      set { machineStartStatus = (byte)value; }
    }

    public TamperResistanceStatus ResistanceStatus
    {
      get
      {
        if (tamperResistanceStatus != 0x00 && tamperResistanceStatus != 0x01 && tamperResistanceStatus != 0x02 && tamperResistanceStatus != 0x03)
          return TamperResistanceStatus.Off;
        return (TamperResistanceStatus)tamperResistanceStatus;
      }
      set { tamperResistanceStatus = (byte)value; }
    }

    private byte fieldID;
    private byte config;
    private ushort delayTime;
    private ushort monitoringCondition;
    private byte maintenanceModeDurationHours;
    private uint smuMinutes;
    private byte tamperResistanceStatus;
    private byte machineStartStatus;

    public override void Serialize(SerializationAction action, byte[] raw, ref uint bitPosition)
    {
      serializer(action, raw, ref bitPosition, 8, ref fieldID);
      SerializeParameters(action, raw, ref bitPosition, FieldIdentifier);
    }

    private void SerializeParameters(SerializationAction action, byte[] raw, ref uint bitPosition, FieldID fieldID)
    {
      switch (fieldID)
      {
        case FieldID.DigitalInput1Config:
        case FieldID.DigitalInput2Config:
        case FieldID.DigitalInput3Config:
        case FieldID.DigitalInput4Config:
          serializer(action, raw, ref bitPosition, 8, ref config);
          break;
        case FieldID.DigitalInput1DelayTime:
        case FieldID.DigitalInput2DelayTime:
        case FieldID.DigitalInput3DelayTime:
        case FieldID.DigitalInput4DelayTime:
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref delayTime);
          break;
        case FieldID.DigitalInput1Description:
        case FieldID.DigitalInput2Description:
        case FieldID.DigitalInput3Description:
        case FieldID.DigitalInput4Description:
          serializeFixedLengthString(action, raw, ref bitPosition, 24, ref description);
          break;
        case FieldID.SMU:
          BigEndianSerializer(action, raw, ref bitPosition, 4, ref smuMinutes);
          break;
        case FieldID.DigitalInput1MonitoringCondition:
        case FieldID.DigitalInput2MonitoringCondition:
        case FieldID.DigitalInput3MonitoringCondition:
        case FieldID.DigitalInput4MonitoringCondition:
          BigEndianSerializer(action, raw, ref bitPosition, 2, ref monitoringCondition);
          break;
        case FieldID.MaintenanceMode:
          serializer(action, raw, ref bitPosition, 8, ref MaintenanceModeEnabled);
          break;
        case FieldID.MaintenanceModeDurationTimer:
          serializer(action, raw, ref bitPosition, 8, ref maintenanceModeDurationHours);
          break;
        case FieldID.MachineStartMode:
          serializer(action, raw, ref bitPosition, 8, ref machineStartStatus);
          break;
        case FieldID.TamperResistanceMode:
          serializer(action, raw, ref bitPosition, 8, ref tamperResistanceStatus);
          break;
        default:
          throw new Exception("Unsupported FieldID");
      }
    }

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder("OTAConfigParameters");
      builder.AppendFormat("\nFieldID:  {0}", FieldIdentifier.ToString());
      switch(FieldIdentifier)
      {
        case FieldID.DigitalInput1Config:
        case FieldID.DigitalInput2Config:
        case FieldID.DigitalInput3Config:
        case FieldID.DigitalInput4Config:
          builder.AppendFormat("\nConfiguration:  {0}", Configuration.ToString());
          break;
        case FieldID.DigitalInput1DelayTime:
        case FieldID.DigitalInput2DelayTime:
        case FieldID.DigitalInput3DelayTime:
        case FieldID.DigitalInput4DelayTime:
          builder.AppendFormat("\nDelayTime:  {0}", DelayTime.ToString());
          break;
        case FieldID.DigitalInput1Description:
        case FieldID.DigitalInput2Description:
        case FieldID.DigitalInput3Description:
        case FieldID.DigitalInput4Description:
          builder.AppendFormat("\nDescription:  {0}", Description.ToString());
          break;
        case FieldID.SMU:
          builder.AppendFormat("\nDescription:  {0}", SMU.ToString());
          break;
        case FieldID.DigitalInput1MonitoringCondition:
        case FieldID.DigitalInput2MonitoringCondition:
        case FieldID.DigitalInput3MonitoringCondition:
        case FieldID.DigitalInput4MonitoringCondition:
          builder.AppendFormat("\nMonitoringCondition:  {0}", MonitoringCondition.ToString());
          break;
        case FieldID.MaintenanceMode:
          builder.AppendFormat("\nMaintenanceModeEnabled:  {0}", MaintenanceModeEnabled);
          break;
        case FieldID.MaintenanceModeDurationTimer:
          builder.AppendFormat("\nMaintenanceModeEnabled:  {0}", MaintenanceModeDuration.ToString());
          break;
        case FieldID.MachineStartMode:
          builder.AppendFormat("\nMachineStartStatus: {0}", StartStatus.ToString());
          break;
        case FieldID.TamperResistanceMode:
          builder.AppendFormat("\nTamperResistanceStatus: {0}", ResistanceStatus.ToString());
          break;
      }

      return builder.ToString();
    }
  }
}
