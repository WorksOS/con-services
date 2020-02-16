using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
namespace VSS.Hosted.VLCommon.MTSMessages
{
  public static class MTSUpdateDeviceConfig
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
   
    private static void UpdateConfig(string serialNumber, DeviceTypeEnum deviceType, IEnumerable<DeviceConfigBase> configs, IConfigStatus configStatus = null)
    {
      foreach (DeviceConfigBase config in configs)
      {
        if (configStatus == null)
          ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, config);
        else
        {
          configStatus.UpdateDeviceConfiguration(serialNumber, deviceType, config);
        }
      }
    }

    public static void UpdateRuntimeCalibration(string serialNumber, DeviceTypeEnum deviceType, double SMH, long runtimeCalibrationID)
    {
      RuntimeAdjConfig runtime = new RuntimeAdjConfig();
      runtime.Runtime = TimeSpan.FromHours(SMH);
      runtime.MessageSourceID = runtimeCalibrationID;
      runtime.Status = MessageStatusEnum.Acknowledged;

      ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, runtime);
    }

    public static void UpdateMachineSecuritySystemInformation(string serialNumber, DeviceTypeEnum deviceType, DeviceMachineSecurityReportingStatusMessage message, long? messageID)
    {
        if ((int) message.CurrentMachineSecurityModeconfiguration > 5) // this needs to be removed once firmware is updated for derate and other conditions
        {
            log.InfoFormat("Message not processed due to un expected current machine security value {0} for serial number - {0}", (int)message.CurrentMachineSecurityModeconfiguration, serialNumber);
            return;
        }
        if ((int)message.LatestMachineSecurityModeconfiguration > 5) // this needs to be removed once firmware is updated for derate and other conditions
        {
            log.InfoFormat("Message not processed due to un expected latest machine security value {0} for serial number - {1}", (int)message.LatestMachineSecurityModeconfiguration, serialNumber);
            return;
        }

         if (message.CurrentMachineSecurityModeconfiguration == MachineSecurityModeSetting.MachineInDisableModebutPowercut) // this needs to be ignored as it is duplicate 'disable' message sent by device once power is restored
        {
            log.InfoFormat("Message not processed as this is duplicate of disabled message generated after powercut: CurrentMachineMode - {0}", message.CurrentMachineSecurityModeconfiguration.ToString());
            return;
        }


        MTSConfigData.DeviceMachineSecurityReportingStatusMessageConfig machineSecurityStatusConfig = new MTSConfigData.DeviceMachineSecurityReportingStatusMessageConfig();
        // in UI there is only one state disabled so if a message received is disabled or other disabled mode, the status is updated to disabled

        GetFormattedStartMode(message.CurrentMachineSecurityModeconfiguration, ref machineSecurityStatusConfig.currentMachineSecurityModeconfiguration);
        GetFormattedStartMode(message.LatestMachineSecurityModeconfiguration, ref machineSecurityStatusConfig.latestMachineSecurityModeconfiguration);

        machineSecurityStatusConfig.tamperResistanceStatus = message.TamperResistanceMode;
        machineSecurityStatusConfig.sourceSecurityModeConfiguration = message.SourceSecurityModeConfiguration;
        machineSecurityStatusConfig.deviceSecurityModeReceivingStatus = message.DeviceSecurityModeReceivingStatus;
        machineSecurityStatusConfig.MessageSourceID = messageID.Value;
        machineSecurityStatusConfig.SentUTC = DateTime.UtcNow;
        if (message.DeviceSecurityModeReceivingStatus == DeviceSecurityModeReceivingStatus.ModechangeRequestReceivedImplementationPending)
        {
            machineSecurityStatusConfig.Status = MessageStatusEnum.Pending;
            machineSecurityStatusConfig.MessageSourceID = -1; // passed as -1, this is do differentiate between machine event with pending and success, for success values is 0
        }
        else
        {
            machineSecurityStatusConfig.Status = MessageStatusEnum.Acknowledged;
        }
        machineSecurityStatusConfig.packetID = message.PacketID;
        ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, machineSecurityStatusConfig);
    }

    private static void GetFormattedStartMode(MachineSecurityModeSetting machineSecurityMode,ref MachineStartStatus? machineSecurityModeConfiguration)
    {
      if (machineSecurityMode == MachineSecurityModeSetting.Disabled || machineSecurityMode == MachineSecurityModeSetting.MachineInDisableModebutsecuritytamperedorbypass)
        machineSecurityModeConfiguration = MachineStartStatus.Disabled;
      else if (machineSecurityMode == MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureEnabled || machineSecurityMode == MachineSecurityModeSetting.NormalOperationWithMachineSecurityFeatureDisabled)
        machineSecurityModeConfiguration = MachineStartStatus.NormalOperation;
    }

    public static void UpdateMachineSecuritySystemInformation(string serialNumber, DeviceTypeEnum deviceType, TamperSecurityAdministrationInformationMessage message, long? messageID)
    {
      MTSConfigData.TamperSecurityAdministrationInformationConfig machineSecurityStatusConfig = GetMachineSecuritySystemConfig(message, messageID);

      if (null != machineSecurityStatusConfig)
        ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, machineSecurityStatusConfig);
    }

    public static void UpdateMachineSecuritySystemInformation(string serialNumber, DeviceTypeEnum deviceType, TamperSecurityStatusInformationMessage message, long? messageID)
    {
      MTSConfigData.TamperSecurityAdministrationInformationConfig machineSecurityStatusConfig = new MTSConfigData.TamperSecurityAdministrationInformationConfig();
      machineSecurityStatusConfig.machineStartStatus = message.StartStatus;
      machineSecurityStatusConfig.machineStartStatusField = FieldID.MachineStartMode;
      machineSecurityStatusConfig.tamperResistanceStatusField = FieldID.TamperResistanceMode;
      machineSecurityStatusConfig.MessageSourceID = messageID.Value;
      machineSecurityStatusConfig.SentUTC = DateTime.UtcNow;
      machineSecurityStatusConfig.Status = MessageStatusEnum.Acknowledged;
      machineSecurityStatusConfig.packetID = message.Parent.PacketID;
      machineSecurityStatusConfig.machineStartStatusTrigger = message.StartStatusTrigger;
      ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, machineSecurityStatusConfig);
    }
    public static void UpdateMaintenanceMode(string serialNumber, DeviceTypeEnum deviceType, MaintenanceAdministrationInformation message, long messageID, DateTime eventUTC, IConfigStatus configStatus = null)
    {
      UpdateConfig(serialNumber, deviceType, new List<DeviceConfigBase> {message.GetMaintModeConfig(MessageStatusEnum.Acknowledged, messageID, eventUTC)}, configStatus);
    }

    private static MTSConfigData.TamperSecurityAdministrationInformationConfig GetMachineSecuritySystemConfig(TamperSecurityAdministrationInformationMessage message, long? messageID)
    {
      MTSConfigData.TamperSecurityAdministrationInformationConfig machineSecuritySystemInformation = null;

      machineSecuritySystemInformation = new MTSConfigData.TamperSecurityAdministrationInformationConfig();
      machineSecuritySystemInformation.machineStartStatus = message.StartStatus;
      machineSecuritySystemInformation.machineStartStatusField = FieldID.MachineStartMode;
      machineSecuritySystemInformation.tamperResistanceStatus = message.ResistanceStatus;
      machineSecuritySystemInformation.tamperResistanceStatusField = FieldID.TamperResistanceMode;
      machineSecuritySystemInformation.MessageSourceID = messageID.Value;
      machineSecuritySystemInformation.SentUTC = DateTime.UtcNow;
      machineSecuritySystemInformation.Status = MessageStatusEnum.Acknowledged;
      machineSecuritySystemInformation.machineStartModeConfigurationSource = message.MachineStartModeConfiguration;
      machineSecuritySystemInformation.tamperResistanceModeConfigurationSource = message.TamperResistanceModeConfiguration;
      machineSecuritySystemInformation.machineSecurityMode = message.MachineSecurityMode;
      machineSecuritySystemInformation.packetID = message.Parent.PacketID;
      machineSecuritySystemInformation.machineStartStatusTrigger = null;
      return machineSecuritySystemInformation;
    }

    public static void UpdateTMSConfig(string serialNumber, DeviceTypeEnum deviceType, TMSInformationMessage message, long? messageID)
    {
        MTSConfigData.TMSConfig tmsConfig = new MTSConfigData.TMSConfig();
        tmsConfig.IsEnabled = message.InstallationStatus == 16 ? true : false;
        if (messageID.HasValue) tmsConfig.MessageSourceID = messageID.Value;
        tmsConfig.Status = MessageStatusEnum.Acknowledged;
        ConfigStatusSvcClient.ProcessDeviceConfiguration(serialNumber, deviceType, tmsConfig);      
    }

  }
}
