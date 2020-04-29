using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Common controller base.
  /// </summary>
  public abstract class BaseController : Controller
  {
    protected readonly IConfigurationStore _configStore;
    protected ICwsAccountClient _cwsAccountClient;
    protected IProjectInternalProxy _projectProxy;
    protected IDeviceInternalProxy _deviceProxy;
    protected IDictionary<string, string> _customHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore,
      ICwsAccountClient cwsAccountClient, IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy)
    {
      _configStore = configStore;
      _cwsAccountClient = cwsAccountClient;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
    }
  }
}
