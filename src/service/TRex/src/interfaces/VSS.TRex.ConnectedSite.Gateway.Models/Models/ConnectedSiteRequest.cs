using VSS.Productivity3D.Models.Models;
using VSS.TRex.ConnectedSite.Gateway.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.Models
{
  public class ConnectedSiteRequest
  {
    public CompactionTagFileRequest TagRequest { get; private set; }
    public ConnectedSiteMessageType MessageType { get; private set; }

    public ConnectedSiteRequest(CompactionTagFileRequest tagRequest, ConnectedSiteMessageType messageType)
    {
      TagRequest = tagRequest;
      MessageType = messageType;
    }
  }
}
