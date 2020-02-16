using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.NH_OPMockObjectSet;
using VSS.UnitTest.Common;

namespace UnitTests
{
  [TestClass()]
  public class DeviceAPITest : UnitTestBase
  {
    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessPL121() // Also tests CreatePLDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL121, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawPLDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessPL321() // Also tests CreatePLDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL321, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawPLDevice(expected, newDevice.GpsDeviceID);
    }

    [TestMethod()]
    [Ignore()]
    public void CreatePLDevice_PL321SuccessWhenAlreadyExists() // Also tests CreateMTSDevice
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessTrimTrac() // Also tests CreateTTDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TrimTrac, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawTTDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessTAP66() 
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TAP66, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      //Device should not be created in NH_RAW
      MTSDevice actualRawDevice = (from mts in Ctx.OpContext.MTSDevice where mts.SerialNumber == expected.GpsDeviceID select mts).SingleOrDefault();
      Assert.IsNull(actualRawDevice, "Device should not be created in NH_RAW");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void CreateTTDeviceDevice_SuccessNonExistingDevice() // Also tests CreateTTDevice
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessCrossCheck() // Also tests CreateMTSDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.CrossCheck, string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessMTS521() // Also tests CreateMTSDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series521, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessMTS522() // Also tests CreateMTSDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series522, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_MTS523Success() // Also tests CreateMTSDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series523, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [TestMethod()]
    [Ignore()]
    public void CreateMTSDevice_MTS521SuccessWhenAlreadyExists() // Also tests CreateMTSDevice
    {
      //DeviceAPI target = new DeviceAPI();
      //Device existingDevice = TestData.TestMTS521;
      //MTSDevice newDevice = target.CreateMTSDevice(Ctx.OpContext, Ctx.RawContext, existingDevice.GpsDeviceID, bitPacketRate, lowPowerRate, sampleRate, updateRate, (DeviceTypeEnum)existingDevice.fk_DeviceTypeID);

      //Assert.IsNotNull(newDevice, "Failed to create Device");
      //Assert.AreEqual(existingDevice.GpsDeviceID, newDevice.SerialNumber, "The device serial number should match.");
      //Assert.AreEqual(DeviceTypeEnum.Series521, (DeviceTypeEnum)newDevice.DeviceType, "The device type should match.");
      //Assert.AreEqual(DeviceStateEnum.Installed, (DeviceStateEnum)newDevice.fk_DeviceState, "Device should be installed.");
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessSNM940() // Also tests CreateMTSDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.SNM940, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_PL440_Success()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL440, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
    }

    #region 35327
    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_PL240_Success()
    {
        DeviceAPI target = new DeviceAPI();
        Device expected = NewDeviceObject(DeviceTypeEnum.PL240, string.Format("SN{0}", DateTime.UtcNow.Ticks));
        Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
          expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
        Assert.IsNotNull(newDevice, "Failed to create Device");

        AssertOpDevicesAreEqual(expected, newDevice.ID);
    }
    #endregion

    #region PL240B
    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_PL240B_Success()
    {
        DeviceAPI target = new DeviceAPI();
        Device expected = NewDeviceObject(DeviceTypeEnum.PL240B, string.Format("SN{0}", DateTime.UtcNow.Ticks));
        Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
          expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
        Assert.IsNotNull(newDevice, "Failed to create Device");

        AssertOpDevicesAreEqual(expected, newDevice.ID);
    }
    #endregion


    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessNoDevice()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.MANUALDEVICE, "");
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateDevice_FailureExistingDevice()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL321, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      Device newDevice2 = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateDevice_FailureTrimTracGpsDeviceIDInvalid() 
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TrimTrac, string.Format("{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateDevice_FailureCrossCheckGpsDeviceIDInvalid()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TrimTrac, string.Format("{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessPL131() 
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL131, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessPL141()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL141, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
    }
    [DatabaseTest]
    [TestMethod()]
    public void CreateDevice_SuccessPL161()
    {
        DeviceAPI target = new DeviceAPI();
        Device expected = NewDeviceObject(DeviceTypeEnum.PL161, string.Format("SN{0}", DateTime.UtcNow.Ticks));//DeviceTypeEnum.PL161
        Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
          expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
        Assert.IsNotNull(newDevice, "Failed to create Device");

        AssertOpDevicesAreEqual(expected, newDevice.ID);
    }


    #region CREATEDEVICE_STORE

    private static Mock<INH_OP> _mockNhOpContext;
    private static DeviceAPI _deviceAPI;
    private static Device _device;

    [ClassInitialize]
    public static void Init(TestContext testContext)
    {
      _deviceAPI = new DeviceAPI();
      _mockNhOpContext = new Mock<INH_OP>();
    }

    [TestCleanup]
    public void TestCleanup()
    {
      Init(null);
    }

    [TestMethod]
    public void CreateDevice_Store_Success_PLE631()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,        
        GpsDeviceID = "12345",
        fk_DeviceTypeID = (int)DeviceTypeEnum.PLE631,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);
      
      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID,(DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_PL121()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = "12345",
        fk_DeviceTypeID = (int)DeviceTypeEnum.PL121,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_MTS521()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = "12345",
        fk_DeviceTypeID = (int)DeviceTypeEnum.Series521,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_TrimTrac()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = GetExpectedIMEI(),
        fk_DeviceTypeID = (int)DeviceTypeEnum.TrimTrac,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_CrossCheck()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8),
        fk_DeviceTypeID = (int)DeviceTypeEnum.CrossCheck,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_PL131()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8),
        fk_DeviceTypeID = (int)DeviceTypeEnum.PL131,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_PL141()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8),
        fk_DeviceTypeID = (int)DeviceTypeEnum.PL141,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_PL161()
    {
        _device = new Device()
        {
            IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
            OwnerBSSID = string.Empty,
            GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8),
            fk_DeviceTypeID = (int)DeviceTypeEnum.PL161,
            fk_DeviceStateID = 2
        };
        MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
        deviceRecords.AddObject(_device);
        _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
        _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

        DailyReport dailyReport = new DailyReport();
        MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
        dailyRecords.AddObject(dailyReport);
        _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
        _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

        var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
        Assert.IsNotNull(newDevice, "Failed to create Device");
        Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
        Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
        Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
        Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
        Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }

    [TestMethod]
    public void CreateDevice_Store_Success_NoDevice()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = "12345",
        fk_DeviceTypeID = (int)DeviceTypeEnum.MANUALDEVICE,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }
        
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateDevice_Store_FailureTrimTracGpsDeviceIDInvalid()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks),
        fk_DeviceTypeID = (int)DeviceTypeEnum.TrimTrac,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }
        
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CreateDevice_Store_FailureCrossCheckGpsDeviceIDInvalid()
    {
      _device = new Device()
      {
        IBKey = "StoreAPI_" + string.Format("{0}", DateTime.UtcNow.Ticks),
        OwnerBSSID = string.Empty,
        GpsDeviceID = string.Format("{0}", DateTime.UtcNow.Ticks),
        fk_DeviceTypeID = (int)DeviceTypeEnum.CrossCheck,
        fk_DeviceStateID = 2
      };
      MockObjectSet<Device> deviceRecords = new MockObjectSet<Device>();
      deviceRecords.AddObject(_device);
      _mockNhOpContext.SetupGet(o => o.DeviceReadOnly).Returns(deviceRecords);
      _mockNhOpContext.SetupGet(o => o.Device).Returns(deviceRecords);

      DailyReport dailyReport = new DailyReport();
      MockObjectSet<DailyReport> dailyRecords = new MockObjectSet<DailyReport>();
      dailyRecords.AddObject(dailyReport);
      _mockNhOpContext.SetupGet(o => o.DailyReport).Returns(dailyRecords);
      _mockNhOpContext.Setup(o => o.SaveChanges()).Returns(1);

      var newDevice = _deviceAPI.CreateDevice(_mockNhOpContext.Object, _device.IBKey, _device.OwnerBSSID, _device.GpsDeviceID, (DeviceTypeEnum)_device.fk_DeviceTypeID);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual(_device.GpsDeviceID, newDevice.GpsDeviceID, "GpsDeviceIDs do not match");
      Assert.AreEqual(_device.fk_DeviceStateID, newDevice.fk_DeviceStateID, "Device States do not match");
      Assert.AreEqual(_device.fk_DeviceTypeID, newDevice.fk_DeviceTypeID, "Device Types do not match");
      Assert.AreEqual(_device.IBKey, newDevice.IBKey, "IB Keys do not match");
      Assert.AreEqual(_device.OwnerBSSID, newDevice.OwnerBSSID, "OwnerBSSIDs do not match");
    }
    
    #endregion

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessTrimTrac() // Also Tests UpdateTTDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TrimTrac, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawTTDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessCrossCheck()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.CrossCheck, string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(activated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessMTS521()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series521, string.Format("521{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(activated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessMTS522()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series522, string.Format("522{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(activated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
     // AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessMTS523()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series523, string.Format("523{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(activated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void ActivateDevice_SuccessSNM940()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.SNM940, string.Format("DCM{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(activated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ActivateDevice_FailureMissingDevice()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.SNM940, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Subscribed;
      expected.fk_DeviceStateID = (int)expectedState;

      bool activated = target.ActivateDevice(Ctx.OpContext, expected.IBKey+'1', expectedState);
    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void ActivateDevice_SuccessDefaultCase()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessPL121()  // Also tests RegisterDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL121, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawPLDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessPL321()  // Also tests RegisterDevice
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL321, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawPLDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    [Ignore]
    public void DeActivateDevice_SuccessDefaultCase()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void UpdatePLDeviceState_PL321Success()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void UpdatePLDeviceState_PL321SuccessNullDevice()
    {

    }

    [DatabaseTest]
    [TestMethod()]    
    public void UpdatePLDeviceState_PL321SuccessUpdatedDeviceState()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.PL321, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");
      Assert.AreEqual((int)DeviceStateEnum.Provisioned, newDevice.fk_DeviceStateID, "DeviceState is not 'Installed' for new Device");      

      DeviceStateEnum expectedState = DeviceStateEnum.DeregisteredTechnician;

      //Update DeviceState to 'BlackListedCancelledService' and 'IsReadOnly' to FALSE
      target.UpdateOpDeviceState(Ctx.OpContext, newDevice.GpsDeviceID, expectedState, (int)DeviceTypeEnum.PL321);

      var opDevice = Ctx.OpContext.DeviceReadOnly.Where(f => f.GpsDeviceID == newDevice.GpsDeviceID).Select(f => f).FirstOrDefault();
      Assert.IsNotNull(opDevice, "Device not found in OP");
      Assert.AreEqual((int)expectedState, newDevice.fk_DeviceStateID, "DeviceState does not match in OP");
    }

    [TestMethod()]
    [Ignore()]
    public void UpdatePLDeviceState_PL321SuccessChangedDeviceReadOnlyValue()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void UpdatePLDeviceState_PL321FailureInvalidOperationException()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void UpdateTTDeviceState_FailureSaveChangesException()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void CreateMTSDevice_SuccessDeviceExists()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void CreateMTSDevice_FailureSaveChangesException()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void UpdateMTSDeviceState_FailureSaveChangesException()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void CreatePLDevice_FailureInvalidGPSDeviceID()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void CreatePLDevice_FailureSaveChangesException()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void CreatePLDevice_SuccessDeviceExists()
    {

    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void UpdateOpDeviceState_FailureInvalidOperationException()
    {

    }

    [TestMethod()]
    [Ignore()]
    public void ValidGpsDeviceID_FailureInvalidGPSDeviceID()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessTrimTrac() // Also Tests UpdateTTDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.TrimTrac, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawTTDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessCrossCheck() // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.CrossCheck, string.Format("{0}", DateTime.UtcNow.Ticks).Substring(0, 8));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessMTS521()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series521, string.Format("521{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessMTS522()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series522, string.Format("522{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessMTS523()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.Series523, string.Format("523{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    public void DeActivateDevice_SuccessSNM940()  // Also tests UpdateMTSDeviceState
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.SNM940, string.Format("DCM{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey, expectedState);
      Assert.IsTrue(deactivated, "Failed to activate device");

      AssertOpDevicesAreEqual(expected, newDevice.ID);
      //AssertRawMTSDevice(expected, newDevice.GpsDeviceID);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    public void DeactivateDevice_FailueMissingDevice()
    {
      DeviceAPI target = new DeviceAPI();
      Device expected = NewDeviceObject(DeviceTypeEnum.SNM940, string.Format("SN{0}", DateTime.UtcNow.Ticks));
      Device newDevice = target.CreateDevice(Ctx.OpContext, expected.IBKey, expected.OwnerBSSID,
        expected.GpsDeviceID, (DeviceTypeEnum)expected.fk_DeviceTypeID, sampleRate, updateRate, lowPowerRate, bitPacketRate, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      DeviceStateEnum expectedState = DeviceStateEnum.Provisioned;
      expected.fk_DeviceStateID = (int)expectedState;

      bool deactivated = target.DeActivateDevice(Ctx.OpContext, expected.IBKey+'1', expectedState);
    }

    [TestMethod()]
    [ExpectedException(typeof(InvalidOperationException))]
    [Ignore()]
    public void CreateSensor_FailureSaveChangesException()
    {

    }

    [DatabaseTest]
    [TestMethod()]
    public void IMEI2UnitID_AllPaths()
    {
      DeviceAPI target = new DeviceAPI();

      string actualID = target.IMEI2UnitID("01030700123456");
      Assert.AreEqual("Y0123456", actualID, "Unit IDs do not match");

      actualID = target.IMEI2UnitID("35323900162534");
      Assert.AreEqual("Y1162534", actualID, "Unit IDs do not match");
      
      actualID = target.IMEI2UnitID("01107400654321");
      Assert.AreEqual("Y2654321", actualID, "Unit IDs do not match");
          
      actualID = target.IMEI2UnitID("01127600615243");
      Assert.AreEqual("Y3615243", actualID, "Unit IDs do not match");

      actualID = target.IMEI2UnitID("018014107050301");
      Assert.IsNull(actualID, "Unit ID is returned");
    }

    [DatabaseTest]
    [TestMethod()]
    public void IsProductLinkDevice_AllPaths()
    {
      DeviceAPI target = new DeviceAPI();

      bool isPLDevice = target.IsProductLinkDevice(DeviceTypeEnum.PL121);
      Assert.IsTrue(isPLDevice, "PL121 is a PL device");

      isPLDevice = target.IsProductLinkDevice(DeviceTypeEnum.PL321);
      Assert.IsTrue(isPLDevice, "PL321 is a PL device");

      isPLDevice = target.IsProductLinkDevice(DeviceTypeEnum.Series521);
      Assert.IsFalse(isPLDevice, "PL121 is NOT a PL device");
    }

    [DatabaseTest]
    [TestMethod]
    public void DailyReportCreateTest()
    {
      DailyReport dailyReport = null;
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Device newDevice = API.Device.CreateDevice(ctx, "TestIBKey", "TestOwnerBSSID", "TestGpsDeviceID",
                                                   DeviceTypeEnum.PL321, null, null, null, null, true);
        dailyReport =
          (from d in ctx.DailyReportReadOnly where d.fk_DeviceID == newDevice.ID select d).FirstOrDefault();
      }
      Assert.IsNotNull(dailyReport, "DailyReport Should Not Be Null");
      Assert.IsFalse(!dailyReport.IsUserCustomized.HasValue || dailyReport.IsUserCustomized.Value,
                     "Daily Report's IsUserCustomized should be null or false");
      Assert.IsNull(dailyReport.LastDailyReportTZBias, "LastTZBias Should be null");
      Assert.IsNull(dailyReport.LastDailyReportUTC, "Last dailyreport utc should be null");
      Assert.IsNull(dailyReport.NextCheckUTC, "Next Check Utc Should be null");
    }

    [DatabaseTest]
    [TestMethod]
    public void UpdateDailyReportTest()
    {
      DailyReport dailyReport = null;
      Device newDevice = null;
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        newDevice = API.Device.CreateDevice(ctx, "TestIBKey", "TestOwnerBSSID", "TestGpsDeviceID",
                                                   DeviceTypeEnum.PL321, null, null, null, null, true);
        dailyReport =
          (from d in ctx.DailyReport where d.fk_DeviceID == newDevice.ID select d).FirstOrDefault();

        dailyReport.IsUserCustomized = true;
        ctx.SaveChanges();
      }

      API.Device.UpdateDeviceState(newDevice.ID, DeviceStateEnum.Installed);
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        dailyReport =
          (from d in opCtx.DailyReport where d.fk_DeviceID == newDevice.ID select d).FirstOrDefault();
      }
      Assert.IsFalse(!dailyReport.IsUserCustomized.HasValue || dailyReport.IsUserCustomized.Value,
                    "Daily Report's IsUserCustomized should be null or false");
      Assert.IsNull(dailyReport.LastDailyReportTZBias, "LastTZBias Should be null");
      Assert.IsNull(dailyReport.LastDailyReportUTC, "Last dailyreport utc should be null");
      Assert.IsNull(dailyReport.NextCheckUTC, "Next Check Utc Should be null");
    }

    #region BSS V2 test cases

    [DatabaseTest]
    [TestMethod]
    public void CreateDevicePersonality_ValidDevice_Success()
    {
      var device = Entity.Device.MTS521.Save();

      var personalities = API.Device.CreateDevicePersonality(
        Ctx.OpContext, 
        device.ID, 
        IdGen.StringId(), 
        IdGen.StringId(), 
        IdGen.StringId(), 
        IdGen.StringId(), 
        IdGen.StringId(), 
        DeviceTypeEnum.Series521);

      Assert.AreEqual(4, personalities.Count, "Expecting 4 device personality objects.");

      var savedPersonalities = Ctx.OpContext.DevicePersonalityReadOnly.Where(t => t.fk_DeviceID == device.ID).ToList();
      Assert.AreEqual(4, savedPersonalities.Count, "4 personalities should have been saved to DB.");

      foreach (var personality in personalities)
      {
        var result = savedPersonalities.Where(t => t.fk_PersonalityTypeID == personality.fk_PersonalityTypeID && t.fk_DeviceID == personality.fk_DeviceID).First();
         Assert.AreEqual(personality.Value, result.Value, "Values should match.");
      }
    }

    [DatabaseTest]
    [TestMethod]
    public void CreateDevicePersonality_TrimTrackDevice_Success()
    {
      var device = Entity.Device.TrimTrac.Save();

      var personalities = API.Device.CreateDevicePersonality(
           Ctx.OpContext,
           device.ID,
           IdGen.StringId(),
           IdGen.StringId(),
           IdGen.StringId(),
           IdGen.StringId(),
           "01030700XXXXXXXX",
           DeviceTypeEnum.TrimTrac);
      Assert.AreEqual(5, personalities.Count, "Expecting 5 device personality objects.");

      var savedPersonalities = Ctx.OpContext.DevicePersonalityReadOnly.Where(t => t.fk_DeviceID == device.ID).ToList();
      Assert.AreEqual(5, savedPersonalities.Count, "5 personalities should have been saved to DB.");

      foreach (var personality in personalities)
      {
        var result = savedPersonalities.Where(t => t.fk_PersonalityTypeID == personality.fk_PersonalityTypeID && t.fk_DeviceID == personality.fk_DeviceID).First();
        Assert.AreEqual(personality.Value, result.Value, "Values should match.");
      }
    }

    #endregion


    #region Test Data
    TimeSpan bitPacketRate = new TimeSpan(24, 0, 0);
    TimeSpan lowPowerRate = new TimeSpan(8, 0, 0);
    TimeSpan sampleRate = new TimeSpan(4, 0, 0);
    TimeSpan updateRate = new TimeSpan(6, 0, 0);
    #endregion

    #region Helper Methods

    private Device NewDeviceObject(DeviceTypeEnum deviceType, string gpsDeviceID)
    {
      Device expected = new Device()
      {
        GpsDeviceID = gpsDeviceID,
        IBKey = string.Format("1234{0}", DateTime.UtcNow.Ticks),
        fk_DeviceStateID = (int)DeviceStateEnum.Provisioned,
        OwnerBSSID = "12123434",
        fk_DeviceTypeID = (int)deviceType,
      };
      return expected;
    }

    private string GetExpectedIMEI()
    {
      return string.Format("01107400{0}", DateTime.UtcNow.Ticks).Substring(0, 15);
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

    #endregion

    /// <summary>
    ///A test for UpdateDailyReportIsUserCustomized
    ///</summary>
    [TestMethod()]
    [DatabaseTest]
    public void UpdateDailyReportIsUserCustomizedResetTest()
    {
      DeviceAPI target = new DeviceAPI();
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        string gpsDeviceID = "TestGPSDeviceID";
        bool isUserCustomized = false;
        Device device = API.Device.CreateDevice(ctx, "-111", "-444", gpsDeviceID, DeviceTypeEnum.PL321, null, null, null, null, true);

        target.UpdateDailyReportIsUserCustomized(ctx, gpsDeviceID, isUserCustomized, null, (int) DeviceTypeEnum.PL321);

        DailyReport dr = (from d in ctx.DailyReportReadOnly where d.fk_DeviceID == device.ID select d).FirstOrDefault();

        Assert.IsFalse(dr.IsUserCustomized.Value, "Incorrect IsUserCustomized field");
        Assert.IsNull(dr.LastDailyReportTZBias, "Incorrect LastDailyReportTZBias field");
        Assert.IsNull(dr.LastDailyReportUTC, "Incorrect LastDailyReportUTC field");
        Assert.IsNull(dr.NextCheckUTC, "Incorrect NextCheckUTC field");
      }
    }

    /// <summary>
    ///A test for UpdateDailyReportIsUserCustomized
    ///</summary>
    [TestMethod()]
    [DatabaseTest]
    public void UpdateDailyReportIsUserCustomizedTrueTest()
    {
      DeviceAPI target = new DeviceAPI();
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        string gpsDeviceID = "TestGPSDeviceID";
        bool isUserCustomized = true;
        Device device = API.Device.CreateDevice(ctx, "-111", "-444", gpsDeviceID, DeviceTypeEnum.PL321, null, null, null, null, true);

        target.UpdateDailyReportIsUserCustomized(ctx, gpsDeviceID, isUserCustomized, DateTime.UtcNow, (int)DeviceTypeEnum.PL321);

        DailyReport dr = (from d in ctx.DailyReportReadOnly where d.fk_DeviceID == device.ID select d).FirstOrDefault();

        Assert.IsTrue(dr.IsUserCustomized.Value, "Incorrect IsUserCustomized field");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfiguration_Success()
    {
      DateTime testUtc = DateTime.UtcNow;
      bool transactedCalledBackInvoked = false;
      int transactedCalledBackInvocationCount = 0;
      bool testIsEnabled = true;
      long testMessageSourceId = 42;
      TimeSpan testDuration = TimeSpan.FromSeconds(43);
      DeviceTypeEnum testDeviceType = DeviceTypeEnum.PL641;
      MessageStatusEnum testStatus = MessageStatusEnum.Sent;

      DeviceAPI target = new DeviceAPI();
      Device expectedDevice = NewDeviceObject(testDeviceType, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expectedDevice.IBKey, expectedDevice.OwnerBSSID, expectedDevice.GpsDeviceID, (DeviceTypeEnum)expectedDevice.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      A5N2ConfigData.MaintenanceModeConfig maintenanceModeConfig = new A5N2ConfigData.MaintenanceModeConfig()
      {
        Duration = testDuration,
        IsEnabled = testIsEnabled, 
        MessageSourceID = testMessageSourceId,
        SentUTC = testUtc,
        Status = testStatus
      };

      Action transactedCallback = () => { transactedCalledBackInvoked = true; transactedCalledBackInvocationCount++; };
      Mock<IObjectContextTransactionScope> mockObjectContextTransactionScope = new Mock<IObjectContextTransactionScope>();
      var transactionParams = new ObjectContextTransactionParams<Action>(transactedCallback, mockObjectContextTransactionScope.Object);

      target.UpdateDeviceConfiguration(newDevice.GpsDeviceID, (DeviceTypeEnum)newDevice.fk_DeviceTypeID, maintenanceModeConfig, transactionParams);

      Device updatedDevice = (from devices in (ObjectContextFactory.NewNHContext<INH_OP>()).Device 
                              where devices.ID == newDevice.ID 
                              select devices).SingleOrDefault();

      A5N2ConfigData.MaintenanceModeConfig resultConfig = (new A5N2ConfigData(updatedDevice.DeviceDetailsXML)).LastSentMaintMode;

      mockObjectContextTransactionScope.Verify(o => o.EnrollObjectContexts(It.IsAny<object[]>()), Times.Once());
      mockObjectContextTransactionScope.Verify(o => o.Commit(), Times.Once());
      Assert.IsTrue(transactedCalledBackInvoked);
      Assert.AreEqual(transactedCalledBackInvocationCount, 1);
      Assert.AreEqual(resultConfig.Duration, testDuration);
      Assert.AreEqual(resultConfig.IsEnabled, testIsEnabled);
      Assert.AreEqual(resultConfig.MessageSourceID, testMessageSourceId);
      Assert.AreEqual(resultConfig.SentUTC, testUtc);
      Assert.AreEqual(resultConfig.Status, testStatus);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationNullTransactionParams_Success()
    {
      DateTime testUtc = DateTime.UtcNow;
      bool testIsEnabled = true;
      long testMessageSourceId = 42;
      TimeSpan testDuration = TimeSpan.FromSeconds(43);
      DeviceTypeEnum testDeviceType = DeviceTypeEnum.PL641;
      MessageStatusEnum testStatus = MessageStatusEnum.Sent;

      DeviceAPI target = new DeviceAPI();
      Device expectedDevice = NewDeviceObject(testDeviceType, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expectedDevice.IBKey, expectedDevice.OwnerBSSID, expectedDevice.GpsDeviceID, (DeviceTypeEnum)expectedDevice.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      A5N2ConfigData.MaintenanceModeConfig maintenanceModeConfig = new A5N2ConfigData.MaintenanceModeConfig()
      {
        Duration = testDuration,
        IsEnabled = testIsEnabled,
        MessageSourceID = testMessageSourceId,
        SentUTC = testUtc,
        Status = testStatus
      };

      target.UpdateDeviceConfiguration(newDevice.GpsDeviceID, (DeviceTypeEnum)newDevice.fk_DeviceTypeID, maintenanceModeConfig, null);

      Device updatedDevice = (from devices in (ObjectContextFactory.NewNHContext<INH_OP>()).Device
                              where devices.ID == newDevice.ID
                              select devices).SingleOrDefault();

      A5N2ConfigData.MaintenanceModeConfig resultConfig = (new A5N2ConfigData(updatedDevice.DeviceDetailsXML)).LastSentMaintMode;

      Assert.AreEqual(resultConfig.Duration, testDuration);
      Assert.AreEqual(resultConfig.IsEnabled, testIsEnabled);
      Assert.AreEqual(resultConfig.MessageSourceID, testMessageSourceId);
      Assert.AreEqual(resultConfig.SentUTC, testUtc);
      Assert.AreEqual(resultConfig.Status, testStatus);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateDeviceConfigurationCallbackException_Success()
    {
      DateTime testUtc = DateTime.UtcNow;
      bool testIsEnabled = true;
      long testMessageSourceId = 42;
      TimeSpan testDuration = TimeSpan.FromSeconds(43);
      DeviceTypeEnum testDeviceType = DeviceTypeEnum.PL641;
      MessageStatusEnum testStatus = MessageStatusEnum.Sent;
      string exceptionMessage = "Darn!";
      bool exceptionThrown = false;

      DeviceAPI target = new DeviceAPI();
      Device expectedDevice = NewDeviceObject(testDeviceType, GetExpectedIMEI());
      Device newDevice = target.CreateDevice(Ctx.OpContext, expectedDevice.IBKey, expectedDevice.OwnerBSSID, expectedDevice.GpsDeviceID, (DeviceTypeEnum)expectedDevice.fk_DeviceTypeID, null, null, null, null, true);
      Assert.IsNotNull(newDevice, "Failed to create Device");

      A5N2ConfigData.MaintenanceModeConfig maintenanceModeConfig = new A5N2ConfigData.MaintenanceModeConfig()
      {
        Duration = testDuration,
        IsEnabled = testIsEnabled,
        MessageSourceID = testMessageSourceId,
        SentUTC = testUtc,
        Status = testStatus
      };

      Action transactedCallback = () => { throw new Exception(exceptionMessage); };
      Mock<IObjectContextTransactionScope> mockObjectContextTransactionScope = new Mock<IObjectContextTransactionScope>();
      var transactionParams = new ObjectContextTransactionParams<Action>(transactedCallback, mockObjectContextTransactionScope.Object);

      try
      {
        target.UpdateDeviceConfiguration(newDevice.GpsDeviceID, (DeviceTypeEnum)newDevice.fk_DeviceTypeID, maintenanceModeConfig, transactionParams);
      }
      catch (Exception ex)
      {
        Assert.AreEqual(ex.Message, exceptionMessage);
        exceptionThrown = true;
      }

      mockObjectContextTransactionScope.Verify(o => o.EnrollObjectContexts(It.IsAny<object[]>()), Times.Once());
      mockObjectContextTransactionScope.Verify(o => o.Commit(), Times.Never());
      Assert.IsTrue(exceptionThrown);
    }

    [TestMethod]
    [DatabaseTest]
    public void UpdateOwnerBSSId_ValidInfo_Updates()
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Device newDevice = API.Device.CreateDevice(ctx, "TestIBKey", "TestOwnerBSSID1", "TestGpsDeviceID",
                                                   DeviceTypeEnum.PL321, null, null, null, null, true);
        
        var newCustomer = API.Customer.CreateCustomer(ctx, "TestCustomer", "TestOwnerBSSID");
        var ret = API.Device.UpdateOwnerBSSID(newDevice.ID, newCustomer.CustomerUID.Value, ctx);
        Assert.IsTrue(ret, "Owner BssId did not get updated");

        var updatedDevice = ctx.DeviceReadOnly.Single(x => x.ID == newDevice.ID);
        Assert.AreEqual(newCustomer.BSSID, updatedDevice.OwnerBSSID, "Bss Id did not update");
      }
    }

    [TestMethod]
    [DatabaseTest]
    public void CancelOwnerBSSId_ValidInfo_Updates()
    {
      var ctx = Ctx.OpContext;
      
      Device newDevice = API.Device.CreateDevice(ctx, "TestIBKey", "TestOwnerBSSID", "TestGpsDeviceID",
                                                  DeviceTypeEnum.PL321, null, null, null, null, true);
      var asset = Entity.Asset.WithDevice(newDevice).Save();
        
      var newCustomer = API.Customer.CreateCustomer(ctx, "TestCustomer", "TestOwnerBSSID");
      bool ret = API.Device.CancelOwnerBSSID(newDevice.ID, newCustomer.CustomerUID.Value, ctx);
      Assert.IsTrue(ret, "Owner BssId did not get updated");

      var adh = ctx.AssetDeviceHistoryReadOnly.FirstOrDefault();
      Assert.AreEqual(asset.AssetID, adh.fk_AssetID, "asset id is not same");
      Assert.AreEqual("TestOwnerBSSID", adh.OwnerBSSID, "AssetDeviceHistory's ownerBSS is wrong");
      var updatedDevice = ctx.DeviceReadOnly.Single(x => x.ID == newDevice.ID);
      Assert.IsTrue(string.IsNullOrEmpty(updatedDevice.OwnerBSSID));
        

      
    }

  }
}
