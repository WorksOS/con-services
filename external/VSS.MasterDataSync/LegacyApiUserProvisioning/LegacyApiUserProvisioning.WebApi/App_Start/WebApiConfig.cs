using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Filters;
using LegacyApiUserProvisioning.WebApi.Filters;
using System.Web.Http.Cors;

namespace LegacyApiUserProvisioning.WebApi
{
    public static class WebApiConfig
    {
        public static void SetUpCors(HttpConfiguration config)
        {
            var corsAttr = new EnableCorsAttribute("*", "*", "POST, OPTIONS, PUT, GET");
            config.EnableCors(corsAttr);
        }

        public static void Register(HttpConfiguration config)
        {
            SetUpCors(config);

            // Web API routes
            config.MapHttpAttributeRoutes();
            RegisterWebApiFilters(GlobalConfiguration.Configuration.Filters);
        }

        public static void RegisterWebApiFilters(HttpFilterCollection filters)
        {
            filters.Add(new ApiExceptionHandler());
        }
    }
}