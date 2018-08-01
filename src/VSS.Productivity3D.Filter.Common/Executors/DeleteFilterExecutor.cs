using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class DeleteFilterExecutor : FilterExecutorBase
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public DeleteFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy,fileListProxy, repository, producer, kafkaTopicName, null)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFilterExecutor()
    { }

    /// <summary>
    /// Processes the DeleteFilter Request
    /// </summary>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var filterRequest = item as FilterRequestFull;
      if (filterRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 37);

        return null;
      }

      var filter =
        (await ((IFilterRepository)Repository).GetFiltersForProjectUser(filterRequest.CustomerUid, filterRequest.ProjectUid,
          filterRequest.UserId, true).ConfigureAwait(false))
        .SingleOrDefault(f => string.Equals(f.FilterUid, filterRequest.FilterUid, StringComparison.OrdinalIgnoreCase));

      if (filter == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
      }
      log.LogDebug($"DeleteFilter retrieved filter {JsonConvert.SerializeObject(filter)}");

      DeleteFilterEvent deleteEvent = await StoreFilterAndNotifyRaptor<DeleteFilterEvent>(filterRequest, new int[] { 12, 13 });
      //Only write to kafka for persistent filters
      if (deleteEvent != null && filter.FilterType != FilterType.Transient)
      {
        var payload = JsonConvert.SerializeObject(new { DeleteFilterEvent = deleteEvent });
        SendToKafka(deleteEvent.FilterUID.ToString(), payload, 14);
      }
 
      return new ContractExecutionResult();
    }
  }
}
