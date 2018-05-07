using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.MasterData.Project.WebAPI.Middleware
{
  /// <summary>
  /// Project authentication middleware
  /// </summary>
  public class ProjectAuthentication : TIDAuthentication
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectAuthentication"/> class.
    /// </summary>
    public ProjectAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
    }

    /// <summary>
    /// project specific logic for requiring customerUid
    /// </summary>
    public override bool RequireCustomerUid(HttpContext context)
    {
      return !(context.Request.Path.Value.Contains("api/v3/project") && context.Request.Method != "GET");
    } 
  }
}
