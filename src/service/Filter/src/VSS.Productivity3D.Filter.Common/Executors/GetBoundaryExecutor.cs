using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetBoundaryExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetBoundaryExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, auxRepository /*, null, null */)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetBoundaryExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetBoundary Request
    /// </summary>
    /// <param name="item"></param>
    /// <returns>a polygon boundary if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<BoundaryUidRequestFull>(item, 51);

      IEnumerable<ProjectGeofence> associations = null;
      try
      {
        //Check it belongs to the project
        associations =
          await ((IProjectRepository)auxRepository).GetAssociatedGeofences(request.ProjectUid)
            .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48, e.Message);
      }

      if (associations == null || !associations.Any(a => string.Equals(request.BoundaryUid, a.GeofenceUID.ToString(), StringComparison.OrdinalIgnoreCase)))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 49);
      }

      Geofence boundary = null;
      try
      {
        boundary = await ((IGeofenceRepository)Repository).GetGeofence(request.BoundaryUid).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48, e.Message);
      }

      if (boundary == null || !string.Equals(boundary.GeofenceUID, request.BoundaryUid,
            StringComparison.OrdinalIgnoreCase))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 49);
      }

      return new GeofenceDataSingleResult(AutoMapperUtility.Automapper.Map<GeofenceData>(boundary));
    }
  }
}
