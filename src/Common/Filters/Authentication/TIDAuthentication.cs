using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.Utilities;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace VSS.Raptor.Service.Common.Filters.Authentication
{
    public class TIDAuthentication
    {
        private readonly RequestDelegate _next;
        private readonly IAuthenticatedProjectsStore authProjectsStore;
        private readonly IProjectListProxy projectListProxy;
        private readonly ILogger log;


        public TIDAuthentication(RequestDelegate next, IAuthenticatedProjectsStore authProjectsStore, IProjectListProxy projectListProxy, ILoggerFactory logger)
        {
            _next = next;
            this.authProjectsStore = authProjectsStore;
            this.projectListProxy = projectListProxy;
            log = logger.CreateLogger<TIDAuthentication>();
        }

        public async Task Invoke(HttpContext context)
        {
          if (!context.Request.Path.Value.Contains("swagger"))
          {

            string authorization = context.Request.Headers["X-Jwt-Assertion"];
            string customerUID = context.Request.Headers["X-VisionLink-CustomerUid"];

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization) || string.IsNullOrEmpty(customerUID))
            {
              await SetResult("No account selected", context);
              return;
            }

            string token = authorization.Substring("Bearer ".Length).Trim();
            // If no token found, no further work possible
            if (string.IsNullOrEmpty(token))
            {
              await SetResult("No authentication token", context);
              return;
            }
            var jwtToken = new JWTToken();
            if (!jwtToken.SetToken(authorization))
            {
              await SetResult("Invalid authentication", context);
              return;
            }
            var customerProjects = projectListProxy.GetProjects(customerUID,
              RequestUtils.GetCustomHeaders(context.Request.Headers));
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
                  projectId = project.LegacyProjectId
                };
                authProjects.Add(projectDesc);
              }
            }
            authProjectsStore.SetAuthenticatedProjectList(customerUID, authProjects);
            log.LogDebug("Authorization: for Customer: {0} projectList is: {1}", customerUID, authProjects.Count);

            var identity = string.IsNullOrEmpty(customerUID)
              ? new GenericIdentity(jwtToken.UserUID)
              : new GenericIdentity(jwtToken.UserUID, customerUID);
            var principal = new GenericPrincipal(identity, new string[] {});
            context.User = principal;
            //Thread.CurrentPrincipal = principal;
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
        public static IApplicationBuilder UseTIDAuthentication (this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TIDAuthentication>();
        }
    }
}
