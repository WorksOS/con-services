using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Hubs
{
  public class AssetStatusHub : AuthenticatedHub<IAssetStatusHub>, IAssetStatusHub
  {
    private Dictionary<Guid, Guid> assetCustomerPairs = new Dictionary<Guid, Guid>();
    private Dictionary<string, List<Guid>> connectionAssets = new Dictionary<string, List<Guid>>();

    public Task StartProcessingAssets(List<Guid> AssetIds, Guid customer)
    {
      /*connectionAssets.Add(Context.ConnectionId,AssetIds);

      AssetIds.Where(a => !assetCustomerPairs.ContainsKey(a))
        .Select(a =>
        {
          assetCustomerPairs.Add(a, customer);
          return true;
        });

      return Task.FromResult(0);*/
      throw new NotImplementedException();
    }

    public Task<Dictionary<Guid, Guid>> GetActiveAssets()
    {
      throw new NotImplementedException();
      /*return Task.FromResult(assetCustomerPairs);*/
    }

    public async Task UpdateAssetStatus(List<AssetStatusNotificationParameters> assets)
    {
      throw new NotImplementedException();
      /*foreach (var asset in assets)
      {
        var connectionid = connectionAssets.Where(v => v.Value.Contains(Guid.Parse(asset.UpdatedAssetStatus.AssetIdentifier))).Select(v => v.Key);
        await Clients.Clients(connectionid.ToList()).UpdateAssetStatus(assets);
      }*/
    }
  }
}
