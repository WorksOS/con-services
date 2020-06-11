using System;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Productivity3D.Project.Abstractions.Models.Cws;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  /// <summary>
  /// Project data for validation
  /// </summary>
  public class ProjectValidation
  {
    public Guid CustomerUid { get; set; }
    public Guid? ProjectUid { get; set; }
    public ProjectType? ProjectType { get; set; } 
    public string ProjectName { get; set; }
    public string ProjectBoundaryWKT { get; set; }
    public ProjectUpdateType UpdateType { get; set; }
    public string CoordinateSystemFileName { get; set; }
    public byte[] CoordinateSystemFileContent { get; set; }
  }
}
