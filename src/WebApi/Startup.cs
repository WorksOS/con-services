using System;
using System.Net;
using System.Xml;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using log4netExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.Filters;
using VSS.Raptor.Service.Common.ResultHandling;
using TCCFileAccess;

namespace VSS.Raptor.Service.WebApi
{
    public class Startup
    {

        private readonly string loggerRepoName = "WebApi";
        private bool isDevEnv = false;
        IServiceCollection serviceCollection;

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            env.ConfigureLog4Net("log4net.xml", loggerRepoName);

            isDevEnv = env.IsEnvironment("Development");
            if (isDevEnv)
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                    .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
                        "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "Cache-Control")
                    .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
            });
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMemoryCache();
            //Configure swagger
            services.AddSwaggerGen();

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Raptor API",
                    Description = "API for 3D compaction and volume data",
                    TermsOfService = "None"
                });
                string path = isDevEnv ? "bin/Debug/net462/" : string.Empty;
                options.IncludeXmlComments(path + "WebApi.xml");
                options.IgnoreObsoleteProperties();
                options.DescribeAllEnumsAsStrings();
            });
            //Swagger documentation can be viewed with http://localhost:5000/swagger/ui/index.html   

            //Configure application services
            services.AddSingleton<IConfigurationStore, GenericConfiguration.GenericConfiguration>();
            services.AddSingleton<IProjectListProxy, ProjectListProxy>();
            services.AddSingleton<IFileRepository, FileRepository>();

            serviceCollection = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddLog4Net(loggerRepoName);

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            var serviceProvider =serviceCollection.BuildServiceProvider();
            app.UseExceptionTrap();
      		//Enable CORS before TID so OPTIONS works without authentication
      		app.UseCors("VSS");

            //For now don't use application insights as it clogs the log with lots of stuff.
            //app.UseApplicationInsightsRequestTelemetry();
            //app.UseApplicationInsightsExceptionTelemetry();

            //app.UseResponseCaching();//Disable for now
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
      }
    }
}
