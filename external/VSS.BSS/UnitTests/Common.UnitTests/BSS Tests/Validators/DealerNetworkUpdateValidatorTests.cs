using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class DealerNetworkUpdateValidatorTests : BssUnitTestBase
  {
    DealerNetworkUpdateValidator validator = null;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new DealerNetworkUpdateValidator();
    }

    [TestMethod]
    public void Validate_DealerNetworkChange_Success()
    {
      var serviceFake = new BssCustomerServiceFake();
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerNetwork = DealerNetworkEnum.None;
      context.New.DealerNetwork = DealerNetworkEnum.CAT;
    
      validator.Validate(context);

      Assert.AreEqual(0, validator.Errors.Count);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.IsTrue(serviceFake.WasExecuted);
    }

    [TestMethod]
    public void Validate_DealerNetworkChange_CustomeRelationshipExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(relationshipsExist: true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerNetwork = DealerNetworkEnum.None;
      context.New.DealerNetwork = DealerNetworkEnum.CAT;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.DealerNetwork, context.New.DealerNetwork, "CustomerRelationship"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_DealerNetworkChange_DeviceExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(devicesExist: true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerNetwork = DealerNetworkEnum.None;
      context.New.DealerNetwork = DealerNetworkEnum.CAT;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.DealerNetwork, context.New.DealerNetwork, "Device"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_DealerNetworkChange_ServiceViewExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(serviceViewsExist: true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.DealerNetwork = DealerNetworkEnum.None;
      context.New.DealerNetwork = DealerNetworkEnum.CAT;

      validator.Validate(context);

      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.DealerNetwork, context.New.DealerNetwork, "Active ServiceViews"), validator.Errors[0].Item2);
    }
  }
}
