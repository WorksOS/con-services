using System;
using Newtonsoft.Json;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  interface IL2ConnectedSiteMessage : IConnectedSiteMessage
  {
    [JsonProperty("timestamp")]
    DateTime? Timestamp { get; set; }

    [JsonProperty("designName")]
    string DesignName { get; set; }

    [JsonProperty("assetType")]
    string AssetType { get; set; }

    [JsonProperty("appVersion")]
    string AppVersion { get; set; }
    /// <summary>
    /// This will normally for GCS900 for messges from harvested TAGS 
    /// </summary>
    [JsonProperty("appName")]
    string AppName { get; }
    /// <summary>
    /// MachineID in tag file
    /// </summary>
    [JsonProperty("assetNickname")]
    string AssetNickname { get; set; }

  }
}
