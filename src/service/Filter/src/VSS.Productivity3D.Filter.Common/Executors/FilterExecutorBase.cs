using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public abstract class FilterExecutorBase : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    protected FilterExecutorBase(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, fileListProxy, repository, producer, kafkaTopicName, auxRepository)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    protected FilterExecutorBase()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Store the filter in the database and notify Raptor of a filter change
    /// </summary>
    /// <typeparam name="T">The type of event</typeparam>
    /// <param name="filterRequest">The filter data</param>
    /// <param name="errorCodes">Error codes to use for exceptions</param>
    protected async Task<T> StoreFilterAndNotifyRaptor<T>(FilterRequestFull filterRequest, int[] errorCodes) where T : IFilterEvent
    {
      var filterEvent = default(T);

      try
      {
        filterEvent = AutoMapperUtility.Automapper.Map<T>(filterRequest);
        filterEvent.ActionUTC = DateTime.UtcNow;

        var count = await ((IFilterRepository)Repository).StoreEvent(filterEvent).ConfigureAwait(false);
        if (count == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCodes[0]);
        }
        else
        {
          // It's not necessary to invalidate the Raptor services proxy filter cache when a filter is created, or if it's transient.
          if (filterRequest.FilterType == FilterType.Transient || filterEvent is CreateFilterEvent)
          {
            return filterEvent;
          }

          await NotifyRaptor(filterRequest);
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCodes[1], e.Message);
      }

      return filterEvent;
    }

    /// <summary>
    /// Notify 3dpm service that a filter has been added/updated/deleted.
    /// </summary>
    private async Task NotifyRaptor(FilterRequestFull filterRequest)
    {
      BaseDataResult notificationResult = null;

      try
      {
        notificationResult = await raptorProxy.NotifyFilterChange(new Guid(filterRequest.FilterUid),
          new Guid(filterRequest.ProjectUid), filterRequest.CustomHeaders);
      }
      catch (ServiceException se)
      {
        log.LogError(se, $"FilterExecutorBase: RaptorServices failed with service exception. FilterUid:{filterRequest.FilterUid}.");
        //rethrow this to surface it
        throw;
      }
      catch (Exception e)
      {
        log.LogError(e, $"FilterExecutorBase: RaptorServices failed with exception. FilterUid:{filterRequest.FilterUid}.");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 30, "raptorProxy.NotifyFilterChange", e.Message);
      }

      log.LogDebug(
        $"FilterExecutorBase: NotifyFilterChange in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 29, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }

    /// <summary>
    /// Send a filter message to kafka.
    /// </summary>
    protected void SendToKafka(string filterUid, string payload, int errorCode)
    {
      try
      {
        producer.Send(kafkaTopicName,
          new List<KeyValuePair<string, string>>
          {
            new KeyValuePair<string, string>(filterUid, payload)
          });
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCode, e.Message);
      }
    }
  }
}
