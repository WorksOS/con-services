using System;
using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// A client for the project event Server Hub
  /// </summary>
  public interface IProjectEventHubClient : IHubClient
  {

    // todoJeannie added for testing from scheduler exportController only
    Task SubscribeToProjectEvents(Guid projectUid);

    Task FileImportIsComplete(ImportedFileStatus importedFileStatus);
   
  }
}
