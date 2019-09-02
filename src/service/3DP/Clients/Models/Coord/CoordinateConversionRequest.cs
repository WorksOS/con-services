using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Productivity3D.Models.Coord
{
  /// <summary>
  /// Coordinate conversion domain object. Model represents a coordinate conversion request.
  /// </summary>
  public class CoordinateConversionRequest : ProjectID
  {

    /// <summary>
    /// 2D coordinate conversion: 
    ///   0 - from Latitude/Longitude to North/East.
    ///   1 - from North/East to Latitude/Longitude.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "conversionType", Required = Required.Always)]
    [Required]
    public TwoDCoordinateConversionType ConversionType { get; private set; }

    /// <summary>
    /// The list of coordinates for conversion.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "conversionCoordinates", Required = Required.Always)]
    [Required]
    public TwoDConversionCoordinate[] ConversionCoordinates { get; private set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    /// 
    private CoordinateConversionRequest() 
    {
      // ...
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectId">The project to send the coordinate conversion request to.</param>
    /// <param name="conversionType">The coordinate conversion type (NE to LL or LL to NE).</param>
    /// <param name="conversionCoordinates">The array of coordinates to be converted.</param>
    /// <returns>An instance of the CoordinateConversionRequest class.</returns>
    /// 
    public CoordinateConversionRequest(long projectId, TwoDCoordinateConversionType conversionType, TwoDConversionCoordinate[] conversionCoordinates)
    {
      ProjectId = projectId;
      ConversionType = conversionType;
      ConversionCoordinates = conversionCoordinates;
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid">The project's unique identifier to send the coordinate conversion request to.</param>
    /// <param name="conversionType">The coordinate conversion type (NE to LL or LL to NE).</param>
    /// <param name="conversionCoordinates">The array of coordinates to be converted.</param>
    /// <returns>An instance of the CoordinateConversionRequest class.</returns>
    /// 
    public CoordinateConversionRequest(Guid projectUid, TwoDCoordinateConversionType conversionType, TwoDConversionCoordinate[] conversionCoordinates)
    {
      ProjectUid = projectUid;
      ConversionType = conversionType;
      ConversionCoordinates = conversionCoordinates;
    }

    /// <summary>
    /// Validation method.
    /// </summary>
    /// 
    public override void Validate()
    {
      base.Validate();
      if (ConversionType == TwoDCoordinateConversionType.LatLonToNorthEast)
      {
        const double NINETY_DEGREES_IN_RADIANS = Math.PI/2;
        const double EPSILON = 10e-8;
 
        foreach (TwoDConversionCoordinate coord in ConversionCoordinates)
        {
          // Check the Latitude value...
          if ((coord.Y < -NINETY_DEGREES_IN_RADIANS - EPSILON) || (NINETY_DEGREES_IN_RADIANS + EPSILON < coord.Y))
            throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                                string.Format("Latitude value of {0} is out of its valid range ({1}, {2}).", coord.Y, -NINETY_DEGREES_IN_RADIANS, NINETY_DEGREES_IN_RADIANS)));

          // Check the Longitude value...
          if ((coord.X < -Math.PI - EPSILON) || (Math.PI + EPSILON < coord.X))
            throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                                string.Format("Longitude value of {0} is out of its valid range ({1}, {2}).", coord.X, -Math.PI, Math.PI)));
        }
      }
    }
  }
}
