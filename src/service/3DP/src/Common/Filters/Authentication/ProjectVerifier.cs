using System;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Productivity3D.Models;

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

      // EarthWorks cutfill interface: CompactionCellController (api/v2/device/patches) doesn't contain a projectId/Uid
      if (actionContext.ActionDescriptor.DisplayName != null && actionContext.ActionDescriptor.DisplayName.ToLower().Contains("getsubgridpatches"))
        return;

      // Identify any query parameter called 'request'.
      if (actionContext.ActionArguments.ContainsKey("request"))
      {  
        //See if the [FromBody] request is a ProjectID derived model
        var requestProjectIdentifier = actionContext.ActionArguments["request"] as ProjectID;
        if (requestProjectIdentifier != null)
        {
          //Either projectId or projectUid will be in the request. We need to set the other one.
          if (requestProjectIdentifier.ProjectId.HasValue)
          {
            projectIdentifier = requestProjectIdentifier.ProjectId.Value;
            if (!requestProjectIdentifier.ProjectUid.HasValue)
              requestProjectIdentifier.ProjectUid = ((RaptorPrincipal)actionContext.HttpContext.User).GetProjectUid(requestProjectIdentifier.ProjectId.Value).Result;
          }
          else if (requestProjectIdentifier.ProjectUid.HasValue)
          {
            projectIdentifier = requestProjectIdentifier.ProjectUid.Value;
            if (!requestProjectIdentifier.ProjectId.HasValue)
              requestProjectIdentifier.ProjectId = ((RaptorPrincipal)actionContext.HttpContext.User).GetLegacyProjectId(requestProjectIdentifier.ProjectUid.Value).Result;
          }
        }          
      }

      //Check for [FromRoute] project identifiers
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
