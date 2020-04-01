using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SubGrids.GridFabric.Arguments;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  public class ProgressiveVolumesSubGridsRequestArgument : SubGridsRequestArgument
  {
    private const byte VERSION_NUMBER = 1;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public TimeSpan Interval { get; set; }

    /// <summary>
    /// Default no-arg constructor
    /// </summary>
    public ProgressiveVolumesSubGridsRequestArgument()
    {
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(StartDate.ToBinary());
      writer.WriteLong(EndDate.ToBinary());
      writer.WriteLong(Interval.Ticks);

    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      StartDate = DateTime.FromBinary(reader.ReadLong());
      EndDate = DateTime.FromBinary(reader.ReadLong());
      Interval = TimeSpan.FromTicks(reader.ReadLong());
    }
  }
}
