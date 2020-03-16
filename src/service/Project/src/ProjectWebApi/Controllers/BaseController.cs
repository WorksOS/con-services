using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
    protected readonly int customErrorMessageOffset = 2000;

  private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceExceptionHandler _serviceExceptionHandler;

    private IProductivity3dV1ProxyCoord _productivity3dV1ProxyCoord;
    private IProductivity3dV2ProxyNotification _productivity3dV2ProxyNotification;
    private IProductivity3dV2ProxyCompaction _productivity3dV2ProxyCompaction;
    private IProjectRepository _projectRepo;
    private IFileRepository _fileRepo;
    private IDataOceanClient _dataOceanClient;
    private ITPaaSApplicationAuthentication _authorization;


    /// <summary> Gets the application logging interface. </summary>
    protected ILogger<T> Logger => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary> Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders. </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory ?? (_loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());

    /// <summary> Gets the service exception handler. </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ?? (_serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary> Gets the config store. </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary> Gets or sets the Productivity3d Coord proxy. </summary>
    protected IProductivity3dV1ProxyCoord Productivity3dV1ProxyCoord => _productivity3dV1ProxyCoord ?? (_productivity3dV1ProxyCoord = HttpContext.RequestServices.GetService<IProductivity3dV1ProxyCoord>());

    /// <summary> Gets or sets the Productivity3d Notifications proxy. </summary>
    protected IProductivity3dV2ProxyNotification Productivity3dV2ProxyNotification => _productivity3dV2ProxyNotification ?? (_productivity3dV2ProxyNotification = HttpContext.RequestServices.GetService<IProductivity3dV2ProxyNotification>());

    /// <summary> Gets or sets the Productivity3d Compaction proxy. </summary>
    protected IProductivity3dV2ProxyCompaction Productivity3dV2ProxyCompaction => _productivity3dV2ProxyCompaction ?? (_productivity3dV2ProxyCompaction = HttpContext.RequestServices.GetService<IProductivity3dV2ProxyCompaction>());

    /// <summary> Gets or sets the Project Repository.  </summary>
    protected IProjectRepository ProjectRepo => _projectRepo ?? (_projectRepo = HttpContext.RequestServices.GetService<IProjectRepository>());

   /// <summary> Gets or sets the TCC File Repository. </summary>
    protected IFileRepository FileRepo => _fileRepo ?? (_fileRepo = HttpContext.RequestServices.GetService<IFileRepository>());

    /// <summary>
    /// Gets or sets the Data Ocean client agent.
    /// </summary>
    protected IDataOceanClient DataOceanClient => _dataOceanClient ?? (_dataOceanClient = HttpContext.RequestServices.GetService<IDataOceanClient>());

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
    /// Gets the customer uid from the current context
    /// </summary>
    protected string customerUid => GetCustomerUid();

    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    protected string userId => GetUserId();

    /// <summary>
    /// Gets the userEmailAddress from the current context
    /// </summary>
    protected string userEmailAddress => GetUserEmailAddress();

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(IConfigurationStore configStore)
    {
      ConfigStore = configStore;
    }

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
    /// Gets the account uid from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect account uid value.</exception>
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
    /// <returns></returns>
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
    /// Gets the users email address from the context.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Incorrect email address value.</exception>
    private string GetUserEmailAddress()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.UserEmail;
      }

      throw new ArgumentException("Incorrect user email address in request context principal.");
    }

    /// <summary>
    /// Gets the project.
    /// </summary>
    /// <param name="legacyProjectId"></param>
    protected async Task<ProjectDatabaseModel> GetProject(long shortRaptorProjectId)
    {
      var customerUid = LogCustomerDetails("GetProject by shortRaptorProjectId", shortRaptorProjectId);
      var project =
        (await ProjectRepo.GetProjectsForCustomer(this.customerUid).ConfigureAwait(false)).FirstOrDefault(
          p => p.ShortRaptorProjectId == shortRaptorProjectId);

      if (project == null)
      {
        Logger.LogWarning($"User doesn't have access to legacyProjectId: {shortRaptorProjectId}");
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.Forbidden, 1);
      }

      Logger.LogInformation($"Project shortRaptorProjectId: {shortRaptorProjectId} retrieved");
      return project;
    }

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    /// <param name="functionName">Calling function name</param>
    /// <param name="projectUid">The Project Uid</param>
    /// <returns>Returns <see cref="TIDCustomPrincipal.CustomerUid"/></returns>
    protected string LogCustomerDetails(string functionName, string projectUid = "")
    {
      Logger.LogInformation(
        $"{functionName}: UserUID={userId}, CustomerUID={customerUid}  and projectUid={projectUid}");

      return customerUid;
    }

    /// <summary>
    /// Log the Customer and Project details.
    /// </summary>
    /// <param name="functionName">Calling function name</param>
    /// <param name="legacyProjectId">The Project Id from legacy</param>
    /// <returns>Returns <see cref="TIDCustomPrincipal.CustomerUid"/></returns>
    protected string LogCustomerDetails(string functionName, long legacyProjectId = 0)
    {
      Logger.LogInformation(
        $"{functionName}: UserUID={userId}, CustomerUID={customerUid}  and legacyProjectId={legacyProjectId}");

      return customerUid;
    }
  }
}
