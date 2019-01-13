using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using src.Utils;
using VSS.ConfigurationStore;
using VSS.Log4Net.Extensions;
using VSS.MasterData.Models.FIlters;

namespace MockProjectWebApi
{
  public class Startup
  {
    /// <summary>
    /// Log4net repository logger name.
    /// </summary>
    public const string LOGGER_REPO_NAME = "MockWebApi";

    private ILogger log;
    private IServiceCollection serviceCollection;

    public Startup(IHostingEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      env.ConfigureLog4Net(repoName: LOGGER_REPO_NAME, configFileRelativePath: "log4net.xml");

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddLogging();

      services.AddCors(options =>
      {
        options.AddPolicy("VSS", builder => builder.AllowAnyOrigin()
          .WithHeaders("Origin", "X-Requested-With", "Content-Type", "Accept", "Authorization",
            "X-VisionLink-CustomerUid", "X-VisionLink-UserUid")
          .WithMethods("OPTIONS", "TRACE", "GET", "POST", "DELETE", "PUT", "HEAD"));
      });

      services.AddMvc();
      services.AddSingleton<IConfigurationStore, GenericConfiguration>();
      services.AddSingleton<IFiltersService, FiltersService>();
      services.AddSingleton<IImportedFilesService, ImportedFilesService>();
      services.AddSingleton<IProjectService, ProjectService>();

      serviceCollection = services;
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
    {
      //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
      //loggerFactory.AddDebug();

      serviceCollection.AddSingleton(loggerFactory);
      var serviceProvider = serviceCollection.BuildServiceProvider();
      log = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(Startup));

      app.UseExceptionTrap();
      app.UseCors("VSS");
      app.UseExceptionDummyPostMiddleware();

      app.UseMvc();
    }
  }
}
