using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITagFileAuthProjectProxy
  {
    /*
     * [Route("api/v2/project/getUids")]     [HttpPost]
     * returns GetProjectAndAssetUidsResult
     */
    Task<ContractExecutionResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest,
      IDictionary<string, string> customHeaders = null);

    /*
     * [Route("api/v2/project/getUid")]     [HttpPost]
     * returns GetProjectUidResult
     */
    Task<ContractExecutionResult> GetProjectUid(GetProjectUidRequest getProjectUidRequest,
      IDictionary<string, string> customHeaders = null);

  }
}