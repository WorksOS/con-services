using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.Swagger.Model;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Repositories;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ExceptionsTrapExtensions = VSS.Common.Exceptions.ExceptionsTrapExtensions;
using VSS.Common.Filters;

namespace VSS.Productivity3D.TagFileAuth.WebAPI
{
  public class Startup
  {
    private readonly string _loggerRepoName = "WebApi";
    private readonly bool _isDevEnv;
    IServiceCollection _serviceCollection;

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net("log4net.xml", _loggerRepoName);
      _isDevEnv = env.IsEnvironment("Development");

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();

      //Configure CORS
      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
                .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization", "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
                .WithMethods("OPTIONS", "TRACE", "GET", "HEAD", "POST", "PUT", "DELETE"));
      });

      // Add framework services.
      services.AddTransient<IRepositoryFactory, RepositoryFactory>()
          .AddTransient<IRepository<IAssetEvent>, AssetRepository>()
          .AddTransient<IRepository<ICustomerEvent>, CustomerRepository>()
          .AddTransient<IRepository<IDeviceEvent>, DeviceRepository>()
          .AddTransient<IRepository<IGeofenceEvent>, GeofenceRepository>()
          .AddTransient<IRepository<IProjectEvent>, ProjectRepository>()
          .AddTransient<IRepository<IFilterEvent>, FilterRepository>()
          .AddTransient<IRepository<ISubscriptionEvent>, SubscriptionRepository>();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddMvc(
        config =>
        {
          // for jsonProperty validation
          config.Filters.Add(new ValidationFilterAttribute());
        });
      services.AddSwaggerGen();

      services.ConfigureSwaggerGen(options =>
      {
        options.SingleApiVersion(new Info
        {
          Version = "v1",
          Title = "Tagfile authorization service API",
          Description = "API for Tagfile authorization service",
          TermsOfService = "None"
        });
        string path = _isDevEnv ? "bin/Debug/netcoreapp1.1/" : string.Empty;
        options.IncludeXmlComments(path + "VSS.Productivity3D.TagFileAuth.WebAPI.xml");
        options.IgnoreObsoleteProperties();
        options.DescribeAllEnumsAsStrings();
      });
      _serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      _serviceCollection.AddSingleton(loggerFactory);
      //new DependencyInjectionProvider(serviceCollection.BuildServiceProvider());
      _serviceCollection.BuildServiceProvider();

      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(_loggerRepoName);

      ExceptionsTrapExtensions.UseExceptionTrap(app);
      //Enable TID here
      //app.UseTIDAuthentication();
      app.UseCors("VSS");

      app.UseMvc();

      app.UseSwagger();
      app.UseSwaggerUi();
    }
  }
}