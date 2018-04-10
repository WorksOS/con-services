using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Authentication.JWT;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  public class TIDAuthentication
  {
    private readonly RequestDelegate next;
    private readonly IProjectListProxy projectListProxy;
    private readonly ICustomerProxy customerProxy;
    private readonly ILogger log;

    public TIDAuthentication(RequestDelegate next, IProjectListProxy projectListProxy, ICustomerProxy customerProxy,
      ILoggerFactory logger)
    {
      this.next = next;
      this.projectListProxy = projectListProxy;
      this.customerProxy = customerProxy;
      log = logger.CreateLogger<TIDAuthentication>();
    }

    public async Task Invoke(HttpContext context)
    {

      //HACK allow internal connections without authn for tagfile submission by the harvester
      bool internalConnection =
        (context.Request.Path.Value.Contains("api/v1/tagfiles") ||
         context.Request.Path.Value.Contains("api/v2/tagfiles")) && context.Request.Method == "POST" &&
        context.Request.HttpContext.Connection.RemoteIpAddress.ToString().StartsWith("10.") &&
        !context.Request.Headers.ContainsKey("X-Jwt-Assertion") &&
        !context.Request.Headers.ContainsKey("Authorization");

      if (!context.Request.Path.Value.Contains("swagger") && !internalConnection)
      {
        bool isApplicationContext;
        string applicationName;
        string userUid;
        string username;
        bool requireCustomerUid = true;
        string customerUid = "";

        string authorization = context.Request.Headers["X-Jwt-Assertion"];

        // The v1 TAG file submission end point does not require a customer UID to be provided
        // However there is some schizophrenia here as we need to support UI manual tag file submission WITH proper authn\z as well
        if ((context.Request.Path.Value.Contains("api/v1/tagfiles") ||
             context.Request.Path.Value.Contains("api/v2/tagfiles")) && context.Request.Method == "POST"
            && !context.Request.Headers.ContainsKey("X-VisionLink-CustomerUid"))
          requireCustomerUid = false;
        else
          customerUid = context.Request.Headers["X-VisionLink-CustomerUid"];

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
          username = jwtToken.EmailAddress;
          userUid = isApplicationContext
            ? jwtToken.ApplicationId
            : jwtToken.UserUid.ToString();
        }
        catch (Exception e)
        {
          log.LogWarning("Invalid JWT token with exception {0}", e.Message);
          await SetResult("Invalid authentication", context);
          return;
        }

        var customHeaders = context.Request.Headers.GetCustomHeaders();
        CustomerData customer = null;

        //If this is an application context do not validate user-customer.
        //  If a user exists jwtToken.IsApplicationUserToken, then it may not be the user being acted on.
        if (isApplicationContext)
        {
          log.LogInformation(
            $"Authorization: Calling context is 'Application' for Customer: {customerUid} Application: {userUid} ApplicationName: {applicationName}");

          // todo is there any broad filtering we can do?
          //if (context.Request.Method != HttpMethod.Get.Method)
          //await SetResult("Failed authentication", context);
          //return;
        }
        else
        {
          // User must have authentication for this customer
          if (requireCustomerUid)
          {
            try
            {
              customer = await customerProxy.GetCustomerForUser(userUid, customerUid, customHeaders);
              if (customer == null)
              {
                var error = $"User {userUid} is not authorized to configure this customer {customerUid}";
                log.LogWarning(error);
                await SetResult(error, context);
                return;
              }
            }
            catch (Exception e)
            {
              log.LogWarning($"Unable to access 'customerProxy.GetCustomersForMe'. Message: {e.Message}.");
              await SetResult("Failed authentication", context);
              return;
            }
          }

          log.LogInformation(
            $"Authorization: Calling context is 'User' for Customer: {customerUid} User: {userUid}");
        }

        // get projectList for this customer and setup CustomContext
        if (requireCustomerUid)
        {
          try
          {

            //Delegate customer->project association resolution to the principal object for now as it has execution context and can invalidate cache if required
            // note that userUid may actually be the ApplicationId if isApplicationContext
            var identity = string.IsNullOrEmpty(customerUid)
              ? new GenericIdentity(userUid)
              : new GenericIdentity(userUid, customerUid);
            //this params were validated and are expected to be non-null
            var principal = new RaptorPrincipal(identity, customerUid, username,
              isApplicationContext ? "Application" : customer.name, projectListProxy,
              customHeaders, isApplicationContext);
            context.User = principal;
          }
          catch (Exception ex)
          {
            log.LogError($"Error setting custom context: {ex.GetBaseException().Message}.");

            await SetResult("Invalid authentication", context);

            return;
          }
        }
      }

      await this.next.Invoke(context);
    }

    private static async Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      await context.Response.WriteAsync(message);
    }
  }
}