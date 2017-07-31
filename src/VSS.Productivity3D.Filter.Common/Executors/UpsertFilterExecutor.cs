using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class UpsertFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public UpsertFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy,
      IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger,
      serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpsertFilterExecutor()
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
      ContractExecutionResult result = null;
      try
      {
        var filterRequest = item as FilterRequestFull;
        if (filterRequest != null)
        {
          MasterData.Repositories.DBModels.Filter filter = null;
          var projectFilters =
            (await filterRepo.GetFiltersForProject(filterRequest.projectUid).ConfigureAwait(false))
            .Where(f => f.UserUid == filterRequest.userUid);
          log.LogDebug(
            $"UpsertFilter retrieved filter count for projectUID {filterRequest.projectUid} of {projectFilters.Count()}");

          // todo!
          if (string.IsNullOrEmpty(filterRequest.name))
            result = await ProcessTransient(filterRequest, projectFilters).ConfigureAwait(false);
          else
            result = await ProcessPersistant(filterRequest, projectFilters).ConfigureAwait(false);

          return result;
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
      }
      return result;
    }


    protected override void ProcessErrorCodes()
    {
    }

    private async Task<FilterDescriptorSingleResult> ProcessTransient(FilterRequestFull filterRequest,
      IEnumerable<MasterData.Repositories.DBModels.Filter> projectFilters)
    {
      // transient
      //   if filterUid supplied, and it exists, then update it. 
      //   else if one exists for the UserUid then update it.

      MasterData.Repositories.DBModels.Filter filter = null;
      if (!string.IsNullOrEmpty(filterRequest.filterUid))
        filter = projectFilters.SingleOrDefault(f => string.Equals(f.FilterUid, filterRequest.filterUid,
          StringComparison.OrdinalIgnoreCase));

      if (filter == null)
        filter = projectFilters.SingleOrDefault(
          f => string.Equals(f.UserUid, filterRequest.userUid, StringComparison.OrdinalIgnoreCase));

      if (filter != null)
      {
        try
        {
          var filterEvent = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
          filterEvent.FilterUID = Guid.Parse(filter.FilterUid);
          var updatedCount = await filterRepo.StoreEvent(filterEvent).ConfigureAwait(false);
          if (updatedCount == 0)
          {
            // error trying to update a transient filter
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
          }
        }
        catch (Exception e)
        {
          // exception trying to update a transient filter
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
        }
      }
      else // (filter == null)
      {
        try
        {
          var filterEvent = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
          filterEvent.FilterUID = Guid.NewGuid();
          var createdCount = await filterRepo.StoreEvent(filterEvent).ConfigureAwait(false);
          if (createdCount == 0)
          {
            // error trying to update a transient filter
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
          }
        }
        catch (Exception e)
        {
          // exception trying to update a transient filter
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
        }
      }
      return new FilterDescriptorSingleResult(new FilterDescriptor());
    }

    private async Task<FilterDescriptorSingleResult> ProcessPersistant(FilterRequestFull filterRequest,
      IEnumerable<MasterData.Repositories.DBModels.Filter> projectFilters)
    {
      // if permanent then a) if old name exists, do a delete(temp) of old
      //                                        and create new
      //                   b) name doesn't exist then create new
      //   if filterUid supplied, and it exists, then update it. 
      //   else if one exists for the UserUid then update it.

      MasterData.Repositories.DBModels.Filter filter = null;
      if (!string.IsNullOrEmpty(filterRequest.filterUid))
        filter = projectFilters.SingleOrDefault(
          f => string.Equals(f.UserUid, filterRequest.userUid, StringComparison.OrdinalIgnoreCase));

      if (filter != null && filter.UserUid != filterRequest.userUid)
      {
        // todo filterUid belongs to a different user
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
      }

      var filterByThisName = projectFilters.SingleOrDefault(
        f => string.Equals(f.UserUid, filterRequest.userUid, StringComparison.OrdinalIgnoreCase) &&
             f.Name == filterRequest.name);
      if (filterByThisName != null && filter != null && filter.FilterUid != filterByThisName.FilterUid)
      {
        // todo user has another filter with this name. todo do we delete this and create a new one?
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
      }

      if (filter != null)
      {
        try
        {
          var filterEvent = AutoMapperUtility.Automapper.Map<UpdateFilterEvent>(filterRequest);
          filterEvent.FilterUID = Guid.Parse(filter.FilterUid);
          var updatedCount = await filterRepo.StoreEvent(filterEvent).ConfigureAwait(false);
          if (updatedCount == 0)
          {
            // error trying to update a transient filter
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
          }
        }
        catch (Exception e)
        {
          // exception trying to update a transient filter
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
        }
      }
      else // (filter == null)
      {
        try
        {
          var filterEvent = AutoMapperUtility.Automapper.Map<CreateFilterEvent>(filterRequest);
          filterEvent.FilterUID = Guid.NewGuid();
          var createdCount = await filterRepo.StoreEvent(filterEvent).ConfigureAwait(false);
          if (createdCount == 0)
          {
            // error trying to update a transient filter
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69);
          }
        }
        catch (Exception e)
        {
          // exception trying to update a transient filter
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 69, e.Message);
        }
      }

      return new FilterDescriptorSingleResult(new FilterDescriptor());
    }

  }
}