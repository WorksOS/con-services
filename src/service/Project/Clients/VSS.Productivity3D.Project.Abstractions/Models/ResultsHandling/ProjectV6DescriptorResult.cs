using System;
using System.Collections.Immutable;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Extensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{

  /// <summary>
  /// List of project descriptors
  /// </summary>
  /// <seealso cref="ContractExecutionResult" />
  public class ProjectV6DescriptorsListResult : ContractExecutionResult
  {
    /// <summary>
    /// Gets or sets the project descriptors.
    /// </summary>
    /// <value>
    /// The project descriptors.
    /// </value>
    public ImmutableList<ProjectV6Descriptor> ProjectDescriptors { get; set; }
  }

  /// <summary>
  ///   Single project descriptor
  /// </summary>
  public class ProjectV6DescriptorsSingleResult : ContractExecutionResult
  {
    private ProjectV6Descriptor _projectV6Descriptor;

    public ProjectV6DescriptorsSingleResult(ProjectV6Descriptor projectV6Descriptor)
    {
      _projectV6Descriptor = projectV6Descriptor;
    }

    /// <summary>
    /// Gets or sets the project descriptor.
    /// </summary>
    /// <value>
    /// The project descriptor.
    /// </value>
    public ProjectV6Descriptor ProjectDescriptor { get { return _projectV6Descriptor; } set { _projectV6Descriptor = value; } }
  }


  /// <summary>
  ///   Describes VL project
  /// </summary>
  public class ProjectV6Descriptor
  {
    /// <summary>
    /// Gets or sets the project uid.
    /// </summary>
    /// <value>
    /// The project uid.
    /// </value>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Gets or sets the project ID for TBC
    /// </summary>
    /// <value>
    /// The legacy project ID.
    /// </value>
    public long ShortRaptorProjectId
    {
      get { return Guid.TryParse(ProjectUid, out var g) ? g.ToLegacyId() : 0; }
    }

    /// <summary>
  /// Gets or sets the type of the project.
  /// </summary>
  /// <value>
  /// The type of the project.
  /// </value>
  public CwsProjectType ProjectType { get; set; }

    /// <summary>
    /// What Role the current user has in this project
    ///    e.g. ADMIN or none
    /// </summary>
    public UserProjectRoleEnum UserProjectRole { get; set; }


    /// <summary>
    /// Gets the name of the project type.
    /// </summary>
    /// <value>
    /// The name of the project type.
    /// </value>
    public string ProjectTypeName => ProjectType.ToString();

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
    /// Gets or sets the IANA time zone.
    /// </summary>
    /// <value>
    /// The IANA project time zone.
    /// </value>
    public string IanaTimeZone { get; set; }

    /// <summary>
    /// Gets or sets the CustomerUID which the project is associated with
    /// </summary>
    /// <value>
    /// The Customer UID.
    /// </value>
    public string CustomerUid { get; set; }


    /// <summary>
    /// Gets or sets the project geofence.
    /// </summary>
    /// <value>
    /// The project geofence in WKT format.
    /// </value>
    public string ProjectGeofenceWKT { get; set; }

    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    /// <value>
    /// The CoordinateSystem FileName.
    /// </value>
    public string CoordinateSystemFileName { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is archived; otherwise, <c>false</c>.
    /// </value>
    public bool IsArchived { get; set; }


    public override bool Equals(object obj)
    {
      var otherProject = obj as ProjectV6Descriptor;
      if (otherProject == null) return false;
      return otherProject.ProjectUid == ProjectUid
             && otherProject.ShortRaptorProjectId == ShortRaptorProjectId
             && otherProject.ProjectType == ProjectType
             && otherProject.Name == Name
             && otherProject.ProjectTimeZone == ProjectTimeZone
             && otherProject.IanaTimeZone == IanaTimeZone
             && otherProject.CustomerUid == CustomerUid
             && otherProject.ProjectGeofenceWKT == ProjectGeofenceWKT
             && otherProject.CoordinateSystemFileName == CoordinateSystemFileName
             && otherProject.IsArchived == IsArchived
          ;
    }

    public override int GetHashCode() { return 0; }
  }
}
