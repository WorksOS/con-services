using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;
using System.Threading.Tasks;
using MasterDataProxies;
using MasterDataProxies.Interfaces;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;
using VSS.Raptor.Service.Common.Filters.Authentication.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using MasterDataProxies;
using MasterDataProxies.Interfaces;

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

                if (string.IsNullOrEmpty(authorization))
                {
                    await SetResult("No authentication token", context);
                    return;
                }
                try
                {
                    var jwtToken = new TPaaSJWT(authorization);
                    var customerProjects = await projectListProxy.GetProjectsV4(customerUID,
                        context.Request.Headers.GetCustomHeaders());
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
                                coordinateSystemFileName = project.CoordinateSystemFileName
                            };
                            authProjects.Add(projectDesc);
                        }
                    }
                    authProjectsStore.SetAuthenticatedProjectList(customerUID, authProjects);
                    log.LogDebug("Authorization: for Customer: {0} projectList is: {1}", customerUID,
                        authProjects.Count);

                    var identity = string.IsNullOrEmpty(customerUID)
                        ? new GenericIdentity(jwtToken.UserUid.ToString())
                        : new GenericIdentity(jwtToken.UserUid.ToString(), customerUID);
                    var principal = new GenericPrincipal(identity, new string[] { });
                    context.User = principal;
                    //Thread.CurrentPrincipal = principal;
                }
                catch
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

    public static class TIDAuthenticationExtensions
    {
        public static IApplicationBuilder UseTIDAuthentication (this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TIDAuthentication>();
        }
    }
}
