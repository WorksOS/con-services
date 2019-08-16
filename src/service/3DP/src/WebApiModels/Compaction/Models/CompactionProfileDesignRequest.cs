using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Productivity3D.Models.Utilities;
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
    public FilterResult Filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public long? FilterID { get; private set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "alignmentDescriptor", Required = Required.Default)]
    public DesignDescriptor AlignmentDescriptor { get; private set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "gridPoints", Required = Required.Default)]
    public ProfileGridPoints GridPoints { get; private set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "wgs84Points", Required = Required.Default)]
    public ProfileLLPoints WGS84Points { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? StartStation { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? EndStation { get; private set; }

    /// <summary>
    /// The descriptor for the design for which to to generate the profile.
    /// </summary>
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Always)]
    public DesignDescriptor DesignDescriptor { get; private set; }

    public CompactionProfileDesignRequest(long projectId, Guid? projectUid, DesignDescriptor designDescriptor, FilterResult filter, long? filterId, DesignDescriptor alignmentDescriptor, ProfileGridPoints gridPoints, ProfileLLPoints wgs84Points, double startStation, double endStation)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      DesignDescriptor = designDescriptor;
      Filter = filter;
      FilterID = filterId;
      AlignmentDescriptor = alignmentDescriptor;
      GridPoints = gridPoints;
      WGS84Points = wgs84Points;
      StartStation = startStation;
      EndStation = endStation;
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (Filter != null)
      {
        Filter.Validate();

        if (FilterID.HasValue && FilterID.Value <= 0)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                $"Filter ID {FilterID.Value} should be greater than zero."));
        }
      }

      if (AlignmentDescriptor != null)
      {
        AlignmentDescriptor.Validate();
      }
      else
      {
        if (GridPoints != null && WGS84Points != null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Only one linear or alignment based profile must be provided."));
        }

        if (GridPoints == null && WGS84Points == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Either a design descriptor or linear or alignment based profile must be provided."));
        }
      }

      if (DesignDescriptor != null)
      {
        DesignDescriptor.Validate();
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
