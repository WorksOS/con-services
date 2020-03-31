using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using CCSS.Productivity3D.Preferences.Abstractions.Interfaces;
using CCSS.Productivity3D.Preferences.Abstractions.ResultsHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Serilog.Extensions;
using VSS.WebApi.Common;
using Microsoft.Extensions.DependencyInjection;
using VSS.MasterData.Proxies;
using VSS.VisionLink.Interfaces.Events.Preference;
using CCSS.Productivity3D.Preferences.Common.Executors;
using CSS.Productivity3D.Preferences.Common.Utilities;
using CCSS.Productivity3D.Preferences.Common.Utilities;
using CCSS.Productivity3D.Preferences.Common.Models;

namespace CCSS.Productivity3D.Preferences.WebApi.Controllers
{
  /// <summary>
  /// API endpoints for preferences. CRUD for preference keys and user preferences.
  /// </summary>
  public class PreferencesController : Controller
  {
    /// <summary> base message number for Preference service </summary>
    private readonly int customErrorMessageOffset = 2000;

    private readonly IHttpContextAccessor HttpContextAccessor;
    private ILogger<PreferencesController> _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IPreferenceRepository _prefRepo;

    /// <summary> Gets the application logging interface. </summary>
    private ILogger<PreferencesController> Logger => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger<PreferencesController>>());

    /// <summary> Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders. </summary>
    private ILoggerFactory LoggerFactory => _loggerFactory ?? (_loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());

    /// <summary> Gets the service exception handler. </summary>
    private IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ?? (_serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary> Gets or sets the Project Repository.  </summary>
    protected IPreferenceRepository PreferenceRepo => _prefRepo ?? (_prefRepo = HttpContext.RequestServices.GetService<IPreferenceRepository>());

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    private IDictionary<string, string> customHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    private string userId => GetUserId();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PreferencesController(IHttpContextAccessor httpContextAccessor)
    {
      this.HttpContextAccessor = httpContextAccessor;
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
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    /// <exception cref="ArgumentException">Incorrect user Id value.</exception>
    private string GetUserId()
    {
      if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }

    /// <summary>
    /// Gets whether the calling context is an application (as opposed to a user).
    /// </summary>
    private bool IsApplication()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.IsApplication;
      }

      throw new ArgumentException("Incorrect principal in request context principal.");
    }

    #region user preferences
    /// <summary>
    /// Gets user preferences for a user. 
    /// </summary>
    [Route("api/v1/user")]
    [HttpGet]
    public async Task<UserPreferenceV1Result> GetUserPreferenceV1([FromQuery] string keyName, [FromQuery] Guid? userUid=null)
    {
      var methodName = $"{nameof(GetUserPreferenceV1)}";

      Logger.LogInformation(methodName);

      userUid = ValidateUserUid(userUid);    

      var userPrefKey = await PreferenceRepo.GetUserPreference(userUid.Value, keyName);
      var result = AutoMapperUtility.Automapper.Map<UserPreferenceV1Result>(userPrefKey);

      Logger.LogResult(methodName, $"keyName={keyName},userUid={userUid}", result);

      return result;
    }


    /// <summary>
    /// Create User Preference. If allowUpdate is true then does an upsert.
    /// </summary>
    [Route("user")]
    [Route("targetuser")]
    [HttpPost]
    public async Task<UserPreferenceV1Result> CreateUserPreference([FromBody] UpsertUserPreferenceRequest request, [FromQuery] bool allowUpdate=false)
    {
      var methodName = $"{nameof(CreateUserPreference)}";

      Logger.LogInformation($"{methodName} request: {0}", JsonConvert.SerializeObject(request));

      request.TargetUserUID = ValidateUserUid(request.TargetUserUID);

      UserPreferenceV1Result result = null;
      var existing = await PreferenceRepo.GetUserPreference(request.TargetUserUID.Value, request.PreferenceKeyName);
      if (existing == null)
      {
        result = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<CreateUserPreferenceExecutor>(LoggerFactory, ServiceExceptionHandler,
              userId, customHeaders, PreferenceRepo)
            .ProcessAsync(AutoMapperUtility.Automapper.Map<CreateUserPreferenceEvent>(request))
        ) as UserPreferenceV1Result;
      }
      else if (allowUpdate)
      {
        result = await WithServiceExceptionTryExecuteAsync(() =>
          RequestExecutorContainerFactory
            .Build<UpdateUserPreferenceExecutor>(LoggerFactory, ServiceExceptionHandler,
              userId, customHeaders, PreferenceRepo)
            .ProcessAsync(AutoMapperUtility.Automapper.Map<UpdateUserPreferenceEvent>(request))
        ) as UserPreferenceV1Result;

      }
      else
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 13);
      }

      Logger.LogResult(methodName, JsonConvert.SerializeObject(request), result);
      return result;
    }

    /// <summary>
    /// Update User Preference
    /// </summary>
    [Route("user")]
    [Route("targetuser")]
    [HttpPut]
    public async Task<UserPreferenceV1Result> UpdateUserPreference([FromBody] UpsertUserPreferenceRequest request)
    {
      var methodName = $"{nameof(UpdateUserPreference)}";

      Logger.LogInformation($"{methodName} request: {0}", JsonConvert.SerializeObject(request));

      request.TargetUserUID = ValidateUserUid(request.TargetUserUID);

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdateUserPreferenceExecutor>(LoggerFactory, ServiceExceptionHandler,
            userId, customHeaders, PreferenceRepo)
          .ProcessAsync(AutoMapperUtility.Automapper.Map<UpdateUserPreferenceEvent>(request))
      ) as UserPreferenceV1Result;

      Logger.LogResult(methodName, JsonConvert.SerializeObject(request), result);
      return result;
    }

    /// <summary>
    /// Delete User Preference
    /// </summary>
    [Route("user")]
    [Route("targetuser")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeleteUserPreference(
      string preferencekeyname, Guid? preferencekeyuid = null, Guid? userGuid = null)
    {
      var methodName = $"{nameof(DeleteUserPreference)}";

      Logger.LogInformation($"{methodName} userGuid: {userGuid}, preferencekeyname: {preferencekeyname}, preferencekeyuid: {preferencekeyuid}");

      userGuid = ValidateUserUid(userGuid);

      var deleteEvent = new DeleteUserPreferenceEvent 
      { 
        UserUID = userGuid, 
        PreferenceKeyName = 
        preferencekeyname, PreferenceKeyUID = preferencekeyuid
      };

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<DeleteUserPreferenceExecutor>(LoggerFactory, ServiceExceptionHandler,
            userId, customHeaders, PreferenceRepo)
          .ProcessAsync(deleteEvent)
      );

      Logger.LogResult(methodName, $"userGuid: {userGuid}, preferencekeyname: {preferencekeyname}, preferencekeyuid: {preferencekeyuid}", result);
      return result;
    }
    #endregion

    #region preference keys

    /// <summary>
    /// Create User Preference Key
    /// </summary>
    [Route("user/key")]
    [HttpPost]
    public async Task<PreferenceKeyV1Result> CreatePreferenceKey([FromBody] CreatePreferenceKeyEvent preferenceEvent)
    {
      var methodName = $"{nameof(CreatePreferenceKey)}";

      Logger.LogInformation($"{methodName} preferenceEvent: {0}", JsonConvert.SerializeObject(preferenceEvent));

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreatePreferenceKeyExecutor>(LoggerFactory, ServiceExceptionHandler,
            userId, customHeaders, PreferenceRepo)
          .ProcessAsync(preferenceEvent)
      ) as PreferenceKeyV1Result;

      Logger.LogResult(methodName, JsonConvert.SerializeObject(preferenceEvent), result);
      return result;      
    }


    /// <summary>
    /// Update User Preference Key
    /// </summary>
    [Route("user/key")]
    [HttpPut]
    public async Task<PreferenceKeyV1Result> UpdatePreferenceKey([FromBody] UpdatePreferenceKeyEvent preferenceEvent)
    {
      var methodName = $"{nameof(UpdatePreferenceKey)}";

      Logger.LogInformation($"{methodName} preferenceEvent: {0}", JsonConvert.SerializeObject(preferenceEvent));

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<UpdatePreferenceKeyExecutor>(LoggerFactory, ServiceExceptionHandler,
            userId, customHeaders, PreferenceRepo)
          .ProcessAsync(preferenceEvent)
      ) as PreferenceKeyV1Result;

      Logger.LogResult(methodName, JsonConvert.SerializeObject(preferenceEvent), result);
      return result;
    }

    /// <summary>
    /// Delete User Preference Key
    /// </summary>
    [Route("user/key")]
    [HttpDelete]
    public async Task<ContractExecutionResult> DeletePreferenceKey([FromBody] DeletePreferenceKeyEvent preferenceEvent)
    {
      var methodName = $"{nameof(DeletePreferenceKey)}";

      Logger.LogInformation($"{methodName} preferenceEvent: {0}", JsonConvert.SerializeObject(preferenceEvent));

      var result = await WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainerFactory
          .Build<CreatePreferenceKeyExecutor>(LoggerFactory, ServiceExceptionHandler,
            userId, customHeaders, PreferenceRepo)
          .ProcessAsync(preferenceEvent)
      );

      Logger.LogResult(methodName, JsonConvert.SerializeObject(preferenceEvent), result);
      return result;
    }

    #endregion

    #region privates
    /// <summary>
    /// Validates the user UID. 
    /// If the UID is specified and the context is a user then check the UID matches that in the context otherwise for an application any user is ok.
    /// If the UID is not specified and the context is a user then get it from the context else for an application it's an error. 
    /// </summary>
    private Guid ValidateUserUid(Guid? userUid)
    {
      string userId = GetUserId();
      var isUserContext = !IsApplication();

      if (userUid.HasValue)
      {
        if (isUserContext)
        {
          //Don't let a user access another user's preferences
          if (string.Compare(userId, userUid.Value.ToString(), StringComparison.InvariantCultureIgnoreCase) != 0)
          {
            ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 8);
          }
        }
      }
      else if (isUserContext)
      {
        userUid = Guid.Parse(userId);
      }
      else
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 9);
      }
      return userUid.Value;
    }
    #endregion
  }
}
