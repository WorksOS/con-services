using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger,
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
    public GetFilterExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetFilters request
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
        MasterData.Repositories.DBModels.Filter filter = null;
        // get filterUid where !deleted 
        //   must be ok for 
        //      customer /project
        //      and UserUid: If the calling context is == Application, then get all 
        //                     else get only those for the calling UserUid
        try
        {
          filter = await filterRepo.GetFilter(filterRequest.FilterUid).ConfigureAwait(false);
        }
        catch (Exception e)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 6, e.Message);
        }

        if (filter == null
            || !string.Equals(filter.CustomerUid, filterRequest.CustomerUid, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(filter.ProjectUid, filterRequest.ProjectUid, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(filter.UserId, filterRequest.UserId, StringComparison.OrdinalIgnoreCase)
          )
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 36);
        }
        else
        {
          result = new FilterDescriptorSingleResult(AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter));
        }
      }
      else
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 5);
      }

      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

  }
}