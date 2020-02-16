using System;
using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.Types;

namespace VSS.Hosted.VLCommon
{
  public interface IUserAPI
  {
    User Create(INH_OP opContext,long customerID, string name, string password,
          string timeZoneName, string email, DateTime? passwordExpiryUTC,
          int languageID, int? units, byte? locationDisplayTypeID,
          string globalID, byte? assetLabelPreferenceType, string firstName, string lastName,
          string jobTitle, string address, string phone, List<UserFeatureAccess> features,
          int? meterLabelPreferenceType, TemperatureUnitEnum temperatureUnit, PressureUnitEnum pressureUnit, bool validEmail = false, string createdBy=null); // ajr14975

    bool Update(INH_OP opContext, long userID, bool? isActive = null, string name = null, string timeZoneName = null,
          DateTime? termsofUseAcceptedUTC = null, int? units = null, byte? locationDisplayTypeID = null, byte? assetLabelPreferenceType = null,
          string firstName = null, string lastName = null, string jobTitle = null, string address = null, string phone = null,
          string email = null, int? meterLabelPreferenceType = null,
          TemperatureUnitEnum temperatureUnit = TemperatureUnitEnum.None, PressureUnitEnum pressureUnit = PressureUnitEnum.None);

    bool UpdateUserProfile(INH_OP opContext, long userID,
       string firstName , string lastName , string email, string jobTitle = null, string address = null,
       string phone = null);

    bool Delete(INH_OP opContext, long userID);

    void DeleteFederatedLogonInfo(INH_OP opContext, long userID);

    void UpdateFederatedInfo(string catLoginId, string federatedValue);

    bool UserNameExists(INH_OP opContext, string username);

    string GetUniqueUserName(); 

    bool UpdatePassword(INH_OP opContext, long userID, string newPassword, bool updationByAdmin = false);
    
    bool UpdatePassword(string forgotPassordGUID, string newPassword);

    bool UpdateFeatureAccess(INH_OP opContext, long userID, IEnumerable<UserFeatureAccess> features,string updatedBy=null);

    bool CreateUpdateUserPreference(INH_OP opContext, long userID, Dictionary<string, string> preferences);

    Dictionary<string, string> GetUserPreferencesByKey(INH_OP opContext, long userID, string prefKey);
    string GetUserPreferenceValueByKey(INH_OP opContext, long userID, string prefKey, string valueKey);

    bool UpdateLanguage(INH_OP opContext, long userID, string languageISOName);

    bool ModifyUserForAccountReg(INH_OP opContext, string oldUserName, string newUserName, string password);

    bool CreateUpdateUserActivation(INH_OP opContext, long userID, int activationStatusID, DateTime? sentUTC, string sentTo);

    bool IsCorporateUser(INH_OP opContext, long? customerID);

    void ValidatePassword(INH_OP opContext, long userID, string password);

    UserLocationPreferenceEnum GetLocationPreference(INH_OP opContext, long userID);

    bool ValidateUser(string username, string password);
    bool HasFeatureAccess(long userID, FeatureEnum feature, FeatureAccessEnum featureAccess);

    bool CreatedUpdateFederationLogonInfo(INH_OP opContext, long userID, string federatedValue, string federatedEntity,string catLoginId=null);
    bool FederatedLogonExists(INH_OP ctx, string federatedEntity, string federatedValue);
    List<User> GetUserDetails(long customerID);
    int GetUserDetailsCount(long customerID);

    User GetCreatedUser(long userID);

    Tuple<string, int> VerifyEmail(INH_OP ctx, string verifyEmailGUID, bool isIdentityUserExists);

    bool ResendEmail(string username);

    bool EmailIDExists(INH_OP opContext, string emailID, string userName, long customerId);

    bool IsEmailAlreadyVerified(string username, INH_OP context);

    bool SkipEmailVerification(INH_OP opContext, string userName);

    bool EmailVerification(INH_OP opContext, string userName, string emailID, bool isVerify);

    User GetFirstAdminUser(long customerID);
    List<UserInfo> SearchUsers(string searchTerm);
    
    UserDetails GetUserStatus(long userID);

    User CreateSSOUser(INH_OP opContext, long customerID, string name, string password,
      string timeZoneName, string email, int languageID, int? units, byte? locationDisplayTypeID,
      string globalID, byte? assetLabelPreferenceType, string firstName, string lastName,
      string jobTitle, string address, string phone, List<UserFeatureAccess> features,
      int? meterLabelPreferenceType, TemperatureUnitEnum temperatureUnit, PressureUnitEnum pressureUnit,
      string createdBy = null);

    bool UpdateSSOUser(INH_OP opContext, long userID, string name, string firstName = null, string lastName = null,
      string jobTitle = null, string address = null, string phone = null, string email = null);

    void CreateUpdateFederatedLogonInfo(long userId, string cwsLoginId);

  }
}
