using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using model = VSS.Productivity3D.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITRexTagFileProxy
  {
    Task<ContractExecutionResult> SendTagFileDirect(model.CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
    Task<ContractExecutionResult> SendTagFileNonDirect(model.CompactionTagFileRequest compactionTagFileRequest,
      IDictionary<string, string> customHeaders = null);
  }
}