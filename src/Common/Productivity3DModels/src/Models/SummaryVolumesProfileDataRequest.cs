using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.Enums;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The representation of a summary volumes profile request
  /// </summary>
  public class SummaryVolumesProfileDataRequest : ProjectID
  {
    /// <summary>
    /// The base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult BaseFilter { get; private set; }

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
    /// The unique identifier of the design surface to be used in either filter to design or design to filter
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? ReferenceDesignUid { get; private set; }

    /// <summary>
    ///  Start grid position x or start lon
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double StartX;

    /// <summary>
    ///  Start grid position y or start lat
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double StartY;

    /// <summary>
    ///  End grid position y or end lat
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double EndX;

    /// <summary>
    ///  End grid position y or end lat
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double EndY;

    /// <summary>
    ///  Are positions grid or latlon
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public bool PositionsAreGrid;

    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryVolumesProfileDataRequest"/> class from being created.
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
      )
    {
      ProjectUid = projectUid;
      BaseFilter = baseFilter;
      TopFilter = topFilter;
      ReferenceDesignUid = referenceDesignUid;
      VolumeCalcType = volumeCalcType;
      PositionsAreGrid = positionsAreGrid;
      StartX = startX;
      StartY = startY;
      EndX = endX;
      EndY = endY;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public override void Validate()
    {
      TopFilter?.Validate();
      BaseFilter?.Validate();
    }
  }
}
