using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AdminUserUpdateTests: BssUnitTestBase
  {
    const string SUCCESS_MESSAGE = @"Updated Admin User with First Name: ";    
    const string ADMIN_USER_VERIFIED = @"Admin User is already verified with Email:";
    const string ADMIN_USER_DOESNT_EXIST = @"Admin User doesn't exist for Customer:";
    const string SKIP_UPDATE_MESSAGE = @"Skipping Admin User Update";

    protected Inputs Inputs;
    protected AdminUserUpdate Activity;

    [TestInitialize]
    public void AdminUserUpdateTests_Init()
    {
      Inputs = new Inputs();
      Activity = new AdminUserUpdate();
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileUpdateAdminUser_ReturnsExceptionResult()
    {
      var serviceFake = new BssCustomerServiceExceptionFake();
      Services.Customers = () => serviceFake;
      Inputs.Add<CustomerContext>(new CustomerContext());

      var result = Activity.Execute(Inputs) as ExceptionResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, "Failed ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_UpdateAdminUserWithInactiveAdminUser_SuccessResultWithErrorSummary()
    {
      var fakeUser = new User() { Active = false };
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(fakeUser);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext() { Id = IdGen.GetId() };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, ADMIN_USER_DOESNT_EXIST);
      StringAssert.Contains(result.Summary, SKIP_UPDATE_MESSAGE);      
    }

    [TestMethod]
    public void Execute_UpdateAdminUserWithNoExistingAdminUser_SuccessMessageWithSummary()
    {      
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake((User)null);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext { Id = IdGen.GetId() };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, ADMIN_USER_DOESNT_EXIST);
      StringAssert.Contains(result.Summary, SKIP_UPDATE_MESSAGE);      
    }

    [TestMethod]
    public void Execute_UpdateAdminUserWithVerifiedAdminUser_SuccessMessageWithSummary()
    {
      var fakeUser = new User() { Active = true, IsEmailValidated = true, EmailVerificationGUID = IdGen.StringId(), EmailVerificationUTC = System.DateTime.UtcNow };
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(fakeUser);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext { Id = IdGen.GetId(), AdminUser = new AdminUserDto() };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, ADMIN_USER_VERIFIED);
      StringAssert.Contains(result.Summary, SKIP_UPDATE_MESSAGE);      
    }

    [TestMethod]
    public void Execute_UpdateAdminUserWithValidAdminUserDetails_SuccessMessageWithSummary()
    {
      var fakeUser = new User() { Active = true, IsEmailValidated = false, EmailVerificationGUID = IdGen.StringId(), EmailVerificationUTC = System.DateTime.UtcNow };
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(fakeUser);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext { Id = IdGen.GetId(), AdminUser = new AdminUserDto() };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary,SUCCESS_MESSAGE);      
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }
  }
}
