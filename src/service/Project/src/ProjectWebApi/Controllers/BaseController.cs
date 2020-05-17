using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.DataOcean.Client;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Serilog.Extensions;
using VSS.TCCFileAccess;
using VSS.WebApi.Common;
using ProjectDatabaseModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.MasterData.Project.WebAPI.Controllers
{
  /// <summary>
  /// Base for all Project v4 controllers
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
    /// <summary> base message number for ProjectService </summary>
    protected readonly int CustomErrorMessageOffset = 2000;

    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceExceptionHandler _serviceExceptionHandler;

    private IProductivity3dV1ProxyCoord _productivity3dV1ProxyCoord;
    private IProductivity3dV2ProxyNotification _productivity3dV2ProxyNotification;
    private IProductivity3dV2ProxyCompaction _productivity3dV2ProxyCompaction;
    private IProjectRepository _projectRepo;
    private IDeviceRepository _deviceRepo;
    private IFileRepository _fileRepo;
    private IDataOceanClient _dataOceanClient;
    private ITPaaSApplicationAuthentication _authorization;
    private ICwsProjectClient _cwsProjectClient;
    private ICwsDeviceClient _cwsDeviceClient;
    private ICwsDesignClient _cwsDesignClient;
    private ICwsProfileSettingsClient _cwsProfileSettingsClient;
    private IConfigurationStore _configurationStore;

    protected ILogger<T> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetService<IServiceExceptionHandler>();
    protected IConfigurationStore ConfigStore
    {
      get => _configurationStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
      set => _configurationStore = value;
    }

    protected IProductivity3dV1ProxyCoord Productivity3dV1ProxyCoord => _productivity3dV1ProxyCoord ??= HttpContext.RequestServices.GetService<IProductivity3dV1ProxyCoord>();
    protected IProductivity3dV2ProxyNotification Productivity3dV2ProxyNotification => _productivity3dV2ProxyNotification ??= HttpContext.RequestServices.GetService<IProductivity3dV2ProxyNotification>();
    protected IProductivity3dV2ProxyCompaction Productivity3dV2ProxyCompaction => _productivity3dV2ProxyCompaction ??= HttpContext.RequestServices.GetService<IProductivity3dV2ProxyCompaction>();
    protected IProjectRepository ProjectRepo => _projectRepo ??= HttpContext.RequestServices.GetService<IProjectRepository>();
    protected IDeviceRepository DeviceRepo => _deviceRepo ??= HttpContext.RequestServices.GetService<IDeviceRepository>();
    protected IFileRepository FileRepo => _fileRepo ??= HttpContext.RequestServices.GetService<IFileRepository>();
    protected IDataOceanClient DataOceanClient => _dataOceanClient ??= HttpContext.RequestServices.GetService<IDataOceanClient>();
    protected ICwsProjectClient CwsProjectClient => _cwsProjectClient ??= HttpContext.RequestServices.GetService<ICwsProjectClient>();
    protected ICwsDeviceClient CwsDeviceClient => _cwsDeviceClient ??= HttpContext.RequestServices.GetService<ICwsDeviceClient>();
    protected ICwsDesignClient CwsDesignClient => _cwsDesignClient ??= HttpContext.RequestServices.GetService<ICwsDesignClient>();
    protected ICwsProfileSettingsClient CwsProfileSettingsClient => _cwsProfileSettingsClient ??= HttpContext.RequestServices.GetService<ICwsProfileSettingsClient>();
    protected ITPaaSApplicationAuthentication Authorization => _authorization ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();

    /// <summary>
    /// Gets the custom customHeaders for the request.
    /// </summary>
    /// <remarks>
    /// Following #83476 we are deliberately passing the x-jwt-assertion header on all requests regardless of whether they're 
    /// 'internal' or not.
    /// </remarks>
    protected IHeaderDictionary customHeaders => Request.Headers.GetCustomHeaders();

    private string _customerUid;

    /// <summary>
    /// Gets the customer uid from the current context.
    /// </summary>
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

    private string _userId;

    /// <summary>
    /// Gets the UserUID/applicationID from the current context.
    /// </summary>
    protected string UserId
    {
      get
      {
        if (!string.IsNullOrEmpty(_userId))
        {
          return _userId;
        }

        if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
        {
          _userId = identity.Name;
          return _userId;
        }

        throw new ArgumentException("Incorrect UserId in request context principal.");
      }
    }

    private string _userEmailAddress;

    /// <summary>
    /// Gets the userEmailAddress from the current context
    /// </summary>
    protected string UserEmailAddress
    {
      get
      {
        if (!string.IsNullOrEmpty(_userEmailAddress))
        {
          return _userEmailAddress;
        }

        if (User is TIDCustomPrincipal principal && (principal.Identity is GenericIdentity identity))
        {
          _userEmailAddress = principal.UserEmail;
          return _userEmailAddress;
        }

        throw new ArgumentException("Incorrect user email address in request context principal."); ;
      }
    }

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController()
    { }

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
          ContractExecutionStatesEnum.InternalProcessingError - CustomErrorMessageOffset, ex.Message, innerException: ex);
      }
      finally
      {
        Logger.LogInformation($"Executed {action.GetMethodInfo().Name} with the result {result?.Code}");
      }

      return result;
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    protected async Task<ProjectDatabaseModel> GetProject(long shortRaptorProjectId)
    {
      LogCustomerDetails("GetProject by shortRaptorProjectId", shortRaptorProjectId);
      var project =
        (await ProjectRepo.GetProjectsForCustomer(CustomerUid).ConfigureAwait(false)).FirstOrDefault(
          p => p.ShortRaptorProjectId == shortRaptorProjectId);

      if (project == null)
      {
        Logger.LogWarning($"User doesn't have access to legacyProjectId: {shortRaptorProjectId}");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      Logger.LogInformation($"Project shortRaptorProjectId: {shortRaptorProjectId} ProjectUID: {project.ProjectUID} CustomerUID: {project.CustomerUID} retrieved");
      return project;
    }

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    protected void LogCustomerDetails(string functionName, string projectUid = "") =>
      Logger.LogInformation(
        $"{functionName}: UserUID={UserId}, CustomerUID={CustomerUid} and projectUid='{projectUid}'");

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    protected void LogCustomerDetails(string functionName, long legacyProjectId = 0) =>
      Logger.LogInformation(
        $"{functionName}: UserUID={UserId}, CustomerUID={CustomerUid} and legacyProjectId='{legacyProjectId}'");
  }
}
