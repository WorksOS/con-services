using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VSS.Hosted.VLCommon;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// This type represents a summary of all device config that can be modified by the user.
  /// Each type of config has a pending and current copy of the config. The pending config is the data
  /// that was last sent to the device, and which has not been acknowledged. The current config has
  /// been acknowledged by the device. Pending will be nullified when it is no longer pending an ACK.
  /// 
  /// This type can be serialized to/from XML for convenient storage.
  /// </summary>
  public abstract class DeviceConfigData
  {
    public DeviceConfigData() { }
    public DeviceConfigData(string xml)
    {
      if (string.IsNullOrEmpty(xml))
        return;

      Parse(xml);
    }
    public DeviceConfigData(XElement xml)
    {
      if (null == xml)
        return;

      Parse(xml);
    }

    #region Implementation
    protected void Update<T>(ref T existingCurrent, ref T existingPending, T latest) where T : DeviceConfigBase
    {
      if (existingCurrent != null && latest != null && latest.Status == MessageStatusEnum.Acknowledged)
        existingCurrent.SetCurrent(latest);
      else if (existingPending != null && latest != null && latest.Status != MessageStatusEnum.Acknowledged)
        existingPending.SetCurrent(latest);

      if (existingPending != null && existingCurrent != null &&
          existingPending.MessageSourceID == existingCurrent.MessageSourceID)
        existingPending = null;
    }

    public void Parse(string xml)
    {
      Parse(XElement.Parse(xml));
    }
    protected void Parse(XElement element)
    {
      XElement pending = element.Elements("Pending").FirstOrDefault();
      XElement current = element.Elements("Current").FirstOrDefault();
      XElement lastSent = element.Elements("LastSent").FirstOrDefault();

      if (pending != null)
        GetPending(pending);
      if (current != null)
        GetCurrent(current);
      if (lastSent != null)
        GetLastSent(lastSent);
    }

    #endregion

    #region ABSTRACT Methods
    public abstract void Update(DeviceConfigBase config);
		
		/// <returns>true if incident was audited</returns>
    public abstract bool AuditConfigChanges(INH_OP ctx, Asset asset, DeviceConfigBase config);
    public abstract void UpdateCurrentStatus(INH_OP ctx, Asset asset, DeviceConfigBase config);
    public abstract XElement ToXElement();

    protected abstract void GetPending(XElement pending);
    protected abstract void GetCurrent(XElement current);
    protected abstract void GetLastSent(XElement lastSent);
    #endregion
  }

  #region Sub types
  [DataContract(Namespace = "http://www.nighthawk.com/nighthawk/service/NHOP/2009/10"),
  KnownType(typeof(MTSConfigData.MileageRuntimeConfig)),
  KnownType(typeof(MTSConfigData.DailyReportConfig)),
  KnownType(typeof(MTSConfigData.SpeedingConfig)),
  KnownType(typeof(MTSConfigData.StoppedConfig)),
  KnownType(typeof(MTSConfigData.MovingConfig)),
  KnownType(typeof(MTSConfigData.MaintenanceModeConfig)),
  KnownType(typeof(MTSConfigData.TMSConfig)),
  KnownType(typeof(MTSConfigData.DiscreteInputConfig)),
  KnownType(typeof(MTSConfigData.DigitalSwitchConfig)),
  KnownType(typeof(MTSConfigData.TamperSecurityAdministrationInformationConfig)),
  KnownType(typeof(MTSConfigData.DeviceMachineSecurityReportingStatusMessageConfig)),
  KnownType(typeof(MTSConfigData.SMHSourceConfig)),
  KnownType(typeof(RuntimeAdjConfig)),
  KnownType(typeof(A5N2ConfigData.DigitalSwitchConfig)),
  KnownType(typeof(A5N2ConfigData.MileageRuntimeConfig)),
  KnownType(typeof(A5N2ConfigData.DailyReportConfig)),
  KnownType(typeof(A5N2ConfigData.SpeedingConfig)),
  KnownType(typeof(A5N2ConfigData.StoppedConfig)),
  KnownType(typeof(A5N2ConfigData.MovingConfig)),
  KnownType(typeof(A5N2ConfigData.SMHSourceConfig)),
  KnownType(typeof(A5N2ConfigData.TMSConfig)),
  KnownType(typeof(A5N2ConfigData.AssetSecurityStartStatus)),
  KnownType(typeof(A5N2ConfigData.AssetSecurityTamperLevel)),
  KnownType(typeof(A5N2ConfigData.AssetSecurityPendingStartStatus)),
  KnownType(typeof(A5N2ConfigData.DailyReportFrequencyConfig)),
  KnownType(typeof(A5N2ConfigData.ReportingFrequencyConfig)),
  ]

  public abstract class DeviceConfigBase
  {
    [DataMember]
    public DateTime? SentUTC;
    [DataMember]
    public MessageStatusEnum Status;
    [DataMember]
    public long MessageSourceID;

    public DeviceConfigBase() { }

    public DeviceConfigBase(string xml)
    {
      ParseBase(XElement.Parse(xml));
    }

    public DeviceConfigBase(XElement xml)
    {
      ParseBase(xml);
    }

    public void SetCurrent(DeviceConfigBase latest)
    {
      this.MessageSourceID = latest.MessageSourceID;
      this.SentUTC = latest.SentUTC;
      this.Status = latest.Status;

      SetCurrentConfig(latest);
    }

    private void ParseBase(XElement element)
    {
      DateTime? sent = element.GetDateTimeAttribute("SentUTC");
      if (sent.HasValue)
        SentUTC = sent.Value.ToUniversalTime();

      int? status = element.GetIntAttribute("Status");
      if (status.HasValue)
        Status = (MessageStatusEnum)status.Value;

      long? messageSource = element.GetLongAttribute("MessageSourceID");
      if (messageSource.HasValue)
        MessageSourceID = messageSource.Value;

      Parse(element);
    }

    public XElement ToXElement()
    {
      var element = ConfigToXElement();

      if (null != element)
      {
        if (SentUTC.HasValue)
          element.SetAttributeValue("SentUTC", SentUTC);

        element.SetAttributeValue("Status", (int)Status);
        element.SetAttributeValue("MessageSourceID", MessageSourceID);
      }

      return element;
    }

    protected abstract void Parse(XElement element);
    protected abstract XElement ConfigToXElement();
    protected abstract void SetCurrentConfig(DeviceConfigBase latest);
  }

  [DataContract]
  public class RuntimeAdjConfig : DeviceConfigBase
  {
    [DataMember]
    public TimeSpan Runtime;

    public RuntimeAdjConfig() { }

    public RuntimeAdjConfig(string xml)
      : base(xml)
    {
    }

    public RuntimeAdjConfig(XElement xml)
      : base(xml)
    {
    }

    protected override XElement ConfigToXElement()
    {
      XElement runtime = new XElement("RuntimeAdjConfig");
      runtime.SetAttributeValue("Runtime", Runtime.ToString());
      runtime.SetAttributeValue("SentUTC", SentUTC);
      runtime.SetAttributeValue("Status", (int)Status);
      runtime.SetAttributeValue("MessageSourceID", MessageSourceID);

      return runtime;
    }

    protected override void Parse(XElement element)
    {
      if (element != null)
      {
        TimeSpan? runtime = element.GetTimeSpanAttribute("Runtime");
        if (runtime.HasValue)
          Runtime = runtime.Value;
      }
    }

    protected override void SetCurrentConfig(DeviceConfigBase latest)
    {
      RuntimeAdjConfig config = latest as RuntimeAdjConfig;
      if (config != null)
      {
        Runtime = config.Runtime;
      }
    }
  }
  #endregion
}