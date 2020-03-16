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
    protected ICustomerProxy customerProxy;
    protected IDeviceProxy deviceProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseController(ILoggerFactory logger, IConfigurationStore configStore, 
      IProjectProxy projectProxy, ICustomerProxy customerProxy, IDeviceProxy deviceProxy)
    {
      this.configStore = configStore;
      this.projectProxy = projectProxy;
      this.customerProxy = customerProxy;
      this.deviceProxy = deviceProxy;
    }
  }
}
