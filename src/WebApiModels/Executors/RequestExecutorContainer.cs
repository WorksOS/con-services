using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
  
    /// <summary>
    /// Logger used in ProcessEx
    /// </summary>
    protected ILogger log;

    protected IConfigurationStore configStore;

    protected AssetRepository assetRepository;
    protected DeviceRepository deviceRepository;
    protected CustomerRepository customerRepository;
    protected ProjectRepository projectRepository;
    protected SubscriptionRepository subscriptionsRepository;
    protected IKafka producer;
    protected string kafkaTopicName;

    /// <summary>
    /// allows mapping between CG (which Raptor requires) and NG
    /// </summary>
    protected ServiceTypeMappings serviceTypeMappings = new ServiceTypeMappings();

    protected static DataRepository dataRepository = null;

    
    /// <summary>
    ///   Generates the errorlist for instantiated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      return (from object enumVal in Enum.GetValues(typeof(ContractExecutionStatesEnum))
              select new Tuple<int, string>((int)enumVal, enumVal.ToString())).ToList();
    }


    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item); 

    protected abstract Task<ContractExecutionResult> ProcessAsyncEx<T>(T item);

    internal static object Build<T>()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return ProcessEx(item);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      if (item == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      return await ProcessAsyncEx(item);
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    /// <param name="subscriptionsRepository"></param>
    /// <param name="logger">Ilogger</param>
    /// <param name="configStore"></param>
    /// <param name="assetRepository"></param>
    /// <param name="deviceRepository"></param>
    /// <param name="customerRepository"></param>
    /// <param name="projectRepository"></param>
    /// <param name="producer"></param>
    /// <param name="kafkaTopicName"></param>
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(ILogger logger, IConfigurationStore configStore, 
      IAssetRepository assetRepository = null, IDeviceRepository deviceRepository = null, 
      ICustomerRepository customerRepository = null, IProjectRepository projectRepository = null,
      ISubscriptionRepository subscriptionsRepository = null, IKafka producer = null, string kafkaTopicName = null) where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { log = logger, configStore = configStore,
        assetRepository = assetRepository as AssetRepository, deviceRepository = deviceRepository as DeviceRepository,
        customerRepository = customerRepository as CustomerRepository, projectRepository = projectRepository as ProjectRepository,
        subscriptionsRepository = subscriptionsRepository as SubscriptionRepository,
        producer = producer, kafkaTopicName = kafkaTopicName};
      dataRepository = new DataRepository(logger, configStore, assetRepository, deviceRepository, 
        customerRepository, projectRepository, 
        subscriptionsRepository,
        producer, kafkaTopicName);
      return executor;
    }
    
  }
}