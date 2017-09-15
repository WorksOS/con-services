using Newtonsoft.Json;
using VSS.MasterData.Models.Interfaces;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Describes geofence data returned by the geofence master data service.
  /// </summary>
  public class GeofenceData : BaseDataResult, IData
  {
    /// <summary>
    /// Gets or sets the descriptor.
    /// </summary>
    /// <value>
    /// The <see cref="GeofenceDescriptor"/> descriptor object.
    /// </value>
    [JsonProperty(PropertyName = "geofenceDescriptor")]
    public GeofenceDescriptor GeofenceDescriptor { get; set; }

    /// <summary>
    /// Key to use for caching geofence master data.
    /// </summary>
    [JsonIgnore]
    public string CacheKey => GeofenceDescriptor.GeofenceUID.ToString();
  }
}