using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Models;
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


    public static CompactionProfileProductionDataRequest CreateCompactionProfileProductionDataRequest(
      long? projectID,
      Guid? callId,
      ProductionDataType profileType,
      Filter filter,
      long? filterID,
      DesignDescriptor alignmentDesign,
      ProfileGridPoints gridPoints,
      ProfileLLPoints wgs84Points,
      double startStation,
      double endStation,
      LiftBuildSettings liftBuildSettings,
      bool returnAllPassesAndLayers,
      DesignDescriptor cutFillDesignDescriptor
    )
    {
      return new CompactionProfileProductionDataRequest
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
        returnAllPassesAndLayers = returnAllPassesAndLayers,
        cutFillDesignDescriptor = cutFillDesignDescriptor
      };
    }
  }
}
