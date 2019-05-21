using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.AssetLocations;

namespace VSS.Productivity3D.Push.Hubs.AssetLocations
{
  /// <summary>
  /// This hub will be used by the UI, and allow for the UI to request location updates for an asset
  /// The hub will also SEND out location events, as outlined in <see cref="IAssetStatusClientHubContext"/> - but it is not callable via clients
  /// </summary>
  public class AssetStatusClientHub : AuthenticatedHub<IAssetStatusClientHubContext>, IAssetStatusClientHub
  {
    private readonly IAssetStatusState assetState;

    public AssetStatusClientHub(ILoggerFactory loggerFactory, IAssetStatusState assetState) : base(loggerFactory)
    {
      this.assetState = assetState;
    }

    public async Task StartProcessingAssets(Guid projectUid)
    {
      // Handy for debugging, but not needed normally
      Logger.LogInformation($"Client requesting information for ProjectUID: {projectUid}");

      var token = AuthorizationToken;
      var jwt = JwtAssertion;

      if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(jwt))
        throw new UnauthorizedAccessException("Missing Authentication Headers");

      // Do we need to check authentication here??? 
      var model = new AssetUpdateSubscriptionModel()
      {
        AuthorizationHeader = token,
        JWTAssertion = jwt,
        CustomerUid = Guid.Parse(CustomerUid),
        ProjectUid = projectUid
      };

      // SignalR needs to await tasks, note from Microsoft Site:
      // Use await when calling asynchronous methods that depend on the hub staying alive. For example, a method such as Clients.All.SendAsync(...) can fail if it's called without await and the hub method completes before SendAsync finishes.
      await assetState.AddSubscription(Context.ConnectionId, model);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await assetState.RemoveSubscription(Context.ConnectionId);
      await base.OnDisconnectedAsync(exception);
    }
  }
}
