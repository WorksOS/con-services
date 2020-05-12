using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;
using Microsoft.Extensions.DependencyInjection;

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
    private ICwsAccountClient _cwsAccountClient;
    private IProjectInternalProxy _projectProxy;
    private IDeviceInternalProxy _deviceProxy;

    protected ILogger<T> Logger => _logger ??= HttpContext.RequestServices.GetService<ILogger<T>>();
    protected IConfigurationStore ConfigStore => _configStore ??= HttpContext.RequestServices.GetService<IConfigurationStore>();
    protected IDictionary<string, string> RequestCustomHeaders => Request.Headers.GetCustomHeaders();
    protected ITPaaSApplicationAuthentication Authorization => _authorization ??= HttpContext.RequestServices.GetService<ITPaaSApplicationAuthentication>();
    protected ICwsAccountClient CwsAccountClient => _cwsAccountClient ??= HttpContext.RequestServices.GetService<ICwsAccountClient>();
    protected IProjectInternalProxy ProjectProxy => _projectProxy ??= HttpContext.RequestServices.GetService<IProjectInternalProxy>();
    protected IDeviceInternalProxy DeviceProxy => _deviceProxy ??= HttpContext.RequestServices.GetService<IDeviceInternalProxy>();

    

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController()
    { }
  }
}
