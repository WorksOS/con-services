using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
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

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetBoundariesExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetBoundariesExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy,fileListProxy, repository, producer, kafkaTopicName, auxRepository)
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

      IEnumerable<Geofence> geofences = null;
      try
      {
        IEnumerable<ProjectGeofence> associations = 
          await (auxRepository as IProjectRepository).GetAssociatedGeofences(request.ProjectUid)
          .ConfigureAwait(false);

        if (associations.Count() > 0)
        {
          geofences = await ((IGeofenceRepository) Repository)
            .GetGeofences(associations.Select(a => a.GeofenceUID.ToString()))
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
        GeofenceData = (geofences ?? new List<Geofence> ())
          .Select(x => AutoMapperUtility.Automapper.Map<GeofenceData>(x))
          .ToImmutableList()
      };
    }
  }
}
