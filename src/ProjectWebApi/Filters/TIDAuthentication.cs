using System;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Authentication.JWT;

namespace ProjectWebApi.Filters
{
    /// <summary>
    /// authentication
    /// </summary>
    public class TIDAuthentication
    {
        private readonly RequestDelegate _next;
        public static string EmailAddress;
        private ILogger<TIDAuthentication> log;


        public TIDAuthentication(RequestDelegate next, ILoggerFactory logger)
        {
            log = logger.CreateLogger<TIDAuthentication>();
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Value.Contains("swagger"))
            {
                bool requiresCustomerUid =
                    true; //context.Request.Method.ToUpper() == "GET"; Actually we do need to have customerUId regardless request

                string authorization = context.Request.Headers["X-Jwt-Assertion"];
                string customerUID = context.Request.Headers["X-VisionLink-CustomerUID"];

                // If no authorization header found, nothing to process further
                if (string.IsNullOrEmpty(authorization) || (requiresCustomerUid && string.IsNullOrEmpty(customerUID)))
                {
                    log.LogWarning("No account selected for the request");
                    await SetResult("No account selected", context);
                    return;
                }

                if (string.IsNullOrEmpty(authorization))
                {
                    log.LogWarning("No JWT token");
                }

                try
                {
                    var jwtToken = new TPaaSJWT(authorization);
                    var identity = string.IsNullOrEmpty(customerUID)
                        ? new GenericIdentity(jwtToken.UserUid.ToString())
                        : new GenericIdentity(jwtToken.UserUid.ToString(), customerUID);
                    context.User = new GenericPrincipal(identity, new string[] { });
                    EmailAddress = jwtToken.EmailAddress;
                }
                catch (Exception e)
                {
                    log.LogWarning("Invalid JWT token with exception {0}", e.Message);
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
        public static IApplicationBuilder UseTIDAuthentication(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TIDAuthentication>();
        }
    }
}
