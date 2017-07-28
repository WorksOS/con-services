using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Filter.Common.ResultHandling;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class UpsertFilterExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public UpsertFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler, IProjectListProxy projectListProxy, IFilterRepository filterRepo, IKafka producer, string kafkaTopicName) : base(configStore, logger, serviceExceptionHandler, projectListProxy, filterRepo, producer, kafkaTopicName)
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
        string projectUid = item as string;

        // todo check if filter exists (process differently if persistant/transient and if IsApplication vs User)
        //var filters = await filterRepo.GetFiltersForProject(projectUid).ConfigureAwait(false);

        // todo if persistant, write kafka message/s
        // todo map filters to result result = FiltersResult.CreateFiltersResult(projectUid, projectSettings?.Settings);
        return new FilterDescriptorSingleResult(new FilterDescriptor());
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