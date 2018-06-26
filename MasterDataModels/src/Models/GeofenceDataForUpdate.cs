using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Describes geofence data to be used to Update a geofence in the GeofenceSvc
  /// </summary>
  public class GeofenceDataForUpdate
  {
    //
    //
    // GET: /UpdateGeofenceEvent/
    public string GeofenceName { get; set; }


    public string Description { get; set; }


    public string GeofenceType { get; set; }


    // Will be uncommented once CG is fixed
    //[GeometryWKTValidator]
    public string GeometryWKT { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public double? AreaSqMeters { get; set; }

    public int? FillColor { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? IsTransparent { get; set; }

    public Guid UserUID { get; set; }

    [Required]
    public Guid GeofenceUID { get; set; }

    public DateTime ActionUTC { get; set; }

    public DateTime ReceivedUTC { get; set; }

    public DateTime? EndDate { get; set; }

  }
}
