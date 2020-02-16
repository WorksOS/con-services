using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceCreateTests : BssUnitTestBase
  {
    DeviceCreate activity;
    Inputs inputs;

    [TestInitialize]
    public void TestInitialize()
    {
      activity = new DeviceCreate();
      inputs = new Inputs();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Execute_NullIbDevice_Exception()
    {
      var context = new AssetDeviceContext();
      context.IBDevice = null;
      inputs.Add<AssetDeviceContext>(context);
      activity.Execute(inputs);
    }

    [TestMethod]
    public void Execute_ThrowException_ReturnsExceptionResult()
    {
      var serviceFake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => serviceFake;

      AssetDeviceContext _context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          Type = DeviceTypeEnum.Series521,
          GpsDeviceId = IdGen.GetId().ToString(),
          IbKey = IdGen.GetId().ToString(),
          PartNumber = IdGen.GetId().ToString(),
          
        },
        Owner = new ExistingOwnerDto { BssId = IdGen.GetId().ToString() },
      };
      inputs.Add<AssetDeviceContext>(_context);

      var exceptionResult = activity.Execute(inputs) as ExceptionResult;

      Assert.IsNotNull(exceptionResult, "Not an ExceptionResult");
      Assert.IsTrue(serviceFake.WasExecuted, "CreateDevice method should have been invoked.");
      StringAssert.Contains(exceptionResult.Summary, string.Format(DeviceCreate.FAILURE_MESSAGE, _context.IBDevice.Type, _context.IBDevice.IbKey));
    }

    [TestMethod]
    public void Execute_NullDevice_ReturnsErrorResult()
    {
      var serviceFake = new BssDeviceServiceFake((Device)null);
      Services.Devices = () => serviceFake;

      AssetDeviceContext _context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          Type = DeviceTypeEnum.Series521,
          GpsDeviceId = IdGen.GetId().ToString(),
          IbKey = IdGen.GetId().ToString(),
          PartNumber = IdGen.GetId().ToString(),
          
        },
        Owner = new ExistingOwnerDto { BssId = IdGen.GetId().ToString() },
      };
      inputs.Add<AssetDeviceContext>(_context);

      var errorResult = activity.Execute(inputs) as ErrorResult;

      Assert.IsNotNull(errorResult, "Not an ErrorResult");
      Assert.IsTrue(serviceFake.WasExecuted, "CreateDevice method should have been invoked.");
      StringAssert.Contains(errorResult.Summary, DeviceCreate.DEVICE_NULL_MESSAGE);
    }

    [TestMethod]
    public void Execute_Success()
    {
      var device = new Device
      {
        ID = IdGen.GetId(),
        fk_DeviceTypeID = (int)DeviceTypeEnum.Series521,
        GpsDeviceID = IdGen.GetId().ToString(),
        IBKey = IdGen.GetId().ToString(),
        OwnerBSSID = IdGen.GetId().ToString()
      };

      var serviceFake = new BssDeviceServiceFake(device);
      Services.Devices = () => serviceFake; 

      AssetDeviceContext _context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          Type = (DeviceTypeEnum)device.fk_DeviceTypeID,
          GpsDeviceId = device.GpsDeviceID,
          IbKey = device.IBKey,
          PartNumber = IdGen.GetId().ToString(),
          
        },
        Owner = new ExistingOwnerDto { BssId = device.OwnerBSSID },
      };
      inputs.Add<AssetDeviceContext>(_context);

      var activityResult = activity.Execute(inputs);

      Assert.IsTrue(serviceFake.WasExecuted, "CreateDevice method should have been invoked.");

      string summary = string.Format(DeviceCreate.SUCCESS_MESSAGE, (DeviceTypeEnum) device.fk_DeviceTypeID, device.ID, device.IBKey);
      StringAssert.Contains(activityResult.Summary, summary, "Success message should have been returned.");
      
      Assert.AreEqual(_context.Device.Type, _context.IBDevice.Type, "DeviceTypes are expected to be same.");
      Assert.AreEqual(_context.Device.GpsDeviceId, _context.IBDevice.GpsDeviceId, "GPSDeviceIDs are expected to be same.");
      Assert.AreEqual(_context.Device.IbKey, _context.IBDevice.IbKey, "IBKeys are expected to be same.");
    }

    [TestMethod]
    public void Execute_XCheck_Success()
    {
      var device = new Device
      {
        ID = IdGen.GetId(),
        fk_DeviceTypeID = (int)DeviceTypeEnum.CrossCheck,
        GpsDeviceID = "01234567",
        IBKey = IdGen.GetId().ToString(),
        OwnerBSSID = IdGen.GetId().ToString()
      };

      var serviceFake = new BssDeviceServiceFake(device);
      Services.Devices = () => serviceFake;

      AssetDeviceContext _context = new AssetDeviceContext
      {
        IBDevice = new DeviceDto
        {
          Type = (DeviceTypeEnum)device.fk_DeviceTypeID,
          GpsDeviceId = device.GpsDeviceID,
          IbKey = device.IBKey,
          PartNumber = IdGen.GetId().ToString(),

        },
        Owner = new ExistingOwnerDto { BssId = device.OwnerBSSID },
      };
      inputs.Add<AssetDeviceContext>(_context);

      var activityResult = activity.Execute(inputs);

      Assert.IsTrue(serviceFake.WasExecuted, "CreateDevice method should have been invoked.");

      string summary = string.Format(DeviceCreate.SUCCESS_MESSAGE, (DeviceTypeEnum)device.fk_DeviceTypeID, device.ID, device.IBKey);
      StringAssert.Contains(activityResult.Summary, summary, "Success message should have been returned.");

      Assert.AreEqual(_context.Device.Type, _context.IBDevice.Type, "DeviceTypes are expected to be same.");
      Assert.AreEqual(_context.Device.GpsDeviceId, _context.IBDevice.GpsDeviceId, "GPSDeviceIDs are expected to be same.");
      Assert.AreEqual(_context.Device.IbKey, _context.IBDevice.IbKey, "IBKeys are expected to be same.");
    }
  }
}
