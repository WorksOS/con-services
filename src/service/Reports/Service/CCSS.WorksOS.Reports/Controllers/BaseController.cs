using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;

namespace CCSS.WorksOS.Reports.Controllers
{
  public class BaseController<T> : Controller where T : BaseController<T>
  {
    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IConfigurationStore _configStore;
    private IServiceExceptionHandler _serviceExceptionHandler;

    protected ILogger<T> Log => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();
    protected IConfigurationStore ConfigStore => _configStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
    protected IServiceExceptionHandler ServiceExceptionHandler => _serviceExceptionHandler ??= HttpContext.RequestServices.GetService<IServiceExceptionHandler>();
    protected IHeaderDictionary customHeaders => Request.Headers.GetCustomHeaders();
  }
}
