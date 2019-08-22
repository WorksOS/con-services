using System;
using System.IO;
using System.Text;
using VSS.TRex.DI;
using VSS.TRex.Events;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Events.Models;
using VSS.TRex.Machines.Interfaces;
using VSS.TRex.SiteModels;
using VSS.TRex.Storage;
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
      var result = sourceSiteModel.SaveToPersistentStoreForTAGFileIngest(sourceSiteModel.PrimaryStorageProxy);  
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
      Assert.True(3 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 3");

      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(3, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out DateTime dateTime, out int state);
      var evt = ((ProductionEvents<int>)targetEventList).Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.MachineDesignNameIDStateEvents.GetImmutableStream();
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(2, targetEventList.Count());

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
      Assert.True(3 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 3");

      var storageProxy = new StorageProxy_Ignite_Transactional(StorageMutability.Mutable);
      storageProxy.SetImmutableStorageProxy(new StorageProxy_Ignite_Transactional(StorageMutability.Immutable));
      events.SaveMachineEventsToPersistentStore(storageProxy);
      var resultantEvents = events.GetEventList(ProductionEventType.DesignChange);
      Assert.Equal(3, resultantEvents.Count());

      resultantEvents.LoadFromStore(storageProxy.ImmutableProxy);
      Assert.Equal(2, resultantEvents.Count());

      resultantEvents.LoadFromStore(storageProxy);
      Assert.Equal(3, resultantEvents.Count());
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_OverrideEvents()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      var referenceDate = DateTime.UtcNow;

      events.DesignOverrideEvents.PutValueAtDate(referenceDate.AddMinutes(-60), new OverrideEvent<int>(referenceDate.AddMinutes(-50), 0));
      events.DesignOverrideEvents.PutValueAtDate(referenceDate.AddMinutes(-30), new OverrideEvent<int>(referenceDate.AddMinutes(-15), 1));
      Assert.True(2 == events.DesignOverrideEvents.Count(), $"List contains {events.DesignOverrideEvents.Count()} DesignOverride events, instead of 2");

      var mutableStream = events.DesignOverrideEvents.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(2, targetEventList.Count());
      var evt = ((ProductionEvents<OverrideEvent<int>>)targetEventList).Events[0];

      events.DesignOverrideEvents.GetStateAtIndex(0, out var dateTime, out var state);
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.DesignOverrideEvents.GetImmutableStream();
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(2, targetEventList.Count());

      events.DesignOverrideEvents.GetStateAtIndex(0, out dateTime, out state);
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);
     }

    [Theory]
    [InlineData(-105, -75)]
    [InlineData(-75, -45)]
    [InlineData(-45, -15)]
    [InlineData(-105, -15)]
    [InlineData(-80, -70)]
    public void Test_ProjectImmutableDataEventsTests_MergingDesignOverrides(int startMins, int endMins)
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      var ids = new[] { 1, 2, 3 };
      var overrideId = 4;

      var referenceDate = DateTime.UtcNow;
      var dateTimes = new[] { referenceDate.AddMinutes(-90), referenceDate.AddMinutes(-60), referenceDate.AddMinutes(-30) };
      for (var i = 0; i < 3; i++)
        events.MachineDesignNameIDStateEvents.PutValueAtDate(dateTimes[i], ids[i]);

      var overrideStartDate = referenceDate.AddMinutes(startMins);
      var overrideEndDate = referenceDate.AddMinutes(endMins);
      events.DesignOverrideEvents.PutValueAtDate(overrideStartDate, new OverrideEvent<int>(overrideEndDate, overrideId));

      //Need to do the adding of events prior to CheckMergedList since getting events.MachineDesignNameIDStateEvents does the merge
      CheckMergedList(startMins, endMins, events.MachineDesignNameIDStateEvents, overrideId, ids, dateTimes, overrideStartDate, overrideEndDate);
    }

    [Theory]
    [InlineData(-105, -75)]
    [InlineData(-75, -45)]
    [InlineData(-45, -15)]
    [InlineData(-105, -15)]
    [InlineData(-80, -70)]
    public void Test_ProjectImmutableDataEventsTests_MergingLayerOverrides(int startMins, int endMins)
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);

      var ids = new[] { (ushort)1, (ushort)2, (ushort)3 };
      var overrideId = (ushort)4;

      var referenceDate = DateTime.UtcNow;
      var dateTimes = new[] { referenceDate.AddMinutes(-90), referenceDate.AddMinutes(-60), referenceDate.AddMinutes(-30) };
      for (var i = 0; i < 3; i++)
        events.LayerIDStateEvents.PutValueAtDate(dateTimes[i], ids[i]);

      var overrideStartDate = referenceDate.AddMinutes(startMins);
      var overrideEndDate = referenceDate.AddMinutes(endMins);
      events.LayerOverrideEvents.PutValueAtDate(overrideStartDate, new OverrideEvent<ushort>(overrideEndDate, overrideId));

      //Need to do the adding of events prior to CheckMergedList since getting events.LayerIDStateEvents does the merge
      CheckMergedList(startMins, endMins, events.LayerIDStateEvents, overrideId, ids, dateTimes, overrideStartDate, overrideEndDate);
    }

    private void CheckMergedList<T>(int startMins, int endMins, IProductionEvents<T> eventsList, T overrideId, T[] ids, DateTime[] dateTimes, DateTime startOverride, DateTime endOverride)
    {
      DateTime[] expectedDates;
      T[] expectedIds;
      if (startMins == -105 && endMins == -75) //override spanning start
      {
        expectedDates = new[] { startOverride, endOverride, dateTimes[1], dateTimes[2] };
        expectedIds = new[] { overrideId, ids[0], ids[1], ids[2] };
      }
      else if (startMins == -75 && endMins == -45) //override the middle
      {
        expectedDates = new[] { dateTimes[0], startOverride, endOverride, dateTimes[2] };
        expectedIds = new[] { ids[0], overrideId, ids[1], ids[2] };
      }
      else if (startMins == -45 && endMins == -15) //override spanning end
      {
        expectedDates = new[] { dateTimes[0], dateTimes[1], startOverride, endOverride };
        expectedIds = new[] { ids[0], ids[1], overrideId, ids[2] };
      }
      else if (startMins == -105 && endMins == -15)  //override whole range
      {
        expectedDates = new[] { startOverride, endOverride };
        expectedIds = new[] { overrideId, ids[2] };
      }
      else //startMins == -80 && endMins == -70 //override within machine events
      {
        expectedDates = new[] { dateTimes[0], startOverride, endOverride, dateTimes[1], dateTimes[2] };
        expectedIds = new[] { ids[0], overrideId, ids[0], ids[1], ids[2] };
      }

      var mutableStream = eventsList.GetMutableStream();
      var targetEventList = Deserialize(mutableStream);
      Assert.Equal(expectedDates.Length, targetEventList.Count());

      var eventList = ((ProductionEvents<T>)targetEventList).Events;
      for (var i = 0; i < eventList.Count; i++)
      {
        Assert.Equal(expectedDates[i], eventList[i].Date);
        Assert.Equal(expectedIds[i], eventList[i].State);
      }

      var immutableStream = eventsList.GetImmutableStream();
      targetEventList = Deserialize(immutableStream);
      Assert.Equal(expectedDates.Length, targetEventList.Count());

      eventList = ((ProductionEvents<T>)targetEventList).Events;
      for (var i = 0; i < eventList.Count; i++)
      {
        Assert.Equal(expectedDates[i], eventList[i].Date);
        Assert.Equal(expectedIds[i], eventList[i].State);
      }
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
        stream.Position = 1;

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
