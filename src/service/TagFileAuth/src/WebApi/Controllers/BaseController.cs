using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Common controller base.
  /// </summary>
  public abstract class BaseController : Controller
  {
    protected readonly IConfigurationStore configStore;
    protected IAccountClient accountClient;
    protected IProjectProxy projectProxy;
    protected IDeviceProxy deviceProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore,
      IAccountClient accountClient, IProjectProxy projectProxy, IDeviceProxy deviceProxy)
    {
      this.configStore = configStore;
      this.accountClient = accountClient;
      this.projectProxy = projectProxy;
      this.deviceProxy = deviceProxy;
    }
  }
}
