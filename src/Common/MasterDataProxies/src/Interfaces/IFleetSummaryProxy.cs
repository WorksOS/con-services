using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VSS.MasterData.Models.Models;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface IFleetSummaryProxy
  {
    Task<AssetStatus> GetAssetStatus(string assetUid);
  }
}
