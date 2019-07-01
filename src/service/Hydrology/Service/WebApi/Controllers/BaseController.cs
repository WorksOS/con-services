using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
    protected readonly int customErrorMessageOffset = 4000; // todoJeannie how to set?

    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    private IServiceExceptionHandler _serviceExceptionHandler;
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ?? (_serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    private ILogger<T> _logger;
    protected ILogger<T> Log => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    private ILoggerFactory _loggerFactory;
    protected ILoggerFactory LoggerFactory => _loggerFactory ?? (_loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());


    private IConfigurationStore _configStore;
    protected IConfigurationStore ConfigStore => _configStore ?? (_configStore = HttpContext.RequestServices.GetService<IConfigurationStore>());

    private ILandLeveling _landLeveling;
    protected ILandLeveling LandLeveling => _landLeveling ?? (_landLeveling = HttpContext.RequestServices.GetService<ILandLeveling>());

    /// <summary>
    /// Gets the customer uid from the current context
    /// </summary>
    protected string customerUid => GetCustomerUid();

    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    /// <value>
    /// The user uid or applicationID as a string.
    /// </value>
    protected string userId => GetUserId();

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly MemoryCacheEntryOptions filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary>
    /// Indicates whether to use the TRex Gateway instead of calling to the Raptor client.
    /// </summary>
    protected bool UseTRexGateway(string key) => bool.TryParse(ConfigStore.GetValueString(key), out var useTrexGateway) && useTrexGateway;

    /// <summary>
    /// 
    /// </summary>
    protected BaseController() { }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = action.Invoke();
        if (Log.IsTraceEnabled())
          Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, errorMessage1: ex.Message, innerException: ex);
      }
      finally
      {
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// With the service exception try execute async.
    /// </summary>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func<Task<TResult>> action) where TResult : ContractExecutionResult
    {
      TResult result = default(TResult);
      try
      {
        result = await action.Invoke();
        if (Log.IsTraceEnabled())
          Log.LogTrace($"Executed {action.Method.Name} with result {JsonConvert.SerializeObject(result)}");

      }
      catch (ServiceException)
      {
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - 2000, ex.Message, innerException: ex);
      }
      finally
      {
        Log.LogInformation($"Executed {action.Method.Name} with the result {result?.Code}");
      }
      return result;
    }

    /// <summary> </summary>
    private string GetCustomerUid()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.CustomerUid;
      }

      throw new ArgumentException("Incorrect customer in request context principal.");
    }

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    private string GetUserId()
    {
      if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}
