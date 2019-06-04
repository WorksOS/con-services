using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using System.Collections.Immutable;
using System.Linq;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetBoundariesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy,
      IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository,
      IGeofenceProxy geofenceProxy, IUnifiedProductivityProxy unifiedProductivityProxy)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, assetResolverProxy,
        fileListProxy, repository, producer, kafkaTopicName, auxRepository, geofenceProxy, unifiedProductivityProxy)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetBoundariesExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the 'Get Custom Boundaries' Request for a project
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as BaseRequestFull;
      if (request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 52);

        return null;
      }

      var boundaries = new List<GeofenceData>();
      var projectRepo = (IProjectRepository) auxRepository;
      //a) Custom boundaries 
      var boundariesTask = BoundaryHelper.GetProjectBoundaries(
        log, serviceExceptionHandler, request.ProjectUid, projectRepo, (IGeofenceRepository) Repository);
      //b) favorite geofences that overlap project 
      var favoritesTask =
        GeofenceProxy.GetFavoriteGeofences(request.CustomerUid, request.UserUid, request.CustomHeaders);
      //c) unified productivity associated geofences
      var associatedTask = UnifiedProductivityProxy.GetAssociatedGeofences(request.ProjectUid, request.CustomHeaders);
      await Task.WhenAll(boundariesTask, favoritesTask, associatedTask);

      boundaries.AddRange(boundariesTask.Result.GeofenceData);
      if (associatedTask.Result != null)
        boundaries.AddRange(associatedTask.Result);

      //Find out which favorite geofences overlap project boundary
      var overlappingGeofences =
        (await projectRepo.DoPolygonsOverlap(request.ProjectGeometryWKT,
          favoritesTask.Result.Select(g => g.GeometryWKT))).ToList();
      for (var i = 0; i < favoritesTask.Result.Count; i++)
      {
        if (overlappingGeofences[i])
          boundaries.Add(favoritesTask.Result[i]);
      }

      //Remove any duplicates
      boundaries = boundaries.Distinct(new DistinctGeofenceComparer()).ToList();

      return new GeofenceDataListResult
      {
        GeofenceData = boundaries.ToImmutableList()
      };

    }
  }
}
