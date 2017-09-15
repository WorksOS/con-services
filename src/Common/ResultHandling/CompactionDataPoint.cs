using Newtonsoft.Json;

namespace VSS.Productivity3D.Common.ResultHandling
{
  /// <summary>
  /// One point of compaction profile data.
  /// </summary>
  public class CompactionDataPoint
  {
    /// <summary>
    /// The type of cell, either a cell edge intersection or the mid point of a segment cutting the cell. A edge can also be the start of a gap in the data.
    /// </summary>
    public ProfileCellType cellType;

    /// <summary>
    /// The station value, or distance from start of the profile line at which the profile line intersects this cell for cell edges 
    /// or the mid point of the line segment cutting through the cell for mid point type points.
    /// </summary>
    public double x;

    /// <summary>
    /// Elevation for profile cell for the type of data, e.g. CMV height for CMV, last pass height for elevation last pass etc.
    /// </summary>
    public float y;

    /// <summary>
    /// The value of the profile data for the type of data e.g. cut-fill, CMV, temperature etc. For speed it is the minimum speed value.
    /// </summary>
    public float value;

    /// <summary>
    /// For summary profile types, what the value represents with respect to the target. Used to select the color for the profile line segment.
    /// </summary>
    public ValueTargetType? valueType;

    /// <summary>
    /// For cut-fill profiles only, the design elevation of the cell.
    /// </summary>
    public float? y2;

    /// <summary>
    /// For speed summary profiles only, the maximum speed value.
    /// </summary>
    public float? value2;

    /// <summary>
    /// The type of profile this cell belongs to. Used to determine which properties to serialize.
    /// </summary>
    [JsonIgnore] public string type;

    /// <summary>
    /// Tell JSON serializer when to serialize property y2
    /// </summary>
    public bool ShouldSerializey2()
    {
      return type == "cutFill";
    }

    /// <summary>
    /// Tell JSON serializer when to serialize property value2
    /// </summary>
    public bool ShouldSerializevalue2()
    {
      return type == "speedSummary";
    }

    /// <summary>
    /// Tell JSON serializer when to serialize property valueType
    /// </summary>
    public bool ShouldSerializevalueType()
    {
      return type.ToLower().Contains("summary");
    }

    /// <summary>
    /// Default constructor
    /// </summary>
    public CompactionDataPoint()
    { }

    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="point">The point to copy</param>
    public CompactionDataPoint(CompactionDataPoint point)
    {
      cellType = point.cellType;
      x = point.x;
      y = point.y;
      value = point.value;
      valueType = point.valueType;
      y2 = point.y2;
      value2 = point.value2;
      type = point.type;
    }
  }
}
