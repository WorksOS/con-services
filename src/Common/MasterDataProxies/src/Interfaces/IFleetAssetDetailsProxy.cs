using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetAssetDetailsProxy
  {
    Task<AssetDetails> GetAssetDetails(string assetUid, IDictionary<string, string> customHeaders = null);
  }
}
