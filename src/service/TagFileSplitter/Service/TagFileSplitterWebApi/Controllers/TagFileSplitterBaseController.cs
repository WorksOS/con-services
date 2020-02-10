using System;
using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Interfaces;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace CCSS.TagFileSplitter.WebAPI.Controllers
{
  /// <summary>
  /// Base for all TagFileSplitter controller
  /// </summary>
  public abstract class TagFileSplitterBaseController<T> : Controller where T : TagFileSplitterBaseController<T>
  {
    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IServiceExceptionHandler _serviceExceptionHandler;
    private IServiceResolution _serviceResolution;
    private IGenericHttpProxy _genericHttpProxy;
    private ITPaaSApplicationAuthentication _authorization;


    /// <summary> Gets the application logging interface. </summary>
    protected ILogger<T> Logger => _logger ?? (_logger = HttpContext.RequestServices.GetService<ILogger<T>>());

    /// <summary> Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders. </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory ?? (_loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>());
    /// <summary> Gets the config store. </summary>
    protected readonly IConfigurationStore ConfigStore;

    /// <summary> Gets the service exception handler. </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ?? (_serviceExceptionHandler = HttpContext.RequestServices.GetService<IServiceExceptionHandler>());

    /// <summary> Gets or sets the Productivity3d generic v2 proxy. </summary>
    protected IServiceResolution ServiceResolution => _serviceResolution ?? (_serviceResolution = HttpContext.RequestServices.GetService<IServiceResolution>());

    /// <summary> Gets or sets the generic proxy. </summary>
    protected IGenericHttpProxy GenericHttpProxy => _genericHttpProxy ?? (_genericHttpProxy = HttpContext.RequestServices.GetService<IGenericHttpProxy>());

    /// <summary> Gets or sets the TPaaS application authentication helper. </summary>
    protected ITPaaSApplicationAuthentication Authorization => _authorization ?? (_authorization = HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>());

    /// <summary>
    /// Gets the customHeaders for the request.
    /// </summary>
    protected IDictionary<string, string> CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the customer uid from the current context
    /// </summary>
    protected string CustomerUid => GetCustomerUid();

    /// <summary>
    /// Gets the user id from the current context
    /// </summary>
    protected string UserId => GetUserId();

    /// <summary>
    /// Gets the userEmailAddress from the current context
    /// </summary>
    protected string UserEmailAddress => GetUserEmailAddress();

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected TagFileSplitterBaseController(IConfigurationStore configStore)
    {
      ConfigStore = configStore;
    }

    /// <summary>
    /// Gets the customer uid from the context.
    /// </summary>
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

    /// <summary>
    /// Gets the users email address from the context. Or for an application token, gets the application name.
    /// </summary>
    private string GetUserEmailAddress()
    {
      if (User is TIDCustomPrincipal principal)
      {
        return principal.UserEmail;
      }

      throw new ArgumentException("Incorrect user email address in request context principal.");
    }
  }
}
