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
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName)
      : base(configStore, logger, serviceExceptionHandler,
          projectListProxy, raptorProxy,
          repository, producer, kafkaTopicName, null)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFiltersExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all filters for the project.
    /// </summary>
    /// <returns>If successful returns a <see cref="FilterDescriptorListResult"/> containing a collection of filters for the project.</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var filterRequest = item as FilterRequestFull;
      if (filterRequest == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 9);
        return null;
      }

      List<MasterData.Repositories.DBModels.Filter> filters = null;

      // get all for ProjectUid where !deleted 
      //   must be ok for 
      //      customer /project
      //      and UserUid: If the calling context is == Application, then get all 
      //                     else get only those for the calling UserUid
      try
      {
        filters = (List<MasterData.Repositories.DBModels.Filter>)await ((IFilterRepository)this.Repository)
          .GetFiltersForProjectUser(filterRequest.CustomerUid, filterRequest.ProjectUid, filterRequest.UserId)
          .ConfigureAwait(false);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10, e.Message);
      }

      FilterJsonHelper.ParseFilterJson(filterRequest.ProjectData, filters);

      // may be none, return success and empty list
      return new FilterDescriptorListResult
      {
        FilterDescriptors = filters?.Select(filter => AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };
    }
  }
}