using System;

namespace VSS.Productivity.Push.Models.Notifications.Models
{
  /// <summary>
  /// Data for the raster tile notification (DXF and GeoTIFF)
  /// </summary>
  public class RasterTileNotificationParameters
  {
    /// <summary>
    /// The minimum zoom level that tiles have been generated for.
    /// </summary>
    public int MinZoomLevel;
    /// <summary>
    /// The maximum zoom level that tiles have been generated for.
    /// </summary>
    public int MaxZoomLevel;

    /// <summary>
    /// The unique ID of the file
    /// </summary>
    public Guid FileUid;
  }
}
