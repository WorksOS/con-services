using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity3D.Push.Abstractions
{
  public interface IAssetStatusHub
  {
    Task StartProcessingAssets(List<Guid> AssetIds, Guid customer);
    Task<Dictionary<Guid, Guid>> GetActiveAssets();
    Task UpdateAssetStatus(List<AssetStatusNotificationParameters> assets);
  }
}