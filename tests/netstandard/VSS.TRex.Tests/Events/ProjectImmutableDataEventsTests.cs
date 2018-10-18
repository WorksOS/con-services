using System;
using System.IO;
using System.Text;
using VSS.TRex.Events;
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
    public void Test_ProjectImmutableDataEventsTests_AllTypesNoDuplicates()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.True(2 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");

      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-58), new GPSAccuracyAndTolerance(GPSAccuracy.Medium, 2));
      events.GPSAccuracyAndToleranceStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-56), new GPSAccuracyAndTolerance(GPSAccuracy.Coarse, 1));
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
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      Assert.True(2 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");
      
      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();

      var targetEventList = new ProductionEvents<int>(-1, Guid.Empty, ProductionEventType.DesignChange, (w, s) => w.Write(s), r => r.ReadInt32());
      targetEventList = DeserializeEvents(mutableStream, targetEventList);
      Assert.Equal(2, targetEventList.Count());

      DateTime dateTime;
      int state;

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      var evt = targetEventList.Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.MachineDesignNameIDStateEvents.GetImmutableStream();
      targetEventList = new ProductionEvents<int>(-1, Guid.Empty, ProductionEventType.DesignChange, (w, s) => w.Write(s), r => r.ReadInt32());
      targetEventList = DeserializeEvents(immutableStream, targetEventList);
      Assert.Equal(2, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      evt = targetEventList.Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_Duplicates()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 2);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 3);
      Assert.True(5 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");

      var mutableStream = events.MachineDesignNameIDStateEvents.GetMutableStream();

      var targetEventList = new ProductionEvents<int>(-1, Guid.Empty, ProductionEventType.DesignChange, (w, s) => w.Write(s), r => r.ReadInt32());
      targetEventList = DeserializeEvents(mutableStream, targetEventList);
      Assert.Equal(5, targetEventList.Count());

      DateTime dateTime;
      int state;

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      var evt = targetEventList.Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);

      var immutableStream = events.MachineDesignNameIDStateEvents.GetImmutableStream();
      targetEventList = new ProductionEvents<int>(-1, Guid.Empty, ProductionEventType.DesignChange, (w, s) => w.Write(s), r => r.ReadInt32());
      targetEventList = DeserializeEvents(immutableStream, targetEventList);
      Assert.Equal(4, targetEventList.Count());

      events.MachineDesignNameIDStateEvents.GetStateAtIndex(0, out dateTime, out state);
      evt = targetEventList.Events[0];
      Assert.Equal(state, evt.State);
      Assert.Equal(dateTime, evt.Date);
    }

    [Fact]
    public void Test_ProjectImmutableDataEventsTests_SaveAndLoad()
    {
      var siteModel = new SiteModel(Guid.Empty, false);
      var events = new ProductionEventLists(siteModel, MachineConsts.kNullInternalSiteModelMachineIndex);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-60), 0);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-30), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 1);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 2);
      events.MachineDesignNameIDStateEvents.PutValueAtDate(DateTime.UtcNow.AddMinutes(-29), 3);
      Assert.True(5 == events.MachineDesignNameIDStateEvents.Count(), $"List contains {events.MachineDesignNameIDStateEvents.Count()} MachineDesignName events, instead of 2");

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

    private ProductionEvents<int> DeserializeEvents(MemoryStream stream, ProductionEvents<int> targetEventList)
    {
      if (stream != null)
      {
        // Practice the binary event reading...
        stream.Position = 0;
        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
          int majorVer = reader.ReadInt32();
          int minorVer = reader.ReadInt32();

          if (majorVer != 1 && minorVer != 0)
            throw new ArgumentException($"Unknown major/minor version numbers: {majorVer}/{minorVer}");

          int count = reader.ReadInt32();

          for (int i = 0; i < count; i++)
          {
            targetEventList.Events.Add(new ProductionEvents<int>.Event
            {
              Date = DateTime.FromBinary(reader.ReadInt64()),
              Flags = reader.ReadByte(),
              State = targetEventList.SerialiseStateIn(reader)
            });
          }
        }
      }

      return targetEventList;
    }

  }
}
