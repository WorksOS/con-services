using System;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Interfaces.Listeners
{
  public interface IRebuildSiteModelTAGNotifierEvent
  {
    Guid ProjectUid { get; set; }

    IProcessTAGFileResponseItem[] ResponseItems { get; set; }
  }
}
