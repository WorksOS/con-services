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
using VSS.GenericConfiguration;

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
    protected readonly IConfigurationStore store;


    public TIDAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger)
    {
      log = logger.CreateLogger<TIDAuthentication>();
      this.customerProxy = customerProxy;
      _next = next;
      this.store = store;
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
          context.User = new TIDCustomPrincipal(identity, customerUid, jwtToken.EmailAddress);
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
          try
          {
            var userUid = ((context.User as TIDCustomPrincipal).Identity as GenericIdentity).Name;
            CustomerDataResult customerResult =
              await customerProxy.GetCustomersForMe(userUid, context.Request.Headers.GetCustomHeaders());
            if (customerResult.status != 200 || customerResult.customer == null || customerResult.customer.Count < 1 ||
                !customerResult.customer.Exists(x => x.uid == customerUid))
            {
              var error = $"User {userUid} is not authorized to configure this customer {customerUid}";
              log.LogWarning(error);
              await SetResult(error, context);
              return;
            }
          }
          catch (Exception e)
          {
            log.LogWarning($"Unable to access the 'customerProxy.GetCustomersForMe' endpoint: {store.GetValueString("CUSTOMERSERVICE_API_URL")}. Message: {e.Message}.");
            await SetResult("Failed authentication", context);
            return;
          }
        }
        log.LogInformation("Authorization: for Customer: {0} userUid: {1} allowed", customerUid,
          Guid.Parse(((context.User as TIDCustomPrincipal).Identity as GenericIdentity).Name));
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
