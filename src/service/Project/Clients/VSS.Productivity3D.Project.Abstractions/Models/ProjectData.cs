using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models
{
  /// <summary>
  ///   Describes VL project
  /// </summary>
  public class ProjectData : IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the project uid.
    /// </summary>
    public string ProjectTrn { get; set; }

    /// <summary>
    /// Gets or sets the short project ID from for Raptor
    /// </summary>
    public int shortRaptorProjectId { get; set; }

    /// <summary>
    /// Gets or sets the type of the project. Only standard supported
    /// </summary>
    public ProjectType ProjectType { get; set; }

    /// <summary>
    /// Gets the name of the project type.
    /// </summary>
    public string ProjectTypeName => this.ProjectType.ToString();

    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Description of the project.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the legacy project time zone. todoMaverick is this still used?
    /// </summary>
    public string ProjectTimeZone { get; set; }

    /// <summary>
    /// Gets or sets the IANA project time zone.
    /// </summary>
    public string IanaTimeZone { get; set; }

    /// <summary>
    /// Gets or sets the start date.
    /// </summary>
    public string StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date.
    /// </summary>
    public string EndDate { get; set; }

    /// <summary>
    /// Gets or sets the AccountTrn which the project is associated with
    /// </summary>
    public string AccountTrn { get; set; }

    
    /// <summary>
    /// Gets or sets the project boundary.
    /// </summary>
    public string ProjectGeofenceWKT { get; set; }

    /// <summary>
    /// Gets or sets the CoordinateSystem FileName which the project is associated with
    /// </summary>
    public string CoordinateSystemFileName { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether this instance is archived.
    /// </summary>
    public bool IsArchived { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>
      {
        AccountTrn,
        ProjectTrn
      };
    }
  }
}
