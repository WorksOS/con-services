using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http.Filters;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.TPaaSAPIGateway.CustomExceptions;

namespace LegacyApiUserProvisioning.WebApi.Filters
{
    public class ApiExceptionHandler : ExceptionFilterAttribute
    {
        private readonly ILog _logger;
        public ApiExceptionHandler()
        {
            _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            _logger.IfErrorFormat($"Exception: {actionExecutedContext.Exception.Message}");
            if (actionExecutedContext.Exception is APIException apiException)
            {
                actionExecutedContext.Response =
                    actionExecutedContext.Request.CreateResponse(apiException.StatusCode, apiException.Error);
            }
            else
            {
                actionExecutedContext.Response =
                    actionExecutedContext.Request.CreateResponse(HttpStatusCode.InternalServerError, "An error has occured");
            }

        }
    }
}