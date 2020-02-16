using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class ReportLogicTestMTS : ReportLogicTestBase
  {  

    #region MTS521

    [DatabaseTest]
    [TestMethod]
    public void MTS521_NoJ1939_EventTimeStampMethod_PlusFuelEstimates_IgnEventsOnly()
    {
      //  MTS521 with no J1939 i.e. no engineParamters with idleTime. . Using EventTimeStamp workDefinition, therefor Util comes from Ign/Moving events, and fuel is estimated from those values.
  
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     -
      //  1  Feb 2010 07:00    ep/hl       1010    -
      //  1  Feb 2010 09:00    ep/hl       1015    -
 
      //  1 Feb 2010 08:00    IgnitionOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    IgnitionOff
      //      Monday                         4       1      3   

      MTS521_NoJ1939_EventTimeStampMethod_PlusFuelEstimates(false);
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_NoJ1939_EventTimeStampMethod_PlusFuelEstimates_IgnAndEngineEvents()
    {
      //  MTS521 with no J1939 i.e. no engineParamters with idleTime. . Using EventTimeStamp workDefinition, therefor Util comes from Ign/Moving events, and fuel is estimated from those values.

      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     -
      //  1  Feb 2010 07:00    ep/hl       1010    -
      //  1  Feb 2010 09:00    ep/hl       1015    -

      //  1 Feb 2010 07:00    IgnitionOn 
      //  1 Feb 2010 08:00    EngineOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    EngineOff
      //  1 Feb 2010 13:00    IgnitionOff
      //      Monday                         4       1      3   
      MTS521_NoJ1939_EventTimeStampMethod_PlusFuelEstimates(true);
    }

    public void MTS521_NoJ1939_EventTimeStampMethod_PlusFuelEstimates(bool useEngineEvents = false)
    {
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(5.0)
          .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(asset)
         .WorkDefinition(WorkDefinitionEnum.MovementEvents) // use EventTimeStamp method
         .Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      // some MTS521 can have EP events with idleTimes, but for this test, we'll have none with idleTimes
      // these should be ignored as the workDefinition is Movement
      double? engineIdleHours = null;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      eventUTC = startOfDeviceDay.AddHours(8);
      if (useEngineEvents)
      {
        Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value.AddHours(-1), true);
        Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      }
      else
      {
        Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      }
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      if (useEngineEvents)
      {
        Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, false);
        Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value.AddHours(-1), false);
      }
      else
      {
        Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);
      }

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      // adding a later Daily Report shouldn't change the utilization as it shouldn't be using these events
      runtimeHours = 1015;
      eventUTC = startOfDeviceDay.AddHours(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(16).KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(4, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(3, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(1, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // these are estimates using burnRates from AssetUtilization, using the EventTimeStamp method
      Assert.AreEqual(35, util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(30, util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(5, util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 3, 1, 3, 0);

    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_NoJ1939_MeterDeltaMethod_PlusFuelEstimates()
    {
      //  MTS521 with no J1939 i.e. no engineParamters with idleTime. Using MeterDelta workDefinition, therefor Util comes from EngineParams, and fuel is estimated from those values.
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     900
      //  1  Feb 2010 07:00    ep/hl       1010    -
      //  1  Feb 2010 09:00    ep/hl       1015     915

      //  1 Feb 2010 08:00    IgnitionOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    IgnitionOff
      //      Monday                         15       15      0   

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(5.0)
          .EstimatedWorkingBurnRateGallonsPerHour(10.0).Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      // some MTS521 can have EP events with idleTimes, but for this test, assume all the runtime was idleTime
      double? engineIdleHours = 900;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      engineIdleHours = 915;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      // adding a later Daily Report should change the utilization as latest rt is different
      runtimeHours = 1015;
      eventUTC = startOfDeviceDay.AddHours(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript(); // shouldn't do anything, just run it to check that it doesn't
      ExecuteMeterDeltaTransformScript(); 

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(0, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // these are estimates using burnRates from AssetUtilization
      Assert.AreEqual(75, util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(0, util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(75, util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
    
      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);
    
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_WithJ1939_MeterDelta_PlusFuelEstimation()
    {
      // MTS521 with J1939 i.e. has engineParamters with idleTime, but NO fuel meter readings (therefore estimate fuel, but will include idleFuel)
      //  uses same events MTS521_NoJ1939_MeterDeltaMethod_PlusFuelEstimates() test, but includes idleTimes
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     900
      //  1  Feb 2010 07:00    ep/hl       1010     904
      //  1  Feb 2010 09:00    ep/hl       1015     906

      //  1 Feb 2010 08:00    IgnitionOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    IgnitionOff
      //      Monday                         4       1      3   

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService()
                  .WithDefaultAssetUtilizationSettings()
                  .Save();
      double idleBurnRate = 5;
      double workingBurnRate = 10;
      Helpers.NHRpt.DimAsset_Populate();
      var expectedHoursProjected = Entity.AssetExpectedRuntimeHoursProjected.ForAsset(asset.AssetID).Save();
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(idleBurnRate)
          .EstimatedWorkingBurnRateGallonsPerHour(workingBurnRate).Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 904;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      // adding a later Daily Report should change the utilization as latest rt is different
      runtimeHours = 1015;
      eventUTC = startOfDeviceDay.AddHours(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 906;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript(); // shouldn't do anything, just run it to check that it doesn't
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(9, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(6, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // these are estimates using burnRates from AssetUtilization
      Assert.AreEqual(((9 * workingBurnRate) + (6 * idleBurnRate)), util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((9 * workingBurnRate), util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((6 * idleBurnRate), util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_WithJ1939_MeterDelta_PlusFuelCalculation()
    {
      // MTS521 with J1939 i.e. has engineParamters with idleTime,AND fuel meter readings (therefore will CALCULATE fuel i.e. from meter deltas)
      //  uses same events MTS521_NoJ1939_MeterDeltaMethod_PlusFuelEstimates() test, but includes idleTimes
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     900
      //  1  Feb 2010 07:00    ep/hl       1010     904
      //  1  Feb 2010 09:00    ep/hl       1015     906

      //  1 Feb 2010 08:00    IgnitionOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    IgnitionOff
      //      Monday                         4       1      3   

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      double idleBurnRate = 5;
      double workingBurnRate = 10;
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(idleBurnRate)
          .EstimatedWorkingBurnRateGallonsPerHour(workingBurnRate).Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay; 
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = 1200;
      double? consumptionGallons = 16000;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      engineIdleHours = 904;
      idleFuelGallons = 1310;
      consumptionGallons = 16230;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      // adding a later Daily Report should change the utilization as latest rt is different
      runtimeHours = 1015;
      eventUTC = startOfDeviceDay.AddHours(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      engineIdleHours = 906;
      idleFuelGallons = 1312;
      consumptionGallons = 16232;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript(); // shouldn't do anything, just run it to check that it doesn't
      ExecuteMeterDeltaTransformScript(); 

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(9, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(6, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // these are calculated from meter deltas
      Assert.AreEqual((16232-16000), util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((16232 - 16000) - (1312 - 1200), util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((1312-1200), util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);
    }

    [Ignore]
    [DatabaseTest]
    [TestMethod]
    public void MTS521_WithJ1939_ChangingWorkDefinition()
    {
      //  MTS521 withJ1939 changing workDefinitionTypes 
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     900
      //  1  Feb 2010 07:00    ep/hl       1010     904

      //  1 Feb 2010 08:00     IgnitionOn 
      //  1  Feb 2010 08:15    EngineStart
      //  1 Feb 2010 08:30     StartMoving  
      //  1 Feb 2010 11:30     StopMoving  
      //  1 Feb 2010 12:00     IgnitionOff
      //  1  Feb 2010 18:15    EngineStop
      //      Monday                         4       1      3 
      //      MeterDelta method:               10       4     6 (working definition for this day=MeterDelta)
      // *****
      //  2  Feb 2010 11:00    Location    1031
      //  2  Feb 2010 11:00    Ep                   909 
      //
      //  2  Feb 2010 07:00    IgnitionOn 
      //  2  Feb 2010 07:30    StartMoving  
      //  2  Feb 2010 11:30    StopMoving  
      //  2  Feb 2010 12:00    IgnitionOff
      //    EventTimeStamp method:            5       1     4 (working definition for this day=EventTimeStamp)
      //    MeterDelta method:               21       5    19
      // *****
      //  3  Feb 2010 13:00    Location    1042
      //  3  Feb 2010 13:00    Ep                   913
      //  
      //  3  Feb 2010 07:00    IgnitionOn 
      //  3  Feb 2010 09:30    StartMoving  
      //  3  Feb 2010 11:30    StopMoving  
      //  3  Feb 2010 12:00    IgnitionOff

      //   EventTimeStamp method:            5       3     2 (working definition for this day=EventTimeStamp)
      //   MeterDelta method:               11       4     7 
      // ***** 
      //  3  Feb 2010 20:00    EngineStart (for next day)
      //   
      //  4  Feb 2010 13:00    Location    1049
      //  4  Feb 2010 13:00    Ep                   916
      //  
      //  4  Feb 2010 04:00    IgnitionOn 
      //  4  Feb 2010 06:00    StartMoving  
      //  4  Feb 2010 11:30    StopMoving  
      //  4  Feb 2010 11:45    EngineStop 
      //  4  Feb 2010 12:00    IgnitionOff
      //   EventTimeStamp method:            8       2.5   5.5
      //   MeterDelta method:                7       3     4 (working definition for this day=MeterDelta)
      // *****
      //  5  Feb 2010 13:00   Location    1053
      //  5  Feb 2010 13:00   Ep                   918
      //  
      //  5  Feb 2010 05:00    IgnitionOn 
      //  5  Feb 2010 05:15    EngineStart 
      //  5  Feb 2010 06:00    StartMoving  
      //  5  Feb 2010 11:30    StopMoving  
      //  5  Feb 2010 12:00    IgnitionOff
      //   EventTimeStamp method:            7       1.5   5.5
      //   MeterDelta method:                4       2     2 (working definition for this day=MeterDelta)
      // *****

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      double idleBurnRate = 5;
      double workingBurnRate = 10;


      // create events in NH_DATA
      // 15th Jan
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;

      var expectedRuntimeHours = Entity.AssetExpectedRuntimeHoursProjected.UpdateUtc(testStartDeviceDay).ForAsset(asset).Save();
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
                      .EstimatedIdleBurnRateGallonsPerHour(idleBurnRate)
                      .EstimatedWorkingBurnRateGallonsPerHour(workingBurnRate).UpdateUtc(testStartDeviceDay).Save();
      var workDef = Entity.AssetWorkingDefinition.ForAsset(asset).WorkDefinition(WorkDefinitionEnum.MeterDelta).UpdateUtc(startOfDeviceDay).Save();
      Helpers.NHRpt.DimTables_Populate();

      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // 1st Feb
      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 904;
      idleFuelGallons = null;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(15);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(18).AddMinutes(15);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, false);

      // 2nd Feb
      startOfDeviceDay = testStartDeviceDay.AddDays(18);
       workDef = Entity.AssetWorkingDefinition.ForAsset(asset)
          .WorkDefinition(WorkDefinitionEnum.MovementEvents).UpdateUtc(startOfDeviceDay).SyncWithRpt().Save();

      // these should be ignored as the workDefinition is Movement
      runtimeHours = 1031;
      eventUTC = startOfDeviceDay.AddHours(11);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 909;
      idleFuelGallons = null;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(7).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // 3rd Feb
      startOfDeviceDay = testStartDeviceDay.AddDays(19);
      // these should be ignored as the workDefinition is Movement
      runtimeHours = 1042;
      eventUTC = startOfDeviceDay.AddHours(13);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 913;
      idleFuelGallons = null;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(9).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // 4th Feb - note trailing EngineOn from prior day.. switching to MetaDelta workDef
      eventUTC = startOfDeviceDay.AddHours(20);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);

      startOfDeviceDay = testStartDeviceDay.AddDays(20);
      workDef = Entity.AssetWorkingDefinition.ForAsset(asset)
        .WorkDefinition(WorkDefinitionEnum.MeterDelta).UpdateUtc(startOfDeviceDay).SyncWithRpt().Save();
 
      runtimeHours = 1049;
      eventUTC = startOfDeviceDay.AddHours(13);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 916;
      idleFuelGallons = null;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(4);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(6);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(45);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, false);

      // 5th Feb
      startOfDeviceDay = testStartDeviceDay.AddDays(21);
      runtimeHours = 1053;
      eventUTC = startOfDeviceDay.AddHours(13);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      engineIdleHours = 918;
      idleFuelGallons = null;
      consumptionGallons = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(5);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(6);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      eventUTC = startOfDeviceDay.AddHours(5).AddMinutes(15);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);


      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      Helpers.NHRpt.DimTables_Populate();
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(5, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      // 1st Feb comes from MeterDeltas
      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(6, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(4, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 1, 0, 0, 10);

      // 2nd Feb comes from TimeStampEvents
      Assert.AreEqual(testStartDeviceDay.AddDays(18).KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[1].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(5, util[1].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(4, util[1].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual(1, util[1].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[1].fk_AssetKeyDate));
      Assert.AreEqual((int)WorkDefinitionEnum.MovementEvents, util[1].ifk_DimWorkDefinitionID, string.Format("WorkDefinition type incorrect for Day:{0}", util[1].fk_AssetKeyDate));

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[1].fk_AssetKeyDate, 3, 1, 4, 0);

      // 3rd Feb comes from TimeStampEvents
      Assert.AreEqual(testStartDeviceDay.AddDays(19).KeyDate(), util[2].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(18).KeyDate(), util[2].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(5, util[2].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(2, util[2].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual(3, util[2].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[2].fk_AssetKeyDate));
      Assert.AreEqual((int)WorkDefinitionEnum.MovementEvents, util[2].ifk_DimWorkDefinitionID, string.Format("WorkDefinition type incorrect for Day:{0}", util[2].fk_AssetKeyDate));

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[2].fk_AssetKeyDate, 3, 3, 2, 0);

      // 4th Feb comes from MeterDeltas
      Assert.AreEqual(testStartDeviceDay.AddDays(20).KeyDate(), util[3].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(19).KeyDate(), util[3].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(7, util[3].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(4, util[3].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual(3, util[3].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[3].fk_AssetKeyDate));
      Assert.AreEqual((int)WorkDefinitionEnum.MeterDelta, util[3].ifk_DimWorkDefinitionID, string.Format("WorkDefinition type incorrect for Day:{0}", util[3].fk_AssetKeyDate));

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[3].fk_AssetKeyDate, 1, 0, 0, 11.75);

      // 5th Feb comes from MeterDeltas
      Assert.AreEqual(testStartDeviceDay.AddDays(21).KeyDate(), util[4].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.AddDays(20).KeyDate(), util[4].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(4, util[4].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(2, util[4].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual(2, util[4].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[4].fk_AssetKeyDate));
      Assert.AreEqual((int)WorkDefinitionEnum.MeterDelta, util[4].ifk_DimWorkDefinitionID, string.Format("WorkDefinition type incorrect for Day:{0}", util[4].fk_AssetKeyDate));

      // 5th Feb - inProgress EngineStart so its not reflected in FactAssetOperation table
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[4].fk_AssetKeyDate, 0, 0, 0, 0);
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_StateSpansMultipleDays()
    {
      ///MTS521 using MeterDelta method where EngineStart/Stop state spans >2 days
      // DeviceDate          Event       AssetOn                       [order added and ETL's]   
      //                     Type        Hours         
      // 30 Jan 2010 16:00   EngineStart  --  -- ignore partial states [1]
      // 1 Feb 2010 08:00    EngineStart  16                           [2] 
      // 2 Feb                            24  -- in-fill intermediate days
      // 3 Feb                            24
      // 4 Feb                            24 
      // 5 Feb                            24
      // 6 Feb 2010 13:00    EngineStop   13                           [3]
      // 
      // then insert start, then stop in middle...
      // 1 Feb 2010 08:00    EngineStart  16
      // 2 Feb      04:00    EngineStop    4                           [5]
      // 3 Feb                            -- -- fixup intermediate days
      // 4 Feb                            -- 
      // 4 Feb      21:00    EngineStart   3                           [4]
      // 5 Feb                            24
      // 6 Feb 2010 13:00    EngineStop   13
      // 
      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // 15th Jan
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay; 
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      // 30Jan 1 engineStart
      startOfDeviceDay = testStartDeviceDay.AddDays(15);
      eventUTC = startOfDeviceDay.AddHours(16);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      List<FactAssetOperationPeriod> faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
                                             where aSPeriod.ifk_DimAssetID == asset.AssetID
                                             orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
                                             select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(0, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (1Start)");

      // 1 Feb 2 engineStarts
      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(0, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (2Starts)");

      // 6 Feb 2 engineStarts, 1 stop
      startOfDeviceDay = testStartDeviceDay.AddDays(22);
      eventUTC = startOfDeviceDay.AddHours(13);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(6, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (2Starts, 1 stop)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(17).KeyDate(), 1, 0, 0, 16);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(18).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(19).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(20).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(21).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(22).KeyDate(), 1, 0, 0, 13);

      // 4 Feb 3 engineStarts(1inserted), 1 stop
      startOfDeviceDay = testStartDeviceDay.AddDays(20);
      eventUTC = startOfDeviceDay.AddHours(21);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(3, faup.Count(), "Incorrect count of FactAssetOperationnPeriod records (3Starts, 1 stop)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(20).KeyDate(), 1, 0, 0, 3);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(21).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(22).KeyDate(), 1, 0, 0, 13);

      // 2 Feb 3 engineStarts, 2 stops (1 inserted)
      startOfDeviceDay = testStartDeviceDay.AddDays(18);
      eventUTC = startOfDeviceDay.AddHours(4);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, eventUTC.Value, false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(5, faup.Count(), "Incorrect count of FactAssetOperationnPeriod records (3Starts, 2 stops)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(17).KeyDate(), 1, 0, 0, 16);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(18).KeyDate(), 1, 0, 0, 4);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(20).KeyDate(), 1, 0, 0, 3);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(21).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(22).KeyDate(), 1, 0, 0, 13);
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_ReprocessDaysTest1()
    {
      // Tests for a bug in uspPvt_FAUD_AssetDateList_DaysToReProcess()
      // Test1 1: uniqueStartDays between each new Periods' startSourceEventDeviceDate and endSourceEventDeviceDate
      //         e.g.  Got event for EffectiveCalenderDate 10 Jan but startingEventDay 5Jan.
      //                ... need to write intermediate days 5,6,7,8,9,10 with 'inProgress'
      //         e.g.  Got event for EffectiveCalenderDate 10 Jan but endingEventDay 14Jan.
      //                ... need to write intermediate days 10,11,12,13,14 with 'inProgress'

      // note that lat/long is UTC so no need to worry about time offsets
      // DeviceDate          Event Type              
      // day1 16:00   EngineStart   
      // checkpoint1: run ETL. should be: no OperationPeriods
      //
      // day3 05:00   EngineStop
      // day3 09:00   EngineStart
      // day3 10:00   EngineStop
      // checkpoint2: run ETL. Should be:   day1 8hours; day2=24; day3=6 (in 2 periods)
      //
     
      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // day1
      DateTime testStartDeviceDay = DateTime.UtcNow.AddDays(-10).StartOfDay(); // i.e. start of day1      
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      startOfDeviceDay = testStartDeviceDay;
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(16), true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      List<FactAssetOperationPeriod> faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
                                             where aSPeriod.ifk_DimAssetID == asset.AssetID
                                             orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
                                             select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(0, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (checkpoint1)");


      // day3
      startOfDeviceDay = testStartDeviceDay.AddDays(2);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(5), false);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(9), true);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(10), false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(4, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint2)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.KeyDate(), 1, 0, 0, 8);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(1).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(2).KeyDate(), 2, 0, 0, 6);
  
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_ReprocessDaysTest2()
    {
      // Tests for a bug in uspPvt_FAUD_AssetDateList_DaysToReProcess()
      // test 2: uniqueStartDays for any state timePeriods (could be 1 or more days) for which we inserted a day inare about to be splatted in.     
      //        e.g.  Going to process EffectiveCalenderDate 10 Feb, (may have gotten a late start/stop move)
      //                   does this day already exist in AO, if so need to re-process that span (may have been a movement span no longer valid)
      //                 ... AO row for 10Feb with StartStateDeviceLocal = 9Feb and EndStateDeviceLocal = 13 Feb
      //                 ... need to re-process intermediate days 9,10,11,12,13 

      // note that lat/long is UTC so no need to worry about time offsets
      // DeviceDate          Event Type              
      // day1 16:00   EngineStart   
      // checkpoint1: run ETL. should be: no OperationPeriods --  ignore partial states [1] 
      //
      // day4 07:00   EngineStop   
      // checkpoint2: run ETL. Should be:   day1 8hours; day2=24; day3=24; day4=7 (in 1 period)
      //
      // day3 05:00   EngineStop
      // day3 09:00   EngineStart
      // checkpoint3: run ETL. Should be:   day1 8hours; day2=24; day3=20 (in 2 periods); day4=7
      //

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // day1
      DateTime testStartDeviceDay = DateTime.UtcNow.AddDays(-10).StartOfDay(); // i.e. start of day1      
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      startOfDeviceDay = testStartDeviceDay;
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(16), true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      List<FactAssetOperationPeriod> faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
                                             where aSPeriod.ifk_DimAssetID == asset.AssetID
                                             orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
                                             select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(0, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (checkpoint1)");


      // day4 
      startOfDeviceDay = testStartDeviceDay.AddDays(3);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(7), false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(4, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint2)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.KeyDate(), 1, 0, 0, 8);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(1).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(2).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(3).KeyDate(), 1, 0, 0, 7);

      // day3 
      startOfDeviceDay = testStartDeviceDay.AddDays(2);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(5), false);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(9), true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(5, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint3)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.KeyDate(), 1, 0, 0, 8);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(1).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(2).KeyDate(), 2, 0, 0, 20);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(3).KeyDate(), 1, 0, 0, 7);

    }


    [DatabaseTest]
    [TestMethod]
    public void MTS521_ReprocessDaysTest2MultiAssetBug()
    {
      // over and above MTS521_ReprocessDaysTest2 test bug fix for the following waste of time:
      //      Where a faop day already exists e.g. on 'Day_6', for THIS asset, i.e. has already been processed. 
      //      At checkpoint3, Also process another asset for 'Day_6' day.
      // as -is, this is a valid test in itself, but to check that days aren't being re-processed unnecessarily, 
      // manually run uspPvt_FAUD_AssetDateList_DaysToReProcess to check that it doesn't re-process THIS assets 'Day_6'.
      //

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      var assetOther = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS522.OwnerBssId(TestData.TestCustomer.BSSID).Save())
            .WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      // day1
      DateTime testStartDeviceDay = DateTime.UtcNow.AddDays(-10).StartOfDay(); // i.e. start of day1      
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.DataHoursLocation_Add(assetOther.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);


      startOfDeviceDay = testStartDeviceDay;
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(16), true);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      List<FactAssetOperationPeriod> faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
                                             where aSPeriod.ifk_DimAssetID == asset.AssetID
                                             orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
                                             select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(0, faup.Count(), "Incorrect count of FactAssetOperationPeriod records (checkpoint1)");


      // day4 
      startOfDeviceDay = testStartDeviceDay.AddDays(3);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(7), false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(4, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint2)");

      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.KeyDate(), 1, 0, 0, 8);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(1).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(2).KeyDate(), 1, 0, 0, 24);
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, testStartDeviceDay.AddDays(3).KeyDate(), 1, 0, 0, 7);

      // day6 asset 
      startOfDeviceDay = testStartDeviceDay.AddDays(5);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(7), true);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(8), false);
      SyncNhDataToNhReport();
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(5, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint2a)");

      // day6 assetOther 
      startOfDeviceDay = testStartDeviceDay.AddDays(5);
      Helpers.NHData.EngineStartStop_Add(assetOther.AssetID, startOfDeviceDay.AddHours(7), true);
      Helpers.NHData.EngineStartStop_Add(assetOther.AssetID, startOfDeviceDay.AddHours(8), false);
      // day3 
      startOfDeviceDay = testStartDeviceDay.AddDays(2);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(5), false);
      Helpers.NHData.EngineStartStop_Add(asset.AssetID, startOfDeviceDay.AddHours(9), true);
      SyncNhDataToNhReport();

      // todo to test the bug is fixed you can only do this manually by leaving this test data in database; runing a composite TSQL then rebuilding your localhost (to cleanup data).
      //     comment out below code, set transaction state to Supress; run the test and analyse TSQL as above.
      ExecuteMeterDeltaTransformScript();
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == asset.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(6, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint3)");
      faup = (from aSPeriod in Ctx.RptContext.FactAssetOperationPeriodReadOnly
              where aSPeriod.ifk_DimAssetID == assetOther.AssetID
              orderby aSPeriod.fk_AssetKeyDate, aSPeriod.StartStateDeviceLocal
              select aSPeriod).ToList<FactAssetOperationPeriod>();
      Assert.AreEqual(1, faup.Count(), "Incorrect count of FactAssetOperationPeriod records checkpoint3, assetOther)");

    }

    #endregion


    #region TimingIssues
      // these tests are difficult to reproduce manually e.g. with SimVisant as they are to do with the order events
      // are processed by various ETls

    [DatabaseTest]
    [TestMethod]
    public void Timing_EngineParamsNotUpdated_sameMID()
    {
        // Tests for a bug 27415 'Missing TotalGallonsCuml in EngineParameter' 
        //      and 28777 'SF00677831: Idle values are not always being propagated accurately from NH_DATA into NH_RPT'
        //
        // add EP and HL events to be used as day0  
        // Test1 1: for next keyDate day1
        //          add ep event with totalgallons, day 1a
        //          run latestEventsPopA and latestEventsPopB
        //          add hl event with runtimeHours, day 1b
        //          add ep event with idlegallons and idleFuel, day 1c
        //          run latestEventsPopA [this will touch the engineParm updateUTC for 1a] and latestEventsPopB [so this will not pick up the updated DEP for 1c]
        //
        //  run all Utilization ETLs
        //  check that FAUD has totalGallons, idleGallons and idleHours for day1 
        //

        var customer = TestData.TestCustomer;
        var user = TestData.TestCustomerUser;
        var session = TestData.CustomerUserActiveUser;
        var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                    .WithCoreService().Save();
        var asset2 = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                    .WithCoreService().Save();

        Helpers.NHRpt.DimTables_Populate();

        // day 0 (HL+EP
        DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
        DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
        DateTime startOfDeviceDay = testStartDeviceDay;
        double? latitude = 20; // GMT
        double? longitude = -8.021608;
        DateTime? eventUTC = startOfDeviceDay.AddHours(7);
        DateTime? eventUTCForInitialHL = eventUTC;
        long? runtimeHours = 1000;
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

        double? engineIdleHours = 100;
        double? idleFuelGallons = 2000;
        double? consumptionGallons = 9000;
        double? percentRemaining = null;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, 
            debugRefID: 0, sourceMsgID: 0, mid: "46",
            maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
            starts: null, revolutions: null,
            consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                                where aud.ifk_DimAssetID == asset.AssetID                                                
                                                orderby aud.fk_AssetKeyDate
                                                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

        Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 0"));
        Assert.IsNull(util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for day 0"));
        Assert.IsNull(util[0].RuntimeHours, string.Format("Runtime hours incorrect for day 0"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.NoData, util[0].ifk_RuntimeHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 0"));

        
        // day 1a
        startOfDeviceDay = testStartDeviceDay.AddDays(1);        
        eventUTC = startOfDeviceDay.AddHours(3);

        engineIdleHours = null;
        idleFuelGallons = null;
        consumptionGallons = 9200;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value,
                   debugRefID: 0, sourceMsgID: 0, mid: "46",
                   maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
                   starts: null, revolutions: null,
                   consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                                where aud.ifk_DimAssetID == asset.AssetID                                                
                                                orderby aud.fk_AssetKeyDate
                                                select aud).ToList<FactAssetUtilizationDaily>();
        // should still only be 1 as no matching RT
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");


        // day 1b
        runtimeHours = 1010;
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

       
        // day 1c
        engineIdleHours = 120;
        idleFuelGallons = 2200;
        consumptionGallons = null;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value,
           debugRefID: 0, sourceMsgID: 0, mid: "46",
           maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
           starts: null, revolutions: null,
           consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        // just to bump UploadService the date
        Helpers.NHData.DataHoursLocation_Add(asset2.AssetID, DimSourceEnum.PLIPGateway, eventUTCForInitialHL.Value, runtimeHours: 1000, latitude: latitude, longitude: longitude);


        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                where aud.ifk_DimAssetID == asset.AssetID
                 && aud.fk_AssetPriorKeyDate != null
                orderby aud.fk_AssetKeyDate
                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

        Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 1b"));
        Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for day 1b"));
        Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for day 1b"));
        Assert.AreEqual(200, util[0].TotalFuelConsumedGallons, string.Format("TotalFuel incorrect for day 1b"));
        Assert.AreEqual(9200, util[0].TotalFuelConsumedGallonsMeter, string.Format("TotalFuelConsumedGallonsMeter incorrect for day 1b"));
        Assert.AreEqual(20, util[0].IdleHours, string.Format("IdleHours incorrect for day 1b"));
        Assert.AreEqual(120, util[0].IdleHoursMeter, string.Format("IdleHoursMeter incorrect for day 1b"));
        Assert.AreEqual(200, util[0].IdleFuelConsumedGallons, string.Format("IdleFuelConsumedGallons incorrect for day 1b"));
        Assert.AreEqual(2200, util[0].IdleFuelConsumedGallonsMeter, string.Format("IdleFuelConsumedGallonsMeter incorrect for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_RuntimeHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_IdleHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
    }

    [DatabaseTest]
    [TestMethod]
    public void Timing_EngineParamsNotUpdated_differentMID()
    {
        // Tests for a bug 27415 'Missing TotalGallonsCuml in EngineParameter' 
        //      and 28777 'SF00677831: Idle values are not always being propagated accurately from NH_DATA into NH_RPT'
        //
        // add EP and HL events to be used as day0  
        // Test1 1: for next keyDate day1
        //          add ep event with totalgallons, day 1a (MID 46)
        //          run latestEventsPopA and latestEventsPopB
        //          add hl event with runtimeHours, day 1b (MID 345, this will create 2 DEP events, which get rolled into 1 EP)
        //          add ep event with idlegallons and idleFuel, day 1c
        //          run latestEventsPopA [this will touch the engineParm updateUTC for 1a] and latestEventsPopB [so this will not pick up the updated DEP for 1c]
        //
        //  run all Utilization ETLs
        //  check that FAUD has totalGallons, idleGallons and idleHours for day1 
        //

        var customer = TestData.TestCustomer;
        var user = TestData.TestCustomerUser;
        var session = TestData.CustomerUserActiveUser;
        var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                    .WithCoreService().Save();
        var asset2 = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                    .WithCoreService().Save();

        Helpers.NHRpt.DimTables_Populate();

        // day 0 (HL+EP
        DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
        DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
        DateTime startOfDeviceDay = testStartDeviceDay;
        double? latitude = 20; // GMT
        double? longitude = -8.021608;
        DateTime? eventUTC = startOfDeviceDay.AddHours(7);
        DateTime? eventUTCForInitialHL = eventUTC;
        long? runtimeHours = 1000;
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

        double? engineIdleHours = 100;
        double? idleFuelGallons = 2000;
        double? consumptionGallons = 9000;
        double? percentRemaining = null;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value,
            debugRefID: 0, sourceMsgID: 0, mid: "46",
            maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
            starts: null, revolutions: null,
            consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                                where aud.ifk_DimAssetID == asset.AssetID
                                                orderby aud.fk_AssetKeyDate
                                                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

        Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 0"));
        Assert.IsNull(util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for day 0"));
        Assert.IsNull(util[0].RuntimeHours, string.Format("Runtime hours incorrect for day 0"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.NoData, util[0].ifk_RuntimeHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 0"));


        // day 1a
        startOfDeviceDay = testStartDeviceDay.AddDays(1);
        eventUTC = startOfDeviceDay.AddHours(3);

        engineIdleHours = null;
        idleFuelGallons = null;
        consumptionGallons = 9200;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value,
                   debugRefID: 0, sourceMsgID: 0, mid: "46",
                   maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
                   starts: null, revolutions: null,
                   consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                where aud.ifk_DimAssetID == asset.AssetID
                orderby aud.fk_AssetKeyDate
                select aud).ToList<FactAssetUtilizationDaily>();
        // should still only be 1 as no matching RT
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");


        // day 1b
        runtimeHours = 1010;
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);


        // day 1c
        engineIdleHours = 120;
        idleFuelGallons = 2200;
        consumptionGallons = null;
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value,
           debugRefID: 0, sourceMsgID: 0, mid: "345",
           maxFuelGallons: null, idleFuelGallons: idleFuelGallons, machineIdleFuelGallons: null, engIdleHrs: engineIdleHours,
           starts: null, revolutions: null,
           consumptionGallons: consumptionGallons, percentRemaining: percentRemaining, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        // just to bump UploadService the date
        Helpers.NHData.DataHoursLocation_Add(asset2.AssetID, DimSourceEnum.PLIPGateway, eventUTCForInitialHL.Value, runtimeHours: 1000, latitude: latitude, longitude: longitude);
        
        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                where aud.ifk_DimAssetID == asset.AssetID
                 && aud.fk_AssetPriorKeyDate != null
                orderby aud.fk_AssetKeyDate
                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

        Assert.AreEqual(testStartDeviceDay.AddDays(1).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 1b"));
        Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for day 1b"));
        Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for day 1b"));
        Assert.AreEqual(200, util[0].TotalFuelConsumedGallons, string.Format("TotalFuel incorrect for day 1b"));
        Assert.AreEqual(9200, util[0].TotalFuelConsumedGallonsMeter, string.Format("TotalFuelConsumedGallonsMeter incorrect for day 1b"));
        Assert.AreEqual(20, util[0].IdleHours, string.Format("IdleHours incorrect for day 1b"));
        Assert.AreEqual(120, util[0].IdleHoursMeter, string.Format("IdleHoursMeter incorrect for day 1b"));
        Assert.AreEqual(200, util[0].IdleFuelConsumedGallons, string.Format("IdleFuelConsumedGallons incorrect for day 1b"));
        Assert.AreEqual(2200, util[0].IdleFuelConsumedGallonsMeter, string.Format("IdleFuelConsumedGallonsMeter incorrect for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_RuntimeHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_TotalFuelConsumedGallonsCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_IdleHoursCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));
        Assert.AreEqual((int)DimUtilizationCalloutTypeEnum.None, util[0].ifk_IdleFuelConsumedGallonsCalloutTypeID, string.Format("runtimeHours callout wrong for day 1b"));

    }

    [DatabaseTest]
    [TestMethod]
    public void Timing_EngineParamsUpdated_withOldRuntime()
    {
        // Tests for a bug 29186 'SF00688903: More recent valid HoursLocation/EngineParms records are not selected for the Utilization daily recordset' 
        //     
        //
        // add EP and HL events to be used as day0  runtimeHoursMeter (10); totalgallonsMeter (200)
        // add ep event for day1 with totalgallonsMeter (225)
        // add ep event for day2a with totalgallonsMeter (251)
        // add hl event for day2a with runtimeHoursMeter (32)
        // add ep event for day2b with totalgallonsMeter (253)        
        //  run all Utilization ETLs
        //
        // add hl event for day1 with runtimeHoursMeter (25)
        // add hl event for day2b with runtimeHoursMeter (33)        
        //  run all Utilization ETLs
        //  check that FAUD day1 has totalGallonsMeter = 225 and runtimeHoursMeter = 25
        //  check that FAUD day2 has totalGallonsMeter = 253 and runtimeHoursMeter = 33
        //

        var customer = TestData.TestCustomer;
        var user = TestData.TestCustomerUser;
        var session = TestData.CustomerUserActiveUser;
        var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                    .WithCoreService().Save();
        var asset2 = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
            .WithCoreService().Save();

        Helpers.NHRpt.DimTables_Populate();

        // day 0 (HL+EP)
        DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
        DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
        DateTime startOfDeviceDay0 = testStartDeviceDay;
        DateTime startOfDeviceDay1 = testStartDeviceDay.AddDays(1);
        DateTime startOfDeviceDay2 = testStartDeviceDay.AddDays(2);
        double? latitude = 20; // GMT
        double? longitude = -8.021608;
        DateTime? eventUTCDay0 = startOfDeviceDay0.AddHours(2);
        DateTime? eventUTCDay1 = startOfDeviceDay1.AddHours(3);
        DateTime? eventUTCDay2a = startOfDeviceDay2.AddHours(2);
        DateTime? eventUTCDay2b = startOfDeviceDay2.AddHours(3);
        DateTime? eventUTCForInitialHL = eventUTCDay0;
        long? runtimeHours = 10;
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTCDay0.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTCDay0.Value,
            debugRefID: 0, sourceMsgID: 0, mid: null,
            maxFuelGallons: null, idleFuelGallons: null, machineIdleFuelGallons: null, engIdleHrs: null,
            starts: null, revolutions: null,
            consumptionGallons: 200, percentRemaining: null, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                                where aud.ifk_DimAssetID == asset.AssetID
                                                orderby aud.fk_AssetKeyDate
                                                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");


        // add EPs
        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTCDay1.Value,
                   debugRefID: 0, sourceMsgID: 0, mid: null,
                   maxFuelGallons: null, idleFuelGallons: null, machineIdleFuelGallons: null, engIdleHrs: null,
                   starts: null, revolutions: null,
                   consumptionGallons: 225, percentRemaining: null, machineIdleHrs: null);

        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTCDay2a.Value,
                   debugRefID: 0, sourceMsgID: 0, mid: null,
                   maxFuelGallons: null, idleFuelGallons: null, machineIdleFuelGallons: null, engIdleHrs: null,
                   starts: null, revolutions: null,
                   consumptionGallons: 251, percentRemaining: null, machineIdleHrs: null);
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTCDay2a.Value, runtimeHours: 32, latitude: latitude, longitude: longitude);

        Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTCDay2b.Value,
                   debugRefID: 0, sourceMsgID: 0, mid: null,
                   maxFuelGallons: null, idleFuelGallons: null, machineIdleFuelGallons: null, engIdleHrs: null,
                   starts: null, revolutions: null,
                   consumptionGallons: 253, percentRemaining: null, machineIdleHrs: null);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();


        // add HL's
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTCDay1.Value, runtimeHours: 25, latitude: latitude, longitude: longitude);        
        Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTCDay2b.Value, runtimeHours: 33, latitude: latitude, longitude: longitude);


        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        // just to bump UploadService the date
        // this is needed because the FAUD_MeterDelta uses HoursLocation bookmarks, rather than EngineParameters.
        Helpers.NHData.DataHoursLocation_Add(asset2.AssetID, DimSourceEnum.PLIPGateway, eventUTCForInitialHL.Value, runtimeHours: 1000, latitude: latitude, longitude: longitude);

        SyncNhDataToNhReport();
        ExecuteMeterDeltaTransformScript();

        util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                where aud.ifk_DimAssetID == asset.AssetID
                 && aud.fk_AssetPriorKeyDate != null
                orderby aud.fk_AssetKeyDate
                select aud).ToList<FactAssetUtilizationDaily>();
        Assert.AreEqual(2, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

        Assert.AreEqual(startOfDeviceDay1.KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 1"));       
        Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours incorrect for day 1"));
        Assert.AreEqual(25, util[0].RuntimeHoursMeter, string.Format("Runtime hours meter incorrect for day 1"));
        Assert.AreEqual(25, util[0].TotalFuelConsumedGallons, string.Format("TotalFuel incorrect for day 1"));
        Assert.AreEqual(225, util[0].TotalFuelConsumedGallonsMeter, string.Format("TotalFuelConsumedGallonsMeter incorrect for day 1"));

        Assert.AreEqual(startOfDeviceDay2.KeyDate(), util[1].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for day 2"));
        Assert.AreEqual(8, util[1].RuntimeHours, string.Format("Runtime hours incorrect for day 2"));
        Assert.AreEqual(33, util[1].RuntimeHoursMeter, string.Format("Runtime hours meter incorrect for day 2"));
        Assert.AreEqual(28, util[1].TotalFuelConsumedGallons, string.Format("TotalFuel incorrect for day 2"));
        Assert.AreEqual(253, util[1].TotalFuelConsumedGallonsMeter, string.Format("TotalFuelConsumedGallonsMeter incorrect for day 2"));
    }

  #endregion


    #region MTS522

    [DatabaseTest]
    [TestMethod]
    public void MTS522_WithJ1939_MeterDelta_PlusFuelEstimation()
    {
      // MTS522 with J1939 i.e. has engineParamters with idleTime, but NO fuel meter readings (therefore estimate fuel, but will include idleFuel)
      //  uses same events MTS521_WithJ1939_MeterDelta_PlusFuelEstimation() test, but includes idleTimes
      //   DeviceDate          Event       Runtime  Idle    Working   Fuel
      //                       Type        Hours    Hours   Hours 
      //  15 Jan 2010 07:00    ep/hl       1000     900
      //  1  Feb 2010 07:00    ep/hl       1010     904
      //  1  Feb 2010 09:00    ep/hl       1015     906

      //  1 Feb 2010 08:00    IgnitionOn 
      //  1 Feb 2010 08:30    StartMoving  
      //  1 Feb 2010 11:30    StopMoving  
      //  1 Feb 2010 12:00    IgnitionOff
      //      Monday                         4       1      3   

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS522.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      double idleBurnRate = 5;
      double workingBurnRate = 10;
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(idleBurnRate)
          .EstimatedWorkingBurnRateGallonsPerHour(workingBurnRate).Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay; 
      long? runtimeHours = 1000;
      double? latitude = 20; // GMT
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = 900;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      engineIdleHours = 904;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // these should be ignored as the workDefinition is MeterDelta
      eventUTC = startOfDeviceDay.AddHours(8);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(8).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, true);
      eventUTC = startOfDeviceDay.AddHours(11).AddMinutes(30);
      Helpers.NHData.Moving_Add(asset.AssetID, eventUTC.Value, false);
      eventUTC = startOfDeviceDay.AddHours(12);
      Helpers.NHData.IgnitionOnOff_Add(asset.AssetID, eventUTC.Value, false);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      // adding a later Daily Report should change the utilization as latest rt is different
      runtimeHours = 1015;
      eventUTC = startOfDeviceDay.AddHours(9);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      engineIdleHours = 906;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript(); // shouldn't do anything, just run it to check that it doesn't
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(15, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(9, util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(6, util[0].IdleHours, string.Format("Idle hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // these are estimates using burnRates from AssetUtilization
      Assert.AreEqual(((9 * workingBurnRate) + (6 * idleBurnRate)), util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((9 * workingBurnRate), util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual((6 * idleBurnRate), util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
    
      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);

    }

    [DatabaseTest]
    [TestMethod]
    public void MTS521_WithJ1939_MeterDelta_DoesNotHaveIdleTime_HasTotalFuel_HasIdleFuel()
    {
      // MTS521 with J1939 i.e. has engineParamters with idleTime, but NO fuel meter readings (therefore estimate fuel, but will include idleFuel)
      //  uses same events MTS521_WithJ1939_MeterDelta_PlusFuelEstimation() test, but includes idleTimes
      //   DeviceDate          Event       Runtime  Idle    Working   Idle  Total
      //                       Type        Hours    Hours   Hours     Fuel  Fuel
      //  15 Jan 2010 07:00    ep/hl       1000                       100   110
      //  1  Feb 2010 07:00    ep/hl       1010                       200   210

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS521.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      double idleBurnRate = 5;
      double workingBurnRate = 10;
      var burnRates = Entity.AssetBurnRates.ForAsset(asset)
          .EstimatedIdleBurnRateGallonsPerHour(idleBurnRate)
          .EstimatedWorkingBurnRateGallonsPerHour(workingBurnRate).Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay; 
      long? runtimeHours = 1000;
      double? latitude = 20;
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);

      double? engineIdleHours = null;
      double? idleFuelGallons = 100;
      double? consumptionGallons = 200;
      double? percentRemaining = null;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      idleFuelGallons = 110;
      consumptionGallons = 210;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude:longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].IdleHours, string.Format("Should not be idle hours for Day:{0}", util[0].fk_AssetKeyDate));

      // these are calculated using runtime, total and idleGallons. Since we don't have an idleTime we can't determine idle or working Burn rates
      Assert.AreEqual(10, util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(0, util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);
    }

    [DatabaseTest]
    [TestMethod]
    public void MTS522_MeterDelta_HasEngineParam_NoIdleTime()
    {
      // Some MTS522 do not report on an idle time meter or fuel, they only report levelPercent. 
      // Therefore they have engineParameter events but no useful Utilization data.
      //   We should use only the Runtime values and ignore the EngineParameter events.  
      //   DeviceDate          Event       Runtime  Idle    Working   Idle  Total LevelPercent
      //                       Type        Hours    Hours   Hours     Fuel  Fuel
      //  15 Jan 2010 07:00    ep/hl       1000                                    90
      //  1  Feb 2010 07:00    ep/hl       1010                                    80

      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS522.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay;
      long? runtimeHours = 1000;
      double? latitude = 20;
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);

      double? engineIdleHours = null;
      double? idleFuelGallons = null;
      double? consumptionGallons = null;
      double? percentRemaining = 90;
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      percentRemaining = 85;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.EngineParameters_Add(asset.AssetID, eventUTC.Value, null, idleFuelGallons, null, engineIdleHours, null, null, consumptionGallons, percentRemaining, null);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript(); // shouldn't do anything, just run it to check that it doesn't

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                              where aud.ifk_DimAssetID == asset.AssetID
                                               && aud.fk_AssetPriorKeyDate != null
                                              orderby aud.fk_AssetKeyDate
                                              select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(testStartDeviceDay.AddDays(17).KeyDate(), util[0].fk_AssetKeyDate, string.Format("StartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(testStartDeviceDay.KeyDate(), util[0].fk_AssetPriorKeyDate, string.Format("PriorStartOfDeviceDay wrong for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.AreEqual(10, util[0].RuntimeHours, string.Format("Runtime hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].WorkingHours, string.Format("Working hours incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].IdleHours, string.Format("Should not be idle hours for Day:{0}", util[0].fk_AssetKeyDate));

      // Since we don't have an idleTime, AND it's a 521/522/940, we don't calculate fuel - should really estimate, but there's an issue in
      // uspPvt_FAUD_AssetDateList_MeterDelta where 'calculateFuel' is set
      Assert.IsNull(util[0].TotalFuelConsumedGallons, string.Format("TotalGals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].WorkingFuelConsumedGallons, string.Format("Working Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));
      Assert.IsNull(util[0].IdleFuelConsumedGallons, string.Format("Idle Gals incorrect for Day:{0}", util[0].fk_AssetKeyDate));

      // there were no EngineStartStop events, therefore no runtime segments for AO.
      AssertAssetOperationTotals(Ctx.RptContext, asset.AssetID, util[0].fk_AssetKeyDate, 0, 0, 0, 0);
    }
  
  #endregion

    #region PL420

    [TestMethod]
    [DatabaseTest]
    public void PL420_AssetUtilizationDaily_PTOTest_FirstDay()
    {
      Asset testAsset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(TestData.TestPL420).WithCoreService().Save();

      Helpers.NHRpt.DimTables_Populate();
      
      var testDate = GetValidTestDate();

      Helpers.NHData.ParameterReportAdd(testAsset.AssetID, testDate.AddMinutes(-1), 0, 65255, 248, 30, null, 1, 0);
      Helpers.NHData.ParameterReportAdd(testAsset.AssetID, testDate.AddMinutes(-3), 0, 65255, 248, 15, null, 1, 0);
      Helpers.NHData.ParameterReportAdd(testAsset.AssetID, testDate, 3, 65255, 248, 45, null, 1, 0);
      Helpers.NHData.ParameterReportAdd(testAsset.AssetID, testDate.AddMinutes(-4), 3, 65255, 248, 60, null, 1, 0);
      Helpers.NHData.ParameterReportAdd(testAsset.AssetID, testDate, 0, 65255, 249, 100, null, 1, 0);

      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == testAsset.AssetID
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(0, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");
      
    }

    [TestMethod]
    [DatabaseTest]
    public void PL420_AssetUtilizationDaily_PTOTest_MultipleDays()
    {
      
      var customer = TestData.TestCustomer;
      var user = TestData.TestCustomerUser;
      var session = TestData.CustomerUserActiveUser;
      var asset = Entity.Asset.WithDefaultAssetUtilizationSettings().WithDevice(Entity.Device.MTS522.OwnerBssId(TestData.TestCustomer.BSSID).Save())
                  .WithCoreService().Save();
      Helpers.NHRpt.DimTables_Populate();

      // create events in NH_DATA
      DateTime dateSetup = (DateTime.UtcNow.AddDays(DayOfWeek.Friday - DateTime.UtcNow.DayOfWeek).AddDays(-28)).Date;
      DateTime testStartDeviceDay = new DateTime(dateSetup.Year, dateSetup.Month, dateSetup.Day);
      DateTime startOfDeviceDay = testStartDeviceDay; 
      long? runtimeHours = 1000;
      double? latitude = 20;
      double? longitude = -8.021608;
      DateTime? eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.CustomUtilizationEvent_Add(asset.AssetID, eventUTC.Value, DimCustomUtilizationEventTypeEnum.TotalEnginePowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 15);
      Helpers.NHData.CustomUtilizationEvent_Add(asset.AssetID, eventUTC.Value, DimCustomUtilizationEventTypeEnum.TotalTransmissionPowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 20);

      startOfDeviceDay = testStartDeviceDay.AddDays(17);
      runtimeHours = 1010;
      eventUTC = startOfDeviceDay.AddHours(7);
      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PR3Gateway, eventUTC.Value, runtimeHours: runtimeHours, latitude: latitude, longitude: longitude);
      Helpers.NHData.CustomUtilizationEvent_Add(asset.AssetID, eventUTC.Value, DimCustomUtilizationEventTypeEnum.TotalEnginePowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 45);
      Helpers.NHData.CustomUtilizationEvent_Add(asset.AssetID, eventUTC.Value, DimCustomUtilizationEventTypeEnum.TotalTransmissionPowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 25);

      // Transforms data from NH_DATA into NH_RPT FactAssetUtilizationDaily
      SyncNhDataToNhReport();
      ExecuteEventTimeStampTransformScript();
      ExecuteMeterDeltaTransformScript();

      List<FactAssetUtilizationDaily> util = (from aud in Ctx.RptContext.FactAssetUtilizationDailyReadOnly
                                          where aud.ifk_DimAssetID == asset.AssetID
                                           && aud.fk_AssetPriorKeyDate != null
                                          orderby aud.fk_AssetKeyDate
                                          select aud).ToList<FactAssetUtilizationDaily>();
      Assert.AreEqual(1, util.Count(), "Incorrect FactAssetUtilizationDaily record count.");

      Assert.AreEqual(30, util.FirstOrDefault().EnginePTOHours, "Incorrect EnginePTOHours");
      Assert.AreEqual(5, util.FirstOrDefault().TransmissionPTOHours, "Incorrect TransmissionPTOHours");
    }
    #endregion

  }
}
