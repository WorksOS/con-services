using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Cut/Fill statistics request
  /// </summary>    
  public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// The set of cut/fill offsets
    /// Current this is always 7 elements in array and assumes grade is set at zero
    /// eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
    /// </summary>
    public double[] Offsets { get; set; }

    /// <summary>
    /// The ID of the design to compute cut fill values between it and the production data elevatoins
    /// </summary>
    public Guid DesignID { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDoubleArray(Offsets);
      writer.WriteGuid(DesignID);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      Offsets = reader.ReadDoubleArray();
      DesignID = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
