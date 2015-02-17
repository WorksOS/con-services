using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Cors;

namespace LandfillService.WebApi
{
    public class AuthNAuthZ : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext context)
        {
            //var principal =
            //  context.Request.GetRequestContext().Principal as ClaimsPrincipal;
            //return principal.Claims.Any(c => c.Type ==
            //  "http://yourschema/identity/claims/admin"
            //  && c.Value == "true");
            if (!context.Request.Headers.Contains("SessionID"))
            {
                System.Diagnostics.Debug.WriteLine("Unauthorised: missing SessionID header");
                return false;
            }

            System.Diagnostics.Debug.WriteLine("Authorising user: " + context.Request.Headers.GetValues("SessionID").First());

            // TODO: authorise the user - check that the request is for a valid project

            return true;
        }
    }

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.EnableCors(new EnableCorsAttribute("*", "*", "GET, POST, OPTIONS"));

            config.Filters.Add(new AuthNAuthZ());

            // Web API configuration and services
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "DefaultApi",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional }
            //);
        }
    }
}
