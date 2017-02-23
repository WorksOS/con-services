using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;
using Microsoft.Extensions.DependencyInjection;

namespace VSS.Raptor.Service.Common.Filters.Authentication
{
  /// <summary>
  /// 
  /// </summary>
  public class NotLandFillProjectVerifier : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public void OnActionExecuting(ActionExecutingContext actionContext)
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

      var authProjectsStore = actionContext.HttpContext.RequestServices.GetService<IAuthenticatedProjectsStore>();
      if (authProjectsStore == null)
        return;

      if (!authProjectsStore.ProjectsById.ContainsKey((long) projectIdValue)) return;

      if (authProjectsStore.ProjectsById[(long) projectIdValue].isLandFill)
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have access to the selected landfill project."
            ));
    }
  }
}