using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  public class PostRequestVerifier : ActionFilterAttribute
  {
    /// <summary>
    /// Occurs before the action method is invoked. Used for the request logging.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      if (actionContext.ActionArguments.TryGetValue("request", out var request))
      {
        if (request == null)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Undefined requested data."));
        }
      }
    }
  }
}
