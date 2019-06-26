using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Windows;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.ServiceDiscovery;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.WebApi.Common;
using WebApiContrib.Core.Formatter.Protobuf;
using Trimble.Vce.Data.Skp;

#if NET_4_7
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using SkuTester.DataModel;
using System.Windows.Media.Imaging;
#endif

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
        // generation of ponding map (GeneratePondMap) uses PresentationFramework
        var thread = new Thread(CreateWpfApp);
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        // seems to be race condition 
        Thread.Sleep(100);
        services.AddPrismServiceResolution();
      }
      catch (Exception e)
      {
        Log.LogError(e, "{nameof(ConfigureAdditionalServices)} Prism DI failed");
        throw e;
      }

      services.AddPrismService<ILandLeveling>();

      // temporarily use this sample originalGround mesh
      var sourcePathAndFilename = "..\\..\\TestData\\Sample\\TestCase.xml";
      var useCase = TestCase.Load(sourcePathAndFilename);

      if (useCase == null)
        throw new ArgumentException("Unable to load surface configuration");
      Log.LogInformation(
        $"{nameof(ConfigureAdditionalServices)} surface configuration loaded: designFile {useCase.Surface} units: {(useCase.IsMetric ? "meters" : "us ft?")} pointType: {(useCase.IsXYZ ? "xyz" : "nee")})");

      if (!File.Exists(useCase.Surface))
        throw new FileNotFoundException($"Original Surface file not found {useCase.Surface}");
      
      try
      {
        using (var landLevelingInstance = services.BuildServiceProvider().GetService<ILandLeveling>())
        {
          var surfaceInfo = (ISurfaceInfo) null;
          if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".dxf") == 0)
            surfaceInfo = landLevelingInstance.ImportSurface(useCase.Surface, (Action<float>) null);
          else
            throw new ArgumentException("{nameof(ConfigureAdditionalServices)} Only DXF surface file type supported at present");

          if (surfaceInfo == null)
            throw new ArgumentException($"{nameof(ConfigureAdditionalServices)} Unable to create Surface from: {useCase.Surface}");

          Log.LogInformation(
            $"{nameof(ConfigureAdditionalServices)} SurfaceInfo: MinElevation {surfaceInfo.MinElevation} MaxElevation {surfaceInfo.MaxElevation} " +
            $"PointCount: {surfaceInfo.Points.Count} TriangleCount: {surfaceInfo.Triangles.Count} BoundaryPointCount: {surfaceInfo.Boundary.Count}");

          GenerateWithSketchup(surfaceInfo, useCase, sourcePathAndFilename);

          GenerateWithoutSketchup(surfaceInfo, useCase, sourcePathAndFilename);

        }
      }
      catch (Exception e)
      {
        Log.LogError(e, "Surface import failed");
        throw e;
      }
#endif

    }

#if NET_4_7

    private bool GenerateWithSketchup(ISurfaceInfo surfaceInfo, TestCase useCase, string sourcePathAndFilename, int levelCount = 10, double resolution = 5)
    {
      Log.LogInformation($"{nameof(GenerateWithSketchup)} Generating with sketchup");
      var targetSketchupFilenameAndPath =
        Path.ChangeExtension(
          Path.Combine(Path.GetDirectoryName(Path.GetFullPath(sourcePathAndFilename)),
            Path.GetFileNameWithoutExtension(sourcePathAndFilename)), "skp");
      
      using (var skuModel = new SkuModel(useCase.IsMetric))
      {
        skuModel.Name = Path.GetFileNameWithoutExtension(targetSketchupFilenameAndPath);
        var pondMapVizTool = new PondMap {Levels = levelCount, Resolution = resolution};
        // this writes the png file
        pondMapVizTool.GenerateAndSaveTexture(surfaceInfo, targetSketchupFilenameAndPath, "original");
      }

      var targetPondingFilenameAndPath =
        Path.ChangeExtension(
          Path.Combine(Path.GetDirectoryName(Path.GetFullPath(targetSketchupFilenameAndPath)),
                       $"{Path.GetFileNameWithoutExtension(targetSketchupFilenameAndPath)}-original-pondmap"
                       ),
 "png");

      if (!File.Exists(targetPondingFilenameAndPath))
        throw new FileNotFoundException($"{nameof(GenerateWithSketchup)} Ponding map not found {targetPondingFilenameAndPath}");

      Log.LogInformation($"{nameof(GenerateWithSketchup)} targetPondingFile: {targetPondingFilenameAndPath}");
      return true;
    }
    private bool GenerateWithoutSketchup(ISurfaceInfo surfaceInfo, TestCase useCase, string sourcePathAndFilename, int levelCount = 10, double resolution = 5)
    {
      Log.LogInformation($"{nameof(GenerateWithoutSketchup)} Generating without sketchup");

      string targetPondingFilenameAndPath = Path.Combine(Path.GetDirectoryName(sourcePathAndFilename),
        string.Format($"{Path.GetFileNameWithoutExtension(useCase.Surface)}-{"originalGround"}-pondmap.png"));

      var pondMap = surfaceInfo.GeneratePondMap(resolution, levelCount, null, null);
      if (pondMap == null)
        throw new ArgumentException($"{nameof(GenerateWithoutSketchup)} Unable to create pond map: resolution: {useCase.Resolution} levelCount: {levelCount}");
      
      SaveBitmap(pondMap, targetPondingFilenameAndPath);

      if (!File.Exists(targetPondingFilenameAndPath))
        throw new FileNotFoundException($"{nameof(GenerateWithoutSketchup)} Ponding map not found {targetPondingFilenameAndPath}");

      Log.LogInformation($"{nameof(GenerateWithoutSketchup)} targetPondingFile: {targetPondingFilenameAndPath}");
      return true;
    }

    private void SaveBitmap(BitmapSource pondMap, string targetFilenameAndPath)
    {
      var pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(pondMap));
      using (Stream stream = (Stream) File.Create(targetFilenameAndPath))
        pngBitmapEncoder.Save(stream);
    }

    private static void CreateWpfApp()
    {
      new Application() { ShutdownMode = ShutdownMode.OnExplicitShutdown }.Run();
    }

#endif

    /// <inheritdoc />
    protected override void ConfigureAdditionalAppSettings(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory factory)
    {
      app.UseResponseCompression();
      app.UseMvc();
    }
  }
}
