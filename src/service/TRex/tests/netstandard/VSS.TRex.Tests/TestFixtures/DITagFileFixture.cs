using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DITagFileFixture : IDisposable
  {
    private static object Lock = new object();

    public static Guid NewSiteModelGuid = Guid.NewGuid();

    public DITagFileFixture()
    {
      lock (Lock)
      {

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Build();


        var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
        storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(storageProxy);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(storageProxy);
        moqStorageProxyFactory.Setup(mk => mk.MutableGridStorage()).Returns(storageProxy);
        moqStorageProxyFactory.Setup(mk => mk.ImmutableGridStorage()).Returns(storageProxy);

       
        var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();

        var moqSiteModels = new Mock<ISiteModels>();
        moqSiteModels.Setup(mk => mk.StorageProxy).Returns(storageProxy);

        DIBuilder
          .Continue()
          .Add(x => x.AddSingleton<IStorageProxy>(storageProxy))
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))
          .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
          .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
          .Add(x => x.AddSingleton<IMutabilityConverter>(new MutabilityConverter()))
          .Build();

        ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuid);

        var moqSiteModelFactory = new Mock<ISiteModelFactory>();
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel()).Returns(mockedSiteModel);
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        // Mock the new sitemodel creation API to return just a new sitemodel
        moqSiteModels.Setup(mk => mk.GetSiteModel(storageProxy, NewSiteModelGuid, true)).Returns(mockedSiteModel);

        DIBuilder
          .Continue()
          .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
          .Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }
}
