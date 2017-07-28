using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  public class PostRequestVerifier : ActionFilterAttribute
  {
    /// <summary>
    ///   Occurs before the action method is invoked. Used for the request logging.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(ActionExecutingContext actionContext)
    {
      if (actionContext.ActionArguments.ContainsKey("request"))
      {
        var request = actionContext.ActionArguments["request"];
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
