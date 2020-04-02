using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Core.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class DeleteBoundaryExecutor : RequestExecutorContainer
  {
    /// <inheritdoc />
    public DeleteBoundaryExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, auxRepository /*, null, null*/)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteBoundaryExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the Filter Boundary Request asynchronously.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>Returns an <see cref="BoundaryRequest"/> object if successful.</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<BoundaryUidRequestFull>(item, 53);
      if (request == null) return null;

      IEnumerable<ProjectGeofence> associations = null;
      try
      {
        //Check it belongs to the project
        associations =
          await (auxRepository as IProjectRepository).GetAssociatedGeofences(request.ProjectUid)
            .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 48, e.Message);
      }

      if (!associations.Any(a => string.Equals(request.BoundaryUid, a.GeofenceUID.ToString(), StringComparison.OrdinalIgnoreCase)))
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

      log.LogDebug($"DeleteBoundary retrieved Geofence {JsonConvert.SerializeObject(boundary)}");

      DeleteGeofenceEvent deleteEvent = null;
      int deletedCount = 0;

      try
      {
        deleteEvent = AutoMapperUtility.Automapper.Map<DeleteGeofenceEvent>(request);
        deleteEvent.ActionUTC = DateTime.UtcNow;

        deletedCount = await ((IGeofenceRepository)Repository).StoreEvent(deleteEvent).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 13, e.Message);
      }

      if (deletedCount == 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12);
      }

      //NOTE: A gap at the moment is that we don't support deleting an association between project and boundary (geofence).
      //That should be done here as part of boundary deletion.

      return new ContractExecutionResult();
    }
  }
}
