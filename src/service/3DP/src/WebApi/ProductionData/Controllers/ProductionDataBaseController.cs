using System;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  public class ProductionDataBaseController<T> : Controller where T : ProductionDataBaseController<T>
  {
    private ILogger<T> _log;
    private ILoggerFactory _loggerFactory;
    private IConfigurationStore _configurationStore;
    private IFileImportProxy _fileImportProxy;
    private ITRexCompactionDataProxy _trexCompactionDataProxy;

    /// <summary>
    /// Gets the application logging interface.
    /// </summary>
    protected ILogger<T> Log => _log ??= HttpContext.RequestServices.GetService<ILogger<T>>();

    /// <summary>
    /// Gets the type used to configure the logging system and create instances of ILogger from the registered ILoggerProviders.
    /// </summary>
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();

    /// <summary>
    /// Gets the filter service proxy interface.
    /// </summary>
    protected IConfigurationStore ConfigStore => _configurationStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();

    /// <summary>
    /// Gets the project settings proxy interface.
    /// </summary>
    protected IFileImportProxy FileImportProxy => _fileImportProxy ??= HttpContext.RequestServices.GetService<IFileImportProxy>();

    /// <summary>
    /// Gets the tRex CompactionData proxy interface.
    /// </summary>
    protected ITRexCompactionDataProxy TRexCompactionDataProxy => _trexCompactionDataProxy ??= HttpContext.RequestServices.GetService<ITRexCompactionDataProxy>();

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    protected IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Gets the User uid/applicationID from the context.
    /// </summary>
    protected string GetUserId()
    {
      if (User is RaptorPrincipal principal && (principal.Identity is GenericIdentity identity))
      {
        return identity.Name;
      }

      throw new ArgumentException("Incorrect UserId in request context principal.");
    }
  }
}
