using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.AssetService.Proxy
{
  public interface IAssetServiceProxy : ICacheProxy
  {
    Task<List<AssetData>> GetAssetsV1(string customerUid, IDictionary<string, string> customHeaders = null);
  }
}
