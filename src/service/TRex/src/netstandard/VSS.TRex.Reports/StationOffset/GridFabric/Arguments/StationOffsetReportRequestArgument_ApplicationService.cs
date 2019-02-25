using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Reports.Gridded;

namespace VSS.TRex.Reports.StationOffset.GridFabric.Arguments
{
  /// <summary>
  /// The argument to be supplied to the grid request
  /// </summary>
  public class StationOffsetReportRequestArgument_ApplicationService : BaseApplicationServiceRequestArgumentReport
  {
    /// <summary>
    /// This design contains the center line which will be sampled along
    /// </summary>
    public Guid AlignmentDesignUid { get; set; }

    /// <summary>
    /// The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.
    /// </summary>
    /// 
    public double CrossSectionInterval { get; set; }

    /// <summary>
    /// Start point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double StartStation { get; set; }

    /// <summary>
    /// End point along the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double EndStation { get; set; }

    /// <summary>
    /// Offsets left and right (or on) the center line in the AlignmentUid design
    /// </summary>
    /// 
    public double[] Offsets { get; set; } = new double[0];

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);
      writer.WriteGuid(AlignmentDesignUid);
      writer.WriteDouble(CrossSectionInterval);
      writer.WriteDouble(StartStation);
      writer.WriteDouble(EndStation);
      writer.WriteDoubleArray(Offsets);
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);
      AlignmentDesignUid = reader.ReadGuid() ?? Guid.Empty;
      CrossSectionInterval = reader.ReadDouble();
      StartStation = reader.ReadDouble();
      EndStation = reader.ReadDouble();
      Offsets = reader.ReadDoubleArray();
    }
  }
}
