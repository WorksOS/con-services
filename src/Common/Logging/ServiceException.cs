using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Newtonsoft.Json;
using LandfillService.Common.Contracts;

#pragma warning disable 1591
namespace LandfillService.Common
{
    /// <summary>
    /// This is an expected exception and should be ignored by unit test failure methods.
    /// </summary>
    /// 
    public class ServiceException : HttpResponseException
    {
        private string content ;
        
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

}
