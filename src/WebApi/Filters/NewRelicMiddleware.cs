#if NET_4_7
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

namespace VSS.Productivity3D.Common.Filters
{
  /// <summary>
  /// This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
    private Dictionary<string, object> EventAttributes;
    private readonly RequestDelegate NextRequestDelegate;

    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <param name="nextRequestDelegate"></param>
    public NewRelicMiddleware(RequestDelegate nextRequestDelegate)
    {
      this.NextRequestDelegate = nextRequestDelegate;
    }

    /// <summary>
    /// Callback executed on each request made to the server.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext"/> object.</param>
    public async Task Invoke(HttpContext context)
    {
      // Serialize the request body into an object we can interrogate for NewRelic instrumentation purposes.
      var requestBodyStream = new MemoryStream();
      await context.Request.Body.CopyToAsync(requestBodyStream);
      requestBodyStream.Seek(0, SeekOrigin.Begin);

      var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
      var obj = JObject.Parse(requestBodyText);

      var requestEventAttributes = new Dictionary<string, object>
      {
        // Asset Id request properties
        {"projectId", obj.GetProperty("projectId")},
        {"deviceType", obj.GetProperty("deviceType")},
        {"radioSerial", obj.GetProperty("radioSerial")},
        // Project Id request properties
        {"assetId", obj.GetProperty("assetId")},
        {"latitude", obj.GetProperty("latitude")},
        {"longitude", obj.GetProperty("longitude")},
        {"height", obj.GetProperty("height")},
        {"timeOfPosition", obj.GetProperty("timeOfPosition")},
        {"tccOrgUid", obj.GetProperty("tccOrgUid")}
      };

      // Set the request body to our stream before invoking the request delegate.
      requestBodyStream.Seek(0, SeekOrigin.Begin);
      context.Request.Body = requestBodyStream;

      // In order to deserialize the response body we need to set it up now as a stream, before invoking the request delegate.
      var bodyStream = context.Response.Body;
      var responseBodyStream = new MemoryStream();
      context.Response.Body = responseBodyStream;

      var watch = System.Diagnostics.Stopwatch.StartNew();

      try
      {
        // Invoke the request and await it's response.
        await this.NextRequestDelegate.Invoke(context);
      }
      catch
      {
        // TODO The response body is being lost if a ServiceException is thrown during execution of the delegate.
        // When this occurs the response returns 200, with no response body. 
      }
      finally
      {
        watch.Stop();

        var responseEventAttributes = new Dictionary<string, object>
        {
          {"endpoint", context.Request.Path.ToString()},
          {"elapsedTime", (Single) watch.ElapsedMilliseconds},
          {"result", context.Response.StatusCode.ToString()}
        };

        requestEventAttributes.ToList().ForEach(x => responseEventAttributes.Add(x.Key, x.Value));

        NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileAuth_Request", responseEventAttributes);

        // Reset the response body stream.
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(bodyStream);
      }
    }
  }

  internal static class JObjectExtensions
  {
    public static string GetProperty(this JObject jObject, string property)
    {
      return jObject.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out JToken token)
        ? token.ToString()
        : null;
    }
  }
}
#endif