using System.Collections.Generic;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Tile.Service.Common.Authentication
{
  /// <summary>
  /// Tile service Authentication middleware
  /// </summary>
  public class TileAuthentication : TIDAuthentication
  {
    private readonly IProjectListProxy projectListProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="TileAuthentication"/> class.
    /// </summary>
    public TileAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    {
      this.projectListProxy = projectListProxy;
    }

    /// <summary>
    /// Create 3dpm principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, 
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName = "")
    {
      //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
      // note that userUid may actually be the ApplicationId if isApplicationContext
      return new TilePrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, projectListProxy, contextHeaders, tpaasApplicationName);
    }

  }
}
