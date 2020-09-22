using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class DesignChangedEventSenderResponse : VersionCheckedBinarizableSerializationBase, IDesignChangedEventSenderResponse
  {
    public static byte VERSION_NUMBER = 1;

    public bool Success { get; set; }
    public Guid NodeUid { get; set; }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        Success = reader.ReadBoolean();
        NodeUid = reader.ReadGuid() ?? Guid.Empty;
      }
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Success);
      writer.WriteGuid(NodeUid);
    }
  }
}
