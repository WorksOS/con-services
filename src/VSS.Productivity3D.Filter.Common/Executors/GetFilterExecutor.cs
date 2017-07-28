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
  public class GetFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy, IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger, serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
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
      try
      {
        string filterUid = item as string;

        // todo 
        // get !deleted 
        //   for customer/project
        //      and UserUid: If the calling context is == Application, then get all 
        //                     else get only those for the calling UserUid
        // try catch
        var filter = await filterRepo.GetFilter(filterUid).ConfigureAwait(false);
        if (filter == null)
        {
          throw new NotImplementedException(); 
        }

        // todo map filters to result result = FiltersResult.CreateFiltersResult(projectUid, projectSettings?.Settings);
        result = new FilterDescriptorSingleResult(new FilterDescriptor(){FilterUid = filter.FilterUid, Name = filter.Name, FilterJson = filter.FilterJson});
        return result;
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