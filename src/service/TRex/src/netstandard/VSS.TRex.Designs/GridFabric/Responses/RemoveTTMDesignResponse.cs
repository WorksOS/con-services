using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class RemoveTTMDesignResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public Guid DesignUid { get; set; }

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(DesignUid);
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      if (reader is null)
      {
        throw new ArgumentNullException(nameof(reader));
      }

      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      DesignUid = reader.ReadGuid() ?? Guid.Empty;
    }
  }
}
