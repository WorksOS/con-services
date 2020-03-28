using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.FIlters;

namespace MockProjectWebApi
{
  public class Startup
  {
    public Startup(IWebHostEnvironment env)
    {
      var builder = new ConfigurationBuilder()
          .SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      Configuration = builder.Build();
    }

    public IConfigurationRoot Configuration { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
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
      services.AddSingleton<IGeofenceservice, GeofenceService>();
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline
    /// </summary>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
      app.UseExceptionTrap();
      app.UseCors("VSS");
      app.UseExceptionDummyPostMiddleware();

      app.UseMvc();
    }
  }
}
