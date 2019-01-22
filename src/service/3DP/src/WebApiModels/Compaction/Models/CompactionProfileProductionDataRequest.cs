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
    public DesignDescriptor cutFillDesignDescriptor { get; private set; }

    /// <summary>
    /// The base filter to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "topFilter", Required = Required.Default)]
    public FilterResult baseFilter { get; private set; }

    /// <summary>
    /// The top filter to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "topFilter", Required = Required.Default)]
    public FilterResult topFilter { get; private set; }

    /// <summary>
    /// The calculation type to use for a summary volumes profile. 
    /// </summary>
    [JsonProperty(PropertyName = "volumeCalcType", Required = Required.Default)]
    public VolumeCalcType? volumeCalcType { get; private set; }

    /// <summary>
    /// The design to use for a summary volumes profile
    /// </summary>
    [JsonProperty(PropertyName = "volumeDesignDescriptor", Required = Required.Default)]
    public DesignDescriptor volumeDesignDescriptor { get; private set; }



    public static CompactionProfileProductionDataRequest CreateCompactionProfileProductionDataRequest(
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
      bool returnAllPassesAndLayers,
      DesignDescriptor cutFillDesignDescriptor,
      FilterResult baseFilter,
      FilterResult topFilter,
      VolumeCalcType? volumeCalcType,
      DesignDescriptor volumeDesignDescriptor
    )
    {
      return new CompactionProfileProductionDataRequest
      {
        ProjectId = projectID,
        callId = callId,
        profileType = profileType,
        Filter = filter,
        filterID = filterID,
        alignmentDesign = alignmentDesign,
        gridPoints = gridPoints,
        wgs84Points = wgs84Points,
        startStation = startStation,
        endStation = endStation,
        liftBuildSettings = liftBuildSettings,
        returnAllPassesAndLayers = returnAllPassesAndLayers,
        cutFillDesignDescriptor = cutFillDesignDescriptor,
        baseFilter = baseFilter,
        topFilter = topFilter,
        volumeCalcType = volumeCalcType,
        volumeDesignDescriptor = volumeDesignDescriptor
      };
    }

    /// <summary>
    /// Validates the request and throws if validation fails.
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      baseFilter?.Validate();
      topFilter?.Validate();
      cutFillDesignDescriptor?.Validate();
      volumeDesignDescriptor?.Validate();

      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.None:
            break;
          case VolumeCalcType.GroundToGround:
            if (baseFilter == null || topFilter == null)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Missing filter(s) for summary volumes profile"));
            }
            break;
          case VolumeCalcType.GroundToDesign:
            if (baseFilter == null || volumeDesignDescriptor == null)
            {
              throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                  "Missing base filter and/or design for summary volumes profile"));
            }
            break;
          case VolumeCalcType.DesignToGround:
            if (volumeDesignDescriptor == null || topFilter == null)
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
