using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Common.Filters.Authentication
{
  public class TIDAuthentication
  {
    private readonly RequestDelegate _next;
    private readonly IProjectListProxy projectListProxy;
    private readonly ICustomerProxy customerProxy;
    private readonly ILogger log;


    public TIDAuthentication(RequestDelegate next, IProjectListProxy projectListProxy, ICustomerProxy customerProxy, ILoggerFactory logger)
    {
      _next = next;
      this.projectListProxy = projectListProxy;
      this.customerProxy = customerProxy;
      log = logger.CreateLogger<TIDAuthentication>();
    }

    public async Task Invoke(HttpContext context)
    {
      if (!context.Request.Path.Value.Contains("swagger"))
      {
        bool isApplicationContext = false;
        string applicationName = "";
        string userUid = "";

        string authorization = context.Request.Headers["X-Jwt-Assertion"];
        string customerUid = context.Request.Headers["X-VisionLink-CustomerUid"];

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization) || string.IsNullOrEmpty(customerUid))
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
          try
          {
            var customerResult =
              await customerProxy.GetCustomersForMe(userUid, context.Request.Headers.GetCustomHeaders());
            if (customerResult.status != 200 || customerResult.customer == null ||
                customerResult.customer.Count < 1 ||
                !customerResult.customer.Exists(x => x.uid == customerUid))
            {
              var error = $"User {userUid} is not authorized for this customer {customerUid}";
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
          log.LogInformation(
            $"Authorization: Calling context is 'User' for Customer: {customerUid} User: {userUid}");

        }

        // get projectList for this customer and setup CustomContext
        try
        {
          var customerProjects = await projectListProxy.GetProjectsV4(customerUid, context.Request.Headers.GetCustomHeaders());
          var authProjects = new List<ProjectDescriptor>();
          if (customerProjects != null)
          {
            foreach (var project in customerProjects)
            {
              var projectDesc = new ProjectDescriptor
              {
                isLandFill = project.ProjectType == ProjectType.LandFill,
                isArchived = project.IsArchived,
                projectUid = project.ProjectUid,
                projectId = project.LegacyProjectId,
                coordinateSystemFileName = project.CoordinateSystemFileName,
                projectGeofenceWKT = project.ProjectGeofenceWKT,
                projectTimeZone = project.ProjectTimeZone
              };
              authProjects.Add(projectDesc);
            }
          }
          log.LogDebug($"Authorization: for Customer: {customerUid} projectList is: {authProjects.Count}");

          // note that userUid may actually be the ApplicationId if isApplicationContext
          var identity = string.IsNullOrEmpty(customerUid)
            ? new GenericIdentity(userUid)
            : new GenericIdentity(userUid, customerUid);
          var principal = new RaptorPrincipal(identity, customerUid, authProjects, isApplicationContext);
          context.User = principal;
          //Thread.CurrentPrincipal = principal;
        }
        catch (Exception)
        {
          await SetResult("Invalid authentication", context);
          return;
        }
      }
      await _next.Invoke(context);
    }

    private async Task SetResult(string message, HttpContext context)
    {
      context.Response.StatusCode = 403;
      await context.Response.WriteAsync(message);
    }
  }
}
