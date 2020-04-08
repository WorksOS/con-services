using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Configuration;

namespace CustomerWebApi.Controllers
{
  public class BaseController : Controller
  {
    public readonly string ConnectionString = string.Empty;
    protected BaseController(IConfigurationStore configurationStore, ILoggerFactory loggerFactory)
    {
      ConnectionString = configurationStore.GetConnectionString("VSPDB");
      _logger = loggerFactory.CreateLogger(GetType().Name);
    }
    /// <summary> base message number for ProjectService </summary>
    protected readonly int customErrorMessageOffset = 2000;

    /// <summary> Gets or sets the Kafka topic. </summary>
    protected readonly string KafkaTopicName;

    private ILogger _logger;
    private ILoggerFactory _loggerFactory;
    //private IServiceExceptionHandler _serviceExceptionHandler;

    private ITPaaSApplicationAuthentication _authorization;


    /// <summary> Gets the application logging interface. </summary>
    protected ILogger Logger => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger>());

    /// <summary> Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders. </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory ?? (_loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());

    /// <summary> Gets the service exception handler. </summary>
    //protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ?? (_serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary> Gets the config store. </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary> Gets the kafka producer </summary>
    /// <summary> Gets or sets the TPaaS application authentication helper. </summary>
    protected ITPaaSApplicationAuthentication Authorization => _authorization ?? (_authorization = HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>());

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    /// <remarks>
    /// Following #83476 we are deliberately passing the x-jwt-assertion header on all requests regardless of whether they're 
    /// 'internal' or not.
    /// </remarks>
    protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();
    //protected IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders(true); //use this when debugging locally and calling other 3dpm services


    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    /// <returns></returns>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func
      <Task<TResult>> action)
      where TResult : ContractExecutionResult
    {
      var result = default(TResult);
      try
      {
        result = await action.Invoke().ConfigureAwait(false);
        if (Logger.IsTraceEnabled())
          Logger.LogTrace($"Executed {action.GetMethodInfo().Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException se)
      {
        Logger.LogError(se, $"Execution failed for: {action.GetMethodInfo().Name}. ");
        throw;
      }
      catch (Exception ex)
      {
        //ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
        //  ContractExecutionStatesEnum.InternalProcessingError - customErrorMessageOffset, ex.Message, innerException: ex);
      }
      finally
      {
        Logger.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }

  }
}
