using System;
using Newtonsoft.Json;
using XnaFan.ImageComparison.Netcore.Common;

namespace CCSS.Tile.Service.IntegrationTests
{

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

    public ThumbnailResult()
    { }

    public bool Equals(ThumbnailResult other) => other != null && (Uid == other.Uid && CommonUtils.CompareImages("Multiple", 1, Data, other.Data, out _));

    public static bool operator ==(ThumbnailResult a, ThumbnailResult b)
    {
      if ((object)a == null || (object)b == null)
        return Equals(a, b);

      return a.Equals(b);
    }

    public static bool operator !=(ThumbnailResult a, ThumbnailResult b)
    {
      return !(a == b);
    }

    public override bool Equals(object obj)
    {
      return obj is ThumbnailResult && this == (ThumbnailResult)obj;
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
