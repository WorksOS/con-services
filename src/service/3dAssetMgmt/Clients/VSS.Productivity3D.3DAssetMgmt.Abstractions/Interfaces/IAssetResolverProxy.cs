using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Productivity3D.AssetMgmt3D.Abstractions.Models;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions
{
  public interface IAssetResolverProxy : ICacheProxy
  {
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<Guid> assetUids,
      IDictionary<string, string> customHeaders = null);

    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<long> assetIds,
      IDictionary<string, string> customHeaders = null);

    Task<MatchingAssetsDisplayModel> GetMatching3D2DAssets(MatchingAssetsDisplayModel asset,
      IDictionary<string, string> customHeaders = null);
  }
}
