using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Productivity3D.Common.Filters.Authentication.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// 
  /// </summary>
  public class ProjectIdVerifier : ActionFilterAttribute
  {
    /// <summary>
    ///   Occurs before the action method is invoked. Used for the request logging.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectIdValue = null;
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];
        projectIdValue =
          request?.GetType()
            ?.GetProperty("projectId", BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
            ?.GetValue(request);
      }

      if (actionContext.ActionArguments.ContainsKey("projectId"))
      {
        projectIdValue = actionContext.ActionArguments["projectId"];
      }

      if (!(projectIdValue is long))
        return;

      //Check done in RaptorPrincipal
      var projectDescr = (actionContext.HttpContext.User as RaptorPrincipal).GetProject((long)projectIdValue);
    }
  }
}