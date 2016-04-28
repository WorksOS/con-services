using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;


namespace LandfillService.Common
{
  public class SkipPropResolver : DefaultContractResolver
  {
    private readonly string propName;

    public SkipPropResolver(string propName)
    {
      this.propName = propName;
    }

    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
      IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

      // only serializer properties that start with the specified character
      properties = properties.Where(p => !p.PropertyName.Equals(propName)).ToList();
      return properties;
    }
  }

  /// <summary>
  /// Main class for logging of all service activities
  /// </summary>
  public static class LoggerSvc
  {
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static readonly int MAX_RESULT_SIZE = 1500;

    private static readonly IContractResolver propResolver = new SkipPropResolver("password");

    public static void LogMessageHandler(string correlationId, string requestInfo, byte[] message)
    {

      log.InfoFormat("R {0}:{1} Data: {2}", correlationId, requestInfo, Encoding.UTF8.GetString(message));
    }


    public static void LogMessage(string component, string method, string url, string message)
    {
      log.InfoFormat("M {0}:{1} Request: {2} Message: {3}", component, method, url, message);
    }

    public static void LogRequest<T>(string component, string method, string url, T parameters)
    {
      try
      {
        string json = JsonConvert.SerializeObject(parameters, new JsonSerializerSettings { ContractResolver = propResolver });
        if (json.Length > MAX_RESULT_SIZE)
          json = string.Format("(TRUNCATED){0}", json.Substring(0, MAX_RESULT_SIZE));

        log.Info(String.Format("E {0}:{1} Request: {2}", component, method, url));
        log.Info(String.Format("B {0}:{1} ({2})", component, method, json));

      }
      catch (Exception e)
      {
        log.Warn("Unexpected error logging method call", e);
        log.InfoFormat("B {0}:{1}(???????????????????)", component, method);
      }
    }

    public static void LogResponse(string component, string method, string url, HttpResponseMessage response)
    {
      try
      {
        string responseStr = response.Content.ReadAsStringAsync().Result;
        if (responseStr.Length > MAX_RESULT_SIZE)
          responseStr = string.Format("(TRUNCATED){0}", responseStr.Substring(0, MAX_RESULT_SIZE));

        log.Info(String.Format("E {0}:{1} Response: {2} | {3}", component, method, response.StatusCode, responseStr));
      }
      catch (Exception e)
      {
        log.Warn("Unexpected error logging method call", e);
        log.InfoFormat("B {0}:{1}(???????????????????)", component, method);
      }
    }
    /// <summary>
    /// Logs the action.
    /// </summary>
    /// <param name="actionContext">The action context.</param>
    public static void LogAction(HttpActionContext actionContext)
    {
      string controllerName = actionContext.ControllerContext.ControllerDescriptor.ControllerName;
      string actionName = actionContext.ActionDescriptor.ActionName;

      try
      {
        StringBuilder builder = new StringBuilder();
        builder.Append(String.Format(
                "B {0}:{1}(", controllerName, actionName));
        bool firstArg = true;
        if (actionContext.ActionArguments != null)
        {
          foreach (object arg in actionContext.ActionArguments)
          {
            if (firstArg)
              firstArg = false;
            else
              builder.Append(",");

            string json = JsonConvert.SerializeObject(arg, new JsonSerializerSettings { ContractResolver = propResolver });

            if (json.Length > MAX_RESULT_SIZE)
            {
              json = string.Format("(TRUNCATED){0}", json.Substring(0, MAX_RESULT_SIZE));
            }
            builder.Append(json);
          }
        }

        builder.Append(")");
        log.Info(String.Format("E {0}:{1} Request:{2}", controllerName, actionName, actionContext.Request.RequestUri));
        log.Info(builder.ToString());
      }
      catch (Exception e)
      {
        log.Warn("Unexpected error logging method call", e);
        log.InfoFormat("B {0}:{1}(???????????????????)", controllerName, actionName);
      }
    }

    /// <summary>
    /// Logs the executed action.
    /// </summary>
    /// <param name="actionExecutedContext">The action executed context.</param>
    public static async void LogExecutedAction(HttpActionExecutedContext actionExecutedContext)
    {
      string controllerName =
              actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
      string actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
      HttpResponseMessage response = actionExecutedContext.Response;
      string status = String.Empty;

      try
      {
        if (response == null)
        {
          status = actionExecutedContext.Exception.Message;
          log.InfoFormat("E {0}:{1} returned Exception:{2}", controllerName, actionName, status);
          var exception = actionExecutedContext.Exception as ServiceException;
          if (exception != null)
          {
            log.InfoFormat("E {0}:{1} returned Status:{2}", controllerName, actionName,
                    exception.GetContent);
          }
          return;
        }

        status = response.StatusCode.ToString();

        // given there is alot of large types and bitmaps returned by Raptor we will only log status code here
        //  log.IfInfoFormat("E {0}:{1} returned Status:{2}", controllerName, actionName, status);



        string res = string.Empty;
        if (response != null && response.Content != null)
        {
          res = await response.Content.ReadAsStringAsync();
        }

        if (res.Length > MAX_RESULT_SIZE)
        {
          log.InfoFormat("E {0}:{1} returned Status: {3} (TRUNCATED) {2}", controllerName, actionName,
                  res.Substring(0, MAX_RESULT_SIZE), status);
        }
        else
        {
          log.InfoFormat("E {0}:{1} returned Status: {3} {2}", controllerName, actionName, res, status);
        }
      }
      catch (Exception ee)
      {
        log.Warn("Unexpected error logging method call return value", ee);
        log.InfoFormat("E {0}:{1} returned (????)", controllerName, actionName);
      }
    }

    /// <summary>
    /// Logs the executed action.
    /// </summary>
    /// <param name="actionExecutedContext">The action executed context.</param>
    public static async void LogExecuteExceptiondAction(ExceptionContext actionExecutedContext)
    {
      string controllerName = String.Empty;
      string actionName = String.Empty;
      if (actionExecutedContext.ActionContext != null)
      {
        controllerName =
                actionExecutedContext.ActionContext.ControllerContext.ControllerDescriptor.ControllerName;
        actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
      }
      try
      {
        string request = await actionExecutedContext.Request.Content.ReadAsStringAsync();
        log.WarnFormat("E {0}:{1} Request caused exception: {2}", controllerName, actionName, request);

        HttpResponseMessage response = actionExecutedContext.Response;

        string status = actionExecutedContext.Exception.Message;
        log.WarnFormat("E {0}:{1} returned Exception: {2}", controllerName, actionName, status);

        status = actionExecutedContext.Exception.Source;
        log.WarnFormat("E {0}:{1} returned Exception: {2}", controllerName, actionName, status);

        status = actionExecutedContext.Exception.StackTrace;
        // given there is alot of large types and bitmaps returned by Raptor we will only log status code here
        log.WarnFormat("E {0}:{1} returned Exception: {2}", controllerName, actionName, status);



      }
      catch (Exception ee)
      {
        log.Warn("Unexpected error logging method call return value", ee);
        log.InfoFormat("E {0}:{1} returned (????)", controllerName, actionName);
      }
    }

  }

  // <summary>
  /// This is an expected exception and should be ignored by unit test failure methods.
  /// </summary>
  /// 
  public class ServiceException : HttpResponseException
  {
    private string content;

    /// <summary>
    /// ServiceException class constructor.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="result"></param>
    public ServiceException(HttpStatusCode code, ContractExecutionResult result)
      : base(new HttpResponseMessage(code))
    {
      content = JsonConvert.SerializeObject(result);
      base.Response.Content = new StringContent(content);
    }

    public string GetContent { get { return content; } }
  }

  /// <summary>
  /// Logs all exceptions.
  /// </summary>
  public class TraceSourceExceptionLogger : ExceptionLogger
  {
    private readonly TraceSource _traceSource;

    public TraceSourceExceptionLogger(TraceSource traceSource)
    {
      this._traceSource = traceSource;
    }

    public override void Log(ExceptionLoggerContext context)
    {
      LoggerSvc.LogExecuteExceptiondAction(context.ExceptionContext);
    }
  }


  public class ApiExceptionHandler : ExceptionHandler
  {
    public override void Handle(ExceptionHandlerContext context)
    {
      var baseException = context.Exception.GetBaseException();
      LoggerSvc.LogExecuteExceptiondAction(context.ExceptionContext);
      context.Result = new InternalServerErrorResult(
              "An unhandled exception occurred; check the log for more information. " + "Details: " + baseException.Message,
              Encoding.UTF8,
              context.Request);
    }


  }


  public enum ContractExecutionStatesEnum { ExecutedSuccessfully, InternalProcessingError, IncorrectRequestedData }

  /// <summary>
  ///     Represents general (minimal) reponse generated by a sevice. All other responses should be derived from this class.
  /// </summary>
  public class ContractExecutionResult
  {
    /// <summary>
    ///     Initializes a new instance of the <see cref="ContractExecutionResult" /> class.
    /// </summary>
    /// <param name="code">The resulting code. Default value is <see cref="ContractExecutionStates.ExecutedSuccessfully" /></param>
    /// <param name="message">The verbose user-friendly message. Default value is empty string.</param>
    public ContractExecutionResult(ContractExecutionStatesEnum code, string message = "")
    {
      Code = code;
      Message = message;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///     <see cref="ContractExecutionStates.ExecutedSuccessfully" /> result
    /// </summary>
    /// <param name="message">The verbose user-friendly message.</param>
    protected ContractExecutionResult(string message)
      : this(ContractExecutionStatesEnum.ExecutedSuccessfully, message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///     <see cref="ContractExecutionStates.ExecutedSuccessfully" /> result and "success" message
    /// </summary>
    public ContractExecutionResult()
      : this("success")
    {
    }


    /// <summary>
    /// Defines machine-readable code.
    /// </summary>
    /// <value>
    /// Result code.
    /// </value>
    public ContractExecutionStatesEnum Code { get; protected set; }
    /// <summary>
    /// Defines user-friendly message.
    /// </summary>
    /// <value>
    /// The message string.
    /// </value>
    public string Message { get; protected set; }

    /// <summary>
    ///     Gets the help sample.
    /// </summary>
    /// <value>
    ///     The help sample.
    /// </value>
    public static ContractExecutionResult HelpSample
    {
      get { return new ContractExecutionResult("success"); }
    }
  }
  public class InternalServerErrorResult : IHttpActionResult
  {
    public InternalServerErrorResult(
        string content,
        Encoding encoding,
        HttpRequestMessage request)
    {
      if (content == null)
      {
        throw new ArgumentNullException("content");
      }

      if (encoding == null)
      {
        throw new ArgumentNullException("encoding");
      }

      if (request == null)
      {
        throw new ArgumentNullException("request");
      }

      this.Content = content;
      this.Encoding = encoding;
      this.Request = request;
    }

    public string Content { get; private set; }

    public Encoding Encoding { get; private set; }

    public HttpRequestMessage Request { get; private set; }

    public ContractExecutionResult ExecutionResult
    {
      get
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, this.Content);
      }
    }

    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
      return Task.FromResult(Execute());
    }

    private HttpResponseMessage Execute()
    {
      var response = this.Request.CreateResponse(HttpStatusCode.InternalServerError);
      response.Content = new StringContent(JsonConvert.SerializeObject(this.ExecutionResult));
      return response;
    }
  }
}