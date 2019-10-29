using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;

namespace VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces
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
     * [Route("api/v2/project/getUidsCTCT")]     [HttpPost]
     * returns GetProjectAndAssetUidsCTCTResult
     */
    Task<GetProjectAndAssetUidsCTCTResult> GetProjectAndAssetUidsCTCT(GetProjectAndAssetUidsCTCTRequest getProjectAndAssetUidsCTCTRequest,
      IDictionary<string, string> customHeaders = null);

  }
}
