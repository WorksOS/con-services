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
    public Guid ProjectUid { get; set; }

    public IProcessTAGFileResponseItem[] ResponseItems { get; set; }

    public override void FromBinary(IBinaryRawReader reader)
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

    public override void ToBinary(IBinaryRawWriter writer)
    {
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
