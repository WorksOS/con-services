using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;

namespace VSS.Productivity3D.AssetMgmt3D.Abstractions
{
  public interface IAssetResolverProxy : ICacheProxy
  {
    Task<IEnumerable<KeyValuePair<Guid, long>>> GetMatchingAssets(List<Guid> assetUid,
      IDictionary<string, string> customHeaders = null);
  }
}
