using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;


namespace VSS.WebApi.Common
{
  /// <summary>
  /// TPaaS authentication.
  /// </summary>
  public class TIDAuthentication
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<TIDAuthentication> log;
    private readonly ICustomerProxy customerProxy;
    private readonly IConfigurationStore store;

    /// <summary>
    /// Service exception handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TIDAuthentication"/> class.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <param name="customerProxy">The customer proxy.</param>
    /// <param name="store">The configStore.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler"></param>
    public TIDAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler)
    {
      log = logger.CreateLogger<TIDAuthentication>();
      this.customerProxy = customerProxy;
      this._next = next;
      this.store = store;
      ServiceExceptionHandler = serviceExceptionHandler;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
      if (!context.Request.Path.Value.Contains("/swagger/") && !InternalConnection(context))
      {
        bool isApplicationContext = false;
        string applicationName = string.Empty;
        string userUid = string.Empty;
        string userEmail = string.Empty;
        bool requireCustomerUid = RequireCustomerUid(context);
        string customerUid = string.Empty;
        string customerName = string.Empty;

        string authorization = context.Request.Headers["X-Jwt-Assertion"];

        if (requireCustomerUid)
        {
          customerUid = context.Request.Headers["X-VisionLink-CustomerUID"];
        }

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization) || (string.IsNullOrEmpty(customerUid) && requireCustomerUid))
        {
          log.LogWarning("No account selected for the request");
          await SetResult("No account selected", context);
          return;
        }

        try
        {
          var jwtToken = new TPaaSJWT(authorization);
          isApplicationContext = jwtToken.IsApplicationToken;
          applicationName = jwtToken.ApplicationName;
          userEmail = isApplicationContext ? applicationName : jwtToken.EmailAddress;
          userUid = isApplicationContext ? jwtToken.ApplicationId : jwtToken.UserUid.ToString();
        }
        catch (Exception e)
        {
          log.LogWarning("Invalid JWT token with exception {0}", e.Message);
          await SetResult("Invalid authentication", context);
          return;
        }

        var customHeaders = context.Request.Headers.GetCustomHeaders();
        //If this is an application context do not validate user-customer
        if (isApplicationContext)
        {
          log.LogInformation(
            $"Authorization: Calling context is 'Application' for Customer: {customerUid} Application: {userUid} ApplicationName: {applicationName}");

          customerName = "Application";
        }
        // User must have be authenticated against this customer
        else if (requireCustomerUid)
        {
          try
          {
            var customer = await customerProxy.GetCustomerForUser(userUid, customerUid, customHeaders);
            if (customer == null)
            {
              var error = $"User {userUid} is not authorized to configure this customer {customerUid}";
              log.LogWarning(error);
              await SetResult(error, context);
              return;
            }
            customerName = customer.name;
          }
          catch (Exception e)
          {
            log.LogWarning(
              $"Unable to access the 'customerProxy.GetCustomersForMe' endpoint: {store.GetValueString("CUSTOMERSERVICE_API_URL")}. Message: {e.Message}.");
            await SetResult("Failed authentication", context);
            return;
          }
        }
        else
        {
          customerName = "Unknown";
        }
    
        log.LogInformation("Authorization: for Customer: {0} userUid: {1} userEmail: {2} allowed", customerUid, userUid,
          userEmail);
        //Set calling context Principal
        context.User = CreatePrincipal(userUid, customerUid, customerName, userEmail, isApplicationContext, customHeaders);
      }

      await this._next.Invoke(context);
    }

    /// <summary>
    /// If true, bypasses authentication. Override in a service if required.
    /// </summary>
    public virtual bool InternalConnection(HttpContext context)
    {
      return false;
    }

    /// <summary>
    /// If true, the customer-user association is validated. Override in a service if required.
    /// </summary>
    public virtual bool RequireCustomerUid(HttpContext context)
    {
      return true;
    }

    /// <summary>
    /// Creates a TID principal. Override in a service to create custom service principals.
    /// </summary>
    public virtual TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, string userEmail, 
      bool isApplicationContext, IDictionary<string, string> contextHeaders)
    {
      return new TIDCustomPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext);
    }

    private async Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = 403;
      await context.Response.WriteAsync(message);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public static class TIDAuthenticationExtensions
  {
    /// <summary>
    /// Uses the tid authentication.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns></returns>
    public static IApplicationBuilder UseTIDAuthentication(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<TIDAuthentication>();
    }
  }
}
