using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using VSS.Nighthawk.Instrumentation;

using VSS.Hosted.VLCommon;
using VSS.UnitTest.Common;
using System.Configuration;
using UnitTests.WebApi;

namespace UnitTests
{
  [TestClass()]
  public class SessionAPITest : UnitTestBase
  {
    private static readonly int verificationReminderDayLimit = Convert.ToInt32(ConfigurationManager.AppSettings["VerificationReminderDayLimit"]);
    private static readonly int verificationPendingDayLimit = Convert.ToInt32(ConfigurationManager.AppSettings["VerificationPendingDayLimit"]);
    private static readonly double passwordExpiryDaysLimit = Convert.ToDouble(ConfigurationManager.AppSettings["PasswordExpiryDaysLimit"]);

    [TestMethod()]
    [DatabaseTest]
    public void AddClientMetrics_Success()
    {
      //This test method will exercise the MetricsRecorder.AddClientMetricRecords by creating a list of metric records and passing them to
      // the static method of the MetricsRecorder class.  There is no return value.  This test will fail if there is an exception thrown during
      // the record insertion.  To check by hand, you can view the datatable (NH_Metrics.CodeInstrumentationMetrics) with the (NOLOCK) attribute
      // while debugging the test method.  I'll assert = true at the end for completenesss.

      //create data object
      ClientMetric cm1 = new ClientMetric()
      {
        startUTC = DateTime.UtcNow.AddMinutes(-10),
        endUTC = DateTime.UtcNow.AddMinutes(-3),
        className = "AssetStuff",
        methodName = "GetSummary",
        source = "clientSource",
        context = "ActiveUserID(243),CustomerID(137),UserName('aaronemp'),SessionID(23e2bc04bbc244d8bd324bf96309dd9d)"
      };

      ClientMetric cm2 = new ClientMetric()
      {
        startUTC = DateTime.UtcNow.AddMinutes(-1),
        endUTC = DateTime.UtcNow.AddMinutes(-0.5),
        className = "FleetStuff",
        methodName = "GetSummaryAssets",
        source = "clientSource",
        context = "ActiveUserID(43),CustomerID(237),UserName('aaronemp'),SessionID(23e2bc04bbc244d8bd324bf9630ddffdd)"
      };

      List<ClientMetric> lst = new List<ClientMetric>();
      lst.Add(cm1);
      lst.Add(cm2);

      MetricsRecorder.AddClientMetricRecords(lst);

      Assert.IsTrue(true, "This method will have no return value");
    }

    [TestMethod()]
    [DatabaseTest]
    public void LoginCheckGood()
    {
      User foo = TestData.TestCustomerUser;
      bool result = API.Session.LoginCheck(TestDataHelper.CUSTOMER_USER_USERNAME);
      Assert.IsTrue(result, "Expect a true return value indicating that there is NOT an active session in progress for this user");
    }

    [TestMethod()]
    [DatabaseTest]
    public void LoginCheckBad()
    {
      User foo = TestData.TestCustomerUser;
      SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      Assert.IsNotNull(actual, "Unit test active user should be non-null");

      bool result = API.Session.LoginCheck(TestDataHelper.CUSTOMER_USER_USERNAME);
      Assert.IsFalse(result, "Expect a false return value indicating that there is a active session in progress for this user already");
    }


    [DatabaseTest]
    [TestMethod()]
    public void Login_Success()
    {
      User foo = TestData.TestCustomerUser;
      SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      Assert.IsNotNull(actual, "Unit test active user should be non-null");
    }

    [TestMethod()]
    [DatabaseTest]
    public void Logout_Success()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User foo = TestData.TestCustomerUser;

      SessionAPI sessionAPI = new SessionAPI();

      SessionContext session = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      bool loggedOut = API.Session.Logout(session);
      Assert.IsTrue(loggedOut, "Should be able to logout active user");

    }


    [TestMethod()]
    [DatabaseTest]
    public void UpdateLastLoginUTC_InLogout_ExpectSuccess()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User user = TestData.TestCustomerAdminUser;

      SessionAPI sessionAPI = new SessionAPI();

      SessionContext session = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      bool loggedOut = API.Session.Logout(session);

      User userToValidate = (from u in session.NHOpContext.User where u.ID == session.UserID && u.Name.Equals(session.UserName) select u).FirstOrDefault();

      if (userToValidate != null)
      {
        Assert.IsNotNull(userToValidate.LastLoginUTC);
        Assert.AreEqual(DateTime.UtcNow.ToShortDateString(), userToValidate.LastLoginUTC.Value.ToShortDateString());
        Assert.AreEqual(DateTime.UtcNow.ToShortTimeString(), userToValidate.LastLoginUTC.Value.ToShortTimeString());

      }
    }



    [DatabaseTest]
    [TestMethod()]
    public void AdminLogin_Success()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User foo = TestData.TestCustomerAdmin;

      SessionAPI sessionAPI = new SessionAPI();

      SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_ADMIN_USERNAME, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      Assert.IsNotNull(actual, "Unit test active user should be non-null");
      Assert.IsTrue(actual.UserID.HasValue, "Invalid login");

      SessionContext session = API.Session.Validate(actual.SessionID);
      bool loggedOut = API.Session.Logout(session);
      Assert.IsTrue(loggedOut, "Should be able to logout active user");
    }

    [DatabaseTest]
    [TestMethod()]
    public void ImpersonatedLogin_Success()
    {
      SessionContext trimbleOpSession = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);

      var impersonatedUser = API.Session.ImpersonatedLogin(trimbleOpSession, TestData.TestCustomerUser.Name);
      Assert.IsNotNull(impersonatedUser, "failed impersonated login");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Login_FailureNoUsername()
    {
      // Expect exception because userName is missing.
      API.Session.Login(string.Empty, "wacky");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Login_FailureNoPassword()
    {
      // Expect exception because password is missing.
      API.Session.Login("Fred", null);
    }

    [DatabaseTest]
    [TestMethod]
    public void Login_CustomerInactive_Failure()
    {
      var customer = Entity.Customer.EndCustomer.Save();
      var user = Entity.User.ForCustomer(customer).WithLanguage(TestData.English).Password("password").Save();
      customer.IsActivated = false;
      Ctx.OpContext.SaveChanges();

      try
      {
        new SessionAPI().Login(user.Name, "password");
        Assert.Fail("Expected exception not thrown.");
      }
      catch (Exception ex)
      {
        Assert.AreEqual("invalidUser001", ex.Message);
      }
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_FailureBadPassword()
    {
      SessionAPI sessionAPI = new SessionAPI();

      User u = LoginLoopWithFailureHandler(sessionAPI.NumberOfLoginAttempts.Value);
      Assert.AreEqual(sessionAPI.NumberOfLoginAttempts, u.LogOnFailedCount, "Expect the failure counts to be stored");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_FailedBeforeRetryDuration()
    {
      SessionAPI sessionAPI = new SessionAPI();

      User u = LoginLoopWithFailureHandler(sessionAPI.NumberOfLoginAttempts.Value);
      Assert.AreEqual(sessionAPI.NumberOfLoginAttempts, u.LogOnFailedCount, "Expect the failure counts to be stored");

      //modify the retry duration to try again within the duration.
      u.LogOnFirstFailedUTC = DateTime.UtcNow.AddMinutes(-(sessionAPI.RetryDurationInMinutes.Value - 2));
      Ctx.OpContext.SaveChanges();

      //User should not be able to login till the lock out time period 
      //expires as the account is locked
      u = LoginLoopWithFailureHandler(1);
      Assert.AreEqual(sessionAPI.NumberOfLoginAttempts, u.LogOnFailedCount, "Expect the failure count to stay at the max");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_FailureAfterRetryDuration()
    {
      SessionAPI sessionAPI = new SessionAPI();

      User u = LoginLoopWithFailureHandler(sessionAPI.NumberOfLoginAttempts.Value);
      Assert.AreEqual(sessionAPI.NumberOfLoginAttempts, u.LogOnFailedCount, "Expect the failure counts to be stored");

      //reset the first failed login time to future time.
      TestData.TestCustomerUser.LogOnFirstFailedUTC = DateTime.UtcNow.AddMinutes(-(sessionAPI.RetryDurationInMinutes.Value + 1));
      Ctx.OpContext.SaveChanges();

      u = LoginLoopWithFailureHandler(1);
      Assert.AreEqual(sessionAPI.NumberOfLoginAttempts, u.LogOnFailedCount, "Expect the failure count to stay at max");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_FailureAfterLockoutDuration()
    {
      string salt = HashUtils.CreateSalt(5);
      User failure = new User
      {
        fk_CustomerID = 50,
        Name = "ErnieUser",
        PasswordHash = API.Session.GetPasswordHash("ILoveBurt", salt),
        Salt = salt,
        EmailContact = "Ernie@TheStreet.com",
        UpdateUTC = DateTime.UtcNow,
        fk_LanguageID = 1,
        GlobalID = "gggggglobal",
        FirstName = "Ernie",
        LastName = "Puppet",
        Active = true,
        LogOnFailedCount = 5,
        LogOnFirstFailedUTC = DateTime.UtcNow,
        LogOnLastFailedUTC = DateTime.UtcNow
      };
      Ctx.OpContext.User.AddObject(failure);
      Ctx.OpContext.SaveChanges();

      //User should not be able to login till the cool-off period 
      //expires as the account is locked
      User u = LoginLoopWithFailureHandler(1);
      Assert.AreEqual(1, u.LogOnFailedCount, "Count should be 1");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_SuccessAfterLockoutDuration()
    {
      string salt = HashUtils.CreateSalt(5);
      User failure = new User
      {
        fk_CustomerID = 50,
        Name = "ErnieUser",
        PasswordHash = API.Session.GetPasswordHash("ILoveBurt", salt),
        Salt = salt,
        EmailContact = "Ernie@TheStreet.com",
        TimezoneName = "Coordinated Universal Time",
        UpdateUTC = DateTime.UtcNow,
        fk_LanguageID = 1,
        GlobalID = "gggggglobal",
        FirstName = "Ernie",
        LastName = "Puppet",
        Active = true,
        LogOnFailedCount = 5,
        LogOnFirstFailedUTC = new DateTime(1980, 1, 1),
        LogOnLastFailedUTC = new DateTime(1980, 2, 1),
        PwdExpiryUTC = DateTime.UtcNow.AddMonths(1),
        IsEmailValidated = true
      };
      Ctx.OpContext.User.AddObject(failure);
      Ctx.OpContext.SaveChanges();

      //User should be able to login with the retry period and lock out period expired
      SessionContext sesh = API.Session.Login(failure.Name, "ILoveBurt");
      Assert.IsNotNull(sesh, "Login should succeed");
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_SuccessLoginWithSessionID()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      var actual = API.Session.LoginWithSessionID(expected.SessionID);
      Assert.AreEqual(expected.ActiveUserID, actual.ActiveUserID, "Incorrect activeUser");
      Assert.AreEqual(expected.SessionID, actual.SessionID, "Incorrect activeUser");
      Assert.AreEqual(expected.UserID.Value, actual.UserID.Value, "Incorrect User");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Login_FailureWithSessionIDBadSession()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      var actual = API.Session.LoginWithSessionID("90900990");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Login_WithSessionIDEmptySession()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      var actual = API.Session.LoginWithSessionID(string.Empty);
    }

    [DatabaseTest]
    [TestMethod]
    public void Login_UserInactive_Failure()
    {
        var customer = Entity.Customer.EndCustomer.Save();
        var user = Entity.User.ForCustomer(customer).WithLanguage(TestData.English).Password("password").Save();
        customer.IsActivated = true;
        user.Active = false;
        Ctx.OpContext.SaveChanges();

        var result = new SessionAPI();
        AssertEx.Throws<UnauthorizedAccessException>(() =>
          result.LoginCheck(user.Name),
          "InactiveUser");
        
    }

    [DatabaseTest]
    [TestMethod()]
    public void Login_SuccessLoginWithKey()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      string key = "1234567890";
      bool saved = API.Session.SaveUserTemporaryKey(session, key);
      var actual = API.Session.LoginWithKey(key);
      Assert.AreEqual(expected.ActiveUserID, actual.ActiveUserID, "Incorrect activeUser");
      Assert.AreEqual(expected.SessionID, actual.SessionID, "Incorrect activeUser");
      Assert.AreEqual(expected.UserID.Value, actual.UserID.Value, "Incorrect User");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Login_FailureWithKeyReused()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      string key = "1234567890";
      bool saved = API.Session.SaveUserTemporaryKey(session, key);
      API.Session.LoginWithKey(key);
      var actual = API.Session.LoginWithKey(key);
      //Assert.IsNull(actual);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Login_FailureWithKeyBadSession()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      var actual = API.Session.LoginWithKey("1234567890");
      //Assert.IsNull(actual);
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Login_WithKeyEmptySession()
    {
      var expected = API.Session.Login(TestData.TestCustomerAdmin.Name, TestDataHelper.CUSTOMER_ADMIN_PASSWORD);
      SessionContext session = API.Session.Validate(expected.SessionID);
      var actual = API.Session.LoginWithKey(string.Empty);
    }

    [DatabaseTest]
    [TestMethod()]
    public void PasswordIsValid_Success()
    {
      SessionContext actual = API.Session.Login(TestData.TestCustomerUser.Name, TestDataHelper.CUSTOMER_USER_PASSWORD);

      bool result = API.Session.PasswordIsValid(TestDataHelper.CUSTOMER_USER_PASSWORD, actual.PasswordHash, actual.UserSalt);
      Assert.IsTrue(result, "Password hash algorithm works reliably");
    }

    [DatabaseTest]
    [TestMethod()]
    public void PasswordIsValid_Failure()
    {
      SessionContext actual = API.Session.Login(TestData.TestCustomerUser.Name, TestDataHelper.CUSTOMER_USER_PASSWORD);

      bool result = API.Session.PasswordIsValid(TestDataHelper.CUSTOMER_USER_PASSWORD, actual.PasswordHash, actual.UserSalt + "ZZZ");
      Assert.IsFalse(result, "Password hash algorithm works reliably");
    }

    [DatabaseTest]
    [TestMethod()]
    [ExpectedException(typeof(SecurityException))]
    public void Validate_FailureInvalidSessionID()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User foo = TestData.TestCustomerUser;

      SessionAPI sessionAPI = new SessionAPI();

      SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      Assert.IsNotNull(actual, "Unit test active user should be non-null");

      // Should fail with SecurityException
      API.Session.Validate(actual.SessionID + "x");
    }

    [DatabaseTest]
    [TestMethod()]
    public void LoginFailure_PasswordHasExpired()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User foo = TestData.TestPasswordExpieryCustomer;
      SessionAPI sessionAPI = new SessionAPI();
      bool exceptionOccoured = false;
      try
      {
        SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      }
      catch (UnauthorizedAccessException e)
      {
        Assert.AreEqual("passwordExpired", e.Message, "Wrong exception being thrown");
        exceptionOccoured = true;
      }

      Assert.IsTrue(exceptionOccoured, "Exception did not occour");
    }

    [DatabaseTest]
    [TestMethod()]
    public void PasswordHasExpired_SSOUser_LoginSuccess()
    {
        //Must access this test data customer to force the system to create the user and store in db.
        User foo = TestData.TestPasswordExpieryCustomer;
        SessionAPI sessionAPI = new SessionAPI();
        
        SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD, isSSO: true);
        
        Assert.IsNotNull(actual);
    }

    [DatabaseTest]
    [TestMethod()]
    public void LoginFailure_FirstTimeLogin()
    {
      //Must access this test data customer to force the system to create the user and store in db.
      User foo = TestData.FirstTimeLoginCustomer;
      SessionAPI sessionAPI = new SessionAPI();
      bool exceptionOccoured = false;
      try
      {
        SessionContext actual = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);
      }
      catch (UnauthorizedAccessException e)
      {
        Assert.AreEqual("firstTimeLogin", e.Message, "Wrong exception being thrown");
        exceptionOccoured = true;
      }

      Assert.IsTrue(exceptionOccoured, "Exception did not occour");

    }

    [DatabaseTest]
    [TestMethod()]
    public void UpdateLastLoginUTC_ExpectSuccess()
    {
      User user = TestData.TestCustomerAdminUser;
      var session = API.Session.Login(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);

      User userToValidate = (from u in session.NHOpContext.User
                             where u.Name.Equals(TestDataHelper.CUSTOMER_USER_USERNAME) && u.Active
                             select u).FirstOrDefault();

      if (userToValidate != null)
      {
        Assert.IsNotNull(userToValidate.LastLoginUTC);
        Assert.AreEqual(DateTime.UtcNow.ToShortDateString(), userToValidate.LastLoginUTC.Value.ToShortDateString());
        Assert.AreEqual(DateTime.UtcNow.ToShortTimeString(), userToValidate.LastLoginUTC.Value.ToShortTimeString());
      }
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetUserDetailsForNonVerifiedUser_VerificationReminderDetails_SuccessWhenNonVerifiedUserAccessFirstTime()
    {      
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);

      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();      
      userDetails.EmailVerificationGUID = null;
      userDetails.EmailVerificationUTC = null;
      userDetails.EmailVerificationTrackingUTC = null;
      Ctx.OpContext.SaveChanges();

      var target = new SessionAPI();
      SessionContext result = target.GetUserSessionDetailsForNonVerifiedUser(session.UserName);

      Assert.IsTrue(result.IsVerificationReminder, "Verification Reminder Should be true");
      Assert.IsFalse(result.IsVerificationPending, "Verification Pending Should be false");
      Assert.AreEqual(verificationReminderDayLimit, result.VerificationRemainingDays, "Verification remaining day count should match.");     
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetUserDetailsForNonVerifiedUser_VerificationReminderDetails_SuccessWhenEmailnotVerifiedWithin7Days()
    {
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
            
      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();
      userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow.AddDays(-1);
      userDetails.EmailVerificationGUID = null;
      userDetails.EmailVerificationUTC = null;
      Ctx.OpContext.SaveChanges();
      
      var target = new SessionAPI();
      SessionContext result = target.GetUserSessionDetailsForNonVerifiedUser(session.UserName);

      Assert.IsTrue(result.IsVerificationReminder, "Verification Reminder Should be true");
      Assert.IsFalse(result.IsVerificationPending, "Verification Pending Should be false");
      Assert.AreEqual((verificationReminderDayLimit-1), result.VerificationRemainingDays, "Verification remaining day count should match.");
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetUserDetailsForNonVerifiedUser_VerificationReminderDetails_SuccessWhenEmailnotVerifiedMorethan7Days()
    {      
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
            
      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();
      userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow.AddDays(-(verificationReminderDayLimit + 1));
      userDetails.EmailVerificationGUID = null;
      userDetails.EmailVerificationUTC = null;
      Ctx.OpContext.SaveChanges();

      var target = new SessionAPI();
      SessionContext result = target.GetUserSessionDetailsForNonVerifiedUser(session.UserName);

      Assert.IsTrue(result.IsVerificationReminder, "Verification Reminder Should be true");
      Assert.IsFalse(result.IsVerificationPending, "Verification Pending Should be false");
      Assert.AreEqual(0, result.VerificationRemainingDays, "Verification remaining day count should match.");
    }

    [DatabaseTest]
    [TestMethod()]    
    public void GetUserDetailsForNonVerifiedUser_FailureWhenAlreadyVerifiedUserTrysToGetVerificationDetails()
    {
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
      
      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();
      DateTime dt = DateTime.UtcNow.AddHours(-5);
      userDetails.EmailVerificationTrackingUTC = userDetails.EmailVerificationUTC = dt;
      userDetails.EmailVerificationGUID = new Guid().ToString();
      Ctx.OpContext.SaveChanges();

      var target = new SessionAPI();      
      AssertEx.Throws<InvalidOperationException>(() =>
        target.GetUserSessionDetailsForNonVerifiedUser(session.UserName),
        "EmailID of the user has been already verified");
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetUserDetailsForNonVerifiedUser_VerificationPendingDetails_SuccessWhenEmailnotVerifiedWithin1Day()
    {
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
            
      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();
      DateTime dt = DateTime.UtcNow.AddHours(-5);
      userDetails.EmailVerificationTrackingUTC = userDetails.EmailVerificationUTC = dt;
      userDetails.EmailVerificationGUID = new Guid().ToString();
      userDetails.IsEmailValidated = false;
      Ctx.OpContext.SaveChanges();

      var target = new SessionAPI();
      SessionContext result = target.GetUserSessionDetailsForNonVerifiedUser(session.UserName);

      Assert.IsFalse(result.IsVerificationReminder, "Verification Reminder Should be false");
      Assert.IsTrue(result.IsVerificationPending, "Verification Pending Should be true");
      Assert.AreEqual(verificationPendingDayLimit, result.VerificationRemainingDays, "Verification remaining day count should match.");
    }

    [DatabaseTest]
    [TestMethod()]
    public void GetUserDetailsForNonVerifiedUser_VerificationPendingDetails_SuccessWhenEmailnotVerifiedMorethan1Day()
    {      
      SessionContext session = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
            
      User userDetails = (from user in Ctx.OpContext.User
                          where user.ID == session.UserID
                          select user).SingleOrDefault();
      DateTime dt = DateTime.UtcNow.AddDays(-(verificationPendingDayLimit + 1));
      userDetails.EmailVerificationTrackingUTC = userDetails.EmailVerificationUTC = dt;
      userDetails.EmailVerificationGUID = new Guid().ToString();
      userDetails.IsEmailValidated = false;
      Ctx.OpContext.SaveChanges();

      var target = new SessionAPI();
      SessionContext result = target.GetUserSessionDetailsForNonVerifiedUser(session.UserName);

      Assert.IsFalse(result.IsVerificationReminder, "Verification Reminder Should be false");
      Assert.IsTrue(result.IsVerificationPending, "Verification Pending Should be true");
      Assert.AreEqual(0, result.VerificationRemainingDays, "Verification remaining day count should match.");
    }

    [DatabaseTest]
    [TestMethod()]
    public void SkipEmailVerificationValidationForSsoUsersSuccess()
    {
      var ssoUser =
        Entity.User.ForCustomer(TestData.TestCustomer)
          .Username("TestSSOUser")
          .EmailValidated(false)
          .WithFeature(x => x.App(FeatureAppEnum.NHWeb)).Save();
      SessionContext actual = API.Session.SSOLogin(ssoUser.Name);

      Assert.IsNotNull(actual, "SSO users should skip email verification validation");
    }

    #region Impersonation Combinations

    [TestMethod]
    [DatabaseTest]
    public void TrmbOpsAsUserXWithActiveUserX()
    {
      // Normal customer, user "mrX", has fleet of 3 assets, two selected in an active session
      Customer normalCustomer = Entity.Customer.EndCustomer.SyncWithRpt().Save();
      User mrX = Entity.User.Username("MrX").Password("mrXLovesMrsX").ForCustomer(normalCustomer).Save();
      Asset customerAsset1 = Entity.Asset.SerialNumberVin("AAA").WithDevice(Entity.Device.MTS521.Save()).SyncWithRpt().Save();
      Asset customerAsset2 = Entity.Asset.SerialNumberVin("BBB").WithDevice(Entity.Device.MTS522.Save()).SyncWithRpt().Save();
      Asset customerAsset3 = Entity.Asset.SerialNumberVin("CCC").WithDevice(Entity.Device.MTS523.Save()).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset3.Device).WithView(view => view.ForAsset(customerAsset3).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      ActiveUser activeMrX = Entity.ActiveUser.ForUser(mrX).Save();
      SessionContext mrXSession1 = Helpers.Sessions.GetContextFor(activeMrX, true);
      Helpers.WorkingSet.Select(activeMrX, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

      int count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
                   where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
                     && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
                   select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "MrX has asset 1 and 3 selected in his working set");
      int rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                      where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
                       && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
                      select 1).Count();
      Assert.AreEqual(2, rptCount, "MrX has asset 1 and 3 selected in his working set in NH_RPT also");

      // Trimble Ops user does an impersonation of "MrX" = TrmbOpsAsMrX
      // This does not expire MrX's session.
      // TrimbleOpsAsMrX does not see MrX's working set
      // TrimbleOpsAsMrX can make their own asset selection, without affected MrX's asset selection

      SessionContext opsSession1 = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);
      SessionContext opsAsMrX = API.Session.ImpersonatedLogin(opsSession1, mrX.Name);
      Assert.AreEqual(mrX.Name, opsAsMrX.UserName, "Impersonated user properties expected in SessionContext");
      Assert.AreEqual(normalCustomer.Name, opsAsMrX.CustomerName, "Impersonated user's customer properties are expected in SessionContext");
      ActiveUser opsAsMrXActiveUser = (from au in Ctx.OpContext.ActiveUserReadOnly
                                       where au.ID == opsAsMrX.ActiveUserID
                                       select au).Single();

      Helpers.WorkingSet.Select(opsAsMrXActiveUser, new List<long> { customerAsset2.AssetID });

      bool mrXSession1Expired = (from au in Ctx.OpContext.ActiveUserReadOnly
                                 where au.SessionID == mrXSession1.SessionID
                                 select au.Expired).Single();
      Assert.IsFalse(mrXSession1Expired, "TrmbOps impersonators must not log out the user they impersonate");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == opsAsMrX.ActiveUserID
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(3, count, "Trimble Ops can see all of impersonated user's assets");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
                 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
                 && aws.Selected
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "MrX's asset working set is not changed by trmbOpsAsMrX");
      rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                  where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
                   && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
                  select 1).Count();
      Assert.AreEqual(2, rptCount, "MrX's asset working set is not changed by trmbOpsAsMrX, in NH_RPT also");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == opsAsMrX.ActiveUserID
                 && aws.fk_AssetID == customerAsset2.AssetID
                 && aws.Selected
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(1, count, "TrmbOpsAsMrX's asset working set is separate to mrX's");
      rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                  where ws.ifk_ActiveUserID == opsAsMrX.ActiveUserID
                   && ws.fk_DimAssetID == customerAsset2.AssetID
                  select 1).Count();
      Assert.AreEqual(1, rptCount, "TrmbOpsAsMrX's asset working set is separate to mrX's, in nh_rpt also");

    }

    [TestMethod]
    [DatabaseTest]
    public void DealerAsUserXWithActiveUserX()
    {
      // Normal customer, user "mrX", has fleet of 3 assets, two selected in an active session
      Customer normalCustomer = Entity.Customer.EndCustomer.SyncWithRpt().Save();
      User mrX = Entity.User.Username("MrX").Password("mrXLovesMrsX").ForCustomer(normalCustomer).Save();
      Asset customerAsset1 = Entity.Asset.SerialNumberVin("AAA").WithDevice(Entity.Device.MTS521.Save()).SyncWithRpt().Save();
      Asset customerAsset2 = Entity.Asset.SerialNumberVin("BBB").WithDevice(Entity.Device.MTS522.Save()).SyncWithRpt().Save();
      Asset customerAsset3 = Entity.Asset.SerialNumberVin("CCC").WithDevice(Entity.Device.MTS523.Save()).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset3.Device).WithView(view => view.ForAsset(customerAsset3).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      ActiveUser activeMrX = Entity.ActiveUser.ForUser(mrX).Save();
      SessionContext mrXSession1 = Helpers.Sessions.GetContextFor(activeMrX, true);
      Helpers.WorkingSet.Select(activeMrX, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

      int count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
                   where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
                     && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
                   select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "MrX has asset 1 and 3 selected in his working set");
      int rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                      where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
                       && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
                      select 1).Count();
      Assert.AreEqual(2, rptCount, "MrX has asset 1 and 3 selected in his working set in NH_RPT also");

      // MrX has relationship with Dealer, through an Account
      Customer dealer = Entity.Customer.Dealer.SyncWithRpt().Save();
      Customer account = Entity.Customer.Account.SyncWithRpt().Save();
      CustomerRelationship dealerAccount = Entity.CustomerRelationship.Relate(dealer, account).Save();
      CustomerRelationship mrXAccount = Entity.CustomerRelationship.Relate(normalCustomer, account).Save();
      // Dealer has views on two of the three assets
      Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(dealer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(dealer)).SyncWithRpt().Save();
      User dealerUser = Entity.User.Username("DealerUser").Password("ISellBigTrucks").ForCustomer(dealer).Save();
      ActiveUser dealerAU = Entity.ActiveUser.ForUser(dealerUser).Save();
      Helpers.WorkingSet.Populate(dealerAU);

      // Dealer does an impersonation of "MrX" = DealerAsMrX
      // This does not expire MrX's session.
      // DealerAsMrX asset population is intersection of Dealers assets and MrX's assets
      // DealerAsMrX does not see MrX's working set
      // DealerAsMrX can make their own asset selection, without affected MrX's asset selection
      SessionContext dealerSession1 = Helpers.Sessions.GetContextFor(dealerAU);
      SessionContext dealerAsMrX = API.Session.ImpersonatedLogin(dealerSession1, mrX.Name);
      Assert.AreEqual(mrX.Name, dealerAsMrX.UserName, "Impersonated user properties expected in SessionContext");
      Assert.AreEqual(normalCustomer.Name, dealerAsMrX.CustomerName, "Impersonated user's customer properties are expected in SessionContext");

      ActiveUser dealerAsMrXActiveUser = (from au in Ctx.OpContext.ActiveUserReadOnly
                                          where au.ID == dealerAsMrX.ActiveUserID
                                          select au).Single();
      Helpers.WorkingSet.Select(dealerAsMrXActiveUser, new List<long> { customerAsset2.AssetID });

      bool mrXSession1Expired = (from au in Ctx.OpContext.ActiveUserReadOnly
                                 where au.SessionID == mrXSession1.SessionID
                                 select au.Expired).Single();
      Assert.IsFalse(mrXSession1Expired, "Dealer impersonators must not log out the user they impersonate");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == dealerAsMrX.ActiveUserID
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "Asset 1 and 2 are in the intersection of dealer assets with customers assets");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == mrXSession1.ActiveUserID
                 && (aws.fk_AssetID == customerAsset1.AssetID || aws.fk_AssetID == customerAsset3.AssetID)
                 && aws.Selected
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "MrX's asset working set is not changed by dealerAsMrX");
      rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                  where ws.ifk_ActiveUserID == mrXSession1.ActiveUserID
                   && (ws.fk_DimAssetID == customerAsset1.AssetID || ws.fk_DimAssetID == customerAsset3.AssetID)
                  select 1).Count();
      Assert.AreEqual(2, rptCount, "MrX's asset working set is not changed by dealerAsMrX, in NH_RPT also");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == dealerAsMrX.ActiveUserID
                 && aws.fk_AssetID == customerAsset2.AssetID
                 && aws.Selected
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(1, count, "DealerAsMrX's asset working set is separate to mrX's");
      rptCount = (from ws in Ctx.RptContext.vw_WorkingSet
                  where ws.ifk_ActiveUserID == dealerAsMrX.ActiveUserID
                   && ws.fk_DimAssetID == customerAsset2.AssetID
                  select 1).Count();
      Assert.AreEqual(1, rptCount, "DealerAsMrX's asset working set is separate to mrX's, in nh_rpt also");

    }

    [TestMethod]
    [DatabaseTest]
    [ExpectedException(typeof(SecurityException), "Expected Security Exception was not thrown.")]
    public void SecondUserXWithActiveUserX()
    {
      string pwd = "mrXLovesMrsX";
      // Normal customer, user "mrX", has fleet of 3 assets, two selected in an active session
      Customer normalCustomer = Entity.Customer.EndCustomer.SyncWithRpt().Save();
      User mrX = Entity.User.Username("MrX").Password(pwd).ForCustomer(normalCustomer).Save();
      Asset customerAsset1 = Entity.Asset.SerialNumberVin("AAA").WithDevice(Entity.Device.MTS521.Save()).SyncWithRpt().Save();
      Asset customerAsset2 = Entity.Asset.SerialNumberVin("BBB").WithDevice(Entity.Device.MTS522.Save()).SyncWithRpt().Save();
      Asset customerAsset3 = Entity.Asset.SerialNumberVin("CCC").WithDevice(Entity.Device.MTS523.Save()).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset1.Device).WithView(view => view.ForAsset(customerAsset1).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset2.Device).WithView(view => view.ForAsset(customerAsset2).ForCustomer(normalCustomer)).SyncWithRpt().Save();
      Entity.Service.Essentials.ForDevice(customerAsset3.Device).WithView(view => view.ForAsset(customerAsset3).ForCustomer(normalCustomer)).SyncWithRpt().Save();

      ActiveUser activeMrX = Entity.ActiveUser.ForUser(mrX).Save();
      SessionContext mrXSession1 = API.Session.Login(mrX.Name, pwd);
      Helpers.WorkingSet.Populate(activeMrX);
      Helpers.WorkingSet.Select(activeMrX, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

      // While session1 is in progress, a second login is made using MrX's credentials.
      SessionContext mrXSession2 = API.Session.Login(mrX.Name, pwd);
      ActiveUser activeMrX2 = (from au in Ctx.OpContext.ActiveUserReadOnly
                               where au.SessionID == mrXSession2.SessionID
                               select au).Single();
      //emulate the client saving the equipmentIDs selection
      Helpers.WorkingSet.Select(activeMrX2, new List<long> { customerAsset1.AssetID, customerAsset3.AssetID });

      //Session2 assumes existing ActiveUser from session1, and therefore assetworkingset, including selecteds.
      int count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
                   where aws.fk_ActiveUserID == mrXSession2.ActiveUserID
                   select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(3, count, "asset population should be the same as for prior session");

      count = (from aws in Ctx.OpContext.vw_AssetWorkingSet
               where aws.fk_ActiveUserID == mrXSession2.ActiveUserID
                 && aws.Selected
               select aws.fk_ActiveUserID).Count();
      Assert.AreEqual(2, count, "asset selection should be the same as for prior session");
      count = (from ws in Ctx.RptContext.vw_WorkingSet
               where ws.ifk_ActiveUserID == mrXSession2.ActiveUserID
               select 1).Count();
      Assert.AreEqual(2, count, "asset selection should be the same as for prior session, in NH_RPT also");

      SessionContext whoCares = API.Session.Validate(mrXSession1.SessionID);
      Assert.IsTrue(false, "Expect an exception to occur in the call above.");
    }

    [TestMethod]
    [DatabaseTest]
    [ExpectedException(typeof(SecurityException))]
    public void TrmbOpsMultipleImpersonations()
    {
      Customer ops = TestData.TestTrimbleOps;
      string pwd = "ILoveVSS";
      User opsUser = Entity.User.Username("OpsUser1").Password(pwd).ForCustomer(ops).Save();

      User u1 = Entity.User.Username("JoeUser1").Password(pwd).ForCustomer(TestData.TestCustomer).Save();
      User u2 = Entity.User.Username("JoeUser2").Password(pwd).ForCustomer(TestData.TestCustomer).Save();

      //setup, assume u1 and u2 are logged in prior to impersonations happening
      bool canLogin = API.Session.LoginCheck(u1.Name);
      Assert.IsTrue(canLogin, "U1 can log in");
      SessionContext u1Sesh = API.Session.Login(u1.Name, pwd);
      canLogin = API.Session.LoginCheck(u2.Name);
      Assert.IsTrue(canLogin, "U2 can log in");
      SessionContext u2Sesh = API.Session.Login(u1.Name, pwd);

      // ops user, impersonates U1
      canLogin = API.Session.LoginCheck(opsUser.Name);
      Assert.IsTrue(canLogin, "Ops user can log in cause this is first time");
      SessionContext opSession = API.Session.Login(opsUser.Name, pwd);
      SessionContext impersonation1 = API.Session.ImpersonatedLogin(opSession, u1.Name);

      // Ops user does another impersonation, U2
      canLogin = API.Session.LoginCheck(opsUser.Name);
      Assert.IsTrue(canLogin, "Ops user can log in again, because they were auto-logged out during the impersonated login");
      opSession = API.Session.Login(opsUser.Name, pwd);
      SessionContext impersonation2 = API.Session.ImpersonatedLogin(opSession, u2.Name);

      // Ops user can impersonate U1 a second time, but not without kicking himself out of the first impersonated session
      canLogin = API.Session.LoginCheck(opsUser.Name);
      Assert.IsTrue(canLogin, "Ops user can log in cause this is first time");
      opSession = API.Session.Login(opsUser.Name, pwd);
      SessionContext impersonation1b = API.Session.ImpersonatedLogin(opSession, u1.Name);

      API.Session.Validate(impersonation1.SessionID);
      Assert.IsTrue(false, "Expect an exception - expect this first impersonation session to have been expired by the second impersonation by the same ops user for the same impersonated user");
    }

    [DatabaseTest]
    [TestMethod]
    [ExpectedException(typeof(SecurityException), "Expected Security Exception was not thrown.")]
    public void LoginDealerAsUser_FailureExpiredDealerSessionIDReturnsSecurityException()
    {
      SessionContext session = API.Session.Validate(TestData.DealerUserActiveUser.SessionID);
      API.Session.Logout(session);  // expire dealer session
      long userID = TestData.DealerAdminActiveUser.fk_UserID;

      API.Session.ImpersonatedLogin(session, TestData.TestDealerAdmin.Name);
    }

    [DatabaseTest]
    [TestMethod]
    [ExpectedException(typeof(UnauthorizedAccessException), "Expected Unauthorized Access Exception was not thrown.")]
    public void LoginDealerAsUser_FailureInvalidUserIDReturnsUnauthorizedAccessException()
    {
      SessionContext session = API.Session.Validate(TestData.DealerUserActiveUser.SessionID);

      string userName = "liveLongAndProsper"; // this user should not exist

      API.Session.ImpersonatedLogin(session, userName);
    }

    [DatabaseTest]
    [TestMethod]
    public void LoginDealerAsUser_SuccessValidSessionIDValidUserIDReturnsActiveUser()
    {
      SessionContext dealer = API.Session.Validate(TestData.DealerUserActiveUser.SessionID);
      var impersonatedUser = API.Session.ImpersonatedLogin(dealer, TestData.TestDealerUser.Name);

      Assert.IsNotNull(impersonatedUser, "ActiveUser object not returned successfully.");
      Assert.AreEqual(TestData.DealerUserActiveUser.fk_UserID, impersonatedUser.UserID.Value, "Impersonated UserID does not match UserID passed in.");
    }

    [DatabaseTest]
    [TestMethod()]
    public void ImpersonatedLoginForNonVerifiedUser_Success()
    {
      SessionContext trimbleOpSession = Helpers.Sessions.GetContextFor(TestData.TrimbleOpsActiveUser);

      var customerUser =
        Entity.User.ForCustomer(TestData.TestCustomer).Username("CustomerUser846").EmailValidated(false).Save();

      var impersonatedUser = API.Session.ImpersonatedLogin(trimbleOpSession, customerUser.Name);
      Assert.IsNotNull(impersonatedUser, "failed impersonated login");
    }


    #endregion

    #region Helper Methods
    string testPreferenceKey = "testPreferenceKey";

    private User LoginLoopWithFailureHandler(int attempts)
    {
      //try to login till the lock out threshold count exceeds with wrong password
      for (int i = 0; i < attempts; i++)
      {
        try
        {
          API.Session.Login(TestData.TestCustomerUser.Name, "Test");
        }
        catch (Exception ex)
        {
          Assert.IsTrue(ex is UnauthorizedAccessException);
        }
      }

      User u = (from users in Ctx.OpContext.UserReadOnly
                where users.Name == TestData.TestCustomerUser.Name
                select users).SingleOrDefault();
      Assert.IsNotNull(u, "The test user should still be there");

      return u;
    }

    private User NewUserObject()
    {
      User expected = new User
      {
        Name = string.Format("TestUser{0}", DateTime.Now.Ticks),
        fk_CustomerID = TestData.TestCustomer.ID,
        PasswordHash = "Who cares",
        Salt = "12345",
        TimezoneName = "Pacific Standard Time",
        PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit),
        FirstName = "Fred",
        LastName = "Bloggs",
        EmailContact = "TestUser1@anywhere.com",
        fk_LanguageID = 1,
        Units = 1,
        Address = "10355 Westmoor Drive, Westminster, CO, 80021, US",
        PhoneNumber = "72012345678",
        LocationDisplayType = 2,
        AssetLabelPreferenceType = 5,
        GlobalID = Guid.NewGuid().ToString()
      };
      return expected;
    }

    private User NewTrmbOpUserObject()
    {
      User expected = new User
      {
        Name = string.Format("TestUser{0}", DateTime.Now.Ticks),
        fk_CustomerID = API.Customer.GetTrimbleOperationsCustomerID(),
        PasswordHash = "Who cares",
        Salt = "76565",
        TimezoneName = "Pacific Standard Time",
        PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit),
        FirstName = "Fred",
        LastName = "Bloggs",
        EmailContact = "TestUser1@anywhere.com",
        fk_LanguageID = 1,
        Units = 1,
        Address = "10355 Westmoor Drive, Westminster, CO, 80021, US",
        PhoneNumber = "72012345678",
        LocationDisplayType = 2,
        AssetLabelPreferenceType = 5,
        GlobalID = Guid.NewGuid().ToString()
      };
      return expected;
    }

    private List<UserFeatureAccess> NewFeatureAccessList()
    {
      List<UserFeatureAccess> featureList = new List<UserFeatureAccess>()
      {
        new UserFeatureAccess()
        {
          featureApp = FeatureAppEnum.NHWeb,
          access = FeatureAccessEnum.Full
        },
        new UserFeatureAccess()
        {
          feature = FeatureEnum.Fleet,
          access = FeatureAccessEnum.View
        },
        new UserFeatureAccess()
        {
          featureChild = FeatureChildEnum.Alerts,
          access = FeatureAccessEnum.None
        }
      };
      return featureList;
    }

    private ICollection<UserFeature> NewFeatureList(List<UserFeatureAccess> featureAccessList, long userID)
    {
      List<UserFeature> featureList = new List<UserFeature>();

      foreach (UserFeatureAccess featureAccess in featureAccessList)
      {
        int featureId = featureAccess.featureApp == 0
                                     ? (featureAccess.feature == 0 ? (int)featureAccess.featureChild : (int)featureAccess.feature)
                                     : (int)featureAccess.featureApp;

        UserFeature userFeature = new UserFeature { fk_Feature = featureId, fk_User = userID, fk_FeatureAccess = (int)featureAccess.access };

        featureList.Add(userFeature);
      }

      return featureList;
    }

    private void ReplaceFeatureAccessList(List<UserFeatureAccess> expectedFeatures)
    {
      expectedFeatures.Clear();
      expectedFeatures.Add(new UserFeatureAccess()
      {
        feature = FeatureEnum.Health,
        access = FeatureAccessEnum.Full
      });
    }

    private Dictionary<string, string> NewPreferencesDictionary(string newPreferenceValue)
    {
      Dictionary<string, string> newPreferences = new Dictionary<string, string>();
      newPreferences.Add(testPreferenceKey, newPreferenceValue);
      return newPreferences;
    }

    private void AssertUsersAreEqual(User expected, long userID)
    {
      User actual = (from u in Ctx.OpContext.User where u.ID == userID select u).SingleOrDefault();

      Assert.AreEqual(expected.fk_CustomerID, actual.fk_CustomerID, "Customer should be the same");
      Assert.AreEqual(expected.Name, actual.Name, "Name should be the same");
      Assert.AreEqual(expected.Salt, actual.Salt, "Salt should be the same");
      Assert.AreEqual(expected.PasswordHash, actual.PasswordHash, "PasswordHash should be the same");
      Assert.AreEqual(expected.TimezoneName, actual.TimezoneName, "TimezoneName should be the same");
      Assert.AreEqual(expected.EmailContact, actual.EmailContact, "EmailContact should be the same");
      Assert.AreEqual(expected.PwdExpiryUTC, actual.PwdExpiryUTC, "PwdExpiryUTC should be the same");
      Assert.AreEqual(expected.fk_LanguageID, actual.fk_LanguageID, "LanguageID should be the same");
      Assert.AreEqual(expected.Units, actual.Units, "Units should be the same");
      Assert.AreEqual(expected.LocationDisplayType, actual.LocationDisplayType, "Location Display Type should be the same");
      Assert.AreEqual(expected.GlobalID, actual.GlobalID, "GlobalID should be the same");
      Assert.AreEqual(expected.AssetLabelPreferenceType, actual.AssetLabelPreferenceType, "Asset Label Pref Type should be the same");
      Assert.AreEqual(expected.FirstName, actual.FirstName, "FirstName should be the same");
      Assert.AreEqual(expected.LastName, actual.LastName, "LastName should be the same");
      Assert.AreEqual(expected.JobTitle, actual.JobTitle, "JobTitle should be the same");
      Assert.AreEqual(expected.Address, actual.Address, "Address should be the same");
      Assert.AreEqual(expected.PhoneNumber, actual.PhoneNumber, "PhoneNumber should be the same");

      var actualUF = (from f in Ctx.OpContext.UserFeatureReadOnly where f.fk_User == actual.ID select f).ToList();
      var expectedUF = (from f in Ctx.OpContext.UserFeatureReadOnly where f.fk_User == expected.ID select f).ToList();

      Assert.AreEqual(expectedUF.Count, actualUF.Count, "UserFeature Count should be the same");
    }

    private void AssertSavedPerference(long userID, string actPreferenceValue)
    {
      string savedPreferenceValue = (from p in Ctx.OpContext.UserPreferences
                                     where p.fk_UserID == userID && p.Key == testPreferenceKey
                                     select p.ValueXML).FirstOrDefault();
      Assert.AreEqual(actPreferenceValue, savedPreferenceValue, "Saved Preference value is incorrect.");
    }

    private void AssertUserActivation(long userID, int activationStatusID, DateTime? sentUTC, string sentTo)
    {
      var userAct = (from ua in Ctx.OpContext.UserActivation where ua.fk_UserID == userID select ua).SingleOrDefault();
      Assert.AreEqual(activationStatusID, userAct.fk_UserActivationStatusID, "Activation ID is not the same");
      Assert.AreEqual(userID, userAct.fk_UserID, "User ID is not the same");
      Assert.AreEqual(sentUTC, userAct.SentUTC, "SentUTC is not the same");
      Assert.AreEqual(sentTo, userAct.SentTo, "SentTo is not the same");
    }

    private List<UserFeatureAccess> GetFullFeatureSet()
    {
      List<UserFeatureAccess> features = new List<UserFeatureAccess>();
      features.Add(new UserFeatureAccess()
      {
        access = FeatureAccessEnum.Full,
        featureApp = FeatureAppEnum.NHAdmin
      });
      features.Add(new UserFeatureAccess()
      {
        access = FeatureAccessEnum.Full,
        featureApp = FeatureAppEnum.NHWeb
      });

      return features;
    }

    private Customer CreateNewCustomer()
    {
      Customer normalCustomer = new Customer { Name = "Normal Customer", UpdateUTC = DateTime.UtcNow, BSSID = "TestBSSID", fk_CustomerTypeID = 1, IsActivated = true };

      Ctx.OpContext.Customer.AddObject(normalCustomer);
      Ctx.OpContext.SaveChanges();

      return normalCustomer;
    }

    private User CreateUser(long customerID)
    {
      return API.User.Create(Ctx.OpContext, customerID, "normalUser", "Password!2", "UTC", "TEST@Test.com", DateTime.UtcNow.AddDays(15),
        1, 1, 1, new Guid().ToString(), null, "TestFn", "TestLN", "test", "Test", "Test", GetFullFeatureSet(), 0, TemperatureUnitEnum.None, PressureUnitEnum.None);
    }

    #endregion
  }
}

