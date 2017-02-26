using System.Net;
using System.Reflection;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.Common.Filters.Authentication
{
  /// <summary>
  ///   Tests if the project is not archived and has Write access
  /// </summary>
  public class ProjectWritableWithUIDVerifier : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectUidValue = null;
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];
        projectUidValue =
          request.GetType()
            .GetProperty("projectUid", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            .GetValue(request);
      }

      if (actionContext.ActionArguments.ContainsKey("projectUid"))
      {
        projectUidValue = actionContext.ActionArguments["projectUid"];
      }

      if (!(projectUidValue is long))
        return;

      var authProjectsStore = actionContext.HttpContext.RequestServices.GetRequiredService<IAuthenticatedProjectsStore>();
      if (authProjectsStore == null)
        return;
      var customerUid = ((actionContext.HttpContext.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      var projectsById = authProjectsStore.GetProjectsById(customerUid);
      if (!projectsById.ContainsKey((long) projectUidValue)) return;

      if (projectsById[(long) projectUidValue].isArchived)
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have write access to the selected project."
            ));
    }
  }
}