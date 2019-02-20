using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity.Push.Models.Notifications
{
  /// <summary>
  /// Project file DXF tiles generated notification
  /// </summary>
  public sealed class ProjectFileDxfTilesGeneratedNotification : Notification
  {
    public const string PROJECT_FILE_DXF_TILES_GENERATED_KEY = "project_file_dxf_tiles_generated";

    public ProjectFileDxfTilesGeneratedNotification(DxfTileNotificationParameters parameters) : base(PROJECT_FILE_DXF_TILES_GENERATED_KEY, parameters, NotificationUidType.File)
    {
    }
  }
}
