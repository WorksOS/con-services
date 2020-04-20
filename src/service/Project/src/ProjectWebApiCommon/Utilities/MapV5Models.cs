using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Extended mapping for V5 models
  /// </summary>
  public class MapV5Models
  {
    // there no legacyIds: Customer or Project
    public static CreateProjectEvent MapCreateProjectV5RequestToEvent(CreateProjectV5Request source, string customerUid)
    {
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(source);

      // project identity must come from profileX now
      createProjectEvent.CustomerUID = new Guid(customerUid);

      var internalPoints = AutoMapperUtility.Automapper.Map<List<Point>>(source.BoundaryLL);
      createProjectEvent.ProjectBoundary =
        GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(internalPoints));
      createProjectEvent.ProjectType = ProjectType.Standard;
      return createProjectEvent;
    }
  }
}
