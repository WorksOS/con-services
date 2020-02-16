using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Data.Objects;
using System.Linq.Expressions;
using EM=VSS.Nighthawk.EntityModels;
using System.Transactions;
using VSS.Nighthawk.ServicesAPI;
using System.Linq;
using VSS.Nighthawk.NHAdminServices;
using VSS.Nighthawk.NHCommonServices;
using VSS.Nighthawk.NHWebServices;

namespace UnitTests
{
    /// <summary>
    ///This is a test class for DeviceConfigSvcTest and is intended
    ///to contain all DeviceConfigSvcTest Unit Tests
    ///</summary>
  [TestClass()]
  public class CrossCheckConfigSvcTest : ServerAPITestBase
  {
    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion

    /****Default config parameters****/

    //Set general config info
    protected ushort deviceShutdownDelaySeconds = 900;
    protected ushort mdtShutdownDelaySeconds = 60;
    protected bool alwaysOnDevice = false;
    //Set site entry/exit config
    protected byte entryHomeZone = 15;
    protected byte entryHomeSite = 15;
    protected byte exitHomeZone = 12;
    protected byte exitHomeSite = 12;
    protected byte hysteresisHomeZoneSeconds = 4;
    protected byte hysteresisHomeSiteSeconds = 4;
    //Set speeding threshold config
    protected double speedingThreshold = 0;
    protected long speedingDuration = 0;
    protected bool speedingEnabled = false;
    //Set stopped threshold config
    protected double stopThreshold = 0;
    protected long stopDuration = 120;    //Default for on-road
    protected bool stopEnabled = true;
    //Set moving config
    protected ushort movingRadius = 300;  //Default radius for on-road
    //Set DriverID config
    protected bool driverIDEnabled = true;
    protected bool enableMDTDriverEntry = true;
    protected bool forceEntryAndLogOut = false;   //No use for Construction Services - set to factory default
    protected DriverIDCharSet charSet = DriverIDCharSet.AlphaNumeric;
    protected byte mdtIDMax = 12;
    protected byte mdtIDMin = 4;
    protected byte displayedListSize = 0;
    protected byte storedListSize = 0;
    protected bool forcedLogon = true;     
    protected bool autoLogoutInvalid = false;   //No use for Construction Services - set to factory default
    protected bool autoLogout = true;
    protected TimeSpan autoLogoutTime = new TimeSpan(0, 1, 0);  //1 minute
    protected bool expireMRU = true;
    protected TimeSpan mruExpiry = new TimeSpan(0, 0, 30);  //30 seconds
    protected bool expireUnvalidatedMRUs = true;
    protected TimeSpan unvalidatedExpiry = new TimeSpan(0, 0, 30);  //30 seconds
    protected bool displayMechanic = false;
    protected string mechanicID = "";
    protected string mechanicDisplayName = "";
    protected bool enableLoggedIn = true;     //No use for Construction Services - set to factory default
    protected byte loggedInoutputPolarity = 0;    //No use for Construction Services - set to factory default

    /****End Default config parameters****/    

    /// <summary>
    ///A test for ConfigureCrossCheckForCustomer
    ///</summary>
    [TestMethod()]
    public void ConfigureCrossCheckForCustomerTest()
    {
      using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
      {
        EM.ActiveUser me = AdminLogin();
        SessionContext session = API.Session.Validate(me.SessionID);
        
        string sessionID = me.SessionID;

        CrossCheckConfigSvc target = new CrossCheckConfigSvc(); // TODO: Initialize to an appropriate value

        long? customerID = FindCustomer(session.NHOpContext, unitTestCustomer);

        string gpsDeviceID1 = "555555";
        string gpsDeviceID2 = "666666";
        List<string> gpsDeviceIDs = new List<string>();
        gpsDeviceIDs.Add(gpsDeviceID1);
        gpsDeviceIDs.Add(gpsDeviceID2);

        EM.ServicePlan newServicePlan = EM.Model.Load<EM.ServicePlan>(session.NHOpContext, (int) EM.ServiceTypeEnum.VLCORE);
        
        bool driverIdEnabled = true;
        
        ServicePlanDetails newServicePlanDetails = new ServicePlanDetails()
        {         
          alertsEnabled = true,
          messagingEnabled = true,
         };

        foreach (string gpsDeviceID in gpsDeviceIDs)
        {
          string validGpsDeviceID = target.ConfigureCrossCheckForCustomer(sessionID, customerID.Value, gpsDeviceID, driverIdEnabled, newServicePlanDetails);
          Assert.AreEqual(gpsDeviceID, validGpsDeviceID, "Error sending config to CrossCheck");

          EM.MTSDevice deviceInRAW = EM.Model.Load<EM.MTSDevice>(session.NHRawContext, gpsDeviceID);

          //Check for device creation and service plan update in NH_RAW
          Assert.AreEqual(deviceInRAW.SerialNumber, gpsDeviceID, string.Format("Could not find device with gpsDeviceID:{0} in NH_RAW", gpsDeviceID));
          Assert.AreEqual(EM.AssetSubscription.DefaultSamplingInterval.TotalSeconds, deviceInRAW.SampleRate, string.Format("Sample rate does not match for gpsDeviceID:{0} in NH_RAW", gpsDeviceID));
          Assert.AreEqual(EM.AssetSubscription.DefaultReportingInterval.TotalSeconds, deviceInRAW.UpdateRate, string.Format("Update rate does not match for gpsDeviceID:{0} in NH_RAW", gpsDeviceID));
          Assert.AreEqual(EM.AssetSubscription.DefaultLowPowerInterval.TotalSeconds, deviceInRAW.LowPowerRate, string.Format("Low Power rate does not match for gpsDeviceID:{0} in NH_RAW", gpsDeviceID));
          deviceInRAW.DeviceStateReference.Load();
          Assert.AreEqual(deviceInRAW.DeviceState.ID, (int)EM.DeviceStateEnum.Subscribed, string.Format("DeviceState does not match for gpsDeviceID:{0} in NH_RAW", gpsDeviceID));

          //Check for device creation and service plan update in NH_OP          
          EM.Device deviceInOP = (from d in session.NHOpContext.Device
                            where d.GpsDeviceID == gpsDeviceID
                            select d).FirstOrDefault<EM.Device>();

          if (deviceInOP != null)
            Assert.AreEqual(deviceInOP.GpsDeviceID, gpsDeviceID, string.Format("Could not find device with gpsDeviceID:{0} in NH_OP", gpsDeviceID));

          EM.Asset matchingAsset = (from d in session.NHOpContext.Device
                                 where d.GpsDeviceID == gpsDeviceID
                                 select d.Asset).FirstOrDefault<EM.Asset>();
          Assert.IsNotNull(matchingAsset, string.Format("No newly created matching asset found for gpsDeviceID:{0}", gpsDeviceID));
        }
      }
    }    
  }
}
