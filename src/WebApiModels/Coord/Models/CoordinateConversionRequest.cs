using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;

namespace VSS.Productivity3D.WebApiModels.Coord.Models
{
  /// <summary>
  /// Coordinate conversion domain object. Model represents a coordinate conversion request.
  /// </summary>
  public class CoordinateConversionRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// 2D coordinate conversion: 
    ///   0 - from Latitude/Longitude to North/East.
    ///   1 - from North/East to Latitude/Longitude.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "conversionType", Required = Required.Always)]
    [Required]
    public TwoDCoordinateConversionType conversionType { get; private set; }

    /// <summary>
    /// The list of coordinates for conversion.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "conversionCoordinates", Required = Required.Always)]
    [Required]
    public TwoDConversionCoordinate[] conversionCoordinates { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    /// 
    private CoordinateConversionRequest() 
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CoordinateConversionRequest class.
    /// </summary>
    /// <param name="projectId">The project to send the coordinate conversion request to.</param>
    /// <param name="conversionType">The coordinate conversion type (NE to LL or LL to NE).</param>
    /// <param name="conversionCoordinates">The array of coordinates to be converted.</param>
    /// <returns>An instance of the CoordinateConversionRequest class.</returns>
    /// 
    public static CoordinateConversionRequest CreateCoordinateConversionRequest(long projectId, TwoDCoordinateConversionType conversionType, TwoDConversionCoordinate[] conversionCoordinates)
    {
      CoordinateConversionRequest request = new CoordinateConversionRequest
                                            {
                                                ProjectId = projectId,
                                                conversionType = conversionType,
                                                conversionCoordinates = conversionCoordinates
                                            };

      return request;
    }
    
    /// <summary>
    /// Validation method.
    /// </summary>
    /// 
    public override void Validate()
    {
      base.Validate();
      if (conversionType == TwoDCoordinateConversionType.LatLonToNorthEast)
      {
        const double NINETY_DEGREES_IN_RADIANS = Math.PI/2;
        const double EPSILON = 10e-8;
 
        foreach (TwoDConversionCoordinate coord in conversionCoordinates)
        {
          // Check the Latitude value...
          if ((coord.y < -NINETY_DEGREES_IN_RADIANS - EPSILON) || (NINETY_DEGREES_IN_RADIANS + EPSILON < coord.y))
            throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                                string.Format("Latitude value of {0} is out of its valid range ({1}, {2}).", coord.y, -NINETY_DEGREES_IN_RADIANS, NINETY_DEGREES_IN_RADIANS)));

          // Check the Longitude value...
          if ((coord.x < -Math.PI - EPSILON) || (Math.PI + EPSILON < coord.x))
            throw new ServiceException(HttpStatusCode.BadRequest,
                            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                                string.Format("Longitude value of {0} is out of its valid range ({1}, {2}).", coord.x, -Math.PI, Math.PI)));
        }
      }
    }
  }
}
