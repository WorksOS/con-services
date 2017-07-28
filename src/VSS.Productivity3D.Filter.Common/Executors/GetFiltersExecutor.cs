using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy, IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger, serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
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
    /// Processes the GetFilters request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      ContractExecutionResult result = null;
      try
      {
        string projectUid = item as string;

        // todo 
        // get !deleted 
        //   for customer/project
        //      and UserUid: If the calling context is == Application, then get all 
        //                     else get only those for the calling UserUid
        //var filters = await filterRepo.GetFiltersForProject(projectUid).ConfigureAwait(false);
        // todo map filters to result result = FiltersResult.CreateFiltersResult(projectUid, projectSettings?.Settings);

        return new FilterDescriptorListResult
        {
          filterDescriptors = null
            //       = filters.Select(filter =>
            //  AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
            //.ToImmutableList()
        }; 
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

  }
}