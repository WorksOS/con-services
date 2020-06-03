using System;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// Project data for validation
  /// </summary>
  public class ProjectValidation
  {
    public Guid CustomerUid { get; set; }
    public Guid? ProjectUid { get; set; }
    public CwsProjectType? ProjectType { get; set; } 
    public string ProjectName { get; set; }
    public string ProjectBoundaryWKT { get; set; }
    public ProjectUpdateType UpdateType { get; set; }
    public string CoordinateSystemFileSpaceId { get; set; }
  }
}
