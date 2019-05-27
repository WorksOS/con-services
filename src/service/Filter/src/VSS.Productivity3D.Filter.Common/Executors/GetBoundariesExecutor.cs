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
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository, IGeofenceProxy geofenceProxy)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, assetResolverProxy, fileListProxy, repository, producer, kafkaTopicName, auxRepository, geofenceProxy)
    { }

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
      var request = item as BaseRequestFull;
      if (request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 52);

        return null;
      }
      //a) Custom boundaries 
      var boundaries = new List<GeofenceData>();
      var projectRepo = (IProjectRepository) auxRepository;
      var customBoundaries = await BoundaryHelper.GetProjectBoundaries(
        log, serviceExceptionHandler,
        request.ProjectUid, projectRepo, (IGeofenceRepository) Repository);
      boundaries.AddRange(customBoundaries.GeofenceData);
      //b) favorite geofences that overlap project 
      var geofences = await GeofenceProxy.GetFavoriteGeofences(request.CustomerUid, request.UserUid, request.CustomHeaders);
      //Find out which geofences overlap project boundary
      var overlappingGeofences =
        (await projectRepo.DoPolygonsOverlap(request.ProjectGeometryWKT, geofences.Select(g => g.GeometryWKT))).ToList();
      for (var i = 0; i < geofences.Count; i++)
      {
        if (overlappingGeofences[i])
          boundaries.Add(geofences[i]);
      }
      //c) unified productivity associated geofences
      var associatedGeofences = await UnifiedProductivityProxy.GetAssociatedGeofences(request.ProjectUid);
      boundaries.AddRange(associatedGeofences);
      //Remove any duplicates
      boundaries = boundaries.Distinct(new DistinctGeofenceComparer()).ToList();
      return new GeofenceDataListResult
      {
        GeofenceData = boundaries.ToImmutableList()
      };

    }
  }
}
