using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Designs.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.TRex.Designs.GridFabric.Events
{

    /// <summary>
    /// Contains all relevant information detailing a mutating change event made to a design
    /// </summary>
    public class DesignChangedEvent : BaseRequestResponse, IDesignChangedEvent
  {
      public Guid SiteModelUid { get; set; } = Guid.Empty;
      public Guid DesignUid { get; set; } = Guid.Empty;
      public ImportedFileType FileType { get; set; }
      public bool DesignRemoved { get; set; }
      private const byte VERSION_NUMBER = 1;

      public override void ToBinary(IBinaryRawWriter writer)
      {
        VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
        writer.WriteGuid(SiteModelUid);
        writer.WriteGuid(DesignUid);
        writer.WriteInt((int)FileType);
        writer.WriteBoolean(DesignRemoved);
      }

      public override void FromBinary(IBinaryRawReader reader)
      {
        VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
        SiteModelUid  = reader.ReadGuid() ?? Guid.Empty;
        DesignUid     = reader.ReadGuid() ?? Guid.Empty;
        FileType      = (ImportedFileType)reader.ReadInt();
        DesignRemoved = reader.ReadBoolean();
      }
  }

}
