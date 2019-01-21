using System;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers.Filters
{
  /// <summary>
  /// Validates the tile request's width and height.
  /// </summary>
  public class ValidateWidthAndHeightAttribute : ActionFilterAttribute
  {
    /// <summary>
    /// Executes before the action method is invoked.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      try
      {
        int width = Convert.ToInt32(context.HttpContext.Request.Query["width"].ToString());
        int height = Convert.ToInt32(context.HttpContext.Request.Query["height"].ToString());

        if (width != WebMercatorProjection.TILE_SIZE || height != WebMercatorProjection.TILE_SIZE)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "Service supports only tile width and height of " + WebMercatorProjection.TILE_SIZE + " pixels"));
        }
      }
      catch (Exception)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Badly formatted width or height parameters."));
      }
      
      base.OnActionExecuting(context);
    }
  }
}
