using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DeviceAssetContextValidatorTests : BssUnitTestBase
  {

    DeviceAssetContextValidator validator;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DeviceAssetContextValidator();
    }

    [TestMethod]
    public void Validate_ValidateContext_OldIBKeysDoesNotExists_Error()
    {
      var context = new DeviceAssetContext
      {
        NewIBKey = IdGen.GetId().ToString(),
        NewDeviceAsset = { DeviceId = IdGen.GetId() }
      };

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).Count(), "1 Errors Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.OldIBKey, "Old"), validator.Errors[0].Item2, "Message should exists");
    }

    [TestMethod]
    public void Validate_ValidateContext_NewIBKeysDoesNotExists_Error()
    {
      var context = new DeviceAssetContext
      {
        OldIBKey = IdGen.GetId().ToString(),
        OldDeviceAsset = { DeviceId = IdGen.GetId() }
      };

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(1, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).Count(), "1 Errors Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.NewIBKey, "New"), validator.Errors[0].Item2, "Message should exists");
    }

    [TestMethod]
    public void Validate_ValidateContext_BothIBKeysDoesNotExists_Error()
    {
      var context = new DeviceAssetContext
      {
        OldIBKey = IdGen.GetId().ToString(),
        NewIBKey = IdGen.GetId().ToString()
      };

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(2, validator.Errors.Where(t => t.Item1 == BssFailureCode.IbKeyDoesNotExist).Count(), "2 Errors Expected");
      Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.OldIBKey, "Old"), validator.Errors[0].Item2, "Message should exists");
      Assert.AreEqual(string.Format(BssConstants.DeviceReplacement.IBKEY_DOES_NOT_EXISTS, context.NewIBKey, "New"), validator.Errors[1].Item2, "Message should exists");
    }

    [TestMethod]
    public void Validate_ValidateContext_Success()
    {
      var context = new DeviceAssetContext
      {
        OldIBKey = IdGen.GetId().ToString(),
        NewIBKey = IdGen.GetId().ToString(),
        NewDeviceAsset = { DeviceId = IdGen.GetId() },
        OldDeviceAsset = { DeviceId = IdGen.GetId() }
      };

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count(), "No Warnings expected");
      Assert.AreEqual(0, validator.Errors.Count(), "No Errors Expected");
    }
  }
}
