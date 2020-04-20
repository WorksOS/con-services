using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using XnaFan.ImageComparison.Netcore.Common;

namespace CCSS.Tile.Service.IntegrationTests
{
  /// <summary>
  /// The result of a request to get multiple thumbnail images.
  /// </summary>
  public class MultipleThumbnailsResult : ContractExecutionResult, IEquatable<MultipleThumbnailsResult>
  {
    /// <summary>
    /// The list of thumbnails for the requested geofences.
    /// </summary>
    [JsonProperty(PropertyName = "thumbnails")]
    public List<ThumbnailResult> Thumbnails { get; set; }

    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public MultipleThumbnailsResult()
      : base("success")
    { }

    public bool Equals(MultipleThumbnailsResult other)
    {
      if (other == null)
        return false;

      return CommonUtils.ListsAreEqual(Thumbnails, other.Thumbnails) &&
             Code == other.Code &&
             Message == other.Message;
    }

    public static bool operator ==(MultipleThumbnailsResult a, MultipleThumbnailsResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(MultipleThumbnailsResult a, MultipleThumbnailsResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is MultipleThumbnailsResult && this == (MultipleThumbnailsResult)obj;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
  }
}
