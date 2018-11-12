using System;
using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  public abstract class AbstractConnectedSiteMessage : IConnectedSiteMessage
  {
    public abstract DateTime? Timestamp { get; set; }
    public double? Lattitude { get; set; }
    public double? Longitude { get; set; }
    public double? Height { get; set; }
    public string HardwareID { get; set; }
    public abstract string Route { get; }
  }
}
