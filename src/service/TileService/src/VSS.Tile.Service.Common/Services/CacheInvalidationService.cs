using System;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity.Push.Models.Attributes;
using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications;

namespace VSS.Tile.Service.Common.Services
{
  public class CacheInvalidationService
  {
    private readonly IProjectListProxy projectList;
    private readonly ILogger<CacheInvalidationService> log;

    public CacheInvalidationService(IProjectListProxy projectList, ILoggerFactory logger)
    {
      this.projectList = projectList;
      this.log = logger.CreateLogger<CacheInvalidationService>();
    }
    
    /// <summary>
    /// Clear project list cache when a project is updated
    /// </summary>
    [Notification(NotificationUidType.Project, ProjectDescriptorChangedNotification.PROJECT_DESCRIPTOR_CHANGED_KEY)]
    // ReSharper disable once UnusedMember.Global
    public void OnProjectDescriptorUpdated(Guid projectUid)
    {
      log.LogInformation($"Clearing Project List Proxy cache for Project UID: {projectUid}");
      projectList.ClearCacheItem(projectUid.ToString());
    }
  }
}