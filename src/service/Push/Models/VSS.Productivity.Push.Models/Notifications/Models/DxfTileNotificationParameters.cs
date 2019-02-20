using System;

namespace VSS.Productivity.Push.Models.Notifications.Models
{
  /// <summary>
  /// Data for the DXF tile notification
  /// </summary>
  public class DxfTileNotificationParameters
  {
    /// <summary>
    /// The minimum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MinZoomLevel;
    /// <summary>
    /// The maximum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MaxZoomLevel;

    /// <summary>
    /// The unique ID of the file
    /// </summary>
    public Guid FileUid;
  }
}
