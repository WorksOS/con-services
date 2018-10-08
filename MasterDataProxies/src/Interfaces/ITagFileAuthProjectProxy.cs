using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ITagFileAuthProjectProxy
  {
    /*
     * [Route("api/v2/project/getUids")]     [HttpPost]
     * returns GetProjectAndAssetUidsResult
     */
    Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest,
      IDictionary<string, string> customHeaders = null);

    /*
     * [Route("api/v2/project/getUid")]     [HttpPost]
     * returns GetProjectUidResult
     */
    Task<GetProjectAndAssetUidsResult> GetProjectUid(GetProjectUidRequest getProjectUidRequest,
      IDictionary<string, string> customHeaders = null);

  }
}