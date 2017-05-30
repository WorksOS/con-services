using System;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using MasterDataProxies.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;

namespace ProjectWebApi.Filters
{
  /// <summary>
  /// authentication
  /// </summary>
  public class TIDAuthentication
  {
    private readonly RequestDelegate _next;
    private ILogger<TIDAuthentication> log;
    private readonly ICustomerProxy customerProxy;


    public TIDAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      ILoggerFactory logger)
    {
      log = logger.CreateLogger<TIDAuthentication>();
      this.customerProxy = customerProxy;
      _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
      if (!context.Request.Path.Value.Contains("swagger"))
      {
        bool requiresCustomerUid =
          !(context.Request.Path.Value.Contains("v3") && context.Request.Method.ToUpper() != "GET");

        string authorization = context.Request.Headers["X-Jwt-Assertion"];
        string customerUid = context.Request.Headers["X-VisionLink-CustomerUID"];

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization) || (requiresCustomerUid && string.IsNullOrEmpty(customerUid)))
        {
          log.LogWarning("No account selected for the request");
          await SetResult("No account selected", context);
          return;
        }

        try
        {
          var jwtToken = new TPaaSJWT(authorization);
          var identity = new GenericIdentity(jwtToken.UserUid.ToString());
          context.User = new TidCustomPrincipal(identity, customerUid, jwtToken.EmailAddress);
        }
        catch (Exception e)
        {
          log.LogWarning("Invalid JWT token with exception {0}", e.Message);
          await SetResult("Invalid authentication", context);
          return;
        }
        
        // User must have be authenticated against this customer
        if (!string.IsNullOrEmpty(customerUid))
        {
          CustomerDataResult customers = await customerProxy.GetCustomersForMe(context.Request.Headers.GetCustomHeaders());
          if (customers.Code != 0 || customers.CustomerDescriptors.Count < 1 || !customers.CustomerDescriptors.Exists(x => x.Uid == customerUid))
          {
            log.LogWarning("User is not authorized to configure this customer");
            await SetResult("User is not authorized to configure this customer", context);
            return;
          }
        }
        log.LogInformation("Authorization: for Customer: {0} userUid: {1} allowed", customerUid, Guid.Parse(((context.User as TidCustomPrincipal).Identity as GenericIdentity).Name));
      }
      await _next.Invoke(context);
    }

    private async Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = 403;
      await context.Response.WriteAsync(message);
    }
  }

  public static class TIDAuthenticationExtensions
  {
    public static IApplicationBuilder UseTIDAuthentication(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<TIDAuthentication>();
    }
  }
}
