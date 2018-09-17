using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Webtools
{
  public class Startup
  {

    public const string LOGGER_REPO_NAME = "Webtools";

    public Startup(IConfigurationStore configuration)
    {
      Configuration = configuration;
    }

    public IConfigurationStore Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

      //services.AddCommon<Startup>(SERVICE_TITLE, "API for TRex Gateway");

      //Set up logging etc. for TRex
      var serviceProvider = services.BuildServiceProvider();
      var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
      Logging.Logger.Inject(loggerFactory);
      DIContext.Inject(serviceProvider);

      services.AddSingleton<ITRexGridFactory>(new TRexGridFactory());
      serviceProvider = services.BuildServiceProvider();
      DIContext.Inject(serviceProvider);

      services.AddSingleton(new ImmutableClientServer("Webtools-Immutable"));
      services.AddSingleton(new MutableClientServer("Webtools-Mutable"));
      serviceProvider = services.BuildServiceProvider();
      DIContext.Inject(serviceProvider);


      // In production, the Angular files will be served from this directory
      services.AddSpaStaticFiles(configuration =>
      {
        configuration.RootPath = "ClientApp/dist";
      });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }
      else
      {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
      }

      app.UseHttpsRedirection();
      app.UseStaticFiles();
      app.UseSpaStaticFiles();

      app.UseMvc(routes =>
      {
        routes.MapRoute(
                  name: "default",
                  template: "{controller}/{action=Index}/{id?}");
      });

      app.UseSpa(spa =>
      {
              // To learn more about options for serving an Angular SPA from ASP.NET Core,
              // see https://go.microsoft.com/fwlink/?linkid=864501

              spa.Options.SourcePath = "ClientApp";

        if (env.IsDevelopment())
        {
          spa.UseAngularCliServer(npmScript: "start");
        }
      });
    }
  }
}
