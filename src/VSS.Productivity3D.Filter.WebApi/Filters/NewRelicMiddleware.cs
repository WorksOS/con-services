using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VSS.Common.Exceptions;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Filters
{
  /// <summary>
  /// This middleware logs events into NewRelic. This must be added after TIDAuth middleware
  /// </summary>
  public class NewRelicMiddleware
  {
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

      var eventAttributes = new Dictionary<string, object>
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

      try
      {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        await this.NextRequestDelegate.Invoke(context);
        watch.Stop();

        await context.Response.Body.CopyToAsync(responseBodyStream);
        responseBodyStream.Seek(0, SeekOrigin.Begin);

        var responseBodyText = new StreamReader(responseBodyStream).ReadToEnd();
        obj = JObject.Parse(responseBodyText);

        eventAttributes.Add("elapsedTime", (Single)watch.ElapsedMilliseconds);
        // ContractExecutionResult response properties
        eventAttributes.Add("code", obj.GetProperty("Code"));
        eventAttributes.Add("message", obj.GetProperty("Message"));

        //Processing results
        eventAttributes.Add("ProcessingResult", obj.GetProperty("Result"));
        eventAttributes.Add("AssetIdResult", obj.GetProperty("assetId"));
        eventAttributes.Add("MachineLevelResult", obj.GetProperty("machineLevel"));
        eventAttributes.Add("ProjectIdResult", obj.GetProperty("projectId"));

        // Reset the response body stream.
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        await responseBodyStream.CopyToAsync(bodyStream);
      }
      catch (ServiceException exception)
      {
        await new MemoryStream(Encoding.UTF8.GetBytes(exception.GetContent)).CopyToAsync(bodyStream);
      }

      // Retrieve response properties for instrumentation recording.
      eventAttributes.Add("endpoint", context.Request.Path.ToString());
      eventAttributes.Add("result", context.Response.StatusCode.ToString());

      NewRelic.Api.Agent.NewRelic.RecordCustomEvent("TagFileAuth_Request", eventAttributes);
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