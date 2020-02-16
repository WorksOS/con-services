using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AssetDeviceContextValidatorTests : BssUnitTestBase
  {
    private AssetDeviceContextValidator Validator;

    [TestInitialize]
    public void DeviceContextValidator_Init()
    {
      Validator = new AssetDeviceContextValidator();
    }

    [TestMethod]
    public void Validate_ValidContext_NoWarningsAndNoErrors()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = IdGen.GetId();
      context.Owner.Type = CustomerTypeEnum.Dealer;
      context.IBDevice.Type = DeviceTypeEnum.PL321;
      context.IBDevice.GpsDeviceId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(0, Validator.Warnings.Count, "Expected 0 warnings.");
      Assert.AreEqual(0, Validator.Errors.Count, "Expected 0 Errors.");
    }

    [TestMethod]
    public void Validate_DeviceDoesNotExistForPartNumber_Error()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = IdGen.GetId();
      context.Owner.Type = CustomerTypeEnum.Dealer;
      context.IBDevice.Type = null;
      context.IBDevice.GpsDeviceId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.PartNumberDoesNotExist, Validator.Errors[0].Item1);
      StringAssert.Contains(Validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.PART_NUMBER_DOES_NOT_EXIST, context.IBDevice.PartNumber), "Summay is expected to contain Part number doesn't exists message.");
    }

    [TestMethod]
    public void Validate_OwnerBssIdDoesNotExist_Error()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = 0;
      context.Owner.Type = CustomerTypeEnum.Dealer;
      context.IBDevice.Type = DeviceTypeEnum.PL321;
      context.IBDevice.GpsDeviceId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.OwnerBssIdDoesNotExist, Validator.Errors[0].Item1);
      StringAssert.Contains(Validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.OWNER_BSSID_DOES_NOT_EXIST, context.IBDevice.OwnerBssId), "Summay is expected to contain Owner BSSID doesn't exists message.");
    }

    [TestMethod]
    public void Validate_NoDeviceAndGpsDeviceIdIsDefined_Error()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = IdGen.GetId();
      context.Owner.Type = CustomerTypeEnum.Dealer;
      context.IBDevice.Type = DeviceTypeEnum.MANUALDEVICE;
      context.IBDevice.GpsDeviceId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.GpsDeviceIdDefined, Validator.Errors[0].Item1);
      StringAssert.Contains(Validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.GPS_DEVICEID_DEFINED_MANUAL_DEVICE), "Summary is expected to contain GPSDeviceID is defined for NoDevice type.");

    }

    [TestMethod]
    public void Validate_GpsDeviceIdIsNotDefined_Error()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = IdGen.GetId();
      context.Owner.Type = CustomerTypeEnum.Dealer;
      context.IBDevice.Type = DeviceTypeEnum.Series521;
      context.IBDevice.GpsDeviceId = string.Empty;

      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.GpsDeviceIdNotDefined, Validator.Errors[0].Item1);
      StringAssert.Contains(Validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.GPS_DEVICEID_NOT_DEFINED), "Summary is expected to contain GPSDevice ID not defined message.");

    }

    [TestMethod]
    public void Validate_OwnerIsNotDealerOrAccount_Error()
    {
      var context = new AssetDeviceContext();
      context.Owner.Id = IdGen.GetId();
      context.Owner.Type = CustomerTypeEnum.Customer;
      context.IBDevice.Type = DeviceTypeEnum.PL321;
      context.IBDevice.GpsDeviceId = IdGen.GetId().ToString();

      Validator.Validate(context);

      Assert.AreEqual(1, Validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.DeviceOwnerTypeInvalid, Validator.Errors[0].Item1);
      StringAssert.Contains(Validator.Errors[0].Item2, string.Format(BssConstants.InstallBase.DEVICE_OWNER_TYPE_INVALID, context.Owner.Type), "Summary is expected to contain Customer can't hold devices message.");
    }
  }
}
