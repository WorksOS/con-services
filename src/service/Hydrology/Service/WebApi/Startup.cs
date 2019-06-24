using System;
using System.IO;
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
    public override string ServiceName => "Hydrology Service API";

    /// <inheritdoc />
    public override string ServiceDescription => "API for analyzing potential ponding";

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
      
      ConfigureApplicationServices(services);

#if NET_4_7
      try
      {
        services.AddPrismServiceResolution();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Prism DI failed");
        throw e;
      }
     services.AddPrismService<ILandLeveling>();

      var configPathAndFilename = "..\\..\\TestData\\Sample\\TestCase.xml";
      var useCase = TestCase.Load(configPathAndFilename);
      if (useCase == null)
        throw new ArgumentException("Unable to load surface configuration");
      Log.LogInformation($"{nameof(ConfigureAdditionalServices)}: hydro surface configuration loaded: designFile {useCase.Surface} units: {(useCase.IsMetric ? "meters" : "us ft?")} points {(useCase.IsXYZ ? "xyz" : "nee")})");

      //var landLeveling = services.BuildServiceProvider().GetService<ILandLeveling>();
      //if (landLeveling == null)
      //  throw new Exception($"Failed to get {nameof(ILandLeveling)}");
      //if (this.mSurface != null)
      //  throw new InvalidOperationException(Morph.Service.Resources.Properties.Resources.ErrorEngineAlreadyHasSurface);
      if (!File.Exists(useCase.Surface))
        throw new FileNotFoundException(useCase.Surface);


      try
      {
        using (var landLevelingInstance = services.BuildServiceProvider().GetService<ILandLeveling>())
        {
          var surfaceInfo = (ISurfaceInfo) null;
          if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".dxf") == 0)
            surfaceInfo = landLevelingInstance.ImportSurface(useCase.Surface, (Action<float>) null);
          else
            throw new ArgumentException("Only DXF surface file type supported at present");

          if (surfaceInfo == null)
            throw new ArgumentException($"Unable to create Surface from: {useCase.Surface}");
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Surface import failed");
        throw e;
      }

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
