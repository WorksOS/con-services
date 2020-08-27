using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.GridFabric.Responses;

namespace VSS.TRex.SurveyedSurface.GridFabric.Responses
{
  public class RemoveSurveyedSurfaceResponse : BaseDesignRequestResponse
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
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        DesignUid = reader.ReadGuid() ?? Guid.Empty;
      }
    }
  }
}
