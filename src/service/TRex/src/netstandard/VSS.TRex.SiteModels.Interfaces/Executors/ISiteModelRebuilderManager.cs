using System;
using System.Collections.Generic;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  public interface ISiteModelRebuilderManager
  {
    int RebuildCount();

    List<IRebuildSiteModelMetaData> GetRebuildersState();

    void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] responseItems);

    bool Rebuild(Guid projectUid, bool archiveTAGFiles);

    bool AddRebuilder(ISiteModelRebuilder rebuilder);

    // All rebuilders present in the manager are aborted
    void AbortAll();
  }
}
