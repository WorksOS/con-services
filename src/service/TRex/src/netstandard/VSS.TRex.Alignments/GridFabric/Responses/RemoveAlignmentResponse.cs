using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.Alignments.GridFabric.Responses
{
  public class RemoveAlignmentResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public Guid AlignmentUid { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(AlignmentUid);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      if (reader is null)
      {
        throw new ArgumentNullException(nameof(reader));
      }

      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      AlignmentUid = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
