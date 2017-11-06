using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Extensions;
using VSS.Productivity3D.Common.Interfaces;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// Defines a bounding box representing a WGS84 latitude/longitude coordinate area 
  /// </summary>
  public class BoundingBox2DLatLon : IValidatable
  {
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "bottomLeftLon", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    public double bottomLeftLon { get; private set; }

    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "bottomLeftLat", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI / 2, Math.PI / 2)]
    public double bottomLeftLat { get; private set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "topRightLon", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    public double topRightLon { get; private set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "topRightLat", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI / 2, Math.PI / 2)]
    public double topRightLat { get; private set; }


    /// <summary>
    /// Private constructor
    /// </summary>
    private BoundingBox2DLatLon()
    { }

    /// <summary>
    /// Create instance of BoundingBox2DLatLon
    /// </summary>
    public static BoundingBox2DLatLon CreateBoundingBox2DLatLon(
        double blLon,
        double blLat,
        double trLon,
        double trLat
        )
    {
      return new BoundingBox2DLatLon
      {
        bottomLeftLon = blLon,
        bottomLeftLat = blLat,
        topRightLon = trLon,
        topRightLat = trLat
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (bottomLeftLon > topRightLon || bottomLeftLat > topRightLat)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Invalid bounding box: corners are not bottom left and top right."));
      }
    }

    //Convenience properties for map tiles for reports
    [JsonIgnore]
    public double centerLatInDecimalDegrees => (bottomLeftLat + (topRightLat - bottomLeftLat) / 2.0).latRadiansToDegrees();
    [JsonIgnore]
    public double centerLngInDecimalDegrees => (bottomLeftLon + (topRightLon - bottomLeftLon) / 2.0).lonRadiansToDegrees();
    [JsonIgnore]
    public double minLatInDecimalDegrees => Math.Min(bottomLeftLat, topRightLat).latRadiansToDegrees();
    [JsonIgnore]
    public double maxLatInDecimalDegrees => Math.Max(bottomLeftLat, topRightLat).latRadiansToDegrees();
    [JsonIgnore]
    public double minLngInDecimalDegrees => Math.Min(bottomLeftLon, topRightLon).lonRadiansToDegrees();
    [JsonIgnore]
    public double maxLngInDecimalDegrees => Math.Max(bottomLeftLon, topRightLon).lonRadiansToDegrees();

    /// <summary>
    /// Calculates the zoom level from the bounding box
    /// </summary>
    /// <returns>The zoom level</returns>
    public int CalculateZoomLevel()
    {
      const int MAXZOOM = 24;

      double selectionLatSize = Math.Abs(topRightLat - bottomLeftLat);
      double selectionLongSize = Math.Abs(topRightLon - bottomLeftLon);

      //Google maps zoom level starts at 0 for whole world (-90.0 to 90.0, -180.0 to 180.0)
      //and doubles the precision both horizontally and vertically for each suceeding level.
      int zoomLevel = 0;
      double latSize = Math.PI; //180.0;
      double longSize = 2 * Math.PI; //360.0;
      while (latSize > selectionLatSize && longSize > selectionLongSize && zoomLevel < MAXZOOM)
      {
        zoomLevel++;
        latSize /= 2;
        longSize /= 2;
      }
      return zoomLevel;
    }
  }
}