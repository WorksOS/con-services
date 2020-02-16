using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace VSS.Hosted.VLCommon
{
  public class TTConfigData : DeviceConfigData
  {
    public TTConfigData() { }
    public TTConfigData(string xml)
      : base(xml)
    {

    }
    public TTConfigData(XElement xml)
      : base(xml)
    {

    }

    #region General DeviceConfigs

    public RuntimeAdjConfig CurrentRuntimeAdj = null;
    public RuntimeAdjConfig PendingRuntimeAdj = null;

    #endregion

    #region implementation
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
      if (config is RuntimeAdjConfig)
      {
        if (CurrentRuntimeAdj == null && config.Status == MessageStatusEnum.Acknowledged)
          CurrentRuntimeAdj = new RuntimeAdjConfig();
        if (PendingRuntimeAdj == null && config.Status != MessageStatusEnum.Acknowledged)
          PendingRuntimeAdj = new RuntimeAdjConfig();
        Update<RuntimeAdjConfig>(ref CurrentRuntimeAdj, ref PendingRuntimeAdj, (RuntimeAdjConfig)config);
      }
    }

    public override bool AuditConfigChanges(INH_OP ctx, Asset asset, DeviceConfigBase config)
    {
	    return false;
    }

    public override void UpdateCurrentStatus(INH_OP ctx, Asset asset, DeviceConfigBase config)
    { 
    }

    public override XElement ToXElement()
    {
      XElement element = new XElement("TTConfigData");
      XElement pending = new XElement("Pending");
      XElement current = new XElement("Current");

      if (null != PendingRuntimeAdj) pending.Add(PendingRuntimeAdj.ToXElement());
      element.Add(pending);

      if (null != CurrentRuntimeAdj) current.Add(CurrentRuntimeAdj.ToXElement());
      element.Add(current);

      return element;
    }

    protected override void GetPending(XElement pending)
    {
      XElement runtime = pending.Elements("RuntimeAdjConfig").FirstOrDefault();
      if (runtime != null)
        PendingRuntimeAdj = new RuntimeAdjConfig(runtime);
    }

    protected override void GetCurrent(XElement current)
    {
      XElement runtime = current.Elements("RuntimeAdjConfig").FirstOrDefault();
      if (runtime != null)
        CurrentRuntimeAdj = new RuntimeAdjConfig(runtime);
    }

    protected override void GetLastSent(XElement lastSent)
    {
      // Not required to be implemented
    }

    #endregion
  }
}
