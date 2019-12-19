using System;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  public interface IProjectEventHub 
  {
    Task StartProcessingProject(Guid projectUid);

    Task SendImportedFileEventToClients(ImportedFileStatus importedFileStatus);
  }
}
