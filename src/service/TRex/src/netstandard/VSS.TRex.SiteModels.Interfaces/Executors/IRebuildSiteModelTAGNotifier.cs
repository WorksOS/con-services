using System;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  public interface IRebuildSiteModelTAGNotifier
  {
    void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] processedItems);
  }
}
