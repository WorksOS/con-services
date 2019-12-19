using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// A client for the project event Server Hub
  /// </summary>
  public interface IProjectEventHubClient : IHubClient
  {
    Task FileImportIsComplete(ImportedFileStatus importedFileStatus);
  }
}
