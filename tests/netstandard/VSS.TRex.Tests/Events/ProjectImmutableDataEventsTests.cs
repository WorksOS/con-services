using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Castle.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Events
{
  public class ProjectImmutableDataEventsTests : IClassFixture<DILoggingFixture>
  {
    private static object Lock = new object();
    private Mock<IStorageProxy> moqStorageProxy;

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_AllTypesNoDuplicates()
    {
      SetupDI();

      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.True(2 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");

      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), new GPSAccuracyAndTolerance(GPSAccuracy.Medium, 1));
      Assert.True(3 == events.GPSAccuracyAndToleranceStateEvents.Count(), $"List contains {events.GPSAccuracyAndToleranceStateEvents.Count()} GPSAccuracy events, instead of 3");

      events.SaveMachineEventsToPersistentStore(moqStorageProxy.Object);
    }

    private void SetupDI()
    {
      Guid siteModelGuid = Guid.NewGuid();

      lock (Lock)
      {
        moqStorageProxy = new Mock<IStorageProxy>();
        moqStorageProxy.Setup(s => s.WriteStreamToPersistentStore(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<FileSystemStreamType>(), It.IsAny<MemoryStream>(), It.IsAny<MemoryStream>()));
      }

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
        .Add(x => x.AddSingleton<IStorageProxy>(moqStorageProxy.Object))
        .Add(x => x.AddSingleton<ISurveyedSurfaces>(moqSurveyedSurfaces.Object))
        .Add(x => x.AddSingleton<IProductionEventsFactory>(new ProductionEventsFactory()))
        .Build();

      ISiteModel mockedSiteModel = new SiteModel(siteModelGuid);

      var moqSiteModelFactory = new Mock<ISiteModelFactory>();
      moqSiteModelFactory.Setup(mk => mk.NewSiteModel()).Returns(mockedSiteModel);
      moqSiteModelFactory.Setup(mk => mk.NewSiteModel(siteModelGuid)).Returns(mockedSiteModel);

      moqSiteModels.Setup(mk => mk.GetSiteModel(siteModelGuid)).Returns(mockedSiteModel);

      // Mock the new sitemodel creation API to return just a new sitemodel
      moqSiteModels.Setup(mk => mk.GetSiteModel(moqStorageProxy.Object, siteModelGuid, true)).Returns(mockedSiteModel);

      //Moq doesn't support extention methods in IConfiguration/Root.
      var moqConfiguration = new Mock<IConfigurationStore>();
      DIBuilder
        .Continue()
        .Add(x => x.AddSingleton<ISiteModelFactory>(new SiteModelFactory()))
        .Add(x => x.AddSingleton<IConfigurationStore>(moqConfiguration.Object))
        .Complete();
    }
  }
}
