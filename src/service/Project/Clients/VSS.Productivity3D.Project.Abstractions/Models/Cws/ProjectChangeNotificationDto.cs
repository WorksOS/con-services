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
  }
}
