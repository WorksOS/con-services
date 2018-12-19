using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.DataOcean.Client;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.MasterData.Repositories;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from controller logic for testability and possible executor versioning.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    protected ILogger log;
    
    /// <summary>
    /// Configuration items
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// handle exceptions
    /// </summary>
    protected IServiceExceptionHandler serviceExceptionHandler;

    protected string customerUid;
    protected string userId;
    protected string userEmailAddress;

    protected IDictionary<string, string> customHeaders;

    /// <summary>
    /// Gets or sets the Kafak consumer.
    /// </summary>
    protected IKafka producer;

    /// <summary>
    /// Gets or sets the Kafka topic.
    /// </summary>
    protected string kafkaTopicName;
    
    /// <summary>
    /// Interface to 3dp service validation
    /// </summary>
    protected IRaptorProxy raptorProxy;

    /// <summary>
    /// 
    /// </summary>
    protected ISubscriptionProxy subscriptionProxy;

    /// <summary>
    /// 
    /// </summary>
    protected ITransferProxy persistantTransferProxy;

    /// <summary>
    /// Interface to filter service for importFile validation
    /// </summary>
    protected IFilterServiceProxy filterServiceProxy;

    /// <summary>
    ///  Trex Import files interface
    /// </summary>
    protected ITRexImportFileProxy tRexImportFileProxy;

    /// <summary>
    /// Repository factory used extensively for project DB
    /// </summary>
    protected IProjectRepository projectRepo;

    /// <summary>
    /// Repository factory used for subscription checking
    /// </summary>
    protected ISubscriptionRepository subscriptionRepo;

    /// <summary>
    /// Repository factory used for accessing files in TCC (at present)
    /// </summary>
    /// 
    protected IFileRepository fileRepo;

    /// <summary>
    /// Repository factory used for Customer db
    /// </summary>
    /// 
    protected ICustomerRepository customerRepo;

    /// <summary>
    /// Context of the API call
    /// </summary>
    /// 
    protected IHttpContextAccessor httpContextAccessor;

    protected IDataOceanClient dataOceanClient;
    protected ITileServiceProxy tileServiceProxy;
    protected ITPaaSApplicationAuthentication authn;

    /// <summary>
    /// Generates the dynamic errorlist for instanciated executor.
    /// </summary>
    /// <returns>List of errors with corresponding descriptions.</returns>
    public List<Tuple<int, string>> GenerateErrorlist()
    {
      List<Tuple<int, string>> result = new List<Tuple<int, string>>();
      for (int i = 0; i < ContractExecutionStates.Count; i++)
      {
        result.Add(new Tuple<int, string>(ContractExecutionStates.ValueAt(i),
          ContractExecutionStates.NameAt(i)));
      }
      ContractExecutionStates.ClearDynamic();
      return result;
    }

    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    /// <returns></returns>
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing asynchronous executor process method override"));
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
      ValidateTItem(item);
      return ProcessEx(item);
    }

    public async Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      ValidateTItem(item);
      return await ProcessAsyncEx(item);
    }

    private static void ValidateTItem<T>(T item)
    {
      if (item == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Serialization error"));
      }
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
    /// 
    /// </summary>
    public void Initialise(ILogger logger, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler,
      string customerUid, string userId = null, string userEmailAddress = null,
      IDictionary<string, string> headers = null,
      IKafka producer = null, string kafkaTopicName = null,
      IRaptorProxy raptorProxy = null, ISubscriptionProxy subscriptionProxy = null,
      ITransferProxy persistantTransferProxy = null, IFilterServiceProxy filterServiceProxy = null, 
      ITRexImportFileProxy tRexImportFileProxy = null, IProjectRepository projectRepo = null, 
      ISubscriptionRepository subscriptionRepo = null, IFileRepository fileRepo = null, 
      ICustomerRepository customerRepo = null, IHttpContextAccessor httpContextAccessor = null, 
      IDataOceanClient dataOceanClient= null, ITileServiceProxy tileServiceProxy = null,
      ITPaaSApplicationAuthentication authn = null)
    {
      log = logger;
      this.configStore = configStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.customerUid = customerUid;
      this.userId = userId;
      this.userEmailAddress = userEmailAddress;
      this.customHeaders = headers;
      this.producer = producer;
      this.kafkaTopicName = kafkaTopicName;
      this.raptorProxy = raptorProxy;
      this.subscriptionProxy = subscriptionProxy;
      this.persistantTransferProxy = persistantTransferProxy;
      this.filterServiceProxy = filterServiceProxy;
      this.tRexImportFileProxy = tRexImportFileProxy;
      this.projectRepo = projectRepo;
      this.subscriptionRepo = subscriptionRepo;
      this.fileRepo = fileRepo;
      this.customerRepo = customerRepo;
      this.httpContextAccessor = httpContextAccessor;
      this.dataOceanClient = dataOceanClient;
      this.tileServiceProxy = tileServiceProxy;
      this.authn = authn;
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
    /// <typeparam name="TExecutor">The type of the executor.</typeparam>
    /// <returns></returns>
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler, IProjectRepository projectRepo, IKafka producer = null, string kafkaTopicName = null)
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor() { log = logger.CreateLogger<TExecutor>(), configStore = configStore, serviceExceptionHandler = serviceExceptionHandler, projectRepo = projectRepo, producer  = producer, kafkaTopicName = kafkaTopicName };
      return executor;
    }


    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="projectUid">The project uid.</param>
    /// <returns></returns>
    public async Task ValidateProjectWithCustomer(string customerUid, string projectUid)
    {
      var project = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(prj => string.Equals(prj.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"projectUid {projectUid} validated");
    }
  }
}

