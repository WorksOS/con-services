using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Types;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgumentReport
  {
    /// <summary>
    /// Points in NEE derived from each station & offset
    /// </summary>
    /// 
    public List<StationOffsetPoint> Points { get; set; } = new List<StationOffsetPoint>();

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteInt(Points.Count);
      foreach (var point in Points)
      {
        point.ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      var pointCount = reader.ReadInt();
      Points = new List<StationOffsetPoint>();
      for (int i = 0; i < pointCount; i++)
      {
        StationOffsetPoint point = null;
        (point = new StationOffsetPoint()).FromBinary(reader);
        Points.Add(point);
      }
    }
    public override int GetHashCode()
    {
      unchecked
      {
        int hashCode = base.GetHashCode();
        hashCode = (hashCode * 397) ^ (Points != null ? Points.GetHashCode() : 0);
        return hashCode;
      }
    }
  }
}
