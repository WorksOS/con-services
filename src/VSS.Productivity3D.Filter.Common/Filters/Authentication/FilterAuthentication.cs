using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Security.Principal;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// Authentication middleware.
  /// </summary>
  public class FilterAuthentication : TIDAuthentication
  {
    private readonly IProjectListProxy ProjectListProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterAuthentication"/> class.
    /// </summary>
    public FilterAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectListProxy projectListProxy) : base(next, customerProxy, store, logger, serviceExceptionHandler)
    { 
      ProjectListProxy = projectListProxy;
    }

    /// <summary>
    /// Create 3dpm filter principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName,
      string userEmail, bool isApplicationContext, IDictionary<string, string> contextHeaders)
    {
      return new FilterPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, ProjectListProxy, contextHeaders);
    }

  }
}