using System;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.TAGFiles.Executors;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Models
{
  public class L1ConnectedSiteMessage : AbstractConnectedSiteMessage, IL1ConnectedSiteMessage
  {
    private const string API_ROUTE = "positions/in/v1/GCS900";
    public override string Route { get { return $"{API_ROUTE}-{HardwareID}"; } }
    public override DateTime? Timestamp { get; set; }

    public L1ConnectedSiteMessage() { }

    public L1ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      Timestamp = tagFilePrescan.SeedTimeUTC;
      Lattitude = tagFilePrescan.SeedLatitude;
      Longitude = tagFilePrescan.SeedLongitude;
      Height = tagFilePrescan.SeedHeight;
      HardwareID = tagFilePrescan.HardwareID;
    }
  }
}
