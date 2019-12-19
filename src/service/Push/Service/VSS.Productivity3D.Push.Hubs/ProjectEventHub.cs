using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Productivity.Push.Models.Notifications.Models;
using VSS.Productivity3D.Push.Abstractions.UINotifications;
using VSS.Productivity3D.Push.Hubs.Authentication;

namespace VSS.Productivity3D.Push.Hubs
{
  /// <summary>
  /// This hub will be used by the UI, and allow for the UI to subscribe to events related to a project
  /// The UI can subscribe for one or more projects
  /// </summary>
  public class ProjectEventHub : AuthenticatedHub<IProjectEventClientHubContext>, IProjectEventHub
  {
    public async Task StartProcessingProject(Guid projectUid)
    {
      // Handy for debugging, but not needed normally
      Logger.LogInformation($"Client: {Context.ConnectionId} requesting events for ProjectUID: {projectUid}");

      if (!Guid.TryParseExact(projectUid.ToString(), "D", out var _) || projectUid == Guid.Empty)
        throw new ArgumentException("ProjectUid must be provided");
      
      // PushAuthentication checks TID authentication and CustomerUser association
      // GetProject() checks if project exists, and if customer have access to this project
      var projectData = await ((PushPrincipal)Context.User).GetProject(projectUid.ToString());
      if (projectData == null)
        throw new UnauthorizedAccessException($"Missing Project or project does not belong to specified customer or don't have access to the project {projectUid}");

      await Groups.AddToGroupAsync(Context.ConnectionId, GetHubGroupName(projectUid.ToString()));
      Logger.LogInformation($"Client: {Context.ConnectionId} added to signalR ProjectUID: {projectUid} group");
    }

    public async Task SendImportedFileEventToClients(ImportedFileStatus importedFileStatus)
    {
      await Clients.Group(GetHubGroupName(importedFileStatus.ProjectUid.ToString()))
        .OnFileImportCompleted(importedFileStatus);
    }

    private string GetHubGroupName(string projectUid)
    {
      var keyPrefix = typeof(ProjectEventHub).Name;
      return $"{keyPrefix}-{projectUid}";
    }

    public ProjectEventHub(ILoggerFactory loggerFactory) : base(loggerFactory) { }

    // the base class will remove connection from any groups, only needed for logging
    public override async Task OnDisconnectedAsync(Exception exception)
    {
      Logger.LogInformation($"Client: {Context.ConnectionId} removed from signalR groups");
      await base.OnDisconnectedAsync(exception);
    }
  }
}
