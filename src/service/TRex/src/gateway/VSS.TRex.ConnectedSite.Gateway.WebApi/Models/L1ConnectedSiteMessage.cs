using System;
using VSS.TRex.Common.Utilities;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.TAGFiles.Executors;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Models
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

    public L1ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      Timestamp = tagFilePrescan.SeedTimeUTC;
      Lattitude = tagFilePrescan.SeedLatitude.HasValue ? (double?)MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLatitude.Value) : null;
      Longitude = tagFilePrescan.SeedLatitude.HasValue ? (double?)MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLongitude.Value) : null;
      Height = tagFilePrescan.SeedHeight;
      HardwareID = tagFilePrescan.HardwareID;
      PlatformType = tagFilePrescan.PlatformType;
    }
  }
}
