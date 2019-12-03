using System;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Push.Abstractions.UINotifications
{
  /// <summary>
  /// The hub definition for UI clients to request project events
  /// </summary>
  public interface IProjectEventClientHub
  {
    // todoJeannie
    Task StartProcessingProject(Guid projectUid);
  }
}
