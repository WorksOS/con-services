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

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(AlignmentUid);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        AlignmentUid = reader.ReadGuid() ?? Guid.Empty;
      }
    }
  }
}
