using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KafkaConsumer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.Project.Data;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.UnifiedProductivity.Service.WebApiModels.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace ProjectWebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
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
                  .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization", "X-VisionLink-CustomerUid")
                  .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
            });

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
            services.AddSingleton<IConfigurationStore, KafkaConsumerConfiguration>();
            services.AddMvc();
            //Configure swagger
            services.AddSwaggerGen();

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "Unified Productivity API",
                    Description = "API for cycle and payload data",
                    TermsOfService = "None"
                });
                //string path = isDevEnv ? "bin/Debug/netcoreapp1.0/" : string.Empty;
                string path = string.Empty;
                options.IncludeXmlComments(path + "WebApi.xml");
                options.IgnoreObsoleteProperties();
                options.DescribeAllEnumsAsStrings();
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseExceptionTrap();
            //Enable TID here
            app.UseTIDAuthentication();
            app.UseCors("VSS");

            app.UseApplicationInsightsRequestTelemetry();

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();
        }
    }
}
