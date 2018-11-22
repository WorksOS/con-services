using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.WebApi.Common
{
  public class RequestIDMiddleware
  {
    private readonly RequestDelegate NextRequestDelegate;

    public const string RequestIDHeaderName = "X-VSS-Request-ID";
    public const string RequestIDAttributeName = "RequestID";

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="nextRequestDelegate"></param>
    public RequestIDMiddleware(RequestDelegate nextRequestDelegate)
    {
      this.NextRequestDelegate = nextRequestDelegate;
    }

    /// <summary>
    /// Callback executed on each request made to the server.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> object.</param>
    public async Task Invoke(HttpContext context)
    {
      if (context.Request.Headers.ContainsKey(RequestIDHeaderName))
        context.Items[RequestIDAttributeName] = context.Request.Headers[RequestIDHeaderName];
      else
      {
        context.Items[RequestIDAttributeName] = Guid.NewGuid();
        context.Request.Headers[RequestIDHeaderName] = context.Items[RequestIDAttributeName].ToString();
      }
      context.Response.OnStarting(() =>
      {
        if (!context.Response.Headers.ContainsKey(RequestIDHeaderName))
          context.Response.Headers[RequestIDHeaderName] = context.Items[RequestIDAttributeName].ToString();
        return Task.FromResult(0);
      });

      await this.NextRequestDelegate.Invoke(context);


    }
  }
}
