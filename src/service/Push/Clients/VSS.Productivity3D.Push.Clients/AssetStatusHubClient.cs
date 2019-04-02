using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.ConfigurationStore;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions;

namespace VSS.Productivity3D.Push.Clients
{
  public class AssetStatusHubClient : BaseClient, IAssetStatusHubClient
  {
    public const string ROUTE = "/assetstatus";

    public AssetStatusHubClient(IConfigurationStore configuration, IServiceResolution resolver,
      ILoggerFactory loggerFactory) : base(configuration, resolver, loggerFactory)
    {
    }

    public Task StartProcessingAssets(List<Guid> AssetIds, Guid customer)
    {
      throw new NotImplementedException();
    }

    public async Task<Dictionary<Guid, Guid>> GetActiveAssets()
    {
      return (Dictionary<Guid, Guid>) await Connection.InvokeCoreAsync(nameof(GetActiveAssets),
        typeof(Dictionary<Guid, Guid>), null);
    }

    public Task UpdateAssetStatus(List<AssetStatusNotificationParameters> assets)
    {
      if (Connected)
        return Connection.SendCoreAsync(nameof(UpdateAssetStatus), new[] {assets});
      Logger.LogWarning("Attempt to send message while client disconnected. Notification not sent.");
      return Task.CompletedTask;
    }

    public override string UrlKey => ROUTE;
    public override void SetupCallbacks()
    {

    }

 
  }
}
