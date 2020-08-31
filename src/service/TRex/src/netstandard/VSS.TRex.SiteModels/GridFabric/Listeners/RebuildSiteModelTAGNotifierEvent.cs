using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.SiteModels.Interfaces.Listeners;
using VSS.TRex.TAGFiles.GridFabric.Responses;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.GridFabric.Listeners
{
  public class RebuildSiteModelTAGNotifierEvent : BaseRequestResponse, IRebuildSiteModelTAGNotifierEvent
  {
    private const byte VERSION_NUMBER = 1;

    public Guid ProjectUid { get; set; }

    public IProcessTAGFileResponseItem[] ResponseItems { get; set; }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        ProjectUid = reader.ReadGuid() ?? Guid.Empty;

        if (reader.ReadBoolean())
        {
          var count = reader.ReadInt();
          ResponseItems = new ProcessTAGFileResponseItem[count];

          for (var i = 0; i < count; i++)
          {
            ResponseItems[i] = new ProcessTAGFileResponseItem();
            ResponseItems[i].FromBinary(reader);
          }
        }
      }
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
      
      writer.WriteGuid(ProjectUid);

      writer.WriteBoolean(ResponseItems!= null);
      if (ResponseItems != null)
      {
        writer.WriteInt(ResponseItems.Length);
        ResponseItems.ForEach(x => x.ToBinary(writer));
      }
    }
  }
}
