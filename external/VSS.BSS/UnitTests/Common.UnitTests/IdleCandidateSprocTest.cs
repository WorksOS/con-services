using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.ServicesAPI;

namespace UnitTests
{
  [TestClass]
  public class IdleCandidateSprocTest : ServerAPITestBase
  {

    [TestMethod]
    public void ExcessiveIdleCandidateSprocTest()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);

        string gpsDeviceID = "110661";
        Asset testAsset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.CrossCheck, DateTime.UtcNow);
        Assert.IsNotNull(testAsset, "Failed to create Test Asset");
        DateTime nowUTC = DateTime.UtcNow;
        CreateAssetSubscription(session, testAsset.ID);
        PopulateDimTables();

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-60), true, 12.3, 39.8980712890625, -105.11262512207, 32.5);

        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          ExcessiveIdleCandidate idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                                                  where eid.DimAsset.ID == testAsset.ID && eid.InProgress == true
                                                  select eid).FirstOrDefault<ExcessiveIdleCandidate>();
          Assert.IsNotNull(idleCandidate, string.Format("There should be an excessive Idle Candidate for Asset {0}", testAsset.Name));
          Assert.IsTrue(TimeSpan.FromMinutes(60).TotalSeconds <= idleCandidate.IdlePeriodSeconds && TimeSpan.FromMinutes(61).TotalSeconds >= idleCandidate.IdlePeriodSeconds, "Incorrect Number of Seconds for Idle Period Seconds");
        }

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-65), false, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        long idleID = -1;
        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          ExcessiveIdleCandidate idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                                                  where eid.DimAsset.ID == testAsset.ID && eid.InProgress == true
                                                  select eid).FirstOrDefault<ExcessiveIdleCandidate>();
          Assert.IsNotNull(idleCandidate, string.Format("There should be an excessive Idle Candidate and it should have not been set to true for Asset {0}", testAsset.Name));
          Assert.IsTrue(TimeSpan.FromMinutes(60).TotalSeconds <= idleCandidate.IdlePeriodSeconds && TimeSpan.FromMinutes(61).TotalSeconds >= idleCandidate.IdlePeriodSeconds, "Incorrect Number of Seconds for Idle Period Seconds");
          idleID = idleCandidate.ID;
        }

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-45), true, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();


        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          ExcessiveIdleCandidate idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                                                  where eid.DimAsset.ID == testAsset.ID
                                                    && eid.ID == idleID
                                                  select eid).FirstOrDefault<ExcessiveIdleCandidate>();
          Assert.IsNotNull(idleCandidate, string.Format("There should be an excessive Idle Candidate for Asset {0}", testAsset.Name));
          Assert.IsTrue(idleCandidate.InProgress, "Inprogress should still be true");
        }

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-40), false, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          ExcessiveIdleCandidate idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                                                  where eid.DimAsset.ID == testAsset.ID && idleID == eid.ID
                                                  select eid).FirstOrDefault<ExcessiveIdleCandidate>();
          Assert.IsNotNull(idleCandidate, string.Format("There should be an excessive Idle Candidate for Asset {0}", testAsset.Name));
          Assert.IsFalse(idleCandidate.InProgress, "Inprogress should be false once Ign-off received.");
        }

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-20), true, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          ExcessiveIdleCandidate idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                                                  where eid.DimAsset.ID == testAsset.ID
                                                    && idleID != eid.ID && eid.InProgress
                                                  select eid).FirstOrDefault<ExcessiveIdleCandidate>();
          Assert.IsNotNull(idleCandidate, string.Format("There should be a new excessive Idle Candidate for Asset {0}", testAsset.Name));
          Assert.IsTrue(TimeSpan.FromMinutes(20).TotalSeconds <= idleCandidate.IdlePeriodSeconds && TimeSpan.FromMinutes(21).TotalSeconds >= idleCandidate.IdlePeriodSeconds, "Incorrect Number of Seconds for Idle Period Seconds");
        }


        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-10), false, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-8), true, 12.3, 39.8980712890625, -105.11262512207, 32.5);
        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-5), false, 12.3, 39.8980712890625, -105.11262512207, 32.5);

        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          var idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                               where eid.DimAsset.ID == testAsset.ID
                               select eid);
          Assert.IsNotNull(idleCandidate, string.Format("There should be a excessive Idle Candidates for Asset {0}", testAsset.Name));
          Assert.AreEqual(3, idleCandidate.Count(), "There should be three IdleCandidates in the table");
          foreach (ExcessiveIdleCandidate i in idleCandidate)
          {
            Assert.IsFalse(i.InProgress, "Inprogress fields should be false for all Idle Candidates");
          }
        }
      }
    }

    [TestMethod]
    public void ExcessiveIdleCandidateSprocWithMotionTest()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        string gpsDeviceID = "110661";
        Asset testAsset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.CrossCheck, DateTime.UtcNow);
        API.Equipment.UpdateWorkingDefinition(session, testAsset.ID, WorkingDefinitionEnum.Movement, 0, true);
        Assert.IsNotNull(testAsset, "Failed to create Test Asset");
        DateTime nowUTC = DateTime.UtcNow;
        CreateAssetSubscription(session, testAsset.ID);
        PopulateDimTables();

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-40), true, 12.3, 39.8980712890625, -105.11262512207, 32.5);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-39), true, 32.5, 39.8980712890625, -105.11262512207, 56.7);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-20), false, 32.5, 39.8980712890625, -105.11262512207, 56.7);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-5), true, 32.5, 39.8980712890625, -105.11262512207, 56.7);

        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          var idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                               where eid.DimAsset.ID == testAsset.ID
                               select eid);
          Assert.IsNotNull(idleCandidate, string.Format("There should be a excessive Idle Candidates for Asset {0}", testAsset.Name));
          Assert.AreEqual(2, idleCandidate.Count(), "There should be two IdleCandidates in the table");
          foreach (ExcessiveIdleCandidate i in idleCandidate)
          {
            Assert.IsFalse(i.InProgress, "Inprogress fields should be false for all Idle Candidates");
          }
        }
      }
    }

    [TestMethod]
    public void ExcessiveIdleCandidateSprocIgnOffWithMotionTest()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        string gpsDeviceID = "110661";
        Asset testAsset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.CrossCheck, DateTime.UtcNow);
        API.Equipment.UpdateWorkingDefinition(session, testAsset.ID, WorkingDefinitionEnum.Movement, 0, true);
        Assert.IsNotNull(testAsset, "Failed to create Test Asset");
        DateTime nowUTC = DateTime.UtcNow;
        CreateAssetSubscription(session, testAsset.ID);
        PopulateDimTables();

        AddIgnOnOffWithLocation(testAsset.ID, nowUTC.AddMinutes(-40), false, 12.3, 39.8980712890625, -105.11262512207, 32.5);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-39), true, 32.5, 39.8980712890625, -105.11262512207, 56.7);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-20), false, 32.5, 39.8980712890625, -105.11262512207, 56.7);

        AddMovingWithLocation(testAsset.ID, nowUTC.AddMinutes(-5), true, 32.5, 39.8980712890625, -105.11262512207, 56.7);


        SyncNhDataToNhReport();
        ExecIdleCandidateUpdate();

        using (NH_RPT ctx = Model.NewNHContext<NH_RPT>())
        {
          var idleCandidate = (from eid in ctx.ExcessiveIdleCandidateReadOnly
                               where eid.DimAsset.ID == testAsset.ID
                               select eid);
          Assert.IsNotNull(idleCandidate, string.Format("There should be a excessive Idle Candidates for Asset {0}", testAsset.Name));
          Assert.AreEqual(1, idleCandidate.Count(), "There should be one IdleCandidates in the table");
          Assert.IsTrue(idleCandidate.FirstOrDefault<ExcessiveIdleCandidate>().IdlePeriodSeconds == TimeSpan.FromMinutes(15).TotalSeconds,
            "Idle candidate period should be 15 minutes");
          foreach (ExcessiveIdleCandidate i in idleCandidate)
          {
            Assert.IsFalse(i.InProgress, "Inprogress fields should be false for all Idle Candidates");
          }
        }
      }
    }

    /*
    private static InboundDeviceEventUtilization CreateInboundDeviceUtilizationEvent(string gpsDeviceID, Asset testAsset, DeviceTypeEnum deviceType, EventTypeEnum eventType, 
      bool isOn, DateTime eventTime, double latitude, double longitude, RuntimeTypeEnum runtimeType, long runTimeSeconds)
    {
      InboundDeviceEventUtilization item = new InboundDeviceEventUtilization();
      item.Asset = new InboundDeviceInfoAsset();
      item.Asset.AssetID = testAsset.ID;
      item.Asset.CustomerID = testAsset.Customer.ID;
      item.Device = new InboundDeviceInfoDevice();
      item.Device.DeviceTypeID = (int)deviceType;
      item.Device.ExtDeviceID = gpsDeviceID;
      item.ReceivedUTC = DateTime.UtcNow;
      item.EventUTC = eventTime;
      item.EventTypeID = (int)eventType;
      item.IsOn = isOn;
      item.Position = new InboundDeviceInfoPosition();
      item.Position.Latitude = latitude;//39.8980712890625;
      item.Position.Longitude = longitude;//-105.11262512207;
      item.RuntimeType = runtimeType;
      item.RuntimeSeconds = runTimeSeconds;
      return item;
    }
    */
  }
}
