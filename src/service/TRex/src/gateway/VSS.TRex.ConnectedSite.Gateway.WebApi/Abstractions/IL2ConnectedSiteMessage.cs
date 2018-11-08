using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.TRex.ConnectedSite.Gateway.WebApi.Abstractions
{
  interface IL2ConnectedSiteMessage : IConnectedSiteMessage
  {
    string DesignName { get; set; }
    string AssetType { get; set; }
    string AppVersion { get; set; }
    /// <summary>
    /// This will normally for GCS900 for messges from harvested TAGS 
    /// </summary>
    string AppName { get; set; }
    /// <summary>
    /// MachineID in tag file
    /// </summary>
    string AssetNickname { get; set; }

  }
}
