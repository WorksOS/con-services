using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests
{
  [TestClass]
  public class AssetCurrentStatusTest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL121).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      DateTime locUTC = DateTime.UtcNow.AddSeconds(-1);
      double? lat = 12.321;
      double? lon = -98.543;
      double? mph = 43.5;
      double? hrs = 345.8;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon,speed:mph);

      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.Latitude, "Null Latitude");
      Assert.IsNotNull(acs.Longitude, "Null Longitude");
      Assert.IsNotNull(acs.SpeedMPH, "Null Speed");
      Assert.IsNotNull(acs.RuntimeHours, "Null RuntimeHours");
      Assert.AreEqual(lat, acs.Latitude.Value, "Latitude not updated");
      Assert.AreEqual(lon, acs.Longitude.Value, "Longitude not updated");
      Assert.IsNull(acs.AltitudeMeters, "Altitude should be null");
      Assert.AreEqual(mph, acs.SpeedMPH.Value, "Speed not updated");
      Assert.AreEqual(hrs, acs.RuntimeHours.Value, "RuntimeHours not updated");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not Reporting and should be");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_InvalidLocation_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL121).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      DateTime locUTC = DateTime.UtcNow.AddSeconds(-1);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC.AddMinutes(1), DateTime.UtcNow.AddMinutes(1), hrs, lat, lon, speed:mph, locIsValid:false);

      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.Latitude, "Null Latitude");
      Assert.IsNotNull(acs.Longitude, "Null Longitude");
      Assert.AreEqual(lat, acs.Latitude.Value, "Latitude not updated");
      Assert.AreEqual(lon, acs.Longitude.Value, "Longitude not updated");
      Assert.AreEqual(locUTC.Date, acs.LastLocationUTC.Value.Date, "LastLocationUTC Date does not match");
      Assert.AreEqual(locUTC.Minute, acs.LastLocationUTC.Value.Minute, "LastLocationUTC Minute does not match");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_Altitude_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL321).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      DateTime locUTC = DateTime.UtcNow.AddSeconds(-1);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double altMeters = 5;
      double hrs = 345.8;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, altMeters, mph);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.Latitude, "Null Latitude");
      Assert.IsNotNull(acs.Longitude, "Null Longitude");
      Assert.IsNotNull(acs.AltitudeMeters, "Null AltitudeMeters");
      Assert.IsNotNull(acs.SpeedMPH, "Null Speed");
      Assert.IsNotNull(acs.RuntimeHours, "Null RuntimeHours");
      Assert.AreEqual(lat, acs.Latitude.Value, "Latitude not updated");
      Assert.AreEqual(lon, acs.Longitude.Value, "Longitude not updated");
      Assert.AreEqual(altMeters, acs.AltitudeMeters.Value, "AltitudeMeters not updated");
      Assert.AreEqual(mph, acs.SpeedMPH.Value, "Speed not updated");
      Assert.AreEqual(hrs, acs.RuntimeHours.Value, "RuntimeHours not updated");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not Reporting and should be");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_LastTMSUTC_Success()
    {
      // ARRANGE //
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);
      DateTime tmsUtc = DateTime.UtcNow.AddMinutes(-10);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestSNM451).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      // Newer tms report (with the timestamp we expect to be in AssetCurrentStatus)
      var tms1 = new DataTirePressureMonitorSystem()
      {
        AssetID = testAsset.AssetID,
        AxlePosition = 1,
        TirePosition = 1,
        DeviceType = DeviceTypeEnum.SNM451,
        fk_DimSourceID = (int)DimSourceEnum.UserEntered,
        InsertUTC = DateTime.UtcNow,
        EventUTC = tmsUtc,
        fk_SensorTypeID = (int)SensorTypeEnum.Temperature,
        fk_TPMSSensorAspectsID = (int)SensorAspectsEnum.NoAlert,
        SensorValue = 23.7
      };
      Ctx.DataContext.DataTirePressureMonitorSystem.AddObject(tms1);

      // Older tms report
      var tms2 = new DataTirePressureMonitorSystem()
      {
        AssetID = testAsset.AssetID,
        AxlePosition = 1,
        TirePosition = 1,
        DeviceType = DeviceTypeEnum.SNM451,
        fk_DimSourceID = (int)DimSourceEnum.UserEntered,
        InsertUTC = DateTime.UtcNow,
        EventUTC = tmsUtc.AddMinutes(-1),
        fk_SensorTypeID = (int)SensorTypeEnum.Temperature,
        fk_TPMSSensorAspectsID = (int)SensorAspectsEnum.NoAlert,
        SensorValue = 24.7
      };
      Ctx.DataContext.DataTirePressureMonitorSystem.AddObject(tms2);
      Ctx.DataContext.SaveChanges();
      
      // ACT //
      Helpers.NHRpt.AssetCurrentStatus_Update();

      // ASSERT //
      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.LastTMSUTC, "No timestamp for last TMS report");
      Assert.AreEqual(tmsUtc, acs.LastTMSUTC.Value, "Incorrect timestamp for last TMS report");
    }
    
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_ManyUpdates_Success()
    {
      // ARRANGE //
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestCrossCheck).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      DateTime locUTC = DateTime.UtcNow.AddMinutes(-20);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.XCGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);

      locUTC = locUTC.AddSeconds(60);
      lat = 55.444;
      lon = -33.333;
      mph = 65.4;
      hrs = 2000.2;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.XCGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);

      locUTC = locUTC.AddSeconds(60);
      double totalConsumption = 999.9;
      double remaining = 39.0;
      double idleHrs = 12.32;
      double engIdleGals = 43.2;
      Helpers.NHData.EngineParameters_Add(testAsset.AssetID, locUTC, totalConsumption, idleHrs, engIdleGals, remaining);

      locUTC = locUTC.AddSeconds(60);
      DateTime lastStateUTC = locUTC;
      Helpers.NHData.IgnitionOnOff_Add(testAsset.AssetID, locUTC, true);

      locUTC = locUTC.AddSeconds(60);
      lat = 55.444;
      lon = -33.333;
      mph = 65.4;
      hrs = 2000.2;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.XCGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);

      DateTime tmsUtc = DateTime.UtcNow.AddMinutes(-10);
      var tms = new DataTirePressureMonitorSystem()
      {
        AssetID = testAsset.AssetID,
        AxlePosition = 1,
        TirePosition = 1,
        DeviceType = DeviceTypeEnum.SNM451,
        fk_DimSourceID = (int)DimSourceEnum.UserEntered,
        InsertUTC = DateTime.UtcNow,
        EventUTC = tmsUtc,
        fk_SensorTypeID = (int)SensorTypeEnum.Temperature,
        fk_TPMSSensorAspectsID = (int)SensorAspectsEnum.NoAlert,
        SensorValue = 23.7
      };
      Ctx.DataContext.DataTirePressureMonitorSystem.AddObject(tms);
      Ctx.DataContext.SaveChanges();

      // ACT //
      Helpers.NHRpt.AssetCurrentStatus_Update();

      // ASSERT //
      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.Latitude, "Null Latitude");
      Assert.IsNotNull(acs.Longitude, "Null Longitude");
      Assert.IsNotNull(acs.SpeedMPH, "Null Speed");
      Assert.IsNotNull(acs.RuntimeHours, "Null RuntimeHours");
      Assert.AreEqual(lat, acs.Latitude.Value, "Latitude not updated");
      Assert.AreEqual(lon, acs.Longitude.Value, "Longitude not updated");
      Assert.AreEqual(mph, acs.SpeedMPH.Value, "Speed not updated");
      Assert.AreEqual(hrs, acs.RuntimeHours.Value, "RuntimeHours not updated");
      Assert.IsNotNull(acs.FuelPercentRemaining, "Null FuelPercentRemaining");
      Assert.AreEqual((int)remaining, acs.FuelPercentRemaining.Value, "Fuel not updated");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AssetOn, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AssetOn and should be");
      Assert.IsNotNull(acs.LastTMSUTC, "No timestamp for last TMS report");
      Assert.AreEqual(tmsUtc, acs.LastTMSUTC.Value, "Incorrect timestamp for last TMS report");
      Assert.AreEqual(lastStateUTC, acs.LastStateUTC.Value, "Expect LastStateUTC to be utc of last Ignition message");
    }

    [Ignore]
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_Location_NoDevice_Success()
    {
      //ActiveUser me = AdminLogin();
      //SessionContext session = API.Session.Validate(me.SessionID);
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      //Asset testAsset = CreateAsset(session.NHOpContext, unitTestCustomerID.Value, "Joe");
      //CreateAssetSubscription(session, testAsset.ID);
      //PopulateDimTables();
      Asset testAsset = Entity.Asset.WithDevice(TestData.TestNoDevice).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      //DateTime locUTC = DateTime.UtcNow.AddSeconds(-1);
      //double lat = 12.321;
      //double lon = -98.543;
      //double mph = 43.5;
      //double hrs = 345.8;
      //AddDataHoursLocation(testAsset.ID, locUTC, hrs, lat, lon, mph);
      //ExecAssetCurrentStatusUpdate();
      DateTime locUTC = DateTime.UtcNow.AddSeconds(-1);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.None, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      //AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
      //                          where s.DimAsset.ID == testAsset.ID
      //                          select s).FirstOrDefault();
      //Assert.IsNotNull(acs, "No asset current status in DB");
      //Assert.IsNotNull(acs.Latitude, "Null lat");
      //Assert.IsNotNull(acs.Longitude, "Null Lon");
      //Assert.IsNotNull(acs.Speed, "Null speed");
      //Assert.IsNotNull(acs.RuntimeHours, "Null hours");
      //Assert.AreEqual(lat, acs.Latitude.Value, "Lat not updated");
      //Assert.AreEqual(lon, acs.Longitude.Value, "Lon not updated");
      //Assert.AreEqual(mph, Decimal.ToDouble(acs.Speed.Value), "sPEED not updated");
      //Assert.AreEqual(hrs, Decimal.ToDouble(acs.RuntimeHours.Value), "RT hrs not updated");
      //acs.DimAssetWorkingStateReference.Load();
      //Assert.AreEqual((int)AssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "Expect to be Reporting due to location");
      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.IsNotNull(acs.Latitude, "Null Latitude");
      Assert.IsNotNull(acs.Longitude, "Null Longitude");
      Assert.IsNotNull(acs.SpeedMPH, "Null Speed");
      Assert.IsNotNull(acs.RuntimeHours, "Null RuntimeHours");
      Assert.AreEqual(lat, acs.Latitude.Value, "Latitude not updated");
      Assert.AreEqual(lon, acs.Longitude.Value, "Longitude not updated");
      Assert.AreEqual(mph, acs.SpeedMPH.Value, "Speed not updated");
      Assert.AreEqual(hrs, acs.RuntimeHours.Value, "RuntimeHours not updated");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not Reporting and should be");
    }

    ///<summary>
    /// The case where there are Ignition/Engine events e.g. MTS522/523/XC
    ///  SupportedStates: AwaitingFirstReport/AssetOn/AssetOff/NotReporting
    ///  Note that must not go from assetOn to Reporting as AssetOn is more significant
    ///</summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_StateChangeWithECMBoardMTS_Success()
    {
      StateChangeWithECMBoardTestSequence(TestData.TestMTS522);
    }

    /// <summary>
    /// Same test as above, but testing with a PL420 (TM3000)
    /// </summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_StateChangeWithECMBoardPL420_Success()
    {
      StateChangeWithECMBoardTestSequence(TestData.TestPL420);
    }

    private void StateChangeWithECMBoardTestSequence(Device device)
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      CustomerRelationship cr = TestData.TestCustomerHierarchy;
      Asset testAsset = Entity.Asset.WithDevice(device).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNull(acs.LastReportedUTC, "LastReportedUTC was not null but should have been because no events were added");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AwaitingFirstReport and should be");

      // Ignition on - tho it's old: state = NotReporting
      Thread.Sleep(10);
      DateTime ignitionUTC = DateTime.UtcNow.AddDays(-30);
      Helpers.NHData.IgnitionOnOff_Add(testAsset.AssetID, ignitionUTC, true);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.NotReporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not NotReporting and should be");
      Assert.AreEqual(ignitionUTC, acs.LastStateUTC, "LastStateUTC incorrect (1)");

      // Ignition off -recent one: state = Reporting
      Thread.Sleep(10);
      ignitionUTC = DateTime.UtcNow.AddDays(-1);
      Helpers.NHData.IgnitionOnOff_Add(testAsset.AssetID, ignitionUTC, false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AssetOff, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AssetOff and should be");
      Assert.AreEqual(ignitionUTC, acs.LastStateUTC, "LastStateUTC incorrect (2)");

      // Ignition on : state = AssetOn
      Thread.Sleep(10);
      ignitionUTC = DateTime.UtcNow.AddSeconds(-8);
      Helpers.NHData.IgnitionOnOff_Add(testAsset.AssetID, ignitionUTC, true);
      Helpers.NHRpt.AssetCurrentStatus_Update();
      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();

      Assert.AreEqual((int)DimAssetWorkingStateEnum.AssetOn, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AssetOn and should be");
      Assert.AreEqual(ignitionUTC, acs.LastStateUTC, "LastStateUTC incorrect (3)");
    }

    ///<summary>
    /// The case where there are Ignition/Engine events e.g. PL with ECM possibly only gets EngineSS
    ///  SupportedStates: AwaitingFirstReport/Reporting/NotReporting
    ///</summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_StateChangeWithECMBoardPL_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL321).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNull(acs.LastReportedUTC, "LastReportedUTC was not null but should have been because no events were added");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AwaitingFirstReport and should be");

      // Ignition on - tho it's old: state = NotReporting
      Thread.Sleep(10);
      DateTime ignitionUTC = DateTime.UtcNow.AddDays(-30);
      Helpers.NHData.IgnitionOnOff_Add(testAsset.AssetID, ignitionUTC, true);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.NotReporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not NotReporting and should be");
      Assert.AreEqual(ignitionUTC, acs.LastStateUTC, "LastStateUTC incorrect (1)");

      // Ignition off -recent one: state = Reporting
      Thread.Sleep(10);
      ignitionUTC = DateTime.UtcNow.AddDays(-1);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, ignitionUTC, false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "Expect state of: Reporting (AssetOff not supported)");
      Assert.AreEqual(ignitionUTC, acs.LastStateUTC, "LastStateUTC incorrect (2)");

      // Ignition on : state = AssetOn
      Thread.Sleep(10);
      ignitionUTC = DateTime.UtcNow.AddSeconds(-8);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, ignitionUTC, true);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs3 = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                 where s.fk_DimAssetID == testAsset.AssetID
                                 select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs3.fk_DimAssetWorkingStateID, "Expect state of: Reporting (AssetOn not supported)");
      Assert.AreEqual(ignitionUTC, acs3.LastStateUTC, "LastStateUTC incorrect (3)");
    }

    /// <summary>
    /// The case where there are no Ignition/Engine events for PL121/321/TT
    ///  SupportedStates: AwaitingFirstReport/Reporting/NotReporting
    /// </summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_StateChangeNoECMBoardPL_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL321).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNull(acs.LastReportedUTC, "Shouldn't be any events yet");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AwaitingFirstReport and should be");

      DateTime locUTC = DateTime.UtcNow.AddDays(-8);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      Thread.Sleep(10);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.NotReporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not NotReporting and should be");
      Assert.AreEqual(locUTC, acs.LastStateUTC, "LastStateUTC incorrect (1)");

      locUTC = DateTime.UtcNow.AddDays(-1);
      Thread.Sleep(10);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not Reporting and should be");
      Assert.AreEqual(locUTC, acs.LastStateUTC, "LastStateUTC incorrect (2)");
    }

    /// <summary>
    /// The case where there are no Ignition/Engine events for MTS5xx
    ///  SupportedStates: AwaitingFirstReport/AssetOn/AssetOff/NotReporting
    /// </summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_StateChangeNoECMBoardMTS_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestMTS523).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNull(acs.LastReportedUTC, "Shouldn't be any events yet");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AwaitingFirstReport and should be");

      DateTime locUTC = DateTime.UtcNow.AddDays(-8);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      Thread.Sleep(10);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.NHSync, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph, locIsValid:false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.NotReporting, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not NotReporting and should be");
      Assert.AreEqual(locUTC, acs.LastStateUTC, "LastStateUTC incorrect (1)");

      locUTC = DateTime.UtcNow.AddDays(-1);
      Thread.Sleep(10);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.NHSync, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph, locIsValid:false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AssetOff, acs.fk_DimAssetWorkingStateID, "DimAssetWorkingState.ID is not AssetOff and should be");
      Assert.AreEqual(locUTC, acs.LastStateUTC, "LastStateUTC incorrect (2)");
    }

    /// <summary> 
    /// The case where FaultEvent & FaultDiagnostic events are included in LastReportedUTC 
    /// </summary>
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_LastReportedUTC_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);

      Asset testAsset = Entity.Asset.WithDevice(TestData.TestPL121).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();

      //FaultEvent
      DateTime feUTC = DateTime.UtcNow.AddMinutes(-60);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, 555, eventUTC:feUTC);

      DateTime locUTC = feUTC.AddMinutes(10);
      double lat = 12.321;
      double lon = -98.543;
      double mph = 43.5;
      double hrs = 345.8;
      // Adding invalid location
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.NHSync, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph, locIsValid:false);

      locUTC = locUTC.AddMinutes(10);
      // Valid location after 10 minutes of being in an invalid location
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.NHSync, locUTC, DateTime.UtcNow, hrs, lat, lon, speed:mph);

      feUTC = locUTC.AddMinutes(10);
      Helpers.NHData.DataFaultEvent_Add(testAsset.AssetID, 555, eventUTC: feUTC);

      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.AreEqual(locUTC.Date, acs.LastLocationUTC.Value.Date, "Last LocationUTC does not match");
      Assert.AreEqual(locUTC.Minute, acs.LastLocationUTC.Value.Minute, "Last LocationUTC does not match");
      Assert.AreEqual(feUTC.Date, acs.LastReportedUTC.Value.Date, "LastReportedUTC does not match");
      Assert.AreEqual(feUTC.Minute, acs.LastReportedUTC.Value.Minute, "LastReportedUTC does not match");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_GenSetReporting()
    {
      Customer customer1 = Entity.Customer.Dealer.SyncWithRpt().Save();
      Asset testAsset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer1.BSSID).Save()).SerialNumberVin("123ZZZGGG").MakeCode("CAT").ProductFamily("GENSET").WithCoreService().Save();

      Helpers.NHRpt.DimAsset_Populate();

      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be AwaitingFirstReport");

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PR3Gateway, DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow, 27, 33,44,null,null, true, 12, 12, 22);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.Reporting, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be Reporting");

      DateTime evtUTC = DateTime.UtcNow.AddMinutes(-8);
      Helpers.NHData.GensetOperationalState_Add(testAsset.AssetID, evtUTC, DimEngineStateEnum.EngineStopping);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, DateTime.UtcNow, false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.EngineStopping, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");

      evtUTC = DateTime.UtcNow.AddMinutes(-5);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PR3Gateway, evtUTC, DateTime.UtcNow, 27, 33, 44, null, null, true, 12, 12, 22);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.EngineStopping, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_GenSetNotReporting()
    {
      Customer customer1 = Entity.Customer.Dealer.SyncWithRpt().Save();
      Asset testAsset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer1.BSSID).Save()).SerialNumberVin("123ZZZGGG").MakeCode("CAT").ProductFamily("GENSET").WithCoreService().Save();

      Helpers.NHRpt.DimAsset_Populate();

      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.AwaitingFirstReport, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be AwaitingFirstReport");

      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PR3Gateway, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow, 27, 33, 44, null, null, true, 12, 12, 22);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimAssetWorkingStateEnum.NotReporting, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be Reporting");

      DateTime evtUTC = DateTime.UtcNow.AddMinutes(-8);
      Helpers.NHData.GensetOperationalState_Add(testAsset.AssetID, evtUTC, DimEngineStateEnum.EngineStopping);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, DateTime.UtcNow, false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.EngineStopping, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");

      evtUTC = DateTime.UtcNow.AddMinutes(-5);
      Helpers.NHData.DataHoursLocation_Add(testAsset.AssetID, DimSourceEnum.PR3Gateway, evtUTC, DateTime.UtcNow, 27, 33, 44, null, null, true, 12, 12, 22);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.EngineStopping, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateAssetCurrentStatus_LatestPTOEngine()
    {
      Asset testAsset = Entity.Asset.WithDevice(TestData.TestMTS523).WithCoreService().Save();
      
      Helpers.NHRpt.DimAsset_Populate();
      DataCustomUtilizationEvent p1 = Helpers.NHData.CustomUtilizationEvent_Add(testAsset.AssetID, DateTime.UtcNow.AddMinutes(-1), DimCustomUtilizationEventTypeEnum.TotalEnginePowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown,15);
      DataCustomUtilizationEvent p2 = Helpers.NHData.CustomUtilizationEvent_Add(testAsset.AssetID, DateTime.UtcNow.AddMinutes(-3), DimCustomUtilizationEventTypeEnum.TotalEnginePowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 10);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.AreEqual(p1.EventUTC.Date, acs.LastEnginePTOHoursMeterUTC.Value.Date, "Last LocationUTC does not match");
      Assert.AreEqual(p1.EventUTC.Minute, acs.LastEnginePTOHoursMeterUTC.Value.Minute, "Last LocationUTC does not match");
      Assert.AreEqual(p1.Value, acs.EnginePTOHoursMeter, "EnginePTOHoursMeter does not match");
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateAssetCurrentStatus_LatestPTOTransmission()
    {
      Asset testAsset = Entity.Asset.WithDevice(TestData.TestMTS523).WithCoreService().Save();
      Helpers.NHRpt.DimAsset_Populate();
      DataCustomUtilizationEvent p1 = Helpers.NHData.CustomUtilizationEvent_Add(testAsset.AssetID, DateTime.UtcNow.AddMinutes(-3), DimCustomUtilizationEventTypeEnum.TotalTransmissionPowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown, 10);
      DataCustomUtilizationEvent p3 = Helpers.NHData.CustomUtilizationEvent_Add(testAsset.AssetID, DateTime.UtcNow.AddMinutes(-1), DimCustomUtilizationEventTypeEnum.TotalTransmissionPowerTakeOffHours, DimSourceEnum.DataIn, DimUnitTypeEnum.Hour, OEMDataSourceTypeEnum.Unknown,20);
     
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in Ctx.RptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.AreEqual(p3.EventUTC.Date, acs.LastTransmissionPTOHoursMeterUTC.Value.Date, "Last LocationUTC does not match");
      Assert.AreEqual(p3.EventUTC.Minute, acs.LastTransmissionPTOHoursMeterUTC.Value.Minute, "Last LocationUTC does not match");
      Assert.AreEqual(p3.Value, acs.TransmissionPTOHoursMeter, "TransmissionPTOHoursMeter does not match");
    }
    
    [TestMethod]
    [DatabaseTest]
    public void AssetCurrentStatusUpdatedWithNewGensetRunning_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);
      Customer customer1 = Entity.Customer.Dealer.SyncWithRpt().Save();      
      Asset testAsset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer1.BSSID).Save()).SerialNumberVin("123ZZZGGG").MakeCode("CAT").ProductFamily("GENSET").WithCoreService().Save();
      
      Helpers.NHRpt.DimAsset_Populate();

      DateTime evtUTC = DateTime.UtcNow.AddSeconds(-1);
      Helpers.NHData.GensetOperationalState_Add(testAsset.AssetID, evtUTC, DimEngineStateEnum.Running);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, DateTime.UtcNow, true);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                where s.fk_DimAssetID == testAsset.AssetID
                                select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.Running, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");      
    }

    [TestMethod]
    [DatabaseTest]
    public void AssetCurrentStatusUpdatedWithNewGensetEngineStopping_Success()
    {
      VSS.Hosted.VLCommon.ActiveUser me = TestData.CustomerAdminActiveUser;
      SessionContext session = Helpers.Sessions.GetContextFor(me);
      Customer customer1 = Entity.Customer.Dealer.SyncWithRpt().Save();   
      Asset testAsset = Entity.Asset.WithDevice(Entity.Device.PL421.OwnerBssId(customer1.BSSID).Save()).SerialNumberVin("123ZZZGGG").MakeCode("CAT").ProductFamily("GENSET").WithCoreService().Save();

      Helpers.NHRpt.DimAsset_Populate();

      DateTime evtUTC = DateTime.UtcNow.AddSeconds(-1);
      Helpers.NHData.GensetOperationalState_Add(testAsset.AssetID, evtUTC, DimEngineStateEnum.EngineStopping);
      Helpers.NHData.EngineStartStop_Add(testAsset.AssetID, DateTime.UtcNow, false);
      Helpers.NHRpt.AssetCurrentStatus_Update();

      AssetCurrentStatus acs = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
             where s.fk_DimAssetID == testAsset.AssetID
             select s).FirstOrDefault();
      Assert.IsNotNull(acs, "No asset current status in DB");
      Assert.AreEqual((int)DimEngineStateEnum.EngineStopping, acs.fk_DimAssetWorkingStateID, "AssetWorkingState should be GensetState");
    }

    #region ManualMaintenanceWatch
    [DatabaseTest]
    [TestMethod]
    public void UpdateAssetCurrentStatus_MMW()
    {
      var customer = Entity.Customer.EndCustomer.Name("UpdateAssetCurrentStatus_MMW_Customer").BssId("BSS123").Save();
      var user = Entity.ActiveUser.ForUser(Entity.User.ForCustomer(customer).LastName("UpdateAssetCurrentStatus_MMW_User").Save()).Save();

      // bug in UnitTestHelper which doesn't allow adding serviceViews for noDevice types.
      // need to do it manually straignt to NH_RPT
      var asset = Entity.Asset.Name("AAA")
          .WithDevice(Entity.Device.NoDevice.OwnerBssId(customer.BSSID).Save())
          .WithDefaultAssetUtilizationSettings()
          .SyncWithRpt().Save();

      Helpers.NHRpt.DimAsset_Populate();
      var session = Helpers.Sessions.GetContextFor(user, true, true);

      var customerID = (from c in session.NHRptContext.DimCustomerReadOnly
                                 where customer.Name == customer.Name
                                 select c.ID).FirstOrDefault();

      DimServiceView dimServiceView = new DimServiceView
      {
        BSSLineID = "blah",
        fk_DimAssetID = asset.AssetID,
        fk_DimServiceTypeID = (int)ServiceTypeEnum.ManualMaintenanceLog,
        fk_StartKeyDate = 20090101,
        fk_EndKeyDate = 99991231,
        fk_DimCustomerID = customerID
      };
      session.NHRptContext.DimServiceView.AddObject(dimServiceView);
      session.NHRptContext.SaveChanges();

     
      // imitate a  API.Maintenance.UpdateRuntimeHoursForManualMaintenance()  
      // setup one week elapsed and count weekdays for which expectedRT would be accumulated given default of sun=0; mon=8; 8;8;8;8;0
      DateTime locUTC = DateTime.UtcNow.AddDays(-7);

      // actualRuntime readings have locations hardcoded to a MST timezone (currently), therefore estmates will be done after EndOfDay in MST timezone
      System.TimeZoneInfo mst = System.TimeZoneInfo.FindSystemTimeZoneById("Mountain Standard Time");
      DateTime locMST = System.TimeZoneInfo.ConvertTimeFromUtc(locUTC, mst);

      // I think this 5/4 kludge is done because the ETL doesn't include the first and last days. 
      // My guess is this is because the first day was included in last calc and the last one 'now' may not yet be complete (business rule?)
      int numberWeekdaysInWeek = locMST.DayOfWeek == System.DayOfWeek.Saturday || locMST.DayOfWeek == System.DayOfWeek.Sunday ? 5 : 4;
      double? lat = 39.8366667; //Lat long for WestMinster colarodo is hard coded for now
      double? lon = -105.0366667;
      double? hrs = 50;

      Helpers.NHData.DataHoursLocation_Add(asset.AssetID, DimSourceEnum.PLIPGateway, locUTC, DateTime.UtcNow, hrs, lat, lon);
      Helpers.NHRpt.LatestEvents_Populate(); // copies DataHoursLocation to NH_RPT..HoursLocation
      Helpers.NHRpt.AssetCurrentStatus_Update(); // updates the TZBias which is used to determine the devices day

      AssetCurrentStatus acsRead = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                                    where s.fk_DimAssetID == asset.AssetID
                                    select s).FirstOrDefault();
      Assert.IsNotNull(acsRead, "No ACS in DB");
      Assert.AreEqual(hrs, acsRead.RuntimeHours.Value, "Null ACS RuntimeHours");
      Assert.AreNotEqual(0, acsRead.TZBiasMinutes, "TZBias shouldn't be zero");
      DateTime beforeUTC = acsRead.LastRuntimeHoursUTC.Value;

      HoursLocation hl = (from s in session.NHRptContext.HoursLocationReadOnly
                          where s.ifk_DimAssetID == asset.AssetID
                                    orderby s.EventUTC descending
                                    select s).FirstOrDefault();
      Assert.IsNotNull(hl, "No HoursLocation in DB");
      Assert.AreEqual(lat, hl.Latitude.Value, "incorrect HL Latitude");
      Assert.AreEqual(lon, hl.Longitude.Value, "incorrect HL Longitude");
      Assert.AreEqual(hrs, hl.RuntimeHoursMeter.Value, "incorrect HL RuntimeHours");

      // now run the ETL and see if it's incremented the ACS runtimeHours according to ExpectedRTimes (which default to 0;8;8...)
      Helpers.NHRpt.AssetCurrentStatus_UpdateMMW();
      DateTime afterUTC = DateTime.UtcNow;
      acsRead = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                 where s.fk_DimAssetID == asset.AssetID
                 select s).FirstOrDefault();
      Assert.IsNotNull(acsRead, "No asset current status in DB");
      Assert.AreEqual(( hrs + (8*numberWeekdaysInWeek)), acsRead.RuntimeHours.Value, "RuntimeHours not incremented correctly");
      Assert.IsTrue(acsRead.LastRuntimeHoursUTC.Value >= beforeUTC, "RuntimeHoursUTC should be later than when ETL started");
      Assert.IsTrue(acsRead.LastRuntimeHoursUTC.Value <= afterUTC.AddMinutes(+1), "RuntimeHoursUTC should be earlier than when ETL started");

      // not due for another update
      Helpers.NHRpt.AssetCurrentStatus_UpdateMMW();
      AssetCurrentStatus acsRead2 = (from s in session.NHRptContext.AssetCurrentStatusReadOnly
                 where s.fk_DimAssetID == asset.AssetID
                 select s).FirstOrDefault();
      Assert.IsNotNull(acsRead2, "No asset current status in DB");
      Assert.AreEqual(acsRead.RuntimeHours.Value, acsRead2.RuntimeHours.Value, "RuntimeHours shouldn't have changed");
      Assert.AreEqual(acsRead.LastRuntimeHoursUTC.Value, acsRead2.LastRuntimeHoursUTC.Value, "RuntimeHoursUTC shouldn't have changed");
    }
    #endregion
  }
}
