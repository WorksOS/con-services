using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.Volumes.GridFabric.Responses
{
  public class ProgressiveVolumesResponse : SubGridsPipelinedResponseBase, IAggregateWith<ProgressiveVolumesResponse>
  {
    private const byte VERSION_NUMBER = 1;

    public ProgressiveVolumeResponseItem[] Volumes { get; set; }

    public ProgressiveVolumesResponse AggregateWith(ProgressiveVolumesResponse other)
    {
      if ((Volumes?.Length ?? 0) != (other.Volumes?.Length ?? 0))
      {
        throw new ArgumentException($"Progressive volumes series should have same length: {Volumes?.Length ?? 0} versus {other.Volumes?.Length ?? 0}");
      }

      // Iterate over each progressive volume in turn and aggregate them together

      for (var i = 0; i < Volumes.Length; i++)
      {
        if (Volumes[i].Date.CompareTo(other.Volumes[i].Date) != 0)
        {
          throw new ArgumentException($"Dates of aggregating progressive volume pair are not the same: {Volumes[i].Date} versus {other.Volumes[i].Date}");
        }

        Volumes[i].AggregateWith(other.Volumes[i]);
      }

      return this;
    }

    /// <summary>
    /// Serializes content to the writer
    /// </summary>
    /// <param name="writer"></param>
    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      var count = Volumes?.Length ?? 0;
      writer.WriteInt(count);
      for (var i = 0; i < count; i++)
      {
        Volumes[i].ToBinary(writer);
      }
    }

    /// <summary>
    /// Serializes content from the writer
    /// </summary>
    /// <param name="reader"></param>
    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      var count = reader.ReadInt();
      Volumes = new ProgressiveVolumeResponseItem[count];
      for (var i = 0; i < count; i++)
      {
        (Volumes[i] = new ProgressiveVolumeResponseItem()).FromBinary(reader);
      }
    }
  }
}
