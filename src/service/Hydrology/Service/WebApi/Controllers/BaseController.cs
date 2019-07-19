using System;
using System.Collections.Generic;
using System.Net;
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

namespace VSS.Hydrology.WebApi.Controllers
{
  /// <summary>
  /// Common base class for Raptor service controllers.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {

    private IServiceExceptionHandler serviceExceptionHandler;
    /// <summary>
    /// Gets the service exception handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler => serviceExceptionHandler ?? (serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    private ILogger<T> logger;
    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => logger ?? (logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    private ILoggerFactory loggerFactory;
    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    protected ILoggerFactory LoggerFactory => loggerFactory ?? (loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());


    private IConfigurationStore configStore;
    /// <summary>
    /// Gets access to environment variables from various sources
    /// </summary>
    protected IConfigurationStore ConfigStore => configStore ?? (configStore = HttpContext.RequestServices.GetService<IConfigurationStore>());

    private ILandLeveling landLeveling;
    /// <summary>
    /// Hydro engine to generate surface etc
    /// </summary>
    protected ILandLeveling LandLeveling => landLeveling ?? (landLeveling = HttpContext.RequestServices.GetService<ILandLeveling>());

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    private readonly MemoryCacheEntryOptions filterCacheOptions = new MemoryCacheEntryOptions
    {
      SlidingExpiration = TimeSpan.FromDays(3)
    };

    /// <summary> </summary>
    protected BaseController() { }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    protected TResult WithServiceExceptionTryExecute<TResult>(Func<TResult> action) where TResult : ContractExecutionResult
    {
      var result = default(TResult);
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
      var result = default(TResult);
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
  }
}
