using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon.Bss.Schema.V2;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class InstallBaseValidatorTests : BssUnitTestBase
  {

    InstallBaseValidator validator;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new InstallBaseValidator();
    }

    public void Validate_ValidMessage_Success()
    {
      var message = BSS.IBCreated.Build();
      validator.Validate(message);
      Assert.AreEqual(0, validator.Errors.Count);
    }

    [TestMethod]
    public void Validate_ActionInvalid_Error()
    {
      var message = BSS.IB(ActionEnum.Deleted).Build();

      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.ActionInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.ACTION_INVALID_FOR_MESSAGE, ActionEnum.Deleted, "InstallBase"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_SerialNumberVinNotDefined_Error()
    {
      var message = BSS.IBCreated.EquipmentSN(string.Empty).Build();
      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.EquipmentSNNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.InstallBase.EQUIPMENTSN_NOT_DEFINED, validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_MakeCodeNotDefined_Error()
    {
      var message = BSS.IBCreated.MakeCode(string.Empty).Build();
      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.MakeCodeNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.InstallBase.MAKE_CODE_NOT_DEFINED, validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_OwnerBssIdNotDefined_Error()
    {
      var message = BSS.IBCreated.OwnerBssId(string.Empty).Build();
      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.OwnerBssNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.InstallBase.OWNER_BSSID_NOT_DEFINED, validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_PartNumberNotDefined_Error()
    {
      var message = BSS.IBCreated.PartNumber(string.Empty).Build();
      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.PartNumberNotDefined, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.InstallBase.PART_NUMBER_NOT_DEFINED, validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_IBKeyNotDefined_Error()
    {
      var message = BSS.IBUpdated.IBKey(string.Empty).Build();
      validator.Validate(message);
      Assert.AreEqual(0, validator.Warnings.Count, "No warnings expected.");
      Assert.AreEqual(1, validator.Errors.Count, "One error expected.");
      Assert.AreEqual(BssConstants.IBKEY_NOT_DEFINED, validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_EquipmentVINTooLong_Error()
    {
      var message = BSS.IBUpdated.EquipmentVIN("51character_stringAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA").Build();
      validator.Validate(message);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.EquipmentVINInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(BssConstants.InstallBase.EQUIPMENTVIN_TOO_LONG, validator.Errors[0].Item2);
    }
  }
}
