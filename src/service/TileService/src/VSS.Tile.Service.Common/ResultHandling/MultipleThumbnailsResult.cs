using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Tile.Service.Common.ResultHandling
{
  /// <summary>
  /// The result of a request to get multiple thumbnail images.
  /// </summary>
  public class MultipleThumbnailsResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of thumbnails for the requested geofences.
    /// </summary>
    [JsonProperty(PropertyName = "thumbnails")]
    public List<ThumbnailResult> Thumbnails { get; set; }
  }

  /// <summary>
  /// A thumbnail image for a geofence
  /// </summary>
  public class ThumbnailResult
  {
    /// <summary>
    /// The Uid of the geofence
    /// </summary>
    [JsonProperty(PropertyName = "uid")]
    public Guid Uid { get; set; }
    /// <summary>
    /// The image as a base64 encoded string
    /// </summary>
    [JsonProperty(PropertyName = "data")]
    public byte[] Data { get; set; }
  }
}
