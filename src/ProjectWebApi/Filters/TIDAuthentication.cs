using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Repositories;
using VSS.Project.Service.WebApi.Authentication;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
            if (!context.Request.Path.Value.Contains("swagger"))
            {

                bool requiresCustomerUid = context.Request.Method.ToUpper() == "GET";

                string authorization = context.Request.Headers["X-Jwt-Assertion"];
                string customerUID = context.Request.Headers["X-VisionLink-CustomerUid"];

                // If no authorization header found, nothing to process further
                if (string.IsNullOrEmpty(authorization) || (requiresCustomerUid && string.IsNullOrEmpty(customerUID)))
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
                var identity = string.IsNullOrEmpty(customerUID)
                    ? new GenericIdentity(jwtToken.UserUID)
                    : new GenericIdentity(jwtToken.UserUID, customerUID);
                context.User = new GenericPrincipal(identity, new string[] {});
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
