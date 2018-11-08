using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.TAGFiles.Executors;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Models
{
  public class L1ConnectedSiteMessage : IConnectedSiteMessage
  {
    private const string API_ROUTE = "positions/in/v1/GCS900";

    public DateTime? Timestamp { get ; set; }
    public double? Lattitude { get; set; }
    public double? Longitude { get; set; }
    public double? Height { get; set; }
    public string Route { get; private set; }

    public L1ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      Timestamp = tagFilePrescan.SeedTimeUTC;
      Lattitude = tagFilePrescan.SeedLatitude;
      Longitude = tagFilePrescan.SeedLongitude;
      Height = tagFilePrescan.SeedHeight;
      Route = $"{API_ROUTE}-{tagFilePrescan.HardwareID}";
    }

    public L1ConnectedSiteMessage()
    {
    }
  }
}
