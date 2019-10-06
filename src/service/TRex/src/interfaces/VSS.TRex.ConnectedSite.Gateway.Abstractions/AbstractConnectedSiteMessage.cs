using System;
using VSS.TRex.Common.Types;

namespace VSS.TRex.ConnectedSite.Gateway.Abstractions
{
  public abstract class AbstractConnectedSiteMessage : IConnectedSiteMessage
  {
    public abstract DateTime? Timestamp { get; set; }
    protected abstract string ApiRoute { get; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? Height { get; set; }
    public string HardwareID { get; set; }
    public MachineControlPlatformType PlatformType { get; set;}
    public string Route => $"{ApiRoute}{PlatformType}-{HardwareID}";


  }
}
