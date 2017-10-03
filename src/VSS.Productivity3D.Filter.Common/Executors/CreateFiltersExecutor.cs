using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Repositories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class CreateFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public CreateFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
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
    public CreateFiltersExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the UpsertFilter request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var newFilters = new List<FilterDescriptor>();

      var filterRequest = item as FilterListRequestFull;
      if (filterRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 38);
      }
      else
      {
        foreach (var filter in filterRequest.filterRequests)
        {
          var newFilter = await ProcessTransient(filterRequest.CustomerUid, filterRequest.UserId,
            filterRequest.ProjectUid, filter).ConfigureAwait(false);
          newFilters.Add(newFilter);
        }
      }

      return new FilterDescriptorListResult() {filterDescriptors = newFilters.ToImmutableList()};
    }

    private async Task<FilterDescriptor> ProcessTransient(string customerUid, string userId, string projectUid,
      FilterRequest filterRequest)
    {
      var createdCount = 0;
      CreateFilterEvent filterEvent = null;
      try
      {
        filterRequest.filterUid = Guid.NewGuid().ToString();
        filterEvent = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
        filterEvent.CustomerUID = Guid.Parse(customerUid);
        filterEvent.UserID = userId;
        filterEvent.ProjectUID = Guid.Parse(projectUid);
        filterEvent.ActionUTC = DateTime.UtcNow;
        createdCount = await filterRepo.StoreEvent(filterEvent).ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 20, e.Message);
      }

      if (createdCount == 0)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 19);

      MasterData.Repositories.DBModels.Filter retrievedFilter = null;
      try
      {
        // would be nice to compare LastActionedUtc here, but that makes it unusable using Moq
        retrievedFilter = (await (filterRepo)
            .GetFiltersForProjectUser(customerUid, projectUid, userId, true)
            .ConfigureAwait(false))
          .OrderByDescending(f => f.LastActionedUtc)
          .FirstOrDefault(f => string.IsNullOrEmpty(f.Name) && f.FilterJson == filterEvent?.FilterJson);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 20, e.Message);
      }

      if (retrievedFilter == null)
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 19);

      return AutoMapperUtility.Automapper.Map<FilterDescriptor>(retrievedFilter);
    }


    protected override void ProcessErrorCodes()
    {
    }
  }
}