using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.MasterData.Project.WebAPI.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Utilities
{
  /// <summary>
  /// Extended mapping for V2 models
  /// </summary>
  public class MapV2Models
  {
    public static CreateProjectEvent MapCreateProjectV2RequestToEvent(CreateProjectV2Request source, string customerUid, long legacyCustomerId)
    {
      // todo try to do all within AutomapperUtility

      var createProjectEvent = AutoMapperUtility.Automapper.Map<CreateProjectEvent>(source);

      // todo var points = AutoMapperUtility.Automapper.Map<List<Point>>(source.BoundaryLL);
      var points = ConvertPoints(source.BoundaryLL);

      var boundaryString =
        ProjectBoundaryValidator.GetWicketFromPoints(ProjectBoundaryValidator.MakingValidPoints(points));
      createProjectEvent.ProjectBoundary = boundaryString;

      createProjectEvent.CustomerID = legacyCustomerId;
      createProjectEvent.CustomerUID = Guid.Parse(customerUid);
      createProjectEvent.ProjectUID = Guid.NewGuid();
      createProjectEvent.ReceivedUTC = createProjectEvent.ActionUTC = DateTime.UtcNow;

      createProjectEvent.CoordinateSystemFileName = source.CoordinateSystem.Name;
      // not able to do this yet createProjectEvent.CoordinateSystemFileContent = ??? todo

      return createProjectEvent;
    }

    private static List<Point> ConvertPoints(List<PointLL> latLngs)
    {
      return latLngs.ConvertAll<Point>(delegate (PointLL ll) { return new Point(ll.latitude, ll.longitude); });
    }

  }
}
