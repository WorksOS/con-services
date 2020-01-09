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

    // todoJeannie Steve
    //     Our internal services will call the hubs FileImportIsComplete
    //     The UI is a consumer of ProjectEvents and needs to call the hubs SubscribeToProjectEvents
    //                 it will the UI trigger this call and provide a callback for OnFileImportCompleted
    //                 Does it ever need to un-subscribe?
    Task SubscribeToProjectEvents(Guid projectUid);

    Task FileImportIsComplete(ImportedFileStatus importedFileStatus);
   
  }
}
