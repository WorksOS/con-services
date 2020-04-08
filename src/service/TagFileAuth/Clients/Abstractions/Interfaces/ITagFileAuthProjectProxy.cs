using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

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
     * [Route("api/v2/project/getUidsEarthWorks")]     [HttpPost]
     * returns GetProjectAndAssetUidsEarthWorksResult
     */
    Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndAssetUidsEarthWorks(GetProjectAndAssetUidsEarthWorksRequest getProjectAndAssetUidsEarthWorksRequest,
      IDictionary<string, string> customHeaders = null);

  }
}
