using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetAssetDetailsProxy
  {
    Task<AssetDetails> GetAssetDetails(string assetUid, IHeaderDictionary customHeaders = null);
  }
}
