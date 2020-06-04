using VSS.Common.Abstractions.Clients.CWS.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.Cws
{
  /// <summary>
  /// A class that represents the payload CWS will send when requesting project validation before updating / creating
  /// </summary>
  public class ProjectValidateDto
  {
    public string AccountTrn { get; set; }

    /// <summary>
    /// Will be set if the project already exists (e.g update)
    /// Will be null for new projects
    /// </summary>
    public string ProjectTrn { get; set; }

    /// <summary>
    /// The project type the project is going to be.
    /// Will be set if a new project or the type is updated.
    /// Otherwise it will be null.
    /// </summary>
    public CwsProjectType? ProjectType { get; set; }

    /// <summary>
    /// The request update type this payload represents
    /// </summary>
    public ProjectUpdateType UpdateType { get; set; }

    /// <summary>
    /// Represents the project name requested for the project.
    /// Will be set if a new project or the name is updated.
    /// Otherwise it will be null
    /// </summary>
    public string ProjectName { get; set; }

    /// <summary>
    /// GeoJSON representation of the project boundary
    /// Will be set if a new project, or the boundary is updated
    /// Otherwise it will be null
    /// </summary>
    public ProjectBoundary Boundary { get; set; }

    /// <summary>
    /// DataOcean FileSpace ID for the coordinate system
    /// Set if project is being created, or if the Coordinate system is being updated
    /// Otherwise it will be null
    /// </summary>
    public string CoordinateSystemFileSpaceId { get; set; }
  }
}
