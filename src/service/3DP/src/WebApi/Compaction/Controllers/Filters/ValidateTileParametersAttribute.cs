using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers.Filters
{
  /// <summary>
  /// Validates the WMS parameters for tile requests.
  /// </summary>
  public class ValidateTileParametersAttribute : ActionFilterAttribute
  {
    /// <summary>
    /// Called before the action method is invoked.
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
      var parameters = context.HttpContext.Request.Query;

      if (string.IsNullOrEmpty(parameters["bbox"]))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service requires bounding box dimensioms to be provided."));
      }

      var service = parameters["service"].ToString();
      var version = parameters["version"].ToString();
      var request = parameters["request"].ToString();
      var format = parameters["format"].ToString();
      var transparent = parameters["transparent"].ToString();
      var layers = parameters["layers"].ToString();
      var crs = parameters["crs"].ToString();


      bool invalid = !string.IsNullOrEmpty(service) && service.ToUpper() != "WMS" ||
                     !string.IsNullOrEmpty(version) && version.ToUpper() != "1.3.0" ||
                     !string.IsNullOrEmpty(request) && request.ToUpper() != "GETMAP" ||
                     !string.IsNullOrEmpty(format) && format.ToUpper() != "IMAGE/PNG" ||
                     !string.IsNullOrEmpty(transparent) && transparent.ToUpper() != "TRUE" ||
                     !string.IsNullOrEmpty(layers) && layers.ToUpper() != "LAYERS" ||
                     !string.IsNullOrEmpty(crs) && crs.ToUpper() != "EPSG:4326" ||
                     !string.IsNullOrEmpty(parameters["styles"]);

      if (invalid)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Service supports only the following: SERVICE=WMS, VERSION=1.3.0, REQUEST=GetMap, FORMAT=image/png, TRANSPARENT=true, LAYERS=Layers, CRS=EPSG:4326, STYLES=(no styles supported)"));
      }

      base.OnActionExecuting(context);
    }
  }
}