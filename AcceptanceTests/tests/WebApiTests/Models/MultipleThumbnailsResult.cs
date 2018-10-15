using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebApiTests.Utilities;

namespace WebApiTests.Models
{
  /// <summary>
  /// The result of a request to get multiple thumbnail images.
  /// </summary>
  public class MultipleThumbnailsResult : RequestResult, IEquatable<MultipleThumbnailsResult>
  {
    #region Members
    /// <summary>
    /// The list of thumbnails for the requested geofences.
    /// </summary>
    [JsonProperty(PropertyName = "thumbnails")]
    public List<ThumbnailResult> Thumbnails { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor: Success by default
    /// </summary>
    public MultipleThumbnailsResult()
      : base("success")
    { }
    #endregion

    #region Equality test
    public bool Equals(MultipleThumbnailsResult other)
    {
      if (other == null)
        return false;

      return CommonUtils.ListsAreEqual(this.Thumbnails, other.Thumbnails) &&
             this.Code == other.Code &&
             this.Message == other.Message;
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
    #endregion

    #region ToString override
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion
  }

  /// <summary>
  /// A thumbnail image for a geofence
  /// </summary>
  public class ThumbnailResult
  {
    #region Members
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
    #endregion

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public ThumbnailResult() 
    { }
    #endregion

    #region Equality test
    public bool Equals(ThumbnailResult other)
    {
      if (other == null)
        return false;

      return this.Uid == other.Uid &&
             CommonUtils.TilesMatch("Multiple", "1", this.Data, other.Data);
    }

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
    #endregion

    #region ToString override
    /// <summary>
    /// ToString override
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
    #endregion
  }
}
