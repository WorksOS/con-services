using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.Http;
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
    protected readonly ILogger<TIDAuthentication> log;
    private readonly ICustomerProxy customerProxy;
    private readonly IConfigurationStore store;

    protected virtual List<string> IgnoredPaths => new List<string> { "/swagger/", "/cache/" };

    /// <summary>
    /// Service exception handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TIDAuthentication"/> class.
    /// </summary>
    public TIDAuthentication(RequestDelegate next,
      ICustomerProxy customerProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler)
    {
      log = logger.CreateLogger<TIDAuthentication>();
      this.customerProxy = customerProxy;
      _next = next;
      this.store = store;
      ServiceExceptionHandler = serviceExceptionHandler;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    public async Task Invoke(HttpContext context)
    {
      if (IgnoredPaths.Select(s => context.Request.Path.Value.Contains(s)).Contains(true))
      {
        await _next(context);
        return;
      }

      if (!InternalConnection(context))
      {
        bool isApplicationContext;
        string applicationName;
        string userUid;
        string userEmail;
        var customerUid = string.Empty;
        string customerName;

        string authorization = context.Request.Headers["X-Jwt-Assertion"];

        // If no authorization header found, nothing to process further
        // note keep these result messages vague (but distinct): https://www.gnucitizen.org/blog/username-enumeration-vulnerabilities/
        if (string.IsNullOrEmpty(authorization))
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
          if (isApplicationContext)
          {
            // Applications can override the User ID, so we can fetch 'per user' information
            // E.g Scheduled reports needs to get the Preferences for the user they are running on behalf of, not the Report Server settings.
            var overrideUserUid = context.Request.Headers[HeaderConstants.X_VISION_LINK_USER_UID];
            if (!string.IsNullOrEmpty(overrideUserUid))
            {
              log.LogInformation($"Overriding User ID via {HeaderConstants.X_VISION_LINK_USER_UID} header with {overrideUserUid}, for application request from {applicationName}.");
              userUid = overrideUserUid;
            }
          }
        }
        catch (Exception e)
        {
          log.LogWarning(e, "Invalid authentication with exception");
          await SetResult("Invalid authentication", context);
          return;
        }

        var requireCustomerUid = RequireCustomerUid(context);
        if (requireCustomerUid)
          customerUid = context.Request.Headers["X-VisionLink-CustomerUID"];

        // If required customer not provided, nothing to process further
        if (string.IsNullOrEmpty(customerUid) && requireCustomerUid)
        {
          log.LogWarning("No account found for the request");
          await SetResult("No account found", context);
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

        log.LogInformation($"Authorization: for Customer: {customerUid} userUid: {userUid} userEmail: {userEmail} allowed");
        //Set calling context Principal
        context.User = CreatePrincipal(userUid, customerUid, customerName, userEmail, isApplicationContext, customHeaders, applicationName);
      }

      await _next.Invoke(context);
    }

    /// <summary>
    /// If true, bypasses authentication. Override in a service if required.
    /// </summary>
    public virtual bool InternalConnection(HttpContext context) => false;

    /// <summary>
    /// If true, the customer-user association is validated. Override in a service if required.
    /// </summary>
    public virtual bool RequireCustomerUid(HttpContext context) => true;

    /// <summary>
    /// Creates a TID principal. Override in a service to create custom service principals.
    /// </summary>
    public virtual TIDCustomPrincipal CreatePrincipal(string userUid, string customerUid, string customerName, string userEmail,
      bool isApplicationContext, IDictionary<string, string> contextHeaders, string tpaasApplicationName = "")
    {
      return new TIDCustomPrincipal(new GenericIdentity(userUid), customerUid, customerName, userEmail, isApplicationContext, tpaasApplicationName);
    }

    private static Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      return context.Response.WriteAsync(message);
    }
  }
}
