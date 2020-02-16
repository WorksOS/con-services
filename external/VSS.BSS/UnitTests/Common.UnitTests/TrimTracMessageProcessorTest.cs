using VSS.Nighthawk.TrimTracGateway;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using VSS.Nighthawk.GatewayLibrary;
using VSS.Nighthawk.EntityModels;
using System.Text;
using VSS.Nighthawk.ServicesAPI;
using System.Linq;

namespace UnitTests
{
    
    
    /// <summary>
    ///This is a test class for TrimTracMessageProcessorTest and is intended
    ///to contain all TrimTracMessageProcessorTest Unit Tests
    ///</summary>
  [TestClass()]
  public class TrimTracMessageProcessorTest : UnitTestBase
  {

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    /// <summary>
    ///A test for ProcessMessage
    ///</summary>
    [TestMethod()]
    [DeploymentItem("VSS.Nighthawk.TrimTracGateway.exe")]
    public void ProcessMessageTest()
    {
      //Define a Trimtrac test device
      DeviceAPI targetApi = new DeviceAPI();
      string IMEI = "011074006343775";
      string unitID = "Y2634377";    
      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;

      //Activate the deivce
      TTDevice mapTTDevice = new TTDevice();
      targetApi.CreateTTDevice(Ctx.RawContext, unitID, IMEI, expectedState, out mapTTDevice);
      //mapTTDevice.DeviceState = new DeviceStateRaw();
      DeviceStateRaw dr = new DeviceStateRaw();
      dr.TTDevice.Add(mapTTDevice);                   
      Ctx.RawContext.SaveChanges();

      //Define the payloads that are going to be passed
      ///Simulation scenario
      ///One RTKS will be sent from the device following which a QTKM will be sent to the device
      ///The device will then respond with an RTKM
      ///The QTKM message status will change from pending to sent to acknowledged in the meanwhile
      string RTKSpayload = ">RTKS00000100F0000000000000000100000;ID=Y2634377;*15<";
      string QTKMpayload = ">QTKM11;PW=00000000;ID=Y2634377;*4F<";
      string RTKMpayload = ">RTKM1100000000000000000000;ID=Y2634377;*4D<";      
     string RTKApayload = ">RTKA0287100003000003000215100000201682000111042518408314;ID=Y2634377;*4A<";
     string STKApayload = ">STKA028710000300000300021510000020168200011;PW=00000000;ID=Y2634377;*75<";

      TrimTracMessageProcessor_Accessor target = new TrimTracMessageProcessor_Accessor();
      object sender = new object();

      //Issue the QTKM in the TTOut table and check for the status. status should be pending
      InsertIntoTTOut(unitID, QTKMpayload);   
      AssertTTOutRow(QTKMpayload);
      AssertTTOutStatus(QTKMpayload, 0);

      //Issue the QTKM in the TTOut table and check for the status. status should be pending
      InsertIntoTTOut(unitID, STKApayload);
      AssertTTOutRow(STKApayload);
      AssertTTOutStatus(STKApayload, 0);

      //Simulate RTKS input
      ReceivedDataEventArgs args = new ReceivedDataEventArgs { ReceivedOnPort = 1121, isTCP = true, EndPoint = "1.2.3.4.5", Buffer = Encoding.ASCII.GetBytes(RTKSpayload) };
      target.ProcessMessage(sender, args);
      AssertTTOutStatus(QTKMpayload, 1);
      AssertTTOutStatus(STKApayload, 1);

      //Simulate RTKM input      
      args = new ReceivedDataEventArgs { ReceivedOnPort = 1121, isTCP = true, EndPoint = "1.2.3.4.5", Buffer = Encoding.ASCII.GetBytes(RTKMpayload) };
      target.ProcessMessage(sender, args);
      AssertTTOutStatus(QTKMpayload, 2);
      AssertTTOutStatus(STKApayload, 1);

      //Simulate RTKS input
      args = new ReceivedDataEventArgs { ReceivedOnPort = 1121, isTCP = true, EndPoint = "1.2.3.4.5", Buffer = Encoding.ASCII.GetBytes(RTKApayload) };
      target.ProcessMessage(sender, args);
      AssertTTOutStatus(STKApayload, 2);
    }

    #region Helper methods

    private Device NewDeviceObject(DeviceTypeEnum deviceType, string gpsDeviceID)
    {
      Device expected = new Device()
      {
        GpsDeviceID = gpsDeviceID,
        IBKey = string.Format("1234{0}", DateTime.UtcNow.Ticks),
        fk_DeviceStateID = (int)DeviceStateEnum.Installed,
        OwnerBSSID = "12123434",
        fk_DeviceTypeID = (int)deviceType,
      };
      return expected;
    }
    
    private void AssertOpDevicesAreEqual(Device expected, long deviceID)
    {
      Device actualOpDevice = (from d in Ctx.OpContext.Device where d.ID == deviceID select d).SingleOrDefault();
      Assert.IsNotNull(actualOpDevice, "Failed to find newly created device in NH_OP");
      Assert.AreEqual(expected.GpsDeviceID, actualOpDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(expected.fk_DeviceStateID, actualOpDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(expected.fk_DeviceTypeID, actualOpDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(expected.IBKey, actualOpDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(expected.OwnerBSSID, actualOpDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    private void AssertRawTTDevice(Device expected, string IMEI)
    {
      TTDevice actualRawDevice = (from tt in Ctx.RawContext.TTDevice where tt.IMEI == IMEI select tt).SingleOrDefault();

      Assert.IsNotNull(actualRawDevice, "Unable to find the newly created device in NH_RAW");
      Assert.AreEqual(expected.fk_DeviceStateID, actualRawDevice.fk_DeviceState, "DeviceState does not match");
      Assert.AreEqual(API.Device.IMEI2UnitID(expected.GpsDeviceID), actualRawDevice.UnitID, "UnitID does not match");
      Assert.AreEqual(expected.GpsDeviceID, actualRawDevice.IMEI, "IMEI does not match");
    }

    private void InsertIntoTTOut(string unitID, string payLoad)
    {
      var TTOut = new TTOut
      {
        ID = 0,
        InsertUTC = DateTime.UtcNow,
        Status = (int)MessageStatusEnum.Pending,
        Payload = payLoad,
        UnitID = unitID
      };      
      Ctx.RawContext.TTOut.AddObject(TTOut);
    }

    private void AssertTTOutRow(string payload)
    {
      int rowCount = (from row in Ctx.RawContext.TTOut where row.Payload == payload select row).Count();
      Assert.AreEqual(1, rowCount, "The out message is not stored in the TTOut table");
    }
    private void AssertTTOutStatus(string payload, int expectedStatus)
    {
      int status = (from row in Ctx.RawContext.TTOut where row.Payload == payload select row.Status).First();
      Assert.AreEqual(expectedStatus, status, "The status of the out message is not as expected");
    }

    #endregion
  }

}
