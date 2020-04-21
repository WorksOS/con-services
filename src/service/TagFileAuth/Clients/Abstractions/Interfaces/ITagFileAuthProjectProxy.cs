using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces
{
  public interface ITagFileAuthProjectProxy
  {
    Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest,
      IDictionary<string, string> customHeaders = null);

    Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndAssetUidsEarthWorks(GetProjectAndAssetUidsEarthWorksRequest getProjectAndAssetUidsEarthWorksRequest,
      IDictionary<string, string> customHeaders = null);
  }
}
