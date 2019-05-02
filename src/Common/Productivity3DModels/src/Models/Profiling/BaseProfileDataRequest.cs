using System;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Models.Models.Profiling
{
  /// <summary>
  /// The representation of a profile request.
  /// </summary>
  public class BaseProfileDataRequest : ProjectID
  {
    /// <summary>
    /// The base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult BaseFilter { get; private set; }

    /// <summary>
    /// The unique identifier of the design surface to be used in either filter to design or design to filter
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? ReferenceDesignUid { get; private set; }

    /// <summary>
    /// The offset if the design surface is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? ReferenceDesignOffset { get; private set; }

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
    /// Default public constructor.
    /// </summary>
    public BaseProfileDataRequest()
    { }


    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="baseFilter"></param>
    /// <param name="referenceDesignUid"></param>
    /// <param name="referenceDesignOffset"></param>
    /// <param name="positionsAreGrid"></param>
    /// <param name="startX"></param>
    /// <param name="startY"></param>
    /// <param name="endX"></param>
    /// <param name="endY"></param>
    public BaseProfileDataRequest(
      Guid projectUid,
      FilterResult baseFilter,
      Guid? referenceDesignUid,
      double? referenceDesignOffset,
      bool positionsAreGrid,
      double startX,
      double startY,
      double endX,
      double endY)
    {
      ProjectUid = projectUid;
      BaseFilter = baseFilter;
      ReferenceDesignUid = referenceDesignUid;
      ReferenceDesignOffset = referenceDesignOffset;
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
      BaseFilter?.Validate();
    }
  }
}
