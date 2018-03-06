using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers.Filters
{
  /// <summary>
  /// Validates the bounding box parameter has been provided.
  /// </summary>
  public class ValidateBoundingBoxAttribute : ActionFilterAttribute
  {
    /// <summary>
    /// Executes before the action method is executed.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      if (string.IsNullOrEmpty(context.HttpContext.Request.Query["bbox"]))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service requires bounding box dimensioms to be provided."));
      }

      base.OnActionExecuting(context);
    }
  }
}