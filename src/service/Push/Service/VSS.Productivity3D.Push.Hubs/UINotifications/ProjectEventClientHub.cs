using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models;
using VSS.Productivity3D.Push.Abstractions.UINotifications;

namespace VSS.Productivity3D.Push.Hubs.UINotifications
{
  /// <summary>
  /// This hub will be used by the UI, and allow for the UI to subscribe to events related to a project
  /// The UI can subscribe for one or more projects
  /// </summary>
  public class ProjectEventClientHub : AuthenticatedHub<IProjectEventClientHubContext>, IProjectEventClientHub
  {
    private readonly IProjectEventState projectEventState;

    public ProjectEventClientHub(ILoggerFactory loggerFactory, IProjectEventState projectEventState) : base(loggerFactory)
    {
      this.projectEventState = projectEventState;
    }

    public async Task StartProcessingProject(Guid projectUid)
    {
      // Handy for debugging, but not needed normally
      Logger.LogInformation($"Client requesting information for ProjectUID: {projectUid}");

      var token = AuthorizationToken;
      var jwt = JwtAssertion;

      if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(jwt))
        throw new UnauthorizedAccessException("Missing Authentication Headers");

      // PushAuthentication checks TID authentication and CustomerUser association

      // GetProject() checks if project exists, and if customer have access to this project
      // todoJeannie currently throws exception. Calls to StartProcessingProject would need to catch
      await ((PushPrincipal)Context.User).GetProject(projectUid.ToString());

      var model = new ProjectEventSubscriptionModel()
      {
        CustomerUid = Guid.Parse(CustomerUid),
        ProjectUid = projectUid,
        AuthorizationHeader = token,
        JWTAssertion = jwt
      };

      // SignalR needs to await tasks
      // can be multiple projects per connection
      // do we want to re-check authorization for each? todoJeannie
      await projectEventState.AddSubscription(Context.ConnectionId, model);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
      await projectEventState.RemoveSubscriptions(Context.ConnectionId);
      await base.OnDisconnectedAsync(exception);
    }
  }
}
