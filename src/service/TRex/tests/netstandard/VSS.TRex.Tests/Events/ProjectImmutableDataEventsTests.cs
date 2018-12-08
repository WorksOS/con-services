using System;
using System.IO;
using System.Text;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;
using VSS.TRex.Storage.Models;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.Events
{
  public class ProjectImmutableDataEventsTests : IClassFixture<DITagFileFixture>
  {
    [Fact]
    public void Test_ProjectImmutableSiteModelTest()
    {
      var sourceSiteModel = new SiteModel(Guid.NewGuid(), false);
      var result = sourceSiteModel.SaveToPersistentStoreForTAGFileIngest(DIContext.Obtain<IStorageProxy>());  
      Assert.True(result, "unable to save SiteModel to Persistent store");

      var targetSiteModel = new SiteModel(sourceSiteModel.ID, false);
      var fileStatus = targetSiteModel.LoadFromPersistentStore();
      Assert.True(FileSystemErrorStatus.OK == fileStatus, "unable to load SiteModel from Persistent store");
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_AllTypesNoDuplicates()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      DateTime referenceDate = DateTime.UtcNow;
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-30), 1);
      Assert.True(2 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");

      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(referenceDate.AddMinutes(-60), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(referenceDate.AddMinutes(-58), new GPSAccuracyAndTolerance(GPSAccuracy.Medium, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(referenceDate.AddMinutes(-56), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 1));
      Assert.True(3 == events.GPSAccuracyAndToleranceStateEvents.Count(), $"List contains {events.GPSAccuracyAndToleranceStateEvents.Count()} GPSAccuracy events, instead of 3");

      var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
      storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));
      events.SaveMachineEventsToPersistentStore(storageProxy);
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_NoDuplicates()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      DateTime referenceDate = DateTime.UtcNow;
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-30), 1);
      Assert.True(2 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");
      
      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(2, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out DateTime dateTime, out int state);
      Assert.Equal(2, targetEventList.Count());
      var evt = ((ProductionEvents<int>)targetEventList).Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.MachineDesignNameIDStateEvents.GetImmutableStream();
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(2, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      evt = ((ProductionEvents<int>)targetEventList).Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);
    }
    
    [Fact]
    public void Test_ProjectImmutableDataEventsTests_Duplicates()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      DateTime referenceDate = DateTime.UtcNow;
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-30), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 2);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 3);
      Assert.True(5 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 5");

      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(5, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out DateTime dateTime, out int state);
      var evt = ((ProductionEvents<int>)targetEventList).Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.MachineDesignNameIDStateEvents.GetImmutableStream();
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(4, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_SaveAndLoad()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      DateTime referenceDate = DateTime.UtcNow;
      
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-30), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 2);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(referenceDate.AddMinutes(-29), 3);
      Assert.True(5 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 5");

      var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
      storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));
      events.SaveMachineEventsToPersistentStore(storageProxy);
      var resultantEvents = events.GetEventList(ProductionEventType.DesignChange);
      Assert.Equal(5, resultantEvents.Count());

      resultantEvents.LoadFromStore(storageProxy.ImmutableProxy);
      Assert.Equal(4, resultantEvents.Count());

      resultantEvents.LoadFromStore(storageProxy);
      Assert.Equal(5, resultantEvents.Count());
    }
    
    private IProductionEvents Deserialize(MemoryStream stream)
    {
      IProductionEvents events;
      using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
      {
        if (stream.Length < 16)
        {
          return null;
        }
        stream.Position = 8;

        var eventType = reader.ReadInt32();
        if (!Enum.IsDefined(typeof(ProductionEventType), eventType))
        {
          return null;
        }

        events = DIContext.Obtain<IProductionEventsFactory>().NewEventList(-1, Guid.Empty, (ProductionEventType)eventType);

        stream.Position = 0;
        events.ReadEvents(reader);
      }

      return events;
    }
  }
}
