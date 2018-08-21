using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.TRex.Exports.Surfaces.Requestors;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Rendering.Servers.Client;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Gateway.Common.Executors
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

    protected ITileRenderingServer tileRenderServer;
    protected IMutableClientServer tagfileClientServer;
    public ITINSurfaceExportRequestor tINSurfaceExportRequestor;
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
    protected virtual Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new ServiceException(HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          "Missing asynchronous executor process method override"));
    }

    /// <summary>
    /// 
    /// </summary>
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
    /// Injected constructor.
    /// </summary>
    protected RequestExecutorContainer(IConfigurationStore configStore, ILoggerFactory logger, 
      IServiceExceptionHandler serviceExceptionHandler,
      ITileRenderingServer tileRenderServer, 
      IMutableClientServer tagfileClientServer) : this()
    {
      this.configStore = configStore;
      if (logger != null)
        log = logger.CreateLogger<RequestExecutorContainer>();
      this.serviceExceptionHandler = serviceExceptionHandler;
      this.tileRenderServer = tileRenderServer;
      this.tagfileClientServer = tagfileClientServer;
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
      Build<TExecutor>(IConfigurationStore configStore, ILoggerFactory logger, 
      IServiceExceptionHandler serviceExceptionHandler, 
      ITileRenderingServer tileRenderServer, 
      IMutableClientServer tagfileClientServer
        )
      where TExecutor : RequestExecutorContainer, new()
    {
      var executor = new TExecutor
      {
        configStore = configStore,
        log = logger.CreateLogger<TExecutor>(),
        serviceExceptionHandler = serviceExceptionHandler,
        tileRenderServer = tileRenderServer,
        tagfileClientServer = tagfileClientServer,
      };

      return executor;
    }
  }
}
