using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

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
    protected ITPaaSApplicationAuthentication _authorization;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore,
      ICwsAccountClient cwsAccountClient, IProjectInternalProxy projectProxy, IDeviceInternalProxy deviceProxy,
      ITPaaSApplicationAuthentication authorization)
    {
      _configStore = configStore;
      _cwsAccountClient = cwsAccountClient;
      _projectProxy = projectProxy;
      _deviceProxy = deviceProxy;
      _authorization = authorization;
    }
  }
}
