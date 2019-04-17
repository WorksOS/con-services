using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetAssetSummaryProxy
  {
    Task<AssetSummary> GetAssetSummary(string assetUid, IDictionary<string, string> customHeaders = null);
  }
}