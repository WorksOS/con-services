using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using VSS.Project.Data;
using VSS.Project.Service.WebApi.Authentication;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.Project.WebApi.Configuration.Principal.Models;

namespace VSS.Project.Service.WebApiModels.Filters
{
    public class TIDAuthentication
    {
        private readonly RequestDelegate _next;
        private IRepository<IProjectEvent> projectRepo;

        public TIDAuthentication(RequestDelegate next, IRepository<IProjectEvent> projects )
        {
            _next = next;
            projectRepo = projects;
        }

        public async Task Invoke(HttpContext context)
        {
            string token = null;
            string authorization = context.Request.Headers["X-Jwt-Assertion"];
            string customerUID = context.Request.Headers["X-VisionLink-CustomerUid"];

            // If no authorization header found, nothing to process further
            if (string.IsNullOrEmpty(authorization) || string.IsNullOrEmpty(customerUID))
            {
                await SetResult("No account selected", context);
                return;
            }

            token = authorization.Substring("Bearer ".Length).Trim();
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

            var projects = await (projectRepo as ProjectRepository).GetProjectsForUser(jwtToken.UserUID);

            var projectList = new Dictionary<long, ProjectDescriptor>();
            foreach (var userProject in projects)
            {
                projectList.Add(userProject.LegacyProjectID,
                  new ProjectDescriptor
                  {
                      ProjectType = userProject.ProjectType,
                      Name = userProject.Name,
                      ProjectTimeZone = userProject.ProjectTimeZone,
                      isArchived = userProject.IsDeleted || userProject.SubscriptionEndDate < DateTime.UtcNow,
                      StartDate = userProject.StartDate.ToString("O"),
                      EndDate = userProject.StartDate.ToString("O"),
                      ProjectUid = userProject.ProjectUID
                  });
            }

            context.User = new ProjectsPrincipal(new GenericIdentity(jwtToken.UserUID, customerUID), new string[] {}, projectList);
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
