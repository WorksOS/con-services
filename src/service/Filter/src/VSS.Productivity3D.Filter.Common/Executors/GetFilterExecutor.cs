using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.AssetMgmt3D.Abstractions;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy, IRaptorProxy raptorProxy, IAssetResolverProxy assetResolverProxy, IFileListProxy fileListProxy,
      RepositoryBase repository, IKafka producer, string kafkaTopicName)
      : base(configStore, logger, serviceExceptionHandler, projectListProxy, raptorProxy, assetResolverProxy, fileListProxy, repository, producer, kafkaTopicName, null, null, null)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFilterExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetFilters Request
    /// </summary>
    /// <returns>If successful returns a <see cref="FilterDescriptorSingleResult"/> object containing the filter.</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<FilterRequestFull>(item, 9);
      if (request == null) return null;

      MasterData.Repositories.DBModels.Filter filter = null;
      // get FilterUid where !deleted 
      //   must be ok for 
      //      customer /project
      //      and UserUid: If the calling context is == Application, then get all 
      //                     else get only those for the calling UserUid
      try
      {
        filter = await ((IFilterRepository)Repository).GetFilter(request.FilterUid);
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 6, e.Message);
      }

      if (filter == null
          || !string.Equals(filter.CustomerUid, request.CustomerUid, StringComparison.OrdinalIgnoreCase)
          || !string.Equals(filter.ProjectUid, request.ProjectUid, StringComparison.OrdinalIgnoreCase)
          || !string.Equals(filter.UserId, request.UserId, StringComparison.OrdinalIgnoreCase) && !request.IsApplicationContext
      )
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 36);
      }


      await FilterJsonHelper.ParseFilterJson(request.ProjectData, filter, raptorProxy, assetResolverProxy, request.CustomHeaders);

      return new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));
    }
  }
}
