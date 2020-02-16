using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Nighthawk.EntityModels;
using VSS.Nighthawk.NHOPSvc.SiteDeterminationSvc;
using VSS.Nighthawk.ServicesAPI;
using VSS.Nighthawk.Utilities;
using System.Threading;

namespace UnitTests
{
  [TestClass()]
  public class FuelLossCandidateTests : ReportLogicTestBase
  {

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //  devFilePath = System.Environment.CurrentDirectory;
    //  hoursSun = 0;
    //  hoursMon = 8;
    //  hoursTue = 8;
    //  hoursWed = 8;
    //  hoursThu = 8;
    //  hoursFri = 8;
    //  hoursSat = 0;

    //  idleBurnRate = 5.0;
    //  workingBurnRate = 10.0;
    //}

    #endregion

    #region PL321
    [TestMethod()]
    public void PL321withMatchingFuelWhichResultsInCandidate()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    EngineStop 
       * 
         2 May 2010 10:00    EnginStart
         2 May 2010 10:03    EngineParameter        10      (i.e. loss of 3%)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.PL321, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.CATUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopEngineUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartEngineUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopEngineUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.PLIPGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataEngineStartStop ignition1 = DataEngineStartStop.CreateDataEngineStartStop(-1, DateTime.UtcNow, StopEngineUTC, asset.ID, false, (int)NhDataSourceEnum.PLIPGateway);

        DataEngineStartStop ignition2 = DataEngineStartStop.CreateDataEngineStartStop(-1, DateTime.UtcNow, StartEngineUTC, asset.ID, true, (int)NhDataSourceEnum.PLIPGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartEngineUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.PLIPGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;

        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataEngineStartStop(ignition1);
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataEngineStartStop(ignition2);
        dataCtx.SaveChanges();


        // check for FuelLossCandidates
        ExecuteFuelLossCandidatePopulate();
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for PL32121");
        Assert.AreEqual(1, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");

        Assert.AreEqual(10, fuelLossCandidateList[0].EngineOnFuelLevel, string.Format("FuelLevel at Engine Start is wrong for Day:{0}", fuelLossCandidateList[0].EngineOnUTC));
        Assert.AreEqual(13, fuelLossCandidateList[0].EngineOffFuelLevel, string.Format("FuelLevel at Engine Stop is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
        Assert.AreEqual(3, fuelLossCandidateList[0].FuelLossPercent, string.Format("FuelLoss is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
      }
    }

    [TestMethod()]
    public void PL321withMatchingFuelWhichResultsInCandidate_Delay()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    EngineStop 
       * 
         2 May 2010 10:00    EnginStart
       * run ETL
         2 May 2010 10:03    EngineParameter        10      (i.e. loss of 3%)
       * run ETL
         2 May 2010 10:02    EngineParameter        12      (should just ignore this late one)
       * run ETL
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.PL321, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.CATUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopEngineUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartEngineUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopEngineUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.PLIPGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataEngineStartStop ignition1 = DataEngineStartStop.CreateDataEngineStartStop(-1, DateTime.UtcNow, StopEngineUTC, asset.ID, false, (int)NhDataSourceEnum.PLIPGateway);
        DataEngineStartStop ignition2 = DataEngineStartStop.CreateDataEngineStartStop(-1, DateTime.UtcNow, StartEngineUTC, asset.ID, true, (int)NhDataSourceEnum.PLIPGateway);
        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataEngineStartStop(ignition1);
        dataCtx.AddToDataEngineStartStop(ignition2);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        Thread.Sleep(50); // Need to emulate elapsed time between exes of the update sproc...
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartEngineUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.PLIPGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        Thread.Sleep(50); // Need to emulate elapsed time between exes of the update sproc...
        DataEngineParameters ep3 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartEngineUTC.AddMinutes(2), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.PLIPGateway);
        ep3.MID = 20;
        ep3.LevelPercent = 12;
        dataCtx.AddToDataEngineParameters(ep3);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for PL32121");
        Assert.AreEqual(1, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");

        Assert.AreEqual(10, fuelLossCandidateList[0].EngineOnFuelLevel, string.Format("FuelLevel at Engine Start is wrong for Day:{0}", fuelLossCandidateList[0].EngineOnUTC));
        Assert.AreEqual(13, fuelLossCandidateList[0].EngineOffFuelLevel, string.Format("FuelLevel at Engine Stop is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
        Assert.AreEqual(3, fuelLossCandidateList[0].FuelLossPercent, string.Format("FuelLoss is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
      }
    }
    #endregion

    #region MTS521
    [TestMethod()]
    public void MTS521withMatchingFuelWhichResultsInCandidate()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
         2 May 2010 10:00    IgnitionOn 
         2 May 2010 10:03    EngineParameter        10      (i.e. loss of 3%)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.STDUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);

        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10; 
                
        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();


        // check for FuelLossCandidates
        ExecuteFuelLossCandidatePopulate();
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                            where fu.DimAsset.ID == asset.ID
                                            select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.AreEqual(1, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");

        Assert.AreEqual(10, fuelLossCandidateList[0].EngineOnFuelLevel, string.Format("FuelLevel at Ignition on is wrong for Day:{0}", fuelLossCandidateList[0].EngineOnUTC));
        Assert.AreEqual(13, fuelLossCandidateList[0].EngineOffFuelLevel, string.Format("FuelLevel at Ignition off is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
        Assert.AreEqual(3, fuelLossCandidateList[0].FuelLossPercent, string.Format("FuelLoss is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
      }
    }

    [TestMethod()]
    public void MTS521PotentialCandidateButNoValidSubscription()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
         2 May 2010 10:00    IgnitionOn 
         2 May 2010 10:03    EngineParameter        10      (potential loss of 3%, but wrong subscription)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.CATUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);

        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;

        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();


        // check for FuelLossCandidates
        ExecuteFuelLossCandidatePopulate();
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.IsTrue(fuelLossCandidateList.Count() == 0, "Incorrect count of fuelLossCandidate records.");
       }
    }

    [TestMethod()]
    public void MTS521withMatchingFuelWhichResultsInCandidate_OutOfOrder()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
       * The following events are however received and processed BEFORE the IgnitionOff (can happen that all events not received till power back on)
         2 May 2010 10:00    IgnitionOn 
         2 May 2010 10:03    EngineParameter        10      (i.e. loss of 3%)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.STDUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // in 2 batches, add events into NH_DATA and process
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
       
        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();
        Thread.Sleep(50); // Need to emulate elapsed time between exes of the update sproc...

        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);
        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();


        // check for FuelLossCandidates
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.AreEqual(1, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");

        Assert.AreEqual(10, fuelLossCandidateList[0].EngineOnFuelLevel, string.Format("FuelLevel at Ignition on is wrong for Day:{0}", fuelLossCandidateList[0].EngineOnUTC));
        Assert.AreEqual(13, fuelLossCandidateList[0].EngineOffFuelLevel, string.Format("FuelLevel at Ignition off is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
        Assert.AreEqual(3, fuelLossCandidateList[0].FuelLossPercent, string.Format("FuelLoss is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
      }
    }

    [TestMethod()]
    public void MTS521withMatchingFuelNoCandidate()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
         2 May 2010 10:00    IgnitionOn 
         2 May 2010 10:03    EngineParameter        99      (i.e. no loss)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.STDUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);

        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 99;

        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();


        // check for FuelLossCandidates
        ExecuteFuelLossCandidatePopulate();
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.AreEqual(0, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");
      }
    }

    [TestMethod()]
    public void MTS521withNoMatchingFuelNoCandidate()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
         2 May 2010 10:00    IgnitionOn 
         2 May 2010 10:06    EngineParameter        10      (fuel too long after the ignitionOn to be considered)
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.STDUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);

        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(6), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;

        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();


        // check for FuelLossCandidates
        ExecuteFuelLossCandidatePopulate();
        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.AreEqual(0, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");        
      }
    }

    [TestMethod()]
    public void MTS521withMatchingFuelWhichResultsInCandidate_Delay()
    {
      /* this is the scenario covered:
         EventUTC           EventType          LevelPercent 
         1 May 2010 06:59    EngineParameter        13
         1 May 2010 07:00    IgnitionOff 
       * 
         2 May 2010 10:00    IgnitionOn
       * run ETL
         2 May 2010 10:03    EngineParameter        10      (i.e. loss of 3%)
       * run ETL
         2 May 2010 10:02    EngineParameter        12      (should just ignore this late one)
       * run ETL
       */
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {

        ActiveUser admin = AdminLogin();
        SessionContext session = API.Session.Validate(admin.SessionID);
        NH_DATA dataCtx = Model.NewNHContext<NH_DATA>();

        //Create an asset with device in NH_OP and NH_RPT
        string gpsDeviceID = "888888";
        Asset asset = CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, DeviceTypeEnum.MTS521, DateTime.UtcNow);
        Assert.IsNotNull(asset, "Failed to create Test Asset");
        CreateAssetSubscription(session, asset.ID, ServiceTypeEnum.STDUTIL, new DateTime(2010, 5, 1, 00, 00, 00), null);
        PopulateDimTables();


        // add all the events into NH_DATA
        DateTime StopIgnUTC = new DateTime(2010, 5, 1, 07, 00, 00);
        DateTime StartIgnUTC = new DateTime(2010, 5, 2, 10, 00, 00);
        DataEngineParameters ep1 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StopIgnUTC.AddMinutes(-1), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep1.MID = 10;
        ep1.LevelPercent = 13;
        DataIgnOnOff ignition1 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StopIgnUTC, asset.ID, false, 0.0, (int)NhDataSourceEnum.MTSGateway);
        DataIgnOnOff ignition2 = DataIgnOnOff.CreateDataIgnOnOff(-1, DateTime.UtcNow, StartIgnUTC, asset.ID, true, 0.0, (int)NhDataSourceEnum.MTSGateway);
        dataCtx.AddToDataEngineParameters(ep1);
        dataCtx.AddToDataIgnOnOff(ignition1);
        dataCtx.AddToDataIgnOnOff(ignition2);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        Thread.Sleep(50); // Need to emulate elapsed time between exes of the update sproc...
        DataEngineParameters ep2 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(3), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep2.MID = 20;
        ep2.LevelPercent = 10;
        dataCtx.AddToDataEngineParameters(ep2);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        Thread.Sleep(50); // Need to emulate elapsed time between exes of the update sproc...
        DataEngineParameters ep3 = DataEngineParameters.CreateDataEngineParameters(-1, DateTime.UtcNow, StartIgnUTC.AddMinutes(2), asset.ID, DateTime.UtcNow, (int)NhDataSourceEnum.MTSGateway);
        ep3.MID = 20;
        ep3.LevelPercent = 12;
        dataCtx.AddToDataEngineParameters(ep3);
        dataCtx.SaveChanges();
        ExecuteFuelLossCandidatePopulate();

        List<FuelLossCandidate> fuelLossCandidateList = (from fu in session.NHRptContext.FuelLossCandidate
                                                         where fu.DimAsset.ID == asset.ID
                                                         select fu).ToList<FuelLossCandidate>();
        Assert.IsNotNull(fuelLossCandidateList, "Failed to Insert Fuelloss for MTS521");
        Assert.AreEqual(1, fuelLossCandidateList.Count(), "Incorrect count of fuelLossCandidate records.");

        Assert.AreEqual(10, fuelLossCandidateList[0].EngineOnFuelLevel, string.Format("FuelLevel at Engine Start is wrong for Day:{0}", fuelLossCandidateList[0].EngineOnUTC));
        Assert.AreEqual(13, fuelLossCandidateList[0].EngineOffFuelLevel, string.Format("FuelLevel at Engine Stop is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
        Assert.AreEqual(3, fuelLossCandidateList[0].FuelLossPercent, string.Format("FuelLoss is wrong for Day:{0}", fuelLossCandidateList[0].EngineOffUTC));
      }
    }
    #endregion

    #region Privates
    private long CreateDevice(SessionContext session, string gpsDeviceID, string externalDeviceID, DeviceTypeEnum deviceType, out Asset asset)
    {
      Device newDevice;
      asset = this.CreateAssetWithDevice(session, session.CustomerID.Value, gpsDeviceID, deviceType, DateTime.UtcNow, out newDevice);
      Assert.IsNotNull(asset, "Failed to create asset");
      return newDevice.ID;
    }

    private bool UpdateWorkingDefinition(NH_OP dataContext, long assetID, WorkingDefinitionEnum workDefn, int sensorNumber, bool sensorStartIsOn, DateTime updateUTC,
          decimal hoursSun, decimal hoursMon, decimal hoursTue, decimal hoursWed, decimal hoursThu, decimal hoursFri, decimal hoursSat,
          double idleBurnRate, double workingBurnRate)
    {
      AssetUtilization assetUtil = (from a in dataContext.AssetUtilization
                                    where a.Asset.ID == assetID
                                    select a).FirstOrDefault<AssetUtilization>();
      if (assetUtil == null)
      {
        assetUtil = AssetUtilization.CreateAssetUtilization(hoursSun, hoursMon, hoursTue, hoursWed, hoursThu, hoursFri, hoursSat, sensorNumber, sensorStartIsOn, updateUTC, updateUTC, new DateTime(2009, 1, 1), -1);
        assetUtil.WorkDefinitionReference.EntityKey = Model.GetEntityKey<WorkDefinition>(dataContext, (int)workDefn);

        assetUtil.AssetReference.EntityKey = Model.GetEntityKey<Asset>(dataContext, assetID);
        dataContext.AddToAssetUtilization(assetUtil);
      }
      else
      {
        assetUtil.WorkDefinitionReference.EntityKey = Model.GetEntityKey<WorkDefinition>(dataContext, (int)workDefn);
        assetUtil.SensorNumber = sensorNumber;
        assetUtil.SensorStartIsOn = sensorStartIsOn;

        assetUtil.HoursMon = hoursMon;
        assetUtil.HoursTue = hoursTue;
        assetUtil.HoursWed = hoursWed;
        assetUtil.HoursThu = hoursThu;
        assetUtil.HoursFri = hoursFri;
        assetUtil.HoursSat = hoursSat;
        assetUtil.HoursSun = hoursSun;
        assetUtil.UpdateUTC = updateUTC;
      }

      assetUtil.EstimatedIdleBurnRateGallonsPerHour = (decimal)idleBurnRate;
      assetUtil.EstimatedWorkingBurnRateGallonsPerHour = (decimal)workingBurnRate;

      int saved = dataContext.SaveChanges();
      return saved > 0;
    }
    #endregion
  }
 }
