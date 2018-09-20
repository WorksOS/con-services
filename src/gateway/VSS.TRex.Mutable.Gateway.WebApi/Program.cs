using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Log4Net.Extensions;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.TRex.Servers.Client;
using VSS.WebApi.Common;

namespace VSS.TRex.Mutable.Gateway.WebApi
{
  public class Program
  {
    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        //typeof(VSS.TRex.Geometry.BoundingIntegerExtent2D),
        //typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        //typeof(VSS.TRex.Common.SubGridsPipelinedReponseBase),
        //typeof(VSS.TRex.Logging.Logger),
        //typeof(VSS.TRex.DI.DIContext),
        //typeof(VSS.TRex.Storage.StorageProxy),
        //typeof(VSS.TRex.SiteModels.SiteModel),
        //typeof(VSS.TRex.Cells.CellEvents),
        //typeof(VSS.TRex.Compression.AttributeValueModifiers),
        //typeof(VSS.TRex.CoordinateSystems.LLH),
        //typeof(VSS.TRex.Designs.DesignBase),
        //typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        //typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        //typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        //typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        //typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        //typeof(VSS.TRex.Machines.Machine),
        //typeof(VSS.TRex.Services.SurveyedSurfaces.SurveyedSurfaceService),
        //typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        //typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        //typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        //typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface)
        typeof(VSS.TRex.TAGFiles.Executors.SubmitTAGFileExecutor),
        typeof(VSS.TRex.TAGFiles.GridFabric.NodeFilters.TAGProcessorRoleBasedNodeFilter)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    public static void Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();

      var webHost = BuildWebHost(args);

      webHost.Run();
    }

    public static IWebHost BuildWebHost(string[] args)
    {
      var kestrelConfig = new ConfigurationBuilder()
        .AddJsonFile("kestrelsettings.json", true, false).Build();

      AutoMapperUtility.AutomapperConfiguration.AssertConfigurationIsValid();

      return WebHost.CreateDefaultBuilder(args)
        .ConfigureLogging(builder =>
        {
          Log4NetProvider.RepoName = Startup.LOGGER_REPO_NAME;
          builder.Services.AddSingleton<ILoggerProvider, Log4NetProvider>();
          builder.SetMinimumLevel(LogLevel.Trace);
          builder.AddConfiguration(kestrelConfig);
        })
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          var env = hostingContext.HostingEnvironment;
          env.ConfigureLog4Net(repoName: Startup.LOGGER_REPO_NAME, configFileRelativePath: "log4net.xml");

        })
        .UsePrometheus()
        .UseStartup<Startup>()
        .Build();
    }
  }
}
