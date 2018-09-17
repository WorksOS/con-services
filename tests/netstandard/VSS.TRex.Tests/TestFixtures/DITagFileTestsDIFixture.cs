using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.Tests.TestFixtures
{
  public static class LocalState
  {
    public static Guid NewSiteModelGuid = Guid.NewGuid();
  }

  public class DITagFileTestsDIFixture : IDisposable
  {
    private static object Lock = new object();

    public static Guid NewSiteModelGuid = Guid.NewGuid();

    public DITagFileTestsDIFixture()
    {
      lock (Lock)
      {
        var moqStorageProxy = new Mock<IStorageProxy>();

        var moqStorageProxyFactory = new Mock<IStorageProxyFactory>();
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Immutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.Storage(StorageMutability.Mutable)).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.MutableGridStorage()).Returns(moqStorageProxy.Object);
        moqStorageProxyFactory.Setup(mk => mk.ImmutableGridStorage()).Returns(moqStorageProxy.Object);

        var moqSurveyedSurfaces = new Mock<ISurveyedSurfaces>();

        var moqSiteModels = new Mock<ISiteModels>();
        moqSiteModels.Setup(mk => mk.StorageProxy).Returns(moqStorageProxy.Object);

        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IStorageProxyFactory>(moqStorageProxyFactory.Object))
          .Add(x => x.AddSingleton<ISiteModels>(moqSiteModels.Object))

          .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
          .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
          .Build();

        ISiteModel mockedSiteModel = new SiteModel(NewSiteModelGuid);

        var moqSiteModelFactory = new Mock<ISiteModelFactory>();
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel()).Returns(mockedSiteModel);
        moqSiteModelFactory.Setup(mk => mk.NewSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        moqSiteModels.Setup(mk => mk.GetSiteModel(NewSiteModelGuid)).Returns(mockedSiteModel);

        // Mock the new sitemodel creation API to return just a new sitemodel
        moqSiteModels.Setup(mk => mk.GetSiteModel(moqStorageProxy.Object, NewSiteModelGuid, true)).Returns(mockedSiteModel);

        DIBuilder
          .Continue()
          .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
          .Complete();
      }
    }

    public void Dispose() { } // Nothing needing doing 
  }

}
