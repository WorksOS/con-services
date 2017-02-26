using System.Net;
using System.Reflection;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using Microsoft.Extensions.DependencyInjection;

namespace VSS.Raptor.Service.Common.Filters.Authentication
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
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
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

      var authProjectsStore = actionContext.HttpContext.RequestServices.GetRequiredService<IAuthenticatedProjectsStore>();
      if (authProjectsStore == null)
        return;
      var customerUid = ((actionContext.HttpContext.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      var projectsById = authProjectsStore.GetProjectsById(customerUid);

      if (!projectsById.ContainsKey((long) projectIdValue)) return;

      if (projectsById[(long) projectIdValue].isArchived)
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have write access to the selected project."
            ));
    }
  }
}