using System;

namespace VSS.Productivity.Push.Models.Notifications
{
  public sealed class ProjectDescriptorChangedNotification : ProjectNotification
  {
    public const string PROJECT_DESCRIPTOR_CHANGED_KEY = "project_descriptor_changed";

    public ProjectDescriptorChangedNotification(Guid projectUid) : base(PROJECT_DESCRIPTOR_CHANGED_KEY, projectUid)
    {
      
    }
  }
}