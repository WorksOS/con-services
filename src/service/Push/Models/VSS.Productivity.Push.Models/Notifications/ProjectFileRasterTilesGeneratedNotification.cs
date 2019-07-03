using VSS.Productivity.Push.Models.Enums;
using VSS.Productivity.Push.Models.Notifications.Models;

namespace VSS.Productivity.Push.Models.Notifications
{
  /// <summary>
  /// Project file raster tiles generated notification
  /// </summary>
  public sealed class ProjectFileRasterTilesGeneratedNotification : Notification
  {
    public const string PROJECT_FILE_RASTER_TILES_GENERATED_KEY = "project_file_raster_tiles_generated";

    public ProjectFileRasterTilesGeneratedNotification(RasterTileNotificationParameters parameters) : base(PROJECT_FILE_RASTER_TILES_GENERATED_KEY, parameters, NotificationUidType.File)
    {
    }
  }
}
