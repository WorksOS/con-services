using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.TagFileSplitter.WebAPI.Common.Executors
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
    protected ILogger Logger;

    /// <summary>
    /// Configuration items
    /// </summary>
    protected IConfigurationStore ConfigStore;

    /// <summary>
    /// handle exceptions
    /// </summary>
    private IServiceExceptionHandler _serviceExceptionHandler;

    protected IServiceResolution ServiceResolution;
    protected IGenericHttpProxy GenericHttpProxy;
    protected IDictionary<string, string> CustomHeaders;
    protected TargetServices TargetServices;
    protected int? TimeoutSeconds;
    protected string UserEmailAddress;


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

    /// <summary> </summary>
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
    private ContractExecutionStatesEnum ContractExecutionStates { get; }

    /// <summary>
    /// Default constructor which creates all structures necessary for error handling.
    /// </summary>
    protected RequestExecutorContainer()
    {
      ContractExecutionStates = new ContractExecutionStatesEnum();
    }

    /// <summary>
    /// 
    /// </summary>
    public void Initialise(ILogger logger, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler,
      IServiceResolution serviceResolution, IGenericHttpProxy genericHttpProxy, IDictionary<string, string> customHeaders,
      TargetServices targetServices, int? timeoutSeconds, string userEmailAddress = null)
    {
      Logger = logger;
      ConfigStore = configStore;
      _serviceExceptionHandler = serviceExceptionHandler;
      ServiceResolution = serviceResolution;
      GenericHttpProxy = genericHttpProxy;
      TargetServices = targetServices;
      TimeoutSeconds = timeoutSeconds;
      UserEmailAddress = userEmailAddress;
    }

    /// <summary>
    /// Default destructor which destroys all structures necessary for error handling.
    /// </summary>
    ~RequestExecutorContainer()
    {
      ContractExecutionStates?.ClearDynamic();
    }
    
    /// <summary>
    /// Casts input object to type T for use with child executors.
    /// </summary>
    protected T CastRequestObjectTo<T>(object item, int errorCode) where T : class
    {
      var request = item as T;

      if (request == null)
      {
        _serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCode);
      }

      return request;
    }
  }
}
