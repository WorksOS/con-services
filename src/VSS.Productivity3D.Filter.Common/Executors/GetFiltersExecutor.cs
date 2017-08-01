using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.ResultHandling;
using VSS.Productivity3D.Filter.Common.Internal;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy,
      IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger,
      serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFiltersExecutor()
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Processes the GetFilters request for a project
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        var projectRequest = item as FilterRequestFull;
        if (projectRequest != null)
        {
          // get all for projectUid where !deleted 
          //   must be ok for 
          //      customer /project
          //      and UserUid: If the calling context is == Application, then get all 
          //                     else get only those for the calling UserUid
          var filters = (await filterRepo.GetFiltersForProject(projectRequest.projectUid).ConfigureAwait(false))
            .Where(f => f.CustomerUid == projectRequest.customerUid
                        && (/*projectRequest.isApplicationContext ||*/ f.UserUid == projectRequest.userUid));

          // may be zero, return success and empty list
          result = new FilterDescriptorListResult
          {
            filterDescriptors = filters.Select(filter =>
                AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
              .ToImmutableList()
          };
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 9);
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10, e.Message);
      }
      return result;
    }

    protected override void ProcessErrorCodes()
    {
    }

  }
}