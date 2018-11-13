using System;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;
using VSS.TRex.TAGFiles.Executors;
using VSS.TRex.Types;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Models
{
  /// <summary>
  /// Concrete Implementation of a L2 Connected Site message aka status message
  /// </summary>
  public class L2ConnectedSiteMessage : AbstractConnectedSiteMessage, IL2ConnectedSiteMessage
  {
    private const string API_ROUTE = "status/in/v1/GCS900";

    public override DateTime? Timestamp { get; set; }
    public string DesignName { get; set; }
    public string AssetType { get; set; }
    public string AppVersion { get; set; }
    public string AppName { get { return "GCS900"; } }
    public string AssetNickname { get; set; }
    public override string Route { get => $"{API_ROUTE}-{HardwareID}"; }

    public L2ConnectedSiteMessage() { }

    public L2ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      Timestamp = tagFilePrescan.SeedTimeUTC;
      Lattitude = tagFilePrescan.SeedLatitude;
      Longitude = tagFilePrescan.SeedLongitude;
      Height = tagFilePrescan.SeedHeight;
      HardwareID = tagFilePrescan.HardwareID;
      AssetNickname = tagFilePrescan.MachineID;
      AppVersion = tagFilePrescan.ApplicationVersion;
      DesignName = tagFilePrescan.DesignName;
      AssetType = ((MachineType)tagFilePrescan.MachineType).ToString();
    }

  }
}
