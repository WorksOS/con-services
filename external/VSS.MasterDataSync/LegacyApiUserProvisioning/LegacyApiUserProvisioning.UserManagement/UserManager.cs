using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using log4net;
using LegacyApiUserProvisioning.UserManagement.Interfaces;
using VSS.Hosted.VLCommon;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserManager : IUserManager
    {
        private readonly ILog _logger;
        private readonly INH_OP _nhOpContext;
        private const string HashAlgorithm = "SHA1";

        private static readonly int passwordHistoryLimit =
            Convert.ToInt32(ConfigurationManager.AppSettings["PasswordHistoryLimit"]);
        private static readonly double passwordExpiryDaysLimit = Convert.ToDouble(ConfigurationManager.AppSettings["PasswordExpiryDaysLimit"]);
        private static readonly string key = ConfigurationManager.AppSettings["VerifyUserEncryptionKey"];
        private static readonly string apiUserHelpURL = ConfigurationManager.AppSettings["APIUserHelpURL"];
        private static readonly string mailFromVerifyEmail = ConfigurationManager.AppSettings["MailFromVerifyEmail"];

        public UserManager(ILog logger, INH_OP nhOpContext)
        {
            _logger = logger;
            _nhOpContext = nhOpContext;
        }

        public IEnumerable<IUser> GetUsersByOrganization(string customerUid)
        {
            const string classMethod = "UserManager.GetUsersByOrganization";

            _logger.IfDebugFormat($"{classMethod} called");

            var isValid = Guid.TryParse(customerUid, out var customerGuid);
            if (!isValid) return null;

            try
            {
                var results = (from c in _nhOpContext.CustomerReadOnly
                               join u in _nhOpContext.UserReadOnly on c.ID equals u.fk_CustomerID
                               join uf in _nhOpContext.UserFeatureReadOnly on u.ID equals uf.fk_User
                               where c.CustomerUID == customerGuid && uf.fk_Feature >= 3000 && uf.fk_Feature < 4000 && u.Active
                               select new User
                               {
                                   Email = u.EmailContact,
                                   FirstName = u.FirstName,
                                   LastName = u.LastName,
                                   UserName = u.Name
                               }).Distinct().ToList();

                _logger.IfInfoFormat($"{classMethod} found {results.Count} api users");

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        public HttpStatusCode UpdateCustomerOfApiUsers(IMigrateUsersRequest migrateUsers)
        {
            const string classMethod = "UserManager.UpdateCustomerOfApiUsers";

            _logger.IfDebugFormat($"{classMethod} called");

            var isValid = Guid.TryParse(migrateUsers.CustomerUid, out var customerGuid);
            if (!isValid || migrateUsers.UserIds == null || !migrateUsers.UserIds.Any())
            {
                _logger.IfErrorFormat($"{classMethod} thrown exception with Invalid request");
                return HttpStatusCode.BadRequest;
            }

            var customer = _nhOpContext.CustomerReadOnly.SingleOrDefault(t => t.CustomerUID == customerGuid);
            if (customer == null)
            {
                _logger.IfErrorFormat($"{classMethod} Customer is not found for CustomerUID {customerGuid.ToString()}");
                return HttpStatusCode.BadRequest;
            }

            long customerId = customer.ID;
            try
            {
                foreach (var userName in migrateUsers.UserIds)
                {
                    var userToUpdate = _nhOpContext.User.Single(t => t.Name == userName);
                    userToUpdate.fk_CustomerID = customerId;
                    userToUpdate.UpdateUTC = DateTime.UtcNow;
                }

                _nhOpContext.SaveChanges();
                _logger.IfInfoFormat($"{classMethod} updated {customerId} for api users");
                return HttpStatusCode.Accepted;
            }
            catch (Exception ex)
            {
                _logger.IfErrorFormat($"{classMethod} thrown exception {ex.Message}");
                return HttpStatusCode.InternalServerError;
            }
        }

        public IEnumerable<IApiFeature> GetApiFeaturesByUserName(string userName)
        {
            const string classMethod = "UserManager.GetApiFeaturesByUserName";
            try
            {
                var result = (from uf in _nhOpContext.UserFeatureReadOnly
                              join u in _nhOpContext.UserReadOnly on uf.fk_User equals u.ID
                              join ft in _nhOpContext.FeatureReadOnly on uf.fk_Feature equals ft.ID
                              join a in _nhOpContext.FeatureAccessReadOnly on uf.fk_FeatureAccess equals a.ID
                              where u.Name == userName && u.Active
                              select new ApiFeatureDto
                              {
                                  Id = ft.ID,
                                  Name = ft.Name,
                                  Access = new FeatureAccessDto { ID = a.ID, Name = a.Name }
                              }
                    ).Distinct().ToList();

                _logger.IfInfoFormat($"{classMethod} user {userName} has features {result} ");

                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        public IUserCreationResponse CreateUser(IUserCreationRequest request)
        {
            var response = new UserCreationResponse();
            try
            {

                request.CustomerId = this.GetCustomerId(request.CustomerUid);
                var user = this.Create(
                    request.CustomerId,
                    request.UserName,
                    request.Password,
                    request.Email,
                    request.FirstName,
                    request.LastName,
                    request.Features,
                    false,
                    request.CreatedBy);

                response.User = user;


                response.Features = GetApiFeaturesByUserName(user.Name);
                return response;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is InvalidOperationException)
                {
                    response.Error = ex.Message;
                }
                else
                {
                    response.Error = "something went wrong";
                }

                _logger.Error(ex);
                return response;
            }
        }

        private long GetCustomerId(string customerUid)
        {
            var isValidGuid = Guid.TryParse(customerUid, out var customerGuid);

            if (!isValidGuid)
            {
                _logger.IfErrorFormat("customer uid is not valid");
                throw new InvalidOperationException("customer uid is not valid");
            }
            var customerId = (from c in _nhOpContext.CustomerReadOnly where c.CustomerUID == customerGuid select c.ID).FirstOrDefault();

            if (customerId == 0)
            {
                throw new InvalidOperationException("customer uid not found");
            }
            return customerId;
        }

        public IUserEditResponse EditUser(IUserEditRequest userEditRequest)
        {
            var response = new UserEditResponseDto();

            try
            {
                long? userId = (from u in _nhOpContext.UserReadOnly
                                where u.Name == userEditRequest.UserName
                                select u.ID).FirstOrDefault();

                if (userId == 0)
                {
                    response.Error = "User not found";
                    return response;
                }

                var features = ConvertFeatureIdsToUserFeatureAccesses(userEditRequest.Features);
                var isFeatureAccessUpdated = this.UpdateFeatureAccess((long)userId, features);

                if (!isFeatureAccessUpdated)
                {
                    _logger.Error("Save failed while updating user feature accesses");
                    response.Error = "Something went wrong";
                    return response;
                }

                if (!string.IsNullOrEmpty(userEditRequest.Password))
                {
                    var isPasswordUpdated = this.UpdatePassword((long)userId, userEditRequest.Password);
                    if (!isPasswordUpdated)
                    {
                        response.Error = "Password update failed";
                    }
                }


                response.Features = this.GetApiFeaturesByUserName(userEditRequest.UserName);

                return response;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                if (ex is InvalidOperationException)
                {
                    response.Error = ex.Message;
                }
                else
                {
                    response.Error = "Something went wrong";
                }

                return response;
            }
        }

        private bool PasswordSet(long userID, string newPassword, VSS.Hosted.VLCommon.User user)
        {
            var success = false;
            user.PasswordHash = HashUtils.ComputeHash(newPassword, HashAlgorithm, user.Salt);
            user.UpdateUTC = DateTime.UtcNow;


            if (_nhOpContext.SaveChanges() > 0)
            {
                var userPasswordHistories = (from i in _nhOpContext.UserPasswordHistory
                                             where i.fk_UserID == userID
                                             select i).OrderByDescending(a => a.InsertUTC).Take(passwordHistoryLimit);


                //If maximum reached then remove oldest one (FIFO)
                if ((userPasswordHistories != null) &&
                    (userPasswordHistories.Count<UserPasswordHistory>() >= passwordHistoryLimit))
                {
                    var oldest = (from r in userPasswordHistories
                                  orderby r.InsertUTC ascending
                                  select r).First();
                    //Delete a row in the 'UserPasswordHistory' table
                    _nhOpContext.UserPasswordHistory.DeleteObject(oldest);
                }

                success = SaveUserPasswordHistory(user);
            }

            return success;
        }

        public bool UpdatePassword(long userID, string newPassword, bool updationByAdmin = false)
        {
            VSS.Hosted.VLCommon.User user =
                (from u in _nhOpContext.User where u.ID == userID && u.Active select u).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user");

            if (!updationByAdmin)
                user.PwdExpiryUTC =
                    DateTime.UtcNow.AddDays(
                        passwordExpiryDaysLimit); //Set password expiry to 180 days from last set date 
            else
                user.PwdExpiryUTC = null;

            var federatedUser = (from fedUser in _nhOpContext.FederatedLogonInfo
                                 where fedUser.fk_UserID == userID
                                 select fedUser).FirstOrDefault();
            if (federatedUser != null)
                _nhOpContext.FederatedLogonInfo.DeleteObject(federatedUser);

            ValidatePassword(userID, newPassword);
            return PasswordSet(userID, newPassword, user);
        }

        private bool UpdateFeatureAccess(long userId, IEnumerable<UserFeatureAccess> features, string updatedBy = null)
        {
            var permissions = new List<string>();

            var user = (from u in _nhOpContext.UserReadOnly
                        where u.ID == userId && u.Active
                        select new { u.ID, u.Name }).SingleOrDefault();

            if (user == null)
                throw new InvalidOperationException("Cannot update deleted user");

            var uFeatures = (from uf in _nhOpContext.UserFeature
                             where uf.fk_User == userId
                             select uf).ToList();
            for (var i = uFeatures.Count - 1; i >= 0; i--)
                _nhOpContext.UserFeature.DeleteObject(uFeatures[i]);

            foreach (var ufa in features)
            {
                var featureId = ufa.FeatureApp == 0
                    ? (ufa.Feature == 0
                        ? (int)ufa.FeatureChild
                        : (int)ufa.Feature)
                    : (int)ufa.FeatureApp;
                var newUserFeature = new UserFeature
                {
                    fk_User = userId,
                    fk_Feature = featureId,
                    fk_FeatureAccess = (int)ufa.Access,
                    Createdby = updatedBy,
                    CreatedDate = DateTime.UtcNow
                };
                _nhOpContext.UserFeature.AddObject(newUserFeature);
                permissions.Add(((FeatureEnum)featureId).ToString());
            }

            var success = _nhOpContext.SaveChanges() > 0;
            if (!string.IsNullOrEmpty(updatedBy))
            {
                _logger.IfInfoFormat($"{updatedBy} has updated the  user {user.Name}" +
                                     $" at time {DateTime.Now} with  permission, {permissions}");
            }

            return success;
        }

        private VSS.Hosted.VLCommon.User Create(
            long customerId,
            string name,
            string password,
            string email,
            string firstName,
            string lastName,
            IEnumerable<int> features,
            bool validEmail = false,
            string createdBy = null)
        {
            const PressureUnitEnum pressureUnit = PressureUnitEnum.PSI;
            const TemperatureUnitEnum temperatureUnit = TemperatureUnitEnum.Fahrenheit;
            int? meterLabelPreferenceType = 1;
            const string timeZoneName = "Mountain Standard Time";
            const int languageId = 1;
            byte? assetLabelPreferenceType = 1;
            var globalId = Guid.NewGuid().ToString();

            var featureAccessList = ConvertFeatureIdsToUserFeatureAccesses(features);

            return this.Create(
                customerId: customerId,
                name: name,
                password: password,
                timeZoneName: timeZoneName,
                email: email,
                passwordExpiryUtc: null,
                languageId: languageId,
                units: null,
                locationDisplayTypeId: null,
                globalId: globalId,
                assetLabelPreferenceType: assetLabelPreferenceType,
                firstName: firstName,
                lastName: lastName,
                jobTitle: null,
                address: null,
                phone: null,
                features: featureAccessList,
                meterLabelPreferenceType: meterLabelPreferenceType,
                temperatureUnit: temperatureUnit,
                pressureUnit: pressureUnit,
                validEmail: validEmail,
                createdBy: createdBy
            );
        }

        private static List<UserFeatureAccess> ConvertFeatureIdsToUserFeatureAccesses(IEnumerable<int> features)
        {
            var featureAccessList = new List<UserFeatureAccess>();
            foreach (var feature in features)
            {
                var featureAccess = new UserFeatureAccess
                {
                    Feature = (FeatureEnum)feature,
                    FeatureApp = 0,
                    FeatureChild = (FeatureChildEnum)feature,
                    Access = FeatureAccessEnum.Full
                };
                featureAccessList.Add(featureAccess);
            }

            return featureAccessList;
        }

        private VSS.Hosted.VLCommon.User Create(
            long customerId,
            string name,
            string password,
            string timeZoneName,
            string email,
            DateTime? passwordExpiryUtc,
            int languageId,
            int? units,
            byte? locationDisplayTypeId,
            string globalId,
            byte? assetLabelPreferenceType,
            string firstName,
            string lastName,
            string jobTitle,
            string address,
            string phone,
            List<UserFeatureAccess> features,
            int? meterLabelPreferenceType,
            TemperatureUnitEnum temperatureUnit,
            PressureUnitEnum pressureUnit,
            bool validEmail = false,
            string createdBy = null)
        {
            var isApiUser = false;
            var newUser = new VSS.Hosted.VLCommon.User();


            foreach (var feature in features)
            {
                isApiUser = UserTypeHelper.VlApiFeatureTypes.Contains(feature.FeatureApp == 0
                    ? (feature.Feature == 0 ? (int)feature.FeatureChild : (int)feature.Feature)
                    : (int)feature.FeatureApp);
                if (isApiUser)
                    break;
            }


            if (!isApiUser) return null;

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("Username and password needs to be specified appropriately");

            if (UserNameExists(_nhOpContext, name))
                throw new InvalidOperationException("DuplicateUserName");

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

            var emailIdAlreadyExists = (from us in _nhOpContext.UserReadOnly
                                        join usf in _nhOpContext.UserFeatureReadOnly on us.ID equals usf.fk_User
                                        where us.EmailContact == email && us.Active && us.fk_CustomerID == customerId &&
                                              usf.Feature.fk_FeatureTypeID == (int)FeatureTypeEnum.API
                                        select us.Name).Any();


            if (emailIdAlreadyExists)
                throw new InvalidOperationException("duplicateEmail");

            newUser.Name = name;
            newUser.Salt = HashUtils.CreateSalt(5);
            newUser.PasswordHash = HashUtils.ComputeHash(password, HashAlgorithm, newUser.Salt);
            newUser.Createdby = createdBy;
            newUser.InsertUTC = DateTime.UtcNow;
            newUser.UpdateUTC = DateTime.UtcNow;
            newUser.GlobalID = globalId ?? Guid.NewGuid().ToString();
            newUser.FirstName = firstName;
            newUser.LastName = lastName;
            newUser.Active = true;
            newUser.LogOnFailedCount = 0;
            newUser.fk_CustomerID = customerId;
            newUser.fk_LanguageID = languageId > 0 ? languageId : 1; // if language is not set, use default of 1 (en_us)
            newUser.TimezoneName = string.IsNullOrEmpty(timeZoneName) ? "Mountain Standard Time" : timeZoneName;
            newUser.PwdExpiryUTC = passwordExpiryUtc;
            newUser.Units = units;
            newUser.LocationDisplayType = locationDisplayTypeId;
            newUser.AssetLabelPreferenceType = assetLabelPreferenceType ?? 1; //Default to 'Asset Name'
            newUser.JobTitle = jobTitle;
            newUser.Address = address;
            newUser.PhoneNumber = phone;
            newUser.MeterLabelPreferenceType = meterLabelPreferenceType;

            newUser.fk_TemperatureUnitID = temperatureUnit == TemperatureUnitEnum.None
                ? (int)TemperatureUnitEnum.Fahrenheit
                : (int)temperatureUnit; // default celsius -> changed to Fahrenheit
            newUser.fk_PressureUnitID =
                pressureUnit == PressureUnitEnum.None ? (int)PressureUnitEnum.PSI : (int)pressureUnit;

            newUser.EmailContact = email;
            newUser.IsEmailValidated = validEmail;

            newUser.EmailVerificationGUID = Guid.NewGuid().ToString();
            newUser.EmailVerificationUTC = DateTime.UtcNow;
            _nhOpContext.User.AddObject(newUser);
            if (_nhOpContext.SaveChanges() <= 0)
                throw new InvalidOperationException("Failed to save user");
            SaveUserFeatures(features, newUser, createdBy);
            if (!SaveUserPasswordHistory(newUser))
                throw new InvalidOperationException("Failed to save password history");

            var language = (from lang in _nhOpContext.LanguageReadOnly
                            where lang.ID == newUser.fk_LanguageID
                            select lang.ISOName).FirstOrDefault();

            ///todo send email
            if (!validEmail)
            {
                if (isApiUser)
                {
                    SendEmailForApiUser(newUser.FirstName, newUser.LastName,
                        newUser.EmailContact, language, newUser.EmailVerificationGUID, newUser.Name,
                        newUser.EmailVerificationUTC.Value, newUser.ID, newUser.fk_CustomerID.Value);
                }
            }

            return newUser;
        }

        private void SendEmailForApiUser(
            string firstName,
            string lastName,
            string email,
            string language,
            string emailVerificationGuid,
            string username,
            DateTime verificationUtc,
            long userId,
            long customerId)
        {

            var verificationInfo = $"{emailVerificationGuid};{verificationUtc.KeyDate().ToString()}";
            EncryptionUtils.EncryptText(verificationInfo, key);

            var listOfApi = ListOfApiAccess(userId, customerId);
            //remove password reset link
            //string activationLink = string.Format("{0}{1}&lan={2}", emailVerificationURL, encrypted, language.Replace('-', '_'));
            //string link = string.Format("<a href=\"{0}\">{1}</A>", activationLink, activationLink);
            var helpLink = string.IsNullOrEmpty(apiUserHelpURL)
                ? string.Empty
                : $"<a href=\"{apiUserHelpURL}\">{apiUserHelpURL}</a>";

            if (!string.IsNullOrEmpty(email))
            {
                var userBody = new StringBuilder();
                const string subject = "VisionLink API Access Assistance";
                const string emailBody =
                    "Dear {0} {1},<br><br> Welcome to VisionLink® API services.<br><br>Your VisionLink API user name is {2}.<br>You have access to the following API services:<br>  {3} <br> Please login to {4} to access schema, documentation and code examples.<br> <br><br>Note: If you did not request a new API user account or updates to your existing account, contact your Administrator.";
                username = $"\"{username}\"";

                _logger.IfInfoFormat("APIUSEREmailTemplate: {0} ", emailBody);

                userBody.AppendLine(string.Format(emailBody, firstName, lastName, username, listOfApi, helpLink));
                API.Email.AddToQueue(
                    mailFromVerifyEmail,
                    email,
                    subject,
                    userBody.ToString(),
                    false,
                    false,
                    "NH Web Services Verify Email");
            }
        }

        private string ListOfApiAccess(long userId, long customerId)
        {
            var fullCustomerName =
                (from c in _nhOpContext.Customer where c.ID == customerId select c.Name).FirstOrDefault();
            var length = fullCustomerName != null && fullCustomerName.Trim().Length > 5
                ? 5
                : fullCustomerName.Trim().Length;
            var customerName = fullCustomerName.Trim().Substring(0, length);
            const string listOfApiAccess = "<ul>{0}</ul>";
            var urls = string.Empty;
            var apiUrls = (from usf in _nhOpContext.UserFeatureReadOnly
                           join fut in _nhOpContext.FeatureURLTemplateReadOnly
                               on usf.fk_Feature equals fut.fk_FeatureID
                           where usf.fk_User == userId
                           select fut).ToList();

            foreach (var api in apiUrls)
            {
                var url = UserTypeHelper.ApiFeatureTypesWithCustomerNameInUrl.Contains(api.fk_FeatureID)
                    ? string.Format(api.URL, customerName)
                    : api.URL;
                var apiUrl = $" <li> {api.ApiTopics} - {url} </li>";
                urls = string.Join(" ", urls, apiUrl);
            }

            return string.Format(listOfApiAccess, urls);
        }

        private bool SaveUserPasswordHistory(VSS.Hosted.VLCommon.User newUser)
        {
            var uph = new UserPasswordHistory
            {
                PasswordHash = newUser.PasswordHash,
                Salt = newUser.Salt,
                InsertUTC = DateTime.UtcNow,
                fk_UserID = newUser.ID
            };
            _nhOpContext.UserPasswordHistory.AddObject(uph);

            return _nhOpContext.SaveChanges() > 0;
        }

        
        private void SaveUserFeatures(List<UserFeatureAccess> userFeatureAccesses, VSS.Hosted.VLCommon.User newUser,
            string createdBy)
        {
            var now = DateTime.UtcNow;
            if (userFeatureAccesses != null && userFeatureAccesses.Any())
            {
                var permissions = new List<string>();
                foreach (var featureAccess in userFeatureAccesses)
                {
                    var featureId = featureAccess.FeatureApp == 0
                        ? (featureAccess.Feature == 0 ? (int)featureAccess.FeatureChild : (int)featureAccess.Feature)
                        : (int)featureAccess.FeatureApp;

                    var uf = new UserFeature
                    {
                        fk_Feature = featureId,
                        fk_User = newUser.ID,
                        fk_FeatureAccess = (int)featureAccess.Access,
                        Createdby = createdBy,
                        CreatedDate = now
                    };
                    permissions.Add(((FeatureEnum)featureId).ToString());
                    _nhOpContext.UserFeature.AddObject(uf);
                }

                var userFeatures = string.Join(",", permissions);

                if (_nhOpContext.SaveChanges() < 1)
                {
                    throw new InvalidOperationException("Failed to add features to user");
                }

                if (!string.IsNullOrEmpty(createdBy))
                {
                    _logger.IfInfoFormat("{0} has created the new user {1} at time {2} with permission {3}.", createdBy,
                        newUser.Name, now, userFeatures);
                }
            }
        }

        private bool IsPreviousPassword(long userId, string password)
        {
            //the password is valid if it is not contained in the UserPasswordHistory table which ought to be a rolling 6 passwords for the user
            var salt = (from u in _nhOpContext.UserReadOnly
                        where u.ID == userId && u.Active
                        select u.Salt).SingleOrDefault<string>();
            //Validate password
            var pwdHash = HashUtils.ComputeHash(password, HashAlgorithm, salt);

            //chose not to check user passwordHash field as well, only checking userPasswordHistory table
            var pwdCount = (from i in _nhOpContext.UserPasswordHistory
                            join u in _nhOpContext.UserReadOnly on i.fk_UserID equals u.ID
                            where i.fk_UserID == userId && u.Active
                            select i).OrderByDescending(a => a.InsertUTC).Take(passwordHistoryLimit)
                .FirstOrDefault(a => a.PasswordHash == pwdHash);

            return pwdCount != null;
        }

        public void ValidatePassword(long userId, string password)
        {
            var username = (from u in _nhOpContext.UserReadOnly
                            where u.ID == userId && u.Active
                            select u.Name).SingleOrDefault<string>();
            ValidatePassword(password, username);

            if (IsPreviousPassword(userId, password))
                throw new InvalidOperationException("passwordMatchesHistoryError");
        }

        private static void ValidatePassword(string newPassword, string username)
        {
            if (newPassword == null) throw new ArgumentNullException(nameof(newPassword));
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullException(
                    "Invalid parameter password. Password can not be null or an empty string");

            if (newPassword.Length < 8)
                throw new ArgumentException("Invalid parameter password. Password can not be less than 8 characters");

            if (newPassword.Length > 150)
                throw new ArgumentException("Invalid parameter password. Password can not be more than 150 characters");

            if (!newPassword.Any(char.IsUpper))
                throw new ArgumentException(
                    "Invalid parameter password. Password must contain at least one uppercase letter");

            if (!newPassword.Any(char.IsLower))
                throw new ArgumentException(
                    "Invalid parameter password. Password must contain at least one lowercase letter");

            if (!newPassword.Any(char.IsNumber))
                throw new ArgumentException("Invalid parameter password. Password must contain at least one number");

            if (newPassword.IndexOfAny(new[] { '!', '@', '$', '#', '%', '^', '&', '*', '-', '+', '_' }) == -1)
                throw new ArgumentException(
                    "Invalid parameter password. Password must contain at least one the following symbols !@#$%^&*-+_");

            if (newPassword.Equals(username))
                throw new ArgumentException("Invalid parameter password. Password must must not be same as username");
        }


        public bool UserNameExists(INH_OP opContext, string username)
        {
            // Even deleted usernames cannot be reused
            var userIDs = from u in opContext.UserReadOnly
                          where u.Name == username
                          select u.ID;

            return userIDs.Any();
        }
    
        public IUserDeleteResponse DeleteUsers(IUserDeleteRequest userDeleteRequest)
        {
            var response = new UserDeleteResponseDto();
            try
            {
                var delete = (from x in _nhOpContext.User where userDeleteRequest.UserList.Contains(x.Name) select x);
                if (delete.Any() && delete.Count() == userDeleteRequest.UserList.Count)
                {
                    foreach (var user in delete)
                    {
                        user.Active = false;
                        user.UpdateUTC = DateTime.UtcNow;
                    }
                }
                else
                {
                    response.Error = "User not found";
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex);
                return response;
            }
            response.Success = _nhOpContext.SaveChanges() > 0 ;
            return response;
        }

    }
}