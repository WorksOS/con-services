using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Runtime.Serialization;
using log4net;

namespace VSS.Hosted.VLCommon
{
  public class A5N2ConfigData : DeviceConfigData
  {
    private AssetSecurityEventStatus startModeEventFlag = AssetSecurityEventStatus.Unknown;
    private AssetSecurityEventStatus tamperResistanceEventFlag = AssetSecurityEventStatus.Unknown;
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    public A5N2ConfigData() { }


    public A5N2ConfigData(string xml)
      : base(xml)
    {

    }
    public A5N2ConfigData(XElement xml)
      : base(xml)
    {

    }

    public bool IsAuditRequired { get; set; }

    #region General DeviceConfigs

    public MileageRuntimeConfig CurrentRTMileage = null;
    public MileageRuntimeConfig LastSentRTMileage = null;

    public RuntimeAdjConfig CurrentRuntimeAdj = null;
    public RuntimeAdjConfig LastSentRuntimeAdj = null;

    public DailyReportConfig CurrentDailyReport = null;
    public DailyReportConfig LastSentDailyReport = null;

    public SpeedingConfig CurrentSpeeding = null;
    public SpeedingConfig LastSentSpeeding = null;

    public MovingConfig CurrentMoving = null;
    public MovingConfig LastSentMoving = null;

    public StoppedConfig CurrentStopped = null;
    public StoppedConfig LastSentStopped = null;

    public TMSConfig CurrentTmsMode = null;
    public TMSConfig LastSentTmsMode = null;

    public AssetSecurityStartStatus CurrentAssetSecurityConfig = null;
    public AssetSecurityStartStatus LastSentAssetSecurityConfig = null;

    public AssetSecurityTamperLevel CurrentAssetSecurityTamperLevel { get; set; }
    public AssetSecurityTamperLevel LastSentAssetSecurityTamperLevel { get; set; }

    public AssetSecurityPendingStartStatus PendingAssetSecurityConfig = null;
    #endregion

    #region SMHDataSource Config
    public SMHSourceConfig CurrentSMHSource = null;
    public SMHSourceConfig LastSentSMHSource = null;
    #endregion

    #region MaintenanceMode Configs

    public MaintenanceModeConfig CurrentMaintMode = null;
    public MaintenanceModeConfig LastSentMaintMode = null;

    #endregion

    #region DigitalInput Configs

    public List<DigitalSwitchConfig> CurrentDigitalSwitches = null;


    #endregion

    #region Daily Report Frequency configs

    public DailyReportFrequencyConfig CurrentReportFrequencyConfig = null;
    public DailyReportFrequencyConfig LastReportFrequencyConfig = null;

    #endregion

    #region Reporting Frequency Configs
    public ReportingFrequencyConfig CurrentReportingFrequencyConfig = null;
    public ReportingFrequencyConfig LastReportingFrequencyConfig = null;
    #endregion
    /// <summary>
    /// Updates this object with the supplied config data.
    /// 
    /// We're assuming we only have the current config; the following is the original comments describing pending
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
        LastSentRTMileage = config as MileageRuntimeConfig;
      }
      else if (config is RuntimeAdjConfig)
      {
        LastSentRuntimeAdj = config as RuntimeAdjConfig;
      }
      else if (config is DailyReportConfig)
      {
        LastSentDailyReport = config as DailyReportConfig;
      }
      else if (config is SpeedingConfig)
      {
        LastSentSpeeding = config as SpeedingConfig;
      }
      else if (config is MovingConfig)
      {
        LastSentMoving = config as MovingConfig;
      }
      else if (config is StoppedConfig)
      {
        LastSentStopped = config as StoppedConfig;
      }
      else if (config is MaintenanceModeConfig)
      {
        LastSentMaintMode = config as MaintenanceModeConfig;
      }
      else if (config is TMSConfig)
      {
        CurrentTmsMode = config as TMSConfig;
      }
      else if (config is AssetSecurityStartStatus)
      {
        if (config.Status == MessageStatusEnum.Acknowledged)
        {

          if (CurrentAssetSecurityConfig == null || CurrentAssetSecurityConfig.MachineStartStatusSentUTC == null ||
              (config.SentUTC != null && config.SentUTC > CurrentAssetSecurityConfig.MachineStartStatusSentUTC))
          {
            LastSentAssetSecurityConfig = null;
            CurrentAssetSecurityConfig = config as AssetSecurityStartStatus;
            IsAuditRequired = true;
            startModeEventFlag = AssetSecurityEventStatus.StartMode;
          }
        }
        else
        {
          IsAuditRequired = true;
          LastSentAssetSecurityConfig = config as AssetSecurityStartStatus;
        }
      }
      else if (config is AssetSecurityTamperLevel)
      {
        if (config.Status == MessageStatusEnum.Acknowledged)
        {
          if (CurrentAssetSecurityTamperLevel == null || CurrentAssetSecurityTamperLevel.TamperLevelSentUtc == null ||
             (config.SentUTC != null && config.SentUTC > CurrentAssetSecurityTamperLevel.TamperLevelSentUtc))
          {
            LastSentAssetSecurityTamperLevel = null;
            CurrentAssetSecurityTamperLevel = config as AssetSecurityTamperLevel;
            IsAuditRequired = true;
            tamperResistanceEventFlag = AssetSecurityEventStatus.TamperLevel;
          }
        }
        else
        {
          IsAuditRequired = true;
          LastSentAssetSecurityTamperLevel = config as AssetSecurityTamperLevel;
        }
      }
      else if (config is AssetSecurityStatus)
      {
        
        var newConfig = config as AssetSecurityStatus;
        if (config.Status == MessageStatusEnum.Acknowledged)
        {
          if (CurrentAssetSecurityConfig == null || CurrentAssetSecurityConfig.MachineStartStatusSentUTC == null ||
            (config.SentUTC != null && config.SentUTC > CurrentAssetSecurityConfig.MachineStartStatusSentUTC))
          {
            LastSentAssetSecurityConfig = null;
            CurrentAssetSecurityConfig = newConfig.AssetSecurityStartStatus;
            IsAuditRequired = true;
            startModeEventFlag = AssetSecurityEventStatus.StartMode;
          }
          if (CurrentAssetSecurityTamperLevel == null || CurrentAssetSecurityTamperLevel.TamperLevelSentUtc == null ||
            (config.SentUTC != null && config.SentUTC > CurrentAssetSecurityTamperLevel.TamperLevelSentUtc))
          {
            LastSentAssetSecurityTamperLevel = null;
            CurrentAssetSecurityTamperLevel = newConfig.AssetSecurityTamperLevel;
            IsAuditRequired = true;
            tamperResistanceEventFlag = AssetSecurityEventStatus.TamperLevel;
          }

        }
        else
        {
          IsAuditRequired = true;
          LastSentAssetSecurityConfig = newConfig.AssetSecurityStartStatus;
          LastSentAssetSecurityTamperLevel = newConfig.AssetSecurityTamperLevel;
        }
      }
      else if (config is AssetSecurityPendingStartStatus)
      {
        var newConfig = config as AssetSecurityPendingStartStatus;

        if (LastSentAssetSecurityConfig == null || LastSentAssetSecurityConfig.MachineStartStatusSentUTC == null ||
            (config.SentUTC != null && config.SentUTC > LastSentAssetSecurityConfig.MachineStartStatusSentUTC))
        {
          PendingAssetSecurityConfig = newConfig;
          IsAuditRequired = true;
          startModeEventFlag = AssetSecurityEventStatus.StartModeConfigured;
        }
      }
      else if (config is AssetSecurityPendingStatus)
      {
        var newConfig = config as AssetSecurityPendingStatus;
        if (LastSentAssetSecurityConfig == null || LastSentAssetSecurityConfig.MachineStartStatusSentUTC == null ||
            (config.SentUTC != null && config.SentUTC > LastSentAssetSecurityConfig.MachineStartStatusSentUTC))
        {
          PendingAssetSecurityConfig = newConfig.AssetSecurityPendingStartStatus;
          IsAuditRequired = true;
          startModeEventFlag = AssetSecurityEventStatus.StartModeConfigured;
        }

        if (CurrentAssetSecurityTamperLevel == null || CurrentAssetSecurityTamperLevel.TamperLevelSentUtc == null ||
            (config.SentUTC != null && config.SentUTC > CurrentAssetSecurityTamperLevel.TamperLevelSentUtc))
        {
          LastSentAssetSecurityTamperLevel = null;
          CurrentAssetSecurityTamperLevel = newConfig.AssetSecurityTamperLevel;
          IsAuditRequired = true;
          tamperResistanceEventFlag = AssetSecurityEventStatus.TamperLevel;
        }
      }

      #region CurrentDigitalSwitches

      if (config is DigitalSwitchConfig)
      {
        DigitalSwitchConfig switchConfig = config as DigitalSwitchConfig;

        CurrentDigitalSwitches[CurrentDigitalSwitches.FindIndex(x => x.SwitchNumber == switchConfig.SwitchNumber)] = switchConfig;
      }

      else if (config is DailyReportFrequencyConfig)
      {
        if (config.Status == MessageStatusEnum.Acknowledged)
        {
          LastReportFrequencyConfig = null;
          CurrentReportFrequencyConfig = config as DailyReportFrequencyConfig;
        }
        else
        {
          LastReportFrequencyConfig = config as DailyReportFrequencyConfig;
        }
      }
      #endregion
      #region ReportingConfiguration
      else if (config is ReportingFrequencyConfig)
      {
        if (config.Status == MessageStatusEnum.Acknowledged)
        {
          LastReportingFrequencyConfig = null;
          CurrentReportingFrequencyConfig = config as ReportingFrequencyConfig;
        }
        else
        {
          LastReportingFrequencyConfig = config as ReportingFrequencyConfig;
        }
      }
      #endregion ReportingConfiguration

    }

    public override bool AuditConfigChanges(INH_OP ctx, Asset asset, DeviceConfigBase config)
    {
      if (!IsAuditRequired)
      {
        //TODO add more logging info
        log.Info("Ignoring the old payload");
        return false;
      }
        

      var assetSecurityIncident = new AssetSecurityIncident();
      if (asset != null)
      {
        assetSecurityIncident.SerialNumberVIN = asset.SerialNumberVIN;
        assetSecurityIncident.fk_MakeCode = asset.fk_MakeCode;
        assetSecurityIncident.fk_DeviceTypeID = asset.Device.fk_DeviceTypeID;
      }
      
      assetSecurityIncident.EventType = ((int)config.Status).ToString(CultureInfo.InvariantCulture);
      assetSecurityIncident.TimeStampUTC = config.SentUTC;

      if (CurrentAssetSecurityTamperLevel != null)
      {
        assetSecurityIncident.CurrentTamperLevel = (int?)CurrentAssetSecurityTamperLevel.TamperLevel;
        if (startModeEventFlag == AssetSecurityEventStatus.Unknown && tamperResistanceEventFlag==AssetSecurityEventStatus.TamperLevel)
        {
          assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.TamperLevel).ToString(CultureInfo.InvariantCulture);
        }
      }
      if (CurrentAssetSecurityConfig != null)
      {
        assetSecurityIncident.CurrentStartMode = (int?)CurrentAssetSecurityConfig.MachineStartStatus;
        assetSecurityIncident.StartModeTrigger = (int?)CurrentAssetSecurityConfig.MachineStartStatusTrigger;

        if (tamperResistanceEventFlag == AssetSecurityEventStatus.Unknown && startModeEventFlag == AssetSecurityEventStatus.StartMode)
        {
          assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartMode).ToString(CultureInfo.InvariantCulture);
        }
      }
      //when both start mode and tamperlevel are received
      if (startModeEventFlag == AssetSecurityEventStatus.StartMode && tamperResistanceEventFlag == AssetSecurityEventStatus.TamperLevel)
      {
        assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartModeTamperLevel).ToString(CultureInfo.InvariantCulture);
      }

      if (startModeEventFlag == AssetSecurityEventStatus.StartModeConfigured && tamperResistanceEventFlag == AssetSecurityEventStatus.TamperLevel)
      {
        assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.TamperAppliedStartModeConfigured).ToString(CultureInfo.InvariantCulture);
      }

      if (startModeEventFlag == AssetSecurityEventStatus.StartModeConfigured && tamperResistanceEventFlag == AssetSecurityEventStatus.Unknown)
      {
        assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartModeConfigured).ToString(CultureInfo.InvariantCulture);
      }

      //Clearing the flags
      startModeEventFlag = AssetSecurityEventStatus.Unknown;
      tamperResistanceEventFlag = AssetSecurityEventStatus.Unknown;

      if (LastSentAssetSecurityTamperLevel != null)
      {
        if (config is AssetSecurityTamperLevel)
        {
          assetSecurityIncident.fk_UserID = LastSentAssetSecurityTamperLevel.UserID;
          assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.TamperLevelPending).ToString(CultureInfo.InvariantCulture);
        }
        assetSecurityIncident.TargetTamperLevel = (int?)LastSentAssetSecurityTamperLevel.TamperLevel;
      }
      if (LastSentAssetSecurityConfig != null)
      {
        if (config is AssetSecurityStartStatus)
        {
          assetSecurityIncident.fk_UserID = LastSentAssetSecurityConfig.UserID;
          //Adding new AssetSecurity Event for startmode
          assetSecurityIncident.EventType = ((int)AssetSecurityEventStatus.StartModePending).ToString(CultureInfo.InvariantCulture);
        }
        assetSecurityIncident.TargetStartMode = (int?)LastSentAssetSecurityConfig.MachineStartStatus;
      }
      if (PendingAssetSecurityConfig != null)
      {
        assetSecurityIncident.TargetStartMode = (int?)PendingAssetSecurityConfig.MachineStartStatus;
        
      }
      if (config is AssetSecurityStartStatus || config is AssetSecurityTamperLevel || config is AssetSecurityStatus || config is AssetSecurityPendingStartStatus || config is AssetSecurityPendingStatus)
      {
        ctx.AssetSecurityIncident.AddObject(assetSecurityIncident);
	      return true;
      }
	    return false;
    }

    //Not implemented intentionally because A5N2 doesn't yet support Pending state
    protected override void GetPending(XElement pending)
    {
      //throw new NotImplementedException();
    }

    public override void UpdateCurrentStatus(INH_OP ctx, Asset asset, DeviceConfigBase config)
    {
      if ((config is AssetSecurityStartStatus || config is AssetSecurityTamperLevel || config is AssetSecurityStatus || config is AssetSecurityPendingStartStatus || config is AssetSecurityPendingStatus) && asset != null)
      {
        StoredProcDefinition updateProceDefinition = new StoredProcDefinition("NH_OP", "uspPub_AssetCurrentSecurityStatus");
        updateProceDefinition.AddInput("@SerialNumbeVIN", asset.SerialNumberVIN);
        SqlAccessMethods.ExecuteNonQuery(updateProceDefinition);
      }
    }

    public override XElement ToXElement()
    {
      XElement element = new XElement("A5N2ConfigData");

      XElement lastSent = new XElement("LastSent");
      if (null != LastSentRTMileage) lastSent.Add(LastSentRTMileage.ToXElement());
      if (null != LastSentRuntimeAdj) lastSent.Add(LastSentRuntimeAdj.ToXElement());
      if (null != LastSentDailyReport) lastSent.Add(LastSentDailyReport.ToXElement());
      if (null != LastSentSpeeding) lastSent.Add(LastSentSpeeding.ToXElement());
      if (null != LastSentMoving) lastSent.Add(LastSentMoving.ToXElement());
      if (null != LastSentStopped) lastSent.Add(LastSentStopped.ToXElement());
      if (null != LastSentMaintMode) lastSent.Add(LastSentMaintMode.ToXElement());
      if (null != LastSentSMHSource) lastSent.Add(LastSentSMHSource.ToXElement());
      if (null != LastSentTmsMode) lastSent.Add(LastSentTmsMode.ToXElement());
      if (null != LastSentAssetSecurityConfig) lastSent.Add(LastSentAssetSecurityConfig.ToXElement());
      if (null != LastSentAssetSecurityTamperLevel) lastSent.Add(LastSentAssetSecurityTamperLevel.ToXElement());
      if (null != PendingAssetSecurityConfig)
      {
        if (null != LastSentAssetSecurityConfig)
        {
          var machineSecurityStartStatus = lastSent.Elements("MachineSecurityStartStatus").FirstOrDefault();
          if (machineSecurityStartStatus != null && PendingAssetSecurityConfig.MachineStartStatus.HasValue)
          {
            machineSecurityStartStatus.SetAttributeValue("MachineStartStatus", (int)PendingAssetSecurityConfig.MachineStartStatus);
            if (PendingAssetSecurityConfig.MachineStartModeConfigSource.HasValue)
              machineSecurityStartStatus.SetAttributeValue("MachineStartModeConfigSource", (int)PendingAssetSecurityConfig.MachineStartModeConfigSource);

            if (PendingAssetSecurityConfig.MachineStartStatusSentUTC.HasValue)
              machineSecurityStartStatus.SetAttributeValue("MachineStartStatusSentUTC", (DateTime)PendingAssetSecurityConfig.MachineStartStatusSentUTC);
            machineSecurityStartStatus.SetAttributeValue("Status", (int)PendingAssetSecurityConfig.Status);
          }
        }
        else
          lastSent.Add(PendingAssetSecurityConfig.ToXElement());
      }

      if (null != LastReportFrequencyConfig) lastSent.Add(LastReportFrequencyConfig.ToXElement());
      if (null != LastReportingFrequencyConfig) lastSent.Add(LastReportingFrequencyConfig.ToXElement());

      element.Add(lastSent);

      XElement current = new XElement("Current");
      if (null != CurrentRTMileage) current.Add(CurrentRTMileage.ToXElement());
      if (null != CurrentRuntimeAdj) current.Add(CurrentRuntimeAdj.ToXElement());
      if (null != CurrentTmsMode) current.Add(CurrentTmsMode.ToXElement());
      if (null != CurrentAssetSecurityConfig) current.Add(CurrentAssetSecurityConfig.ToXElement());
      if (null != CurrentAssetSecurityTamperLevel) current.Add(CurrentAssetSecurityTamperLevel.ToXElement());
      if (null != CurrentReportFrequencyConfig) current.Add(CurrentReportFrequencyConfig.ToXElement());
      if (null != CurrentReportingFrequencyConfig) current.Add(CurrentReportingFrequencyConfig.ToXElement());

      #region CurrentDigitalSwitches

      if (CurrentDigitalSwitches != null)
      {
        var switchSection = new XElement("SwitchesConfig");

        foreach (DigitalSwitchConfig sw in CurrentDigitalSwitches)
          switchSection.Add(new XElement(sw.ToXElement()));

        current.Add(switchSection);
      }

      #endregion

      element.Add(current);
      return element;
    }

    #region Implementation

    protected override void GetCurrent(XElement current)
    {
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

      XElement smhSource = current.Elements("SMHSourceConfig").FirstOrDefault();
      if (smhSource != null)
        CurrentSMHSource = new SMHSourceConfig(smhSource);

      XElement tms = current.Elements("TMSConfig").FirstOrDefault();
      if (tms != null)
        CurrentTmsMode = new TMSConfig(tms);

      #region CurrentDigitalSwitches

      CurrentDigitalSwitches = new List<DigitalSwitchConfig>();

      XElement switchesElement = current.Elements("SwitchesConfig").FirstOrDefault();
      if (switchesElement != null)
      {
        foreach (XElement e in from r in switchesElement.Descendants("DiscreteSwitchDetail") select r)
        {
          CurrentDigitalSwitches.Add(new DigitalSwitchConfig(e));
        }
      }
      #endregion

      XElement assetSecurityStartStatus = current.Elements("MachineSecurityStartStatus").FirstOrDefault();
      if (assetSecurityStartStatus != null)
        CurrentAssetSecurityConfig = new AssetSecurityStartStatus(assetSecurityStartStatus);

      XElement assetSecurityStatus = current.Elements("TamperResistanceMode").FirstOrDefault();
      if (assetSecurityStatus != null)
        CurrentAssetSecurityTamperLevel = new AssetSecurityTamperLevel(assetSecurityStatus);

      XElement reportFrequency = current.Elements("DailyReportFrequencyConfig").FirstOrDefault();
      if (reportFrequency != null)
        CurrentReportFrequencyConfig = new DailyReportFrequencyConfig(reportFrequency);

      XElement reportingFrequency = current.Elements("ReportingFrequencyConfig").FirstOrDefault();
      if (reportingFrequency != null)
        CurrentReportingFrequencyConfig = new ReportingFrequencyConfig(reportingFrequency);
    }

    protected override void GetLastSent(XElement lastSent)
    {
      XElement rtMileage = lastSent.Elements("MileageRuntimeConfig").FirstOrDefault();
      if (rtMileage != null)
        LastSentRTMileage = new MileageRuntimeConfig(rtMileage);

      XElement runtime = lastSent.Elements("RuntimeAdjConfig").FirstOrDefault();
      if (runtime != null)
        LastSentRuntimeAdj = new RuntimeAdjConfig(runtime);

      XElement daily = lastSent.Elements("DailyReportConfig").FirstOrDefault();
      if (daily != null)
        LastSentDailyReport = new DailyReportConfig(daily);

      XElement speeding = lastSent.Elements("SpeedingConfig").FirstOrDefault();
      if (speeding != null)
        LastSentSpeeding = new SpeedingConfig(speeding);

      XElement moving = lastSent.Elements("MovingConfig").FirstOrDefault();
      if (moving != null)
        LastSentMoving = new MovingConfig(moving);

      XElement stopped = lastSent.Elements("StoppedConfig").FirstOrDefault();
      if (stopped != null)
        LastSentStopped = new StoppedConfig(stopped);

      XElement maintMode = lastSent.Elements("MaintenanceModeConfig").FirstOrDefault();
      if (maintMode != null)
        LastSentMaintMode = new MaintenanceModeConfig(maintMode);

      XElement smhSource = lastSent.Elements("SMHSourceConfig").FirstOrDefault();
      if (smhSource != null)
        LastSentSMHSource = new SMHSourceConfig(smhSource);

      XElement assetSecurityStatus = lastSent.Elements("MachineSecurityStartStatus").FirstOrDefault();
      if (assetSecurityStatus != null)
        LastSentAssetSecurityConfig = new AssetSecurityStartStatus(assetSecurityStatus);

      XElement assetSecurityTamperLevel = lastSent.Elements("TamperResistanceMode").FirstOrDefault();
      if (assetSecurityTamperLevel != null)
        LastSentAssetSecurityTamperLevel = new AssetSecurityTamperLevel(assetSecurityTamperLevel);

      XElement reportFrequency = lastSent.Elements("DailyReportFrequencyConfig").FirstOrDefault();
      if (reportFrequency != null)
        LastReportFrequencyConfig = new DailyReportFrequencyConfig(reportFrequency);
      
      XElement reportingFrequency = lastSent.Elements("ReportingFrequencyConfig").FirstOrDefault();
      if (reportingFrequency != null)
        LastReportingFrequencyConfig = new ReportingFrequencyConfig(reportingFrequency);
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
      public double RadiusInFeet;
      [DataMember]
      public double ThresholdMPH;
      [DataMember]
      public TimeSpan Duration;

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
        movingConfig.SetAttributeValue("ThresholdsMPH", ThresholdMPH);
        movingConfig.SetAttributeValue("Duration", Duration.ToString());

        return movingConfig;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {
        MovingConfig move = latest as MovingConfig;
        this.RadiusInFeet = move.RadiusInFeet;
      }

      protected override void Parse(XElement element)
      {
        double? radiusInFeet = element.GetDoubleAttribute("RadiusInFeet");
        RadiusInFeet = (radiusInFeet.HasValue) ? radiusInFeet.Value : -1;

        double? threshold = element.GetDoubleAttribute("ThresholdsMPH");
        ThresholdMPH = (threshold.HasValue) ? threshold.Value : -1;

        TimeSpan? duration = element.GetTimeSpanAttribute("Duration");
        Duration = (duration.HasValue) ? duration.Value : TimeSpan.FromSeconds(-1);
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


    #region CurrentDigitalSwitches

    [DataContract]
    public class DigitalSwitchConfig : DeviceConfigBase
    {
      [DataMember]
      public int SwitchNumber;
      [DataMember]
      public bool Enabled;
      [DataMember]
      public TimeSpan? DelayTime;
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
        XElement discreteInput = new XElement("DiscreteSwitchDetail");
        discreteInput.SetAttributeValue("SwitchNumber", (int)SwitchNumber);
        discreteInput.SetAttributeValue("Enabled", (bool)Enabled);
        if (DelayTime.HasValue)
          discreteInput.SetAttributeValue("DelayTime", DelayTime.ToString());
        if (MonitoringCondition.HasValue)
          discreteInput.SetAttributeValue("MonitoringCondition", (int)MonitoringCondition.Value);

        return discreteInput;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {
        DigitalSwitchConfig digSwitch = latest as DigitalSwitchConfig;
        SwitchNumber = digSwitch.SwitchNumber;
        Enabled = digSwitch.Enabled;
        if (digSwitch.DelayTime.HasValue)
          this.DelayTime = digSwitch.DelayTime.Value;
        if (digSwitch.MonitoringCondition.HasValue)
          this.MonitoringCondition = digSwitch.MonitoringCondition.Value;
      }

      protected override void Parse(XElement data)
      {
        XElement element;
        if (data.HasElements)
          element = data.Elements("DiscreteSwitchDetail").FirstOrDefault();
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

        int? SN = element.GetIntAttribute("SwitchNumber");
        if (SN.HasValue)
          SwitchNumber = (int)SN.Value;

        bool? EN = element.GetBooleanAttribute("Enabled");
        if (EN.HasValue)
          Enabled = EN.Value;

        TimeSpan? delayTime = element.GetTimeSpanAttribute("DelayTime");
        if (delayTime.HasValue)
          DelayTime = delayTime.Value;

        int? monitoringCondition = element.GetIntAttribute("MonitoringCondition");
        if (monitoringCondition.HasValue)
          MonitoringCondition = (DigitalInputMonitoringConditions)monitoringCondition.Value;
      }
    }

    #endregion

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

    #endregion

    #region "AssetSecurityStatus"

    [DataContract]
    public class AssetSecurityStartStatus : DeviceConfigBase
    {
      [DataMember]
      public MachineStartStatus? MachineStartStatus;

      [DataMember]
      public MachineStartStatusTrigger? MachineStartStatusTrigger;

      [DataMember]
      public DateTime? MachineStartStatusSentUTC;

      [DataMember]
      public long? UserID;

      public AssetSecurityStartStatus() { }

      public AssetSecurityStartStatus(string xml)
        : base(xml)
      {

      }

      public AssetSecurityStartStatus(XElement xml)
        : base(xml)
      {

      }

      protected override XElement ConfigToXElement()
      {
        XElement assetSecurityStatus = new XElement("MachineSecurityStartStatus");

        if (MachineStartStatus.HasValue)
          assetSecurityStatus.SetAttributeValue("MachineStartStatus", (int)MachineStartStatus.Value);

        if (MachineStartStatusTrigger.HasValue)
          assetSecurityStatus.SetAttributeValue("MachineStartStatusTrigger", (int)MachineStartStatusTrigger.Value);

        if (MachineStartStatusSentUTC.HasValue)
          assetSecurityStatus.SetAttributeValue("MachineStartStatusSentUTC", (DateTime)MachineStartStatusSentUTC);

        assetSecurityStatus.SetAttributeValue("SourceMessageId", MessageSourceID);

        return assetSecurityStatus;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {
        AssetSecurityStartStatus assetSecurityStatus = latest as AssetSecurityStartStatus;

        if (assetSecurityStatus != null && (assetSecurityStatus.MachineStartStatus != null && assetSecurityStatus.MachineStartStatusTrigger != null))
        {
          MachineStartStatus = assetSecurityStatus.MachineStartStatus;
          MachineStartStatusTrigger = assetSecurityStatus.MachineStartStatusTrigger;
          MachineStartStatusSentUTC = assetSecurityStatus.MachineStartStatusSentUTC;
        }
      }

      protected override void Parse(XElement element)
      {
        var machineStartStatus = element.GetIntAttribute("MachineStartStatus");
        if (machineStartStatus.HasValue)
          MachineStartStatus = (MachineStartStatus)machineStartStatus.Value;

        var machineStartStatusTrigger = element.GetIntAttribute("MachineStartStatusTrigger");
        if (machineStartStatusTrigger.HasValue)
          MachineStartStatusTrigger = (MachineStartStatusTrigger)machineStartStatusTrigger.Value;

        var machineStartStatusSent = element.GetDateTimeAttribute("MachineStartStatusSentUTC");
        if (machineStartStatusSent.HasValue)
          MachineStartStatusSentUTC = machineStartStatusSent.Value;

      }
    }

    #endregion

    #region AssetTamperResistanceLevel
    [DataContract]
    public class AssetSecurityTamperLevel : DeviceConfigBase
    {
      [DataMember]
      public TamperResistanceStatus? TamperLevel;

      [DataMember]
      public TamperResistanceModeConfigurationSource? TamperConfigurationSource;

      [DataMember]
      public DateTime? TamperLevelSentUtc;

      [DataMember]
      public long? UserID;

      public AssetSecurityTamperLevel() { }

      public AssetSecurityTamperLevel(string xml)
        : base(xml)
      {

      }

      public AssetSecurityTamperLevel(XElement xml)
        : base(xml)
      {

      }

      protected override XElement ConfigToXElement()
      {
        XElement assetSecurityStatus = new XElement("TamperResistanceMode");

        if (TamperLevel.HasValue)
          assetSecurityStatus.SetAttributeValue("TamperResistanceStatus", (int)TamperLevel.Value);

        if (TamperConfigurationSource.HasValue)
          assetSecurityStatus.SetAttributeValue("TamperResistanceModeConfigurationSource", (int)TamperConfigurationSource.Value);

        if (TamperLevelSentUtc.HasValue)
          assetSecurityStatus.SetAttributeValue("TamperResistanceStatusSentUTC", (DateTime)TamperLevelSentUtc);

        assetSecurityStatus.SetAttributeValue("SourceMessageId", MessageSourceID);

        return assetSecurityStatus;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {
        var assetTamperLevel = latest as AssetSecurityTamperLevel;

        if (assetTamperLevel != null && (assetTamperLevel.TamperLevel != null))
        {
          TamperLevel = assetTamperLevel.TamperLevel;
          TamperConfigurationSource = assetTamperLevel.TamperConfigurationSource;
          TamperLevelSentUtc = assetTamperLevel.TamperLevelSentUtc;
        }
      }

      protected override void Parse(XElement element)
      {
        var machineTamperLevel = element.GetIntAttribute("TamperResistanceStatus");
        if (machineTamperLevel.HasValue)
          TamperLevel = (TamperResistanceStatus)machineTamperLevel.Value;

        var machineTamperConfigurationSource = element.GetIntAttribute("TamperResistanceModeConfigurationSource");
        if (machineTamperConfigurationSource.HasValue)
          TamperConfigurationSource = (TamperResistanceModeConfigurationSource)machineTamperConfigurationSource.Value;

        var machineTamperLevelSent = element.GetDateTimeAttribute("TamperResistanceStatusSentUTC");
        if (machineTamperLevelSent.HasValue)
          TamperLevelSentUtc = machineTamperLevelSent.Value;

      }
    }
    #endregion

    [DataContract]
    public class AssetSecurityStatus : DeviceConfigBase
    {
      [DataMember]
      public AssetSecurityTamperLevel AssetSecurityTamperLevel { get; set; }

      [DataMember]
      public AssetSecurityStartStatus AssetSecurityStartStatus { get; set; }

      protected override void Parse(XElement element)
      {

      }

      protected override XElement ConfigToXElement()
      {
        XElement assetSecurityStatus = new XElement("AssetSecurityStatus");
        assetSecurityStatus.Add(AssetSecurityStartStatus.ToXElement());
        assetSecurityStatus.Add(AssetSecurityTamperLevel.ToXElement());
        return assetSecurityStatus;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {

      }
    }

    [DataContract]
    public class AssetSecurityPendingStartStatus : DeviceConfigBase
    {
      [DataMember]
      public MachineStartStatus? MachineStartStatus;

      [DataMember]
      public MachineStartModeConfigurationSource? MachineStartModeConfigSource;

      [DataMember]
      public DateTime? MachineStartStatusSentUTC;

      public AssetSecurityPendingStartStatus() { }

      public AssetSecurityPendingStartStatus(string xml)
        : base(xml)
      {

      }

      public AssetSecurityPendingStartStatus(XElement xml)
        : base(xml)
      {

      }

      protected override XElement ConfigToXElement()
      {
        XElement assetSecurityPendingStatus = new XElement("MachineSecurityStartStatus");

        if (MachineStartStatus.HasValue)
          assetSecurityPendingStatus.SetAttributeValue("MachineStartStatus", (int)MachineStartStatus.Value);

        if (MachineStartModeConfigSource.HasValue)
          assetSecurityPendingStatus.SetAttributeValue("MachineStartModeConfigSource", (int)MachineStartModeConfigSource.Value);

        if (MachineStartStatusSentUTC.HasValue)
          assetSecurityPendingStatus.SetAttributeValue("MachineStartStatusSentUTC", (DateTime)MachineStartStatusSentUTC);

        assetSecurityPendingStatus.SetAttributeValue("SourceMessageId", MessageSourceID);

        return assetSecurityPendingStatus;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {
        AssetSecurityPendingStartStatus assetSecurityStatus = latest as AssetSecurityPendingStartStatus;

        if (assetSecurityStatus != null && assetSecurityStatus.MachineStartStatus != null)
        {
          MachineStartStatus = assetSecurityStatus.MachineStartStatus;
          if (assetSecurityStatus.MachineStartModeConfigSource != null)
            MachineStartModeConfigSource = assetSecurityStatus.MachineStartModeConfigSource;
          MachineStartStatusSentUTC = assetSecurityStatus.MachineStartStatusSentUTC;
        }
      }

      protected override void Parse(XElement element)
      {
        var machineStartStatus = element.GetIntAttribute("MachineStartStatus");
        if (machineStartStatus.HasValue)
          MachineStartStatus = (MachineStartStatus)machineStartStatus.Value;

        var machineStartModeConfigSource = element.GetIntAttribute("MachineStartModeConfigSource");
        if (machineStartModeConfigSource.HasValue)
          MachineStartModeConfigSource = (MachineStartModeConfigurationSource)machineStartModeConfigSource.Value;

        var machineStartStatusSent = element.GetDateTimeAttribute("MachineStartStatusSentUTC");
        if (machineStartStatusSent.HasValue)
          MachineStartStatusSentUTC = machineStartStatusSent.Value;

      }
    }

    [DataContract]
    public class AssetSecurityPendingStatus : DeviceConfigBase
    {
      [DataMember]
      public AssetSecurityTamperLevel AssetSecurityTamperLevel { get; set; }

      [DataMember]
      public AssetSecurityPendingStartStatus AssetSecurityPendingStartStatus { get; set; }

      protected override void Parse(XElement element)
      {

      }

      protected override XElement ConfigToXElement()
      {
        XElement assetSecurityPendingStatus = new XElement("AssetSecurityPendingStatus");
        assetSecurityPendingStatus.Add(AssetSecurityPendingStartStatus.ToXElement());
        assetSecurityPendingStatus.Add(AssetSecurityTamperLevel.ToXElement());
        return assetSecurityPendingStatus;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latest)
      {

      }
    }

    [DataContract]
    public class DailyReportFrequencyConfig : DeviceConfigBase
    {
      [DataMember]
      public int ReportFrequency { get; set; }

      public DailyReportFrequencyConfig() { }

      public DailyReportFrequencyConfig(string xml)
        : base(xml)
      {

      }

      public DailyReportFrequencyConfig(XElement xml)
        : base(xml)
      {

      }
      protected override XElement ConfigToXElement()
      {
        XElement dailyReportFrequencyConfig = new XElement("DailyReportFrequencyConfig");
        dailyReportFrequencyConfig.SetAttributeValue("ReportFrequency", ReportFrequency);

        return dailyReportFrequencyConfig;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latestConfig)
      {
        DailyReportFrequencyConfig daily = latestConfig as DailyReportFrequencyConfig;
        if (daily != null)
          ReportFrequency = daily.ReportFrequency;
      }

      protected override void Parse(XElement element)
      {
        int? frequency = element.GetIntAttribute("ReportFrequency");

        if (frequency.HasValue)
          ReportFrequency = frequency.Value;
      }
    }

    [DataContract]
    public class ReportingFrequencyConfig : DeviceConfigBase
    {
      [DataMember]
      public int Interval { get; set; }

      [DataMember]
      public int Frequency { get; set; }

      public ReportingFrequencyConfig() { }

      public ReportingFrequencyConfig(string xml)
        : base(xml)
      {

      }

      public ReportingFrequencyConfig(XElement xml)
        : base(xml)
      {

      }
      protected override XElement ConfigToXElement()
      {
        XElement reportFrequencyConfig = new XElement("ReportingFrequencyConfig");
        reportFrequencyConfig.SetAttributeValue("Interval", (ReportingFrequencyInterval)Interval);
        reportFrequencyConfig.SetAttributeValue("Frequency", Frequency);
        return reportFrequencyConfig;
      }

      protected override void SetCurrentConfig(DeviceConfigBase latestConfig)
      {
        ReportingFrequencyConfig reportingFrequency = latestConfig as ReportingFrequencyConfig;
        if (reportingFrequency != null)
        {
          Interval = reportingFrequency.Interval;
          Frequency = reportingFrequency.Frequency;
        }
      }

      protected override void Parse(XElement element)
      {
        string intervalValue = element.GetStringAttribute("Interval");
        ReportingFrequencyInterval interval=ReportingFrequencyInterval.Day;

        Interval = !string.IsNullOrEmpty(intervalValue) && Enum.TryParse<ReportingFrequencyInterval>(intervalValue, out interval) ? (int)interval : 0;

        int? frequencyElement = element.GetIntAttribute("Frequency");
        Frequency = frequencyElement.HasValue ? frequencyElement.Value : 0;
      }
    }
  }
}
