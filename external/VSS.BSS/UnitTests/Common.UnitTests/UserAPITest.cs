using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity.Core;
using System.Linq;
using FluorineFx.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UnitTests.WebApi;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.UnitTest.Common;
using User = VSS.Hosted.VLCommon.User;
using VSS.Hosted.VLCommon.Services.Types;

namespace UnitTests
{
    [TestClass()]
    public class UserAPITest : UnitTestBase
    {
        private UserAPI _userApi = new UserAPI();
        private string _key = ConfigurationManager.AppSettings["VerifyUserEncryptionKey"];
        private static readonly int passwordHistoryLimit = Convert.ToInt32(ConfigurationManager.AppSettings["PasswordHistoryLimit"]);
        private static readonly double passwordExpiryDaysLimit = Convert.ToDouble(ConfigurationManager.AppSettings["PasswordExpiryDaysLimit"]);

        [DatabaseTest]
        [TestMethod()]
        public void Create_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");
            AssertUsersAreEqual(expected, newUser.ID);
            CheckEmailIsSent();
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_FailureEmptyName()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();

            expected.Name = null;

            User newUser = CreateUser(target, expected, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_FailureEmptyPassword()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();

            testPassword = null;

            User newUser = CreateUser(target, expected, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Create_FailurePasswordTooShort()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();

            testPassword = "Pwd";

            User newUser = CreateUser(target, expected, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_FailureEmptyEmail()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();

            expected.EmailContact = null;

            User newUser = CreateUser(target, expected, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureEmailAlreadyExists()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            User failedUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            User failed = CreateUser(target, failedUser, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_Success_ValidMailParameterIsFalse_EmailTriggered()
        {
            var target = new UserAPI();
            User user = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, user, expectedFeatures);

            bool doesEmailExist = (from email in Ctx.OpContext.EmailQueueReadOnly
                                   where email.MailTo == user.EmailContact
                                   select email).Any();
            Assert.IsTrue(doesEmailExist);
        }        

        [DatabaseTest]
        [TestMethod()]
        public void Create_Success_ValidMailParameterIsTrue_EmailNotTriggeredAndNextGenSyncs()
        {
						
            var target = new UserAPI();
            User user = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = target.Create(Ctx.OpContext, user.fk_CustomerID.Value, user.Name, testPassword, user.TimezoneName,
            user.EmailContact, user.PwdExpiryUTC, user.fk_LanguageID, user.Units,
            user.LocationDisplayType, user.GlobalID, user.AssetLabelPreferenceType, user.FirstName,
            user.LastName, user.JobTitle, user.Address, user.PhoneNumber, expectedFeatures, user.MeterLabelPreferenceType,
            (TemperatureUnitEnum)user.fk_TemperatureUnitID, (PressureUnitEnum)user.fk_PressureUnitID, validEmail: true);

            bool doesEmailNotExist = ((from email in Ctx.OpContext.EmailQueueReadOnly
                                       where email.MailTo == user.EmailContact
                                       select email).Any() == false);
            Assert.IsTrue(doesEmailNotExist);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_Success_EmailAlreadyExists_ButADifferentOrganization()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            User failedUser = NewUserObject();
            failedUser.fk_CustomerID = TestData.TestDealer.ID;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            User newUserForDealer = CreateUser(target, failedUser, expectedFeatures);
            Assert.IsNotNull(newUserForDealer, "User should have been created");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_Success_ApiUserandClientUserHaveSameEmailID()
        {
            var target = new UserAPI();
            User client = NewUserObject();
            User api = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            List<UserFeatureAccess> expectedApiFeatures = NewApiFeatureAccessList();
            User newUser = CreateUser(target, client, expectedFeatures);
            User newUser1 = CreateUser(target, api, expectedApiFeatures);
            Assert.IsNotNull(newUser1);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureApiUserAlreadyExists()
        {
            var target = new UserAPI();
            User client = NewUserObject();
            User api = NewUserObject();
            User api2 = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            List<UserFeatureAccess> expectedApiFeatures = NewApiFeatureAccessList();
            User newUser = CreateUser(target, client, expectedFeatures);
            User newUser1 = CreateUser(target, api, expectedApiFeatures);
            User newUser2 = CreateUser(target, api2, expectedApiFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureDuplicateNameSameCustomer()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            User dupNameUser = NewUserObject();
            dupNameUser.Name = expected.Name;

            User newDupUser = CreateUser(target, dupNameUser, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureDuplicateNameDifferentCustomer()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            User dupNameUser = NewUserObject();
            dupNameUser.Name = expected.Name;
            dupNameUser.fk_CustomerID = TestData.TestDealer.ID;

            User newDupUser = CreateUser(target, dupNameUser, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(UpdateException))]
        public void Create_FailureBigName()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();


            expected.Name = "This is a very big long name so that the maximum number of characters is exceeded " +
            "This is a very big long name so that the maximum number of characters is exceeded This is a very big long name so that the maximum number of characters is exceeded";

            User newUser = CreateUser(target, expected, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_AssetSecurityFull()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureListAssetSecurity(2);
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");
            AssertUsersAreEqual(expected, newUser.ID);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Create_AssetSecurityNone()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureListAssetSecurity(0);
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");
            AssertUsersAreEqual(expected, newUser.ID);
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expected.Active = false;
            expected.TimezoneName = "Central Standard Time";
            expected.TermsofUseAcceptedUTC = DateTime.UtcNow.AddDays(5);
            expected.Units = 3;
            expected.LocationDisplayType = 7;
            expected.AssetLabelPreferenceType = 4;
            expected.FirstName = "Jim";
            expected.LastName = "Doe";
            expected.JobTitle = "Dispatcher";
            expected.Address = "1234 Main St, Missisippi, MI, US";
            expected.PhoneNumber = "30398765432";
            expected.EmailContact = newUser.EmailContact;
            expected.MeterLabelPreferenceType = 0;

            bool updated = target.Update(Ctx.OpContext, newUser.ID, expected.Active, expected.Name, expected.TimezoneName,
              expected.TermsofUseAcceptedUTC, expected.Units, expected.LocationDisplayType, expected.AssetLabelPreferenceType,
              expected.FirstName, expected.LastName, expected.JobTitle, expected.Address, expected.PhoneNumber, expected.EmailContact, expected.MeterLabelPreferenceType);
            Assert.IsTrue(updated, "Updated failed");

            AssertUsersAreEqual(expected, newUser.ID);
            CheckEmailIsSent();
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Success_EmailIDChanged()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            newUser.IsEmailValidated = true;
            string oldPasswordHash = newUser.PasswordHash;
            Assert.IsNotNull(newUser, "User has not been created");

            expected.EmailContact = "Testemail@testemail.com";

            bool updated = target.Update(Ctx.OpContext, newUser.ID, null, null, null,
              null, null, null, null, null, null, null, null, null, expected.EmailContact, null);
            Assert.IsTrue(updated, "Updated failed");

            CheckEmailIsSent();
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Success_DeleteUser_ChangeUsername()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            newUser.IsEmailValidated = true;
            string oldPasswordHash = newUser.PasswordHash;
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.Update(Ctx.OpContext, newUser.ID, false, null, null,
              null, null, null, null, null, null, null, null, null, null, null);
            Assert.IsTrue(updated, "Updated failed");

            Assert.IsTrue(newUser.Name.Substring(0, newUser.Name.Length).Contains("_delete"));
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_TOS_UTC_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expected.Active = true;
            expected.TimezoneName = "Central Standard Time";
            expected.TermsofUseAcceptedUTC = null;
            expected.Units = 3;
            expected.LocationDisplayType = 7;
            expected.AssetLabelPreferenceType = 4;
            expected.FirstName = "Jim";
            expected.LastName = "Doe";
            expected.JobTitle = "Dispatcher";
            expected.Address = "1234 Main St, Missisippi, MI, US";
            expected.PhoneNumber = "30398765432";
            expected.EmailContact = newUser.EmailContact;
            expected.MeterLabelPreferenceType = 0;

            //set the other attributes
            bool updated = target.Update(Ctx.OpContext, newUser.ID, expected.Active, expected.Name, expected.TimezoneName,
           expected.TermsofUseAcceptedUTC, expected.Units, expected.LocationDisplayType, expected.AssetLabelPreferenceType,
           expected.FirstName, expected.LastName, expected.JobTitle, expected.Address, expected.PhoneNumber, expected.EmailContact, expected.MeterLabelPreferenceType);
            Assert.IsTrue(updated, "Updated failed");
            DateTime tstamp = DateTime.UtcNow.AddDays(5);
            //now set the TOS UTC only...make sure the address, job title and phone number are not null
            updated = target.Update(Ctx.OpContext, newUser.ID, termsofUseAcceptedUTC: tstamp);
            Assert.IsTrue(updated, "Updated failed");

            Assert.AreEqual(newUser.JobTitle, expected.JobTitle);
            Assert.AreEqual(newUser.PhoneNumber, expected.PhoneNumber);
            Assert.AreEqual(newUser.Address, expected.Address);
            Assert.AreEqual(newUser.TermsofUseAcceptedUTC, tstamp);
            AssertUsersAreEqual(expected, newUser.ID);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Update_Failure()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expected.TimezoneName = "Central Standard Time";

            bool updated = target.Update(Ctx.OpContext, (newUser.ID + 1), expected.Active, expected.Name, expected.TimezoneName,
              expected.TermsofUseAcceptedUTC, expected.Units, expected.LocationDisplayType, expected.AssetLabelPreferenceType,
              expected.FirstName, expected.LastName, expected.JobTitle, expected.Address, expected.PhoneNumber, expected.EmailContact, expected.MeterLabelPreferenceType);
            Assert.IsTrue(updated, "Updated failed");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Failure_EmailAlreaedyExists()
        {
            var target = new UserAPI();
            User user1 = NewUserObject();
            User user2 = NewUserObject();
            user2.EmailContact = "testEmail@trimble.com";
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser1 = CreateUser(target, user1, expectedFeatures);
            User newUser2 = CreateUser(target, user2, expectedFeatures);
            AssertEx.Throws<InvalidOperationException>(() => target.Update(Ctx.OpContext, newUser2.ID, email: newUser1.EmailContact), "duplicateEmail");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Failure_EmailAlreaedyExists_ApiUser()
        {
            var target = new UserAPI();
            User user1 = NewUserObject();
            User user2 = NewUserObject();
            user2.EmailContact = "testEmail@trimble.com";
            List<UserFeatureAccess> expectedFeatures = NewApiFeatureAccessList();
            User newUser1 = CreateUser(target, user1, expectedFeatures);
            User newUser2 = CreateUser(target, user2, expectedFeatures);
            AssertEx.Throws<InvalidOperationException>(() => target.Update(Ctx.OpContext, newUser2.ID, email: newUser1.EmailContact), "duplicateEmail");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Success_EmailAlreaedyExists_ButDifferentOrganization()
        {
            var target = new UserAPI();
            User user1 = NewUserObject();
            User user2 = NewUserObject();
            user2.EmailContact = "testEmail@trimble.com";
            user2.fk_CustomerID = TestData.TestDealer.ID;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser1 = CreateUser(target, user1, expectedFeatures);
            User newUser2 = CreateUser(target, user2, expectedFeatures);
            bool updated = target.Update(Ctx.OpContext, newUser2.ID, email: newUser1.EmailContact);
            Assert.IsTrue(updated, "User should have been updated with the new emailID");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Update_Success_SameEmailForClientAndApiUser()
        {
            var target = new UserAPI();
            User user1 = NewUserObject();
            User user2 = NewUserObject();
            user2.EmailContact = "testEmail@trimble.com";
            user2.fk_CustomerID = TestData.TestDealer.ID;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            List<UserFeatureAccess> expectedApiFeatures = NewApiFeatureAccessList();
            User newUser1 = CreateUser(target, user1, expectedFeatures);
            User newUser2 = CreateUser(target, user2, expectedApiFeatures);
            bool updated = target.Update(Ctx.OpContext, newUser2.ID, email: newUser1.EmailContact);
            Assert.IsTrue(updated, "User should have been updated with the new emailID");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Delete_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool deleted = target.Delete(Ctx.OpContext, newUser.ID);
            Assert.IsTrue(deleted, "Failed to delete user");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Delete_FederatedLogonInfo_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");
            bool fliCreated = target.CreatedUpdateFederationLogonInfo(Ctx.OpContext, newUser.ID, "CAT", "unitTestEntity");
            Assert.IsTrue(fliCreated, "FederatedLogonInfo was not created");

            bool deleted = target.Delete(Ctx.OpContext, newUser.ID);
            Assert.IsTrue(deleted, "Failed to delete user");

            var fliRow =
              (from fli in Ctx.OpContext.FederatedLogonInfo where fli.fk_UserID == newUser.ID select fli).SingleOrDefault();
            Assert.IsNull(fliRow, "Expected the FederatedLogonInfo row to be removed");
        }

        [DatabaseTest]
        [TestMethod()]
        public void Delete_Failure()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool deleted = target.Delete(Ctx.OpContext, newUser.ID + 1);
            Assert.IsFalse(deleted, "Deleted user");
        }

        [DatabaseTest]
        [TestMethod()]
        public void UserNameExists_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool exists = target.UserNameExists(Ctx.OpContext, expected.Name);
            Assert.IsTrue(exists, "Failed to find existing user");
        }

        [DatabaseTest]
        [TestMethod()]
        public void UserNameExists_Failure()
        {
            var target = new UserAPI();
            User expected = NewUserObject();

            bool exists = target.UserNameExists(Ctx.OpContext, expected.Name);
            Assert.IsFalse(exists, "Failed to find existing user");
        }

        [DatabaseTest]
        [TestMethod()]
        public void GetUniqueUserName_Success()
        {
            var target = new UserAPI();
            string generatedUserName = target.GetUniqueUserName();            
            int identifiedUser = (from u in Ctx.OpContext.User where u.Name == generatedUserName select u).Count();

            Assert.IsTrue(generatedUserName.Length.Equals(10));
            Assert.IsTrue(identifiedUser.Equals(0));
        }
        
        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            expected.PwdExpiryUTC = DateTime.UtcNow;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string newPassword = "NewPwds6!";

            bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, newPassword);
            Assert.IsTrue(updated, "password updated");

            expected.PasswordHash = HashUtils.ComputeHash(newPassword, UserAPI.hashAlgorithm, expected.Salt);
            expected.PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit);

            AssertPasswordHistory(newUser.ID, 2);
            AssertUsersAreEqual(expected, newUser.ID);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_Failure_MissingUser()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID + 1, "NewPwd99");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PasswordUpdate_Failure_PasswordEmpty()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void PasswordUpdate_Failure_Password2Short()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "NewPw");
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Failure_PasswordDoesNotContainUppercase()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            bool exceptionOccoured = false;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            try
            {
                bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "newpd12#");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Invalid parameter password. Password must contain atleast one uppercase letter", e.Message, "UpperCase Check not happening");
                exceptionOccoured = true;
            }
            Assert.IsTrue(exceptionOccoured, "Exception did not occour");
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Failure_PasswordDoesNotContainLowercase()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            bool exceptionOccoured = false;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            try
            {
                bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "NEWPD12#");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Invalid parameter password. Password must contain atleast one lowercase letter", e.Message, "LowerCase Check not happening");
                exceptionOccoured = true;
            }
            Assert.IsTrue(exceptionOccoured, "Exception did not occour");
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Failure_PasswordDoesNotContainNumber()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            bool exceptionOccoured = false;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            try
            {
                bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "newpD$##");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Invalid parameter password. Password must contain atleast one number", e.Message, "Number Check not happening");
                exceptionOccoured = true;
            }
            Assert.IsTrue(exceptionOccoured, "Exception did not occour");
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Failure_PasswordDoesNotContainSymbol()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            bool exceptionOccoured = false;
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            try
            {
                bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "newPd124");
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Invalid parameter password. Password must contain atleast one the following symbols !@#$%^&*-+_", e.Message, "Symbol Check not happening");
                exceptionOccoured = true;
            }
            Assert.IsTrue(exceptionOccoured, "Exception did not occour");
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_Failure_PasswordIsTheSameAsUserName()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            bool exceptionOccoured = false;
            expected.Name = "Usern1m%";
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            try
            {
                bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, expected.Name);
            }
            catch (ArgumentException e)
            {
                Assert.AreEqual("Invalid parameter password. Password must must not be same as username", e.Message, "UserName Check not happening");
                exceptionOccoured = true;
            }
            Assert.IsTrue(exceptionOccoured, "Exception did not occour");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_FailurePasswordIsPrevious()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            AssertPasswordHistory(newUser.ID, 1);
            AssertUsersAreEqual(expected, newUser.ID);

            bool updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, "CrIcKeT2!");
            Assert.IsTrue(updated, "Failed to update password");

            AssertPasswordHistory(newUser.ID, 2);

            updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, testPassword);
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_HistoryLimitedTo6()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string newPassword = testPassword + "AA";
            bool updated = false;

            for (int i = 0; i < 8; i++)
            {
                updated = target.UpdatePassword(Ctx.OpContext, newUser.ID, newPassword + i);
                Assert.IsTrue(updated, "Password failed to update");
            }
            AssertPasswordHistory(newUser.ID, passwordHistoryLimit);
        }

        #region ForgotPassword
        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_ForgotPasswordInvalidGuid()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = DateTime.UtcNow;
                ctx.SaveChanges();
                target.UpdatePassword("56757invalidGuid", "newPassword");
            }

        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_ForgotPasswordExceeded24Hours()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = DateTime.UtcNow.AddHours(-25);
                ctx.SaveChanges();
                target.UpdatePassword(user.PasswordResetGUID, "newPassword");
            }

        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_ForgotPasswordUserInactive()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.Active = false;
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = DateTime.UtcNow;
                ctx.SaveChanges();
                target.UpdatePassword(user.PasswordResetGUID, "newPassword");
            }

        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void PasswordUpdate_ForgotPasswordInvalidPassword()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = DateTime.UtcNow;
                ctx.SaveChanges();
                target.UpdatePassword(user.PasswordResetGUID, user.Name);
            }
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_ForgotPasswordSuccess()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = DateTime.UtcNow;
                ctx.SaveChanges();
                target.UpdatePassword(user.PasswordResetGUID, "newPasswor!2");
            }
        }

        [DatabaseTest]
        [TestMethod()]
        public void PasswordUpdate_ForgotPasswordSuccess_UnlockUserAccount()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            var dateTimeUtc = DateTime.UtcNow;
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in ctx.User where u.ID == expected.ID select u).SingleOrDefault();
                user.PasswordResetGUID = Guid.NewGuid().ToString();
                user.PasswordResetUTC = dateTimeUtc;

                //Locking the user account
                user.LogOnFailedCount = 5;
                user.LogOnFirstFailedUTC = dateTimeUtc;
                user.LogOnLastFailedUTC = dateTimeUtc;

                ctx.SaveChanges();
                target.UpdatePassword(user.PasswordResetGUID, "newPasswor!2");
                AssertUserUnlock(user.ID);
            }

        }
        #endregion

        [DatabaseTest]
        [TestMethod()]
        public void UpdateFeatureAccess_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            ReplaceFeatureAccessList(expectedFeatures);

            bool updated = API.User.UpdateFeatureAccess(Ctx.OpContext, newUser.ID, expectedFeatures);
            Assert.IsTrue(updated, "Failed to updated feature access for user");

            var ufeatures = (from uf in Ctx.OpContext.UserFeature
                             where uf.fk_User == newUser.ID
                             select uf).ToArray<UserFeature>();
            Assert.IsNotNull(ufeatures, "Failed to get updated user features");
            Assert.AreEqual<int>(expectedFeatures.Count, ufeatures.Length, "Wrong number of updated user features");
            Assert.AreEqual<int>((int)expectedFeatures[0].feature, ufeatures[0].fk_Feature, "Wrong feature");
            Assert.AreEqual<int>((int)expectedFeatures[0].access, ufeatures[0].fk_FeatureAccess, "Wrong access");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateFeatureAccess_FailureMissingUser()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            ReplaceFeatureAccessList(expectedFeatures);

            bool updated = API.User.UpdateFeatureAccess(Ctx.OpContext, newUser.ID + 1, expectedFeatures);
        }

        [DatabaseTest]
        [TestMethod()]
        public void UpdateFeatureAccess_FailureNoFeatures()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expectedFeatures.Clear();

            bool updated = API.User.UpdateFeatureAccess(Ctx.OpContext, newUser.ID, expectedFeatures);
            Assert.IsTrue(updated, "Failed to updated feature access for user");
        }

        [DatabaseTest]
        [TestMethod()]
        public void CreateUpdateUserPreference_SuccessNewAndExisting()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string newPreferenceValue = "<SomeXML thatSpecifies=\"the preference value\" />";
            Dictionary<string, string> newPreferences = NewPreferencesDictionary(newPreferenceValue);

            bool preferencesCreated = target.CreateUpdateUserPreference(Ctx.OpContext, newUser.ID, newPreferences);
            Assert.IsTrue(preferencesCreated, "Error creating UserPreference");

            AssertSavedPerference(newUser.ID, newPreferenceValue);

            string updatedPreferenceValue = "<SomeModifiedXML thatNowSpecifies=\"the modified preference value\" />";
            Dictionary<string, string> updatedPreferences = NewPreferencesDictionary(updatedPreferenceValue);

            bool preferencesUpdated = API.User.CreateUpdateUserPreference(Ctx.OpContext, newUser.ID, updatedPreferences);
            Assert.IsTrue(preferencesUpdated, "Error updating UserPreference");

            AssertSavedPerference(newUser.ID, updatedPreferenceValue);
        }

        [DatabaseTest]
        [TestMethod()]
        public void CreateUpdateUserPreferenceMaps_SuccessNewAndExisting()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string newPreferenceValue = "alk";
            Dictionary<string, string> newPreferences = NewPreferencesDictionary(newPreferenceValue);

            bool preferencesCreated = target.CreateUpdateUserPreference(Ctx.OpContext, newUser.ID, newPreferences);
            Assert.IsTrue(preferencesCreated, "Error creating UserPreference");

            AssertSavedPerference(newUser.ID, newPreferenceValue);

            string updatedPreferenceValue = "google";
            Dictionary<string, string> updatedPreferences = NewPreferencesDictionary(updatedPreferenceValue);

            bool preferencesUpdated = API.User.CreateUpdateUserPreference(Ctx.OpContext, newUser.ID, updatedPreferences);
            Assert.IsTrue(preferencesUpdated, "Error updating UserPreference");

            AssertSavedPerference(newUser.ID, updatedPreferenceValue);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateUpdateUserPreference_FailureMissingUser()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string newPreferenceValue = "<SomeXML thatSpecifies=\"the preference value\" />";
            Dictionary<string, string> newPreferences = NewPreferencesDictionary(newPreferenceValue);

            bool preferencesCreated = target.CreateUpdateUserPreference(Ctx.OpContext, newUser.ID + 1, newPreferences);
        }

        [DatabaseTest]
        [TestMethod()]
        public void UpdateLanguage_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdateLanguage(Ctx.OpContext, newUser.ID, "es-ES");
            Assert.IsTrue(updated, "Language update failed");

            expected.fk_LanguageID = 3;

            AssertUsersAreEqual(expected, newUser.ID);
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateLanguage_FailureMissingUser()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdateLanguage(Ctx.OpContext, newUser.ID + 1, "es-ES");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateLanguage_FailureMissingLanguage()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool updated = target.UpdateLanguage(Ctx.OpContext, newUser.ID, "en-NZ");
        }

        [DatabaseTest]
        [TestMethod()]
        public void ModifyUserForAccountReg_Success()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();

            expected.Name = expected.GlobalID;

            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expected.Name = "primaryuser";
            string newPassword = "C0d3M0nk3ys";

            bool modified = target.ModifyUserForAccountReg(Ctx.OpContext, expected.GlobalID, expected.Name, newPassword);
            Assert.IsTrue(modified, "Failed to modify user for account registration");

            expected.PasswordHash = HashUtils.ComputeHash(newPassword, UserAPI.hashAlgorithm, expected.Salt);

            AssertPasswordHistory(newUser.ID, 2);
            AssertUsersAreEqual(expected, newUser.ID);
        }

        [TestMethod]
        public void IsCorporateUser_Success_ReturnsTrueWhenCorporateUser()
        {
            var corpUser = TestData.CorporateActiveUser;
            SessionContext session = Helpers.Sessions.GetContextFor(corpUser);

            var isCorporateUser = _userApi.IsCorporateUser(session.NHOpContext, session.CustomerID);
            Assert.IsTrue(isCorporateUser, "The method should return true for the corporate user.");
        }

        [TestMethod]
        public void IsCorporateUser_Success_ReturnsFalseWhenDealerUser()
        {
            var dlrUser = TestData.DealerAdminActiveUser;
            SessionContext session = Helpers.Sessions.GetContextFor(dlrUser);

            var isCorporateUser = _userApi.IsCorporateUser(session.NHOpContext, session.CustomerID);
            Assert.IsFalse(isCorporateUser, "The method should return false for the dealer user.");
        }

        [TestMethod]
        public void IsCorporateUser_Success_ReturnsFalseWhenCustomerUser()
        {
            var customerUser = TestData.CustomerAdminActiveUser;
            SessionContext session = Helpers.Sessions.GetContextFor(customerUser);

            var isCorporateUser = _userApi.IsCorporateUser(session.NHOpContext, session.CustomerID);
            Assert.IsFalse(isCorporateUser, "The method should return false for the customer user.");
        }

        [DatabaseTest]
        [TestMethod()]
        public void ModifyUserForAccountReg_FailureMissingUserName()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            expected.Name = "primaryuser";
            string newPassword = "CricKet!";

            bool modified = target.ModifyUserForAccountReg(Ctx.OpContext, expected.GlobalID, expected.Name, newPassword);
            Assert.IsFalse(modified, "Modify is expected to fail");
        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyUserForAccountReg_FailureMultipleUsers()
        {
            // Not sure how can create more than one user with same name...to trigger this scenario
        }

        [DatabaseTest]
        [TestMethod()]
        public void CreateUpdateUserActivation_SuccessNewNoSentTo()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool created = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 1, null, null);
            Assert.IsTrue(created, "Failed to create user activation");

            AssertUserActivation(newUser.ID, 1, null, null);
        }

        [DatabaseTest]
        [TestMethod()]
        public void CreateUpdateUserActivation_SuccessNewWithSentTo()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            string sentTo = "fred_dagg@footrot.flats.com";

            bool created = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 1, null, sentTo);
            Assert.IsTrue(created, "Failed to create user activation");

            AssertUserActivation(newUser.ID, 1, null, sentTo);
        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        public void CreateUpdateUserActivation_SuccessExistingNoSentToNoSentUTC()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool created = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 1, null, null);
            Assert.IsTrue(created, "Failed to create user activation");

            AssertUserActivation(newUser.ID, 1, null, null);

            bool updated = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 2, null, null);
            Assert.IsTrue(updated, "Failed to update user activation");

            AssertUserActivation(newUser.ID, 2, null, null);
        }

        [DatabaseTest]
        [TestMethod()]
        public void CreateUpdateUserActivation_SuccessExistingWithSentToWithSentUTC()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);
            Assert.IsNotNull(newUser, "User has not been created");

            bool created = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 1, null, null);
            Assert.IsTrue(created, "Failed to create user activation");

            AssertUserActivation(newUser.ID, 1, null, null);

            string sentTo = "fred_bloggs@infamy.com";
            DateTime sentUtc = DateTime.UtcNow;

            bool updated = target.CreateUpdateUserActivation(Ctx.OpContext, newUser.ID, 3, sentUtc, sentTo);
            Assert.IsTrue(updated, "Failed to update user activation");

            AssertUserActivation(newUser.ID, 3, sentUtc, sentTo);
        }        

        [DatabaseTest]
        [TestMethod()]
        public void VerifyUserEmail_Success_NewUser()
        {
            var target = new UserAPI();
            UserAPI.IsNewViewEnabled = false;
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            var verify = (target.VerifyEmail(Ctx.OpContext,User.EmailVerificationGUID, false));
            Assert.AreEqual(0, verify.Item2, "Wrong type of user");

            User u = (from user in Ctx.OpContext.UserReadOnly
                      where user.Name == User.Name
                      select user).FirstOrDefault();
            Assert.IsNotNull(u, "Failed to retrieve user");
            Assert.IsTrue(u.IsEmailValidated, "Email Should have been validated");
        }

        [DatabaseTest]
        [TestMethod()]
        public void VerifyUserEmail_Success_OldUser()
        {
            var target = new UserAPI();
            UserAPI.IsNewViewEnabled = false;
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            User.LastLoginUTC = DateTime.UtcNow;
            Ctx.OpContext.SaveChanges();

            var verify = (target.VerifyEmail(Ctx.OpContext, User.EmailVerificationGUID, false));
            Assert.AreEqual(1, verify.Item2, "Wrong type of user");

            User u = (from user in Ctx.OpContext.UserReadOnly
                      where user.Name == User.Name
                      select user).FirstOrDefault();
            Assert.IsNotNull(u, "Failed to retrieve user");
            Assert.IsTrue(u.IsEmailValidated, "Email Should have been validated");
        }

        [DatabaseTest]
        [TestMethod()]
        public void VerifyUserEmail_Success_ApiUser()
        {
            var target = new UserAPI();
            UserAPI.IsNewViewEnabled = false;
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewApiFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            User.LastLoginUTC = DateTime.UtcNow;
            Ctx.OpContext.SaveChanges();

            var verify = (target.VerifyEmail(Ctx.OpContext, User.EmailVerificationGUID, false));
            Assert.AreEqual(2, verify.Item2, "Wrong type of user");

            User u = (from user in Ctx.OpContext.UserReadOnly
                      where user.Name == User.Name
                      select user).FirstOrDefault();
            Assert.IsNotNull(u, "Failed to retrieve user");
            Assert.IsTrue(u.IsEmailValidated, "Email Should have been validated");
        }

        [DatabaseTest]
        [TestMethod()]
        public void VerifyUserEmail_Success_IdentityExistingUser()
        {
            var target = new UserAPI();
            UserAPI.IsNewViewEnabled = true;
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            var verify = (target.VerifyEmail(Ctx.OpContext, User.EmailVerificationGUID, true));
            Assert.AreEqual(4, verify.Item2, "Wrong type of user");

            User u = (from user in Ctx.OpContext.UserReadOnly
                      where user.Name == User.Name
                      select user).FirstOrDefault();
            Assert.IsNotNull(u, "Failed to retrieve user");
            Assert.IsTrue(u.IsEmailValidated, "Email Should have been validated");
        }

        [DatabaseTest]
        [TestMethod()]
        public void VerifyUserEmail_Success_IdentityNewUser()
        {
            var target = new UserAPI();
            UserAPI.IsNewViewEnabled = true;
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            var verify = (target.VerifyEmail(Ctx.OpContext, User.EmailVerificationGUID, false));
            Assert.AreEqual(3, verify.Item2, "Wrong type of user");

            User u = (from user in Ctx.OpContext.UserReadOnly
                      where user.Name == User.Name
                      select user).FirstOrDefault();
            Assert.IsNotNull(u, "Failed to retrieve user");
            Assert.IsTrue(u.IsEmailValidated, "Email Should have been validated");
        }

        [DatabaseTest]
        [TestMethod]
        public void IsEmailAlreadyValidated_EmailAlreadyVerified()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            User.IsEmailValidated = true;
            User.EmailVerificationGUID = Guid.NewGuid().ToString();
            User.EmailVerificationUTC = DateTime.UtcNow;
            Ctx.OpContext.SaveChanges();

            var isValidated = target.IsEmailAlreadyVerified(User.Name, Ctx.OpContext);
            Assert.IsTrue(isValidated, "Should return true");
        }

        [DatabaseTest]
        [TestMethod]
        public void IsEmailAlreadyValidated_EmailNotVerified()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);

            var isValidated = target.IsEmailAlreadyVerified(User.Name, Ctx.OpContext);
            Assert.IsFalse(isValidated, "Should return false");
        }

        [DatabaseTest]
        [TestMethod()]
        public void ResendEmail_Success()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            var resend = target.ResendEmail(User.Name);
            Assert.IsTrue(resend, "Email Should be resent");

            var email = (from emailQueue in Ctx.OpContext.EmailQueue
                         select emailQueue).ToList();

            Assert.AreEqual(2, email.Count, "Verification Email should have been sent again");
        }

        [DatabaseTest]
        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ResendEmail_Failure_WorngUserName()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            Assert.IsNotNull(User, "Failed to create user");

            var verify = target.ResendEmail("SomeUser1");
        }

        [DatabaseTest]
        [TestMethod()]
        public void SkipEmailVerification_Success_OnFirstTime()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);
            var result = target.SkipEmailVerification(Ctx.OpContext, newUser.Name);

            Assert.IsTrue(result, "EmailVerificationTrackingUTC is not updated.");

            User userDetails = (from user in Ctx.OpContext.UserReadOnly
                                where user.Name == User.Name
                                select user).SingleOrDefault();

            Assert.IsNotNull(userDetails.EmailVerificationTrackingUTC, "EmailVerificationTrackingUTC is not updated on first time continue.");
        }

        [DatabaseTest]
        [TestMethod()]
        public void SkipEmailVerification_Success_OnConsecutiveTime()
        {
            var target = new UserAPI();
            User newUser = NewUserObject();
            List<UserFeatureAccess> expectedFeatrure = NewFeatureAccessList();
            User User = CreateUser(target, newUser, expectedFeatrure);

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == User.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            Ctx.OpContext.SaveChanges();

            var result = target.SkipEmailVerification(Ctx.OpContext, newUser.Name);

            Assert.IsFalse(result, "EmailVerificationTrackingUTC should not be updated.");
        }

        [DatabaseTest]
        [TestMethod()]
        public void EmailVerification_Success_CalledByResend_WithoutModifyingEmail()
        {
            var customer = Entity.Customer.Dealer.Save();
            var createdUser = Entity.User.Username("test").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser).Save();

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == createdUser.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            userDetails.fk_LanguageID = 1;
            Ctx.OpContext.SaveChanges();

            var target = new UserAPI();
            var resultForResend = target.EmailVerification(Ctx.OpContext, userDetails.Name, userDetails.EmailContact, false);
            Assert.IsTrue(resultForResend, "Email should be sent to the user for his saved email address.");

            var email = (from emailQueue in Ctx.OpContext.EmailQueueReadOnly
                         select emailQueue).ToList();

            Assert.AreEqual(1, email.Count, "Verification Email should have been sent again");
        }

        [DatabaseTest]
        [TestMethod()]
        public void EmailVerification_Success_CalledByResend_WithModifyingEmail()
        {
            var customer = Entity.Customer.Dealer.Save();
            var createdUser = Entity.User.Username("test").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser).Save();

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == createdUser.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            userDetails.fk_LanguageID = 1;
            Ctx.OpContext.SaveChanges();

            var target = new UserAPI();
            var resultForResend = target.EmailVerification(Ctx.OpContext, userDetails.Name, "NewTestUser1@anywhere.com", false);
            Assert.IsTrue(resultForResend, "Email should be sent to the user for his new email address.");

            var email = (from emailQueue in Ctx.OpContext.EmailQueueReadOnly
                         select emailQueue).ToList();

            Assert.AreEqual(1, email.Count, "Verification Email should have been sent again to new email address");
            Assert.AreEqual(1, userDetails.EmailModifiedCount, "Email Modified Count Mismatch");
            Assert.AreEqual(false, userDetails.IsEmailValidated, "Email Validated not reset");
        }

        [DatabaseTest]
        [TestMethod()]
        public void EmailVerification_Success_CalledBySendNow_WithModifyingEmail_EmailAlreadyExists()
        {
            var customer = Entity.Customer.Dealer.Save();
            var createdUser = Entity.User.Username("test").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser).Save();

            var createdUser1 = Entity.User.Username("test1").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser1).Save();

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == createdUser.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            userDetails.fk_LanguageID = 1;
            Ctx.OpContext.SaveChanges();

            var target = new UserAPI();
            AssertEx.Throws<InvalidOperationException>(() => target.EmailVerification(Ctx.OpContext, userDetails.Name, "TEST_EMAIL@DOMAIN.COM", false), "duplicateEmail");
        }

        [DatabaseTest]
        [TestMethod()]
        public void EmailVerification_Success_CalledByVerify_WithoutModifyingEmail()
        {
            var customer = Entity.Customer.Dealer.Save();
            var createdUser = Entity.User.Username("test").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser).Save();

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == createdUser.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            userDetails.fk_LanguageID = 1;
            Ctx.OpContext.SaveChanges();

            var target = new UserAPI();
            var resultForVerify = target.EmailVerification(Ctx.OpContext, userDetails.Name, userDetails.EmailContact, true);
            Assert.IsTrue(resultForVerify, "Email should be sent to the user for his saved email address.");

            var email = (from emailQueue in Ctx.OpContext.EmailQueueReadOnly
                         select emailQueue).ToList();

            Assert.AreEqual(1, email.Count, "Verification Email should have been sent again to new email address");
            Assert.AreEqual(null, userDetails.EmailModifiedCount, "Email Modified Count Mismatch");
        }

        [DatabaseTest]
        [TestMethod()]
        public void EmailVerification_Success_CalledByVerify_WithModifyingEmail()
        {
            var customer = Entity.Customer.Dealer.Save();
            var createdUser = Entity.User.Username("test").ForCustomer(customer).Save();
            Entity.ActiveUser.ForUser(createdUser).Save();

            User userDetails = (from user in Ctx.OpContext.User
                                where user.Name == createdUser.Name
                                select user).SingleOrDefault();
            userDetails.EmailVerificationTrackingUTC = DateTime.UtcNow;
            userDetails.fk_LanguageID = 1;
            Ctx.OpContext.SaveChanges();

            var target = new UserAPI();
            var resultForVerify = target.EmailVerification(Ctx.OpContext, userDetails.Name, "NewTestUser1@anywhere.com", true);
            Assert.IsTrue(resultForVerify, "Email should be sent to the user for his saved email address.");

            var email = (from emailQueue in Ctx.OpContext.EmailQueueReadOnly
                         select emailQueue).ToList();

            Assert.AreEqual(1, email.Count, "Verification Email should have been sent again to new email address");
            Assert.AreEqual(1, userDetails.EmailModifiedCount, "Email Modified Count Mismatch");
        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureSaveChangesException()
        {

        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Create_FailureSavePasswordHistory()
        {

        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyUserForAccountReg_FailureMultipleUsersForSameName()
        {

        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyUserForAccountReg_FailureUserAlreadyExists()
        {

        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ModifyUserForAccountReg_FailureSavePasswordHistory()
        {

        }

        [DatabaseTest]
        [TestMethod()]
        [Ignore]
        [ExpectedException(typeof(InvalidOperationException))]
        public void SaveUserFeatures_FailureSaveChangesException()
        {

        }

        [TestMethod]
        [DatabaseTest]
        public void ValidateUser_UserNotExists_Failure()
        {
            var result = _userApi.ValidateUser(string.Empty, string.Empty);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DatabaseTest]
        public void ValidateUser_UserExists_WrongPassword()
        {
            var customer = Entity.Customer.Dealer.Save();
            var user = Entity.User.Username("test").ForCustomer(customer).Save();
            var result = _userApi.ValidateUser(user.Name, string.Empty);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DatabaseTest]
        public void ValidateUser_UserExists_Invalid()
        {
            var customer = Entity.Customer.Dealer.Save();
            var user = Entity.User.Username("test").ForCustomer(customer).Inactive().Save();
            var result = _userApi.ValidateUser(user.Name, string.Empty);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [DatabaseTest]
        public void ValidateUser_UserExists_Valid()
        {
            var customer = Entity.Customer.Dealer.Save();
            var user = Entity.User.Username("test").Password("test").ForCustomer(customer).Save();
            var result = _userApi.ValidateUser(user.Name, "test");
            Assert.IsTrue(result);
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_RecordDoesnotExists_CreatesNew()
        {
            var customer = TestData.TestCustomerAdmin;
            var user = TestData.CustomerAdminActiveUser;

            var context = API.Session.Validate(user.SessionID);

            var federatedValue = IdGen.StringId();
            var result = API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, federatedValue, "CAT");
            Assert.IsTrue(result);

            Assert.IsTrue(Ctx.OpContext.FederatedLogonInfoReadOnly.Any(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "CAT" && t.FederationValue == federatedValue));
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_RecordExists_SameUser_NoChange()
        {
            var customer = TestData.TestCustomerAdmin;
            var user = TestData.CustomerAdminActiveUser;

            var context = API.Session.Validate(user.SessionID);
            var federatedValue = IdGen.StringId();
            Ctx.OpContext.FederatedLogonInfo.AddObject(new FederatedLogonInfo { FederationEntity = "CAT", FederationValue = federatedValue, fk_UserID = user.fk_UserID });
            Ctx.OpContext.SaveChanges();

            var result = API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, federatedValue, "CAT");
            Assert.IsTrue(result);

            Assert.AreEqual(1, Ctx.OpContext.FederatedLogonInfoReadOnly.Count(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "CAT" && t.FederationValue == federatedValue));
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_RecordExists_DifferentFederatedValue_UpdateInfo()
        {
            var customer = TestData.TestCustomerAdmin;
            var user = TestData.CustomerAdminActiveUser;

            var context = API.Session.Validate(user.SessionID);

            var federatedValue1 = IdGen.StringId();
            Ctx.OpContext.FederatedLogonInfo.AddObject(new FederatedLogonInfo { FederationEntity = "CAT", FederationValue = federatedValue1, fk_UserID = user.fk_UserID });
            Ctx.OpContext.SaveChanges();

            var federatedValue2 = IdGen.StringId();
            var result = API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, federatedValue2, "CAT");
            Assert.IsTrue(result);

            Assert.IsTrue(Ctx.OpContext.FederatedLogonInfoReadOnly.Any(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "CAT" && t.FederationValue == federatedValue2));

            Assert.IsFalse(Ctx.OpContext.FederatedLogonInfoReadOnly.Any(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "CAT" && t.FederationValue == federatedValue1));
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_RecordExists_DifferentUser_UpdateInfo()
        {
            var customer = TestData.TestCustomerAdmin;
            var user = TestData.CustomerAdminActiveUser;
            var user2 = TestData.CustomerUserActiveUser;

            var context = API.Session.Validate(user.SessionID);

            var federatedValue = IdGen.StringId();
            Ctx.OpContext.FederatedLogonInfo.AddObject(new FederatedLogonInfo { FederationEntity = "CAT", FederationValue = federatedValue, fk_UserID = user2.fk_UserID });
            Ctx.OpContext.SaveChanges();

            AssertEx.Throws<UpdateException>(() => API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, federatedValue, "CAT"), "An error occurred while updating the entries. See the inner exception for details.");
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_RecordExists_DifferentFederatedValueAndSouce_CreateNew()
        {
            var customer = TestData.TestCustomerAdmin;
            var user = TestData.CustomerAdminActiveUser;

            var context = API.Session.Validate(user.SessionID);

            Ctx.OpContext.FederatedLogonInfo.AddObject(new FederatedLogonInfo { FederationEntity = "KW", FederationValue = IdGen.StringId(), fk_UserID = user.fk_UserID });
            Ctx.OpContext.SaveChanges();

            var result = API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, IdGen.StringId(), "CAT");
            Assert.IsTrue(result);

            Assert.IsTrue(Ctx.OpContext.FederatedLogonInfoReadOnly.Any(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "CAT"));

            Assert.IsTrue(Ctx.OpContext.FederatedLogonInfoReadOnly.Any(
              t => t.fk_UserID == user.fk_UserID && t.FederationEntity == "KW"));
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_NullFederationValue_ThrowsException()
        {
            var user = TestData.CustomerAdminActiveUser;
            var context = API.Session.Validate(user.SessionID);
            AssertEx.Throws<ArgumentNullException>(() =>
              API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, null, "CAT"),
              "federatedValue cannot be null");
        }

        [TestMethod]
        [DatabaseTest]
        public void FederatedLogOn_NullFederationEntity_ThrowsException()
        {
            var user = TestData.CustomerAdminActiveUser;
            var context = API.Session.Validate(user.SessionID);
            AssertEx.Throws<ArgumentNullException>(() =>
              API.User.CreatedUpdateFederationLogonInfo(context.NHOpContext, context.UserID.Value, IdGen.StringId(), null),
              "federatedEntity cannot be null");
        }

        [TestMethod]
        [DatabaseTest]
        public void GetUserPreferencesByKeyTest()
        {
            INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>();
            User user = TestData.TestCustomerAdminUser;
            UserPreferences preferences = new UserPreferences
                                            {
                                                Key = "FormattingValues",
                                                UpdateUTC = DateTime.UtcNow,
                                                fk_UserID = user.ID,
                                                ValueXML = "<FormattingValues version=\"1\" dateFormat=\"MM/DD/YY\" " +
                                                           "dateTimeFormat=\"MM_DD_YY L:NN A\" timeFormat=\"L:NN A\" currencyPrecision=\"2\"" +
                                                           " currencySymbol=\"$\" thousandsSeparator=\",\" decimalSeparator=\".\" " +
                                                           "chartDateFormat=\"MM/DD\" dateSeparator=\"/\" timeSeparator=\":\" clockIndicator=\"12\" decimalPrecision=\"1\" />"
                                            };
            opCtx.UserPreferences.AddObject(preferences);
            opCtx.SaveChanges();
            Dictionary<string, string> userPrefs = API.User.GetUserPreferencesByKey(opCtx, user.ID, preferences.Key);
            Assert.IsNotNull(userPrefs, "userPrefs Should have returned some values");
            Assert.AreEqual(13, userPrefs.Count, "Incorrect number of userPrefs returned");
            Assert.AreEqual("MM_DD_YY L:NN A", userPrefs["dateTimeFormat"], "incorrect userpref value");
        }

        [TestMethod]
        [DatabaseTest]
        public void GetUserPreferenceValueByKeyTest()
        {
            INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>();
            User user = TestData.TestCustomerAdminUser;
            UserPreferences preferences = new UserPreferences
                                            {
                                                Key = "FormattingValues",
                                                UpdateUTC = DateTime.UtcNow,
                                                fk_UserID = user.ID,
                                                ValueXML = "<FormattingValues version=\"1\" dateFormat=\"MM/DD/YY\" " +
                                                           "dateTimeFormat=\"MM_DD_YY L:NN A\" timeFormat=\"L:NN A\" currencyPrecision=\"2\"" +
                                                           " currencySymbol=\"$\" thousandsSeparator=\",\" decimalSeparator=\".\" " +
                                                           "chartDateFormat=\"MM/DD\" dateSeparator=\"/\" timeSeparator=\":\" clockIndicator=\"12\" decimalPrecision=\"1\" />"
                                            };
            opCtx.UserPreferences.AddObject(preferences);
            opCtx.SaveChanges();
            string userPrefs = API.User.GetUserPreferenceValueByKey(opCtx, user.ID, preferences.Key, "dateTimeFormat");
            Assert.IsFalse(string.IsNullOrEmpty(userPrefs), "userPrefs Should have returned a values");
            Assert.AreEqual("MM_DD_YY L:NN A", userPrefs, "incorrect userpref value");
        }

        [DatabaseTest]
        [TestMethod()]
        public void UpdateLastLoginUTC_ExpectSuccess()
        {
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = TestData.TestCustomerAdminUser;
                bool valid = API.User.ValidateUser(TestDataHelper.CUSTOMER_USER_USERNAME, TestDataHelper.CUSTOMER_USER_PASSWORD);

                User userToValidate = (from u in opCtx.User
                                       join c in opCtx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                                       where u.Name.Equals(TestDataHelper.CUSTOMER_USER_USERNAME) && u.Active
                                       select u).FirstOrDefault();

                if (userToValidate != null)
                {
                    Assert.IsNotNull(userToValidate.LastLoginUTC);
                    Assert.AreEqual(DateTime.UtcNow.ToShortDateString(), userToValidate.LastLoginUTC.Value.ToShortDateString());
                    Assert.AreEqual(DateTime.UtcNow.ToShortTimeString(), userToValidate.LastLoginUTC.Value.ToShortTimeString());
                }
            }
        }

        #region Test Data
        string testPassword = "WhoCare1$";
        string testPreferenceKey = "testPreferenceKey";

        #endregion

        #region Helper Methods

        private User NewUserObject()
        {
            User expected = new User
            {
                Name = string.Format("TestUser{0}", DateTime.Now.Ticks),
                fk_CustomerID = TestData.TestCustomer.ID,
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
                GlobalID = Guid.NewGuid().ToString(),
                MeterLabelPreferenceType = 0,
                fk_PressureUnitID = (int)PressureUnitEnum.PSI
            };
            return expected;
        }

        private User CreateUser(IUserAPI target, User expected, List<UserFeatureAccess> expectedFeatures)
        {
            User actual = target.Create(Ctx.OpContext, expected.fk_CustomerID.Value, expected.Name, testPassword, expected.TimezoneName,
              expected.EmailContact, expected.PwdExpiryUTC, expected.fk_LanguageID, expected.Units,
              expected.LocationDisplayType, expected.GlobalID, expected.AssetLabelPreferenceType, expected.FirstName,
              expected.LastName, expected.JobTitle, expected.Address, expected.PhoneNumber, expectedFeatures, expected.MeterLabelPreferenceType,
              (TemperatureUnitEnum)expected.fk_TemperatureUnitID, (PressureUnitEnum)expected.fk_PressureUnitID);

            if (actual != null)
            {
                expected.ID = actual.ID;
                expected.Salt = actual.Salt;
                expected.PasswordHash = actual.PasswordHash;
                List<UserFeature> uflist = NewFeatureList(expectedFeatures, expected.ID).ToList();
            }
            return actual;
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

        private List<UserFeatureAccess> NewApiFeatureAccessList()
        {
            List<UserFeatureAccess> featureList = new List<UserFeatureAccess>()
      {
        new UserFeatureAccess()
        {
          feature = FeatureEnum.VLReadyAPI,
          access = FeatureAccessEnum.Full
        }
      };
            return featureList;
        }

        private List<UserFeatureAccess> NewFeatureListAssetSecurity(int accessId)
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
          feature = FeatureEnum.AssetSecurity,
          access = (accessId==0)?FeatureAccessEnum.None:((accessId==1)?FeatureAccessEnum.View:FeatureAccessEnum.Full)
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
            Assert.AreEqual(expected.PwdExpiryUTC.KeyDate(), actual.PwdExpiryUTC.KeyDate(), "PwdExpiryUTC should be the same");
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
            Assert.IsFalse(actual.IsEmailValidated, "Email Should not be validated");
            Assert.AreEqual(expected.fk_PressureUnitID, actual.fk_PressureUnitID, "PressureUnitID should be the same");

            var actualUF = (from f in Ctx.OpContext.UserFeatureReadOnly where f.fk_User == actual.ID select f).ToList();
            var expectedUF = (from f in Ctx.OpContext.UserFeatureReadOnly where f.fk_User == expected.ID select f).ToList();

            Assert.AreEqual(expectedUF.Count, actualUF.Count, "UserFeature Count should be the same");

        }

        private void AssertUserUnlock(long userID)
        {
            User lockedUser = (from u in Ctx.OpContext.User where u.ID == userID select u).SingleOrDefault();

            Assert.AreEqual(lockedUser.LogOnFailedCount, 0);
            Assert.AreEqual(lockedUser.LogOnFirstFailedUTC, null);
            Assert.AreEqual(lockedUser.LogOnLastFailedUTC, null);
        }

        private void AssertPasswordHistory(long userID, int historyCount)
        {
            var pwdHistory = (from uph in Ctx.OpContext.UserPasswordHistoryReadOnly where uph.fk_UserID == userID select uph).OrderByDescending(a => a.InsertUTC).Take(passwordHistoryLimit);
            Assert.AreEqual(historyCount, pwdHistory.Count(), "Missing Password history");
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

        [DatabaseTest]
        [TestMethod()]
        public void GetUserLocationPreference_Test()
        {
            var target = new UserAPI();
            User expected = NewUserObject();

            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);

            Assert.IsNotNull(newUser, "User has not been created");

            UserLocationPreferenceEnum expPref = target.GetLocationPreference(Ctx.OpContext, newUser.ID);

            Assert.IsNotNull(expPref, "Valid User preference has not been chosen");
        }

        [DatabaseTest]
        [TestMethod()]
        public void GetUserLocationPreference_TestForNULL()
        {
            var target = new UserAPI();
            User expected = NewUserObject();
            expected.LocationDisplayType = null;

            List<UserFeatureAccess> expectedFeatures = NewFeatureAccessList();
            User newUser = CreateUser(target, expected, expectedFeatures);

            Assert.IsNotNull(newUser, "User has not been created");

            UserLocationPreferenceEnum expPref = target.GetLocationPreference(Ctx.OpContext, newUser.ID);

            Assert.IsNotNull(expPref, "Valid User preference has not been chosen");
            Assert.AreEqual(UserLocationPreferenceEnum.Site, expPref, "Valid User preference has not been chosen");
        }
        #endregion

        #region VLTierSupportTool Tests
        
        [TestMethod]
        [DatabaseTest]
        public void TestSearchUsersByUserNameResults()
        {
            //Arrange
            string registeredDealer = "TestSearchUsers_Dealer";
            string dealerCode = "DEALER01";
            Customer dealer = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            User dealerUser1 = Entity.User.ForCustomer(dealer).Username("TestUserName1").WithLanguage(TestData.English).EmailValidated(true).Save();
            User dealerUser2 = Entity.User.ForCustomer(dealer).Email("TestGmail@google.com").WithLanguage(TestData.English).Save();

            //Act
            UserAPI target = new UserAPI();
            List<UserInfo> results = target.SearchUsers("TestUserName1");
            
            //Assert
            Assert.AreEqual(1, results.Count, "Results count doesnt match");
        }

        [TestMethod]
        [DatabaseTest]
        public void TestSearchUsersByEmailResults()
        {
            //Arrange
            string registeredDealer = "TestSearchUsers_Dealer";
            string dealerCode = "DEALER01";
            Customer dealer = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            User dealerUser1 = Entity.User.ForCustomer(dealer).Username("TestUserName1").WithLanguage(TestData.English).Save();
            User dealerUser2 = Entity.User.ForCustomer(dealer).Email("TestGmail@google.com").WithLanguage(TestData.English).Save();

            //Act
            UserAPI target = new UserAPI();
            List<UserInfo> results = target.SearchUsers("TestGmail");
            
            //Assert
            Assert.AreEqual(1, results.Count, "Results count doesnt match");
        }

        [TestMethod]
        [DatabaseTest]
        public void TestSearchUsersByUserNameAndEmailResults()
        {
            //Arrange
            string registeredDealer = "TestSearchUsers_Dealer";
            string dealerCode = "DEALER01";
            Customer dealer = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            User dealerUser1 = Entity.User.ForCustomer(dealer).Username("TestUserName1").WithLanguage(TestData.English).Save();
            User dealerUser2 = Entity.User.ForCustomer(dealer).Email("TestUserName1@google.com").WithLanguage(TestData.English).Save();

            //Act
            UserAPI target = new UserAPI();
            List<UserInfo> results = target.SearchUsers("UserName1");
            
            //Assert
            Assert.AreEqual(2, results.Count, "Results count doesnt match");
        }

        [TestMethod]
        [DatabaseTest]
        public void TestGetUserStatusByIDClientUserResults()
        {
            //Arrange
            string registeredDealer = "TestSearchUsers_Dealer";
            string dealerCode = "DEALER01";
            
            Customer dealer = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            Customer dealer1 = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            User dealerUser1 = Entity.User.ForCustomer(dealer).Email("TestUserEmail@gmail.com").WithLanguage(TestData.English).EmailValidated(true).Save();
            User dealerUser2 = Entity.User.ForCustomer(dealer1).Email("TestUserEmail@gmail.com").WithLanguage(TestData.English).EmailValidated(true).Save();
            
            //Act
            UserAPI target = new UserAPI();
            var results = target.GetUserStatus(dealerUser1.ID);

            //Assert
            Assert.AreEqual(1, results.UserFeatures.Count, "Feature count does not match");
            Assert.AreEqual(true, results.IsEmailVerified);
            Assert.AreEqual(2, results.CustomerList.Count, "Email with same user count doesnt match");
        }

        [TestMethod]
        [DatabaseTest]
        public void TestGetUserStatusByIDClientAndAPIUserResults()
        {
            //Arrange
            string registeredDealer = "TestSearchUsers_Dealer";
            string dealerCode = "DEALER01";
            Customer dealer = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();
            Customer dealer1 = Entity.Customer.EndCustomer.Name(registeredDealer).NetworkDealerCode(dealerCode).BssId("BSS123").SyncWithRpt().Save();

            User dealerUser1 = Entity.User.ForCustomer(dealer).Email("TestUserEmail@gmail.com").WithLanguage(TestData.English).EmailValidated(true).
                WithFeature(x => x.App(FeatureAppEnum.DataServices).Access(FeatureAccessEnum.Full)).WithFeature(x => x.App(FeatureAppEnum.NHWeb).Access(FeatureAccessEnum.Full)).Save();
            User dealerUser2 = Entity.User.ForCustomer(dealer).Email("TestUserEmail@gmail.com").WithLanguage(TestData.English).EmailValidated(true).
                WithFeature(x => x.App(FeatureAppEnum.DataServices).Access(FeatureAccessEnum.Full)).WithFeature(x => x.App(FeatureAppEnum.NHWeb).Access(FeatureAccessEnum.Full)).Save();

            //Act
            UserAPI target = new UserAPI();
            var results = target.GetUserStatus(dealerUser1.ID);

            //Assert
            Assert.AreEqual(2, results.UserFeatures.Count, "Feature count does not match");
            Assert.AreEqual(true, results.IsEmailVerified);
            Assert.AreEqual(2, results.CustomerList.Count, "Email with same user count doesnt match");
        }

        #endregion

        #region Helper

        public void CheckEmailIsSent() // To check if email verification is sent 
        {
            var emailQueue = (from email in Ctx.OpContext.EmailQueue
                              select email).FirstOrDefault();
            Assert.IsNotNull(emailQueue, "Email should be sent to user");
        }
        public void CheckEmailIsNotSent() // To check if email verification is sent 
        {
            var emailQueue = (from email in Ctx.OpContext.EmailQueue
                              select email).FirstOrDefault();
            Assert.IsNull(emailQueue, "Email should not be sent to user");
        }

        #endregion

        #region OLD TESTS - Left here for reference as functionality was moved
        ///// <summary>
        /////A test for Delete with all user based dependencies
        /////</summary>

        //[TestMethod()]
        //public void UserDeleteWithDependenciesTest()
        //{
        //  using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        //  {
        //    // Let's get everything in order we need to create a customer and user with assets and dependencies
        //    ActiveUser me = AdminLogin();
        //    SessionContext session = API.Session.Validate(me.SessionID);

        //    Customer ownerCustomer = new Customer() { Name = "OwnerOfAssets" };

        //    long customerAID = CreateCustomer(session, ownerCustomer, CustomerTypeEnum.Customer, null).Value;
        //    Assert.IsNotNull(customerAID, "Should be able to create customer A");
        //    Assert.IsTrue(customerAID > 0, "Invalid customer A ID");

        //    //Create assets for customer
        //    string device1 = "88888888";
        //    Asset custAXC = CreateAssetWithDevice(session, customerAID, device1, DeviceTypeEnum.CrossCheck, DateTime.UtcNow);

        //    string device2 = "PL0001Test";
        //    Asset custAPL = CreateAssetWithDevice(session, customerAID, device2, DeviceTypeEnum.ProductLink, DateTime.UtcNow);
        //    custAPL.SerialNumberVIN = "-1";
        //    custAPL.Make = "CAT";
        //    CreateAssetSubscription(session, custAPL.ID);
        //    // use API.Equipment.Update here as an alternative since CreateAssetSubscription already calls SaveChanges()
        //    // we only need to update the equipment
        //    session.NHOpContext.SaveChanges();

        //    // Create a user for customer
        //    string pwdSalt = HashUtils.CreateSalt(5);
        //    User ownerUser = new User()
        //    {
        //      Name = "OwnerUser",
        //      PasswordHash = HashUtils.ComputeHash("OwnerUser", "SHA1", pwdSalt),
        //      Salt = pwdSalt,
        //      PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit),
        //      FirstName = "Test",
        //      LastName = "User 1",
        //      EmailContact = "TestUser1@anywhere.com"
        //    };

        //    long? userID = API.User.Create(session, ownerUser, customerAID, new List<UserFeatureAccess>() { });
        //    Assert.IsNotNull(userID, "Failed to create user-A");
        //    long custAUserID = userID.Value;

        //    // UserFeature created
        //    List<UserFeatureAccess> userFeatures = new List<UserFeatureAccess>();
        //    userFeatures.Add(new UserFeatureAccess()
        //    {
        //      featureApp = FeatureAppEnum.NHWeb,
        //      access = FeatureAccessEnum.Full
        //    });
        //    userFeatures.Add(new UserFeatureAccess()
        //    {
        //      feature = FeatureEnum.Fleet,
        //      access = FeatureAccessEnum.View
        //    });
        //    bool created = API.User.UpdateFeatureAccess(session, custAUserID, userFeatures);
        //    Assert.IsTrue(created, "Should be able to create feature access for user");

        //    // UserPreferences created
        //    string testPreferenceKey = "testPreferenceKey"; 
        //    string newPreferenceValue = "<SomeXML thatSpecifies=\"the preference value\" />";
        //    Dictionary<string, string> newPreferences = new Dictionary<string, string>();
        //    newPreferences.Add(testPreferenceKey, newPreferenceValue);
        //    bool preferencesCreated = API.User.CreateUpdateUserPreference(session, newPreferences);
        //    Assert.IsTrue(preferencesCreated, "Error creating UserPreference");

        //    // UserPasswordHistory create
        //    string newPassword = "C0d3M0nk3ys";
        //    bool actual = API.User.UpdatePassword(session, custAUserID, newPassword);
        //    Assert.IsTrue(actual, "CustAUser Pwd changed ok");

        //    // ***** TODO: ActiveUser create ***** //

        //    // Now let's delete that pesky user
        //    bool deleted = API.User.Delete(session, userID.Value);
        //    Assert.IsTrue(deleted, "Cust A User not deleted");

        //    // Let's double check all deletions happened
        //    var user = (from au in session.NHOpContext.UserReadOnly
        //                where au.ID == userID.Value && au.Active
        //                select au);
        //    Assert.IsTrue(user != null && user.Count() == 0, "User should have been deleted");

        //    // ***** TODO: ActiveUser deleted ***** //
        //  }
        //}

        //[TestMethod()]
        //public void VisibleAssetsForUserTest()
        //{
        //  using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionOptions))
        //  {
        //    ActiveUser me = AdminLogin();
        //    SessionContext session = API.Session.Validate(me.SessionID);

        //    Make m = Make.CreateMake("JAS", DateTime.UtcNow);
        //    m.Name = "Daly Trucks";
        //    session.NHOpContext.AddToMake(m);
        //    session.NHOpContext.SaveChanges();

        //    // Valid, PL321, active/active, with a current subs.
        //    Device pl321Device = new Device();
        //    Asset assetW321ValidSubs = CreateAssetWithDevice(session, session.CustomerID.Value, "PL321ValidSubs",
        //      DeviceTypeEnum.PL321, out pl321Device);
        //    assetW321ValidSubs.SerialNumberVIN = "UT-1";
        //    assetW321ValidSubs.Make = m.Name;

        //    AssetSubscription subs1 = API.Equipment.CreateSubscription(session, assetW321ValidSubs.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Invalid, because asset is inactive.
        //    Device newDeviceInActive = new Device();
        //    Asset inActiveAsset = CreateAssetWithDevice(session, session.CustomerID.Value, "InactiveAsset",
        //      DeviceTypeEnum.PL321, out newDeviceInActive);
        //    inActiveAsset.Active = false;
        //    inActiveAsset.SerialNumberVIN = "UT-2";
        //    inActiveAsset.Make = m.Name;
        //    AssetSubscription subs2 = API.Equipment.CreateSubscription(session, inActiveAsset.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Valid because we want to see assets with subscriptions regardless of the presence of a device.
        //    Asset assetNoDevice = CreateAsset(session.NHOpContext, session.CustomerID.Value, "AssetNoDevice", m.Name, "3", "family", "model");
        //    assetNoDevice.SerialNumberVIN = "UT-3";
        //    assetNoDevice.Make = m.Name;
        //    AssetSubscription subs3 = API.Equipment.CreateSubscription(session, assetNoDevice.ID, ServiceTypeEnum.ManualMaintenanceLog, DateTime.UtcNow.AddDays(-15), null, "-99");


        //    // Valid even though the device is inactive since there is a subscription
        //    Device inactiveDevice = new Device();
        //    Asset activeAssetInActiveDevice = CreateAssetWithDevice(session, session.CustomerID.Value, "InactiveDevice",
        //      DeviceTypeEnum.PL321, out inactiveDevice);
        //    inactiveDevice.Active = false;
        //    activeAssetInActiveDevice.SerialNumberVIN = "UT-5";
        //    activeAssetInActiveDevice.Make = m.Name;
        //    AssetSubscription subs5 = API.Equipment.CreateSubscription(session, activeAssetInActiveDevice.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Invalid because there is no current subscription ('PL' assets, imported from CAT, must have a subscription to show in NH)
        //    Device pl321NoSubscription = new Device();
        //    Asset activeAssetNoSubscription = CreateAssetWithDevice(session, session.CustomerID.Value, "NoSubscription",
        //      DeviceTypeEnum.PL321, out pl321NoSubscription);
        //    activeAssetNoSubscription.SerialNumberVIN = "UT-6";
        //    activeAssetNoSubscription.Make = m.Name;


        //    Device mtsDevice = new Device();
        //    Asset activeAssetMTSDevice = CreateAssetWithDevice(session, session.CustomerID.Value, "testActiveMTS",
        //      DeviceTypeEnum.Series522523, out mtsDevice);
        //    activeAssetMTSDevice.SerialNumberVIN = "UT-7";
        //    activeAssetMTSDevice.Make = "Independant";
        //    API.Equipment.CreateSubscription(session, activeAssetMTSDevice.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Crosschecks and TrimTrac are OUT OF SCOPE, but we are still showing them in the UI until told not to.
        //    Device ccDevice = new Device();
        //    Asset activeAssetCCDevice = CreateAssetWithDevice(session, session.CustomerID.Value, "-8787",
        //      DeviceTypeEnum.CrossCheck, out ccDevice);
        //    activeAssetCCDevice.SerialNumberVIN = "UT-8";
        //    activeAssetCCDevice.Make = "Independant";
        //    API.Equipment.CreateSubscription(session, activeAssetCCDevice.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Crosschecks and TrimTrac are OUT OF SCOPE, but we are still showing them in the UI until told not to.
        //    Device ttDevice = new Device();
        //    Asset activeAssetTTDevice = CreateAssetWithDevice(session, session.CustomerID.Value, "010307002345678956756",
        //      DeviceTypeEnum.TrimTrac, out ttDevice);
        //    activeAssetTTDevice.SerialNumberVIN = "UT-9";
        //    activeAssetTTDevice.Make = "Independant";
        //    API.Equipment.CreateSubscription(session, activeAssetTTDevice.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), null, "-99");

        //    // Valid 121, providing it's subscription is current
        //    Device pl121Device = new Device();
        //    Asset activeAssetPL121Device = CreateAssetWithDevice(session, session.CustomerID.Value, "testGpsPL121",
        //      DeviceTypeEnum.PL121, out pl121Device);
        //    activeAssetPL121Device.SerialNumberVIN = "UT-10";
        //    activeAssetPL121Device.Make = m.Name;
        //    AssetSubscription subs6 = API.Equipment.CreateSubscription(session, activeAssetPL121Device.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(1.0), "-99");

        //    // If the asset had a subs, even if it has expired, we include it.
        //    Device pl321ExpSubsDevice = new Device();
        //    Asset activeAssetPL321ExpiredSubs = CreateAssetWithDevice(session, session.CustomerID.Value, "testGpsPL321Expired",
        //      DeviceTypeEnum.PL321, out pl321ExpSubsDevice);
        //    activeAssetPL321ExpiredSubs.SerialNumberVIN = "UT-14";
        //    activeAssetPL321ExpiredSubs.Make = m.Name;
        //    AssetSubscription subs10 = API.Equipment.CreateSubscription(session, activeAssetPL321ExpiredSubs.ID, ServiceTypeEnum.Essentials, DateTime.UtcNow.AddDays(-15), DateTime.UtcNow.AddDays(-1.0), "-99");

        //    session.NHOpContext.SaveChanges();

        //    Logout(session);
        //    session = Login2();

        //    IEnumerable<Asset> actual = (from aws in session.NHOpContext.AssetWorkingSetReadOnly
        //                                 where aws.SessionID == session.SessionID
        //                                 select aws.Asset).ToList();

        //    bool foundValid321 = false;
        //    bool foundValidMTS = false;
        //    bool foundValidCC = false;
        //    bool foundValidTT = false;
        //    bool foundValid121 = false;
        //    bool foundValidExpiredSubs = false;

        //    bool foundInvalidInactiveAsset = false;
        //    bool foundNoDevice = false;
        //    bool foundInvalidInactiveDevice = false;
        //    bool foundInvalidNoSubs = false;

        //    foreach (Asset a in actual)
        //    {
        //      foundValid321 = foundValid321 || (a.SerialNumberVIN == assetW321ValidSubs.SerialNumberVIN);
        //      foundValidMTS = foundValidMTS || (a.SerialNumberVIN == activeAssetMTSDevice.SerialNumberVIN);
        //      foundValidCC = foundValidCC || (a.SerialNumberVIN == activeAssetCCDevice.SerialNumberVIN);
        //      foundValidTT = foundValidTT || (a.SerialNumberVIN == activeAssetTTDevice.SerialNumberVIN);
        //      foundValid121 = foundValid121 || (a.SerialNumberVIN == activeAssetPL121Device.SerialNumberVIN);
        //      foundValidExpiredSubs = foundValidExpiredSubs || (a.SerialNumberVIN == activeAssetPL321ExpiredSubs.SerialNumberVIN);

        //      foundInvalidInactiveAsset = foundInvalidInactiveAsset || (a.SerialNumberVIN == inActiveAsset.SerialNumberVIN);
        //      foundNoDevice = foundNoDevice || (a.SerialNumberVIN == assetNoDevice.SerialNumberVIN);
        //      foundInvalidInactiveDevice = foundInvalidInactiveDevice || (a.SerialNumberVIN == activeAssetInActiveDevice.SerialNumberVIN);
        //      foundInvalidNoSubs = foundInvalidNoSubs || (a.SerialNumberVIN == activeAssetNoSubscription.SerialNumberVIN);
        //    }

        //    Assert.IsTrue(foundValid321, "!foundValid321");
        //    Assert.IsTrue(foundValidMTS, "!foundValidMTS");
        //    Assert.IsTrue(foundValidCC, "!foundValidCC");
        //    Assert.IsTrue(foundValidTT, "!foundValidTT");
        //    Assert.IsTrue(foundValid121, "!foundValid121");
        //    Assert.IsTrue(foundValidExpiredSubs, "!foundValidExpiredSubs");
        //    Assert.IsTrue(foundNoDevice, "foundNoDevice");
        //    Assert.IsTrue(foundInvalidInactiveDevice, "foundInvalidInactiveDevice");

        //    Assert.IsFalse(foundInvalidInactiveAsset, "foundInvalidInactiveAsset");
        //    Assert.IsFalse(foundInvalidNoSubs, "foundInvalidNoSubs");

        //    //Don't test total returned as there may be junk lying around in unit test customer

        //  }
        //}

        //[TestMethod()]
        //public void ClearWorkingSetTest()
        //{
        //  using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
        //  {
        //    ActiveUser admin = AdminLogin();
        //    SessionContext adminSesh = API.Session.Validate(admin.SessionID);

        //    Asset a1 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-878787", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a1.ID);
        //    Asset a2 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-12345", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a2.ID);
        //    Asset a3 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-67894", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a3.ID);

        //    ActiveUser au = Login();
        //    SessionContext sesh = API.Session.Validate(au.SessionID);

        //    Helpers.NHRpt.DimTables_Populate();

        //    UserAPI target = new UserAPI();
        //    bool updated = target.SetWorkingSetSelection(sesh, new List<long> { a1.ID, a2.ID, a3.ID });
        //    Assert.IsTrue(updated,"Update working set failed");

        //    int opCount = (from ws in sesh.NHOpContext.AssetWorkingSetReadOnly
        //                 where ws.SessionID == sesh.SessionID
        //                 && ws.Selected
        //                 select 1).Count();
        //    Assert.AreEqual(3, opCount, "Wrong count of working set assets in NH_OP following update");

        //    int rptCount = (from ws in sesh.NHRptContext.vw_WorkingSet
        //                    where ws.SessionID == sesh.SessionID
        //                    select 1).Count();
        //    Assert.AreEqual(3, rptCount, "Wrong count of working set assets in NH_RPT following update");

        //    bool cleared = target.SetWorkingSetSelection(sesh, new List<long>());
        //    Assert.IsTrue(cleared, "Working set not cleared");


        //    opCount = (from ws in sesh.NHOpContext.AssetWorkingSetReadOnly
        //                   where ws.SessionID == sesh.SessionID
        //                   && ws.Selected
        //                   select 1).Count();
        //    Assert.AreEqual(0, opCount, "Wrong count of working set assets in NH_OP following clear");

        //    rptCount = (from ws in sesh.NHRptContext.vw_WorkingSet
        //                    where ws.SessionID == sesh.SessionID
        //                    select 1).Count();
        //    Assert.AreEqual(0, rptCount, "Wrong count of working set assets in NH_RPT following clear");
        //  }
        //}

        //[TestMethod()]
        //public void SetWorkingSetSelectionEmptySet()
        //{
        //  using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, this.TransactionOptions))
        //  {
        //    ActiveUser admin = AdminLogin();
        //    SessionContext adminSesh = API.Session.Validate(admin.SessionID);

        //    Asset a1 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-878787", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a1.ID);
        //    Asset a2 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-12345", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a2.ID);
        //    Asset a3 = CreateAssetWithDevice(adminSesh, unitTestCustomerID.Value, "-67894", DeviceTypeEnum.Series522523, null);
        //    CreateAssetSubscription(adminSesh, a3.ID);

        //    ActiveUser au = Login();
        //    SessionContext sesh = API.Session.Validate(au.SessionID);

        //    Helpers.NHRpt.DimTables_Populate();

        //    UserAPI target = new UserAPI();
        //    bool updated = target.SetWorkingSetSelection(sesh, new List<long> { a1.ID, a2.ID, a3.ID });
        //    Assert.IsTrue(updated, "Update working set failed");

        //    int opCount = (from ws in sesh.NHOpContext.AssetWorkingSetReadOnly
        //                   where ws.SessionID == sesh.SessionID
        //                   && ws.Selected
        //                   select 1).Count();
        //    Assert.AreEqual(3, opCount, "Wrong count of working set assets in NH_OP following update");

        //    int rptCount = (from ws in sesh.NHRptContext.vw_WorkingSet
        //                    where ws.SessionID == sesh.SessionID
        //                    select 1).Count();
        //    Assert.AreEqual(3, rptCount, "Wrong count of working set assets in NH_RPT following update");

        //    updated = target.SetWorkingSetSelection(sesh, null);
        //    Assert.IsTrue(updated, "Working set not cleared");

        //    opCount = (from ws in sesh.NHOpContext.AssetWorkingSetReadOnly
        //               where ws.SessionID == sesh.SessionID
        //               && ws.Selected
        //               select 1).Count();
        //    Assert.AreEqual(0, opCount, "Wrong count of working set assets in NH_OP following clear");

        //    rptCount = (from ws in sesh.NHRptContext.vw_WorkingSet
        //                where ws.SessionID == sesh.SessionID
        //                select 1).Count();
        //    Assert.AreEqual(0, rptCount, "Wrong count of working set assets in NH_RPT following clear");
        //  }
        //}
        #endregion
    }
}
