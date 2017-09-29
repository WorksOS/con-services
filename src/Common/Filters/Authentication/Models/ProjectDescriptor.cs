namespace VSS.Productivity3D.Common.Filters.Authentication.Models
{
  /// <summary>
  /// Describes VL project 
  /// </summary>
  public class ProjectDescriptor
  {
    /// <summary>
    /// Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is archived; otherwise, <c>false</c>.
    /// </value>
    public bool isArchived { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is landfill.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is landfill; otherwise, <c>false</c>.
    /// </value>
    public bool isLandFill { get; set; }
    /// <summary>
    /// Gets or sets a unique project identifier's value.
    /// </summary>
    public string projectUid { get; set; }
    /// <summary>
    /// Gets or sets a unique project identifier's value from legacy VisionLink.
    /// </summary>
    public long projectId { get; set; }
    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    public string coordinateSystemFileName { get; set; }
    /// <summary>
    /// Gets or sets the project boundary as WKT
    /// </summary>
    public string projectGeofenceWKT { get; set; }
    /// <summary>
    /// Gets or sets the legacy project time zone.
    /// </summary>
   public string projectTimeZone { get; set; }
    /// <summary>
    /// Gets or sets the IANA project time zone.
    /// </summary>
    public string landfillTimeZone { get; set; }
  }
}