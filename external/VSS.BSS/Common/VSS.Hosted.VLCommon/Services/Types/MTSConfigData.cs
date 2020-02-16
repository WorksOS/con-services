using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spring.Util;
using VSS.Hosted.VLCommon;
using System.Runtime.Serialization;
using log4net;

namespace VSS.Hosted.VLCommon
{
	public class MTSConfigData : DeviceConfigData
	{

		private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
		private AssetSecurityEventStatus tamperStartEventFlag = AssetSecurityEventStatus.Unknown;

		public MTSConfigData() { }
		public MTSConfigData(string xml)
			: base(xml)
		{

		}
		public MTSConfigData(XElement xml)
			: base(xml)
		{

		}

		#region General DeviceConfigs

		public MileageRuntimeConfig CurrentRTMileage = null;
		public MileageRuntimeConfig PendingRTMileage = null;

		public PasscodeConfig CurrentPasscode = null;
		public PasscodeConfig PendingPasscode = null;

		public RuntimeAdjConfig CurrentRuntimeAdj = null;
		public RuntimeAdjConfig PendingRuntimeAdj = null;

		public DailyReportConfig CurrentDailyReport = null;
		public DailyReportConfig PendingDailyReport = null;

		public SpeedingConfig CurrentSpeeding = null;
		public SpeedingConfig PendingSpeeding = null;

		public MovingConfig CurrentMoving = null;
		public MovingConfig PendingMoving = null;

		public StoppedConfig CurrentStopped = null;
		public StoppedConfig PendingStopped = null;

		#endregion

		#region SMHDataSource Config
		public SMHSourceConfig CurrentSMHSource = null;
		public SMHSourceConfig PendingSMHSource = null;
		#endregion

		#region MaintenanceMode Configs

		public MaintenanceModeConfig CurrentMaintMode = null;
		public MaintenanceModeConfig PendingMaintMode = null;

		#endregion

		#region TPMS Configs

		public TMSConfig CurrentTmsMode = null;
		public TMSConfig PendingTmsMode = null;

		#endregion

		#region DigitalInput Configs

		public DiscreteInputConfig CurrentIO = null;
		public DiscreteInputConfig PendingIO = null;

		public DigitalSwitchConfig CurrentDigitalSwitch1 = null;
		public DigitalSwitchConfig PendingDigitalSwitch1 = null;

		public DigitalSwitchConfig CurrentDigitalSwitch2 = null;
		public DigitalSwitchConfig PendingDigitalSwitch2 = null;

		public DigitalSwitchConfig CurrentDigitalSwitch3 = null;
		public DigitalSwitchConfig PendingDigitalSwitch3 = null;

		public DigitalSwitchConfig CurrentDigitalSwitch4 = null;
		public DigitalSwitchConfig PendingDigitalSwitch4 = null;

		#endregion

		#region Machine Security Information Configs - for Gateway message security

		public TamperSecurityAdministrationInformationConfig currentMachineSecuritySystemInformationConfig = null;
		public TamperSecurityAdministrationInformationConfig pendingMachineSecuritySystemInformationConfig = null;

		#endregion

		#region Device Machine Security Information Configs - for Radio Message Security

		public DeviceMachineSecurityReportingStatusMessageConfig currentDeviceMachineSecurityReportingStatusMessageConfig = null;
		public DeviceMachineSecurityReportingStatusMessageConfig pendingDeviceMachineSecurityReportingStatusMessageConfig = null;

		#endregion

		/// <summary>
		/// Updates this object with the supplied config data.
		/// 
		/// There are two expected cases:
		/// 1) the supplied config is a newly sent config, that is not yet ack'd
		/// 2) the supplied config is an acknowledged, previously sent config
		/// </summary>
		/// <param name="config"></param>
		public override void Update(DeviceConfigBase config)
		{
			if (config is MileageRuntimeConfig)
			{
				if (CurrentRTMileage == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentRTMileage = new MileageRuntimeConfig();
				if (PendingRTMileage == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingRTMileage = new MileageRuntimeConfig();
				Update<MileageRuntimeConfig>(ref CurrentRTMileage, ref PendingRTMileage, (MileageRuntimeConfig)config);
			}
			if (config is PasscodeConfig)
			{
				if (CurrentPasscode == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentPasscode = new PasscodeConfig();
				if (PendingPasscode == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingPasscode = new PasscodeConfig();
				Update<PasscodeConfig>(ref CurrentPasscode, ref PendingPasscode, (PasscodeConfig)config);
			}
			else if (config is RuntimeAdjConfig)
			{
				if (CurrentRuntimeAdj == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentRuntimeAdj = new RuntimeAdjConfig();
				if (PendingRuntimeAdj == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingRuntimeAdj = new RuntimeAdjConfig();
				Update<RuntimeAdjConfig>(ref CurrentRuntimeAdj, ref PendingRuntimeAdj, (RuntimeAdjConfig)config);
			}
			else if (config is DailyReportConfig)
			{
				if (CurrentDailyReport == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentDailyReport = new DailyReportConfig();
				if (PendingDailyReport == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingDailyReport = new DailyReportConfig();
				Update<DailyReportConfig>(ref CurrentDailyReport, ref PendingDailyReport, (DailyReportConfig)config);
			}
			else if (config is SpeedingConfig)
			{
				if (CurrentSpeeding == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentSpeeding = new SpeedingConfig();
				if (PendingSpeeding == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingSpeeding = new SpeedingConfig();
				Update<SpeedingConfig>(ref CurrentSpeeding, ref PendingSpeeding, (SpeedingConfig)config);
			}
			else if (config is MovingConfig)
			{
				if (CurrentMoving == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentMoving = new MovingConfig();
				if (PendingMoving == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingMoving = new MovingConfig();
				Update<MovingConfig>(ref CurrentMoving, ref PendingMoving, (MovingConfig)config);
			}
			else if (config is StoppedConfig)
			{
				if (CurrentStopped == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentStopped = new StoppedConfig();
				if (PendingStopped == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingStopped = new StoppedConfig();
				Update<StoppedConfig>(ref CurrentStopped, ref PendingStopped, (StoppedConfig)config);
			}
			else if (config is MaintenanceModeConfig)
			{
				if (CurrentMaintMode == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentMaintMode = new MaintenanceModeConfig();
				if (PendingMaintMode == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingMaintMode = new MaintenanceModeConfig();
				Update<MaintenanceModeConfig>(ref CurrentMaintMode, ref PendingMaintMode, (MaintenanceModeConfig)config);
			}
			else if (config is TMSConfig)
			{
				if (CurrentTmsMode != null && (CurrentTmsMode.IsEnabled == (config as TMSConfig).IsEnabled))
				{
					// the 522/523 send a tmsinfo report with every engine-onoff/ignition-onoff, 
					// which would update the config even when the settings haven't changed
					// therefore, if the config hasn't changed, do nothing!
				}
				else
				{
					if (CurrentTmsMode == null && config.Status == MessageStatusEnum.Acknowledged)
						CurrentTmsMode = new TMSConfig();
					if (PendingTmsMode == null && config.Status != MessageStatusEnum.Acknowledged)
						PendingTmsMode = new TMSConfig();
					Update<TMSConfig>(ref CurrentTmsMode, ref PendingTmsMode, (TMSConfig)config);
				}
			}
			else if (config is DiscreteInputConfig)
			{
				if (CurrentIO == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentIO = new DiscreteInputConfig();
				if (PendingIO == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingIO = new DiscreteInputConfig();
				Update<DiscreteInputConfig>(ref CurrentIO, ref PendingIO, (DiscreteInputConfig)config);
			}
			else if (config is SMHSourceConfig)
			{
				if (CurrentSMHSource == null && config.Status == MessageStatusEnum.Acknowledged)
					CurrentSMHSource = new SMHSourceConfig();
				if (PendingSMHSource == null && config.Status != MessageStatusEnum.Acknowledged)
					PendingSMHSource = new SMHSourceConfig();
				Update<SMHSourceConfig>(ref CurrentSMHSource, ref PendingSMHSource, (SMHSourceConfig)config);
			}
			else if (config is DigitalSwitchConfig)
			{
				DigitalSwitchConfig switchConfig = config as DigitalSwitchConfig;
				switch (switchConfig.Field)
				{
					case FieldID.DigitalInput1Config:
						if (CurrentDigitalSwitch1 == null && config.Status == MessageStatusEnum.Acknowledged)
							CurrentDigitalSwitch1 = new DigitalSwitchConfig();
						if (PendingDigitalSwitch1 == null && config.Status != MessageStatusEnum.Acknowledged)
							PendingDigitalSwitch1 = new DigitalSwitchConfig();
						Update<DigitalSwitchConfig>(ref CurrentDigitalSwitch1, ref PendingDigitalSwitch1, (DigitalSwitchConfig)config);
						break;
					case FieldID.DigitalInput2Config:
						if (CurrentDigitalSwitch2 == null && config.Status == MessageStatusEnum.Acknowledged)
							CurrentDigitalSwitch2 = new DigitalSwitchConfig();
						if (PendingDigitalSwitch2 == null && config.Status != MessageStatusEnum.Acknowledged)
							PendingDigitalSwitch2 = new DigitalSwitchConfig();
						Update<DigitalSwitchConfig>(ref CurrentDigitalSwitch2, ref PendingDigitalSwitch2, (DigitalSwitchConfig)config);
						break;
					case FieldID.DigitalInput3Config:
						if (CurrentDigitalSwitch3 == null && config.Status == MessageStatusEnum.Acknowledged)
							CurrentDigitalSwitch3 = new DigitalSwitchConfig();
						if (PendingDigitalSwitch3 == null && config.Status != MessageStatusEnum.Acknowledged)
							PendingDigitalSwitch3 = new DigitalSwitchConfig();
						Update<DigitalSwitchConfig>(ref CurrentDigitalSwitch3, ref PendingDigitalSwitch3, (DigitalSwitchConfig)config);
						break;
					case FieldID.DigitalInput4Config:
						if (CurrentDigitalSwitch4 == null && config.Status == MessageStatusEnum.Acknowledged)
							CurrentDigitalSwitch4 = new DigitalSwitchConfig();
						if (PendingDigitalSwitch4 == null && config.Status != MessageStatusEnum.Acknowledged)
							PendingDigitalSwitch4 = new DigitalSwitchConfig();
						Update<DigitalSwitchConfig>(ref CurrentDigitalSwitch4, ref PendingDigitalSwitch4, (DigitalSwitchConfig)config);
						break;
					default: break;
				}
			}
			else if (config is TamperSecurityAdministrationInformationConfig)
			{
				TamperSecurityAdministrationInformationConfig latest = (TamperSecurityAdministrationInformationConfig)config;

				if (currentMachineSecuritySystemInformationConfig == null && config.Status == MessageStatusEnum.Acknowledged)
					currentMachineSecuritySystemInformationConfig = new TamperSecurityAdministrationInformationConfig();
				if (pendingMachineSecuritySystemInformationConfig == null && config.Status != MessageStatusEnum.Acknowledged)
					pendingMachineSecuritySystemInformationConfig = new TamperSecurityAdministrationInformationConfig();

				Update<TamperSecurityAdministrationInformationConfig>(ref currentMachineSecuritySystemInformationConfig,
					 ref pendingMachineSecuritySystemInformationConfig, (TamperSecurityAdministrationInformationConfig)config);

				if (latest.packetID == 83) // 53 message
				{
					//updates pending, current to null
					bool isTamperResistanceChanged = currentMachineSecuritySystemInformationConfig.SetTamperLevel(latest);
					currentMachineSecuritySystemInformationConfig.machineSecurityMode = latest.machineSecurityMode;

					if (isTamperResistanceChanged)
					{
						tamperStartEventFlag = AssetSecurityEventStatus.TamperLevel;
					}
					if (pendingMachineSecuritySystemInformationConfig == null)
						pendingMachineSecuritySystemInformationConfig = new TamperSecurityAdministrationInformationConfig();

					if ((pendingMachineSecuritySystemInformationConfig.machineStartStatusSentUTC >
						currentMachineSecuritySystemInformationConfig.machineStartStatusSentUTC) ||
							(currentMachineSecuritySystemInformationConfig.machineStartStatus != latest.machineStartStatus))
					{
						bool isStartModeChanged = false;
						isStartModeChanged = pendingMachineSecuritySystemInformationConfig.SetStartMode(latest);
						pendingMachineSecuritySystemInformationConfig.machineStartModeConfigurationSource = latest.machineStartModeConfigurationSource;
						if (isStartModeChanged && isTamperResistanceChanged)//setting flags for start mode
							tamperStartEventFlag = AssetSecurityEventStatus.TamperAppliedStartModeConfigured;

						else if (isStartModeChanged)
						{
							tamperStartEventFlag = AssetSecurityEventStatus.StartModeConfigured;
						}
					}
					currentMachineSecuritySystemInformationConfig.machineStartStatusTrigger = null;


				}
				else if (latest.packetID == 70) // 46 message(gets from machine)
				{
					//updates pending to null, current to pending 
					// Handle two types of 46 messages:
					// 1.User configured start mode change 
					// 2.Device changes start mode because of tampering and sends a security message 
					bool isStartModechanged = false;
					isStartModechanged = currentMachineSecuritySystemInformationConfig.SetStartMode(latest);
					currentMachineSecuritySystemInformationConfig.machineStartStatusTrigger = latest.machineStartStatusTrigger;
					if (pendingMachineSecuritySystemInformationConfig != null &&
							pendingMachineSecuritySystemInformationConfig.machineStartStatus == latest.machineStartStatus)
					{
						currentMachineSecuritySystemInformationConfig.machineStartModeConfigurationSource =
							pendingMachineSecuritySystemInformationConfig.machineStartModeConfigurationSource;
						pendingMachineSecuritySystemInformationConfig.packetID = null;
						if (isStartModechanged)
							tamperStartEventFlag = AssetSecurityEventStatus.StartMode;
					}

				}
				else
				{
					bool startModeChanged = pendingMachineSecuritySystemInformationConfig.SetStartMode(latest);
					bool tamperResistanceChanged = pendingMachineSecuritySystemInformationConfig.SetTamperLevel(latest);
					pendingMachineSecuritySystemInformationConfig.userID = latest.userID;


					if (tamperResistanceChanged && startModeChanged)
					{
						tamperStartEventFlag = AssetSecurityEventStatus.StartModeTamperLevelPending;
					}
					else if (tamperResistanceChanged)
					{
						tamperStartEventFlag = AssetSecurityEventStatus.TamperLevelPending;
					}
					else if (startModeChanged)
					{
						tamperStartEventFlag = AssetSecurityEventStatus.StartModePending;
					}


				}
			}
			else if (config is DeviceMachineSecurityReportingStatusMessageConfig)
			{
				DeviceMachineSecurityReportingStatusMessageConfig latest = (DeviceMachineSecurityReportingStatusMessageConfig)config;

				if (currentDeviceMachineSecurityReportingStatusMessageConfig == null && config.Status == MessageStatusEnum.Acknowledged)
					currentDeviceMachineSecurityReportingStatusMessageConfig = new DeviceMachineSecurityReportingStatusMessageConfig();
				if (pendingDeviceMachineSecurityReportingStatusMessageConfig == null && config.Status != MessageStatusEnum.Acknowledged)
					pendingDeviceMachineSecurityReportingStatusMessageConfig = new DeviceMachineSecurityReportingStatusMessageConfig();

				Update<DeviceMachineSecurityReportingStatusMessageConfig>(ref currentDeviceMachineSecurityReportingStatusMessageConfig,
					 ref pendingDeviceMachineSecurityReportingStatusMessageConfig, (DeviceMachineSecurityReportingStatusMessageConfig)config);
			}
		}

		public override bool AuditConfigChanges(INH_OP ctx, Asset asset, DeviceConfigBase config)
		{
			AssetSecurityIncident assetSecurityIncident = new AssetSecurityIncident();

			if (config is TamperSecurityAdministrationInformationConfig && asset != null)
			{
				assetSecurityIncident.SerialNumberVIN = asset.SerialNumberVIN;
				assetSecurityIncident.fk_MakeCode = asset.fk_MakeCode;
				assetSecurityIncident.fk_DeviceTypeID = asset.Device.fk_DeviceTypeID;
				//Setting event type based on the startmode/tamperlevel change
				assetSecurityIncident.EventType = ((int)tamperStartEventFlag).ToString();
				assetSecurityIncident.TimeStampUTC = config.SentUTC;
				
				var pending = pendingMachineSecuritySystemInformationConfig;
				var current = currentMachineSecuritySystemInformationConfig;
				if (pending != null)
				{
					assetSecurityIncident.fk_UserID = pending.userID;
					if (current == null || !current.machineStartStatusSentUTC.HasValue || pending.machineStartStatusSentUTC > current.machineStartStatusSentUTC)
					{
						assetSecurityIncident.TargetStartMode = (int?)pending.machineStartStatus;
					}
					if (current == null || !current.tamperResistanceStatusSentUTC.HasValue || pending.tamperResistanceStatusSentUTC > current.tamperResistanceStatusSentUTC)
					{
						assetSecurityIncident.TargetTamperLevel = (int?)pending.tamperResistanceStatus;
					}
				}

				if (current != null)
				{
					assetSecurityIncident.CurrentTamperLevel = (int?)current.tamperResistanceStatus;
					assetSecurityIncident.CurrentStartMode = (int?)current.machineStartStatus;
					if (current.packetID == 70 && ((int)config.Status).ToString() == "2")
						assetSecurityIncident.StartModeTrigger = (int?)current.machineStartStatusTrigger;
				}

                ///// Undoing Removing Duplicate logic as part of Bug 36353
				//removing the duplicates since we'er not saving config.MessageSource in the incident table so it appears like we're auditing duplicate messages
				//bool skippingAuditForAuditedPendingMessage = pending == null || !pending.userID.HasValue;
					
					/* 
					 * //fix for duplicate pending audits being sent(we only audit pending coming from UI)
					((config.Status == MessageStatusEnum.Pending && pending != null && !pending.userID.HasValue)
					|| config.Status != MessageStatusEnum.Pending)
					&&
					//if we're getting a pending from other than UI
					(current == null || //most likely case but below is just in case
					 (assetSecurityIncident.CurrentTamperLevel == null && assetSecurityIncident.CurrentStartMode == null &&
						assetSecurityIncident.StartModeTrigger == null)); //if we're getting current with no data
				*/
				
				//if(skippingAuditForAuditedPendingMessage)
					//return false;

				ctx.AssetSecurityIncident.AddObject(assetSecurityIncident);
			}
			if (config is DeviceMachineSecurityReportingStatusMessageConfig && asset != null)
			{
				assetSecurityIncident.SerialNumberVIN = asset.SerialNumberVIN;
				assetSecurityIncident.fk_MakeCode = asset.fk_MakeCode;
				assetSecurityIncident.fk_DeviceTypeID = asset.Device.fk_DeviceTypeID;

				assetSecurityIncident.EventType = ((int)config.Status).ToString();
				assetSecurityIncident.TimeStampUTC = config.SentUTC;

				var pending = pendingDeviceMachineSecurityReportingStatusMessageConfig;
				var current = currentDeviceMachineSecurityReportingStatusMessageConfig;
				if (pending != null)
				{
					assetSecurityIncident.fk_UserID = pending.userID;


					if (current == null || !current.machineStartStatusSentUTC.HasValue || pending.machineStartStatusSentUTC > current.machineStartStatusSentUTC)
					{
						assetSecurityIncident.TargetStartMode = (int?)pending.latestMachineSecurityModeconfiguration;
						// This is predified values as we send default values to device
						assetSecurityIncident.TargetTamperLevel = null;

						//setting startmode event
						assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartModePending).ToString();
					}
				}
				if (current != null)
				{
					assetSecurityIncident.CurrentTamperLevel = (int?)current.tamperResistanceStatus;
					assetSecurityIncident.CurrentStartMode = (int?)current.currentMachineSecurityModeconfiguration;
					if (config.Status == MessageStatusEnum.Acknowledged)
					{
						assetSecurityIncident.TargetStartMode = null;
						assetSecurityIncident.TargetTamperLevel = null;

						//setting startmode and tamper level event
						assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartMode).ToString();
					}
				}

				ctx.AssetSecurityIncident.AddObject(assetSecurityIncident);
			}
			return true;
		}

		public override void UpdateCurrentStatus(INH_OP ctx, Asset asset, DeviceConfigBase config)
		{
			if (config is TamperSecurityAdministrationInformationConfig || config is DeviceMachineSecurityReportingStatusMessageConfig && asset != null)
			{
				StoredProcDefinition updateProceDefinition = new StoredProcDefinition("NH_OP", "uspPub_AssetCurrentSecurityStatus");
				updateProceDefinition.AddInput("@SerialNumbeVIN", asset.SerialNumberVIN);
				SqlAccessMethods.ExecuteNonQuery(updateProceDefinition);
			}
		}

		public override XElement ToXElement()
		{
			XElement element = new XElement("MTSConfigData");
			XElement pending = new XElement("Pending");
			XElement current = new XElement("Current");

			if (null != PendingRTMileage) pending.Add(PendingRTMileage.ToXElement());
			if (null != PendingPasscode) pending.Add(PendingPasscode.ToXElement());
			if (null != PendingRuntimeAdj) pending.Add(PendingRuntimeAdj.ToXElement());
			if (null != PendingDailyReport) pending.Add(PendingDailyReport.ToXElement());
			if (null != PendingSpeeding) pending.Add(PendingSpeeding.ToXElement());
			if (null != PendingMoving) pending.Add(PendingMoving.ToXElement());
			if (null != PendingStopped) pending.Add(PendingStopped.ToXElement());
			if (null != PendingMaintMode) pending.Add(PendingMaintMode.ToXElement());
			if (null != PendingTmsMode) pending.Add(PendingTmsMode.ToXElement());
			if (null != PendingIO) pending.Add(PendingIO.ToXElement());
			if (null != PendingSMHSource) pending.Add(PendingSMHSource.ToXElement());

			if (PendingDigitalSwitch1 != null) pending.Add(new XElement("DigitalSwitch1", PendingDigitalSwitch1.ToXElement()));
			if (PendingDigitalSwitch2 != null) pending.Add(new XElement("DigitalSwitch2", PendingDigitalSwitch2.ToXElement()));
			if (PendingDigitalSwitch3 != null) pending.Add(new XElement("DigitalSwitch3", PendingDigitalSwitch3.ToXElement()));
			if (PendingDigitalSwitch4 != null) pending.Add(new XElement("DigitalSwitch4", PendingDigitalSwitch4.ToXElement()));
			if (pendingMachineSecuritySystemInformationConfig != null)
				pending.Add(pendingMachineSecuritySystemInformationConfig.ToXElement());
			if (pendingDeviceMachineSecurityReportingStatusMessageConfig != null)
				pending.Add(pendingDeviceMachineSecurityReportingStatusMessageConfig.ToXElement());

			element.Add(pending);

			if (null != CurrentRTMileage) current.Add(CurrentRTMileage.ToXElement());
			if (null != CurrentPasscode) current.Add(CurrentPasscode.ToXElement());
			if (null != CurrentRuntimeAdj) current.Add(CurrentRuntimeAdj.ToXElement());
			if (null != CurrentDailyReport) current.Add(CurrentDailyReport.ToXElement());
			if (null != CurrentSpeeding) current.Add(CurrentSpeeding.ToXElement());
			if (null != CurrentMoving) current.Add(CurrentMoving.ToXElement());
			if (null != CurrentStopped) current.Add(CurrentStopped.ToXElement());
			if (null != CurrentMaintMode) current.Add(CurrentMaintMode.ToXElement());
			if (null != CurrentTmsMode) current.Add(CurrentTmsMode.ToXElement());
			if (null != CurrentIO) current.Add(CurrentIO.ToXElement());
			if (null != CurrentSMHSource) current.Add(CurrentSMHSource.ToXElement());

			if (CurrentDigitalSwitch1 != null) current.Add(new XElement("DigitalSwitch1", CurrentDigitalSwitch1.ToXElement()));
			if (CurrentDigitalSwitch2 != null) current.Add(new XElement("DigitalSwitch2", CurrentDigitalSwitch2.ToXElement()));
			if (CurrentDigitalSwitch3 != null) current.Add(new XElement("DigitalSwitch3", CurrentDigitalSwitch3.ToXElement()));
			if (CurrentDigitalSwitch4 != null) current.Add(new XElement("DigitalSwitch4", CurrentDigitalSwitch4.ToXElement()));

			if (currentMachineSecuritySystemInformationConfig != null)
				current.Add(currentMachineSecuritySystemInformationConfig.ToXElement());

			if (currentDeviceMachineSecurityReportingStatusMessageConfig != null)
				current.Add(currentDeviceMachineSecurityReportingStatusMessageConfig.ToXElement());

			element.Add(current);

			return element;
		}

		#region Implementation

		protected override void GetPending(XElement pending)
		{
			XElement rtMileage = pending.Elements("MileageRuntimeConfig").FirstOrDefault();
			if (rtMileage != null)
				PendingRTMileage = new MileageRuntimeConfig(rtMileage);

			XElement passcode = pending.Elements("PasscodeConfig").FirstOrDefault();
			if (passcode != null)
				PendingPasscode = new PasscodeConfig(passcode);

			XElement runtime = pending.Elements("RuntimeAdjConfig").FirstOrDefault();
			if (runtime != null)
				PendingRuntimeAdj = new RuntimeAdjConfig(runtime);

			XElement daily = pending.Elements("DailyReportConfig").FirstOrDefault();
			if (daily != null)
				PendingDailyReport = new DailyReportConfig(daily);

			XElement speeding = pending.Elements("SpeedingConfig").FirstOrDefault();
			if (speeding != null)
				PendingSpeeding = new SpeedingConfig(speeding);

			XElement moving = pending.Elements("MovingConfig").FirstOrDefault();
			if (moving != null)
				PendingMoving = new MovingConfig(moving);

			XElement stopped = pending.Elements("StoppedConfig").FirstOrDefault();
			if (stopped != null)
				PendingStopped = new StoppedConfig(stopped);

			XElement maintMode = pending.Elements("MaintenanceModeConfig").FirstOrDefault();
			if (maintMode != null)
				PendingMaintMode = new MaintenanceModeConfig(maintMode);


			XElement tmsconfig = pending.Elements("TMSConfig").FirstOrDefault();
			if (tmsconfig != null)
				PendingTmsMode = new TMSConfig(tmsconfig);

			XElement io = pending.Elements("DiscreteInputConfig").FirstOrDefault();
			if (io != null)
				PendingIO = new DiscreteInputConfig(io);

			XElement digitalSwitch1 = pending.Elements("DigitalSwitch1").FirstOrDefault();
			if (digitalSwitch1 != null)
				PendingDigitalSwitch1 = new DigitalSwitchConfig(digitalSwitch1);

			XElement digitalSwitch2 = pending.Elements("DigitalSwitch2").FirstOrDefault();
			if (digitalSwitch2 != null)
				PendingDigitalSwitch2 = new DigitalSwitchConfig(digitalSwitch2);

			XElement digitalSwitch3 = pending.Elements("DigitalSwitch3").FirstOrDefault();
			if (digitalSwitch3 != null)
				PendingDigitalSwitch3 = new DigitalSwitchConfig(digitalSwitch3);

			XElement digitalSwitch4 = pending.Elements("DigitalSwitch4").FirstOrDefault();
			if (digitalSwitch4 != null)
				PendingDigitalSwitch4 = new DigitalSwitchConfig(digitalSwitch4);

			XElement machineSecuritySystem = pending.Elements("MachineSecuritySystemConfig").FirstOrDefault();
			if (machineSecuritySystem != null)
				pendingMachineSecuritySystemInformationConfig = new TamperSecurityAdministrationInformationConfig(machineSecuritySystem);

			XElement devicemachineSecuritySystem = pending.Elements("DeviceMachineSecurityConfig").FirstOrDefault();
			if (devicemachineSecuritySystem != null)
				pendingDeviceMachineSecurityReportingStatusMessageConfig = new DeviceMachineSecurityReportingStatusMessageConfig(devicemachineSecuritySystem);


			XElement smhSource = pending.Elements("SMHSourceConfig").FirstOrDefault();
			if (smhSource != null)
				PendingSMHSource = new SMHSourceConfig(smhSource);
		}

		protected override void GetCurrent(XElement current)
		{
			XElement rtMileage = current.Elements("MileageRuntimeConfig").FirstOrDefault();
			if (rtMileage != null)
				CurrentRTMileage = new MileageRuntimeConfig(rtMileage);

			XElement passcode = current.Elements("PasscodeConfig").FirstOrDefault();
			if (passcode != null)
				CurrentPasscode = new PasscodeConfig(passcode);

			XElement runtime = current.Elements("RuntimeAdjConfig").FirstOrDefault();
			if (runtime != null)
				CurrentRuntimeAdj = new RuntimeAdjConfig(runtime);

			XElement daily = current.Elements("DailyReportConfig").FirstOrDefault();
			if (daily != null)
				CurrentDailyReport = new DailyReportConfig(daily);

			XElement speeding = current.Elements("SpeedingConfig").FirstOrDefault();
			if (speeding != null)
				CurrentSpeeding = new SpeedingConfig(speeding);

			XElement moving = current.Elements("MovingConfig").FirstOrDefault();
			if (moving != null)
				CurrentMoving = new MovingConfig(moving);

			XElement stopped = current.Elements("StoppedConfig").FirstOrDefault();
			if (stopped != null)
				CurrentStopped = new StoppedConfig(stopped);

			XElement maintMode = current.Elements("MaintenanceModeConfig").FirstOrDefault();
			if (maintMode != null)
				CurrentMaintMode = new MaintenanceModeConfig(maintMode);

			XElement tmsconfig = current.Elements("TMSConfig").FirstOrDefault();
			if (tmsconfig != null)
				CurrentTmsMode = new TMSConfig(tmsconfig);

			XElement io = current.Elements("DiscreteInputConfig").FirstOrDefault();
			if (io != null)
				CurrentIO = new DiscreteInputConfig(io);

			XElement digitalSwitch1 = current.Elements("DigitalSwitch1").FirstOrDefault();
			if (digitalSwitch1 != null)
				CurrentDigitalSwitch1 = new DigitalSwitchConfig(digitalSwitch1);

			XElement digitalSwitch2 = current.Elements("DigitalSwitch2").FirstOrDefault();
			if (digitalSwitch2 != null)
				CurrentDigitalSwitch2 = new DigitalSwitchConfig(digitalSwitch2);

			XElement digitalSwitch3 = current.Elements("DigitalSwitch3").FirstOrDefault();
			if (digitalSwitch3 != null)
				CurrentDigitalSwitch3 = new DigitalSwitchConfig(digitalSwitch3);

			XElement digitalSwitch4 = current.Elements("DigitalSwitch4").FirstOrDefault();
			if (digitalSwitch4 != null)
				CurrentDigitalSwitch4 = new DigitalSwitchConfig(digitalSwitch4);

			XElement machineSecuritySystem = current.Elements("MachineSecuritySystemConfig").FirstOrDefault();
			if (machineSecuritySystem != null)
				currentMachineSecuritySystemInformationConfig = new TamperSecurityAdministrationInformationConfig(machineSecuritySystem);

			XElement devicemachineSecuritySystem = current.Elements("DeviceMachineSecurityConfig").FirstOrDefault();
			if (devicemachineSecuritySystem != null)
				currentDeviceMachineSecurityReportingStatusMessageConfig = new DeviceMachineSecurityReportingStatusMessageConfig(devicemachineSecuritySystem);

			XElement smhSource = current.Elements("SMHSourceConfig").FirstOrDefault();
			if (smhSource != null)
				CurrentSMHSource = new SMHSourceConfig(smhSource);
		}

		protected override void GetLastSent(XElement lastSent)
		{
			// Not required to be implemented
		}

		#endregion

		#region Sub types

		[DataContract]
		public class MileageRuntimeConfig : DeviceConfigBase
		{
			[DataMember]
			public double Mileage;
			[DataMember]
			public long RuntimeHours;

			public MileageRuntimeConfig() { }

			public MileageRuntimeConfig(string xml)
				: base(xml)
			{

			}

			public MileageRuntimeConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement mileageRuntimeConfig = new XElement("MileageRuntimeConfig");
				mileageRuntimeConfig.SetAttributeValue("Mileage", Mileage);
				mileageRuntimeConfig.SetAttributeValue("RuntimeHours", RuntimeHours);

				return mileageRuntimeConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				MileageRuntimeConfig mileage = latest as MileageRuntimeConfig;
				this.Mileage = mileage.Mileage;
				this.RuntimeHours = mileage.RuntimeHours;
			}

			protected override void Parse(XElement element)
			{
				double? mileage = element.GetDoubleAttribute("Mileage");
				if (mileage.HasValue)
					Mileage = mileage.Value;

				long? runtime = element.GetLongAttribute("RuntimeHours");
				if (runtime.HasValue)
					RuntimeHours = runtime.Value;
			}
		}

		[DataContract]
		public class PasscodeConfig : DeviceConfigBase
		{
			[DataMember]
			public string Passcode;

			public PasscodeConfig() { }

			public PasscodeConfig(string xml)
				: base(xml)
			{

			}

			public PasscodeConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement passcodeConfig = new XElement("PasscodeConfig");
				passcodeConfig.SetAttributeValue("Passcode", Passcode);

				return passcodeConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				PasscodeConfig passcode = latest as PasscodeConfig;
				this.Passcode = passcode.Passcode;
			}

			protected override void Parse(XElement element)
			{
				Passcode = element.GetStringAttribute("Passcode");
			}
		}

		[DataContract]
		public class DailyReportConfig : DeviceConfigBase
		{
			[DataMember]
			public TimeSpan DailyReportTimeUTC;

			public DailyReportConfig() { }

			public DailyReportConfig(string xml)
				: base(xml)
			{

			}

			public DailyReportConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement dailyReportConfig = new XElement("DailyReportConfig");
				dailyReportConfig.SetAttributeValue("DailyReportTimeUTC", DailyReportTimeUTC.ToString());

				return dailyReportConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				DailyReportConfig daily = latest as DailyReportConfig;
				if (daily != null)
					this.DailyReportTimeUTC = daily.DailyReportTimeUTC;
			}

			protected override void Parse(XElement element)
			{
				TimeSpan? dailyReportTimeUTC = element.GetTimeSpanAttribute("DailyReportTimeUTC");
				if (dailyReportTimeUTC.HasValue)
					DailyReportTimeUTC = dailyReportTimeUTC.Value;
			}
		}

		[DataContract]
		public class SpeedingConfig : DeviceConfigBase
		{
			[DataMember]
			public TimeSpan Duration;
			[DataMember]
			public int ThresholdMPH;
			[DataMember]
			public bool IsEnabled;

			public SpeedingConfig() { }

			public SpeedingConfig(string xml)
				: base(xml)
			{

			}

			public SpeedingConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement speedingConfig = new XElement("SpeedingConfig");
				speedingConfig.SetAttributeValue("ThresholdMPH", ThresholdMPH);
				speedingConfig.SetAttributeValue("Duration", Duration.ToString());
				speedingConfig.SetAttributeValue("IsEnabled", IsEnabled);

				return speedingConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				SpeedingConfig speed = latest as SpeedingConfig;
				this.Duration = speed.Duration;
				this.IsEnabled = speed.IsEnabled;
				this.ThresholdMPH = speed.ThresholdMPH;
			}

			protected override void Parse(XElement element)
			{
				int? threshold = element.GetIntAttribute("ThresholdMPH");
				if (threshold.HasValue)
					ThresholdMPH = threshold.Value;

				TimeSpan? duration = element.GetTimeSpanAttribute("Duration");
				if (duration.HasValue)
					Duration = duration.Value;

				bool? isEnabled = element.GetBooleanAttribute("IsEnabled");
				if (isEnabled.HasValue)
					IsEnabled = isEnabled.Value;
			}
		}

		[DataContract]
		public class MovingConfig : DeviceConfigBase
		{
			[DataMember]
			public int RadiusInFeet;

			public MovingConfig() { }

			public MovingConfig(string xml)
				: base(xml)
			{

			}

			public MovingConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement movingConfig = new XElement("MovingConfig");
				movingConfig.SetAttributeValue("RadiusInFeet", RadiusInFeet);

				return movingConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				MovingConfig move = latest as MovingConfig;
				this.RadiusInFeet = move.RadiusInFeet;
			}

			protected override void Parse(XElement element)
			{
				int? radiusInFeet = element.GetIntAttribute("RadiusInFeet");
				if (radiusInFeet.HasValue)
					RadiusInFeet = radiusInFeet.Value;
			}
		}

		[DataContract]
		public class StoppedConfig : DeviceConfigBase
		{
			[DataMember]
			public double ThresholdMPH;
			[DataMember]
			public TimeSpan Duration;
			[DataMember]
			public bool IsEnabled;

			public StoppedConfig() { }

			public StoppedConfig(string xml)
				: base(xml)
			{

			}

			public StoppedConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement stoppedConfig = new XElement("StoppedConfig");
				stoppedConfig.SetAttributeValue("ThresholdsMPH", ThresholdMPH);
				stoppedConfig.SetAttributeValue("Duration", Duration.ToString());
				stoppedConfig.SetAttributeValue("IsEnabled", IsEnabled);

				return stoppedConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				StoppedConfig stopped = latest as StoppedConfig;
				this.Duration = stopped.Duration;
				this.IsEnabled = stopped.IsEnabled;
				this.ThresholdMPH = stopped.ThresholdMPH;
			}

			protected override void Parse(XElement element)
			{
				double? threshold = element.GetDoubleAttribute("ThresholdsMPH");
				if (threshold.HasValue)
					ThresholdMPH = threshold.Value;

				TimeSpan? duration = element.GetTimeSpanAttribute("Duration");
				if (duration.HasValue)
					Duration = duration.Value;

				bool? isEnabled = element.GetBooleanAttribute("IsEnabled");
				if (isEnabled.HasValue)
					IsEnabled = isEnabled.Value;
			}
		}

		[DataContract]
		public class MaintenanceModeConfig : DeviceConfigBase
		{
			[DataMember]
			public bool IsEnabled;
			[DataMember]
			public TimeSpan Duration;

			public MaintenanceModeConfig() { }

			public MaintenanceModeConfig(string xml)
				: base(xml)
			{

			}

			public MaintenanceModeConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement maintenanceModeConfig = new XElement("MaintenanceModeConfig");
				maintenanceModeConfig.SetAttributeValue("Duration", Duration.ToString());
				maintenanceModeConfig.SetAttributeValue("IsEnabled", IsEnabled);

				return maintenanceModeConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				MaintenanceModeConfig maintMode = latest as MaintenanceModeConfig;
				this.Duration = maintMode.Duration;
				this.IsEnabled = maintMode.IsEnabled;
			}

			protected override void Parse(XElement element)
			{
				TimeSpan? duration = element.GetTimeSpanAttribute("Duration");
				if (duration.HasValue)
					Duration = duration.Value;

				bool? isEnabled = element.GetBooleanAttribute("IsEnabled");
				if (isEnabled.HasValue)
					IsEnabled = isEnabled.Value;
			}
		}

		[DataContract]
		public class TMSConfig : DeviceConfigBase
		{
			[DataMember]
			public bool IsEnabled;

			public TMSConfig() { }

			public TMSConfig(string xml)
				: base(xml)
			{

			}

			public TMSConfig(XElement xml)
				: base(xml)
			{

			}

			protected override XElement ConfigToXElement()
			{
				XElement tmsConfig = new XElement("TMSConfig");
				tmsConfig.SetAttributeValue("IsEnabled", IsEnabled);

				return tmsConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				TMSConfig tms = latest as TMSConfig;
				this.IsEnabled = tms.IsEnabled;
			}

			protected override void Parse(XElement element)
			{
				bool? isEnabled = element.GetBooleanAttribute("IsEnabled");
				if (isEnabled.HasValue)
					IsEnabled = isEnabled.Value;
			}
		}

		[DataContract]
		public class DiscreteInputConfig : DeviceConfigBase
		{
			[DataMember]
			public bool IO1Enabled;
			[DataMember]
			public bool IO2Enabled;
			[DataMember]
			public bool IO3Enabled;
			[DataMember]
			public bool IO1PolarityIsHigh;
			[DataMember]
			public bool IO2PolarityIsHigh;
			[DataMember]
			public bool IO3PolarityIsHigh;
			[DataMember]
			public double IO1HysteresisHalfSeconds;
			[DataMember]
			public double IO2HysteresisHalfSeconds;
			[DataMember]
			public double IO3HysteresisHalfSeconds;
			[DataMember]
			public bool IO1IgnRequired;
			[DataMember]
			public bool IO2IgnRequired;
			[DataMember]
			public bool IO3IgnRequired;

			public DiscreteInputConfig() { }

			public DiscreteInputConfig(string xml)
				: base(xml)
			{

			}

			public DiscreteInputConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement maintenanceModeConfig = new XElement("DiscreteInputConfig");
				maintenanceModeConfig.SetAttributeValue("IO1Enabled", IO1Enabled);
				maintenanceModeConfig.SetAttributeValue("IO2Enabled", IO2Enabled);
				maintenanceModeConfig.SetAttributeValue("IO3Enabled", IO3Enabled);
				maintenanceModeConfig.SetAttributeValue("IO1PolarityIsHigh", IO1PolarityIsHigh);
				maintenanceModeConfig.SetAttributeValue("IO2PolarityIsHigh", IO2PolarityIsHigh);
				maintenanceModeConfig.SetAttributeValue("IO3PolarityIsHigh", IO3PolarityIsHigh);
				maintenanceModeConfig.SetAttributeValue("IO1HysteresisHalfSeconds", IO1HysteresisHalfSeconds);
				maintenanceModeConfig.SetAttributeValue("IO2HysteresisHalfSeconds", IO2HysteresisHalfSeconds);
				maintenanceModeConfig.SetAttributeValue("IO3HysteresisHalfSeconds", IO3HysteresisHalfSeconds);
				maintenanceModeConfig.SetAttributeValue("IO1IgnRequired", IO1IgnRequired);
				maintenanceModeConfig.SetAttributeValue("IO2IgnRequired", IO2IgnRequired);
				maintenanceModeConfig.SetAttributeValue("IO3IgnRequired", IO3IgnRequired);

				return maintenanceModeConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				DiscreteInputConfig io = latest as DiscreteInputConfig;
				this.IO1Enabled = io.IO1Enabled;
				this.IO1HysteresisHalfSeconds = io.IO1HysteresisHalfSeconds;
				this.IO1IgnRequired = io.IO1IgnRequired;
				this.IO1PolarityIsHigh = io.IO1PolarityIsHigh;
				this.IO2Enabled = io.IO2Enabled;
				this.IO2HysteresisHalfSeconds = io.IO2HysteresisHalfSeconds;
				this.IO2IgnRequired = io.IO2IgnRequired;
				this.IO2PolarityIsHigh = io.IO2PolarityIsHigh;
				this.IO3Enabled = io.IO3Enabled;
				this.IO3HysteresisHalfSeconds = io.IO3HysteresisHalfSeconds;
				this.IO3IgnRequired = io.IO3IgnRequired;
				this.IO3PolarityIsHigh = io.IO3PolarityIsHigh;
			}

			protected override void Parse(XElement element)
			{
				bool? io1Enabled = element.GetBooleanAttribute("IO1Enabled");
				if (io1Enabled.HasValue)
					IO1Enabled = io1Enabled.Value;

				bool? io2Enabled = element.GetBooleanAttribute("IO2Enabled");
				if (io2Enabled.HasValue)
					IO2Enabled = io2Enabled.Value;

				bool? io3Enabled = element.GetBooleanAttribute("IO3Enabled");
				if (io3Enabled.HasValue)
					IO3Enabled = io3Enabled.Value;

				bool? io1PolarityIsHigh = element.GetBooleanAttribute("IO1PolarityIsHigh");
				if (io1PolarityIsHigh.HasValue)
					IO1PolarityIsHigh = io1PolarityIsHigh.Value;

				bool? io2PolarityIsHigh = element.GetBooleanAttribute("IO2PolarityIsHigh");
				if (io2PolarityIsHigh.HasValue)
					IO2PolarityIsHigh = io2PolarityIsHigh.Value;

				bool? io3PolarityIsHigh = element.GetBooleanAttribute("IO3PolarityIsHigh");
				if (io3PolarityIsHigh.HasValue)
					IO3PolarityIsHigh = io3PolarityIsHigh.Value;

				double? io1Hysteresis = element.GetDoubleAttribute("IO1HysteresisHalfSeconds");
				if (io1Hysteresis.HasValue)
					IO1HysteresisHalfSeconds = io1Hysteresis.Value;

				double? io2Hysteresis = element.GetDoubleAttribute("IO2HysteresisHalfSeconds");
				if (io2Hysteresis.HasValue)
					IO2HysteresisHalfSeconds = io2Hysteresis.Value;

				double? io3Hysteresis = element.GetDoubleAttribute("IO3HysteresisHalfSeconds");
				if (io3Hysteresis.HasValue)
					IO3HysteresisHalfSeconds = io3Hysteresis.Value;

				bool? ign1Req = element.GetBooleanAttribute("IO1IgnRequired");
				if (ign1Req.HasValue)
					IO1IgnRequired = ign1Req.Value;

				bool? ign2Req = element.GetBooleanAttribute("IO2IgnRequired");
				if (ign2Req.HasValue)
					IO2IgnRequired = ign2Req.Value;

				bool? ign3Req = element.GetBooleanAttribute("IO3IgnRequired");
				if (ign3Req.HasValue)
					IO3IgnRequired = ign3Req.Value;
			}
		}

		[DataContract]
		public class DigitalSwitchConfig : DeviceConfigBase
		{
			[DataMember]
			public FieldID Field;
			[DataMember]
			public InputConfig? Config;
			[DataMember]
			public TimeSpan? DelayTime;
			[DataMember]
			public string Description;
			[DataMember]
			public DigitalInputMonitoringConditions? MonitoringCondition;

			public DigitalSwitchConfig() { }

			public DigitalSwitchConfig(string xml)
				: base(xml)
			{

			}

			public DigitalSwitchConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement discreteInput = new XElement("DiscreteSwitchConfig");
				discreteInput.SetAttributeValue("Field", (int)Field);
				if (Config.HasValue)
					discreteInput.SetAttributeValue("Config", (int)Config.Value);
				if (DelayTime.HasValue)
					discreteInput.SetAttributeValue("DelayTime", DelayTime.ToString());
				if (!string.IsNullOrEmpty(Description))
					discreteInput.SetAttributeValue("Description", Description);
				if (MonitoringCondition.HasValue)
					discreteInput.SetAttributeValue("MonitoringCondition", (int)MonitoringCondition.Value);

				return discreteInput;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				DigitalSwitchConfig digSwitch = latest as DigitalSwitchConfig;
				Field = digSwitch.Field;
				if (digSwitch.Config.HasValue)
					this.Config = digSwitch.Config.Value;
				if (digSwitch.DelayTime.HasValue)
					this.DelayTime = digSwitch.DelayTime.Value;
				if (digSwitch.Description != null)
					this.Description = digSwitch.Description;
				if (digSwitch.MonitoringCondition.HasValue)
					this.MonitoringCondition = digSwitch.MonitoringCondition.Value;
			}

			protected override void Parse(XElement data)
			{
				XElement element;
				if (data.HasElements)
					element = data.Elements("DiscreteSwitchConfig").FirstOrDefault();
				else
					element = data;

				DateTime? sent = element.GetDateTimeAttribute("SentUTC");
				if (sent.HasValue)
					SentUTC = sent.Value.ToUniversalTime();

				int? status = element.GetIntAttribute("Status");
				if (status.HasValue)
					Status = (MessageStatusEnum)status.Value;

				long? messageSource = element.GetLongAttribute("MessageSourceID");
				if (messageSource.HasValue)
					MessageSourceID = messageSource.Value;

				int? field = element.GetIntAttribute("Field");
				if (field.HasValue)
					Field = (FieldID)field.Value;

				int? config = element.GetIntAttribute("Config");
				if (config.HasValue)
					Config = (InputConfig)config.Value;

				TimeSpan? delayTime = element.GetTimeSpanAttribute("DelayTime");
				if (delayTime.HasValue)
					DelayTime = delayTime.Value;

				string description = element.GetStringAttribute("Description");
				if (!string.IsNullOrEmpty(description))
					Description = description;

				int? monitoringCondition = element.GetIntAttribute("MonitoringCondition");
				if (monitoringCondition.HasValue)
					MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition.Value;
			}
		}

		[DataContract]
		public class TamperSecurityAdministrationInformationConfig : DeviceConfigBase
		{
			[DataMember]
			public FieldID? machineStartStatusField;

			[DataMember]
			public FieldID? tamperResistanceStatusField;

			[DataMember]
			public MachineStartStatus? machineStartStatus;

			[DataMember]
			public TamperResistanceStatus? tamperResistanceStatus;

			[DataMember]
			public MachineStartModeConfigurationSource? machineStartModeConfigurationSource;

			[DataMember]
			public TamperResistanceModeConfigurationSource? tamperResistanceModeConfigurationSource;

			[DataMember]
			public MachineSecurityMode? machineSecurityMode;

			[DataMember]
			public MachineStartStatusTrigger? machineStartStatusTrigger;

			[DataMember]
			public int? packetID;

			[DataMember]
			public long? userID;

			[DataMember]
			public DateTime? machineStartStatusSentUTC;

			[DataMember]
			public DateTime? tamperResistanceStatusSentUTC;

			public TamperSecurityAdministrationInformationConfig() { }

			public TamperSecurityAdministrationInformationConfig(string xml)
				: base(xml)
			{

			}

			public TamperSecurityAdministrationInformationConfig(XElement xml)
				: base(xml)
			{

			}

			protected override XElement ConfigToXElement()
			{
				XElement runtime = new XElement("MachineSecuritySystemConfig");

				if (machineStartStatus.HasValue)
				{
					runtime.SetAttributeValue("MachineStartStatusField", (int)this.machineStartStatusField);
					runtime.SetAttributeValue("MachineStartStatus", (int)this.machineStartStatus.Value);
				}

				if (machineStartStatusSentUTC.HasValue)
					runtime.SetAttributeValue("MachineStartStatusSentUTC", (DateTime)this.machineStartStatusSentUTC);

				if (tamperResistanceStatus.HasValue)
				{
					runtime.SetAttributeValue("TamperResistanceStatusField", (int)this.tamperResistanceStatusField);
					runtime.SetAttributeValue("TamperResistanceStatus", (int)this.tamperResistanceStatus.Value);
				}

				if (tamperResistanceStatusSentUTC.HasValue)
					runtime.SetAttributeValue("TamperResistanceStatusSentUTC", (DateTime)this.tamperResistanceStatusSentUTC);

				if (machineStartModeConfigurationSource.HasValue)
					runtime.SetAttributeValue("MachineStartModeConfigurationSource", (int)this.machineStartModeConfigurationSource.Value);

				if (tamperResistanceModeConfigurationSource.HasValue)
					runtime.SetAttributeValue("TamperResistanceModeConfigurationSource", (int)this.tamperResistanceModeConfigurationSource.Value);

				if (machineSecurityMode.HasValue)
					runtime.SetAttributeValue("MachineSecurityMode", (int)this.machineSecurityMode.Value);

				if (machineStartStatusTrigger.HasValue)
					runtime.SetAttributeValue("MachineStartStatusTrigger", (int)this.machineStartStatusTrigger.Value);

				if (packetID.HasValue)
					runtime.SetAttributeValue("PacketID", (int)this.packetID.Value);

				return runtime;
			}

			protected override void Parse(XElement element)
			{
				int? _machineStartStatusField = element.GetIntAttribute("MachineStartStatusField");
				if (_machineStartStatusField.HasValue)
					this.machineStartStatusField = (FieldID)_machineStartStatusField.Value;

				int? _tamperResistanceStatusField = element.GetIntAttribute("TamperResistanceStatusField");
				if (_tamperResistanceStatusField.HasValue)
					this.tamperResistanceStatusField = (FieldID)_tamperResistanceStatusField.Value;

				int? _machineStartStatus = element.GetIntAttribute("MachineStartStatus");
				if (_machineStartStatus.HasValue)
					this.machineStartStatus = (MachineStartStatus)_machineStartStatus.Value;

				int? _tamperResistanceStatus = element.GetIntAttribute("TamperResistanceStatus");
				if (_tamperResistanceStatus.HasValue)
					this.tamperResistanceStatus = (TamperResistanceStatus)_tamperResistanceStatus.Value;

				int? _machineStartModeConfigurationSource = element.GetIntAttribute("MachineStartModeConfigurationSource");
				if (_machineStartModeConfigurationSource.HasValue)
					this.machineStartModeConfigurationSource = (MachineStartModeConfigurationSource)_machineStartModeConfigurationSource;

				int? _tamperResistanceModeConfigurationSource = element.GetIntAttribute("TamperResistanceModeConfigurationSource");
				if (_tamperResistanceModeConfigurationSource.HasValue)
					this.tamperResistanceModeConfigurationSource = (TamperResistanceModeConfigurationSource)_tamperResistanceModeConfigurationSource.Value;

				int? _machineSecurityMode = element.GetIntAttribute("MachineSecurityMode");
				if (_machineSecurityMode.HasValue)
					this.machineSecurityMode = (MachineSecurityMode)_machineSecurityMode;

				int? _machineStartStatusTrigger = element.GetIntAttribute("MachineStartStatusTrigger");
				if (_machineStartStatusTrigger.HasValue)
					this.machineStartStatusTrigger = (MachineStartStatusTrigger)_machineStartStatusTrigger;

				DateTime? machineStartStatusSent = element.GetDateTimeAttribute("MachineStartStatusSentUTC");
				if (machineStartStatusSent.HasValue)
					machineStartStatusSentUTC = machineStartStatusSent.Value.ToUniversalTime();

				DateTime? TamperResistanceSent = element.GetDateTimeAttribute("TamperResistanceStatusSentUTC");
				if (TamperResistanceSent.HasValue)
					tamperResistanceStatusSentUTC = TamperResistanceSent.Value.ToUniversalTime();

				int? PacketID = element.GetIntAttribute("PacketID");
				if (PacketID.HasValue)
					packetID = PacketID.Value;
			}

			public bool SetTamperLevel(TamperSecurityAdministrationInformationConfig latest)
			{
				bool isTamperResistanceStatusChanged = false;
				if (latest.tamperResistanceStatus != null && latest.tamperResistanceStatus != TamperResistanceStatus.NoPending)
				{
					this.tamperResistanceStatus = latest.tamperResistanceStatus;
					this.tamperResistanceStatusField = latest.tamperResistanceStatusField;
					this.tamperResistanceModeConfigurationSource = latest.tamperResistanceModeConfigurationSource;
					this.tamperResistanceStatusSentUTC = latest.SentUTC;
					isTamperResistanceStatusChanged = true;
				}
				return isTamperResistanceStatusChanged;
			}

			public bool SetStartMode(TamperSecurityAdministrationInformationConfig latest)
			{
				bool isMachineStartStausChanged = false;
				if (latest.machineStartStatus != null && latest.machineStartStatus != MachineStartStatus.NoPending)
				{
					this.machineStartStatus = latest.machineStartStatus;
					this.machineStartStatusField = latest.machineStartStatusField;
					this.machineStartModeConfigurationSource = latest.machineStartModeConfigurationSource;
					this.machineStartStatusSentUTC = latest.SentUTC;
					this.packetID = latest.packetID;
					isMachineStartStausChanged = true;
				}

				return isMachineStartStausChanged;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				// RCE 12265 : Implementation has been moved to Update()
			}
		}

		[DataContract]
		public class DeviceMachineSecurityReportingStatusMessageConfig : DeviceConfigBase
		{
			[DataMember]
			public MachineStartStatus? latestMachineSecurityModeconfiguration;

			[DataMember]
			public MachineStartStatus? currentMachineSecurityModeconfiguration;

			[DataMember]
			public TamperResistanceStatus? tamperResistanceStatus;

			[DataMember]
			public DeviceSecurityModeReceivingStatus? deviceSecurityModeReceivingStatus;

			[DataMember]
			public SourceSecurityModeConfiguration? sourceSecurityModeConfiguration;

			[DataMember]
			public int? packetID;

			[DataMember]
			public long? userID;

			[DataMember]
			public DateTime? machineStartStatusSentUTC;

			public DeviceMachineSecurityReportingStatusMessageConfig() { }

			public DeviceMachineSecurityReportingStatusMessageConfig(string xml)
				: base(xml)
			{

			}

			public DeviceMachineSecurityReportingStatusMessageConfig(XElement xml)
				: base(xml)
			{

			}

			protected override XElement ConfigToXElement()
			{
				XElement runtime = new XElement("DeviceMachineSecurityConfig");

				if (currentMachineSecurityModeconfiguration.HasValue)
					runtime.SetAttributeValue("CurrentMachineSecurityMode", (int)this.currentMachineSecurityModeconfiguration.Value);

				if (latestMachineSecurityModeconfiguration.HasValue)
					runtime.SetAttributeValue("LatestMachineSecurityMode", (int)this.latestMachineSecurityModeconfiguration.Value);

				if (machineStartStatusSentUTC.HasValue)
					runtime.SetAttributeValue("MachineStartStatusSentUTC", (DateTime)this.machineStartStatusSentUTC);

				if (tamperResistanceStatus.HasValue)
					runtime.SetAttributeValue("TamperResistanceStatus", (int)this.tamperResistanceStatus.Value);

				if (deviceSecurityModeReceivingStatus.HasValue)
					runtime.SetAttributeValue("DeviceSecurityModeReceivingStatus", (int)this.deviceSecurityModeReceivingStatus.Value);

				if (sourceSecurityModeConfiguration.HasValue)
					runtime.SetAttributeValue("SourceSecurityMode", (int)this.sourceSecurityModeConfiguration.Value);

				if (packetID.HasValue)
					runtime.SetAttributeValue("PacketID", (int)this.packetID.Value);

				if (userID.HasValue)
					runtime.SetAttributeValue("UserID", (int)this.userID.Value);

				return runtime;
			}

			protected override void Parse(XElement element)
			{
				int? _currentMachineSecurityMode = element.GetIntAttribute("CurrentMachineSecurityMode");
				if (_currentMachineSecurityMode.HasValue)
					this.currentMachineSecurityModeconfiguration = (MachineStartStatus)_currentMachineSecurityMode.Value;

				int? _latestMachineSecurityMode = element.GetIntAttribute("LatestMachineSecurityMode");
				if (_latestMachineSecurityMode.HasValue)
					this.latestMachineSecurityModeconfiguration = (MachineStartStatus)_latestMachineSecurityMode.Value;

				DateTime? machineStartStatusSent = element.GetDateTimeAttribute("MachineStartStatusSentUTC");
				if (machineStartStatusSent.HasValue)
					machineStartStatusSentUTC = machineStartStatusSent.Value.ToUniversalTime();

				int? _tamperResistanceStatus = element.GetIntAttribute("TamperResistanceStatus");
				if (_tamperResistanceStatus.HasValue)
					this.tamperResistanceStatus = (TamperResistanceStatus)_tamperResistanceStatus.Value;

				int? _deviceSecurityModeReceivingStatus = element.GetIntAttribute("DeviceSecurityModeReceivingStatus");
				if (_deviceSecurityModeReceivingStatus.HasValue)
					this.deviceSecurityModeReceivingStatus = (DeviceSecurityModeReceivingStatus)_deviceSecurityModeReceivingStatus.Value;

				int? _sourceSecurityMode = element.GetIntAttribute("SourceSecurityMode");
				if (_sourceSecurityMode.HasValue)
					this.sourceSecurityModeConfiguration = (SourceSecurityModeConfiguration)_sourceSecurityMode.Value;

				int? PacketID = element.GetIntAttribute("PacketID");
				if (PacketID.HasValue)
					packetID = PacketID.Value;

				int? UserID = element.GetIntAttribute("UserID");
				if (UserID.HasValue)
					userID = UserID.Value;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				DeviceMachineSecurityReportingStatusMessageConfig statusMessage = latest as DeviceMachineSecurityReportingStatusMessageConfig;
				this.latestMachineSecurityModeconfiguration = statusMessage.latestMachineSecurityModeconfiguration;
				this.currentMachineSecurityModeconfiguration = statusMessage.currentMachineSecurityModeconfiguration;
				this.tamperResistanceStatus = statusMessage.tamperResistanceStatus;
				this.deviceSecurityModeReceivingStatus = statusMessage.deviceSecurityModeReceivingStatus;
				this.sourceSecurityModeConfiguration = statusMessage.sourceSecurityModeConfiguration;
				this.packetID = statusMessage.packetID;
				this.userID = statusMessage.userID;
			}
		}

		[DataContract]
		public class SMHSourceConfig : DeviceConfigBase
		{
			[DataMember]
			public int PrimaryDataSource = 1;


			public SMHSourceConfig() { }

			public SMHSourceConfig(string xml)
				: base(xml)
			{

			}

			public SMHSourceConfig(XElement xml)
				: base(xml)
			{

			}
			protected override XElement ConfigToXElement()
			{
				XElement smhSourceConfig = new XElement("SMHSourceConfig");
				smhSourceConfig.SetAttributeValue("PrimaryDataSource", PrimaryDataSource);
				return smhSourceConfig;
			}

			protected override void SetCurrentConfig(DeviceConfigBase latest)
			{
				SMHSourceConfig smhSourceConfig = latest as SMHSourceConfig;
				this.PrimaryDataSource = smhSourceConfig.PrimaryDataSource;
			}

			protected override void Parse(XElement element)
			{
				int? smhDataSource = element.GetIntAttribute("PrimaryDataSource");
				if (smhDataSource.HasValue)
					this.PrimaryDataSource = smhDataSource.Value;
				else
				{
					// for support existing Device Configuration
					bool? useVehicleOdometer = element.GetBooleanAttribute("UseVehicleOdometer");
					if (useVehicleOdometer.HasValue)
						this.PrimaryDataSource = Convert.ToInt32(useVehicleOdometer.Value);
				}
			}

		}
		#endregion
	}
}
