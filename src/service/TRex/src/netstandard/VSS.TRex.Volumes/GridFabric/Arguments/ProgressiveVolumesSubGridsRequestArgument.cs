using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SubGrids.GridFabric.Arguments;
using VSS.TRex.SubGrids.Interfaces;
using VSS.TRex.Volumes.Interfaces;

namespace VSS.TRex.Volumes.GridFabric.Arguments
{
  public class ProgressiveVolumesSubGridsRequestArgument : SubGridsRequestArgument, IProgressiveVolumesSubGridsRequestArgument
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
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(StartDate.ToBinary());
      writer.WriteLong(EndDate.ToBinary());
      writer.WriteLong(Interval.Ticks);

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
        StartDate = DateTime.FromBinary(reader.ReadLong());
        EndDate = DateTime.FromBinary(reader.ReadLong());
        Interval = TimeSpan.FromTicks(reader.ReadLong());
      }
    }
  }
}
