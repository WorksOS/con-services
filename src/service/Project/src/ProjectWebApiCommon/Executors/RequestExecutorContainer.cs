using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.AWS.TransferProxy.Interfaces;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Pegasus.Client;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Repository;
using VSS.Productivity3D.Scheduler.Abstractions;
using VSS.TCCFileAccess;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Common.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to separate executor logic
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

    protected IHeaderDictionary customHeaders;

    /// <summary>
    /// Interfaces to Productivity3d
    /// </summary>
    protected IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord;

    protected IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification;

    protected IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction;

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
    /// Repository factory used for accessing files in TCC (at present)
    /// </summary>
    protected IFileRepository fileRepo;

    /// <summary>
    /// Context of the API call
    /// </summary>
    protected IHttpContextAccessor httpContextAccessor;

    protected IDataOceanClient dataOceanClient;
    protected ITPaaSApplicationAuthentication authn;
    protected ISchedulerProxy schedulerProxy;
    protected IPegasusClient pegasusClient;
    protected ICwsProjectClient cwsProjectClient;
    protected ICwsDeviceClient cwsDeviceClient;
    protected ICwsDesignClient cwsDesignClient;
    protected ICwsProfileSettingsClient cwsProfileSettingsClient;

    /// <summary>
    /// Processes the specified item. This is the main method to execute real action.
    /// </summary>
    /// <typeparam name="T">>Generic type which should be</typeparam>
    /// <param name="item">>The item.</param>
    protected abstract ContractExecutionResult ProcessEx<T>(T item);

    /// <summary>
    /// 
    /// </summary>
    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing asynchronous executor process method override"));
    }

    /// <summary>
    /// 
    /// </summary>
    public ContractExecutionResult Process<T>(T item)
    {
      ValidateTItem(item);
      return ProcessEx(item);
    }

    public Task<ContractExecutionResult> ProcessAsync<T>(T item)
    {
      ValidateTItem(item);
      return ProcessAsyncEx(item);
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

    public void Initialise(ILogger logger, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler,
      string customerUid, string userId = null, string userEmailAddress = null,
      IHeaderDictionary headers = null,
      IProductivity3dV1ProxyCoord productivity3dV1ProxyCoord = null,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification = null,
      IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction = null,
      ITransferProxy persistantTransferProxy = null, IFilterServiceProxy filterServiceProxy = null,
      ITRexImportFileProxy tRexImportFileProxy = null, IProjectRepository projectRepo = null,
      IFileRepository fileRepo = null, IHttpContextAccessor httpContextAccessor = null,
      IDataOceanClient dataOceanClient = null, ITPaaSApplicationAuthentication authn = null,
      ISchedulerProxy schedulerProxy = null, IPegasusClient pegasusClient = null,
      ICwsProjectClient cwsProjectClient = null, ICwsDeviceClient cwsDeviceClient = null,
      ICwsDesignClient cwsDesignClient = null, ICwsProfileSettingsClient cwsProfileSettingsClient = null)
    {
      log = logger;
      this.configStore = configStore;
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.customerUid = customerUid;
      this.userId = userId;
      this.userEmailAddress = userEmailAddress;
      this.customHeaders = headers;
      this.productivity3dV1ProxyCoord = productivity3dV1ProxyCoord;
      this.productivity3dV2ProxyNotification = productivity3dV2ProxyNotification;
      this.productivity3dV2ProxyCompaction = productivity3dV2ProxyCompaction;
      this.persistantTransferProxy = persistantTransferProxy;
      this.filterServiceProxy = filterServiceProxy;
      this.tRexImportFileProxy = tRexImportFileProxy;
      this.projectRepo = projectRepo;
      this.fileRepo = fileRepo;
      this.httpContextAccessor = httpContextAccessor;
      this.dataOceanClient = dataOceanClient;
      this.authn = authn;
      this.schedulerProxy = schedulerProxy;
      this.pegasusClient = pegasusClient;
      this.cwsProjectClient = cwsProjectClient;
      this.cwsDeviceClient = cwsDeviceClient;
      this.cwsDesignClient = cwsDesignClient;
      this.cwsProfileSettingsClient = cwsProfileSettingsClient;
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
    public static TExecutor Build<TExecutor>(ILoggerFactory logger, IConfigurationStore configStore, IServiceExceptionHandler serviceExceptionHandler,
      IProjectRepository projectRepo)
      where TExecutor : RequestExecutorContainer, new()
    {
      return new TExecutor
      {
        log = logger.CreateLogger<TExecutor>(),
        configStore = configStore,
        serviceExceptionHandler = serviceExceptionHandler,
        projectRepo = projectRepo
      };
    }


    /// <summary>
    /// Validates a project identifier.
    /// </summary>
    public async Task ValidateProjectWithCustomer(string customerUid, string projectUid)
    {
      var project = (await projectRepo.GetProjectsForCustomer(customerUid).ConfigureAwait(false)).FirstOrDefault(prj => string.Equals(prj.ProjectUID, projectUid, StringComparison.OrdinalIgnoreCase));

      if (project == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 1);
      }

      log.LogInformation($"projectUid {projectUid} validated");
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

