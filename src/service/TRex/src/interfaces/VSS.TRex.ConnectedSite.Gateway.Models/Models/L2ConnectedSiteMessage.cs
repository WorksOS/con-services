using System;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;
using VSS.TRex.Types;

namespace VSS.TRex.ConnectedSite.Gateway.Models
{
  /// <summary>
  /// Concrete Implementation of a L2 Connected Site message aka status message
  /// </summary>
  public class L2ConnectedSiteMessage : AbstractConnectedSiteMessage, IL2ConnectedSiteMessage
  {
    protected override string ApiRoute => "devicegateway/status/";
    public override DateTime? Timestamp { get; set; }
    public string DesignName { get; set; }
    public string AssetType { get; set; }
    public string AppVersion { get; set; }
    public string AppName => "GCS900";
    public string AssetNickname { get; set; }

    public L2ConnectedSiteMessage() { }



  }
}
