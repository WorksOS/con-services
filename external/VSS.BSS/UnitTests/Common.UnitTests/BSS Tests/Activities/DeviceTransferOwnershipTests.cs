using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceTransferOwnershipTests :BssUnitTestBase
  {
    private Inputs Inputs;
    private AssetDeviceContext Context;
    private DeviceTransferOwnership Activity;

    [TestInitialize]
    public void DeviceTransferOwnershipTests_Init()
    {
      Inputs = new Inputs();
      Context = new AssetDeviceContext();
      Inputs.Add<AssetDeviceContext>(Context);
      Activity = new DeviceTransferOwnership();
    }

    [TestMethod]
    public void Execute_ServiceCalledWithCorrectArgs_SuccessMessage() 
    {
      var serviceFake = new BssDeviceServiceFake(true);
      Services.Devices = () => serviceFake;

      long oldOwnerId = IdGen.GetId();
      string oldOwnerBssId = IdGen.GetId().ToString();
      CustomerTypeEnum oldOwnerType = CustomerTypeEnum.Dealer;
      string oldOwnerName = "OLD OWNER NAME";

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series522;
      Context.Device.OwnerId = oldOwnerId;
      Context.Device.Owner.Type = oldOwnerType;
      Context.Device.Owner.Name = oldOwnerName;
      Context.Device.OwnerBssId = oldOwnerBssId;

      Context.Owner.Id = IdGen.GetId();
      Context.Owner.Type = CustomerTypeEnum.Account;
      Context.Owner.Name = "NEW OWNER NAME";
      Context.Owner.BssId = IdGen.GetId().ToString();

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Success");
      string message = string.Format(DeviceTransferOwnership.SUCCESS_MESSAGE,
          Context.Device.Type, Context.Device.Id, oldOwnerType, oldOwnerName, oldOwnerId, oldOwnerBssId, 
          Context.Owner.Type, Context.Owner.Name, Context.Owner.Id, Context.Owner.BssId);
      StringAssert.Contains(result.Summary, message);

      Assert.IsTrue(serviceFake.WasExecuted, "Service should have been executed.");
      Assert.AreEqual(Context.Device.Id, serviceFake.DeviceIdArg, "DeviceIdArg not equal.");
      Assert.AreEqual(Context.Owner.BssId, serviceFake.OwnerBssIdArg, "OwnerBssIdArg not equal.");

      Assert.AreEqual(Context.Device.OwnerId, Context.Owner.Id, "Context not updated properly.");
      Assert.AreEqual(Context.Device.Owner.BssId, Context.Owner.BssId, "Context not updated properly.");
    }

    [TestMethod]
    public void Execute_DeviceDoesNotExist_CancelMessage()
    {
      var serviceFake = new BssDeviceServiceFake(true);
      Services.Devices = () => serviceFake;

      Context.Device.Id = 0;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, DeviceTransferOwnership.CANCELLED_DEVICE_DOES_NOT_EXIST_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Service should not have been executed.");
    }

    [TestMethod]
    public void Execute_DeviceOwnerAndNewOwnerAreSame_CancelMessage()
    {
      var serviceFake = new BssDeviceServiceFake(true);
      Services.Devices = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.Type = DeviceTypeEnum.Series522;
      Context.Device.OwnerId = IdGen.GetId();
      Context.Owner.Id = Context.Device.OwnerId;

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Cancelled");
      StringAssert.Contains(result.Summary, DeviceTransferOwnership.CANCELLED_CURRENT_AND_OWNER_SAME_MESSAGE);
      Assert.IsFalse(serviceFake.WasExecuted, "Service should not have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceReturnsFalse_ErrorMessage()
    {
      var serviceFake = new BssDeviceServiceFake(false);
      Services.Devices = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = IdGen.GetId();
      Context.Owner.Id = IdGen.GetId();

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Error");
      StringAssert.Contains(result.Summary, DeviceTransferOwnership.RETURNED_FALSE_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Service should have been executed.");
    }

    [TestMethod]
    public void Execute_ServiceThrowsException_ExceptionSummary()
    {
      var serviceFake = new BssDeviceServiceExceptionFake();
      Services.Devices = () => serviceFake;

      Context.Device.Id = IdGen.GetId();
      Context.Device.OwnerId = IdGen.GetId();
      Context.Owner.Id = IdGen.GetId();

      var result = Activity.Execute(Inputs);

      StringAssert.Contains(result.Summary, "Exception");
      StringAssert.Contains(result.Summary, DeviceTransferOwnership.EXCEPTION_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "Service should have been executed.");
    }
  }
}
