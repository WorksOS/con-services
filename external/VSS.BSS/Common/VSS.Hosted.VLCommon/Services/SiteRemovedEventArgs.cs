using System;

namespace VSS.Hosted.VLCommon.Events
{
  public class SiteRemovedEventArgs : EventArgs
  {
    public Site Site { get; set; }
    public DeviceTypeEnum DeviceType { get; set; }
    public string DeviceId { get; set; }
  }
}
