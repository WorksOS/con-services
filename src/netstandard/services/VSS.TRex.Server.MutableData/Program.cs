using System;
using Microsoft.Extensions.Configuration;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.CoordinateSystems;
using VSS.TRex.CoordinateSystems.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Servers.Compute;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Classes;
using System.Threading.Tasks;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.GridFabric.Events;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces;
using VSS.ConfigurationStore;
using VSS.TRex.TAGFiles.Classes.Queues;

namespace VSS.TRex.Server.MutableData
{
  public class Program
  {
    public static IConfiguration Configuration { get; set; }

    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceFactory>(new SurveyedSurfaces.SurveyedSurfaceFactory()))
        .Build()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(x => x.AddSingleton<ITagFileAuthProjectProxy, TagFileAuthProjectProxy>())
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage())))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<ICoordinateConversion>(new CoordinateConversion()))
        .Add(x => x.AddSingleton(Configuration))
        .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps.ExistenceMaps()))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<ISegmentRetirementQueue>(factory => new SegmentRetirementQueue()))
        .Build()
        .Add(x => x.AddSingleton(new TagProcComputeServer()))
        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))

        // Register the sender for the sie model attribute change notifications
        .Add(x => x.AddSingleton< ISiteModelAttributesChangedEventSender>(new SiteModelAttributesChangedEventSender()))

        .Add(x => x.AddSingleton<ISiteModelMetadataManager>(factory => new SiteModelMetadataManager()))

       .Complete();
    }

    // This static array ensures that all required assemblies are included into the artifacts by the linker
    private static void EnsureAssemblyDependenciesAreLoaded()
    {
      // This static array ensures that all required assemblies are included into the artifacts by the linker
      Type[] AssemblyDependencies =
      {
        typeof(VSS.TRex.Geometry.BoundingIntegerExtent2D),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Common.SubGridsPipelinedReponseBase),
        typeof(VSS.TRex.Logging.Logger),
        typeof(VSS.TRex.DI.DIContext),
        typeof(VSS.TRex.Storage.StorageProxy),
        typeof(VSS.TRex.SiteModels.SiteModel),
        typeof(VSS.TRex.Cells.CellEvents),
        typeof(VSS.TRex.Compression.AttributeValueModifiers),
        typeof(VSS.TRex.CoordinateSystems.LLH),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.Events.CellPassFastEventLookerUpper),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.Machines.Machine),
        typeof(VSS.TRex.Services.SurveyedSurfaces.SurveyedSurfaceService),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
        typeof(VSS.TRex.SubGridTrees.Server.MutabilityConverter),
        typeof(VSS.TRex.SurveyedSurfaces.SurveyedSurface),
        typeof(VSS.TRex.TAGFiles.Executors.SubmitTAGFileExecutor)
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }

    static async Task<int> Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();

      // Load settings for Mutabledata
      Configuration = new ConfigurationBuilder()
          //   .SetBasePath(Directory.GetCurrentDirectory()) dont set for default running path
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddEnvironmentVariables() // this should override appsettings.json with environment variables with but does not appear to work!
          .Build();

      DependencyInjection();

      if (Configuration.GetSection("EnableTFAService").Value == null)
        Console.WriteLine("*** Warning! **** Check for missing configuration values. e.g EnableTFAService");

      Console.WriteLine("**** Configuration Settings ****");
      foreach (var env in Configuration.GetChildren())
      {
        Console.WriteLine($"{env.Key}:{env.Value}");
      }

      var cancelTokenSource = new CancellationTokenSource();
      AppDomain.CurrentDomain.ProcessExit += (s, e) =>
      {
        Console.WriteLine("Exiting");
        cancelTokenSource.Cancel();
      };
      await Task.Delay(-1, cancelTokenSource.Token);
      return 0;
    }
  }
}
