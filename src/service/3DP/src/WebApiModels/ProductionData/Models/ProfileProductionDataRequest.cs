using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProfileProductionDataRequest : ProjectID
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    /// <summary>
    /// The type of profile to be generated.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "profileType", Required = Required.Always)]
    [Required]
    public ProductionDataType profileType { get; protected set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; protected set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long? filterID { get; protected set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "alignmentDesign", Required = Required.Default)]
    public DesignDescriptor alignmentDesign { get; protected set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "gridPoints", Required = Required.Default)]
    public ProfileGridPoints gridPoints { get; protected set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the WGS84 lat/lon coordinates. Values are expressed in radians.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "wgs84Points", Required = Required.Default)]
    public ProfileLLPoints wgs84Points { get; protected set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to start computing the profile from. Values are expressed in meters.
    /// </summary>
    /// 
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? startStation { get; protected set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    /// 
    [Range(ValidationConstants3D.MIN_STATION, ValidationConstants3D.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? endStation { get; protected set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; protected set; }

    /// <summary>
    /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "returnAllPassesAndLayers", Required = Required.Always)]
    [Required]
    public bool returnAllPassesAndLayers { get; protected set; }

    public static ProfileProductionDataRequest CreateProfileProductionData(
      long? projectID,
      Guid? callId,
      ProductionDataType profileType,
      FilterResult filter,
      long? filterID,
      DesignDescriptor alignmentDesign,
      ProfileGridPoints gridPoints,
      ProfileLLPoints wgs84Points,
      double startStation,
      double endStation,
      LiftBuildSettings liftBuildSettings,
      bool returnAllPassesAndLayers
      )
    {
      return new ProfileProductionDataRequest
      {
        ProjectId = projectID,
        callId = callId,
        profileType = profileType,
        filter = filter,
        filterID = filterID,
        alignmentDesign = alignmentDesign,
        gridPoints = gridPoints,
        wgs84Points = wgs84Points,
        startStation = startStation,
        endStation = endStation,
        liftBuildSettings = liftBuildSettings,
        returnAllPassesAndLayers = returnAllPassesAndLayers
      };
    }
    
    /// <summary>
    /// Returns whether this request is for an alignment design profile.
    /// </summary>
    public bool IsAlignmentDesign => alignmentDesign != null;

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      // Validate the profile type...
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

      if (alignmentDesign == null && gridPoints == null && wgs84Points == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Either a linear or alignment based profile must be provided."));
      }

      if (alignmentDesign != null && (gridPoints != null || wgs84Points != null))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Only one type, either a linear or alignment based profile must be provided."));
      }

      if (alignmentDesign != null)
      {
        // Validate alignment design parts...
        if (!startStation.HasValue || !endStation.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "If using an alignment design for profiling, the alignment file, start and end station must be provided."));
        }

        alignmentDesign.Validate();
      }
      else
      {
        // Validate profile points...
        if (gridPoints != null && wgs84Points != null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "The profile line requires series either grid or WGS84 points."));
        }

        if (gridPoints == null && wgs84Points == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "The profile line should be represented by series either grid or WGS84 points, not both."));
        }
      }

      liftBuildSettings?.Validate();
    }
  }
}
