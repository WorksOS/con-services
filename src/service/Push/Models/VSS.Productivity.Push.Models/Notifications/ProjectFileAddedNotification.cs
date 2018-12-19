using System;

namespace VSS.Productivity.Push.Models.Notifications
{
  /// <summary>
  /// Project File Added Notification
  /// </summary>
  public sealed class ProjectFileAddedNotification : ProjectNotification
  {
    public const string PROJECT_FILE_ADDED_KEY = "project_file_added";

    public ProjectFileAddedNotification(Guid projectUid) : base(PROJECT_FILE_ADDED_KEY, projectUid)
    {
    }
  }
}
