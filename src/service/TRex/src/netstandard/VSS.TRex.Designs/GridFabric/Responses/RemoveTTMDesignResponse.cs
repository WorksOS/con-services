using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class RemoveTTMDesignResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public Guid DesignUid { get; set; }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      base.InternalToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteGuid(DesignUid);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      if (reader is null)
      {
        throw new ArgumentNullException(nameof(reader));
      }

      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        DesignUid = reader.ReadGuid() ?? Guid.Empty;
      }
    }
  }
}
