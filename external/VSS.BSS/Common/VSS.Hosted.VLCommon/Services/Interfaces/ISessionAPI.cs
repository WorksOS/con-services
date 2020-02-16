
using System.Collections.Generic;
using VSS.Nighthawk.Instrumentation;

namespace VSS.Hosted.VLCommon
{
    public interface ISessionAPI
    {
        bool LoginCheck(string userName);
        SessionContext Login(string userName, string password, bool isSSO = false);
        SessionContext CreateTPaaSUserSession(string userUID, string customerUID);
        SessionContext SSOLogin(string username);
        SessionContext LoginForBusinessCenter(string username, string password);
        SessionContext LoginWithSessionID(string sessionID);
        SessionContext LoginWithKey(string key);
        SessionContext ImpersonatedLogin(SessionContext impersonatorContext, string userName);
        bool ResetPasswordWhenSessionNotCreated(string userName, string oldPassword, string newPassword);
        string GetUniqueID(SessionContext session);
        bool Logout(SessionContext session);
        SessionContext Validate(string sessionID);
        void AddClientMetrics(List<ClientMetric> records);
        bool PasswordIsValid(string clearTextPassword, string passwordHash, string salt);
        string GetPasswordHash(string userEnteredPassword, string userSalt);
        void InvalidateSessionContextCache(SessionContext session);
        string GetServerVersion();
        List<UserFeature> GetUserFeatureAccess(long userID);
        List<Language> GetSupportedLanguages();
        bool SaveUserTemporaryKey(SessionContext session, string temporaryKey);
        SessionContext GetUserSessionDetailsForNonVerifiedUser(string userName);
    }
}
