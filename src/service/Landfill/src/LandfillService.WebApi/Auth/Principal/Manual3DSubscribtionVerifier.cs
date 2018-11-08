using System.Net;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;
using VSS.VisionLink.Utilization.WebApi.ResultHandling;

namespace VSS.VisionLink.Utilization.WebApi.Configuration.Principal
{
  /// <summary>
  ///   Tests if manual 3dsubscribtion is enabled
  /// </summary>
  public class Manual3DSubscribtionVerifier : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
      var principal = actionContext.RequestContext.Principal as ITidUtilizationPrincipal;
      if (principal == null)
        return;

      if (!(principal.Subscriptions.Contains(Subscription.Threedmanual)))
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Incorrect Subscription"
            ));
    }
  }
}