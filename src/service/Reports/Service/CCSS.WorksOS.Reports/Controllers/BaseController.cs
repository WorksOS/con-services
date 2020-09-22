using System;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace CCSS.WorksOS.Reports.Controllers
{
  public class BaseController<T> : Controller where T : BaseController<T>
  {
    /// <summary> base message number for ReportsService </summary>
    protected readonly int CustomErrorMessageOffset = 4000;

    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IConfigurationStore _configStore;
    private IServiceExceptionHandler _serviceExceptionHandler;

    private IPreferenceProxy _preferenceProxy;
    private IWebRequest _gracefulClient;


    protected ILogger<T> Log => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();
    protected IConfigurationStore ConfigStore => _configStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetService<IServiceExceptionHandler>();

    protected IPreferenceProxy PreferenceProxy => _preferenceProxy ??= HttpContext.RequestServices.GetService<IPreferenceProxy>();
    protected IWebRequest GracefulClient => _gracefulClient ??= HttpContext.RequestServices.GetService<IWebRequest>();

    protected IHeaderDictionary customHeaders => Request.Headers.GetCustomHeaders();

    private string _customerUid;

    protected string CustomerUid
    {
      get
      {
        if (!string.IsNullOrEmpty(_customerUid))
        {
          return _customerUid;
        }

        if (User is TIDCustomPrincipal principal)
        {
          _customerUid = principal.CustomerUid;
          return _customerUid;
        }

        throw new ArgumentException("Incorrect customer in request context principal.");
      }
    }

    private string _useruid;

    /// <summary>
    /// Gets the UserUID/applicationID from the current context.
    /// </summary>
    protected string UserUid
    {
      get
      {
        if (!string.IsNullOrEmpty(_useruid))
        {
          return _useruid;
        }

        if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
        {
          _useruid = identity.Name;
          return _useruid;
        }

        throw new ArgumentException("Incorrect UserId in request context principal.");
      }
    }

    
    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    protected async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func
      <Task<TResult>> action)
      where TResult : ContractExecutionResult
    {
      var result = default(TResult);
      try
      {
        result = await action.Invoke().ConfigureAwait(false);
        if (Log.IsTraceEnabled())
          Log.LogTrace($"Executed {action.GetMethodInfo().Name} with result {JsonConvert.SerializeObject(result)}");
      }
      catch (ServiceException se)
      {
        Log.LogError(se, $"Execution failed for: {action.GetMethodInfo().Name}. ");
        throw;
      }
      catch (Exception ex)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - CustomErrorMessageOffset, ex.Message, innerException: ex);
      }
      finally
      {
        Log.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }
  }
}
