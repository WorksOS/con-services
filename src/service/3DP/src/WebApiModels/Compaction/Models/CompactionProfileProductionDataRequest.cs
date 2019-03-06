using System;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Models
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile for compaction.
  /// </summary>
  public class CompactionProfileProductionDataRequest : ProfileProductionDataRequest
  {
    /// <summary>
    /// The design to use for a cut fill profile
    /// </summary>
    [JsonProperty(PropertyName = "cutFillDesignDescriptor", Required = Required.Default)]
    public DesignDescriptor CutFillDesignDescriptor { get; private set; }

    /// <summary>
    /// The base filter to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "baseFilter", Required = Required.Default)]
    public FilterResult BaseFilter { get; private set; }

    /// <summary>
    /// The top filter to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "topFilter", Required = Required.Default)]
    public FilterResult TopFilter { get; private set; }

    /// <summary>
    /// The calculation type to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "volumeCalcType", Required = Required.Default)]
    public VolumeCalcType? VolumeCalcType { get; private set; }

    /// <summary>
    /// The design to use for a summary volumes profile
    /// </summary>
    [JsonProperty(PropertyName = "volumeDesignDescriptor", Required = Required.Default)]
    public DesignDescriptor VolumeDesignDescriptor { get; private set; }

    [JsonIgnore]
    public bool ExplicitFilters { get; set; }



    public CompactionProfileProductionDataRequest(
      long? projectID,
      Guid? projectUid,
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
      bool returnAllPassesAndLayers,
      DesignDescriptor cutFillDesignDescriptor,
      FilterResult baseFilter,
      FilterResult topFilter,
      VolumeCalcType? volumeCalcType,
      DesignDescriptor volumeDesignDescriptor,
      bool explicitFilters = false
    )
    {
      ProjectId = projectID;
      ProjectUid = projectUid;
      CallId = callId;
      ProfileType = profileType;
      Filter = filter;
      FilterID = filterID;
      AlignmentDesign = alignmentDesign;
      GridPoints = gridPoints;
      WGS84Points = wgs84Points;
      base.StartStation = startStation;
      EndStation = endStation;
      LiftBuildSettings = liftBuildSettings;
      ReturnAllPassesAndLayers = returnAllPassesAndLayers;
      CutFillDesignDescriptor = cutFillDesignDescriptor;
      BaseFilter = baseFilter;
      TopFilter = topFilter;
      VolumeCalcType = volumeCalcType;
      VolumeDesignDescriptor = volumeDesignDescriptor;
      ExplicitFilters = explicitFilters;
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      BaseFilter?.Validate();
      TopFilter?.Validate();
      CutFillDesignDescriptor?.Validate();
      VolumeDesignDescriptor?.Validate();

      if (VolumeCalcType.HasValue)
      {
        switch (VolumeCalcType.Value)
        {
          case MasterData.Models.Models.VolumeCalcType.None:
            break;
          case MasterData.Models.Models.VolumeCalcType.GroundToGround:
            if (BaseFilter == null || TopFilter == null)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Missing filter(s) for summary volumes profile"));
            }
            break;
          case MasterData.Models.Models.VolumeCalcType.GroundToDesign:
            if (BaseFilter == null || VolumeDesignDescriptor == null)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Missing base filter and/or design for summary volumes profile"));
            }
            break;
          case MasterData.Models.Models.VolumeCalcType.DesignToGround:
            if (VolumeDesignDescriptor == null || TopFilter == null)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Missing design and/or top filter for summary volumes profile"));
            }
            break;
        }
      }
    }
  }
}
