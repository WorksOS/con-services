using Newtonsoft.Json;

namespace VSS.Productivity3D.Project.Abstractions.Models.Cws
{
  /// <summary>
  /// Represents a change notification, once a project changed has been committed.
  /// Can include Metadata and/or Coordinate system currently
  /// </summary>
  public class ProjectChangeNotificationDto
  {
    /// <summary>
    /// Account TRN the change relates to
    /// </summary>
    public string AccountTrn { get; set; }

    /// <summary>
    /// Project TRN the change relates to
    /// </summary>
    public string ProjectTrn { get; set; }

    /// <summary>
    /// Change Notification Type - can be multiple
    /// </summary>
    public NotificationType NotificationType { get; set; }

    /// <summary>
    /// File name for the coordinate system
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemFilename", Required = Required.Default)]
    public string CoordinateSystemFileName { get; set; }

    /// <summary>
    /// Base64 encoded file contents for the coordinate system.
    /// </summary>
    [JsonProperty(PropertyName = "CoordinateSystemData", Required = Required.Default)]
    public byte[] CoordinateSystemFileContent { get; set; }
  }
}
