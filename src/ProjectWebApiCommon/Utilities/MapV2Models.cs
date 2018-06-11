using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Extended mapping for V2 models
  /// </summary>
  public class MapV2Models
  {
    // there no legacyIds: Customer or Project
    public static CreateProjectEvent MapCreateProjectV2RequestToEvent(CreateProjectV2Request source, string customerUid)
    {
      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(source);
      createProjectEvent.ProjectUID = Guid.NewGuid();
      createProjectEvent.CustomerUID = Guid.Parse(customerUid);
      createProjectEvent.ProjectBoundary = GeofenceValidation.GetWicketFromPoints(GeofenceValidation.MakingValidPoints(source.BoundaryLL));
      return createProjectEvent;
    }
  }
}
