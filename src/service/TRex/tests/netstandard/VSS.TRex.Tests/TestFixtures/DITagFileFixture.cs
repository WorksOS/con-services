using System;
using System.IO;
using Apache.Ignite.Core;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.Alignments;
using VSS.TRex.Alignments.Interfaces;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Server.Utilities;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Executors;
using Xunit;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITagFileFixture : IDisposable
  {
    public static Guid NewSiteModelGuid => Guid.NewGuid();

    public static TAGFileConverter ReadTAGFile(string fileName)
    {
      var converter = new TAGFileConverter();

      Assert.True(converter.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", fileName), FileMode.Open, FileAccess.Read)),
        "Converter execute returned false");

      return converter;
    }

    public static TAGFileConverter ReadTAGFile(string subFolder, string fileName)
    {
      var converter = new TAGFileConverter();

      var fn = Path.Combine("TestData", "TAGFiles", subFolder, fileName);
      Assert.True(converter.Execute(new FileStream(fn, FileMode.Open, FileAccess.Read)), "Converter execute returned false");

      return converter;
    }

    public static TAGFileConverter ReadTAGFileFullPath(string fileName)
    {
      var converter = new TAGFileConverter();

      Assert.True(converter.Execute(new FileStream(fileName, FileMode.Open, FileAccess.Read)),
        "Converter execute returned false");

      return converter;
    }

    private static void AddProxyCacheFactoriesToDI()
    {
      DIBuilder
        .Continue()

        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex
        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]>>>
          (factory => (ignite, mutability) => null))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>
          (factory => (ignite, mutability) => null))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>
          (factory => (ignite, mutability) => new StorageProxyCacheTransacted_TestHarness<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>
          (factory => (ignite, mutability) => new StorageProxyCacheTransacted_TestHarness<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.NonSpatialCacheName(mutability)))));
    }

    public DITagFileFixture()
    {
      var mockSiteModelMetadataManager = new Mock<ISiteModelMetadataManager>();
      var mockSiteModelAttributesChangedEventSender = new Mock<ISiteModelAttributesChangedEventSender>();

      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())

        .Add(x => AddProxyCacheFactoriesToDI())

        .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage())))
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))

        .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))

        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
        .Add(x => x.AddSingleton<ISiteModelMetadataManager>(mockSiteModelMetadataManager.Object))

        .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
        .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))
        .Add(x => x.AddSingleton<IAlignmentManager>(factory => new AlignmentManager()))

        .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(mockSiteModelAttributesChangedEventSender.Object))

        // Register the hook used to capture cell pass mutation events while processing TAG files.
        .Add(x => x.AddSingleton<ICell_NonStatic_MutationHook>(new Cell_NonStatic_MutationHook()))

        .Complete();
    }

    public void Dispose()
    {
      DIBuilder.Eject();
    }
  }
}
