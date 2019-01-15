using System;
using System.IO;
using Apache.Ignite.Core;
using Apache.Ignite.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.Designs;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SiteModels.Interfaces.Events;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Caches;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.TAGFiles.Executors;
using Xunit;

namespace VSS.TRex.Tests.TestFixtures
{
  public interface IStorageProxyCacheTransacted_TestHarness<TK, TV> : IStorageProxyCacheTransacted<TK, TV>
  {
  }

  public class StorageProxyCacheTransacted_TestHarness<TK, TV> : StorageProxyCacheTransacted<TK, TV>, IStorageProxyCacheTransacted_TestHarness<TK, TV>
  {
    public StorageProxyCacheTransacted_TestHarness(ICache<TK, TV> cache) : base(cache)
    {
    }

    /// <summary>
    /// Override the commit behaviour to make it a null operation for unit test activities
    /// </summary>
    public override void Commit(out int numDeleted, out int numUpdated, out long numBytesWritten)
    {
      // Do nothing on purpose
      numDeleted = 0;
      numUpdated = 0;
      numBytesWritten = 0;
    }
  }

  public class MockSiteModelAttributesChangedEventSender : ISiteModelAttributesChangedEventSender
  {
    /// <summary>
    /// Notify all interested nodes in the immutable grid a site model has changed attributes
    /// </summary>
    /// <param name="siteModelID"></param>
    void ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrid, Guid siteModelID,
      bool existenceMapChanged = false, ISubGridTreeBitMask existenceMapChangeMask = null,
      bool designsChanged = false, bool surveyedSurfacesChanged = false, bool CsibChanged = false,
      bool machinesChanged = false, bool machineTargetValuesChanged = false, bool machineDesignsModified = false, bool proofingRunsModified = false)
    {
      // Do nothing in the Mock
    }

    void ISiteModelAttributesChangedEventSender.ModelAttributesChanged(SiteModelNotificationEventGridMutability targetGrid, Guid siteModelID, bool existenceMapChanged, ISubGridTreeBitMask existenceMapChangeMask, bool designsChanged, bool surveyedSurfacesChanged, bool CsibChanged, bool machinesChanged, bool machineTargetValuesChanged, bool machineDesignsModified, bool proofingRunsModified)
    {
      // Do nothing in the Mock
    }
  }

  public class DITagFileFixture : IDisposable
  {
    private static readonly object Lock = new object();

    public static Guid NewSiteModelGuid => Guid.NewGuid();

    public static TAGFileConverter ReadTAGFile(string fileName)
    {
      var converter = new TAGFileConverter();

      Assert.True(converter.Execute(new FileStream(Path.Combine("TestData", "TAGFiles", fileName), FileMode.Open, FileAccess.Read)),
        "Converter execute returned false");

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

        // Add the factories for the storage proxy caches, both standard and transacted, for spatial and non spatial caches in TRex
        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (IIgnite ignite, StorageMutability mutability) => null))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCache<INonSpatialAffinityKey, byte[]>>>
        (factory => (IIgnite ignite, StorageMutability mutability) => null))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<ISubGridSpatialAffinityKey, byte[]>>>
        (factory => (IIgnite ignite, StorageMutability mutability) =>
          new StorageProxyCacheTransacted_TestHarness<ISubGridSpatialAffinityKey, byte[]>(ignite?.GetCache<ISubGridSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability)))))

        .Add(x => x.AddSingleton<Func<IIgnite, StorageMutability, IStorageProxyCacheTransacted<INonSpatialAffinityKey, byte[]>>>
        (factory => (IIgnite ignite, StorageMutability mutability) =>
          new StorageProxyCacheTransacted_TestHarness<INonSpatialAffinityKey, byte[]>(ignite?.GetCache<INonSpatialAffinityKey, byte[]>(TRexCaches.SpatialCacheName(mutability)))));
    }

    public DITagFileFixture()
    {
      lock (Lock)
      {

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Build();

//        var mutableStorageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
//        var immutableStorageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Immutable);
//        mutableStorageProxy.SetImmutableStorageProxy(immutableStorageProxy);

//        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
//        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(mutableStorageProxy);
//        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(immutableStorageProxy);
//        moqStorageProxyFactory.Setup(mk => mk.MutableGridStorage()).Returns(mutableStorageProxy);
//        moqStorageProxyFactory.Setup(mk => mk.ImmutableGridStorage()).Returns(immutableStorageProxy);

       
        //var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();

        //        var moqSiteModels = new Mock<ISiteModels>();
        //        moqSiteModels.Setup(mk => mk.StorageProxy).Returns(mutableStorageProxy);

        var mockSiteModelMetadataManager = new Mock<ISiteModelMetadataManager>();
        //mockSiteModelMetadataManager.Setup(x => x.Add(It.IsAny<Guid>(), It.IsAny<ISiteModelMetadata>()));

        DIBuilder
          .Continue()

          //.Add(x => x.AddSingleton<IStorageProxy>(mutableStorageProxy))
          //.Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))

          .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
          .Add(x => AddProxyCacheFactoriesToDI())

          .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))

          .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels(() => DIContext.Obtain<IStorageProxyFactory>().MutableGridStorage())))

          //.Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
          .Add(x => x.AddTransient<ISurveyedSurfaces>(factory => new SurveyedSurfaces.SurveyedSurfaces()))

          .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
          .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
          .Add(x => x.AddSingleton<ISiteModelMetadataManager>(mockSiteModelMetadataManager.Object))

          .Add(x => x.AddSingleton<IDesignManager>(factory => new DesignManager()))
          .Add(x => x.AddSingleton<ISurveyedSurfaceManager>(factory => new SurveyedSurfaceManager()))

          .Add(x => x.AddSingleton<ISiteModelAttributesChangedEventSender>(new MockSiteModelAttributesChangedEventSender()))

          .Build();

    //    ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuid);

     //   var moqSiteModelFactory = new Mock<ISiteModelFactory>();
     //   moqSiteModelFactory.Setup(mk => mk.NewSiteModel()).Returns(mockedSiteModel);
    //    moqSiteModelFactory.Setup(mk => mk.NewSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

     //   moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        // Mock the new site model creation API to return just a new site model
        //moqSiteModels.Setup(mk => mk.GetSiteModel(storageProxy, NewSiteModelGuid, true)).Returns(mockedSiteModel);

        DIBuilder
          .Continue()
          .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
          .Complete();
      }
    }

    public void Dispose()
    {
      DIBuilder.Continue().Eject();
    } 
  }
}
