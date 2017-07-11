using log4netExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.GenericConfiguration;
using VSS.Productivity3D.Common.Filters;
using VSS.Productivity3D.TCCFileAccess;

namespace VSS.Productivity3D.WebApi
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
                        "X-VisionLink-CustomerUid", "X-VisionLink-UserUid", "Cache-Control")
                    .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
            });
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            //Configure swagger
            services.AddSwaggerGen();

            services.ConfigureSwaggerGen(options =>
            {
                options.SingleApiVersion(new Info
                {
                    Version = "v1",
                    Title = "File Access API",
                    TermsOfService = "None"
                });
                string path = isDevEnv ? "bin/Debug/netcoreapp1.1/" : string.Empty;
                options.IncludeXmlComments(path + "WebApi.xml");
                options.IgnoreObsoleteProperties();
                options.DescribeAllEnumsAsStrings();

            });
            //Swagger documentation can be viewed with http://localhost:5000/swagger/ui/index.html   

            //Configure application services
            services.AddSingleton<IConfigurationStore, GenericConfiguration.GenericConfiguration>();
            services.AddSingleton<IFileRepository, FileRepository>();
            services.AddMvc();


            serviceCollection = services;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddLog4Net(loggerRepoName);

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
