using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  /// <summary>
  ///   Represents abstract container for all Request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    /// <summary>
    /// Configuration items
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;

    /// <summary>
    /// handle exceptions
    /// </summary>
    protected IServiceExceptionHandler serviceExceptionHandler;

    /// <summary>
    /// Proxy used to validate Customer/project relationship
    /// </summary>
    protected IProjectProxy projectProxy;

    /// <summary>
    /// Gets the proxy used to retrieve files, e.g. design or alignment.
    /// </summary>
    protected IFileImportProxy fileImportProxy;

    /// <summary>
    /// Implementation of the proxy interface for <see cref="IRaptorProxy"/>.
    /// </summary>
    protected IRaptorProxy raptorProxy;

    /// <summary>
    /// Main DB repo for filters/boundaries used in ProcessEx
    /// </summary>
    protected RepositoryBase Repository;

    /// <summary>
    /// Gets or sets the Kafak consumer.
    /// </summary>
    protected IKafka producer;

    /// <summary>
    /// Gets or sets the Kafka topic.
    /// </summary>
    protected string kafkaTopicName;

    /// <summary>
    /// Auxilary DB repo for filters/boundaries used in ProcessEx
    /// </summary>
    protected RepositoryBase auxRepository;

    protected IGeofenceProxy GeofenceProxy;

    protected IUnifiedProductivityProxy UnifiedProductivityProxy;

    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam Name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam Name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "Missing asynchronous executor process method override"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <typeparam Name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ServiceException"></exception>
    public ContractExecutionResult Process<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }

      return ProcessEx(item);
    }

    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }

      return await ProcessAsyncEx(item);
    }

    /// <summary>
    /// Gets the available contract execution error states.
    /// </summary>
    /// <value>
    /// The contract execution states.
    /// </value>
    protected ContractExecutionStatesEnum ContractExecutionStates { get; }

    /// <summary>
    /// Default constructor which creates all structures necessary for error handling.
    /// </summary>
    protected RequestExecutorContainer()
    {
      ContractExecutionStates = new ContractExecutionStatesEnum();
      ProcessErrorCodes();
    }

    /// <summary>
    /// Dynamically defines new error codes for the executor instance. Don't forget to clean them up after exit.
    /// </summary>
    protected virtual void ProcessErrorCodes()
    { }

    /// <summary>
    /// Injected constructor for mocking Raptor client.
    /// </summary>
    protected RequestExecutorContainer(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy, IRaptorProxy raptorProxy, IFileImportProxy fileImportProxy, RepositoryBase repository,
      IKafka producer, string kafkaTopicName, RepositoryBase auxRepository, IGeofenceProxy geofenceProxy, IUnifiedProductivityProxy unifiedProductivityProxy) : this()
    {
      this.configStore = configStore;
      if (logger != null)
        log = logger.CreateLogger<RequestExecutorContainer>();
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.projectProxy = projectProxy;
      this.raptorProxy = raptorProxy;
      Repository = repository;
      this.producer = producer;
      this.kafkaTopicName = kafkaTopicName;
      this.auxRepository = auxRepository;
      this.fileImportProxy = fileImportProxy;
      GeofenceProxy = geofenceProxy;
      UnifiedProductivityProxy = unifiedProductivityProxy;
    }

    /// <summary>
    /// Default destructor which destroys all structures necessary for error handling.
    /// </summary>
    ~RequestExecutorContainer()
    {
      ContractExecutionStates?.ClearDynamic();
    }

    /// <summary>
    ///   Builds this instance for specified executor type.
    /// </summary>
    /// <typeparam Name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor
      Build<TExecutor>(IConfigurationStore configStore,
      ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      RepositoryBase repository, RepositoryBase auxRepository,
      IProjectProxy projectProxy = null, IRaptorProxy raptorProxy = null, 
      IKafka producer = null, string kafkaTopicName = null, IFileImportProxy fileImportProxy = null, IGeofenceProxy geofenceProxy = null,
      IUnifiedProductivityProxy unifiedProductivityProxy = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor
      {
        configStore = configStore,
        log = logger.CreateLogger<TExecutor>(),
        serviceExceptionHandler = serviceExceptionHandler,
        projectProxy = projectProxy,
        raptorProxy = raptorProxy,
        fileImportProxy = fileImportProxy,
        Repository = repository,
        producer = producer,
        kafkaTopicName = kafkaTopicName,
        auxRepository = auxRepository,
        GeofenceProxy = geofenceProxy,
        UnifiedProductivityProxy = unifiedProductivityProxy
      };

      return executor;
    }

    /// <summary>
    /// Casts input object to type T for use with child executors.
    /// </summary>
    protected T CastRequestObjectTo<T>(object item, int errorCode) where T : class
    {
      var request = item as T;

      if (request == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCode);
      }

      return request;
    }
  }
}
