using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.Designs.Servers.Client;
using System.Threading;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Services.Designs;
using VSS.TRex.Storage.Models;
using System.Threading.Tasks;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.DesignElevation
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Build()
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().ImmutableGridStorage())))
        .Add(x => x.AddTransient<ISiteModel>(factory => new SiteModel()))
        .Add(x => x.AddSingleton<IExistenceMaps>(new ExistenceMaps.ExistenceMaps()))
        .Add(x => x.AddSingleton(new CalculateDesignElevationsServer()))
        .Add(x => x.AddSingleton<IDesignsService>(new DesignsService(StorageMutability.Immutable)))
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
        typeof(VSS.TRex.Cells.CellEvents),
        typeof(VSS.TRex.Designs.DesignBase),
        typeof(VSS.TRex.Designs.TTM.HashOrdinate),
        typeof(VSS.TRex.Designs.TTM.Optimised.HeaderConsts),
        typeof(VSS.TRex.ExistenceMaps.ExistenceMaps),
        typeof(VSS.TRex.GridFabric.BaseIgniteClass),
        typeof(VSS.TRex.SubGridTrees.Client.ClientCMVLeafSubGrid),
        typeof(VSS.TRex.SubGridTrees.Core.Utilities.SubGridUtilities),
      };

      foreach (var asmType in AssemblyDependencies)
        if (asmType.Assembly == null)
          Console.WriteLine($"Assembly for type {asmType} has not been loaded.");
    }
    static async Task<int> Main(string[] args)
    {
      EnsureAssemblyDependenciesAreLoaded();

      DependencyInjection();

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
