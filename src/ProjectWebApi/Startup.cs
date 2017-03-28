using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.Project.Service.WebApiModels.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using log4netExtensions;
using KafkaConsumer.Kafka;
using Repositories;
using VSS.GenericConfiguration;
using ProjectWebApi.ResultsHandling;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies;

namespace ProjectWebApi
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
            services.AddLogging();

            //Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                    .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
                        "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "X-Jwt-Assertion")
                    .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
            });

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
            services.AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();
            services.AddTransient<IKafka, RdKafkaDriver>();
            services.AddSingleton<IConfigurationStore, GenericConfiguration>();
            services.AddSingleton<ISubscriptionProxy, SubscriptionProxy>();
            services.AddSingleton<IGeofenceProxy, GeofenceProxy>();

            services.AddMvc();
            //Configure swagger
            services.AddSwaggerGen();

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v3",
                    Title = "Project Master Data API",
                    Description = "API for project data",
                    TermsOfService = "None"
                });
                string path = isDevEnv ? "bin/Debug/netcoreapp1.1/" : string.Empty;
                options.IncludeXmlComments(path + "ProjectWebApi.xml");
                options.IgnoreObsoleteProperties();
                options.DescribeAllEnumsAsStrings();
            });
            serviceCollection = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);
            //new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
            serviceCollection.BuildServiceProvider();

            loggerFactory.AddDebug();
            loggerFactory.AddLog4Net(loggerRepoName);

            app.UseExceptionTrap();
            //Enable CORS before TID so OPTIONS works without authentication
            app.UseCors("VSS");
            //Enable TID here
            app.UseTIDAuthentication();

            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUi();
        }
    }
}
