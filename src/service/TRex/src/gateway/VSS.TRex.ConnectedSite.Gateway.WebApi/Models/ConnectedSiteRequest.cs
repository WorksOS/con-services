using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Models
{
  public class ConnectedSiteRequest
  {
    public CompactionTagFileRequest TagRequest { get; set; }
    public ConnectedSiteMessageType MessageType { get; set; }
  }
}
