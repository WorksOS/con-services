using System;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.Models
{
  /// <summary>
  /// Concrete Implementation of a L1 Connected Site message aka position message
  /// </summary>
  public class L1ConnectedSiteMessage : AbstractConnectedSiteMessage, IL1ConnectedSiteMessage
  {
    private const string API_ROUTE = "positions/in/v1/";
    public override string Route { get => $"{API_ROUTE}{PlatformType}-{HardwareID}"; }
    public override DateTime? Timestamp { get; set; }

    public L1ConnectedSiteMessage() { }
  }
}
