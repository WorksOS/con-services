using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.TRex.Gateway.Common.Abstractions
{
  public interface ITRexConnectedSiteProxy
  {
    Task<ContractExecutionResult> SendTagFileNonDirectToConnectedSite(CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
