using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;

namespace VSS.Productivity3D.TagFileAuth.Abstractions.Interfaces
{
  public interface ITagFileAuthProjectProxy
  {
    Task<GetProjectAndAssetUidsResult> GetProjectAndAssetUids(GetProjectAndAssetUidsRequest getProjectAndAssetUidsRequest,
      IHeaderDictionary customHeaders = null);

    Task<GetProjectAndAssetUidsEarthWorksResult> GetProjectAndAssetUidsEarthWorks(GetProjectAndAssetUidsEarthWorksRequest getProjectAndAssetUidsEarthWorksRequest,
      IHeaderDictionary customHeaders = null);
  }
}
