using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Morph.Services.Core.Interfaces;
using SkuTester.DataModel;
using VSS.Common.ServiceDiscovery;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using WebApiContrib.Core.Formatter.Protobuf;

namespace VSS.Hydrology.WebApi
{
  public partial class Startup : BaseStartup
  {
    /// <inheritdoc />
    public override string ServiceName => "3dpm Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "API for 3D compaction and volume data";

    /// <inheritdoc />
    public override string ServiceVersion => "v1";

    /// <summary>
    /// Gets the default configuration object.
    /// </summary>
    public IConfigurationRoot ConfigurationRoot{ get; }

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
      services.AddServiceDiscovery();

      /*services.AddOpenTracing(builder =>
      {
        builder.ConfigureAspNetCore(options =>
        {
          options.Hosting.IgnorePatterns.Add(request => request.Request.Path.ToString() == "/ping");
        });
      });

      services.AddJaeger(SERVICE_TITLE);

      services.AddOpenTracing();*/
      
      ConfigureApplicationServices(services);

#if NET_4_7
      services.AddPrismServiceResolution();
      services.AddPrismService<ILandLeveling>();

      var landLeveling = services.BuildServiceProvider().GetService<ILandLeveling>();
      if(landLeveling == null)
        throw new Exception($"Failed to get {nameof(ILandLeveling)}");

      var configPathAndFilename = "..\\..\\TestData\\Sample\\TestCase.xml";
      var useCase = TestCase.Load(configPathAndFilename);
      if (useCase == null)
        throw new ArgumentException("Unable to load surface configuration");
      Log.LogInformation($"{nameof(ConfigureAdditionalServices)}: hydro surface configuration loaded: designFile {useCase.Surface} units: {(useCase.IsMetric ? "meters" : "us ft?")} points {(useCase.IsXYZ ? "xyz" : "nee")})");

      //var landLevelingInstance = ServiceLocator.Current.GetInstance<ILandLeveling>();
      //using (var landLevelingInstance = ServiceLocator.Current.GetInstance<ILandLeveling>())
      //{
      //  var surfaceInfo = (ISurfaceInfo) null;
      //  if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".dxf") == 0)
      //    surfaceInfo = landLevelingInstance.ImportSurface(useCase.Surface, (Action<float>) null);
      //  else
      //    throw new ArgumentException("Only DXF surface file type supported at present");

      //  if (surfaceInfo == null)
      //    throw new ArgumentException($"Unable to create Surface from: {useCase.Surface}");
      //}

#endif

    }

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseResponseCompression();
      app.UseMvc();
    }
  }
}
