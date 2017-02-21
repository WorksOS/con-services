using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Proxies.Models
{
  /// <summary>
  /// Describes geofence data returned by the geofence master data service.
  /// </summary>
  public class GeofenceData : IData
  {
    public bool IsFavorite { get; set; }

    public string GeofenceName { get; set; }

    public string Description { get; set; }

    public string GeofenceType { get; set; }

    public string GeometryWKT { get; set; }

    public int FillColor { get; set; }

    public bool IsTransparent { get; set; }

    public Guid CustomerUID { get; set; }

    public Guid GeofenceUID { get; set; }

    /// <summary>
    /// Key to use for caching geofence master data
    /// </summary>
    public string CacheKey
    {
      get { return GeofenceUID.ToString(); }
    }

  }
}
