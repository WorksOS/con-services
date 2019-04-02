using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Http;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Scheduler.Jobs.AssetWorksManagerJob
{
  public class AssetStatusJob : IJob
  {

    public static Guid VSSJOB_UID = Guid.Parse("8b179467-2b36-49fc-aee0-ee5d4f4e8efa");
    public Guid VSSJobUid => VSSJOB_UID;

    private readonly ITPaaSApplicationAuthentication authentication;
    private readonly IAssetStatusHubClient assetStatusHub;
    private readonly IFleetSummaryProxy fleetProxy;
    private readonly ILogger log;
    private Dictionary<Guid, Guid> assetsToRetrieve;

    private static string bearerToken;

    public AssetStatusJob(ITPaaSApplicationAuthentication authn, IAssetStatusHubClient assetHubClient, IFleetSummaryProxy fleet, ILoggerFactory logger)
    {
      authentication = authn;
      this.assetStatusHub = assetHubClient;
      log = logger.CreateLogger<AssetStatusJob>();
      fleetProxy = fleet;
    }



    public async Task Setup(object o)
    {
      bearerToken = authentication.GetApplicationBearerToken();
      await assetStatusHub.Connect();
      assetsToRetrieve = await assetStatusHub.GetActiveAssets();

    }

    public async Task Run(object o)
    {
      /*  AssetStatusRequest request;
        AssetStatus assetStatus;

        try
        {
          request = (o as JObject).ToObject<AssetStatusRequest>();
          request.Validate();
          assetStatus = await fleetProxy.GetAssetStatus(request.AssetIdentifier.FirstOrDefault());
        }
        catch (Exception e)
        {
          log.LogError(e, "Exception when converting parameters to AssetStatusRequest");
          throw new ServiceException(HttpStatusCode.InternalServerError,
            new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
              "Missing or Wrong parameters passed to AssetWorksManager job"));
        }*/

      await assetStatusHub.UpdateAssetStatus(new List<AssetStatusNotificationParameters>()
        {new AssetStatusNotificationParameters()});

    }

    public Task TearDown(object o)
    {
      return assetStatusHub.Disconnect();
    }

    private Dictionary<string, string> CustomHeaders()
    {
      return new Dictionary<string, string>
      {
        {HeaderConstants.AUTHORIZATION, $"Bearer {bearerToken}"}
      };
    }
  }
}
