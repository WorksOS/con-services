using System;
using Apache.Ignite.Core.Binary;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.Analytics.PassCountStatistics.GridFabric
{
  /// <summary>
  /// Argument containing the parameters required for a Pass Count statistics request
  /// </summary>    
  public class PassCountStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<PassCountStatisticsArgument>();

    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// Pass Count details values.
    /// </summary>
    public int[] PassCountDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      try
      {
        base.ToBinary(writer);

        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

        writer.WriteIntArray(PassCountDetailValues);
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in ToBinary()");
      }
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    public override void FromBinary(IBinaryRawReader reader)
    {
      try
      {
        base.FromBinary(reader);

        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

        PassCountDetailValues = reader.ReadIntArray();
      }
      catch (Exception e)
      {
        _log.LogError(e, "Exception in FromBinary()");
      }
    }
  }
}
