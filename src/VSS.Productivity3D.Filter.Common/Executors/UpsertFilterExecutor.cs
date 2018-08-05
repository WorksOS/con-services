using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Filter.Common.Validators;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class UpsertFilterExecutor : FilterExecutorBase
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public UpsertFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName, RepositoryBase auxRepository)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, fileListProxy, repository, producer, kafkaTopicName, auxRepository)
    {
      FileListProxy = fileListProxy;
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public UpsertFilterExecutor()
    { }

    /// <summary>
    /// Processes the UpsertFilter Request
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      FilterDescriptorSingleResult result;
      var filterRequest = item as FilterRequestFull;
      if (filterRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 38);
      }

      // Hydrate the polygon filter if present.
      filterRequest.FilterJson = await ValidationUtil.HydrateJsonWithBoundary(auxRepository as GeofenceRepository, log, serviceExceptionHandler, filterRequest);

      if (filterRequest.FilterType == FilterType.Transient)
      {
        result = await ProcessTransient(filterRequest).ConfigureAwait(false);
      }
      else
      {
        // Hydrate the alignment and design filenames if present (persistent filters only).
        FilterFilenameUtil.GetFilterFileNames(log, serviceExceptionHandler, FileListProxy, filterRequest);

        result = await ProcessPersistent(filterRequest).ConfigureAwait(false);
      }

      FilterJsonHelper.ParseFilterJson(filterRequest.ProjectData, result.FilterDescriptor, raptorProxy, filterRequest.CustomHeaders);

      return result;
    }

    private async Task<FilterDescriptorSingleResult> ProcessTransient(FilterRequestFull filterRequest)
    {
      // if filterUid supplied, then exception as cannot update a transient filter
      //   else create new one Note that can have duplicate transient name (i.e. "") per cust/prj/user
      if (!string.IsNullOrEmpty(filterRequest.FilterUid))
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 16);
      }

      return await CreateFilter(filterRequest);
    }

    private async Task<FilterDescriptorSingleResult> ProcessPersistent(FilterRequestFull filterRequest)
    {
      // if FilterUid supplied, and it exists for customer/user/project, then update it
      // if Name exists, then exception
      // else create new filter
      // write to kafka (update or create)
      var existingPersistentFilters = new List<MasterData.Repositories.DBModels.Filter>();
      try
      {
        existingPersistentFilters =
        (await ((IFilterRepository)Repository)
          .GetFiltersForProjectUser(filterRequest.CustomerUid, filterRequest.ProjectUid, filterRequest.UserId, true)
          .ConfigureAwait(false)).Where(f => f.FilterType == filterRequest.FilterType).ToList();
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 15, e.Message);
      }

      if (!string.IsNullOrEmpty(filterRequest.FilterUid))
      {
        var existingFilter = existingPersistentFilters.SingleOrDefault(
          f => string.Equals(f.FilterUid, filterRequest.FilterUid, StringComparison.OrdinalIgnoreCase));

        if (existingFilter == null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 21);
        }

        // don't allow update to Name to a Name which already exists (for a different filterUid) for persistent filters
        //Allowed duplicate name for report filters
        if (filterRequest.FilterType == FilterType.Persistent)
        {
          var filterOfSameName = existingPersistentFilters
            .FirstOrDefault(f => string.Equals(f.Name, filterRequest.Name, StringComparison.OrdinalIgnoreCase)
                                 && !string.Equals(f.FilterUid, filterRequest.FilterUid,
                                   StringComparison.OrdinalIgnoreCase));

          if (filterOfSameName != null)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 39);
          }
        }

        // only Name can be updated, NOT FilterJson. Do this here as well as in AutoMapper, just to be sure!
        filterRequest.FilterJson = existingFilter.FilterJson;
        var updateFilterEvent = await StoreFilterAndNotifyRaptor<UpdateFilterEvent>(filterRequest, new[] { 17, 18 });

        if (updateFilterEvent != null)
        {
          var payload = JsonConvert.SerializeObject(new { UpdateFilterEvent = updateFilterEvent });
          SendToKafka(updateFilterEvent.FilterUID.ToString(), payload, 26);
        }

        return RetrieveFilter(updateFilterEvent);

      }

      if (filterRequest.FilterType == FilterType.Persistent)
      {
        var filterOfSameName = existingPersistentFilters
          .FirstOrDefault(f => (string.Equals(f.Name, filterRequest.Name, StringComparison.OrdinalIgnoreCase)));

        if (filterOfSameName != null)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 39);
        }
      }

      return await CreateFilter(filterRequest);
    }

    /// <summary>
    /// Creates the requested filter
    /// </summary>
    private async Task<FilterDescriptorSingleResult> CreateFilter(FilterRequestFull filterRequest)
    {
      var isTransient = filterRequest.FilterType == FilterType.Transient;

      filterRequest.FilterUid = Guid.NewGuid().ToString();
      var createFilterEvent = await StoreFilterAndNotifyRaptor<CreateFilterEvent>(filterRequest, isTransient ? new[] { 19, 20 } : new[] { 24, 25 });

      //Only write to kafka for persistent filters
      if (isTransient && createFilterEvent != null)
      {
        var payload = JsonConvert.SerializeObject(new { CreateFilterEvent = createFilterEvent });
        SendToKafka(createFilterEvent.FilterUID.ToString(), payload, 26);
      }

      return RetrieveFilter(createFilterEvent);
    }

    /// <summary>
    /// Retrieve the filter just saved
    /// </summary>
    private FilterDescriptorSingleResult RetrieveFilter<T>(T filterRequest)
    {
      var mappingResult = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filterRequest));

      return mappingResult;
    }
  }
}
