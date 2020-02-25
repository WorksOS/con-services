using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using VSS.Authentication.JWT;

namespace VSS.Project.Service.WebApiModels.Filters
{
    public class TIDAuthentication
    {
        private readonly RequestDelegate _next;

        public TIDAuthentication(RequestDelegate next )
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Value.Contains("swagger"))
            {

                bool requiresCustomerUid = true; //context.Request.Method.ToUpper() == "GET"; Actually we do need to have customerUId regardless request

	        string authorization = context.Request.Headers["X-Jwt-Assertion"];
                string customerUID = context.Request.Headers["X-VisionLink-CustomerUid"];

                // If no authorization header found, nothing to process further
                if (string.IsNullOrEmpty(authorization) || (requiresCustomerUid && string.IsNullOrEmpty(customerUID)))
                {
                    await SetResult("No account selected", context);
                    return;
                }

                // If no token found, no further work possible
                if (string.IsNullOrEmpty(authorization))
                {
                    await SetResult("No authentication token", context);
                    return;
                }

                try
                {
                    var jwtToken = new TPaaSJWT(token);
                    var identity = string.IsNullOrEmpty(customerUID)
                        ? new GenericIdentity(jwtToken.UserUid.ToString())
                        : new GenericIdentity(jwtToken.UserUid.ToString(), customerUID);
                    context.User = new GenericPrincipal(identity, new string[] { });
                }
                catch (Exception e)
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
