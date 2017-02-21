using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.Raptor.Service.Common.Utilities;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProfileProductionDataRequest : ProjectID, IValidatable
  {
    /// <summary>
    /// An identifying string from the caller
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; private set; }

    /// <summary>
    /// The type of profile to be generated.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "profileType", Required = Required.Always)]
    public ProductionDataType profileType { get; private set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public Filter filter { get; private set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long? filterID { get; private set; }

    /// <summary>
    /// The descriptor for an alignment centerline design to be used as the geometry along which the profile is generated
    /// Value may be null.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "alignmentDesign", Required = Required.Default)]
    public DesignDescriptor alignmentDesign { get; private set; }

    /// <summary>
    /// A series of points along which to generate the profile. Coordinates are expressed in terms of the grid coordinate system used by the project. Values are expressed in meters.
    /// Value may be null.
    /// </summary>
    /// 
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
    /// 
    [Range(ValidationConstants.MIN_STATION, ValidationConstants.MAX_STATION)]
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? startStation { get; private set; }

    /// <summary>
    /// The station on an alignment centerline design (if one is provided) to finish computing the profile at. Values are expressed in meters.
    /// </summary>
    /// 
    [Range(ValidationConstants.MIN_STATION, ValidationConstants.MAX_STATION)]
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? endStation { get; private set; }

    /// <summary>
    /// The set of parameters and configuration information relevant to analysis of compaction material layers information for related profile queries.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// Return all analysed layers and cell passes along with the summary cell based results of the profile query
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "returnAllPassesAndLayers", Required = Required.Always)]
    public bool returnAllPassesAndLayers { get; private set; }

    /// <summary>
    /// Private constructor.
    /// </summary>
    public ProfileProductionDataRequest()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of ProfileProductionDataRequest class.
    /// </summary>
    /// <param name="projectID"></param>
    /// <param name="callId"></param>
    /// <param name="profileType"></param>
    /// <param name="filter"></param>
    /// <param name="filterID"></param>
    /// <param name="alignmentDesign"></param>
    /// <param name="gridPoints"></param>
    /// <param name="wgs84Points"></param>
    /// <param name="startStation"></param>
    /// <param name="endStation"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="returnAllPassesAndLayers"></param>
    /// <returns></returns>
    /// 
    public static ProfileProductionDataRequest CreateProfileProductionData(
      long projectID,
      Guid? callId,
      ProductionDataType profileType,
      Filter filter,
      long filterID,
      DesignDescriptor alignmentDesign,
      ProfileGridPoints gridPoints,
      ProfileLLPoints wgs84Points,
      double startStation,
      double endStation,
      LiftBuildSettings liftBuildSettings,
      bool returnAllPassesAndLayers
      )
    {
      return new ProfileProductionDataRequest()
        {
          projectId = projectID,
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
    /// Creates a sample instance of ProfileProductionDataRequest class to be displayed in the Help documentation.
    /// </summary>
    /// <returns></returns>
    /// 
    public new static ProfileProductionDataRequest HelpSample
    {
      get
      {
        return new ProfileProductionDataRequest()
        {
          projectId = 404,
          callId = new Guid(),
          profileType = ProductionDataType.All,
          filter = Filter.HelpSample,
          filterID = 1,
          alignmentDesign = DesignDescriptor.HelpSample,
          gridPoints = null,
          wgs84Points = ProfileLLPoints.HelpSample,
          startStation = 0,
          endStation = 100,
          liftBuildSettings = LiftBuildSettings.HelpSample,
          returnAllPassesAndLayers = true
        };
      }
    }


    /// <summary>
    /// Validation method.
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      // Validate the profile type...
      if (!Enumerable.Range((int)ProductionDataType.All, (int)ProductionDataType.CCVChange + 1).Contains((int)profileType))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                string.Format("Profile type {0} is out of range. It should be between: {1} and {2}.", (int)profileType, (int)ProductionDataType.All, (int)ProductionDataType.CCVChange)));
      }
            
      if (filter != null)
      {
        // Validate filter...
        filter.Validate();

        if (filterID.HasValue && (filterID.Value) <= 0)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  string.Format("Filter ID {0} should be greater than zero.", filterID.Value)));
        }
      }

      if ((alignmentDesign == null) && (gridPoints == null) && (wgs84Points == null))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Either a linear or alignment based profile must be provided."));
      }

      if ((alignmentDesign != null) && ((gridPoints != null) || (wgs84Points != null)))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                "Only one type, either a linear or alignment based, of profile must be provided."));
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
        if ((gridPoints != null) && (wgs84Points != null))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "The profile line requires series either grid or WGS84 points."));
        }

        if ((gridPoints == null) && (wgs84Points == null))
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "The profile line should be represented by series either grid or WGS84 points, not both."));
        }
      }

      if (liftBuildSettings != null)
      {
        // Validate compaction settings...
        liftBuildSettings.Validate();
      }
    }


  }  
}