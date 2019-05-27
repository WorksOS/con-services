using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using System.Collections.Immutable;
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
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository, IGeofenceProxy geofenceProxy)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, raptorProxy, assetResolverProxy, fileImportProxy, repository, producer, kafkaTopicName, auxRepository, geofenceProxy)
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
    /// Processes the GetFilters Request for a project
    /// </summary>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as BaseRequestFull;
      if (request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 52);

        return null;
      }

      var boundaries =  await BoundaryHelper.GetProjectBoundaries(
        log, serviceExceptionHandler,
        request.ProjectUid, (IProjectRepository)auxRepository, (IGeofenceRepository)Repository).ConfigureAwait(false);

      var geofences = await GeofenceProxy.GetFavoriteGeofences(request.CustomerUid, request.UserUid, request.CustomHeaders);
      geofences.AddRange(boundaries.GeofenceData);

      return new GeofenceDataListResult
      {
        GeofenceData = geofences.ToImmutableList()
      };

    }
  }
}
