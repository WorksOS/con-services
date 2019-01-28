using System;
using VSS.TRex.Common.Utilities;
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
    private const string API_ROUTE = "status/in/v1/";

    public override DateTime? Timestamp { get; set; }
    public string DesignName { get; set; }
    public string AssetType { get; set; }
    public string AppVersion { get; set; }
    public string AppName { get { return "GCS900"; } }
    public string AssetNickname { get; set; }
    public override string Route { get => $"{API_ROUTE}{MachineSerialUtilities.MapSerialToModel(HardwareID)}-{HardwareID}"; }

    public L2ConnectedSiteMessage() { }

    public L2ConnectedSiteMessage(TAGFilePreScan tagFilePrescan)
    {
      Timestamp = tagFilePrescan.SeedTimeUTC;
      Lattitude = tagFilePrescan.SeedLatitude.HasValue ? MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLatitude.Value) : 0;
      Longitude = tagFilePrescan.SeedLatitude.HasValue ? MathUtilities.RadiansToDegrees(tagFilePrescan.SeedLongitude.Value) : 0;
      Height = tagFilePrescan.SeedHeight;
      HardwareID = tagFilePrescan.HardwareID;
      AssetNickname = tagFilePrescan.MachineID;
      AppVersion = tagFilePrescan.ApplicationVersion;
      DesignName = tagFilePrescan.DesignName;
      AssetType = ((MachineType)tagFilePrescan.MachineType).ToString();
    }

  }
}
