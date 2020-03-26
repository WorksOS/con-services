using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Filter.Common.Filters.Authentication;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Models;

namespace VSS.Productivity3D.Filter.WebAPI.Controllers
{
  /// <summary>
  /// Base controller for all Filter service controller classes.
  /// </summary>
  public abstract class BaseController : Controller
  {
    /// <summary>
    /// Gets the service's implementation of <see cref="IProjectProxy"/>.
    /// </summary>
    protected readonly IProjectProxy ProjectProxy;

    /// <summary>
    /// Gets the service's TRex/Raptor interface controller.
    /// </summary>
    protected readonly IProductivity3dV2ProxyNotification Productivity3dV2ProxyNotification;
    protected readonly IProductivity3dV2ProxyCompaction Productivity3dV2ProxyCompaction;

    /// <summary>
    /// Gets the service's configuration settings.
    /// </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary>
    /// Gets the service's Logger factory.
    /// </summary>
    protected readonly ILoggerFactory Logger;

    /// <summary>
    /// Gets or sets the service's log controller.
    /// </summary>
    protected ILogger Log;

    /// <summary>
    /// Gets or sets the service's exception hander implementation of <see cref="IServiceExceptionHandler"/>.
    /// </summary>
    protected readonly IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    protected string GetUserId => ((User as FilterPrincipal)?.Identity as GenericIdentity)?.Name;

    /// <summary>
    /// Gets the customer UID from the context
    /// </summary>
    protected string CustomerUid => (User as FilterPrincipal)?.CustomerUid;

    /// <summary>
    /// Gets the application flag from the context
    /// </summary>
    protected bool IsApplication => (User as FilterPrincipal)?.IsApplication ?? false;

    /// <summary>
    /// Gets the project data for the specified project
    /// </summary>
    protected Task<ProjectData> GetProject(string projectUid) => (User as FilterPrincipal)?.GetProject(projectUid);

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy, IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction, string eventType)
    {
      Logger = logger;
      Log = logger.CreateLogger<BaseController>();

      ConfigStore = configStore;
      ServiceExceptionHandler = serviceExceptionHandler;
      ProjectProxy = projectProxy;
      Productivity3dV2ProxyNotification = productivity3dV2ProxyNotification;
      Productivity3dV2ProxyCompaction = productivity3dV2ProxyCompaction;
    }
  }
}
