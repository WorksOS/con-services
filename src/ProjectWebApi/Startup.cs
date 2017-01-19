using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.Project.Data;
using VSS.Project.Service.Interfaces;
using VSS.Project.Service.Utils;
using VSS.Project.Service.Utils.Kafka;
using VSS.Project.Service.WebApiModels.Filters;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using log4netExtensions;
using KafkaConsumer;

namespace ProjectWebApi
{
  public class Startup
  {
    private readonly string loggerRepoName = "WebApi";
    private bool isDevEnv = false;

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
                .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization", "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
                .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      // Add framework services.
      services.AddApplicationInsightsTelemetry(Configuration);
      services.AddTransient<IRepository<IProjectEvent>, ProjectRepository>();
      services.AddTransient<IKafka, RdKafkaDriver>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddMvc();
      //Configure swagger
      services.AddSwaggerGen();

      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "Project Master Data API",
          Description = "API for project data",
          TermsOfService = "None"
        });
        string path = isDevEnv ? "bin/Debug/netcoreapp1.1/" : string.Empty;
        options.IncludeXmlComments(path + "ProjectWebApi.xml");
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });

    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

            app.UseExceptionTrap();
            //Enable TID here
            //app.UseTIDAuthentication();
            app.UseCors("VSS");
            app.UseExceptionTrap();
            //Enable TID here
            app.UseTIDAuthentication();
            app.UseCors("VSS");
      app.UseExceptionTrap();
      //Enable TID here
      //app.UseTIDAuthentication();
      app.UseCors("VSS");

      app.UseApplicationInsightsRequestTelemetry();

      app.UseApplicationInsightsExceptionTelemetry();

      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();
    }
  }
}
