﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetBoundaryExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetBoundaryExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IFileImportProxy fileImportProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, raptorProxy, assetResolverProxy, fileImportProxy, repository, producer, kafkaTopicName, auxRepository, null, null)
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
          await ((IProjectRepository) auxRepository).GetAssociatedGeofences(request.ProjectUid)
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
