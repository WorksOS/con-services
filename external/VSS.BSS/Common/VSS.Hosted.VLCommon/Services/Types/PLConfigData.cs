using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
  public class PLConfigData
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public GeneralRegistry CurrentGeneralRegistry;
    public TransmissionRegistry CurrentTransmissionRegistry;
    public DigitalRegistry CurrentDigitalRegistry;
    public GeneralRegistry PendingGeneralRegistry;
    public TransmissionRegistry PendingTransmissionRegistry;
    public DigitalRegistry PendingDigitalRegistry;
    
    public PLConfigData() { }
    public PLConfigData(string xml)
    {
      if (string.IsNullOrEmpty(xml))
        return;

      var element = XElement.Parse(xml);
      Parse(element);
    }

    public PLConfigData(XElement element)
    {
      if (null == element)
        return;
      Parse(element);
    }

    public XElement ToXElement()
    {
      var plConfigData = new XElement("PLConfigData");
      var current = new XElement("Current");
      var pending = new XElement("Pending");

      if (PendingGeneralRegistry != null) pending.Add(PendingGeneralRegistry.ToXElement());
      if (PendingTransmissionRegistry != null) pending.Add(PendingTransmissionRegistry.ToXElement());
      if (PendingDigitalRegistry != null) pending.Add(PendingDigitalRegistry.ToXElement());

      plConfigData.Add(pending);

      if (CurrentGeneralRegistry != null) current.Add(CurrentGeneralRegistry.ToXElement());
      if (CurrentTransmissionRegistry != null) current.Add(CurrentTransmissionRegistry.ToXElement());
      if (CurrentDigitalRegistry != null) current.Add(CurrentDigitalRegistry.ToXElement());

      plConfigData.Add(current);

      return plConfigData;
    }

    public void Update(PLConfigBase newConfig, MessageStatusEnum status)
    {
      var config = newConfig as GeneralRegistry;
      if (config != null)
      {
        if(CurrentGeneralRegistry == null && status == MessageStatusEnum.Acknowledged)
          CurrentGeneralRegistry = new GeneralRegistry();
        if (PendingGeneralRegistry == null && status != MessageStatusEnum.Acknowledged)
          PendingGeneralRegistry = new GeneralRegistry();
        
        Update(ref CurrentGeneralRegistry, ref PendingGeneralRegistry, config, status);
      }
      else
      {
        var latest = newConfig as TransmissionRegistry;
        if (latest != null)
        {
          if (CurrentTransmissionRegistry == null && status == MessageStatusEnum.Acknowledged)
            CurrentTransmissionRegistry = new TransmissionRegistry();
          if (PendingTransmissionRegistry == null && status != MessageStatusEnum.Acknowledged)
            PendingTransmissionRegistry = new TransmissionRegistry();
        
          Update(ref CurrentTransmissionRegistry, ref PendingTransmissionRegistry, latest, status);
        }
        else
        {
          var registry = newConfig as DigitalRegistry;
          if (registry != null)
          {
            if (CurrentDigitalRegistry == null && status == MessageStatusEnum.Acknowledged)
              CurrentDigitalRegistry = new DigitalRegistry();
            if(PendingDigitalRegistry == null && status != MessageStatusEnum.Acknowledged)
              PendingDigitalRegistry = new DigitalRegistry();
        
            Update(ref CurrentDigitalRegistry, ref PendingDigitalRegistry, registry, status);
          }
        }
      }
    }

    private static void Update<T>(ref T existingCurrent, ref T existingPending,
      T latest, MessageStatusEnum status) where T : PLConfigBase 
    {
      if (status == MessageStatusEnum.Acknowledged)
      {
        existingCurrent.SetCurrent(latest);

        if (existingPending != null)
        {
          var fullyReconciled = existingPending.ReconcilePending(latest);
          if (fullyReconciled)
          {
            existingPending = null;
          }
        }
      }
      else
      {
        existingPending.SetCurrent(latest);
      }
    }
    
    private void Parse(XElement element)
    {
      var pending = element.Elements("Pending").FirstOrDefault();
      var current = element.Elements("Current").FirstOrDefault();

      if (current == null && pending == null)
        current = element;

      if(pending != null)
        GetPending(pending);
      if(current != null)
        GetCurrent(current);
    }

    private void GetCurrent(XElement current)
    {
      var general = current.Elements("GeneralRegistry").FirstOrDefault();
      if (general != null)
        CurrentGeneralRegistry = new GeneralRegistry(general);

      var transmission = current.Elements("TransmissionRegistry").FirstOrDefault();
      if (transmission != null)
        CurrentTransmissionRegistry = new TransmissionRegistry(transmission);

      var digital = current.Elements("DigitalRegistry").FirstOrDefault();
      if (digital != null)
        CurrentDigitalRegistry = new DigitalRegistry(digital);
    }

    private void GetPending(XElement pending)
    {
      var general = pending.Elements("GeneralRegistry").FirstOrDefault();
      if (general != null)
        PendingGeneralRegistry = new GeneralRegistry(general);

      var transmission = pending.Elements("TransmissionRegistry").FirstOrDefault();
      if (transmission != null)
        PendingTransmissionRegistry = new TransmissionRegistry(transmission);

      var digital = pending.Elements("DigitalRegistry").FirstOrDefault();
      if (digital != null)
        PendingDigitalRegistry = new DigitalRegistry(digital);
    }
    
    public int? OldestPendingKeyDate
    {
      get
      {
        if (PendingGeneralRegistry == null && PendingDigitalRegistry == null && PendingTransmissionRegistry == null)
          return null;
        
        var utcNowKeyDate = DateTime.UtcNow.KeyDate();
        var oldestKeyDate = utcNowKeyDate;
        var hasPending = false;

        if (PendingGeneralRegistry != null)
        {
          if (PendingGeneralRegistry.GlobalGramSentUTC.HasValue &&
              PendingGeneralRegistry.GlobalGramSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingGeneralRegistry.GlobalGramSentUTC.Value.KeyDate();
            hasPending = true;
          }

          if (PendingGeneralRegistry.RuntimeHoursSentUTC.HasValue &&
              PendingGeneralRegistry.RuntimeHoursSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingGeneralRegistry.RuntimeHoursSentUTC.Value.KeyDate();
            hasPending = true;
          }

          if (PendingGeneralRegistry.StartStopEnableSentUTC.HasValue &&
              PendingGeneralRegistry.StartStopEnableSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingGeneralRegistry.StartStopEnableSentUTC.Value.KeyDate();
            hasPending = true;
          }

          if (PendingGeneralRegistry.ReportSchedule != null)
          {
            if (PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.HasValue
              && PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.Value.KeyDate() <= oldestKeyDate)
            {
              oldestKeyDate = PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.Value.KeyDate();
              hasPending = true;
            }

            if (PendingGeneralRegistry.ReportSchedule.Reports != null)
            {
              foreach (var report in PendingGeneralRegistry.ReportSchedule.Reports)
              {
                if (report != null && report.SentUTC.HasValue && report.SentUTC.KeyDate() <= oldestKeyDate)
                {
                  oldestKeyDate = report.SentUTC.Value.KeyDate();
                  hasPending = true;
                }
              }
            }
          }
        }

        if (PendingDigitalRegistry != null)
        {
          if (PendingDigitalRegistry.Sensors != null)
          {
            foreach (var sensor in PendingDigitalRegistry.Sensors)
            {
              if (sensor != null)
              {
                if (sensor.DelayTimeSentUTC.HasValue && sensor.DelayTimeSentUTC.KeyDate() <= oldestKeyDate)
                {
                  oldestKeyDate = sensor.DelayTimeSentUTC.KeyDate();
                  hasPending = true;
                }

                if (sensor.DescriptionSentUTC.HasValue && sensor.DescriptionSentUTC.KeyDate() <= oldestKeyDate)
                {
                  oldestKeyDate = sensor.DescriptionSentUTC.KeyDate();
                  hasPending = true;
                }

                if (sensor.MonitorConditionSentUTC.HasValue && sensor.MonitorConditionSentUTC.KeyDate() <= oldestKeyDate)
                {
                  oldestKeyDate = sensor.MonitorConditionSentUTC.KeyDate();
                  hasPending = true;
                }

                if (sensor.SensorConfigSentUTC.HasValue && sensor.SensorConfigSentUTC.KeyDate() <= oldestKeyDate)
                {
                  oldestKeyDate = sensor.SensorConfigSentUTC.KeyDate();
                  hasPending = true;
                }
              }
            }
          }
        }

        if (PendingTransmissionRegistry != null)
        {
          if (PendingTransmissionRegistry.EventIntervalHoursSentUTC.HasValue &&
              PendingTransmissionRegistry.EventIntervalHoursSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingTransmissionRegistry.EventIntervalHoursSentUTC.Value.KeyDate();
            hasPending = true;
          }

          if (PendingTransmissionRegistry.NextMessageIntervalSentUTC.HasValue &&
              PendingTransmissionRegistry.NextMessageIntervalSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingTransmissionRegistry.NextMessageIntervalSentUTC.Value.KeyDate();
            hasPending = true;
          }

          if (PendingTransmissionRegistry.SMUFuelSentUTC.HasValue &&
              PendingTransmissionRegistry.SMUFuelSentUTC.Value.KeyDate() <= oldestKeyDate)
          {
            oldestKeyDate = PendingTransmissionRegistry.SMUFuelSentUTC.Value.KeyDate();
            hasPending = true;
          }
          
          if (PendingTransmissionRegistry.EventReporting != null)
          {
            if (PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.HasValue &&
                PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.Value.KeyDate() <= oldestKeyDate)
            {
              oldestKeyDate = PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.Value.KeyDate();
              hasPending = true;
            }

            if (PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.HasValue &&
                PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.Value.KeyDate() <= oldestKeyDate)
            {
              oldestKeyDate = PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.Value.KeyDate();
              hasPending = true;
            }

            if (PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.HasValue &&
                PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.Value.KeyDate() <= oldestKeyDate)
            {
              oldestKeyDate = PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.Value.KeyDate();
              hasPending = true;
            }

            if (PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.HasValue &&
                PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.Value.KeyDate() <= oldestKeyDate)
            {
              oldestKeyDate = PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.Value.KeyDate();
              hasPending = true;
            }
          }
        }

        if (hasPending == false)
        {
          return null;
        }

        return oldestKeyDate;
      }
    }

    public void CleanupStalePendingRegistryEntries(int cleanupOlderThanKeyDate)
    {
      if (PendingGeneralRegistry != null)
      {
        if (PendingGeneralRegistry.GlobalGramSentUTC.HasValue &&
            PendingGeneralRegistry.GlobalGramSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingGeneralRegistry.GlobalGramEnable = null;
          PendingGeneralRegistry.GlobalGramSentUTC = null;
        }

        if (PendingGeneralRegistry.RuntimeHoursSentUTC.HasValue &&
            PendingGeneralRegistry.RuntimeHoursSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingGeneralRegistry.RunTimeHoursAdj = null;
          PendingGeneralRegistry.RuntimeHoursSentUTC = null;
        }

        if (PendingGeneralRegistry.StartStopEnableSentUTC.HasValue &&
            PendingGeneralRegistry.StartStopEnableSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingGeneralRegistry.StartStopEnable = null;
          PendingGeneralRegistry.StartStopEnableSentUTC = null;
        }

        if (PendingGeneralRegistry.ReportSchedule != null)
        {
          if (PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.HasValue &&
              PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingGeneralRegistry.ReportSchedule.ReportStartTime = null;
            PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC = null;
          }

          if (PendingGeneralRegistry.ReportSchedule.Reports != null)
          {
            var staleReports 
              = PendingGeneralRegistry.ReportSchedule.Reports.Where(r => r.SentUTC.KeyDate() < cleanupOlderThanKeyDate).ToArray();
            
            foreach (var staleReport in staleReports)
            {
              PendingGeneralRegistry.ReportSchedule.Reports.Remove(staleReport);
            }
          }

          if (!PendingGeneralRegistry.ReportSchedule.ReportStartTimeSentUTC.HasValue
            && (PendingGeneralRegistry.ReportSchedule.Reports == null || PendingGeneralRegistry.ReportSchedule.Reports.Count == 0))
          {
            PendingGeneralRegistry.ReportSchedule = null;
          }
        }

        if (!PendingGeneralRegistry.GlobalGramSentUTC.HasValue
            && !PendingGeneralRegistry.RuntimeHoursSentUTC.HasValue
            && !PendingGeneralRegistry.StartStopEnableSentUTC.HasValue
            && PendingGeneralRegistry.ReportSchedule == null)
        {
          PendingGeneralRegistry = null;
        }
      }

      if ((PendingDigitalRegistry != null) && (PendingDigitalRegistry.Sensors != null))
      {
        for (var sensorCount = 0; sensorCount < PendingDigitalRegistry.Sensors.Count; sensorCount++)
        {
          if (PendingDigitalRegistry.Sensors[sensorCount].DelayTimeSentUTC.HasValue && PendingDigitalRegistry.Sensors[sensorCount].DelayTimeSentUTC.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingDigitalRegistry.Sensors[sensorCount].DelayTime = null;
            PendingDigitalRegistry.Sensors[sensorCount].DelayTimeSentUTC = null;
          }

          if (PendingDigitalRegistry.Sensors[sensorCount].DescriptionSentUTC.HasValue && PendingDigitalRegistry.Sensors[sensorCount].DescriptionSentUTC.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingDigitalRegistry.Sensors[sensorCount].Description = null;
            PendingDigitalRegistry.Sensors[sensorCount].DescriptionSentUTC = null;
          }

          if (PendingDigitalRegistry.Sensors[sensorCount].MonitorConditionSentUTC.HasValue && PendingDigitalRegistry.Sensors[sensorCount].MonitorConditionSentUTC.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingDigitalRegistry.Sensors[sensorCount].MonitorCondition = null;
            PendingDigitalRegistry.Sensors[sensorCount].MonitorConditionSentUTC = null;
          }

          if (PendingDigitalRegistry.Sensors[sensorCount].SensorConfigSentUTC.HasValue && PendingDigitalRegistry.Sensors[sensorCount].SensorConfigSentUTC.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingDigitalRegistry.Sensors[sensorCount].SensorConfiguration = null;
            PendingDigitalRegistry.Sensors[sensorCount].SensorConfigSentUTC = null;
          }

          if (!PendingDigitalRegistry.Sensors[sensorCount].DelayTimeSentUTC.HasValue
              && !PendingDigitalRegistry.Sensors[sensorCount].DescriptionSentUTC.HasValue
              && !PendingDigitalRegistry.Sensors[sensorCount].MonitorConditionSentUTC.HasValue
              && !PendingDigitalRegistry.Sensors[sensorCount].SensorConfigSentUTC.HasValue)
          {
            PendingDigitalRegistry.Sensors.Remove(PendingDigitalRegistry.Sensors[sensorCount]);
          }
        }

        if (PendingDigitalRegistry.Sensors == null || PendingDigitalRegistry.Sensors.Count == 0)
        {
          PendingDigitalRegistry = null;
        }
      }

      if (PendingTransmissionRegistry != null)
      {
        if (PendingTransmissionRegistry.EventIntervalHoursSentUTC.HasValue &&
            PendingTransmissionRegistry.EventIntervalHoursSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingTransmissionRegistry.EventIntervalHours = null;
          PendingTransmissionRegistry.EventIntervalHoursSentUTC = null;
        }

        if (PendingTransmissionRegistry.NextMessageIntervalSentUTC.HasValue &&
            PendingTransmissionRegistry.NextMessageIntervalSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingTransmissionRegistry.NextMessageInterval = null;
          PendingTransmissionRegistry.NextMessageIntervalSentUTC = null;
        }

        if (PendingTransmissionRegistry.SMUFuelSentUTC.HasValue &&
            PendingTransmissionRegistry.SMUFuelSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
        {
          PendingTransmissionRegistry.SMUFuel = null;
          PendingTransmissionRegistry.SMUFuelSentUTC = null;
        }

        if (PendingTransmissionRegistry.EventReporting != null)
        {
          if (PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.HasValue &&
              PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingTransmissionRegistry.EventReporting.DiagnosticFreqCode = null;
            PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC = null;
          }

          if (PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.HasValue &&
              PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingTransmissionRegistry.EventReporting.Level1EventFreqCode = null;
            PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC = null;
          }

          if (PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.HasValue &&
              PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingTransmissionRegistry.EventReporting.Level2EventFreqCode = null;
            PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC = null;
          }

          if (PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.HasValue &&
              PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.Value.KeyDate() < cleanupOlderThanKeyDate)
          {
            PendingTransmissionRegistry.EventReporting.Level3EventFreqCode = null;
            PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC = null;
          }

          if (!PendingTransmissionRegistry.EventReporting.DiagnosticFreqSentUTC.HasValue
            && !PendingTransmissionRegistry.EventReporting.Level1EventFreqSentUTC.HasValue
            && !PendingTransmissionRegistry.EventReporting.Level2EventFreqSentUTC.HasValue
            && !PendingTransmissionRegistry.EventReporting.Level3EventFreqSentUTC.HasValue)
          {
            PendingTransmissionRegistry.EventReporting = null;
          }
        }

        if (!PendingTransmissionRegistry.EventIntervalHoursSentUTC.HasValue
          && !PendingTransmissionRegistry.NextMessageIntervalSentUTC.HasValue
          && !PendingTransmissionRegistry.SMUFuelSentUTC.HasValue
          && PendingTransmissionRegistry.EventReporting == null)
        {
          PendingTransmissionRegistry = null;
        }
      }
    }

    [DataContract(Namespace = "http://www.nighthawk.com/nighthawk/service/NHOP/2009/10"),
      KnownType(typeof(GeneralRegistry)),
      KnownType(typeof(TransmissionRegistry)),
      KnownType(typeof(DigitalRegistry)),]
    public abstract class PLConfigBase
    {
      public abstract void SetCurrent(PLConfigBase config);
      public abstract bool ReconcilePending(PLConfigBase incomingPlConfigBase);
    }

    /// general Registry parses xml of the type below when this class does a toxelement reports and software info it
    /// uses attributes instead of elements to save on some space
    ///      <GeneralRegistry>
    ///         <lastRegistrationDate>2006-07-31 13:51:40.0</lastRegistrationDate>
    ///         <registrationStatus>Registered</registrationStatus>
    ///         <ReportingSchedule>
    ///            <reportStartTimeHHMM>1300</reportStartTimeHHMM>
    ///            <Report>
    ///               <type>Position</type>
    ///               <frequency>4</frequency>
    ///            </Report>
    ///            <Report>
    ///               <type>SMU</type>
    ///               <frequency/>
    ///            </Report>
    ///         </ReportingSchedule>
    ///         <globalGramEnabledCode>N</globalGramEnabledCode>
    ///         <moduleType>PL321SR</moduleType>
    ///         <dataLinkType>CDL</dataLinkType>
    ///         <blockDataTransfer>Y</blockDataTransfer>
    ///         <regDealerCode>TD00</regDealerCode>
    ///         <SoftwareInfo>
    ///            <hc11SoftwarePartNumber>2588729-24</hc11SoftwarePartNumber>
    ///            <modemSoftwarePartNumber>2674603-00</modemSoftwarePartNumber>
    ///            <hardwareSerialNumber>2399954-01</hardwareSerialNumber>
    ///            <softwareRevision>2.2B</softwareRevision>
    ///         </SoftwareInfo>
    ///      </GeneralRegistry>
    [DataContract]
    public class GeneralRegistry : PLConfigBase
    {
      [DataMember]
      public TimeSpan? RunTimeHoursAdj;
      [DataMember]
      public DateTime? RuntimeHoursSentUTC;
      [DataMember]
      public DateTime? LastRegistrationDate;
      [DataMember]
      public string RegistrationStatus;
      [DataMember]
      public ReportingSchedule ReportSchedule;
      [DataMember]
      public bool? GlobalGramEnable;
      [DataMember]
      public DateTime? GlobalGramSentUTC;
      [DataMember]
      public string ModuleType;
      [DataMember]
      public string DataLinkType;
      [DataMember]
      public bool? BlockDataTransfer;
      [DataMember]
      public string RegDealerCode;
      [DataMember]
      public SoftwareInfo Software;
      [DataMember]
      public bool? StartStopEnable;
      [DataMember]
      public DateTime? StartStopEnableSentUTC;

      public GeneralRegistry() { }
      public GeneralRegistry(XElement element)
      {
        Parse(element);
      }

      public override void SetCurrent(PLConfigBase config)
      {
        var registry = config as GeneralRegistry;
        if (registry == null) return;
        if (registry.RunTimeHoursAdj.HasValue)
          RunTimeHoursAdj = registry.RunTimeHoursAdj;
        if (registry.RuntimeHoursSentUTC.HasValue)
          RuntimeHoursSentUTC = registry.RuntimeHoursSentUTC;
        if (registry.LastRegistrationDate.HasValue)
          LastRegistrationDate = registry.LastRegistrationDate;
        if (!string.IsNullOrEmpty(registry.RegistrationStatus))
          RegistrationStatus = registry.RegistrationStatus;
        if (registry.ReportSchedule != null)
        {
          if (ReportSchedule == null)
            ReportSchedule = new ReportingSchedule();
          ReportSchedule.SetReportSchedule(registry.ReportSchedule);
        }
        if (registry.GlobalGramEnable.HasValue)
          GlobalGramEnable = registry.GlobalGramEnable;
        if (registry.GlobalGramSentUTC.HasValue)
          GlobalGramSentUTC = registry.GlobalGramSentUTC;
        if(!string.IsNullOrEmpty(registry.ModuleType))
          ModuleType = registry.ModuleType;
        if (!string.IsNullOrEmpty(registry.DataLinkType))
          DataLinkType = registry.DataLinkType;
        if (registry.BlockDataTransfer.HasValue)
          BlockDataTransfer = registry.BlockDataTransfer;
        if (!string.IsNullOrEmpty(registry.RegDealerCode))
          RegDealerCode = registry.RegDealerCode;
        if (registry.Software != null)
        {
          if (Software == null)
            Software = new SoftwareInfo();
          Software.SetSoftwareInfo(registry.Software);
        }
        if (registry.StartStopEnable.HasValue)
          StartStopEnable = registry.StartStopEnable;
        if (registry.StartStopEnableSentUTC.HasValue)
          StartStopEnableSentUTC = registry.StartStopEnableSentUTC;
      }
      
      public override bool ReconcilePending(PLConfigBase incomingConfigBase)
      {
        //compare incoming to self and remove any incoming from self
        var incomingGeneralRegistry = incomingConfigBase as GeneralRegistry;

        if (incomingGeneralRegistry != null)
        {
          if (incomingGeneralRegistry.BlockDataTransfer.HasValue && BlockDataTransfer.HasValue 
            && incomingGeneralRegistry.BlockDataTransfer.Value == BlockDataTransfer.Value)
          {
            BlockDataTransfer = null;
          }

          if (incomingGeneralRegistry.DataLinkType != null && incomingGeneralRegistry.DataLinkType.Equals(DataLinkType))
          {
            DataLinkType = null;
          }

          if (incomingGeneralRegistry.GlobalGramEnable.HasValue && GlobalGramEnable.HasValue
            && incomingGeneralRegistry.GlobalGramEnable.Value == GlobalGramEnable.Value)
          {
            GlobalGramEnable = null;
            GlobalGramSentUTC = null;
          }

          if (incomingGeneralRegistry.ModuleType != null && incomingGeneralRegistry.ModuleType.Equals(ModuleType))
          {
            ModuleType = null;
          }

          if (incomingGeneralRegistry.RegDealerCode != null && incomingGeneralRegistry.RegDealerCode.Equals(RegDealerCode))
          {
            RegDealerCode = null;
          }

          if (incomingGeneralRegistry.RegistrationStatus != null && incomingGeneralRegistry.RegistrationStatus.Equals(RegistrationStatus))
          {
            RegistrationStatus = null;
          }

          if (incomingGeneralRegistry.ReportSchedule != null && ReportSchedule != null)
          {
            if (incomingGeneralRegistry.ReportSchedule.ReportStartTime.HasValue && ReportSchedule.ReportStartTime.HasValue
              && incomingGeneralRegistry.ReportSchedule.ReportStartTime == ReportSchedule.ReportStartTime)
            {
              ReportSchedule.ReportStartTime = null;
              ReportSchedule.ReportStartTimeSentUTC = null;
            }

            if (ReportSchedule.Reports != null)
            {
              foreach (var reportToRemove in ReportSchedule.Reports
                .Where(
                  currentReport => incomingGeneralRegistry.ReportSchedule.Reports.Any(currentReport.ConfigEquivalent))
                .ToArray())
              {
                ReportSchedule.Reports.Remove(reportToRemove);
              }
            }
          }

          if (incomingGeneralRegistry.Software != null && Software != null)
          {
            if (incomingGeneralRegistry.Software.HC11SoftwarePartNumber.Equals(Software.HC11SoftwarePartNumber))
            {
              Software.HC11SoftwarePartNumber = null;
            }

            if (incomingGeneralRegistry.Software.HardwareSerialNumber.Equals(Software.HardwareSerialNumber))
            {
              Software.HardwareSerialNumber = null;
            }

            if (incomingGeneralRegistry.Software.ModemSoftwarePartNumber.Equals(Software.ModemSoftwarePartNumber))
            {
              Software.ModemSoftwarePartNumber = null;
            }

            if (incomingGeneralRegistry.Software.SoftwareRevision.Equals(Software.SoftwareRevision))
            {
              Software.SoftwareRevision = null;
            }
          }

          if (incomingGeneralRegistry.RunTimeHoursAdj.HasValue
            && incomingGeneralRegistry.RuntimeHoursSentUTC.HasValue 
            && RuntimeHoursSentUTC.HasValue
            && incomingGeneralRegistry.RuntimeHoursSentUTC >= RuntimeHoursSentUTC)
          {
            RunTimeHoursAdj = null;
            RuntimeHoursSentUTC = null;
          }

          if (incomingGeneralRegistry.StartStopEnable.HasValue && StartStopEnable.HasValue
            && incomingGeneralRegistry.StartStopEnable.Value == StartStopEnable.Value)
          {
            StartStopEnable = null;
            StartStopEnableSentUTC = null;
          }
        }

        var somethingDifferent = false // false here is just for readability
          || BlockDataTransfer      != null
          || DataLinkType           != null
          || GlobalGramEnable       != null
          || ModuleType             != null
          || RegDealerCode          != null
          || RegistrationStatus     != null
          || RunTimeHoursAdj        != null
          || StartStopEnable        != null;

        if (!somethingDifferent && ReportSchedule != null)
        {
          // keep looking
          somethingDifferent = somethingDifferent
            || ReportSchedule.ReportStartTime         != null;
        }

        if (!somethingDifferent && ReportSchedule.Reports != null)
        {
          // keep looking
          foreach (var report in ReportSchedule.Reports)
          {
            somethingDifferent = somethingDifferent
              || report.frequency   != null
              || report.ReportType  != null;
          }
        }

        if (!somethingDifferent && Software != null)
        {
          // keep looking
          somethingDifferent = somethingDifferent
            || Software.HC11SoftwarePartNumber  != null
            || Software.HardwareSerialNumber    != null
            || Software.ModemSoftwarePartNumber != null
            || Software.SoftwareRevision        != null;
        }

        var fullyReconciled = !somethingDifferent;
        return fullyReconciled;
      }

      public XElement ToXElement()
      {
        var element = new XElement("GeneralRegistry");
        if (RunTimeHoursAdj.HasValue)
          element.Add(new XElement("runTimeHoursAdj", RunTimeHoursAdj.ToString()));
        if (RuntimeHoursSentUTC.HasValue)
          element.Add(new XElement("RunTimeHoursSentUTC", RuntimeHoursSentUTC));
        if(LastRegistrationDate.HasValue)
          element.Add(new XElement("lastRegistrationDate", LastRegistrationDate));
        if(!string.IsNullOrEmpty(RegistrationStatus))
          element.Add(new XElement("registrationStatus", RegistrationStatus));
        if(ReportSchedule != null)
          element.Add(ReportSchedule.ToXElement());
        if(GlobalGramEnable.HasValue)
          element.Add(new XElement("globalGramEnabledCode", GlobalGramEnable.Value ? "Y" : "N"));
        if(GlobalGramSentUTC.HasValue)
          element.Add(new XElement("GlobalGramSentUTC", GlobalGramSentUTC));
        if(!string.IsNullOrEmpty(ModuleType))
          element.Add(new XElement("moduleType", ModuleType));
        if(!string.IsNullOrEmpty(DataLinkType))
          element.Add(new XElement("dataLinkType", DataLinkType));
        if(BlockDataTransfer.HasValue)
          element.Add(new XElement("blockDataTransfer", BlockDataTransfer.Value ? "Y" : "N"));
        if(!string.IsNullOrEmpty(RegDealerCode))
          element.Add(new XElement("regDealerCode", RegDealerCode));
        if(Software != null)
          element.Add(Software.ToXElement());
        if(StartStopEnable.HasValue)
          element.Add(new XElement("startStopEnableCode", StartStopEnable.Value ? "Y" : "N"));
        if (StartStopEnableSentUTC.HasValue)
          element.Add(new XElement("StartStopEnableSentUTC", StartStopEnableSentUTC));

        return element;
      }

      private void Parse(XElement element)
      {
        RunTimeHoursAdj = element.GetTimeSpanElement("runTimeHoursAdj");
        RuntimeHoursSentUTC = element.GetUTCDateTimeElement("RunTimeHoursSentUTC");
        LastRegistrationDate = element.GetUTCDateTimeElement("lastRegistrationDate");
        RegistrationStatus = element.GetStringElement("registrationStatus");
        var report = element.Elements("ReportingSchedule").FirstOrDefault();
        if (ReportSchedule == null &&  report != null)
          ReportSchedule = new ReportingSchedule(report);
        var global = element.GetStringElement("globalGramEnabledCode");
        if(!string.IsNullOrEmpty(global))
          GlobalGramEnable = global == "Y";

        GlobalGramSentUTC = element.GetUTCDateTimeElement("GlobalGramSentUTC");

        ModuleType = element.GetStringElement("moduleType");
        DataLinkType = element.GetStringElement("dataLinkType");
        var blockData = element.GetStringElement("blockDataTransfer");
        if (!string.IsNullOrEmpty(blockData))
          BlockDataTransfer = blockData == "Y";
        RegDealerCode = element.GetStringElement("regDealerCode");
        var software = element.Elements("SoftwareInfo").FirstOrDefault();
        if (software != null)
          Software = new SoftwareInfo(software);
        var startStop = element.GetStringElement("startStopEnableCode");
        if (!string.IsNullOrEmpty(startStop))
          StartStopEnable = startStop == "Y";

        StartStopEnableSentUTC = element.GetUTCDateTimeElement("StartStopEnableSentUTC");
      }

      [DataContract]
      public class ReportingSchedule
      {
        [DataMember] public TimeSpan? ReportStartTime;
        [DataMember] public DateTime? ReportStartTimeSentUTC;
        [DataMember] public List<Report> Reports;

        public ReportingSchedule()
        {
        }

        public ReportingSchedule(XElement element)
        {
          Parse(element);
        }

        [DataContract]
        public class Report
        {
          [DataMember] public string ReportType;
          [DataMember] public int? frequency;
          [DataMember] public DateTime? SentUTC;

          public Report()
          {
          }

          public Report(XElement element)
          {
            Parse(element);
          }

          public XElement ToXElement()
          {
            var reportElement = new XElement("Report");
            reportElement.SetAttributeValue("SentUTC", SentUTC);
            reportElement.SetAttributeValue("type", ReportType);
            reportElement.SetAttributeValue("frequency", frequency);
            return reportElement;
          }

          private void Parse(XElement element)
          {
            SentUTC = element.GetUTCDateTimeAttribute("SentUTC");
            ReportType = element.GetStringAttribute("type");
            if (string.IsNullOrEmpty(ReportType))
              ReportType = element.GetStringElement("type");
            frequency = element.GetIntAttribute("frequency");
            if (!frequency.HasValue)
              frequency = element.GetIntElement("frequency");
          }

          public bool ConfigEquivalent(Report other)
          {
            return ReportType == other.ReportType &&
                   frequency == other.frequency;
          }
        }

        public XElement ToXElement()
        {
          var reportScheduleElement = new XElement("ReportingSchedule");

          if (ReportStartTime.HasValue)
          {
            var reportStartTime = string.Format("{0}{1}{2}{3}", ReportStartTime.Value.Hours < 10 ? "0" : string.Empty,
              ReportStartTime.Value.Hours,
              ReportStartTime.Value.Minutes < 10 ? "0" : string.Empty, ReportStartTime.Value.Minutes);

            var reportstartTimeElement = new XElement("reportStartTimeHHMM", reportStartTime);

            reportScheduleElement.Add(reportstartTimeElement);
          }

          if (ReportStartTimeSentUTC.HasValue)
            reportScheduleElement.Add(new XElement("ReportStartTimeSentUTC", ReportStartTimeSentUTC));

          if (Reports != null && Reports.Count > 0)
          {
            foreach (var report in Reports)
            {
              reportScheduleElement.Add(report.ToXElement());
            }
          }
          return reportScheduleElement;
        }

        private void Parse(XElement element)
        {
          var reportStartTimeElement = element.Elements("reportStartTimeHHMM").FirstOrDefault();

          var reportStartTime = reportStartTimeElement == null ? null : reportStartTimeElement.Value;

          if (!string.IsNullOrEmpty(reportStartTime))
          {
            int hour;
            int minute;
            if (int.TryParse(reportStartTime.Substring(0, 2), out hour) &&
                int.TryParse(reportStartTime.Substring(2, 2), out minute))
            {
              ReportStartTime = new TimeSpan(hour, minute, 0);
            }
          }

          ReportStartTimeSentUTC = element.GetUTCDateTimeElement("ReportStartTimeSentUTC");

          var reportElements = element.Elements("Report").ToList();

          if (reportElements.Count > 0)
          {
            if (Reports == null)
              Reports = new List<Report>();
            
            foreach (var report in reportElements)
              Reports.Add(new Report(report));
          }
        }

        internal void SetReportSchedule(ReportingSchedule reportingSchedule)
        {
          if (reportingSchedule.ReportStartTime.HasValue)
            ReportStartTime = reportingSchedule.ReportStartTime;

          if (reportingSchedule.ReportStartTimeSentUTC.HasValue)
            ReportStartTimeSentUTC = reportingSchedule.ReportStartTimeSentUTC;

          if (reportingSchedule.Reports != null)
          {
            if (Reports == null)
              Reports = new List<Report>();

            foreach (var r in reportingSchedule.Reports)
            {
              var old = (from o in Reports
                where o.ReportType == r.ReportType
                select o).FirstOrDefault();

              if (old == null)
                Reports.Add(new Report
                {
                  ReportType = r.ReportType,
                  frequency = r.frequency,
                  SentUTC = r.SentUTC
                });
              else
              {
                old.frequency = r.frequency;
                old.SentUTC = r.SentUTC;
              }
            }
          }
        }
      }
      
      [DataContract]
      public class SoftwareInfo
      {
        [DataMember]
        public string HC11SoftwarePartNumber;
        [DataMember]
        public string ModemSoftwarePartNumber;
        [DataMember]
        public string HardwareSerialNumber;
        [DataMember]
        public string SoftwareRevision;

        public SoftwareInfo() { }
        public SoftwareInfo(XElement element)
        {
          Parse(element);
        }
        public XElement ToXElement()
        {
          var element = new XElement("SoftwareInfo");
          element.SetAttributeValue("hc11SoftwarePartNumber", HC11SoftwarePartNumber);
          element.SetAttributeValue("modemSoftwarePartNumber", ModemSoftwarePartNumber);
          element.SetAttributeValue("hardwareSerialNumber", HardwareSerialNumber);
          element.SetAttributeValue("softwareRevision", SoftwareRevision);

          return element;
        }

        private void Parse(XElement element)
        {
          HC11SoftwarePartNumber = element.GetStringAttribute("hc11SoftwarePartNumber");
          if(string.IsNullOrEmpty(HC11SoftwarePartNumber))
            HC11SoftwarePartNumber = element.GetStringElement("hc11SoftwarePartNumber");
          
          ModemSoftwarePartNumber = element.GetStringAttribute("modemSoftwarePartNumber");
          if (string.IsNullOrEmpty(ModemSoftwarePartNumber))
            ModemSoftwarePartNumber = element.GetStringElement("modemSoftwarePartNumber");

          HardwareSerialNumber = element.GetStringAttribute("hardwareSerialNumber");
          if (string.IsNullOrEmpty(HardwareSerialNumber))
            HardwareSerialNumber = element.GetStringElement("hardwareSerialNumber");

          SoftwareRevision = element.GetStringAttribute("softwareRevision");
          if(string.IsNullOrEmpty(SoftwareRevision))
            SoftwareRevision = element.GetStringElement("softwareRevision");
        }

        internal void SetSoftwareInfo(SoftwareInfo softwareInfo)
        {
          if (softwareInfo != null)
          {
            if (!string.IsNullOrEmpty(softwareInfo.HardwareSerialNumber))
              HardwareSerialNumber = softwareInfo.HardwareSerialNumber;
            if (!string.IsNullOrEmpty(softwareInfo.HC11SoftwarePartNumber))
              HC11SoftwarePartNumber = softwareInfo.HC11SoftwarePartNumber;
            if (!string.IsNullOrEmpty(softwareInfo.ModemSoftwarePartNumber))
              ModemSoftwarePartNumber = softwareInfo.ModemSoftwarePartNumber;
            if (!string.IsNullOrEmpty(softwareInfo.SoftwareRevision))
              SoftwareRevision = softwareInfo.SoftwareRevision;
          }
        }
      }
    }

    /// Transmission Registry parses xml of the type below when this class does a toxelement event reporting frequencey it
    /// uses attributes instead of elements to save on some space
    ///      <TransmissionRegistry>
    ///         <EventReportingFrequency>
    ///            <level1EventFreqCode>NEXT</level1EventFreqCode>
    ///            <level2EventFreqCode>NEXT</level2EventFreqCode>
    ///            <level3EventFreqCode>IMMEDIATELY</level3EventFreqCode>
    ///            <diagnosticFreqCode>NEXT</diagnosticFreqCode>
    ///         </EventReportingFrequency>
    ///         <smuFuelReporting>SMU</smuFuelReporting>
    ///         <nextMessageIntervalHours>168</nextMessageIntervalHours>
    ///         <eventIntervalHours>20</eventIntervalHours>
    ///      </TransmissionRegistry>
    [DataContract]
    public class TransmissionRegistry : PLConfigBase
    {
      [DataMember]
      public EventReportingFrequency EventReporting;
      [DataMember]
      public SMUFuelReporting? SMUFuel;
      [DataMember]
      public DateTime? SMUFuelSentUTC;
      [DataMember]
      public int? NextMessageInterval;
      [DataMember]
      public DateTime? NextMessageIntervalSentUTC;
      [DataMember]
      public int? EventIntervalHours;
      [DataMember]
      public DateTime? EventIntervalHoursSentUTC;

      public TransmissionRegistry() { }
      public TransmissionRegistry(XElement element)
      {
        Parse(element);
      }

      public XElement ToXElement()
      {
        var registry = new XElement("TransmissionRegistry");
        if (EventReporting != null)
          registry.Add(EventReporting.ToXElement());
        if (SMUFuel.HasValue)
          registry.Add(new XElement("smuFuelReporting", SMUFuel.Value.ToString()));
        if(SMUFuelSentUTC.HasValue)
          registry.Add(new XElement("SMUFuelSentUTC", SMUFuelSentUTC.Value));
        if (NextMessageInterval.HasValue)
          registry.Add(new XElement("nextMessageIntervalHours", NextMessageInterval.Value));
        if (NextMessageIntervalSentUTC.HasValue)
          registry.Add(new XElement("NextMessageIntervalSentUTC", NextMessageIntervalSentUTC.Value));
        if (EventIntervalHours.HasValue)
          registry.Add(new XElement("eventIntervalHours", EventIntervalHours));
        if (EventIntervalHoursSentUTC.HasValue)
          registry.Add(new XElement("EventIntervalHoursSentUTC", EventIntervalHoursSentUTC.Value));

        return registry;
      }

      public override void SetCurrent(PLConfigBase config)
      {
        var newRegistry = config as TransmissionRegistry;
        if (newRegistry == null) return;
        if (newRegistry.EventReporting != null)
        {
          if (EventReporting == null)
            EventReporting = new EventReportingFrequency();
          EventReporting.SetEventReporting(newRegistry.EventReporting);
        }
        if (newRegistry.SMUFuel.HasValue)
          SMUFuel = newRegistry.SMUFuel;
        if (newRegistry.SMUFuelSentUTC.HasValue)
          SMUFuelSentUTC = newRegistry.SMUFuelSentUTC;
        if(newRegistry.NextMessageInterval.HasValue)
          NextMessageInterval = newRegistry.NextMessageInterval;
        if (newRegistry.NextMessageIntervalSentUTC.HasValue)
          NextMessageIntervalSentUTC = newRegistry.NextMessageIntervalSentUTC;
        if(newRegistry.EventIntervalHours.HasValue)
          EventIntervalHours = newRegistry.EventIntervalHours;
        if (newRegistry.EventIntervalHoursSentUTC.HasValue)
          EventIntervalHoursSentUTC = newRegistry.EventIntervalHoursSentUTC;
      }


      public override bool ReconcilePending(PLConfigBase incomingConfigBase)
      {
        //compare incoming to self and remove any incoming from self
        var incomingTransmissionRegistry = incomingConfigBase as TransmissionRegistry;

        if (incomingTransmissionRegistry != null)
        {
          if (incomingTransmissionRegistry.EventIntervalHours.HasValue && EventIntervalHours.HasValue
            && incomingTransmissionRegistry.EventIntervalHours.Value == EventIntervalHours.Value)
          {
            EventIntervalHours = null;
            EventIntervalHoursSentUTC = null;
          }

          if (incomingTransmissionRegistry.EventReporting != null && EventReporting != null)
          {
            if (incomingTransmissionRegistry.EventReporting.DiagnosticFreqCode.HasValue && EventReporting.DiagnosticFreqCode.HasValue
              && incomingTransmissionRegistry.EventReporting.DiagnosticFreqCode == EventReporting.DiagnosticFreqCode)
            {
              EventReporting.DiagnosticFreqCode = null;
              EventReporting.DiagnosticFreqSentUTC = null;
            }

            if (incomingTransmissionRegistry.EventReporting.Level1EventFreqCode.HasValue && EventReporting.Level1EventFreqCode.HasValue
              && incomingTransmissionRegistry.EventReporting.Level1EventFreqCode == EventReporting.Level1EventFreqCode)
            {
              EventReporting.Level1EventFreqCode = null;
              EventReporting.Level1EventFreqSentUTC = null;
            }
            
            if (incomingTransmissionRegistry.EventReporting.Level2EventFreqCode.HasValue && EventReporting.Level2EventFreqCode.HasValue
              && incomingTransmissionRegistry.EventReporting.Level2EventFreqCode == EventReporting.Level2EventFreqCode)
            {
              EventReporting.Level2EventFreqCode = null;
              EventReporting.Level2EventFreqSentUTC = null;
            }

            if (incomingTransmissionRegistry.EventReporting.Level3EventFreqCode.HasValue && EventReporting.Level3EventFreqCode.HasValue
              && incomingTransmissionRegistry.EventReporting.Level3EventFreqCode == EventReporting.Level3EventFreqCode)
            {
              EventReporting.Level3EventFreqCode = null;
              EventReporting.Level3EventFreqSentUTC = null;
            }

            if (EventReporting.DiagnosticFreqCode == null
                && !incomingTransmissionRegistry.EventReporting.Level1EventFreqCode.HasValue
                && !incomingTransmissionRegistry.EventReporting.Level2EventFreqCode.HasValue
                && !incomingTransmissionRegistry.EventReporting.Level3EventFreqCode.HasValue)
            {
              incomingTransmissionRegistry.EventReporting = null;
            }
          }
          
          if (incomingTransmissionRegistry.NextMessageInterval.HasValue && NextMessageInterval.HasValue
            && incomingTransmissionRegistry.NextMessageInterval.Value == NextMessageInterval.Value)
          {
            NextMessageInterval = null;
            NextMessageIntervalSentUTC = null;
          }

          if (incomingTransmissionRegistry.SMUFuel.HasValue && SMUFuel.HasValue
            && incomingTransmissionRegistry.SMUFuel.Value == SMUFuel.Value)
          {
            SMUFuel = null;
            SMUFuelSentUTC = null;
          }
        }

        var somethingDifferent = false // false here is just for readability
          || SMUFuel != null
          || NextMessageInterval != null
          || EventIntervalHours != null;

        if (!somethingDifferent && EventReporting != null)
        {
          // keep looking
          somethingDifferent = somethingDifferent
            || EventReporting.DiagnosticFreqCode != null
            || EventReporting.Level1EventFreqCode != null
            || EventReporting.Level2EventFreqCode != null
            || EventReporting.Level3EventFreqCode != null;
        }

        var fullyReconciled = !somethingDifferent;
        return fullyReconciled;
      }

      private void Parse(XElement element)
      {
        var reporting = element.Elements("EventReportingFrequency").FirstOrDefault();
        if (reporting != null)
        {
          EventReporting = new EventReportingFrequency(reporting);
        }
        var smu = element.GetStringElement("smuFuelReporting");
        if(!string.IsNullOrEmpty(smu))
          SMUFuel = Enum.Parse(typeof(SMUFuelReporting), smu, true) as SMUFuelReporting?;

        SMUFuelSentUTC = element.GetUTCDateTimeElement("SMUFuelSentUTC");
        NextMessageInterval = element.GetIntElement("nextMessageIntervalHours");

        NextMessageIntervalSentUTC = element.GetUTCDateTimeElement("NextMessageIntervalSentUTC");

        EventIntervalHours = element.GetIntElement("eventIntervalHours");

        EventIntervalHoursSentUTC = element.GetUTCDateTimeElement("EventIntervalHoursSentUTC");
      }

      [DataContract]
      public class EventReportingFrequency
      {
        [DataMember]
        public EventFrequency? Level1EventFreqCode;
        [DataMember]
        public DateTime? Level1EventFreqSentUTC;
        [DataMember]
        public EventFrequency? Level2EventFreqCode;
        [DataMember]
        public DateTime? Level2EventFreqSentUTC;
        [DataMember]
        public EventFrequency? Level3EventFreqCode;
        [DataMember]
        public DateTime? Level3EventFreqSentUTC;
        [DataMember]
        public EventFrequency? DiagnosticFreqCode;
        [DataMember]
        public DateTime? DiagnosticFreqSentUTC;

        public EventReportingFrequency() { }
        public EventReportingFrequency(XElement element) 
        {
          Parse(element);
        }

        public XElement ToXElement()
        {
          var eventReporting = new XElement("EventReportingFrequency");
          if(Level1EventFreqCode.HasValue)
            eventReporting.SetAttributeValue("level1EventFreqCode", Level1EventFreqCode.Value.ToString());
          if(Level1EventFreqSentUTC.HasValue)
            eventReporting.SetAttributeValue("Level1EventFreqSentUTC", Level1EventFreqSentUTC);
          if(Level2EventFreqCode.HasValue)
            eventReporting.SetAttributeValue("level2EventFreqCode", Level2EventFreqCode.Value.ToString());
          if (Level2EventFreqSentUTC.HasValue)
            eventReporting.SetAttributeValue("Level2EventFreqSentUTC", Level2EventFreqSentUTC);
          if(Level3EventFreqCode.HasValue)
          eventReporting.SetAttributeValue("level3EventFreqCode", Level3EventFreqCode.Value.ToString());
          if (Level3EventFreqSentUTC.HasValue)
            eventReporting.SetAttributeValue("Level3EventFreqSentUTC", Level3EventFreqSentUTC);
          if(DiagnosticFreqCode.HasValue)
            eventReporting.SetAttributeValue("diagnosticFreqCode", DiagnosticFreqCode.Value.ToString());
          if (DiagnosticFreqSentUTC.HasValue)
            eventReporting.SetAttributeValue("DiagnosticFreqSentUTC", DiagnosticFreqSentUTC);

          return eventReporting;
        }

        internal void SetEventReporting(EventReportingFrequency eventReportingFrequency)
        {
          if (eventReportingFrequency != null)
          {
            if (eventReportingFrequency.Level1EventFreqCode.HasValue)
              Level1EventFreqCode = eventReportingFrequency.Level1EventFreqCode;
            if (eventReportingFrequency.Level1EventFreqSentUTC.HasValue)
              Level1EventFreqSentUTC = eventReportingFrequency.Level1EventFreqSentUTC;
            
            if (eventReportingFrequency.Level2EventFreqCode.HasValue)
              Level2EventFreqCode = eventReportingFrequency.Level2EventFreqCode;
            if (eventReportingFrequency.Level2EventFreqSentUTC.HasValue)
              Level2EventFreqSentUTC = eventReportingFrequency.Level2EventFreqSentUTC;

            if (eventReportingFrequency.Level3EventFreqCode.HasValue)
              Level3EventFreqCode = eventReportingFrequency.Level3EventFreqCode;
            if (eventReportingFrequency.Level3EventFreqSentUTC.HasValue)
              Level3EventFreqSentUTC = eventReportingFrequency.Level3EventFreqSentUTC;

            if (eventReportingFrequency.DiagnosticFreqCode.HasValue)
              DiagnosticFreqCode = eventReportingFrequency.DiagnosticFreqCode;
            if (eventReportingFrequency.DiagnosticFreqSentUTC.HasValue)
              DiagnosticFreqSentUTC = eventReportingFrequency.DiagnosticFreqSentUTC;
          }
        }

        private void Parse(XElement element)
        {
          var freqCode = element.GetStringAttribute("level1EventFreqCode");
          if(string.IsNullOrEmpty(freqCode))
            freqCode = element.GetStringElement("level1EventFreqCode");
          if(!string.IsNullOrEmpty(freqCode))
            Level1EventFreqCode = Enum.Parse(typeof(EventFrequency), freqCode, true) as EventFrequency?;

          Level1EventFreqSentUTC = element.GetUTCDateTimeAttribute("Level1EventFreqSentUTC");

          freqCode = element.GetStringAttribute("level2EventFreqCode");
          if (string.IsNullOrEmpty(freqCode))
            freqCode = element.GetStringElement("level2EventFreqCode");
          if (!string.IsNullOrEmpty(freqCode))
            Level2EventFreqCode = Enum.Parse(typeof(EventFrequency), freqCode, true) as EventFrequency?;

          Level2EventFreqSentUTC = element.GetUTCDateTimeAttribute("Level2EventFreqSentUTC");

          freqCode = element.GetStringAttribute("level3EventFreqCode");
          if (string.IsNullOrEmpty(freqCode))
            freqCode = element.GetStringElement("level3EventFreqCode");
          if (!string.IsNullOrEmpty(freqCode))
            Level3EventFreqCode = Enum.Parse(typeof(EventFrequency), freqCode, true) as EventFrequency?;

          Level3EventFreqSentUTC = element.GetUTCDateTimeAttribute("Level3EventFreqSentUTC");

          freqCode = element.GetStringAttribute("diagnosticFreqCode");
          if (string.IsNullOrEmpty(freqCode))
            freqCode = element.GetStringElement("diagnosticFreqCode");
          if (!string.IsNullOrEmpty(freqCode))
            DiagnosticFreqCode = Enum.Parse(typeof(EventFrequency), freqCode,true) as EventFrequency?;

          DiagnosticFreqSentUTC = element.GetUTCDateTimeAttribute("DiagnosticFreqSentUTC");
        }
      }
    }

    
    /// <summary>
    /// /// Digital Registry parses xml of the type below when this class does a toxelement sensor information it
    /// uses attributes instead of elements to save on some space
    /// <DigitalRegistry>
    ///<sensorInformation>
    ///  <sensorNumber>1</sensorNumber>
    ///  <sensorConfigurationCode>57</sensorConfigurationCode>
    ///  <delayTimeSec>900</delayTimeSec>
    ///  <userDescription>Partikelfilter</userDescription>
    ///  <monitorCondition>028C</monitorCondition>
    ///</sensorInformation>
    ///</DigitalRegistry>
    /// </summary>
    [DataContract]
    public class DigitalRegistry : PLConfigBase
    {
      [DataMember]
      public List<SensorInformation> Sensors;

      public DigitalRegistry() { }
      public DigitalRegistry(XElement element)
      {
        Parse(element);
      }

      public XElement ToXElement()
      {
        var element = new XElement("DigitalRegistry");
        if (Sensors != null)
        {
          foreach (var sensor in Sensors)
          {
            if (sensor != null)
            {
              element.Add(sensor.ToXElement());
            }
          }
        }
        return element;
      }

      public override void SetCurrent(PLConfigBase config)
      {
        var newRegistry = config as DigitalRegistry;
        if (newRegistry != null && newRegistry.Sensors != null)
        {
          if (Sensors == null)
            Sensors = new List<SensorInformation>();

          var newSensors = newRegistry.Sensors ==null ? null : newRegistry.Sensors.Where(e => e != null).ToList();

          if (newSensors != null && newSensors.Count > 0)
          {
            foreach (var s in newSensors)
            {
              var sensor = (from a in Sensors
                                          where a.SensorNumber == s.SensorNumber
                                          select a).FirstOrDefault();
              if (sensor == null)
                Sensors.Add(new SensorInformation
                {
                  DelayTime = s.DelayTime,
                  DelayTimeSentUTC = s.DelayTimeSentUTC,
                  Description = s.Description,
                  DescriptionSentUTC = s.DescriptionSentUTC,
                  MonitorCondition = s.MonitorCondition,
                  MonitorConditionSentUTC = s.MonitorConditionSentUTC,
                  SensorConfigSentUTC = s.SensorConfigSentUTC,
                  SensorConfiguration = s.SensorConfiguration,
                  SensorNumber = s.SensorNumber
                });
              else
              {
                if (s.DelayTime.HasValue)
                  sensor.DelayTime = s.DelayTime;
                if (s.DelayTimeSentUTC.HasValue)
                  sensor.DelayTimeSentUTC = s.DelayTimeSentUTC;
                if (!string.IsNullOrEmpty(s.Description))
                  sensor.Description = s.Description;
                if (s.DescriptionSentUTC.HasValue)
                  sensor.DescriptionSentUTC = s.DescriptionSentUTC;
                if (s.MonitorCondition.HasValue)
                  sensor.MonitorCondition = s.MonitorCondition;
                if (s.MonitorConditionSentUTC.HasValue)
                  sensor.MonitorConditionSentUTC = s.MonitorConditionSentUTC;
                if (s.SensorConfiguration.HasValue)
                  sensor.SensorConfiguration = s.SensorConfiguration;
                if (s.SensorConfigSentUTC.HasValue)
                  sensor.SensorConfigSentUTC = s.SensorConfigSentUTC;
              }
            }
          }
        }
        else
        {
          Sensors = null;
        }
      }

      public override bool ReconcilePending(PLConfigBase incomingPlConfigBase)
      {
        var incomingDigitalRegistry = incomingPlConfigBase as DigitalRegistry;

        if (incomingDigitalRegistry != null)
        {
          if (incomingDigitalRegistry.Sensors != null && Sensors != null)
          {
            foreach (var incomingSensor in incomingDigitalRegistry.Sensors)
            {
              var pendingSensor = Sensors.FirstOrDefault(f => f.SensorNumber == incomingSensor.SensorNumber);

              if (pendingSensor != null)
              {
                if (incomingSensor.DelayTime.HasValue && pendingSensor.DelayTime.HasValue
                    && incomingSensor.DelayTime.Value == pendingSensor.DelayTime.Value)
                {
                  Sensors[Sensors.IndexOf(pendingSensor)].DelayTime = null;
                  Sensors[Sensors.IndexOf(pendingSensor)].DelayTimeSentUTC = null;
                }

                if (incomingSensor.Description != null && incomingSensor.Description == pendingSensor.Description)
                {
                  Sensors[Sensors.IndexOf(pendingSensor)].Description = null;
                  Sensors[Sensors.IndexOf(pendingSensor)].DescriptionSentUTC = null;
                }

                if (incomingSensor.MonitorCondition.HasValue && pendingSensor.MonitorCondition.HasValue
                  && incomingSensor.MonitorCondition.Value == pendingSensor.MonitorCondition.Value)
                {
                  Sensors[Sensors.IndexOf(pendingSensor)].MonitorCondition = null;
                  Sensors[Sensors.IndexOf(pendingSensor)].MonitorConditionSentUTC = null;
                }

                if (incomingSensor.SensorConfiguration.HasValue && pendingSensor.SensorConfiguration.HasValue
                  && incomingSensor.SensorConfiguration.Value == pendingSensor.SensorConfiguration.Value)
                {
                  Sensors[Sensors.IndexOf(pendingSensor)].SensorConfiguration = null;
                  Sensors[Sensors.IndexOf(pendingSensor)].SensorConfigSentUTC = null;
                }

                if (!Sensors[Sensors.IndexOf(pendingSensor)].DelayTime.HasValue
                    && String.IsNullOrEmpty(Sensors[Sensors.IndexOf(pendingSensor)].Description)
                    && !Sensors[Sensors.IndexOf(pendingSensor)].MonitorCondition.HasValue
                    && !Sensors[Sensors.IndexOf(pendingSensor)].SensorConfiguration.HasValue)
                {
                  Sensors.Remove(pendingSensor);
                }
              }
            }
          }
        }

        var fullyReconciled = false;

        if (Sensors != null && Sensors.Count == 0)
        {
          Sensors = null;
          fullyReconciled = true;
        }

        return fullyReconciled;
      }

      private void Parse(XElement element)
      {
        var sensorElements = element.Elements("sensorInformation").ToList();
        if (sensorElements.Count > 0)
        {
          Sensors = new List<SensorInformation>();
          foreach (var sensor in sensorElements)
          {
            Sensors.Add(new SensorInformation(sensor));
          }
        }
      }

      [DataContract]
      public class SensorInformation
      {
        [DataMember]
        public int SensorNumber;
        [DataMember]
        public InputConfig? SensorConfiguration;
        [DataMember]
        public DateTime? SensorConfigSentUTC;

        [DataMember]
        public TimeSpan? DelayTime;
        [DataMember]
        public DateTime? DelayTimeSentUTC;

        [DataMember]
        public string Description;
        [DataMember]
        public DateTime? DescriptionSentUTC;

        [DataMember]
        public DigitalInputMonitoringConditions? MonitorCondition;
        [DataMember]
        public DateTime? MonitorConditionSentUTC;

        public SensorInformation() { }
        public SensorInformation(XElement element)
        {
          Parse(element);
        }

        public XElement ToXElement()
        {
          var element = new XElement("sensorInformation");
          element.SetAttributeValue("sensorNumber", SensorNumber);
          if (SensorConfiguration.HasValue)
            element.SetAttributeValue("sensorConfigurationCode", ((int)SensorConfiguration.Value).ToString("X"));

          if (SensorConfigSentUTC.HasValue)
            element.SetAttributeValue("SensorConfigSentUTC", SensorConfigSentUTC);
          if (DelayTime.HasValue)
            element.SetAttributeValue("delayTimeSec", DelayTime.Value.TotalSeconds);
          if (DelayTimeSentUTC.HasValue)
            element.SetAttributeValue("DelayTimeSentUTC", DelayTimeSentUTC);
          if (!string.IsNullOrEmpty(Description))
            element.SetAttributeValue("userDescription", Description);
          if (DescriptionSentUTC.HasValue)
            element.SetAttributeValue("DescriptionSentUTC", DescriptionSentUTC);
          if (MonitorCondition.HasValue)
            element.SetAttributeValue("monitorCondition", ((int)MonitorCondition.Value).ToString("X"));
          if (MonitorConditionSentUTC.HasValue)
            element.SetAttributeValue("MonitorConditionSentUTC", MonitorConditionSentUTC);

          return element;
        }

        private void Parse(XElement element)
        {
          var sensorNum = element.GetIntAttribute("sensorNumber");
          if (!sensorNum.HasValue)
            sensorNum = element.GetIntElement("sensorNumber");
          if (sensorNum.HasValue)
            SensorNumber = sensorNum.Value;

          var sensorConfigString = element.GetStringAttribute("sensorConfigurationCode");
          if (string.IsNullOrEmpty(sensorConfigString))
            sensorConfigString = element.GetStringElement("sensorConfigurationCode");
          int sensorConfig;
          if (int.TryParse(sensorConfigString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out sensorConfig))
            if (Enum.IsDefined(typeof (InputConfig), sensorConfig))
              SensorConfiguration =
                (InputConfig?) Enum.ToObject(typeof (InputConfig), sensorConfig);
            else
            {
              Log.IfWarnFormat("PLConfigData.SensorInformation.Parse: Invalid sensorConfigurationCode '{0}'",
                sensorConfigString);
              SensorConfiguration = null;
            }

          SensorConfigSentUTC = element.GetUTCDateTimeAttribute("SensorConfigSentUTC");

          var delayTimeSec = element.GetDoubleAttribute("delayTimeSec");
          if (!delayTimeSec.HasValue)
            delayTimeSec = element.GetDoubleElement("delayTimeSec");
          if (delayTimeSec.HasValue)
            DelayTime = TimeSpan.FromSeconds(delayTimeSec.Value);

          DelayTimeSentUTC = element.GetUTCDateTimeAttribute("DelayTimeSentUTC");

          Description = element.GetStringAttribute("userDescription");
          if (string.IsNullOrEmpty(Description))
            Description = element.GetStringElement("userDescription");

          DescriptionSentUTC = element.GetUTCDateTimeAttribute("DescriptionSentUTC");

          var monitor = element.GetStringAttribute("monitorCondition");
          if (string.IsNullOrEmpty(monitor))
            monitor = element.GetStringElement("monitorCondition");
          int condition;
          if (int.TryParse(monitor, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out condition))
            if (Enum.IsDefined(typeof (DigitalInputMonitoringConditions), condition))
              MonitorCondition =
                (DigitalInputMonitoringConditions?) Enum.ToObject(typeof (DigitalInputMonitoringConditions), condition);
            else
            {
              Log.IfWarnFormat("PLConfigData.SensorInformation.Parse: Invalid monitorCondition '{0}'", monitor);
              MonitorCondition = null;
            }

          MonitorConditionSentUTC = element.GetUTCDateTimeAttribute("MonitorConditionSentUTC");
        }
      }
    }
  }
}
