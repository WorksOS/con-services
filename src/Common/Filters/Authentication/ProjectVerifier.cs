using System;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  /// <summary>
  /// Validation filter attribute for the Project identifier and isArchived state.
  /// </summary>
  public class ProjectVerifier : ActionFilterAttribute
  {
    private const string PROJECT_ID = "projectid";
    private const string PROJECT_UID = "projectuid";

    /// <summary>
    /// Gets or sets whether the Filter will check for and reject archived Projects.
    /// </summary>
    public bool AllowArchivedState { get; set; } = true;

    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      object projectIdentifier = null;

      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];

        // Ignore any query parameter called 'request'.
        if (request.GetType() != typeof(string))
        {
          projectIdentifier = request.GetType()
            .GetProperty(PROJECT_ID, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.GetValue(request);

          // Unlikely to reach here, the above check is for legacy APIs where the projectId (not projectUid) was likely to be in play.
          if (projectIdentifier == null)
          {
            projectIdentifier = request.GetType()
              .GetProperty(PROJECT_UID, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)?.GetValue(request);
          }
        }
      }

      ProjectData projectDescriptor;

      if (actionContext.ActionArguments.ContainsKey(PROJECT_ID))
      {
        projectIdentifier = actionContext.ActionArguments[PROJECT_ID];
      }
      else if (actionContext.ActionArguments.ContainsKey(PROJECT_UID))
      {
        projectIdentifier = actionContext.ActionArguments[PROJECT_UID];
      }
      else
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "ProjectId and ProjectUID cannot both be null."));
      }

      switch (projectIdentifier)
      {
        // RaptorPrincipal will handle the failure case where project isn't found.
        case long projectId:
          projectDescriptor = ((RaptorPrincipal)actionContext.HttpContext.User).GetProject(projectId).Result;
          break;
        case string projectUid:
          projectDescriptor = ((RaptorPrincipal)actionContext.HttpContext.User).GetProject(projectUid).Result;
          break;
        case Guid projectUid:
          projectDescriptor = ((RaptorPrincipal)actionContext.HttpContext.User).GetProject(projectUid).Result;
          break;
        default:
          return;
      }

      if (projectDescriptor != null && projectDescriptor.IsArchived && !AllowArchivedState)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project has been archived and this function is not allowed."));
      }
    }
  }
}
