using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob
{
  public class AssetStatusJob : IJob
  {
    public static Guid VSSJOB_UID = Guid.Parse("8b179467-2b36-49fc-aee0-ee5d4f4e8efa");

    public Guid VSSJobUid => VSSJOB_UID;

    private readonly IAssetStatusServerHubClient assetStatusServerHubClient;
    private readonly IFleetAssetDetailsProxy assetDetailsProxy;
    private readonly IFleetAssetSummaryProxy assetSummaryProxy;
    private readonly IRaptorProxy raptorProxy;
    private readonly IAssetResolverProxy assetResolverProxy;
    private readonly ILogger log;

    private List<AssetUpdateSubscriptionModel> subscriptions;

    public AssetStatusJob(IAssetStatusServerHubClient assetStatusServerHubClient,
      IFleetAssetDetailsProxy assetDetailsProxy, IFleetAssetSummaryProxy assetSummaryProxy, IRaptorProxy raptorProxy,
      IAssetResolverProxy assetResolverProxy, ILoggerFactory logger)
    {
      this.assetStatusServerHubClient = assetStatusServerHubClient;
      log = logger.CreateLogger<AssetStatusJob>();
      this.assetDetailsProxy = assetDetailsProxy;
      this.raptorProxy = raptorProxy;
      this.assetSummaryProxy = assetSummaryProxy;
      this.assetResolverProxy = assetResolverProxy;
    }

    public Task Setup(object o)
    {
      if (assetStatusServerHubClient.IsConnecting || assetStatusServerHubClient.Connected)
        return Task.CompletedTask;
      log.LogInformation("Asset Status Hub Client not connected, starting a connection.");
      return assetStatusServerHubClient.Connect();
    }

    public async Task Run(object o)
    {
      subscriptions = await assetStatusServerHubClient.GetSubscriptions();
      log.LogInformation($"Found {subscriptions.Count} subscriptions to process");
      var tasks = subscriptions.Select(ProcessSubscription).ToList();
      await Task.WhenAll(tasks);
    }
    public Task TearDown(object o)
    {
      return Task.CompletedTask;
    }

    private async Task ProcessSubscription(AssetUpdateSubscriptionModel subscriptionModel)
    {
      try
      {
        //Get machines
        //https://3dproductivity.myvisionlink.com/t/trimble.com/vss-3dproductivity/2.0/projects/a530371d-20a1-40cf-99ce-e11c54140be4/machines
        var route = $"/projects/{subscriptionModel.ProjectUid}/machines";
        var machines = await raptorProxy.ExecuteGenericV2Request<Machine3DStatuses>(route,
          HttpMethod.Get,
          null,
          subscriptionModel.Headers());

        if (machines.Code != ContractExecutionStatesEnum.ExecutedSuccessfully || !machines.MachineStatuses.Any())
          //Nothing to do here. Breaking.
          return;

        var processingAssets = new List<Task>();

        //Now for each machine try to identify a matching asset 
        foreach (var machine in machines.MachineStatuses)
        {
          var task = ProcessAssetEvents(machine, subscriptionModel.ProjectUid, subscriptionModel.CustomerUid, subscriptionModel.Headers());
          processingAssets.Add(task);
        }

        await Task.WhenAll(processingAssets);
      }
      catch (Exception e)
      {
        log.LogError(e, $"Exception when running subscription {JsonConvert.SerializeObject(subscriptionModel)}");
      }
    }

    /// <summary>
    /// Fetch asset data, the proxy will cache multiple request to the same asset
    /// </summary>
    private async Task<(AssetDetails details, AssetSummary summary)> GetAssetData(string assetUid, IDictionary<string, string> headers)
    {
      var assetDetailsTask = assetDetailsProxy.GetAssetDetails(assetUid, headers);
      var assetSummaryTask = assetSummaryProxy.GetAssetSummary(assetUid, headers);

      await Task.WhenAll(assetDetailsTask, assetSummaryTask);

      return (assetDetailsTask.Result, assetSummaryTask.Result);
    }


    private async Task ProcessAssetEvents(MachineStatus machine, Guid projectUid, Guid customerUid,
      IDictionary<string, string> headers)
    {
      var assetAggregateStatus = new AssetAggregateStatus
      {
        Machine3D = machine,
        ProjectUid = projectUid,
        CustomerUid = customerUid
      };

      var assets = await assetResolverProxy.GetMatchingAssets(new List<long>
      {
        machine.AssetId
      }, headers);

      // Prevent multiple iterations of the IEnumerable
      var assetList = assets?.ToList();
      if (assetList != null && assetList.Any())
      {
        var matchingAsset = await assetResolverProxy.GetMatching3D2DAssets(assetList.First().Key, headers);
        //Change that for the actual matched asset. Since we supplied 3d asset get data for the matching 2d asset.
        //if there is no 2d asset we should try using SNM asset

        string uid;
        if (matchingAsset == null || matchingAsset.Code != ContractExecutionStatesEnum.ExecutedSuccessfully)
          uid = assetList.First().Key.ToString();
        else
          uid = matchingAsset.MatchingAssetUID;

        var (details, summary) = await GetAssetData(uid, headers);

        assetAggregateStatus.Details = details;
        assetAggregateStatus.Summary = summary;
        assetAggregateStatus.AssetUid = Guid.Parse(uid);
        
      }

      await assetStatusServerHubClient.UpdateAssetLocationsForClient(assetAggregateStatus);
    }

  }
}
