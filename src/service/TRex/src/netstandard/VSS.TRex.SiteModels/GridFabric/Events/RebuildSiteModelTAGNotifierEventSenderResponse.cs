using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SiteModels.Interfaces.Listeners;

namespace VSS.TRex.SiteModels.GridFabric.Events
{
  public class RebuildSiteModelTAGNotifierEventSenderResponse : VersionCheckedBinarizableSerializationBase, IRebuildSiteModelTAGNotifierEventSenderResponse
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
