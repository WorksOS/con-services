﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Internal;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class DeleteFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public DeleteFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy, IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger, serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
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
          (await filterRepo.GetFiltersForProjectUser(filterRequest.customerUid, filterRequest.projectUid, filterRequest.userUid).ConfigureAwait(false))
            .SingleOrDefault(f => f.FilterUid == filterRequest.filterUid);
        log.LogDebug($"DeleteFilter retrieved filter {JsonConvert.SerializeObject(projectFilter)}");
        if (projectFilter == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
        }

        DeleteFilterEvent deleteFilterEvent = null;
        try
        {
          deleteFilterEvent = AutoMapperUtility.Automapper.Map<DeleteFilterEvent>(filterRequest);
          deleteFilterEvent.FilterUID = Guid.Parse(filterRequest.filterUid);
          deleteFilterEvent.ActionUTC = DateTime.UtcNow;
          var deletedCount = await filterRepo.StoreEvent(deleteFilterEvent).ConfigureAwait(false);
          if (deletedCount == 0)
          {
            // error trying to delete a persistant filter
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 12);
          }
        }
        catch (Exception e)
        {
          // exception trying to delete a persistant filter
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 13, e.Message);
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

      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

  }
}