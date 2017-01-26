using VSS.Project.Data.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace ProjectWebApi.Models
{
  /// <summary>
  ///   Describes VL project
  /// </summary>
  public class ProjectDescriptor
  {
    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
    /// </value>
    public bool IsArchived { get; set; }

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }
    /// <summary>
    /// Gets or sets the project time zone.
    /// </summary>
    /// <value>
    /// The project time zone.
    /// </value>
    public string ProjectTimeZone { get; set; }
    /// <summary>
    /// Gets or sets the type of the project.
    /// </summary>
    /// <value>
    /// The type of the project.
    /// </value>
    public ProjectType ProjectType { get; set; }
    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public string StartDate { get; set; }
    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    /// <value>
    /// The end date.
    /// </value>
    public string EndDate { get; set; }
    /// <summary>
    /// Gets or sets the project uid.
    /// </summary>
    /// <value>
    /// The project uid.
    /// </value>
    public string ProjectUid { get; set; }
    /// <summary>
    /// Gets or sets the project geofence.
    /// </summary>
    /// <value>
    /// The project geofence in WKT format.
    /// </value>
    public string ProjectGeofenceWKT { get; set; }
    /// <summary>
    /// Gets or sets the project ID from legacy VisionLink
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    public int LegacyProjectId { get; set; }

    public override bool Equals(object obj)
    {
      var otherProject = obj as ProjectDescriptor;
      if (otherProject == null) return false;
      return otherProject.ProjectUid == this.ProjectUid
            && otherProject.Name == this.Name
            && otherProject.LegacyProjectId == this.LegacyProjectId
            && otherProject.StartDate == this.StartDate
            && otherProject.EndDate == this.EndDate
            && otherProject.ProjectGeofenceWKT == this.ProjectGeofenceWKT
            && otherProject.ProjectTimeZone == this.ProjectTimeZone
            && otherProject.ProjectType == this.ProjectType
            && otherProject.IsArchived == this.IsArchived
            ;
    }
  }
}