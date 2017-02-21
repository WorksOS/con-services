using System;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.ResultHandling;


namespace VSS.Raptor.Service.Common.Filters.Authentication
{
  /// <summary>
  /// 
  /// </summary>
  public class NotLandFillProjectWithUIDVerifier : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
      const string propertyName = "projectUid";

      var principal = actionContext.RequestContext.Principal as IRaptorPrincipal;
      if (principal == null)
        return;
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

      var found = false;
      var landFillProject = false;

      foreach (var project in principal.Projects.Where(project => project.Value.projectUid == (string)projectUidValue))
      {
        found = true;
        landFillProject = project.Value.isLandFill;
        break;
      }

      Guid outputGuid;

      if (!found || !Guid.TryParse((string) projectUidValue, out outputGuid)) return;

      if (landFillProject)
        throw new ServiceException(HttpStatusCode.Unauthorized,
          new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
            "Don't have access to the selected landfill project."
            ));
    }
  }
}