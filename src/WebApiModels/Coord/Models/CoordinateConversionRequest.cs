using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Coord.Models
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
    public TwoDCoordinateConversionType conversionType { get; private set; }

    /// <summary>
    /// The list of coordinates for conversion.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "conversionCoordinates", Required = Required.Always)]
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
    /// CoordinateConversionRequest sample instance.
    /// </summary>
    /// 
    public new static CoordinateConversionRequest HelpSample
    {
      get { return new CoordinateConversionRequest() 
              { projectId = 1, 
                conversionType = TwoDCoordinateConversionType.NorthEastToLatLon, 
                conversionCoordinates = new TwoDConversionCoordinate[]
                                        {
                                            TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381043.710, 807625.050),
                                            TwoDConversionCoordinate.CreateTwoDConversionCoordinate(381821.617, 807359.462),
                                            TwoDConversionCoordinate.CreateTwoDConversionCoordinate(380781.358, 806969.174),
                                        } 
              }; 
          }
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
                                                projectId = projectId,
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