using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class CustomerTypeUpdateValidatorTests : BssUnitTestBase
  {
    CustomerTypeUpdateValidator validator;

    [TestInitialize]
    public void TestInitialize()
    {
      validator = new CustomerTypeUpdateValidator();
    }

    #region Account CustomerType change

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeAccount_Success()
    {
      var serviceFake = new BssCustomerServiceFake(false);

      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.Type = CustomerTypeEnum.Account;
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);
      Assert.AreEqual(0, validator.Errors.Count);
      Assert.AreEqual(0, validator.Warnings.Count);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeAccount_CustomerRelationshipExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(relationshipsExist: true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.Type = CustomerTypeEnum.Account;
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);
      
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "CustomerRelationship"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeAccount_DeviceExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(devicesExist: true);
      Services.Customers = () => serviceFake;
      var context = new CustomerContext();
      context.Type = CustomerTypeEnum.Account;
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "Device"), validator.Errors[0].Item2);
    }

    #endregion

    #region Customer CustomerType change

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeCustomer_Success()
    {
      var serviceFake = new BssCustomerServiceFake();
      Services.Customers = () => serviceFake;
      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Customer; 
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Errors.Count);
      Assert.AreEqual(0, validator.Warnings.Count);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeCustomer_CustomerRelationshipExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(relationshipsExist: true);
      Services.Customers = () => serviceFake;

      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Customer; 
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "CustomerRelationship"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeCustomer_ActiveServiceViewExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(serviceViewsExist: true);
      Services.Customers = () => serviceFake;
      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Customer; 
      context.New.Type = CustomerTypeEnum.Dealer;

      validator.Validate(context);
      
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "Active ServiceViews"), validator.Errors[0].Item2);
    }

    #endregion

    #region Dealer CustomerType change

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeDealer_Success()
    {
      var serviceFake = new BssCustomerServiceFake();
      Services.Customers = () => serviceFake;
      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Dealer; 
      context.New.Type = CustomerTypeEnum.Customer;

      validator.Validate(context);

      Assert.AreEqual(0, validator.Errors.Count);
      Assert.AreEqual(0, validator.Warnings.Count);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeDealer_CustomerRelationshipExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(relationshipsExist: true);
      Services.Customers = () => serviceFake;
      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Account; 
      context.New.Type = CustomerTypeEnum.Customer;

      validator.Validate(context);
      
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "CustomerRelationship"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeDealer_DeviceExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(devicesExist: true);
      Services.Customers = () => serviceFake;

      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Dealer; 
      context.New.Type = CustomerTypeEnum.Customer;

      validator.Validate(context);
      
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "Device"), validator.Errors[0].Item2);
    }

    [TestMethod]
    public void Validate_CustomerTypeChange_CurrentTypeDealer_ActiveServiceViewExists_Error()
    {
      var serviceFake = new BssCustomerServiceFake(serviceViewsExist: true);
      Services.Customers = () => serviceFake;
      CustomerContext context = new CustomerContext();
      context.Type = CustomerTypeEnum.Dealer; 
      context.New.Type = CustomerTypeEnum.Customer;

      validator.Validate(context);
      
      Assert.AreEqual(0, validator.Warnings.Count);
      Assert.AreEqual(1, validator.Errors.Count);
      Assert.AreEqual(BssFailureCode.CustomerTypeChangeInvalid, validator.Errors[0].Item1);
      Assert.AreEqual(string.Format(BssConstants.Hierarchy.CUSTOMER_TYPE_CHANGE_INVALID, context.Type, context.New.Type, "Active ServiceViews"), validator.Errors[0].Item2);
    }

    #endregion
  }
}