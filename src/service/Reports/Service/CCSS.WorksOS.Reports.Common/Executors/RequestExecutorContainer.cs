using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;

namespace CCSS.WorksOS.Reports.Common.Executors
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to separate executor logic
  ///   from controller logic for testability and possible executor version.
  /// </summary>
  public abstract class RequestExecutorContainer
  {
    protected ILogger _log;
    protected IConfigurationStore _configStore;
    protected IServiceExceptionHandler _serviceExceptionHandler;
    protected IHttpContextAccessor httpContextAccessor;

    protected string _customerUid;
    protected string _userUid;
    protected IHeaderDictionary _customHeaders;
    
    protected IPreferenceProxy _preferenceProxy;
    protected IWebRequest _gracefulClient;

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
      string customerUid, string userUid, IHeaderDictionary headers,
      IPreferenceProxy preferenceProxy, IWebRequest gracefulClient)
    {
      _log = logger;
      _configStore = configStore;
      _serviceExceptionHandler = serviceExceptionHandler;
      _customerUid = customerUid;
      _userUid = userUid;
      _customHeaders = headers;
      _preferenceProxy = preferenceProxy;
      _gracefulClient = gracefulClient;
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

