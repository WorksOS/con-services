using LandfillService.Common;
using LandfillService.Common.Contracts;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using LandfillService.WebApi.Auth;

namespace LandfillService.WebApi
{

  /// <summary>
  /// Defines logging methods executed on request and after request is executed
  /// </summary>
  public class LogActionFilterAttribute : ActionFilterAttribute
  {

    /// <summary>
    /// Occurs before the action method is invoked. Used for the request logging.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public override void OnActionExecuting(HttpActionContext actionContext)
    {
      LoggerSvc.LogAction(actionContext);
    }

    /// <summary>
    /// Occurs after the action method is invoked. Used for logging the result of execution.
    /// </summary>
    /// <param name="actionExecutedContext">The action executed context.</param>
    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
      LoggerSvc.LogExecutedAction(actionExecutedContext);
    }
  }

  /// <summary>
  /// Attribute class handling serialization errors.
  /// </summary>
  public class HandleSerializationErrorAttribute : ExceptionFilterAttribute
  {
    /// <summary>
    /// Custom handler called when [exception].
    /// </summary>
    /// <param name="context">The context of executed context.</param>
    /// <exception cref="ServiceException">Service-wide exception.</exception>
    /// <exception cref="ContractExecutionResult">Argument of the exception.</exception>
    public override void OnException(HttpActionExecutedContext context)
    {
      if (context.Exception is JsonSerializationException)
      {
        LoggerSvc.LogAction(context.ActionContext);
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.IncorrectRequestedData,
            context.Exception.Message));
      }
    }
  }

  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.EnableCors(new EnableCorsAttribute("*", "*", "GET, POST, OPTIONS"));

      // config.Filters.Add(new TIDAuthFilter());
      config.Filters.Add(new LogActionFilterAttribute());
      config.Filters.Add(new HandleSerializationErrorAttribute());

      config.Filters.Add(new TIDAuthFilter());

      // Web API configuration and services: allow JSON only
      config.Formatters.Clear();
      config.Formatters.Insert(0, new JsonMediaTypeFormatter());
      config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

      // Web API routes
      config.MapHttpAttributeRoutes();

      // Log exceptions
      config.Services.Add(typeof(IExceptionLogger),
        new TraceSourceExceptionLogger(new
          TraceSource(Assembly.GetExecutingAssembly().FullName, SourceLevels.All)));

      config.Services.Replace(typeof(IExceptionHandler), new ApiExceptionHandler());

    }
  }
}
