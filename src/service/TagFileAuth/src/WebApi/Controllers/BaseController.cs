using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.TRex.Gateway.Common.Abstractions;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Common controller base.
  /// </summary>
  public abstract class BaseController<T> : Controller where T : BaseController<T>
  {
    private ILogger<T> _logger;
    private IConfigurationStore _configStore;
    private ITPaaSApplicationAuthentication _authorization;
    private IProjectInternalProxy _projectProxy;
    private IDeviceInternalProxy _deviceProxy;
    private ITRexCompactionDataProxy _tRexCompactionDataProxy;

    protected ILogger<T> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected IConfigurationStore ConfigStore => _configStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
    protected IHeaderDictionary RequestCustomHeaders => Request.Headers.GetCustomHeaders();
    protected ITPaaSApplicationAuthentication Authorization => _authorization ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();
    protected IProjectInternalProxy ProjectProxy => _projectProxy ??= HttpContext.RequestServices.GetService<IProjectInternalProxy>();
    protected IDeviceInternalProxy DeviceProxy => _deviceProxy ??= HttpContext.RequestServices.GetService<IDeviceInternalProxy>();
    protected ITRexCompactionDataProxy TRexCompactionDataProxy => _tRexCompactionDataProxy ??= HttpContext.RequestServices.GetService<ITRexCompactionDataProxy>();

    protected BaseController()
    { }
  }
}
