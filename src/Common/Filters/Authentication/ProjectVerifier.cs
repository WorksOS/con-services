using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// Validation filter attribute for the Project identifier.
  /// </summary>
  public class ProjectVerifier : ActionFilterAttribute
  {
    private const string PROJECT_ID = "projectid";
    private const string PROJECT_UID = "projectuid";

    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectIdentifier = null;

      // Identify any query parameter called 'request'.
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];

        if (request.GetType() != typeof(string))
        {
          projectIdentifier = request.GetType()
                                     .GetProperty(PROJECT_ID, BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)?.GetValue(request);

          if (projectIdentifier == null)
          {
            projectIdentifier = request
              .GetType()
              .GetProperty(PROJECT_UID, BindingFlags.IgnoreCase | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)?.GetValue(request);

            var projectUid = Convert.ToString(projectIdentifier);
            var projectDescriptor = ((RaptorPrincipal)actionContext.HttpContext.User).GetProject(projectUid).Result;

            ((ProjectID) request).ProjectId = projectDescriptor.LegacyProjectId;
          }
        }
      }

      if (actionContext.ActionArguments.ContainsKey(PROJECT_ID))
      {
        projectIdentifier = actionContext.ActionArguments[PROJECT_ID];
      }
      else if (actionContext.ActionArguments.ContainsKey(PROJECT_UID))
      {
        projectIdentifier = actionContext.ActionArguments[PROJECT_UID];
      }
      if (projectIdentifier == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "ProjectId and ProjectUID cannot both be null."));
      }
    }
  }
}
