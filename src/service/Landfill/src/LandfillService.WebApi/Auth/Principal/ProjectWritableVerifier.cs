using System.Net;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using VSS.VisionLink.Utilization.WebApi.Configuration.Principal.Models;
using VSS.VisionLink.Utilization.WebApi.ResultHandling;

namespace VSS.VisionLink.Utilization.WebApi.Configuration.Principal
{
  /// <summary>
  ///   Tests if the project is not archived and has Write access
  /// </summary>
  public class ProjectWritableVerifier : ActionFilterAttribute
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
      object projectIdValue = null;
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];
        projectIdValue =
          request.GetType()
            .GetProperty("projectId", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            .GetValue(request);
      }

      if (actionContext.ActionArguments.ContainsKey("projectId"))
      {
        projectIdValue = actionContext.ActionArguments["projectId"];
      }

      if (!(projectIdValue is long))
        return;

      if (!principal.Projects.ContainsKey((long) projectIdValue)) return;

      if (principal.Projects[(long) projectIdValue].isArchived)
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have write access to the selected project."
            ));
    }
  }
}