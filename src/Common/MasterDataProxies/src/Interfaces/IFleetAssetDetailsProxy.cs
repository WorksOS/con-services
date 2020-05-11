using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetAssetDetailsProxy
  {
    Task<AssetDetails> GetAssetDetails(string assetUid, IDictionary<string, string> customHeaders = null);
  }
}
