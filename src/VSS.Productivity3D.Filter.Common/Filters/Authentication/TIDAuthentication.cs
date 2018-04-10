using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Threading.Tasks;
using VSS.Authentication.JWT;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Filters.Authentication
{
  /// <summary>
  /// Authentication middleware.
  /// </summary>
  public class TIDAuthentication
  {
    private readonly RequestDelegate NextRequestDelegate;
    private readonly ILogger<TIDAuthentication> Log;
    private readonly ICustomerProxy CustomerProxy;
    private readonly IProjectListProxy ProjectListProxy;
    private readonly IConfigurationStore Store;

    /// <summary>
    /// Service exception handler.
    /// </summary>
    protected IServiceExceptionHandler ServiceExceptionHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TIDAuthentication"/> class.
    /// </summary>
    /// <param name="nextRequestDelegate">The nextRequestDelegate.</param>
    /// <param name="customerProxy">The customer proxy.</param>
    /// <param name="store">The store.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="serviceExceptionHandler"></param>
    public TIDAuthentication(RequestDelegate nextRequestDelegate,
      ICustomerProxy customerProxy,
      IProjectListProxy projectListProxy,
      IConfigurationStore store,
      ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler)
    {
      this.Log = logger.CreateLogger<TIDAuthentication>();
      this.CustomerProxy = customerProxy;
      this.ProjectListProxy = projectListProxy;
      this.NextRequestDelegate = nextRequestDelegate;
      this.Store = store;
      ServiceExceptionHandler = serviceExceptionHandler;
    }

    /// <summary>
    /// Invokes the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
      if (!context.Request.Path.Value.Contains("/swagger/"))
      {
        bool isApplicationContext;
        string applicationName;
        string userUid;
        string userEmail;

        string authorization = context.Request.Headers["X-Jwt-Assertion"];
        string customerUid = context.Request.Headers["X-VisionLink-CustomerUID"];

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization) || (string.IsNullOrEmpty(customerUid)))
        {
          this.Log.LogWarning("No account selected for the Request");
          await SetResult("No account selected", context);
          return;
        }

        try
        {
          var jwtToken = new TPaaSJWT(authorization);
          isApplicationContext = jwtToken.IsApplicationToken;
          applicationName = jwtToken.ApplicationName;
          userEmail = jwtToken.EmailAddress;
          userUid = isApplicationContext
            ? jwtToken.ApplicationId
            : jwtToken.UserUid.ToString();
        }
        catch (Exception e)
        {
          this.Log.LogWarning("Invalid JWT token with exception {0}", e.Message);
          await SetResult("Invalid authentication", context);
          return;
        }

        var customHeaders = context.Request.Headers.GetCustomHeaders();
        //If this is an application context do not validate user-customer
        if (isApplicationContext)
        {
          //Set calling context Principal
          context.User = new TIDCustomPrincipal(new GenericIdentity(userUid), customerUid, userEmail, "Application", ProjectListProxy, customHeaders , isApplication: true);
          this.Log.LogInformation(
            "Authorization: Calling context is Application Context for Customer: {0} Application: {1} ApplicationName: {2}",
            customerUid, userUid, applicationName);

          await this.NextRequestDelegate.Invoke(context);
          return;
        }
        CustomerData customer;

        // User must have be authenticated against this customer
        try
        {
          customer = await CustomerProxy.GetCustomerForUser(userUid, customerUid, customHeaders);
          if (customer == null)
          {
            var error = $"User {userUid} is not authorized to configure this customer {customerUid}";
            this.Log.LogWarning(error);
            await SetResult(error, context);
            return;
          }
        }
        catch (Exception e)
        {
          this.Log.LogWarning(
            $"Unable to access the 'customerProxy.GetCustomersForMe' endpoint: {this.Store.GetValueString("CUSTOMERSERVICE_API_URL")}. Message: {e.Message}.");
          await SetResult("Failed authentication", context);
          return;
        }

        this.Log.LogInformation("Authorization: for Customer: {0} UserUid: {1} UserEmail: {2} allowed", customerUid, userUid,
          userEmail);
        //Set calling context Principal
        context.User = new TIDCustomPrincipal(new GenericIdentity(userUid), customerUid, userEmail, customer.name,
          ProjectListProxy, customHeaders, isApplication: isApplicationContext);
      }

      await this.NextRequestDelegate.Invoke(context);
    }

    private async Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
      await context.Response.WriteAsync(message);
    }
  }
}