using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Security;
using log4net;

using VSS.Hosted.VLCommon;
using System.Collections.Generic;
using System.Threading;
using System.Data.Entity.Core.Objects;
using VSS.Nighthawk.Instrumentation;
using VSS.Hosted.VLCommon.Resources;

namespace VSS.Hosted.VLCommon
{
  /// <summary>
  /// Provides VisionLink authentication methods.
  /// </summary>
  internal class SessionAPI : ISessionAPI
  {
    private static readonly int verificationReminderDayLimit = Convert.ToInt32(ConfigurationManager.AppSettings["VerificationReminderDayLimit"]);
    private static readonly int verificationPendingDayLimit = Convert.ToInt32(ConfigurationManager.AppSettings["VerificationPendingDayLimit"]);
    private static readonly bool IsNewViewEnabled = (string.IsNullOrEmpty(ConfigurationManager.AppSettings["EnableNewView"])) ? false : Convert.ToBoolean(ConfigurationManager.AppSettings["EnableNewView"]);

    static readonly Func<NH_OP, long, IQueryable<long>> userIDQuery = CompiledQuery.Compile<NH_OP, long, IQueryable<long>>((ctx, userID)
      => from au in ctx.ActiveUserReadOnly
         where au.fk_UserID == userID
           && false == au.fk_ImpersonatedUserID.HasValue
           && false == au.Expired
         select au.fk_UserID);

    readonly List<int> _vlClientfeatureTypes = new List<int>
      {
        (int) FeatureEnum.NHAdmin,
        (int) FeatureEnum.NHWeb,
        (int)FeatureEnum.NHWebAdmin,
        (int) FeatureEnum.VLAdmin,
        (int) FeatureEnum.VLTier1Support};
    /// <summary>
    /// Returns true if the userName exists, is active, belongs to an activated customer,
    /// AND, more importantly, there is not an active session running for that user at the moment.
    /// Otherwise returns false.
    /// </summary>
    /// <param name="userName"></param>
    /// <returns></returns>
    public bool LoginCheck(string userName)
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        long? userID;
        long? customerID;
        string failureCode;
        if (!IsAuthenticLogon(ctx, userName, string.Empty, false, out userID, out customerID, out failureCode))
        {
          throw new UnauthorizedAccessException(failureCode);
        }

        bool isActiveSession = false;
        if (ctx is NH_OP)
        {
          isActiveSession = userIDQuery.Invoke(ctx as NH_OP, userID.Value).Count() == 1;
        }
        else
        {
          isActiveSession = (from au in ctx.ActiveUserReadOnly
                             where au.fk_UserID == userID.Value
                               && false == au.fk_ImpersonatedUserID.HasValue
                               && false == au.Expired
                             select au.fk_UserID).Count() > 0;
        }

        return !isActiveSession;
      }
    }

    /// <summary>
    /// VL User login method
    /// </summary>
    /// <param name="userName">Login username, unique within VisionLink</param>
    /// <param name="password">Clear text password</param>
    /// <returns>A SessionContext</returns>
    public SessionContext Login(string userName, string password, bool isSSO = false)
    {
      //Validate parameters
      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
      {
        throw new ArgumentNullException("Invalid login parameters");
      }
      return Authenticate(userName, password: password, isSSO: isSSO);
    }

    /// <summary>
    /// VL User Validatelogin method
    /// </summary>
    /// <param name="userName">Login username, unique within VisionLink</param>
    /// <param name="password">Clear text password</param>
    /// <returns>A SessionContext</returns>
    public SessionContext CreateTPaaSUserSession(string userUID, string customerUID)
    {
        //Validate parameters
        if (string.IsNullOrEmpty(userUID) || string.IsNullOrEmpty(customerUID))
        {
            throw new ArgumentNullException("Invalid login parameters");
        }
        return NGAuthentication(userUID, customerUID);
    }

    /// <summary>
    /// Single-signOn autentication point, for users who have already 'registered' as a single-sign on user
    /// with VisionLink.
    /// </summary>
    /// <param name="username">Login username, unique within VisionLink</param>
    /// <returns>A SessionContext</returns>
    public SessionContext SSOLogin(string username)
    {
      //Validate parameters
      if (string.IsNullOrEmpty(username))
      {
        throw new ArgumentNullException("Invalid login parameters");
      }

      return Authenticate(username, validatePassword: false, isSSO: true);
    }

    public bool ResetPasswordWhenSessionNotCreated(string userName, string oldPassword, string newPassword)
    {
      long? userID;
      string failureCode;
      bool success = false;

      if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
      {
        throw new ArgumentNullException("Invalid Password Change Parameters");
      }

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {

        bool isAuthentic = CheckPassword(ctx, userName, oldPassword,
            out userID, out failureCode);

        if (isAuthentic && userID.HasValue)
        {
          success = API.User.UpdatePassword(ctx, userID.Value, newPassword);
        }
      }

      return success;
    }

    private bool CheckPassword(INH_OP ctx, string userName, string password,
      out long? userID, out string failureCode)
    {
      User user = (from u in ctx.User
                   join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                   where userName == u.Name && u.Active && c.IsActivated
                   select u).SingleOrDefault();

      if (null == user)
      {
        log.IfInfoFormat("Authentication failure for '{0}'. Unknown or deactivated user", userName);
        throw new UnauthorizedAccessException(LoginFailureCode.invalidUser001.ToString());
      }

      userID = user.ID;
      failureCode = string.Empty;

      if (UserAccountIsLocked(user))
      {
        failureCode = LoginFailureCode.invalidUser002.ToString();
      }

      if (string.IsNullOrEmpty(failureCode) && !PasswordIsValid(password, user.PasswordHash, user.Salt))
      {
        failureCode = "incorrcetCurrentPassword";
      }

      if (!string.IsNullOrEmpty(failureCode))
      {
        throw new UnauthorizedAccessException(failureCode);
      }
      return string.IsNullOrEmpty(failureCode);
    }

    /// <summary>
    /// Login method for BC users.
    /// </summary>
    /// <param name="username">Login username, unique within VisionLink</param>
    /// <param name="password">Clear text password</param>
    /// <returns>A SessionContext</returns>
    public SessionContext LoginForBusinessCenter(string username, string password)
    {
      //Validate parameters
      if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
      {
        throw new ArgumentNullException("Invalid login parameters");
      }

      SessionContext context = null;
      string userUid = null;
      string customerUid = null;
      //Dirty hack for NG until BC implement TPaaS login
      bool ngLogin = username.Contains("@");
      if (ngLogin)
      {
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          var users = (from u in ctx.UserReadOnly
                       join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                       where u.EmailContact == username || u.Name == username
                       select new {u.Name, u.PasswordHash, u.Salt, u.UserUID, c.CustomerUID}).ToList();
          foreach (var user in users)
          {
            if (PasswordIsValid(password, user.PasswordHash, user.Salt))
            {
              username = user.Name;
              userUid = user.UserUID;
              customerUid = user.CustomerUID.HasValue ? user.CustomerUID.Value.ToString() : null;
              break;
            }
          }
        }
      }
      try
      {
        //Try CG login first
        context = Authenticate(username, password: password, useExistingSession: !LoginCheck(username));
      }
      catch (Exception ex)
      {
        if (!ngLogin || string.IsNullOrEmpty(userUid) || string.IsNullOrEmpty(customerUid))
          throw;

        //try NG login
        context = NGAuthentication(userUid, customerUid, !LoginCheck(username));
      }
      return context;
    }

    /// <summary>
    /// Login method for single-signOn.
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public SessionContext LoginWithSessionID(string sessionID)
    {
      return Validate(sessionID);
    }

    /// <summary>
    /// Login method for single-signOn for BC.
    /// </summary>
    /// <param name="key">The temporary login key from BC</param>
    /// <returns>A SessionContext</returns>
    public SessionContext LoginWithKey(string key)
    {
      const string errorMessage = "Invalid temporary key";

      if (string.IsNullOrEmpty(key))
        throw new SecurityException(errorMessage);

      SessionContext session = null;

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var activeUser = (from au in ctx.ActiveUser
                          where au.TemporaryLoginKey == key
          select au).SingleOrDefault();

        if (activeUser == null)
          throw new SecurityException(errorMessage);
 
        session = Validate(activeUser.SessionID);
        //Zap the key now it's been used
        activeUser.TemporaryLoginKey = null;
        ctx.SaveChanges();
        
        return session;
      }
    }

    /// <summary>
    /// Adds a record to the NHMetrics. CodeInstrumentationMetrics table.
    /// </summary>
    /// <param name="records"></param>
    /// <returns></returns>
    public void AddClientMetrics(List<ClientMetric> records)
    {
      MetricsRecorder.AddClientMetricRecords(records);
    }

    /// <summary>
    /// Login method for previously authenticated VL user to authenticate another user.
    /// 
    /// Valid cases are Dealers impersonating their account customers who have VL logins;
    /// and Trimble Operations team members impersonating ANY other VL user.
    /// </summary>
    /// <param name="impersonatorContext">SessionContext of impersonator.</param>
    /// <param name="userName">UserName of user being impersonated</param>
    /// <returns></returns>
    public SessionContext ImpersonatedLogin(SessionContext impersonatorContext, string userName)
    {
      // Better double-check that the impersonator still has a valid session
      SessionContext validSession = Validate(impersonatorContext.SessionID);

      // Impersonators session is ended here, now that it has served it's purpose.
      if (validSession.CustomerID == API.Customer.GetTrimbleOperationsCustomerID())
      {
        Logout(impersonatorContext);
      }

      return Authenticate(userName, validatePassword: false, impersonatorUserID: impersonatorContext.UserID);
    }

    /// <summary>
    /// Returns guid for a VisionLink user
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public string GetUniqueID(SessionContext session)
    {
      return (from u in session.NHOpContext.UserReadOnly
              where u.ID == session.UserID
              select u.GlobalID).SingleOrDefault<string>();
    }

    /// <summary>
    /// Ends the VisionLink session for the user.
    /// </summary>
    /// <param name="session"></param>
    /// <returns></returns>
    public bool Logout(SessionContext session)
    {
      SessionContextCache.Remove(session.SessionID);
      sessionIDCache.Remove(session.SessionID);

      ActiveUser expired = (from aus in session.NHOpContext.ActiveUser
                            where aus.ID == session.ActiveUserID && false == aus.Expired
                            select aus).FirstOrDefault();
      
      if (null != expired) 
      {
        //Updating last login UTC in User table
        DateTime currentDateTime = DateTime.UtcNow;
        User user = (from u in session.NHOpContext.User where u.ID == expired.fk_UserID select u).FirstOrDefault();
        user.LastLoginUTC = currentDateTime;
        expired.Expired = true;
        expired.LastActivityUTC = currentDateTime;
        session.NHOpContext.SaveChanges();
      }

      return true;
    }

  

    /// <summary>
    /// Saves the user temporary key.
    /// </summary>
    /// <param name="session">The session id.</param>
    /// <param name="temporaryKey">The temporary key to save.</param>
    /// <returns><c>true</c> if success, <c>false</c> otherwise.</returns>
      public bool SaveUserTemporaryKey(SessionContext session, string temporaryKey)
      {
          ActiveUser activeUser = (from aus in session.NHOpContext.ActiveUser
                                where aus.ID == session.ActiveUserID && false == aus.Expired
                                select aus).FirstOrDefault();

          if (null != activeUser)
          {
              activeUser.TemporaryLoginKey = temporaryKey;
              session.NHOpContext.SaveChanges();
              return true;
          }

          return false;
      }

    /// <summary>
    /// Creates and returns a SessionContext for a logged on user.
    /// Throws SecurityException for an invalid sessionID param or an expired sesssionID.
    /// 
    /// Does not restrict a user to having only one active session.
    /// </summary>
    /// <param name="sessionID"></param>
    /// <returns></returns>
    public SessionContext Validate(string sessionID)
    {
      if (string.IsNullOrEmpty(sessionID))
        throw new SecurityException("Invalid Session ID");

      SessionContext sesh = null;
      if (sessionIDCache.Get(sessionID) == null)
      {
        bool isValid = false;

        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          isValid = (from aus in ctx.ActiveUserReadOnly where aus.SessionID == sessionID && aus.Expired == false select 1).Count() == 1;

          //Since we are not locking the cache, handle concurrency problems ourselves
          try
          {
            sessionIDCache.Add(new CacheItem(sessionID, isValid),
                               new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.Add(sessionCacheLife) });
          }
          catch (ArgumentException ex)
          {
            if (ex.Message != "An item with the same key has already been added")
              throw ex;
          }
        }
      }

      if (!(bool)sessionIDCache.Get(sessionID))
        throw new SecurityException("Session expired", new IntentionallyThrownException());

      if (IsNewViewEnabled)
      {
          using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
          {
              bool isValidSession = (from aus in ctx.ActiveUser where aus.SessionID == sessionID select aus).Any();
              if (!isValidSession)
              {                  
                  if (SessionContextCache.Get(sessionID) != null)
                  {
                      SessionContextCache.Remove(sessionID);
                  }
                  if ((bool)sessionIDCache.Get(sessionID))
                  {
                      sessionIDCache.Remove(sessionID);
                  }
                  throw new SecurityException("Session expired", new IntentionallyThrownException());
              }
          }
      }

      // Load SessionContext from cache, if there. Else create it and cache it.
      sesh = (SessionContext)SessionContextCache.Get(sessionID);
      if (null == sesh)
      {
        // SessionContext not in the cache? Either this is the first time that a Validate call has hit this web server,
        // or the SessionContext has expired out of the cache and needs renewing.
        //
        // This is also used to detect that it is time to record activity on the ActiveUser to prevent its expiry within the DB
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {
          var info = (from aus in ctx.ActiveUser
                      let u = aus.fk_ImpersonatedUserID == null ? (from user in ctx.UserReadOnly where user.ID == aus.fk_UserID select user).FirstOrDefault() : 
                                                                  (from user in ctx.UserReadOnly where user.ID == aus.fk_ImpersonatedUserID select user).FirstOrDefault()
                      let m = (from up in ctx.UserPreferencesReadOnly where up.fk_UserID == aus.fk_UserID && "mapAPIProvider" == up.Key select up.ValueXML).FirstOrDefault()
                      join l in ctx.LanguageReadOnly on u.fk_LanguageID equals l.ID
                      join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                      where aus.SessionID == sessionID && false == aus.Expired
                      select new
                                {
                                  AU = aus,
                                  tempMP = m != null? m : c.MapAPIProvider,
                                  Sesh = new SessionContext
                                          {
                                            SessionID = sessionID,
                                            ActiveUserID = aus.ID,
                                            CustomerID = u.fk_CustomerID,
                                            CustomerName = c.Name,                                             
                                            CustomerTypeID = c.fk_CustomerTypeID,
                                            UserID = u.ID,
                                            UserName = u.Name,
                                            PasswordHash = u.PasswordHash, // pwdHash + Salt are useful for validating a pwd without incuring a DB read.
                                            UserSalt = u.Salt,
                                            UserFirstName = u.FirstName,
                                            UserLastName = u.LastName,
                                            UserPhone = u.PhoneNumber,
                                            UserEmail = u.EmailContact,
                                            UserUnits = u.Units,
                                            UserLanguage = l.ISOName,
                                            UserLanguageID = l.ID,
                                            UserTimeZone = u.TimezoneName,
                                            UserAssetLabelTypeID = u.AssetLabelPreferenceType,
                                            UserLocationDisplayType = u.LocationDisplayType,
                                            meterLabelPreferenceType = u.MeterLabelPreferenceType,
                                            TemperatureUnit = (TemperatureUnitEnum)u.fk_TemperatureUnitID,
                                            PressureUnit = (PressureUnitEnum)u.fk_PressureUnitID,
                                            PasswordExpiery = u.PwdExpiryUTC != null && u.PwdExpiryUTC.HasValue ? u.PwdExpiryUTC.Value : (DateTime?)null                                            
                                          }
                                }).SingleOrDefault();

          if (null == info) // Not expected, but this indicates a problem with the way the user was set up
            throw new SecurityException("Session expired");

          sesh = info.Sesh;
          sesh.lastLogin = info.AU.LastActivityUTC;
          if (info.tempMP != null)
            sesh.MapAPIProvider = info.tempMP.Trim().ToLower(); //Safe casting;
          else
            sesh.MapAPIProvider = ConfigurationManager.AppSettings["MapAPIProvider"] ?? "alk";           

          info.AU.LastActivityUTC = DateTime.UtcNow;
          ctx.SaveChanges();

          //Since we are not locking the cache, handle concurrency problems ourselves
          try
          {
            SessionContextCache.Add(new CacheItem(sessionID, sesh),
                                    new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.Add(sessionCacheLife) });
          }
          catch (ArgumentException ex)
          {
            if (ex.Message != "An item with the same key has already been added")
              throw ex;
          }
        }
      }

      log4net.ThreadContext.Properties["UserID"] = sesh.UserID.ToString(); // Comes in handy for providing a context to logging
      log4net.ThreadContext.Properties["SessionID"] = sessionID; // Comes in handy for providing a context to logging

      Thread.SetData(Thread.GetNamedDataSlot("SessionContext"), sesh.ToString()); // Initially added to make SessionContext available to the instrumentation metrics recorder
      Thread.SetData(Thread.GetNamedDataSlot("UserName"), sesh.UserName);
      Thread.SetData(Thread.GetNamedDataSlot("CustomerName"), sesh.CustomerName);
      Thread.SetData(Thread.GetNamedDataSlot("CustomerUsage_CustomerID"), sesh.CustomerID);

      return sesh;
    }

    public string GetPasswordHash(string userEnteredPassword, string userSalt)
    {
      return HashUtils.ComputeHash(userEnteredPassword, "SHA1", userSalt);
    }

    /// <summary>
    /// Validate the user password. 
    /// </summary>
    public bool PasswordIsValid(string clearTextPassword, string passwordHash, string salt)
    {
      string pwdHash = GetPasswordHash(clearTextPassword, salt);

      return passwordHash.Equals(pwdHash, StringComparison.InvariantCulture);
    }

    public void InvalidateSessionContextCache(SessionContext session)
    {
      if (SessionContextCache.Contains(session.SessionID))
        SessionContextCache.Remove(session.SessionID);
    }

    public List<UserFeature> GetUserFeatureAccess(long userID)
    {
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        var userFeatures = (from uf in opCtx.UserFeatureReadOnly 
                            where uf.fk_User == userID
                            select uf
                            ).ToList();
        return userFeatures;
      }
    }

    public SessionContext GetUserSessionDetailsForNonVerifiedUser(string userName)
    {
      if (string.IsNullOrEmpty(userName))
        throw new ArgumentNullException("Invalid User Name");

      User user = null;
      ActiveUser activeUser = null;      

      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        user = (from u in ctx.User
                     join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                     where u.Name == userName && u.Active && c.IsActivated
                          select u).SingleOrDefault();
                
        activeUser = (from aus in ctx.ActiveUserReadOnly
                      where aus.fk_UserID == user.ID && aus.Expired == false
                           select aus).FirstOrDefault();
      }

      if (activeUser == null)
          throw new InvalidOperationException("Invalid Active User", new IntentionallyThrownException());

      SessionContext session = Validate(activeUser.SessionID);
      session.IsVerificationReminder= (user.EmailVerificationUTC ==null && user.EmailVerificationGUID == null) ? true : false;
      session.IsVerificationPending = (user.IsEmailValidated == false && (user.EmailVerificationUTC != null && user.EmailVerificationGUID != null)) ? true : false;

      DateTime currentUTC = DateTime.UtcNow;
      TimeSpan timeDiff = new TimeSpan();

      if (user.EmailVerificationTrackingUTC != null)
      {        
        timeDiff = currentUTC.Subtract((DateTime)user.EmailVerificationTrackingUTC);
      }

      if (session.IsVerificationReminder)
      {
        if(user.EmailVerificationTrackingUTC == null)
        {
          //The value will be null during the first time of verification reminder popup when continue is not performed.
          session.VerificationRemainingDays = verificationReminderDayLimit;
        }
        else
        {
          Double vrDaysRemaining = verificationReminderDayLimit - timeDiff.TotalDays;
          session.VerificationRemainingDays = verificationRemainingNextDayRoundOff(vrDaysRemaining);
        }        
      }
      else if (session.IsVerificationPending)
      {
        Double vpDaysRemaining = verificationPendingDayLimit - timeDiff.TotalDays;
        session.VerificationRemainingDays = verificationRemainingNextDayRoundOff(vpDaysRemaining);        
      }
      else
      {
        throw new InvalidOperationException("EmailID of the user has been already verified", new IntentionallyThrownException());
      }

      return session;
    }

    #region Utilities
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    private MemoryCache SessionContextCache = new MemoryCache("SessionContextCache");  
    private MemoryCache sessionIDCache = new MemoryCache("sessionIDCache");
    private TimeSpan sessionCacheLife = TimeSpan.FromMinutes(15);

    private static string serverVersion = null;

    // Andy:  This failure code crap is terrible.  Don't be fooled by the enum, only string matching is used.
    // We need to make this a proper enum with nice names and remove string matching.
    // If these failure codes change or any are added, you'll need to update and regression test all 
    // dependent apps including Foreman and VLMobile.
    private enum LoginFailureCode
    {
      invalidUser001 = 0,    // Invalid Username or Password
      invalidUser002 = 1,    // Account Locked
      sessionInProgress = 2, // never used as far as I can tell
      invalidUser003 = 3,    // Reached Max Login Attempts - Means invalid username or password AND account locked - 
                             //   -  Also 003 is reused by Foreman app to indicate trimble ops login. Boo!!!
      // invalidUser004      // No Projects - defined only in Foreman app !?!
      passwordExpired,
      firstTimeLogin,
      emailNotVerified
    }

    private static int? numberOfLoginAttempts;
    private static int? retryDurationInMinutes;
    private static int? lockoutDurationInMinutes;

    /// <summary>
    /// Default values to consider when the actual values can't be read from config file
    /// </summary>
    private const int DefaultNumberOfLoginAttempts = 5;
    private const int DefaultRetryDurationInMinutes = 20;
    private const int DefaultLockoutDurationInMinutes = 30;
    private const string invalidUser ="Unknown or deactivated user";
    private const string lockedUser = "User account is locked";


    /// <summary>
    /// Max number of retries the user can do with in a given time frame
    /// </summary>
    public int? NumberOfLoginAttempts
    {
      get
      {
        if (!numberOfLoginAttempts.HasValue)
          numberOfLoginAttempts = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfLoginAttempts"]);

        //if the config value is negative or zero take default value
        if (numberOfLoginAttempts <= 0)
          return DefaultNumberOfLoginAttempts;

        return numberOfLoginAttempts;
      }
    }

    /// <summary>
    /// The time frame in which the user is allowed to give max retries
    /// </summary>
    public int? RetryDurationInMinutes
    {
      get
      {
        if (!retryDurationInMinutes.HasValue)
          retryDurationInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["RetryDurationInMinutes"]);

        //if the config value is negative or zero take default value
        if (retryDurationInMinutes <= 0)
          return DefaultRetryDurationInMinutes;

        return retryDurationInMinutes;
      }
    }

    /// <summary>
    /// How much time the account should be kept locked before allowing next login
    /// </summary>
    public int? LockoutDurationInMinutes
    {
      get
      {
        if (!lockoutDurationInMinutes.HasValue)
          lockoutDurationInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["LockoutDurationInMinutes"]);

        //if the config value is negative or zero take default value
        if (lockoutDurationInMinutes <= 0)
          return DefaultLockoutDurationInMinutes;

        return lockoutDurationInMinutes;
      }
    }

    /// <summary>
    /// Checks the user account lock status before letting the user login.
    /// </summary>
    /// <param name="user">user for which this check needs to be performed.</param>
    /// <returns></returns>
    private bool UserAccountIsLocked(User user)
    {
      //Check that the user didn't exceeded the max allowed number of login tries
      if (user.LogOnFailedCount >= NumberOfLoginAttempts)
      {
        TimeSpan timeSinceFirstFailedLogonAttempt = DateTime.UtcNow.Subtract((user.LogOnFirstFailedUTC ?? DateTime.UtcNow));

        //check the retry duration as the account is locked and
        if (timeSinceFirstFailedLogonAttempt.TotalMinutes <= RetryDurationInMinutes)
          return true;

        TimeSpan timeSinceLastFailedLogonAttempt = DateTime.UtcNow.Subtract((user.LogOnLastFailedUTC ?? DateTime.UtcNow));

        //check the lock out duration as the account is locked and
        if (timeSinceLastFailedLogonAttempt.TotalMinutes <= LockoutDurationInMinutes)
          return true;
      }

      return false;
    }

    /// <summary>
    /// If the password is not valid, increase the failed retries count by 1 and return an exception.
    /// This count is used by UserAccountIsLocked method to check the user account lock status.
    /// </summary>
    private void RecordLoginFailure(User user)
    {
      TimeSpan timeSinceFirstFailedLogonAttempt = DateTime.UtcNow.Subtract((user.LogOnFirstFailedUTC ?? DateTime.UtcNow));
      TimeSpan timeSinceLastFailedLogonAttempt = DateTime.UtcNow.Subtract((user.LogOnLastFailedUTC ?? DateTime.UtcNow));

      //increase the failed count by one or reset the count if user exceeded the retry duration/lock out period
      if (timeSinceFirstFailedLogonAttempt.TotalMinutes > RetryDurationInMinutes || timeSinceLastFailedLogonAttempt.TotalMinutes > LockoutDurationInMinutes)
      {
        user.LogOnFailedCount = 1;
        user.LogOnFirstFailedUTC = DateTime.UtcNow;
      }
      else
        user.LogOnFailedCount++;

      if (!user.LogOnFirstFailedUTC.HasValue)
        user.LogOnFirstFailedUTC = DateTime.UtcNow;

      user.LogOnLastFailedUTC = DateTime.UtcNow;
    }

    private void ResetLoginFailure(User user)
    {
      if (user.LogOnFailedCount > 0)
      {
        user.LogOnFailedCount = 0;
        user.LogOnLastFailedUTC = null;
        user.LogOnFirstFailedUTC = null;
      }
    }

    /// <summary>
    /// Creates and returns a SessionContext, to identify the user during this session.
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="password"></param>
    /// <param name="validatePassword">Optional parameter for impersonation logins</param>
    /// <param name="impersonatorSessionID">Optional parameter, provided by dealer-impersonator</param>
    /// <param name="useExistingSession">Optional parameter, true for BC logins where user can be logged in both through the browser and BC</param>
    /// <returns></returns>
    private SessionContext Authenticate(string userName, string password = "crackM!@3", bool validatePassword = true, 
      long? impersonatorUserID = null, bool useExistingSession = false,  bool isSSO = false)
    {
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        long? userID = null;
        long? customerID = null;
        string failureCode;
        bool isAuthentic = IsAuthenticLogon(ctx, userName, password, validatePassword, 
          out userID, out customerID, out failureCode);

        if (isAuthentic && userID.HasValue)
        {
          ActiveUser au = GetActiveUser(ctx, userID.Value, impersonatorUserID);

          if (!useExistingSession)
          {
            string sessionID = System.Guid.NewGuid().ToString("N");
            long auID = SaveActiveUser(ctx, ref au, userID.Value, sessionID, impersonatorUserID);
          }
          var isClientUser = (from uf in ctx.UserFeatureReadOnly
                              where uf.fk_User == userID && _vlClientfeatureTypes.Contains(uf.fk_Feature) && uf.fk_FeatureAccess != (int)FeatureAccessEnum.None
                              select uf.fk_User).Any();

          User validUser = (from u in ctx.User
                       join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                       where userName == u.Name && u.Active && c.IsActivated
                       select u).SingleOrDefault(); 

          if (validatePassword)
          {
            var passwordExpiery = (from user in ctx.User
              where user.ID == userID.Value
              select new
              {
                pwdExpirey = user.PwdExpiryUTC,
                timeZoneName = user.TimezoneName
              }).FirstOrDefault();
            if (string.IsNullOrEmpty(passwordExpiery.timeZoneName))
              throw new ArgumentNullException("Invalid time zone name");

            NamedTimeZone ntz = new NamedTimeZone(passwordExpiery.timeZoneName);
            DateTime userPreferenceDate = DateTime.UtcNow.AddMinutes(-ntz.BiasMinutes);

           
            if (isClientUser && !isSSO)
            {                        

              if (passwordExpiery.pwdExpirey.HasValue &&
                  passwordExpiery.pwdExpirey.Value.KeyDate() < userPreferenceDate.KeyDate())
              {
                log.IfInfoFormat("Password Expiered for '{0}'", userName);
                throw new UnauthorizedAccessException(LoginFailureCode.passwordExpired.ToString());
              }
              if (!passwordExpiery.pwdExpirey.HasValue)
              {
                log.IfInfoFormat("First Time login for '{0}'", userName);
                throw new UnauthorizedAccessException(LoginFailureCode.firstTimeLogin.ToString());
              }

              
            }
          }
          if (isClientUser && !isSSO && validatePassword)
          {
              if ((!validUser.IsEmailValidated) || (validUser.EmailVerificationUTC == null && validUser.EmailVerificationGUID == null))
              {
                  log.IfInfoFormat("Authentication failure for '{0}'. EmailID of the user has not been verified", userName);
                  throw new UnauthorizedAccessException(LoginFailureCode.emailNotVerified.ToString());
              }
          }
          return Validate(au.SessionID);
        }

        string reason = failureCode == LoginFailureCode.invalidUser001.ToString() ? "Wrong password." : "User account is locked.";
        log.IfInfoFormat("Authentication failure for '{0}'. {1}", userName, reason);
        throw new UnauthorizedAccessException(failureCode);
      }
    }

    private SessionContext NGAuthentication(string userUID, string customerUID, bool useExistingSession = false)    
    {
        using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
        {           
            string failureCode;
            long? userID = null;
            long? customerID = null;
            bool isAuthentic = ValidateUser(ctx, userUID, customerUID, out userID, out customerID, out failureCode);

            if (isAuthentic && userID.HasValue)
            {
                ActiveUser au = GetActiveUser(ctx, userID.Value, null);

              if (!useExistingSession)
              {
                string sessionID = System.Guid.NewGuid().ToString("N");
                long auID = SaveActiveUser(ctx, ref au, userID.Value, sessionID, null);
              }

              var isClientUser = (from uf in ctx.UserFeatureReadOnly
                                    where uf.fk_User == userID && _vlClientfeatureTypes.Contains(uf.fk_Feature) && uf.fk_FeatureAccess != (int)FeatureAccessEnum.None
                                    select uf.fk_User).Any();

                var timeZoneName = (from user in ctx.User
                                           where user.ID == userID.Value
                                           select user.TimezoneName ).FirstOrDefault();
                    if (!isClientUser)
                        throw new UnauthorizedAccessException("invalidAccess");

                    if (string.IsNullOrEmpty(timeZoneName))
                        throw new ArgumentNullException("Invalid time zone name");

                    NamedTimeZone ntz = new NamedTimeZone(timeZoneName);
                    DateTime userPreferenceDate = DateTime.UtcNow.AddMinutes(-ntz.BiasMinutes);

                return Validate(au.SessionID);
            }

            string reason = failureCode == LoginFailureCode.invalidUser001.ToString() ? invalidUser : lockedUser;
            log.IfInfoFormat("Authentication failure for '{0}'. {1}", userID, reason);
            throw new UnauthorizedAccessException(failureCode);
        }
    } 
    private bool IsAuthenticLogon(INH_OP ctx, string userName, string password, bool validatePassword, 
      out long? userID, out long? customerID, out string failureCode)
    {
      
      User user = (from u in ctx.User
                   join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                   where userName == u.Name && c.IsActivated
                   select u).SingleOrDefault();

      // Andy:  This failure code crap is terrible.  Don't be fooled by the enum, only string matching is used.
      // We need to make this a proper enum with nice names and remove string matching.
      // If these failure codes change or any are added, you'll need to update and regression test all 
      // dependent apps including Foreman and VLMobile.
      if (null == user)
      {
          log.IfInfoFormat("Authentication failure for '{0}'. Unknown or deactivated user", userName);
          throw new UnauthorizedAccessException(LoginFailureCode.invalidUser001.ToString());
      }
      if (!user.Active)
      {
          throw new UnauthorizedAccessException("InactiveUser", new IntentionallyThrownException());
      }
     
      userID = user.ID;
      customerID = user.fk_CustomerID;
      failureCode = string.Empty;

      if (validatePassword)
      {
        Guid guid;
        if (Guid.TryParse(userName, out guid))
            throw new UnauthorizedAccessException("GuidUserAccessDenied");

        if (UserAccountIsLocked(user))
        {
          failureCode = LoginFailureCode.invalidUser002.ToString();
        }
        bool passwordValid = PasswordIsValid(password, user.PasswordHash, user.Salt);
        if (string.IsNullOrEmpty(failureCode) && !passwordValid)
        {
          failureCode = LoginFailureCode.invalidUser001.ToString();
          RecordLoginFailure(user);         
          if (user.LogOnFailedCount == DefaultNumberOfLoginAttempts)
          {
           failureCode = LoginFailureCode.invalidUser003.ToString();        
          }         

        }

        if (passwordValid && string.IsNullOrEmpty(failureCode))
        {
          //Updating last login UTC column in User table.(US 30092)
          user.LastLoginUTC = DateTime.UtcNow;
          ctx.SaveChanges();
        }

        if (string.IsNullOrEmpty(failureCode))
        {
          ResetLoginFailure(user);
        }

        if (!string.IsNullOrEmpty(failureCode))
        {
          ctx.SaveChanges();
        }
      }

      return string.IsNullOrEmpty(failureCode);
    }

    private bool ValidateUser(INH_OP ctx, string userUID, string customerUID, 
    out long? userID, out long? customerID, out string failureCode)
    {

        User user = (from u in ctx.User
                     join c in ctx.CustomerReadOnly on u.fk_CustomerID equals c.ID
                     where userUID == u.UserUID && u.Active && c.IsActivated && c.CustomerUID.ToString() == customerUID
                     select u).SingleOrDefault();

        if (null == user)
        {
            log.IfInfoFormat("Authentication failure for '{0}'. Unknown or deactivated user", userUID);
            throw new UnauthorizedAccessException(LoginFailureCode.invalidUser001.ToString());
        }
        if (!user.Active)
        {
            throw new UnauthorizedAccessException("InactiveUser", new IntentionallyThrownException());
        }

        userID = user.ID;
        customerID = user.fk_CustomerID;
        failureCode = string.Empty;         

            if (UserAccountIsLocked(user))
            {
                failureCode = LoginFailureCode.invalidUser002.ToString();
            }
        
            if (string.IsNullOrEmpty(failureCode))
            {
                //Updating last login UTC column in User table.(US 30092)
                user.LastLoginUTC = DateTime.UtcNow;
                ctx.SaveChanges();                
                ResetLoginFailure(user);
            }

            if (!string.IsNullOrEmpty(failureCode))
            {
                ctx.SaveChanges();
            }
        

        return string.IsNullOrEmpty(failureCode);
    }
    
    private ActiveUser GetActiveUser(INH_OP ctx, long userID, long? impersonatorUserID)
    {
      ActiveUser au = null;

      if (impersonatorUserID.HasValue)
      {
        au = (from aus in ctx.ActiveUser
              where aus.fk_UserID == impersonatorUserID.Value
                      && aus.fk_ImpersonatedUserID == userID
              select aus).FirstOrDefault();
      }
      else
      {
        au = (from aus in ctx.ActiveUser
              where aus.fk_UserID == userID && false == aus.fk_ImpersonatedUserID.HasValue
              select aus).FirstOrDefault();
      }

      return au;
    }

    private long SaveActiveUser(INH_OP ctx, ref ActiveUser au, long userID, string newSessionID, long? impersonatorUserID)
    {
      if (null == au)
      {
        au = new ActiveUser
        {
          fk_UserID = impersonatorUserID ?? userID,
          SessionID = newSessionID,
          Expired = false,
          fk_ImpersonatedUserID = (impersonatorUserID.HasValue ? userID : (long?)null),
          InsertUTC = DateTime.UtcNow,
          LastActivityUTC = DateTime.UtcNow
        };
        ctx.ActiveUser.AddObject(au);
      }
      if (au.SessionID != null)
        sessionIDCache.Remove(au.SessionID);

      au.SessionID = newSessionID;
      au.Expired = false;

      ctx.SaveChanges();

      return au.ID;
    }

    public string GetServerVersion()
    {
      if (null == serverVersion)
      {
        Assembly dt = Assembly.Load("VSS.Hosted.VLCommon");

        if (null != dt)
          serverVersion = dt.GetName().Version.ToString();
      }

      return serverVersion;
    }

    public List<Language> GetSupportedLanguages() 
    {
      List<Language> languages = new List<Language>();
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
         languages = (from lang in ctx.LanguageReadOnly
                      where lang.ID > 0
                      orderby lang.DisplayOrder ascending
                      select lang).ToList();
      }
      
      return languages;      
    }

    private int verificationRemainingNextDayRoundOff(double remainingDays)
    {
      int remainingNextDays = 0;

      if ((remainingDays) > 0)
      {
        if (remainingDays % 1 == 0)
        {
          remainingNextDays = (int)Math.Floor(remainingDays);
        }
        else
        {
          remainingNextDays = (int)Math.Floor(remainingDays) + 1;
        }
      }

      return remainingNextDays; 
    }

    #endregion
  }
}
