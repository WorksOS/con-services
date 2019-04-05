using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Records;
using VSS.TRex.GridFabric.Arguments;
using VSS.TRex.Types;

namespace VSS.TRex.Analytics.TemperatureStatistics.GridFabric
{
	/// <summary>
	/// Argument containing the parameters required for a Temperature statistics request
	/// </summary>    
	public class TemperatureStatisticsArgument : BaseApplicationServiceRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    /// <summary>
    /// The flag is to indicate whether or not the temperature warning levels to be user overrides.
    /// </summary>
    public bool OverrideTemperatureWarningLevels { get; set; }

    /// <summary>
    /// User overriding temperature warning level values.
    /// </summary>
    public TemperatureWarningLevelsRecord OverridingTemperatureWarningLevels;

    /// <summary>
    /// Temperature details values.
    /// </summary>
    public int[] TemperatureDetailValues { get; set; }

    /// <summary>
    /// Serialises content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(OverrideTemperatureWarningLevels);

      OverridingTemperatureWarningLevels.ToBinary(writer);

      writer.WriteIntArray(TemperatureDetailValues);
    }

    /// <summary>
    /// Serialises content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      OverrideTemperatureWarningLevels = reader.ReadBoolean();

      OverridingTemperatureWarningLevels.FromBinary(reader);

      TemperatureDetailValues = reader.ReadIntArray();
    }
  }
}
