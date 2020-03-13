using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    protected IProjectProxy projectProxy;
    protected IAccountProxy accountProxy;
    protected IDeviceProxy deviceProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore, 
      IProjectProxy projectProxy, IAccountProxy accountProxy, IDeviceProxy deviceProxy)
    {
      this.configStore = configStore;
      this.projectProxy = projectProxy;
      this.accountProxy = accountProxy;
      this.deviceProxy = deviceProxy;
    }
  }
}
