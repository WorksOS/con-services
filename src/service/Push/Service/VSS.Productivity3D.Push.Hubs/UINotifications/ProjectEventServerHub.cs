using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.UINotifications;

namespace VSS.Productivity3D.Push.Hubs.UINotifications
{
  /// <summary>
  /// This hub allows other internal services to trigger project events for all clients that have requested events
  /// </summary>
  public class ProjectEventServerHub : AuthenticatedHub<IProjectEventServerHub>, IProjectEventServerHub
  {
    private readonly IProjectEventState projectEventState;
    private readonly IHubContext<ProjectEventClientHub, IProjectEventClientHubContext> hub;

    public ProjectEventServerHub(ILoggerFactory loggerFactory, IProjectEventState projectEventState, IHubContext<ProjectEventClientHub, IProjectEventClientHubContext> hub) : base(loggerFactory)
    {
      this.projectEventState = projectEventState;
      this.hub = hub;
    }

    /// <inheritdoc />
    public async Task<List<ProjectEventSubscriptionModel>> GetSubscriptions()
    {
      return await projectEventState.GetSubscriptions();
    }

    /// <inheritdoc />
    public async Task<bool> AddProjectEventForClients(/* todoJeannie AssetAggregateStatus assets */)
    {
      //var connections = await projectEventState.GetClientsForProject(assets.CustomerUid, assets.ProjectUid);
      //await hub.Clients.Clients(connections).UpdateProjectEvent(assets);
      return true;
    }
  }
}
