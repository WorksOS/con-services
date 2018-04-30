using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VSS.MasterData.Models.FIlters
{
  public class RequestIDMiddleware
  {
    private readonly RequestDelegate NextRequestDelegate;

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
      if (context.Request.Headers.ContainsKey("X-Request-ID"))
        context.Items["RequestID"] = context.Request.Headers["X-Request-ID"];
      else
      {
        context.Items["RequestID"] = Guid.NewGuid();
        context.Request.Headers["X-Request-ID"] = context.Items["RequestID"].ToString();
      }
      context.Response.OnStarting(() =>
      {
        if (!context.Response.Headers.ContainsKey("X-Request-ID"))
          context.Response.Headers["X-Request-ID"] = context.Items["RequestID"].ToString();
        return Task.FromResult(0);
      });

      await this.NextRequestDelegate.Invoke(context);


    }
  }
}
