using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.MDPStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a MDP statistics request
  /// </summary>    
  public class MDPStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// MDP details values.
    /// </summary>
    public int[] MDPDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteIntArray(MDPDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        MDPDetailValues = reader.ReadIntArray();
      }
    }
  }
}
