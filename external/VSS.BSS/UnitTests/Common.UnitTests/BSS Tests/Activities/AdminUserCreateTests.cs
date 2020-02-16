using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Hosted.VLCommon.Bss;
using VSS.UnitTest.Common;
using VSS.Hosted.VLCommon;

namespace UnitTests.BSS_Tests
{
  [TestClass]
  public class AdminUserCreateTests : BssUnitTestBase
  {
    protected Inputs Inputs;
    protected AdminUserCreate Activity;

    [TestInitialize]
    public void AdminUserCreateTests_Init()
    {
      Inputs = new Inputs();
      Activity = new AdminUserCreate();
    }

    [TestMethod]
    public void Execute_ThrowsExceptionWhileCreateAdminUser_ReturnsExceptionResult()
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
    public void Execute_CreateAdminUserReturnsNull_ReturnsErrorResult()
    {
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake((User)null);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext();
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs) as ErrorResult;

      Assert.IsNotNull(result);
      StringAssert.Contains(result.Summary, AdminUserCreate.USER_NULL_MESSAGE);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_NewAdminUserInformationNotDefined_SuccessMessageUpdateContext()
    {
      var fakeUser = new User();
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(fakeUser);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext { Id = IdGen.GetId() };
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsTrue(context.AdminUserExists);
      StringAssert.Contains(result.Summary, "Created ");
      StringAssert.Contains(result.Summary, AdminUserCreate.FIRSTNAME_NOT_DEFINED);
      StringAssert.Contains(result.Summary, AdminUserCreate.LASTNAME_NOT_DEFINED);
      StringAssert.Contains(result.Summary, AdminUserCreate.EMAIL_NOT_DEFINED);
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }

    [TestMethod]
    public void Execute_NewAdminDefined_SuccessMessageUpdatesContext()
    {
      var fakeUser = new User();
      BssCustomerServiceFake serviceFake = new BssCustomerServiceFake(fakeUser);
      Services.Customers = () => serviceFake;

      var context = new CustomerContext { Id = IdGen.GetId() };
      context.AdminUser.FirstName = "FIRSTNAME";
      context.AdminUser.LastName = "LASTNAME";
      context.AdminUser.Email = "EMAIL@DOMAIN.COM";
      Inputs.Add<CustomerContext>(context);

      var result = Activity.Execute(Inputs);

      Assert.IsTrue(context.AdminUserExists);
      StringAssert.Contains(result.Summary, "Created ");
      Assert.IsTrue(serviceFake.WasExecuted, "WasExecuted");
    }
  }
}
