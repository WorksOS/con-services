using System;
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
  /// 
  /// </summary>
  public class ProjectUidVerifier : ActionFilterAttribute
  {
    /// <summary>
    ///   Occurs before the action method is invoked. Used for the request logging.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      const string propertyName = "projectUid";
 
      object projectUidValue = null;
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];
        projectUidValue =
          request.GetType()
            .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            .GetValue(request);
      }

      if (actionContext.ActionArguments.ContainsKey(propertyName))
      {
        projectUidValue = actionContext.ActionArguments[propertyName];
      }

      if (!(projectUidValue is string))
        return;

      var authProjectsStore = actionContext.HttpContext.RequestServices.GetRequiredService<IAuthenticatedProjectsStore>();
      if (authProjectsStore == null)
        return;
      var customerUid = ((actionContext.HttpContext.User as GenericPrincipal).Identity as GenericIdentity).AuthenticationType;
      var projectsByUid = authProjectsStore.GetProjectsByUid(customerUid);
      var found = projectsByUid.ContainsKey((string) projectUidValue);

      Guid outputGuid;

      if (!found || !Guid.TryParse((string)projectUidValue, out outputGuid))
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            String.Format("Don't have access to the selected project with the UID: {0}", projectUidValue)));
    }
  }
}