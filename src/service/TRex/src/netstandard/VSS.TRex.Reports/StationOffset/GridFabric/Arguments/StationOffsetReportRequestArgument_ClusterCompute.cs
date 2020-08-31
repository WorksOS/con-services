using System.Collections.Generic;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Models;
using VSS.TRex.Common.Types;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ClusterCompute : BaseApplicationServiceRequestArgumentReport
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Points in NEE derived from each station & offset
    /// </summary>
    /// 
    public List<StationOffsetPoint> Points { get; set; } = new List<StationOffsetPoint>();

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteInt(Points.Count);
      foreach (var point in Points)
      {
        point.ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        var pointCount = reader.ReadInt();
        Points = new List<StationOffsetPoint>();
        for (int i = 0; i < pointCount; i++)
        {
          StationOffsetPoint point = null;
          (point = new StationOffsetPoint()).FromBinary(reader);
          Points.Add(point);
        }
      }
    }
  }
}
