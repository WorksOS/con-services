using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CutFillStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Cut/Fill statistics request
  /// </summary>    
  public class CutFillStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The set of cut/fill offsets
    /// Current this is always 7 elements in array and assumes grade is set at zero
    /// eg: 0.5, 0.2, 0.1, 0.0, -0.1, -0.2, -0.5
    /// </summary>
    public double[] Offsets { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteDoubleArray(Offsets);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      Offsets = reader.ReadDoubleArray();
    }
  }
}
