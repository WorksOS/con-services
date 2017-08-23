using System;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class DesignProfileProductionDataRequest : ProjectID
  {
    /// <summary>
    /// The type of profile to be generated.
    /// </summary>
    [JsonProperty(PropertyName = "profileType", Required = Required.Always)]
    [Required]
    public ProductionDataType profileType { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public Filter filter { get; private set; }

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
    [JsonProperty(PropertyName = "designDescriptor", Required = Required.Default)]
    public DesignDescriptor designDescriptor { get; private set; }

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
    [Range(ValidationConstants.MIN_STATION, ValidationConstants.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? startStation { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    [Range(ValidationConstants.MIN_STATION, ValidationConstants.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? endStation { get; private set; }

    /// <summary>
    /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
    /// </summary>
    [JsonProperty(PropertyName = "returnAllPassesAndLayers", Required = Required.Always)]
    [Required]
    public bool returnAllPassesAndLayers { get; private set; }

    /// <summary>
    /// Imported file UID for the design profile.
    /// </summary>
    [JsonProperty(PropertyName = "importedFileUid", Required = Required.Always)]
    [Required]
    public Guid importedFileUid { get; private set; }

    public static DesignProfileProductionDataRequest CreateProfileProductionData(long? projectID, Guid importedFileUid, ProductionDataType profileType, Filter filter, long? filterId, DesignDescriptor designDescriptor, ProfileGridPoints gridPoints, ProfileLLPoints wgs84Points, double startStation, double endStation)
    {
      return new DesignProfileProductionDataRequest
      {
        projectId = projectID,
        importedFileUid = importedFileUid,
        profileType = profileType,
        filter = filter,
        filterID = filterId,
        designDescriptor = designDescriptor,
        gridPoints = gridPoints,
        wgs84Points = wgs84Points,
        startStation = startStation,
        endStation = endStation
      };
    }

    /// <summary>
    /// Creates a sample instance of ProfileProductionDataRequest class to be displayed in the Help documentation.
    /// </summary>
    public new static DesignProfileProductionDataRequest HelpSample => new DesignProfileProductionDataRequest
    {
      projectId = 404,
      profileType = ProductionDataType.All,
      filter = Filter.HelpSample,
      filterID = 1,
      designDescriptor = DesignDescriptor.HelpSample,
      gridPoints = null,
      wgs84Points = ProfileLLPoints.HelpSample,
      startStation = 0,
      endStation = 100
    };

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (!Enumerable.Range((int)ProductionDataType.All, (int)ProductionDataType.CCVChange + 1).Contains((int)profileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              $"Profile type {(int)profileType} is out of range. It should be between: {(int)ProductionDataType.All} and {(int)ProductionDataType.CCVChange}."));
      }

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

      if (designDescriptor != null)
      {      
        designDescriptor.Validate();
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
    }
  }
}