using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.CMVChangeStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a CMV change statistics request.
  /// The CMV change is exposed on the client as CMV % change.
  /// </summary>    
  public class CMVChangeStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    /// <summary>
    /// CMV change details values.
    /// </summary>
    public double[] CMVChangeDetailsDatalValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      writer.WriteDoubleArray(CMVChangeDetailsDatalValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      CMVChangeDetailsDatalValues = reader.ReadDoubleArray();
    }
  }
}
