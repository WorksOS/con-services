using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetBoundariesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IFileImportProxy fileImportProxy, 
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository,
      IGeofenceProxy geofenceProxy, IUnifiedProductivityProxy unifiedProductivityProxy)
       : base(configStore, logger, serviceExceptionHandler, projectProxy, raptorProxy, assetResolverProxy,
       fileImportProxy, repository, producer, kafkaTopicName, auxRepository, geofenceProxy, unifiedProductivityProxy)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetBoundariesExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the 'Get Custom Boundaries' Request for a project
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<BaseRequestFull>(item, 52);
      if (request == null) return null;

      var boundaries = new List<GeofenceData>();
      var projectRepo = (IProjectRepository) auxRepository;

      Task<GeofenceDataListResult> boundariesTask = null;
      Task<List<GeofenceData>> favoritesTask = null;
      Task<List<GeofenceData>> associatedTask = null;
      try
      {
        //a) Custom boundaries 
        boundariesTask = BoundaryHelper.GetProjectBoundaries(
        log, serviceExceptionHandler, request.ProjectUid, projectRepo, (IGeofenceRepository) Repository);
        //b) favorite geofences that overlap project 
        favoritesTask =
          GeofenceProxy.GetFavoriteGeofences(request.CustomerUid, request.UserUid, request.CustomHeaders);
        //c) unified productivity associated geofences
        associatedTask = UnifiedProductivityProxy.GetAssociatedGeofences(request.ProjectUid, request.CustomHeaders);
 
        await Task.WhenAll(boundariesTask, favoritesTask, associatedTask);
      }
      catch (Exception e)
      {
        log.LogError(e, "Failed to retrieve all boundaries");
      }

      try
      {
        if (boundariesTask != null && !boundariesTask.IsFaulted && boundariesTask.Result != null)
          boundaries.AddRange(boundariesTask.Result.GeofenceData);
        if (associatedTask != null && !associatedTask.IsFaulted && associatedTask.Result != null)
          boundaries.AddRange(associatedTask.Result);
        if (favoritesTask != null && !favoritesTask.IsFaulted && favoritesTask.Result != null)
        {
          //Find out which favorite geofences overlap project boundary
          var overlappingGeofences =
            (await projectRepo.DoPolygonsOverlap(request.ProjectGeometryWKT,
              favoritesTask.Result.Select(g => g.GeometryWKT))).ToList();
          for (var i = 0; i < favoritesTask.Result.Count; i++)
          {
            if (overlappingGeofences[i])
              boundaries.Add(favoritesTask.Result[i]);
          }
        }
      }
      catch (Exception ex)
      {
        log.LogError(ex, "Failed to aggregate all boundaries");
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
