using System;
using System.Threading;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.Common.ServiceDiscovery;
using VSS.Hydrology.WebApi.Abstractions.ResultsHandling;
using VSS.Hydrology.WebApi.Middleware;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using WebApiContrib.Core.Formatter.Protobuf;
#if NET_4_7
using Morph.Services.Core.Interfaces;
#endif

namespace VSS.Hydrology.WebApi
{
  public partial class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "Hydrology Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "API for analyzing potential ponding and drainage";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// Gets the default configuration object.
    /// </summary>
    private IConfigurationRoot ConfigurationRoot { get; }

    /// <inheritdoc />
    public Startup(IHostingEnvironment env) : base(env, null, useSerilog: true)
    {
      var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

      builder.AddEnvironmentVariables();
      ConfigurationRoot = builder.Build();
    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalServices(IServiceCollection services)
    {
      services.AddMvc(options =>
      {
        options.OutputFormatters.Add(new ProtobufOutputFormatter(new ProtobufFormatterOptions()));
      });

      services.AddResponseCompression();
      services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
      services.AddTransient<IWebRequest, GracefulWebRequest>();
      services.AddTransient<ICustomerProxy, CustomerProxy>();
      services.AddScoped<IServiceExceptionHandler, ServiceExceptionHandler>();
      services.AddScoped<IErrorCodesProvider, HydroErrorCodesProvider>();
      services.AddServiceDiscovery();

      ConfigureApplicationServices(services);

#if NET_4_7
      try
      {
        // generation of images uses PresentationFramework
        var thread = new Thread(CreateWpfApp);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // seems to be race condition 
        Thread.Sleep(100);
        services.AddPrismServiceResolution();
      }
      catch (Exception e)
      {
        Log.LogError(e, $"{nameof(ConfigureAdditionalServices)} Prism DI failed");
        throw e;
      }

      services.AddPrismService<ILandLeveling>();
#endif
    }


#if NET_4_7
    private static void CreateWpfApp()
    {
      new Application() {ShutdownMode = ShutdownMode.OnExplicitShutdown}.Run();
    }
#endif

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env,
      ILoggerFactory factory)
    {
      // too app.UseFilterMiddleware<HydrologyAuthentication>();
      app.UseResponseCompression();
      app.UseMvc();
    }
  }
}
