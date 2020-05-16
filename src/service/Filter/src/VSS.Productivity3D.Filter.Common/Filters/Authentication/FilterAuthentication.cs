using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.WebApi.Common;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// Authentication middleware.
  /// </summary>
  public class FilterAuthentication : TIDAuthentication
  {
    private readonly IProjectProxy _projectProxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterAuthentication"/> class.
    /// </summary>
    public FilterAuthentication(RequestDelegate next,
      ICwsAccountClient cwsAccountClient,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy) : base(next, cwsAccountClient, store, logger, serviceExceptionHandler)
    {
      _projectProxy = projectProxy;
    }

    /// <summary>
    /// Create 3dpm filter principal
    /// </summary>
    public override TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName,
      string userEmail, bool isApplicationContext, IHeaderDictionary contextHeaders, string tpaasApplicationName = "")
    {
      return new FilterPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, _projectProxy, contextHeaders);
    }
  }
}
