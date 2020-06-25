using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.AWS.TransferProxy;
using VSS.TRex.TAGFiles.Models;

namespace VSS.TRex.SiteModels.Interfaces.Executors
{
  public interface ISiteModelRebuilderManager
  {
    int RebuildCount();

    List<IRebuildSiteModelMetaData> GetRebuildersState();

    void TAGFileProcessed(Guid projectUid, IProcessTAGFileResponseItem[] responseItems);

    bool Rebuild(Guid projectUid, bool archiveTAGFiles, TransferProxyType proxyType);

    bool AddRebuilder(ISiteModelRebuilder rebuilder);

    void AbortAll();
    void Abort(Guid projectUid);

    Task BeginOperations();
  }
}
