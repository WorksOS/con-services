using System;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using Common.Filters.Authentication.Models;
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
    public class NotLandFillProjectWithUIDVerifier : ActionFilterAttribute
    {  
      /// <summary>
      /// Occurs before the action method is invoked.
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

        var projectDescr = (actionContext.HttpContext.User as RaptorPrincipal).GetProject((string)projectUidValue);
        if (projectDescr.isLandFill)
        {
          throw new ServiceException(HttpStatusCode.Unauthorized,
            new ContractExecutionResult(ContractExecutionStatesEnum.AuthError,
              "Don't have access to the selected landfill project."
            ));
        }
      }
    }
}