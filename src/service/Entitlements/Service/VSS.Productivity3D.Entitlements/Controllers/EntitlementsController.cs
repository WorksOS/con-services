using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions;
using VSS.Productivity3D.Entitlements.Abstractions.Interfaces;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Response;
using VSS.Productivity3D.Entitlements.Common.Authentication;
using VSS.Productivity3D.Entitlements.Common.Executors;
using VSS.Productivity3D.Entitlements.Common.Models;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Entitlements.WebApi.Controllers
{
  /// <summary>
  /// API end points for entitlement checks.
  /// </summary>
  public class EntitlementsController : Controller
  {
    /// <summary> base message number for Preference service </summary>
    private readonly int customErrorMessageOffset = 6000;

    private ILogger<EntitlementsController> _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IConfigurationStore _configurationStore;
    private ITPaaSApplicationAuthentication _authn;
    private IEmsClient _emsClient;
    private readonly bool _enableEntitlementCheck;

    /// <summary> Gets the application logging interface. </summary>
    private ILogger<EntitlementsController> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<EntitlementsController>>();

    /// <summary> Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders. </summary>
    private ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();

    /// <summary> Gets the service exception handler. </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetService<IServiceExceptionHandler>();

    /// <summary> Gets the configuration store </summary>
    private IConfigurationStore ConfigStore => _configurationStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();

    /// <summary>
    /// The TPaaS authentication for generating an application bearer token
    /// </summary>
    private ITPaaSApplicationAuthentication Authn => _authn ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();

    /// <summary>
    /// The EMS (entitlement management system) client
    /// </summary>
    private IEmsClient EmsClient => _emsClient ??= HttpContext.RequestServices.GetService<IEmsClient>();

    private static List<string> AcceptedEmails;

    /// <summary> Constructor </summary>
    public EntitlementsController()
    {
      if (AcceptedEmails == null)
      {
        Logger.LogInformation($"Loading testing entitlement accepted emails");
        LoadTestingEmails();
      }

      _enableEntitlementCheck = ConfigStore.GetValueBool(ConfigConstants.ENABLE_ENTITLEMENTS_CONFIG_KEY, false);
    }

    /// <summary>
    /// Attempt to request an entitlement for the request feature and user
    /// </summary>
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(EntitlementResponseModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpPost("api/v1/entitlement")]
    public async Task<IActionResult> GetEntitlement([FromBody] EntitlementRequestModel request)
    {
      Logger.LogInformation($"Entitlement Request: {JsonConvert.SerializeObject(request)}");

      if (request == null)
        return BadRequest();

      var model = new GetEntitlementsRequest 
        {User = User as EntitlementUserClaim, Request = request, AcceptedEmails = AcceptedEmails, EnableEntitlementCheck = _enableEntitlementCheck};
      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<GetEntitlementsExecutor>(LoggerFactory, ServiceExceptionHandler, Authn, EmsClient)
          .ProcessAsync(model)
      );

      if (result.Code == ContractExecutionStatesEnum.ValidationError)
        return BadRequest(result.Message);

      if (result.Code == ContractExecutionStatesEnum.AuthError)
        return Forbid();

      if (result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully)
      {
        var response = new EntitlementResponseModel
        {
          Feature = request.Feature,
          Sku = request.Sku,
          IsEntitled = true,
          OrganizationIdentifier = request.OrganizationIdentifier,
          UserEmail = request.UserEmail,
          UserUid = request.UserUid
        };

        Logger.LogInformation($"Generated Entitlements Response: {JsonConvert.SerializeObject(response)}");

        return Json(response);
      }

      //Shouldn't get here ...
      return BadRequest();
    }

    /// <summary>
    /// With the service exception try execute.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action.</param>
    private async Task<TResult> WithServiceExceptionTryExecuteAsync<TResult>(Func
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
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
          ContractExecutionStatesEnum.InternalProcessingError - customErrorMessageOffset, ex.Message, innerException: ex);
      }
      finally
      {
        Logger.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// While we wait for a real entitlement server, we will use a list of hardcoded emails
    /// These will be loaded from env, once.
    /// </summary>
    private void LoadTestingEmails()
    {
      var data = _configurationStore.GetValueString(ConfigConstants.ENTITLEMENTS_ACCEPT_EMAIL_KEY, string.Empty);
      if (string.IsNullOrEmpty(data))
      {
        Logger.LogWarning($"No Allowed Emails for Entitlements loaded");
        AcceptedEmails = new List<string>();
      }

        
      AcceptedEmails = data.Split(';').Select(e => e.ToLower().Trim()).ToList();
      foreach (var email in AcceptedEmails)
      {
        Logger.LogInformation($"Accepting entitlements from `{email}`");
      }
    }
  }
}
