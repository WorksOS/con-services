using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using VSS.Raptor.Service.WebApiModels.Models;

namespace VSS.Raptor.Service.WebApiModels.Filters
{
    public class TIDAuthentication
    {
        private readonly RequestDelegate _next;

        public TIDAuthentication(RequestDelegate next)
        {
            _next = next;
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
            if (!jwtToken.SetToken(token))
            {
                await SetResult("Invalid authentication", context);
                return;
            }
            context.User = new GenericPrincipal(new GenericIdentity(jwtToken.UserUID, customerUID), new string[] {});
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
