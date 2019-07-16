using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.TRex.Gateway.Common.Requests
{
  /// <summary>
  /// The representation of a design filter boundary request.
  /// </summary>
  public class DesignFilterBoundaryRequest : DesignDataRequest
  {
    /// <summary>
    /// The starting station position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(Required = Required.Default)]
    public double StartStation { get; private set; }

    /// <summary>
    /// The ending station position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(Required = Required.Default)]
    public double EndStation { get; private set; }

    /// <summary>
    /// The left offset position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// This value may be negative, in which case it will be to the right of the alignment.
    /// </summary>
    [Range(ValidationConstants3D.MIN_OFFSET, ValidationConstants3D.MAX_OFFSET)]
    [JsonProperty(Required = Required.Default)]
    public double LeftOffset { get; private set; }

    /// <summary>
    /// The right offset position on a alignment being used as a spatial filter. The value is expressed in meters.
    /// This value may be negative, in which case it will be to the left of the alignment.
    /// </summary>
    [Range(ValidationConstants3D.MIN_OFFSET, ValidationConstants3D.MAX_OFFSET)]
    [JsonProperty(Required = Required.Default)]
    public double RightOffset { get; private set; }

    public DesignFilterBoundaryRequest(
      Guid projectUid, 
      Guid designUid, 
      string fileName, 
      double startStation,
      double endStation,
      double leftOffset,
      double rightOffset) : base (projectUid, designUid, fileName)
    {
      StartStation = startStation;
      EndStation = endStation;
      LeftOffset = leftOffset;
      RightOffset = rightOffset;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (StartStation > EndStation)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The start station must be less than the end station."));

      if (LeftOffset > RightOffset)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The left offset must be less than the right offset."));
    }
  }
}
