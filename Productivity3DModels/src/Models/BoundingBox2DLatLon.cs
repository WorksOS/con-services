using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// Defines a bounding box representing a WGS84 latitude/longitude coordinate area 
  /// </summary>
  public class BoundingBox2DLatLon 
  {
    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "bottomLeftLon", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    public double BottomLeftLon { get; private set; }

    /// <summary>
    /// The bottom left corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "bottomLeftLat", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI / 2, Math.PI / 2)]
    public double BottomLeftLat { get; private set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "topRightLon", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI, Math.PI)]
    public double TopRightLon { get; private set; }

    /// <summary>
    /// The top right corner of the bounding box, expressed in radians
    /// </summary>
    [JsonProperty(PropertyName = "topRightLat", Required = Required.Always)]
    [Required]
    [DecimalIsWithinRange(-Math.PI / 2, Math.PI / 2)]
    public double TopRightLat { get; private set; }


    /// <summary>
    /// Default private constructor
    /// </summary>
    private BoundingBox2DLatLon()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="blLon"></param>
    /// <param name="blLat"></param>
    /// <param name="trLon"></param>
    /// <param name="trLat"></param>
    public BoundingBox2DLatLon
    (
      double blLon,
      double blLat,
      double trLon,
      double trLat
    )
    {
      BottomLeftLon = blLon;
      BottomLeftLat = blLat;
      TopRightLon = trLon;
      TopRightLat = trLat;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
      if (BottomLeftLon > TopRightLon || BottomLeftLat > TopRightLat)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Invalid bounding box: corners are not bottom left and top right."));
      }
    }
  }
}