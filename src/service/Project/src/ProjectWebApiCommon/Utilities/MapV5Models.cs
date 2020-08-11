using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Extended mapping for V5 models
  /// </summary>
  public class MapV5Models
  {
    // there no legacyIds: Customer or Project
    public static ProjectValidation MapCreateProjectV5RequestToProjectValidation(CreateProjectV5Request source, string customerUid)
    {
      var projectValidation = new ProjectValidation()
      {
        CustomerUid = new Guid(customerUid),
        ProjectType = CwsProjectType.AcceptsTagFiles,
        ProjectName = source.ProjectName,
        UpdateType = ProjectUpdateType.Created,
        CoordinateSystemFileName = source.CoordinateSystem.Name
      };

      var internalPoints = AutoMapperUtility.Automapper.Map<List<Point>>(source.BoundaryLL);
      projectValidation.ProjectBoundaryWKT =
        GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(internalPoints));
      return projectValidation;
    }
  }
}
