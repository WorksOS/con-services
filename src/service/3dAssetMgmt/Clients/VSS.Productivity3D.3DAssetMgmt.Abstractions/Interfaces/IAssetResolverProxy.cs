using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions
{
  public interface IAssetResolverProxy : ICacheProxy
  {
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<Guid> assetUids,
      IHeaderDictionary customHeaders = null);

    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<long> assetIds,
      IHeaderDictionary customHeaders = null);

    Task<MatchingAssetsDisplayModel> GetMatching3D2DAssets(MatchingAssetsDisplayModel asset,
      IHeaderDictionary customHeaders = null);
  }
}
