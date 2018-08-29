using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Utilities
{
  public class BoundaryHelper
  {
    public static async Task<GeofenceDataListResult> GetProjectBoundaries(
      ILogger log, IServiceExceptionHandler serviceExceptionHandler,
      string projectUid, IProjectRepository projectRepository, IGeofenceRepository geofenceRepository)
    {
      IEnumerable<Geofence> geofences = null;
      try
      {
        IEnumerable<ProjectGeofence> associations = await projectRepository
          .GetAssociatedGeofences(projectUid)
          .ConfigureAwait(false);

        var projectGeofences = associations.ToList();
        if (projectGeofences.Any())
        {
          geofences = await geofenceRepository
            .GetGeofences(projectGeofences.Select(a => a.GeofenceUID.ToString()))
            .ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 49, e.Message);
      }

      // may be none, return success and empty list
      return new GeofenceDataListResult
      {
        GeofenceData = (geofences ?? new List<Geofence>())
          .Select(x => AutoMapperUtility.Automapper.Map<GeofenceData>(x))
          .ToImmutableList()
      };
    }
  }
}
