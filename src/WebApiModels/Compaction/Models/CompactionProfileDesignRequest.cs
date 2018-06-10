using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class CompactionProfileDesignRequest : ProjectID
  {
    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public long? filterID { get; private set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "alignmentDescriptor", Required = Required.Default)]
    public DesignDescriptor alignmentDescriptor { get; private set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "gridPoints", Required = Required.Default)]
    public ProfileGridPoints gridPoints { get; private set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "wgs84Points", Required = Required.Default)]
    public ProfileLLPoints wgs84Points { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? startStation { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? endStation { get; private set; }

    /// <summary>
    /// The descriptor for the design for which to to generate the profile.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Always)]
    public DesignDescriptor designDescriptor { get; private set; }

    public static CompactionProfileDesignRequest CreateCompactionProfileDesignRequest(long projectId, DesignDescriptor designDescriptor, FilterResult filter, long? filterId, DesignDescriptor alignmentDescriptor, ProfileGridPoints gridPoints, ProfileLLPoints wgs84Points, double startStation, double endStation)
    {
      return new CompactionProfileDesignRequest
      {
        ProjectId = projectId,
        designDescriptor = designDescriptor,
        filter = filter,
        filterID = filterId,
        alignmentDescriptor = alignmentDescriptor,
        gridPoints = gridPoints,
        wgs84Points = wgs84Points,
        startStation = startStation,
        endStation = endStation
      };
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (filter != null)
      {
        filter.Validate();

        if (filterID.HasValue && filterID.Value <= 0)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Filter ID {filterID.Value} should be greater than zero."));
        }
      }

      if (alignmentDescriptor != null)
      {
        alignmentDescriptor.Validate();
      }
      else
      {
        if (gridPoints != null && wgs84Points != null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Only one linear or alignment based profile must be provided."));
        }

        if (gridPoints == null && wgs84Points == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Either a design descriptor or linear or alignment based profile must be provided."));
        }
      }

      if (designDescriptor != null)
      {
        designDescriptor.Validate();
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Design must be specified for a design profile"));
      }
    }
  }
}
