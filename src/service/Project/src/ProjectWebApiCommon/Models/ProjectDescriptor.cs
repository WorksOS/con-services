using System.Collections.Immutable;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  /// <summary>
  /// Describes standard output for the project descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class ProjectDescriptorsListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public ImmutableList<ProjectDescriptor> ProjectDescriptors { get; set; }
  }


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
    /// Gets the name of the project type.
    /// </summary>
    /// <value>
    /// The name of the project type.
    /// </value>
    public string ProjectTypeName => this.ProjectType.ToString();

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

    public int ShortRaptorProjectId { get; set; }

    /// <summary>
    /// Gets or sets the CustomerUID which the project is associated with
    /// </summary>
    public string CustomerUID { get; set; }

    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    /// <value>
    /// The CoordinateSystem FileName.
    /// </value>
    public string CoordinateSystemFileName { get; set; }

    public override bool Equals(object obj)
    {
      var otherProject = obj as ProjectDescriptor;
      if (otherProject == null) return false;
      return otherProject.ProjectUid == this.ProjectUid
             && otherProject.Name == this.Name
             && otherProject.ShortRaptorProjectId == this.ShortRaptorProjectId
             && otherProject.StartDate == this.StartDate
             && otherProject.EndDate == this.EndDate
             && otherProject.ProjectGeofenceWKT == this.ProjectGeofenceWKT
             && otherProject.ProjectTimeZone == this.ProjectTimeZone
             && otherProject.ProjectType == this.ProjectType
             && otherProject.IsArchived == this.IsArchived
             && otherProject.CustomerUID == this.CustomerUID
             && otherProject.CoordinateSystemFileName == this.CoordinateSystemFileName
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}
