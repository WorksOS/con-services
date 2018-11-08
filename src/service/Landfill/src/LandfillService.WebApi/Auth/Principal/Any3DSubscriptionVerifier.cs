using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;

namespace VSS.VisionLink.Utilization.WebApi.Configuration.Principal
{
  /// <summary>
  ///   Tests if any 3d subscribtion is available
  /// </summary>
  public class Any3DSubscriptionVerifier : ActionFilterAttribute
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

      //We don't need to check subscriptions here

      /*  if (!(principal.Subscriptions.Contains(Subscription.Threedmanual) || principal.Subscriptions.Contains(Subscription.ThreeD)))
        throw new ServiceException(HttpStatusCode.Unauthorized,
            new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
                "Incorrect Subscription"
                ));*/
    }
  }
}