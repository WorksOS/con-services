using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetAssetSummaryProxy
  {
    Task<AssetSummary> GetAssetSummary(string assetUid, IHeaderDictionary customHeaders = null);
  }
}
