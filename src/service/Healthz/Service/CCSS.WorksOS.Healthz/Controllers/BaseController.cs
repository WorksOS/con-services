using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;

namespace CCSS.WorksOS.Healthz.Controllers
{
  public class BaseController<T> : Controller where T : BaseController<T>
  {
    private ILogger<T> _logger;
    private ILoggerFactory _loggerFactory;
    private IConfigurationStore _configStore;

    protected ILogger<T> Log => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected ILoggerFactory LoggerFactory => _loggerFactory ??= HttpContext.RequestServices.GetService<ILoggerFactory>();
    protected IConfigurationStore ConfigStore => _configStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
  }
}
