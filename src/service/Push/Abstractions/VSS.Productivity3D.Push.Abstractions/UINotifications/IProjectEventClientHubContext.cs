using System.Threading.Tasks;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// This is the hub context, that is used service side to generate project events (can only receive these in the UI, not send)
  /// Hence the two different interfaces (the hub doesn't define this, only the hub context).
  /// </summary>
  public interface IProjectEventClientHubContext : IProjectEventHub
  {
    Task OnFileImportCompleted(ImportedFileStatus importedFileStatus);
  }
}
