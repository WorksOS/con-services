using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class DeleteFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public DeleteFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, 
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, 
      IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) 
      : base(configStore, logger, serviceExceptionHandler, 
          projectListProxy, raptorProxy, 
          filterRepo, producer, kafkaTopicName)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFilterExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the DeleteFilter request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;

      var filterRequest = item as FilterRequestFull;
      if (filterRequest != null)
      {
        var projectFilter =
          (await filterRepo.GetFiltersForProjectUser(filterRequest.customerUid, filterRequest.projectUid,
            filterRequest.userId).ConfigureAwait(false))
          .SingleOrDefault(f => f.FilterUid == filterRequest.filterUid);
        
        if (projectFilter == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
        }
        log.LogDebug($"DeleteFilter retrieved filter {JsonConvert.SerializeObject(projectFilter)}");

        DeleteFilterEvent deleteFilterEvent = null;
        int deletedCount = 0;
        try
        {
          deleteFilterEvent = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(projectFilter);
          deletedCount = await filterRepo.StoreEvent(deleteFilterEvent).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 13, e.Message);
        }

        if (deletedCount == 0)
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12);

        // this just clears the cache, do we care i.e. fail and rollback?
        try
        {
          if (deletedCount > 0)
            await FilterValidation
              .NotifyRaptorFilterChange(raptorProxy, log, serviceExceptionHandler, projectFilter?.FilterUid)
              .ConfigureAwait(false);

        }
        catch (Exception e)
        {
          log.LogError(
            $"DeleteFilter failed to clear 3dp cache for {JsonConvert.SerializeObject(projectFilter)}. Exception: {e.Message}");
        }

        try
        {
          if (deleteFilterEvent != null)
          {
            var messagePayloadDeleteEvent =
              JsonConvert.SerializeObject(new {DeleteFilterEvent = deleteFilterEvent});
            producer.Send(kafkaTopicName,
              new List<KeyValuePair<string, string>>
              {
                new KeyValuePair<string, string>(deleteFilterEvent.FilterUID.ToString(), messagePayloadDeleteEvent)
              });
          }
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 14, e.Message);
        }

        return new ContractExecutionResult();
      }
      else
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 37);
      }

      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

  }
}