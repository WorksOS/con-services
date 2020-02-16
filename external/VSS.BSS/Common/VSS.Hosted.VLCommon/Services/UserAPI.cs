using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Web.Security;
using VSS.Hosted.VLCommon.Resources;
using VSS.Hosted.VLCommon.Helpers;
using log4net;
using VSS.Hosted.VLCommon.Services.MDM;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Hosted.VLCommon.Services.Types;
using System.Security.Cryptography;

namespace VSS.Hosted.VLCommon
{
    internal class UserAPI : IUserAPI
    {
        internal static string hashAlgorithm = "SHA1";
        private static readonly string emailVerificationURL = ConfigurationManager.AppSettings["EmailVerificationURL"];
        private static readonly string mailFromVerifyEmail = ConfigurationManager.AppSettings["MailFromVerifyEmail"];
        private static readonly string key = ConfigurationManager.AppSettings["VerifyUserEncryptionKey"];
        private static readonly int linkValidity = Convert.ToInt32(ConfigurationManager.AppSettings["VerifyUserLinkValidity"]);
        private static readonly int passwordHistoryLimit = Convert.ToInt32(ConfigurationManager.AppSettings["PasswordHistoryLimit"]);
        private static readonly double passwordExpiryDaysLimit = Convert.ToDouble(ConfigurationManager.AppSettings["PasswordExpiryDaysLimit"]);
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool IsNewViewEnabled = (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableNewView"])) ? false : Convert.ToBoolean(ConfigurationManager.AppSettings["EnableNewView"]);
        private const string USERNAME_CHARACTER_PATTERN = @"0123456789abcdefghijklmnopqrstuvwxyz";
        public const int USERNAME_LENGTH = 10;

        private static readonly bool EnableDissociateCustomerUserSync = Convert.ToBoolean(ConfigurationManager.AppSettings["VSP.DissociateCustomerUser.EnableSync"]);

        private bool TriggerExternalUpdates { get; set; }

        private readonly ICustomerService _customerServiceApi;

        public UserAPI()
        {
            _customerServiceApi = API.CustomerService;
            var triggerExternalUpdatesStr = ConfigurationManager.AppSettings["TriggerExternalUpdates"];
            if (string.IsNullOrEmpty(triggerExternalUpdatesStr))
            {
                TriggerExternalUpdates = false;
            }
            else
                TriggerExternalUpdates = bool.Parse(triggerExternalUpdatesStr);
        }

        public UserAPI(ICustomerService customerService)
            : this()
        {
            _customerServiceApi = customerService;
        }

        public User Create(INH_OP opContext, long customerID, string name, string password,
              string timeZoneName, string email, DateTime? passwordExpiryUTC,
              int languageID, int? units, byte? locationDisplayTypeID,
              string globalID, byte? assetLabelPreferenceType, string firstName, string lastName,
              string jobTitle, string address, string phone, List<UserFeatureAccess> features,
              int? meterLabelPreferenceType, TemperatureUnitEnum temperatureUnit, PressureUnitEnum pressureUnit, bool validEmail = false, string createdBy = null) // ajr14975
        {
            bool isApiUser = false;
            bool isSupportUser = false;

            foreach (var featuer in features)
            {
                isApiUser = UserTypeHelper.VlApifeatureTypes.Contains(featuer.featureApp == 0
                                    ? (featuer.feature == 0 ? (int)featuer.featureChild : (int)featuer.feature)
                                    : (int)featuer.featureApp);

                isSupportUser = UserTypeHelper.VlSupportfeatureTypes.Contains(featuer.featureApp == 0
                                      ? (featuer.feature == 0 ? (int)featuer.featureChild : (int)featuer.feature)
                                      : (int)featuer.featureApp);
                if (isApiUser || isSupportUser)
                    break;
            }

            if ((isApiUser || isSupportUser) || !IsNewViewEnabled)
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException("Username and password needs to be specified appropriately");

                if (UserNameExists(opContext, name))
                    throw new InvalidOperationException("DuplicateUserName", new IntentionallyThrownException());
            }

            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException("Email address must be specified");

            if (!string.IsNullOrEmpty(password))
            {
                ValidatePassword(password, name);
            }
            else
            {
                password = Membership.GeneratePassword(10, 2);
            }

            User newUser = new User();
            if (isApiUser || isSupportUser || !IsNewViewEnabled)
            {
                newUser.Name = name;
            }
            else
            {
                newUser.Name = GetUniqueUserName();
            }
            newUser.Salt = HashUtils.CreateSalt(5);
            newUser.PasswordHash = HashUtils.ComputeHash(password, hashAlgorithm, newUser.Salt);
            newUser.Createdby = createdBy;
            newUser.InsertUTC = DateTime.UtcNow;
            newUser.UpdateUTC = DateTime.UtcNow;
            newUser.GlobalID = globalID ?? Guid.NewGuid().ToString();
            newUser.FirstName = firstName;
            newUser.LastName = lastName;
            newUser.Active = true;
            newUser.LogOnFailedCount = 0;
            newUser.fk_CustomerID = customerID;
            newUser.fk_LanguageID = languageID > 0 ? languageID : 1; // if language is not set, use default of 1 (en_us)
            newUser.TimezoneName = string.IsNullOrEmpty(timeZoneName) ? "Mountain Standard Time" : timeZoneName;
            newUser.PwdExpiryUTC = passwordExpiryUTC;
            newUser.Units = units;
            newUser.LocationDisplayType = locationDisplayTypeID;
            newUser.AssetLabelPreferenceType = assetLabelPreferenceType.HasValue ? assetLabelPreferenceType.Value : (byte)1;  //Default to 'Asset Name'
            newUser.JobTitle = jobTitle;
            newUser.Address = address;
            newUser.PhoneNumber = phone;
            newUser.MeterLabelPreferenceType = meterLabelPreferenceType;

            newUser.fk_TemperatureUnitID = temperatureUnit == TemperatureUnitEnum.None ? (int)TemperatureUnitEnum.Fahrenheit : (int)temperatureUnit; // default celsius -> changed to Fahrenheit
            newUser.fk_PressureUnitID = pressureUnit == PressureUnitEnum.None ? (int)PressureUnitEnum.PSI : (int)pressureUnit;

            bool emailIdAlreadyExists = false;

            if (isApiUser)
            {
                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.fk_CustomerID == customerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API
                                        select us.Name).Any();
            }
            else if (isSupportUser)
            {
                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.fk_CustomerID == customerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Support
                                        select us.Name).Any();
            }
            else
            {
                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.fk_CustomerID == customerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Client
                                        select us.Name).Any();
            }

            if (emailIdAlreadyExists)
                throw new InvalidOperationException("duplicateEmail");
            newUser.EmailContact = email;
            newUser.IsEmailValidated = validEmail;

            newUser.EmailVerificationGUID = Guid.NewGuid().ToString();
            newUser.EmailVerificationUTC = DateTime.UtcNow;

            opContext.User.AddObject(newUser);

            if (opContext.SaveChanges() <= 0)
                throw new InvalidOperationException("Failed to save user");

            SaveUserFeatures(opContext, features, newUser, createdBy);

            if (!SaveUserPasswordHistory(opContext, newUser))
                throw new InvalidOperationException("Failed to save password history");
            if (!validEmail)
            {
                string language = (from lang in opContext.LanguageReadOnly
                                   where lang.ID == newUser.fk_LanguageID
                                   select lang.ISOName).FirstOrDefault();

                if (!isApiUser && !isSupportUser)
                    SendEmail(newUser.FirstName, newUser.LastName, newUser.EmailContact, language, newUser.EmailVerificationGUID, (IsNewViewEnabled ? newUser.EmailContact : newUser.Name), newUser.EmailVerificationUTC.Value);
                else if (isSupportUser)
                    SendEmail(newUser.FirstName, newUser.LastName, newUser.EmailContact, language, newUser.EmailVerificationGUID, newUser.Name, newUser.EmailVerificationUTC.Value);
                else
                {
                    SendEmailForApiUser(opContext, newUser.FirstName, newUser.LastName, newUser.EmailContact, language, newUser.EmailVerificationGUID, newUser.Name, newUser.EmailVerificationUTC.Value, newUser.ID, newUser.fk_CustomerID.Value);
                }
            }

            return newUser;
        }


        public User CreateSSOUser(INH_OP opContext, long customerID, string name, string password,
            string timeZoneName, string email, int languageID, int? units, byte? locationDisplayTypeID,
            string globalID, byte? assetLabelPreferenceType, string firstName, string lastName,
            string jobTitle, string address, string phone, List<UserFeatureAccess> features,
            int? meterLabelPreferenceType, TemperatureUnitEnum temperatureUnit, PressureUnitEnum pressureUnit, string createdBy = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Username needs to be specified appropriately");

            if (UserNameExists(opContext, name))
                throw new InvalidOperationException("DuplicateUserName", new IntentionallyThrownException());

            var salt = HashUtils.CreateSalt(5);
            var newUser = new User
            {
                Name = name,
                Salt = salt,
                PasswordHash = HashUtils.ComputeHash(password, hashAlgorithm, salt),
                Createdby = createdBy,
                InsertUTC = DateTime.UtcNow,
                UpdateUTC = DateTime.UtcNow,
                GlobalID = globalID ?? Guid.NewGuid().ToString(),
                FirstName = firstName,
                LastName = lastName,
                EmailContact = email,
                Active = true,
                LogOnFailedCount = 0,
                fk_CustomerID = customerID,
                fk_LanguageID = languageID > 0 ? languageID : 1, // if language is not set, use default of 1 (en_us)
                TimezoneName = string.IsNullOrEmpty(timeZoneName) ? "Mountain Standard Time" : timeZoneName,
                Units = units,
                LocationDisplayType = locationDisplayTypeID,
                AssetLabelPreferenceType = assetLabelPreferenceType.HasValue ? assetLabelPreferenceType.Value : (byte)1,  //Default to 'Asset Name'
                JobTitle = jobTitle,
                Address = address,
                PhoneNumber = phone,
                MeterLabelPreferenceType = meterLabelPreferenceType,

                fk_TemperatureUnitID = temperatureUnit == TemperatureUnitEnum.None ? (int)TemperatureUnitEnum.Fahrenheit : (int)temperatureUnit, // default celsius -> changed to Fahrenheit
                fk_PressureUnitID = pressureUnit == PressureUnitEnum.None ? (int)PressureUnitEnum.PSI : (int)pressureUnit,
                IsEmailValidated = true,
                EmailVerificationGUID = Guid.NewGuid().ToString(),
                EmailVerificationUTC = DateTime.UtcNow
            };

            opContext.User.AddObject(newUser);

            if (opContext.SaveChanges() <= 0)
                throw new InvalidOperationException("Failed to save user");

            SaveUserFeatures(opContext, features, newUser, createdBy);

            return newUser;
        }

        public void CreateUpdateFederatedLogonInfo(long userId, string cwsLoginId)
        {
            using (var opContext = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var existingInfo = (from f in opContext.FederatedLogonInfo
                                    where
                                      f.fk_UserID == userId
                                    select f).SingleOrDefault();

                if (existingInfo != null)
                {
                    existingInfo.CWSLoginID = cwsLoginId;
                }
                else
                {
                    existingInfo = new FederatedLogonInfo
                    {
                        fk_UserID = userId,
                        CWSLoginID = cwsLoginId
                    };

                    opContext.FederatedLogonInfo.AddObject(existingInfo);
                }
                opContext.SaveChanges();
            }
        }

        public bool Update(INH_OP opContext, long userID, bool? isActive = null, string name = null, string timeZoneName = null,
              DateTime? termsofUseAcceptedUTC = null, int? units = null, byte? locationDisplayTypeID = null, byte? assetLabelPreferenceType = null,
              string firstName = null, string lastName = null, string jobTitle = null, string address = null,
              string phone = null, string email = null, int? meterLabelPreferenceType = null, TemperatureUnitEnum temperatureUnit = TemperatureUnitEnum.None, PressureUnitEnum pressureUnit = PressureUnitEnum.None)
        {
            User user = (from u in opContext.User where u.ID == userID && u.Active select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            string password = string.Empty;
            string userName = string.Empty;

            if (isActive != null)
            {
                user.Active = isActive.Value;
                if (!isActive.Value)
                {
                    Random r = new Random();
                    user.Name += "_delete" + GenerateAlphanumericString(user.Name);
                }
            }
            if (timeZoneName != null)
                user.TimezoneName = timeZoneName;
            if (termsofUseAcceptedUTC != null)
                user.TermsofUseAcceptedUTC = termsofUseAcceptedUTC.Value;
            if (units != null)
                user.Units = units.Value;
            if (name != null)
                user.Name = name;
            if (locationDisplayTypeID != null)
                user.LocationDisplayType = locationDisplayTypeID.Value;
            if (assetLabelPreferenceType != null)
                user.AssetLabelPreferenceType = assetLabelPreferenceType.Value;
            if (meterLabelPreferenceType != null)
                user.MeterLabelPreferenceType = meterLabelPreferenceType.Value;
            if (firstName != null)
                user.FirstName = firstName;
            if (lastName != null)
                user.LastName = lastName;
            if (jobTitle != null)
                user.JobTitle = jobTitle;
            if (address != null)
                user.Address = address;
            if (phone != null)
                user.PhoneNumber = phone;

            if (temperatureUnit != TemperatureUnitEnum.None) // ajr
                user.fk_TemperatureUnitID = (int)temperatureUnit;

            if (pressureUnit != PressureUnitEnum.None) // ajr
                user.fk_PressureUnitID = (int)pressureUnit;

            DisableUserAccountExternal(user, opContext);

            //Email Validations handled here
            return ValidateAndUpdateEmail(opContext, email, user);


        }

        public bool UpdateSSOUser(INH_OP opContext, long userID, string name, string firstName = null, string lastName = null, string jobTitle = null, string address = null, string phone = null, string email = null)
        {
            User user = (from u in opContext.User where u.ID == userID && u.Active select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            var userNameExists = opContext.UserReadOnly.Any(u => u.Name == name && u.ID != user.ID);

            if (userNameExists)
                throw new InvalidOperationException("DuplicateUserName", new IntentionallyThrownException());

            bool isNameChanged = user.Name != name;

            if (name != null)
                user.Name = name;
            if (firstName != null)
                user.FirstName = firstName;
            if (lastName != null)
                user.LastName = lastName;
            if (jobTitle != null)
                user.JobTitle = jobTitle;
            if (address != null)
                user.Address = address;
            if (phone != null)
                user.PhoneNumber = phone;
            if (email != null)
                user.EmailContact = email;

            user.UpdateUTC = DateTime.UtcNow;
            bool success = opContext.SaveChanges() > 0;

            if (isNameChanged)
                API.User.CreateUpdateFederatedLogonInfo(user.ID, user.Name);

            return success;
        }

        public bool UpdateUserProfile(INH_OP opContext, long userID,
           string firstName, string lastName, string email, string jobTitle = null, string address = null,
           string phone = null)
        {
            User user = (from u in opContext.User where u.ID == userID && u.Active select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            if (firstName != null)
                user.FirstName = firstName;
            if (lastName != null)
                user.LastName = lastName;
            if (jobTitle != null)
                user.JobTitle = jobTitle;
            if (address != null)
                user.Address = address;
            if (phone != null)
                user.PhoneNumber = phone;

            return ValidateAndUpdateEmail(opContext, email, user);


        }

        private bool ValidateAndUpdateEmail(INH_OP opContext, string email, User user)
        {
            int emailModifyCount = Convert.ToInt32(ConfigurationManager.AppSettings["EmailModifyLimit"]);
            string language = string.Empty;
            bool emailChanged = false;
            bool emailIdAlreadyExists = false;
            bool isApiUser = false;
            bool isSupportUser = false;
            DateTime CurrentTime = DateTime.UtcNow;

            //save old user values
            User oldUser = new User();
            oldUser.ID = user.ID;
            oldUser.fk_CustomerID = user.fk_CustomerID;
            oldUser.Name = user.Name;
            oldUser.EmailContact = user.EmailContact;
            oldUser.UserUID = user.UserUID;

            isApiUser = (from usf in opContext.UserFeatureReadOnly where usf.fk_User == user.ID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API select usf).Any();
            isSupportUser = (from usf in opContext.UserFeatureReadOnly where usf.fk_User == user.ID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Support select usf).Any();

            if (email != null)
            {
                emailChanged = user.EmailContact != email;

                // Maximum 20 Email modification only allowed from any edit screen
                if (user.EmailModifiedCount != null && emailChanged && !isApiUser && !isSupportUser)
                {
                    if (user.EmailModifiedCount >= emailModifyCount)
                        throw new InvalidOperationException("emailChangeLimitExceed", new IntentionallyThrownException());
                }

                // Checking for EmailID already exists in VL within Customer Account     

                emailIdAlreadyExists = IsEmailAlreadyExists(opContext, email, user);

                if (emailIdAlreadyExists)
                    throw new InvalidOperationException("duplicateEmail");

                if (emailChanged)
                {
                    string oldEmail = user.EmailContact;
                    emailModifyCount = (user.EmailModifiedCount == null) ? 0 : (int)user.EmailModifiedCount;
                    user.EmailContact = email;
                    user.IsEmailValidated = false;
                    user.EmailVerificationGUID = Guid.NewGuid().ToString();
                    user.EmailVerificationUTC = CurrentTime;
                    user.EmailVerificationTrackingUTC = CurrentTime;
                    user.EmailModifiedCount = emailModifyCount + 1;
                    language = (from lang in opContext.LanguageReadOnly where lang.ID == user.fk_LanguageID select lang.ISOName).FirstOrDefault();
                    // Since Email has changed, needs to reset the following TPass related values.
                    user.UserUID = null;
                    user.Domain = null;
                    user.IdentityMigrationUTC = null;
                    user.IsVLLoginID = false;
                }
            }

            user.UpdateUTC = CurrentTime;
            if (opContext.SaveChanges() > 0)
            {
                if (emailChanged)
                {
                    if (EnableDissociateCustomerUserSync)
                    {
                      MdmHelpers.SyncDissociateCustomerUserWithNextGen(_customerServiceApi, oldUser, opContext);
                    }
                    if (isApiUser)
                        SendEmailForApiUser(opContext, user.FirstName, user.LastName, user.EmailContact, language, user.EmailVerificationGUID, user.Name, user.EmailVerificationUTC.Value, user.ID, user.fk_CustomerID.Value);
                    else if(isSupportUser)
                        SendEmail(user.FirstName, user.LastName, user.EmailContact, language, user.EmailVerificationGUID, user.Name, user.EmailVerificationUTC.Value);
                    else
                        SendEmail(user.FirstName, user.LastName, user.EmailContact, language, user.EmailVerificationGUID, (IsNewViewEnabled ? user.EmailContact : user.Name), user.EmailVerificationUTC.Value);
                }
                return true;
            }

            return false;

        }

        private static bool IsEmailAlreadyExists(INH_OP opContext, string email, User user)
        {
            bool emailIdAlreadyExists = false;
            bool isApiUser = (from usf in opContext.UserFeatureReadOnly where usf.fk_User == user.ID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API select usf).Any();
            bool isSupportUser = (from usf in opContext.UserFeatureReadOnly where usf.fk_User == user.ID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Support select usf).Any();

            if (isApiUser)
            {
                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.Name != user.Name && us.fk_CustomerID == user.fk_CustomerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API
                                        select us.Name).Any();
            }
            else if (isSupportUser)
            {
                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.Name != user.Name && us.fk_CustomerID == user.fk_CustomerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Support
                                        select us.Name).Any();
            }
            else
            {

                emailIdAlreadyExists = (from us in opContext.UserReadOnly
                                        join usf in opContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.Name != user.Name && us.fk_CustomerID == user.fk_CustomerID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Client
                                        select us.Name).Any();
            }
            return emailIdAlreadyExists;
        }


        public bool Delete(INH_OP opContext, long userID)
        {
            bool deleted = false;
            var user = (from u in opContext.User where u.ID == userID select u).SingleOrDefault();

            if (user != null)
            {
                user.UpdateUTC = DateTime.UtcNow;
                user.Active = false;
                deleted = opContext.SaveChanges() > 0;

                DeleteFederatedLogonInfo(opContext, userID);
                DisableUserAccountExternal(user, opContext);


            }

            return deleted;
        }



        public void DeleteFederatedLogonInfo(INH_OP opContext, long userID)
        {
            var federatedLogonInfos =
              (from fli in opContext.FederatedLogonInfo where fli.fk_UserID == userID select fli);
            foreach (var federatedLogonInfo in federatedLogonInfos)
            {
                opContext.FederatedLogonInfo.DeleteObject(federatedLogonInfo);
            }
            opContext.SaveChanges();
        }

        public bool UpdatePassword(INH_OP opContext, long userID, string newPassword, bool updationByAdmin = false)
        {
            User user = (from u in opContext.User where u.ID == userID && u.Active select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            if (!updationByAdmin)
                user.PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit); //Set password expiry to 180 days from last set date 
            else
                user.PwdExpiryUTC = null;

            var federatedUser = (from fedUser in opContext.FederatedLogonInfo
                                 where fedUser.fk_UserID == userID
                                 select fedUser).FirstOrDefault();
            if (federatedUser != null)
                opContext.FederatedLogonInfo.DeleteObject(federatedUser);

            ValidatePassword(opContext, userID, newPassword);
            return PasswordSet(opContext, userID, newPassword, user);
        }

        public Tuple<string, int> VerifyEmail(INH_OP ctx, string verifyEmailGuid, bool isIdentityUserExists)
        {
            Tuple<string, int> verified;

            User user = (from u in ctx.User where u.EmailVerificationGUID == verifyEmailGuid select u).SingleOrDefault();
            user.IsEmailValidated = true;

            var isApiUser = (from uf in ctx.UserFeatureReadOnly
                             where uf.fk_User == user.ID && uf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API
                    && uf.fk_FeatureAccess != (int)FeatureAccessEnum.None
                             select uf.fk_User).Any();

            var isSupportUser = (from uf in ctx.UserFeatureReadOnly
                                 where uf.fk_User == user.ID && uf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Support
                        && uf.fk_FeatureAccess != (int)FeatureAccessEnum.None
                                 select uf.fk_User).Any();

            if (IsNewViewEnabled && !isApiUser && !isSupportUser && isIdentityUserExists)
            {
                user.PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit);
            }

            if (1 >= ctx.SaveChanges())
            {
                if (!isApiUser && !isSupportUser)
                {
                    if (IsNewViewEnabled)
                    {
                        if (isIdentityUserExists)
                        {
                            verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.ExistingIdentityClientUser);
                        }
                        else
                        {
                            verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.FirstTimeIdentityClientUser);
                        }
                    }
                    else
                    {
                        if (user.LastLoginUTC.HasValue)
                            verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.ExistingClientUser);
                        else
                            verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.FirstTimeClientUser);
                    }
                }
                else if (isSupportUser)
                {
                    verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.SupportUser);
                }
                else
                    verified = new Tuple<string, int>(user.GlobalID, (int)VerifyEmailCode.ApiUser);
                return verified;
            }
            return null;
        }

        public bool ResendEmail(string username)
        {
            using (INH_OP opContext = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User user = (from u in opContext.User where u.Name == username && u.Active select u).SingleOrDefault();
                bool isApiUser = false;
                if (user == null)
                    throw new InvalidOperationException("Cannot resend email for deleted user", new IntentionallyThrownException());

                user.IsEmailValidated = false;
                user.EmailVerificationGUID = Guid.NewGuid().ToString();
                user.EmailVerificationUTC = DateTime.UtcNow;
                if (opContext.SaveChanges() > 0)
                {
                    string language = (from lang in opContext.LanguageReadOnly where lang.ID == user.fk_LanguageID select lang.ISOName).FirstOrDefault();
                    isApiUser = (from uf in opContext.UserFeatureReadOnly
                                 where uf.fk_User == user.ID && uf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API
                                   && uf.fk_FeatureAccess != (int)FeatureAccessEnum.None
                                 select uf.fk_User).Any();
                    if (isApiUser)
                        SendEmailForApiUser(opContext, user.FirstName, user.LastName, user.EmailContact, language, user.EmailVerificationGUID, user.Name, user.EmailVerificationUTC.Value, user.ID, user.fk_CustomerID.Value);
                    else
                        SendEmail(user.FirstName, user.LastName, user.EmailContact, language, user.EmailVerificationGUID, (IsNewViewEnabled ? user.EmailContact : user.Name), user.EmailVerificationUTC.Value);
                    return true;
                }
                return false;
            }
        }

        public bool IsEmailAlreadyVerified(string username, INH_OP context)
        {
            bool isEmailVerified = false;
            var user = (from u in context.UserReadOnly where u.Name == username select u).SingleOrDefault();
            return isEmailVerified = (user.IsEmailValidated && (user.EmailVerificationUTC != null && user.EmailVerificationGUID != null)) ? true : false;
        }

        public bool SkipEmailVerification(INH_OP opContext, string userName)
        {
            User user = (from u in opContext.User
                         join c in opContext.CustomerReadOnly on u.fk_CustomerID equals c.ID
                         where u.Name == userName && u.Active && c.IsActivated
                         select u).SingleOrDefault();

            if (user.EmailVerificationTrackingUTC == null)
            {
                user.EmailVerificationTrackingUTC = DateTime.UtcNow;
                return (opContext.SaveChanges() > 0);
            }
            return false;
        }

        public bool EmailVerification(INH_OP opContext, string userName, string emailID, bool isVerify)
        {
            bool isEmailResendOrVerifySucceeded = false;
            User user = null;

            user = (from u in opContext.User
                    join c in opContext.CustomerReadOnly on u.fk_CustomerID equals c.ID
                    where u.Name == userName && u.Active && c.IsActivated
                    select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Invalid User", new IntentionallyThrownException());

            if (user.EmailContact != emailID)
            {
                isEmailResendOrVerifySucceeded = ValidateAndUpdateEmail(opContext, emailID, user);
            }
            else
            {
                if (IsEmailAlreadyExists(opContext, emailID, user))
                    throw new InvalidOperationException("duplicateEmail");
                else
                    isEmailResendOrVerifySucceeded = ResendEmail(userName);

                if (isVerify)
                {
                    user.EmailVerificationTrackingUTC = DateTime.UtcNow;
                    return ((opContext.SaveChanges() > 0) && isEmailResendOrVerifySucceeded);
                }
            }
            return isEmailResendOrVerifySucceeded;
        }

        public bool UpdatePassword(string forgotPassordGUID, string newPassword)
        {

            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                DateTime yesterday = DateTime.UtcNow.AddDays(-1);
                User user = (from u in ctx.User where u.PasswordResetGUID == forgotPassordGUID select u).SingleOrDefault();

                if (user == null)
                    throw new InvalidOperationException("invalidURL", new IntentionallyThrownException());

                else if (!user.Active)
                    throw new InvalidOperationException("inactiveUser", new IntentionallyThrownException());

                else if (user.PasswordResetUTC < yesterday)
                    throw new InvalidOperationException("urlExpired", new IntentionallyThrownException());

                else if (user.Name == newPassword)
                    throw new InvalidOperationException("usernameAndPasswordMatchErrorMesg", new IntentionallyThrownException());

                else
                    ValidatePassword(ctx, user.ID, newPassword);

                user.PasswordResetGUID = null;
                user.PasswordResetUTC = null;
                user.PwdExpiryUTC = DateTime.UtcNow.AddDays(passwordExpiryDaysLimit);

                //Unlock the user account on successful password reset
                if (PasswordSet(ctx, user.ID, newPassword, user))
                {
                    user.LogOnFailedCount = 0;
                    user.LogOnFirstFailedUTC = null;
                    user.LogOnLastFailedUTC = null;
                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
        }


        private bool PasswordSet(INH_OP opContext, long userID, string newPassword, User user)
        {
            bool success = false;
            user.PasswordHash = HashUtils.ComputeHash(newPassword, hashAlgorithm, user.Salt);
            user.UpdateUTC = DateTime.UtcNow;


            if (opContext.SaveChanges() > 0)
            {
                var userPasswordHistories = (from i in opContext.UserPasswordHistory
                                             where i.fk_UserID == userID
                                             select i).OrderByDescending(a => a.InsertUTC).Take(passwordHistoryLimit);


                //If maximum reached then remove oldest one (FIFO)
                if ((userPasswordHistories != null) && (userPasswordHistories.Count<UserPasswordHistory>() >= passwordHistoryLimit))
                {
                    UserPasswordHistory oldest = (from r in userPasswordHistories
                                                  orderby r.InsertUTC ascending
                                                  select r).First();
                    //Delete a row in the 'UserPasswordHistory' table
                    opContext.UserPasswordHistory.DeleteObject(oldest);
                }
                success = SaveUserPasswordHistory(opContext, user);
            }
            return success;
        }

        public bool UpdateFeatureAccess(INH_OP opContext, long userID, IEnumerable<UserFeatureAccess> features, string updatedBy)
        {
            List<string> permissions = new List<string>();

            var user = (from u in opContext.UserReadOnly
                        where u.ID == userID && u.Active
                        select
                          new
                          {
                              ID = u.ID,
                              Name = u.Name
                          }).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            List<UserFeature> uFeatures = (from uf in opContext.UserFeature
                                           where uf.fk_User == userID
                                           select uf).ToList();
            for (int i = uFeatures.Count - 1; i >= 0; i--)
                opContext.UserFeature.DeleteObject(uFeatures[i]);

            foreach (UserFeatureAccess ufa in features)
            {
                int featureID = ufa.featureApp == 0 ? (ufa.feature == 0 ? (int)ufa.featureChild : (int)ufa.feature) : (int)ufa.featureApp;
                UserFeature newUserFeature = new UserFeature();
                newUserFeature.fk_User = userID;
                newUserFeature.fk_Feature = featureID;
                newUserFeature.fk_FeatureAccess = (int)ufa.access;
                newUserFeature.Createdby = updatedBy;
                newUserFeature.CreatedDate = DateTime.UtcNow;
                opContext.UserFeature.AddObject(newUserFeature);
                permissions.Add(((FeatureEnum)featureID).ToString());
            }
            string userfeatures = string.Join(", ", permissions);

            var success = opContext.SaveChanges() > 0;
            if (!string.IsNullOrEmpty(updatedBy))
            {
                Log.IfInfoFormat("{0} has updated the  user {1}  at time {2} with  permission {3}.", updatedBy, user.Name, DateTime.UtcNow, userfeatures);
            }
            return success;
        }

        public bool CreateUpdateUserPreference(INH_OP opContext, long userID, Dictionary<string, string> preferences)
        {
            var user = (from u in opContext.UserReadOnly where u.ID == userID && u.Active select u.ID).SingleOrDefault();
            if (user < 1)
                throw new InvalidOperationException("Cannot update deleted user", new IntentionallyThrownException());

            foreach (string prefKey in preferences.Keys)
            {

                UserPreferences existingPreference = (from p in opContext.UserPreferences
                                                      where p.fk_UserID == userID
                                                          && p.Key == prefKey
                                                      select p).FirstOrDefault<UserPreferences>();

                if (existingPreference != null)
                {
                    // update existing preference
                    existingPreference.ValueXML = preferences[prefKey];
                    existingPreference.UpdateUTC = DateTime.UtcNow;
                }
                else
                {
                    // create a new preference and add it to the context  
                    UserPreferences newPref = new UserPreferences();
                    newPref.Key = prefKey;
                    newPref.ValueXML = preferences[prefKey];
                    newPref.fk_UserID = userID;
                    newPref.UpdateUTC = DateTime.UtcNow;
                    opContext.UserPreferences.AddObject(newPref);
                }
            }
            return opContext.SaveChanges() > 0;
        }

        public Dictionary<string, string> GetUserPreferencesByKey(INH_OP opContext, long userID, string prefKey)
        {
            string userPrefXml = (from p in opContext.UserPreferences
                                  where p.fk_UserID == userID
                                      && p.Key == prefKey
                                  select p.ValueXML).FirstOrDefault<string>();

            if (userPrefXml != null)
            {
                XElement xmlblob = XElement.Parse(userPrefXml);
                var prefs = (from x in xmlblob.Attributes()
                             select x).ToDictionary(g => g.Name.ToString(), h => h.Value);
                return prefs;
            }

            return null;
        }

        public string GetUserPreferenceValueByKey(INH_OP opContext, long userID, string prefKey, string valueKey)
        {
            Dictionary<string, string> prefs = GetUserPreferencesByKey(opContext, userID, prefKey);
            if (prefs != null && prefs.ContainsKey(valueKey))
            {
                return prefs[valueKey];
            }

            return null;
        }

        public bool UpdateLanguage(INH_OP opContext, long userID, string languageISOName)
        {
            User user = (from u in opContext.User where u.ID == userID && u.Active select u).SingleOrDefault();
            if (user == null)
                throw new InvalidOperationException("User not found.");

            Language lang = (from l in opContext.Language
                             where l.ISOName == languageISOName
                             select l).FirstOrDefault();
            if (lang == null)
                throw new InvalidOperationException("Language not found.");

            user.fk_LanguageID = lang.ID;
            user.UpdateUTC = DateTime.UtcNow;

            return opContext.SaveChanges() > 0;
        }

        public bool CreateUpdateUserActivation(INH_OP opContext, long userID, int activationStatusID, DateTime? sentUTC, string sentTo)
        {
            var userAct = (from ua in opContext.UserActivation
                           where ua.fk_UserID == userID
                           select ua).SingleOrDefault();

            //Create
            if (null == userAct)
            {
                UserActivation activation = new UserActivation();
                activation.fk_UserID = userID;
                activation.fk_UserActivationStatusID = activationStatusID;

                if (!string.IsNullOrEmpty(sentTo))
                {
                    activation.SentTo = sentTo;
                }
                opContext.UserActivation.AddObject(activation);
            }
            else
            {
                userAct.fk_UserActivationStatusID = activationStatusID;

                if (!string.IsNullOrEmpty(sentTo))
                {
                    userAct.SentTo = sentTo;
                }
                if (sentUTC.HasValue)
                {
                    userAct.SentUTC = sentUTC.Value;
                }
            }
            return opContext.SaveChanges() > 0;
        }

        public bool UserNameExists(INH_OP opContext, string username)
        {
            // Even deleted usernames cannot be reused
            var userIDs = from u in opContext.UserReadOnly
                          where u.Name == username
                          select u.ID;

            return userIDs.Any();
        }

        public string GetUniqueUserName()
        {
            string generatedUserName;

            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                do
                {
                    generatedUserName = GenerateAlphaNumericString(USERNAME_LENGTH, USERNAME_CHARACTER_PATTERN);

                } while (UserNameExists(opCtx, generatedUserName));
            }

            return generatedUserName;
        }

        public bool EmailIDExists(INH_OP opContext, string emailID, string userName, long customerId)
        {

            return (from u in opContext.UserReadOnly
                    join uf in opContext.UserFeatureReadOnly
                    on u.ID equals uf.fk_User
                    where u.EmailContact == emailID && u.fk_CustomerID == customerId && u.Active && uf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Client
                    select u.ID).Any();
        }

        //Method to update the username/password for a new account registration
        public bool ModifyUserForAccountReg(INH_OP opContext, string oldUserName, string newUserName, string password)
        {
            //Get the list of users for the old user name
            List<User> users = (from u in opContext.User
                                where u.Name == oldUserName
                                && u.Active
                                select u).ToList<User>();

            //Return 'false' if no default user is found
            if (users.Count == 0)
                return false;

            if (users.Count > 1)
                throw new InvalidOperationException(string.Format("Multiple users exist for name: {0}", oldUserName), new IntentionallyThrownException());

            int existingUsersForNewName = (from u in opContext.UserReadOnly
                                           where u.Name == newUserName
                                           && u.Active
                                           select u).Count<User>();

            if (existingUsersForNewName != 0)
                throw new InvalidOperationException(string.Format("User {0} already exists. Please choose other name", newUserName), new IntentionallyThrownException());

            User accountUser = users.First<User>();
            accountUser.Name = newUserName;
            accountUser.PasswordHash = HashUtils.ComputeHash(password, hashAlgorithm, accountUser.Salt);

            if (!SaveUserPasswordHistory(opContext, accountUser))
                throw new InvalidOperationException("Failed to save user password and history changes");

            return CreateUpdateUserActivation(opContext, accountUser.ID, (int)UserActivationStatusEnum.Active, null, string.Empty);
        }

        public bool IsCorporateUser(INH_OP opContext, long? customerID)
        {
            bool isCorporateUser = (from c in opContext.CustomerReadOnly
                                    where c.ID == customerID
                                    select c.fk_CustomerTypeID == (int)CustomerTypeEnum.Corporate).Single();
            return isCorporateUser;
        }

        public void ValidatePassword(INH_OP opContext, long userID, string password)
        {
            var username = (from u in opContext.UserReadOnly
                            where u.ID == userID && u.Active
                            select u.Name).SingleOrDefault<string>();
            ValidatePassword(password, username);

            if (IsPreviousPassword(opContext, userID, password))
                throw new InvalidOperationException("passwordMatchesHistoryError", new IntentionallyThrownException());
        }

        // Returns the user's location preference - Site, Address or LatLong
        public UserLocationPreferenceEnum GetLocationPreference(INH_OP opContext, long userID)
        {
            byte? locPref = opContext.UserReadOnly.Where(uu => uu.ID == userID).Select(uu => uu.LocationDisplayType).FirstOrDefault();
            if (!locPref.HasValue)
                locPref = 0;

            UserLocationPreferenceEnum userLocPref = (UserLocationPreferenceEnum)Enum.Parse(typeof(UserLocationPreferenceEnum), locPref.ToString());
            return userLocPref;
        }

        public bool HasFeatureAccess(long userID, FeatureEnum feature, FeatureAccessEnum featureAccess)
        {
            bool hasAccess = false;
            using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                AuthorizorAttribute attribute = new AuthorizorAttribute
                {
                    FeatureApp = FeatureAppEnum.NHWeb,
                    FeatureAccess = featureAccess,
                    Feature = feature
                };
                string featureRet;
                Authorizor auth = new Authorizor();
                hasAccess = auth.HasAuthorization(opCtx, userID, new object[] { attribute }, out featureRet);
            }

            return hasAccess;
        }

        //given an username and password, 
        //this method will verify that the username and 
        //password exists in our database and the user is active.
        public bool ValidateUser(string username, string password)
        {
            var valid = false;

            using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {

                User user = (from u in ctx.User
                             join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                             where u.Name.Equals(username) && u.Active
                             select u).FirstOrDefault();

                if (user != null)
                {
                    var pwdHash = HashUtils.ComputeHash(password, "SHA1", user.Salt);
                    if (pwdHash == user.PasswordHash)
                    {
                        valid = true;
                        //Updating last login UTC column in User table.(US 30092)
                        user.LastLoginUTC = DateTime.UtcNow;
                        ctx.SaveChanges();
                    }
                }
            }
            return valid;
        }

        public bool CreatedUpdateFederationLogonInfo(INH_OP opContext, long userID, string federatedValue, string federatedEntity, string catLoginId = null)
        {
            if (federatedValue == null)
            {
                throw new ArgumentNullException("federatedValue cannot be null");
            }

            if (federatedEntity == null)
            {
                throw new ArgumentNullException("federatedEntity cannot be null");
            }

            var existingInfo = (from f in opContext.FederatedLogonInfo
                                where
                                  f.FederationEntity.ToUpper() == federatedEntity.ToUpper() &&
                                  f.fk_UserID == userID
                                select f).SingleOrDefault();

            if (existingInfo != null)
            {
                existingInfo.FederationValue = federatedValue;
                existingInfo.CWSLoginID = (!string.IsNullOrEmpty(catLoginId)) ? catLoginId : null;
            }
            else
            {
                existingInfo = new FederatedLogonInfo
                                 {
                                     fk_UserID = userID,
                                     FederationEntity = federatedEntity,
                                     FederationValue = federatedValue,
                                     CWSLoginID = (!string.IsNullOrEmpty(catLoginId)) ? catLoginId : null
                                 };

                opContext.FederatedLogonInfo.AddObject(existingInfo);
            }
            return opContext.SaveChanges() > 0;
        }

        public void UpdateFederatedInfo(string catLoginId, string federatedValue)
        {
            using (var opContext = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                var existingInfo = (from f in opContext.FederatedLogonInfo
                                    where f.FederationValue == federatedValue
                                    select f).SingleOrDefault();

                if (existingInfo != null && (string.IsNullOrEmpty(existingInfo.CWSLoginID) && (!string.IsNullOrEmpty(catLoginId))))
                {
                    existingInfo.CWSLoginID = catLoginId;
                    opContext.SaveChanges();
                }

            }

        }

        public bool FederatedLogonExists(INH_OP ctx, string federatedEntity, string federatedValue)
        {
            return ctx.FederatedLogonInfoReadOnly.Any(t => t.FederationEntity == federatedEntity && t.FederationValue == federatedValue);
        }

        public static string GenerateAlphanumericString(string username)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(username, 5)
              .Select(s => s[random.Next(5)]).ToArray());
        }

        #region VLSupport

        public List<User> GetUserDetails(long customerID)
        {
            using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                return GetUserEnumerable(ctx, customerID).ToList();
            }
        }

        public int GetUserDetailsCount(long customerID)
        {
            using (var ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                return GetUserEnumerable(ctx, customerID).Count();
            }
        }

        public User GetCreatedUser(long userID)
        {
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                User createdUser = (from u in opCtx.UserReadOnly
                                    where u.ID == userID && u.Active
                                    select u).SingleOrDefault();
                return createdUser;
            }
        }

        public User GetFirstAdminUser(long customerID)
        {
            using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                return (from u in opCtx.UserReadOnly
                        where u.fk_CustomerID == customerID &&
                        u.Createdby == Bss.BssCustomerService.USER_CREATED_BY
                        select u).SingleOrDefault();
            }
        }
        #endregion

        #region Implementation Helpers

        private string GenerateAlphaNumericString(int length, string characterSet)
        {
            var rng = new RNGCryptoServiceProvider();
            var random = new byte[length];
            rng.GetNonZeroBytes(random);

            var buffer = new char[length];
            var usableChars = characterSet.ToCharArray();
            var usableLength = usableChars.Length;

            for (int index = 0; index < length; index++)
            {
                buffer[index] = usableChars[random[index] % usableLength];
            }

            return new string(buffer);
        }

        private void SaveUserFeatures(INH_OP opContext, List<UserFeatureAccess> features, User newUser, string createdBy)
        {
            var now = DateTime.UtcNow;
            if (features != null && features.Any())
            {
                List<string> permissions = new List<string>();
                foreach (UserFeatureAccess ufa in features)
                {
                    int featureID = ufa.featureApp == 0
                                      ? (ufa.feature == 0 ? (int)ufa.featureChild : (int)ufa.feature)
                                      : (int)ufa.featureApp;

                    var uf = new UserFeature
                    {
                        fk_Feature = featureID,
                        fk_User = newUser.ID,
                        fk_FeatureAccess = (int)ufa.access,
                        Createdby = createdBy,
                        CreatedDate = now
                    };
                    permissions.Add(((FeatureEnum)featureID).ToString());
                    opContext.UserFeature.AddObject(uf);
                }

                string userfeatures = string.Join(",", permissions);

                if (opContext.SaveChanges() < 1)
                {
                    throw new InvalidOperationException("Failed to add features to user");
                }
                if (!string.IsNullOrEmpty(createdBy))
                {
                    Log.IfInfoFormat("{0} has created the new user {1} at time {2} with permission {3}.", createdBy, newUser.Name, now, userfeatures);
                }
            }
        }

        private static void ValidatePassword(string newPassword, string username)
        {
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullException("Invalid parameter password. Password can not be null or an empty string");

            if (newPassword.Length < 8)
                throw new ArgumentException("Invalid parameter password. Password can not be less than 8 characters");

            if (newPassword.Length > 150)
                throw new ArgumentException("Invalid parameter password. Password can not be more than 150 characters");

            if (!newPassword.Any(c => Char.IsUpper(c)))
                throw new ArgumentException("Invalid parameter password. Password must contain atleast one uppercase letter");

            if (!newPassword.Any(c => Char.IsLower(c)))
                throw new ArgumentException("Invalid parameter password. Password must contain atleast one lowercase letter");

            if (!newPassword.Any(c => Char.IsNumber(c)))
                throw new ArgumentException("Invalid parameter password. Password must contain atleast one number");

            if (newPassword.IndexOfAny(new char[] { '!', '@', '$', '#', '%', '^', '&', '*', '-', '+', '_' }) == -1)
                throw new ArgumentException("Invalid parameter password. Password must contain atleast one the following symbols !@#$%^&*-+_");

            if (newPassword.Equals(username))
                throw new ArgumentException("Invalid parameter password. Password must must not be same as username");
        }

        private static bool IsPreviousPassword(INH_OP opContext, long userID, string password)
        {
            //the password is valid if it is not contained in the UserPasswordHistory table which ought to be a rolling 6 passwords for the user
            string salt = (from u in opContext.UserReadOnly
                           where u.ID == userID && u.Active
                           select u.Salt).SingleOrDefault<string>();
            //Validate password
            string pwdHash = HashUtils.ComputeHash(password, hashAlgorithm, salt);

            //chose not to check user passwordHash field as well, only checking userPasswordHistory table
            var pwdCount = (from i in opContext.UserPasswordHistory
                            join u in opContext.UserReadOnly on i.fk_UserID equals u.ID
                            where i.fk_UserID == userID && u.Active
                            select i).OrderByDescending(a => a.InsertUTC).Take(passwordHistoryLimit).FirstOrDefault(a => a.PasswordHash == pwdHash);

            return pwdCount != null;
        }

        private bool SaveUserPasswordHistory(INH_OP opContext, User newUser)
        {
            UserPasswordHistory uph = new UserPasswordHistory();
            uph.PasswordHash = newUser.PasswordHash;
            uph.Salt = newUser.Salt;
            uph.InsertUTC = DateTime.UtcNow;
            uph.fk_UserID = newUser.ID;
            opContext.UserPasswordHistory.AddObject(uph);

            return opContext.SaveChanges() > 0;
        }

        void DisableUserAccountExternal(User user, INH_OP opContext)
        {
            if (!user.Active)
            {
                if (EnableDissociateCustomerUserSync)
                    MdmHelpers.SyncDissociateCustomerUserWithNextGen(_customerServiceApi, user, opContext);

                if (TriggerExternalUpdates)
                {
                }
            }
        }

        private void SendEmail(string firstName, string lastName, string email, string language, string emailVerificationGUID, string username, DateTime verificationUTC)
        {
            CultureInfo userLanguage = new CultureInfo(language);
            string customerName;

            using (INH_OP opContext = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                customerName = (from c in opContext.CustomerReadOnly
                                join u in opContext.UserReadOnly on c.ID equals u.fk_CustomerID
                                where u.EmailVerificationGUID == emailVerificationGUID
                                select c.Name).FirstOrDefault();
            }

            string verificationInfo = string.Format("{0};{1}", emailVerificationGUID, verificationUTC.KeyDate().ToString());
            string encrypted = EncryptionUtils.EncryptText(verificationInfo, key);

            string activationLink = string.Format("{0}{1}&lan={2}", emailVerificationURL, encrypted, language.Replace('-', '_'));
            string link = string.Format("<a href=\"{0}\">{1}</A>", activationLink, activationLink);
            if (!string.IsNullOrEmpty(email))
            {
                StringBuilder userBody = new StringBuilder();
                String subject = String.Format("{0}", VLResourceManager.GetString("VerifyUserEmailSubject", language));
                username = string.Format("\"{0}\"", username);
                customerName = string.Format("\"{0}\"", customerName);
                userBody.AppendLine(string.Format(VLResourceManager.GetString("VerifyUserEmailBody", userLanguage), firstName, lastName, link, username, customerName));
                API.Email.AddToQueue(mailFromVerifyEmail, email, subject, userBody.ToString(), false, false, "NH Web Services Verify Email");
            }
        }



        private void SendEmailForApiUser(INH_OP ctx, string firstName, string lastName, string email, string language, string emailVerificationGUID, string username, DateTime verificationUTC, long userID, long customerID)
        {
            CultureInfo userLanguage = new CultureInfo(language);

            string verificationInfo = string.Format("{0};{1}", emailVerificationGUID, verificationUTC.KeyDate().ToString());
            string encrypted = EncryptionUtils.EncryptText(verificationInfo, key);

            string listOfApi = ListOfApiAccess(ctx, userID, customerID);

            string activationLink = string.Format("{0}{1}&lan={2}", emailVerificationURL, encrypted, language.Replace('-', '_'));
            string link = string.Format("<a href=\"{0}\">{1}</A>", activationLink, activationLink);
            if (!string.IsNullOrEmpty(email))
            {
                StringBuilder userBody = new StringBuilder();
                String subject = String.Format("{0}", VLResourceManager.GetString("ApiUserEmailSubject", language));
                username = string.Format("\"{0}\"", username);
                userBody.AppendLine(string.Format(VLResourceManager.GetString("ApiUserEmailBody", userLanguage), firstName, lastName, link, username, listOfApi));
                API.Email.AddToQueue(mailFromVerifyEmail, email, subject, userBody.ToString(), false, false, "NH Web Services Verify Email");
            }
        }

        private string ListOfApiAccess(INH_OP ctx, long userID, long customerID)
        {
            var fullCustomerName = (from c in ctx.Customer where c.ID == customerID select c.Name).FirstOrDefault();
            var length = fullCustomerName.Trim().Length > 5 ? 5 : fullCustomerName.Trim().Length;
            var customerName = fullCustomerName.Trim().Substring(0, length);
            string listOfApiAccess = "<ul>{0}</ul>";
            string urls = string.Empty;
            var ApiUrl = (from usf in ctx.UserFeatureReadOnly
                          join fut in ctx.FeatureURLTemplateReadOnly
                          on usf.fk_Feature equals fut.fk_FeatureID
                          where usf.fk_User == userID
                          select fut).ToList();
            foreach (var api in ApiUrl)
            {
                string url;
                if (UserTypeHelper.ApiFeatureTypesWithCustomerNameInUrl.Contains(api.fk_FeatureID))
                    url = string.Format(api.URL, customerName);
                else
                    url = api.URL;
                string apiUrl = string.Format(" <li> {0} - {1} </li>", api.ApiTopics, url);
                urls = string.Join(" ", urls, apiUrl);
            }
            return string.Format(listOfApiAccess, urls);
        }

        private IEnumerable<User> GetUserEnumerable(INH_OP ctx, long customerID)
        {
            return (from user in ctx.User
                    where user.fk_CustomerID == customerID && user.Active == true
                    select user);
        }

        private enum VerifyEmailCode
        {
            FirstTimeClientUser = 0,
            ExistingClientUser = 1,
            ApiUser = 2,
            FirstTimeIdentityClientUser = 3,
            ExistingIdentityClientUser = 4,
            SupportUser = 5
        }

        #endregion

        #region VLTierSupportTool Methods

        public List<UserInfo> SearchUsers(string searchTerm)
        {
            List<UserInfo> searchResults = new List<UserInfo>();
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                searchResults = (from a in ctx.UserReadOnly
                                 where ((a.Name == searchTerm)
                                 || (a.Name.Contains(searchTerm))
                                 || (a.EmailContact == searchTerm)
                                 || (a.EmailContact.Contains(searchTerm)))
                                 select new UserInfo
                                 {
                                     FirstName = a.FirstName,
                                     LastName = a.LastName,
                                     Name = a.Name,
                                     ID = a.ID,
                                     EmailContact = a.EmailContact
                                 }
                                ).ToList();
            }
            return searchResults;
        }

        public UserDetails GetUserStatus(long userID)
        {
            UserDetails userStatusInfo = new UserDetails();
            using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
            {
                userStatusInfo = (from u in ctx.UserReadOnly

                                  let features = (from uf in ctx.UserFeatureReadOnly
                                                  join f in ctx.FeatureReadOnly on uf.fk_Feature equals f.ID
                                                  join fa in ctx.FeatureAccessReadOnly on uf.fk_FeatureAccess equals fa.ID
                                                  join ft in ctx.FeatureTypeReadOnly on f.fk_FeatureTypeID equals ft.ID
                                                  where uf.fk_User == userID
                                                  select new UserFeatureAndAccess
                                                  {
                                                      FeatureName = f.Name,
                                                      FeatureType = ft.Name,
                                                      FeatureAccess = fa.Name
                                                  }).ToList()

                                  let customers = (from usr in ctx.UserReadOnly
                                                   join c in ctx.CustomerReadOnly on usr.fk_CustomerID equals c.ID
                                                   where usr.EmailContact == u.EmailContact
                                                   select new CustomerDetails
                                                   {
                                                       CustomerName = c.Name,
                                                       UserName = usr.Name,
                                                       IsActive = usr.Active
                                                   }).ToList()

                                  let isClientUser = (from usf in ctx.UserFeatureReadOnly where usf.fk_User == u.ID && usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.Client select usf).Any()

                                  where u.ID == userID

                                  select new UserDetails
                                  {
                                      EmailVerificationUTC = u.EmailVerificationUTC,
                                      PasswordExpiredUTC = u.PwdExpiryUTC,
                                      IsEmailVerified = (u.IsEmailValidated && u.EmailVerificationUTC != null && u.EmailVerificationGUID != null),
                                      IsActive = u.Active,
                                      UserFeatures = features,
                                      CustomerList = customers,
                                      IsClientUser = isClientUser
                                  }).SingleOrDefault();
            }
            return userStatusInfo;
        }

        #endregion
    }
}
