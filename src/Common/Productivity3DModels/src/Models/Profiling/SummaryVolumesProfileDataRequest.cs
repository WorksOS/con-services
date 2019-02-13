using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.Models.Profiling
{
  /// <summary>
  /// The representation of a summary volumes profile request
  /// </summary>
  public class SummaryVolumesProfileDataRequest : BaseProfileDataRequest
  {
    /// <summary>
    /// The top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult TopFilter { get; private set; }

    /// <summary>
    /// The type of volume computation to be performed as a summary volumes request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public VolumesType VolumeCalcType { get; private set; }
    
    /// <summary>
    /// Default public constructor.
    /// </summary>
    public SummaryVolumesProfileDataRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="baseFilter"></param>
    /// <param name="topFilter"></param>
    /// <param name="referenceDesignUid"></param>
    /// <param name="volumeCalcType"></param>
    /// <param name="positionsAreGrid"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    public SummaryVolumesProfileDataRequest(
      Guid projectUid, 
      FilterResult baseFilter, 
      FilterResult topFilter, 
      Guid? referenceDesignUid, 
      VolumesType volumeCalcType,
      bool positionsAreGrid,
      double  startX,
      double startY,
      double endX,
      double endY
      ) : base (projectUid, baseFilter, referenceDesignUid, positionsAreGrid, startX, startY, endX, endY)
    {
      TopFilter = topFilter;
      VolumeCalcType = volumeCalcType;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      BaseFilter?.Validate();
    }
  }
}
