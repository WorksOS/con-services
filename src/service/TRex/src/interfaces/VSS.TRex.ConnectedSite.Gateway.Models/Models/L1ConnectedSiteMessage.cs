using System;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.Models
{
  /// <summary>
  /// Concrete Implementation of a L1 Connected Site message aka position message
  /// </summary>
  public class L1ConnectedSiteMessage : AbstractConnectedSiteMessage, IL1ConnectedSiteMessage
  {
    protected override string ApiRoute => "devicegateway/positions/";
    public override DateTime? Timestamp { get; set; }

    public L1ConnectedSiteMessage() { }
  }
}
