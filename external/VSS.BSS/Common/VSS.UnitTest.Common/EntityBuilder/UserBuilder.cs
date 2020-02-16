using System;
using System.Collections.Generic;
using VSS.UnitTest.Common.Contexts;

using VSS.Hosted.VLCommon;
using UserFeature = VSS.Hosted.VLCommon.UserFeature;

namespace VSS.UnitTest.Common.EntityBuilder 
{
  public enum AssetLabelPreference
  {
    AssetName = 1,
    SerialNumber = 2
  }
  public class UserBuilder
  {
    private long _id = IdGen.GetId();
    private string _firstName = "FIRST_NAME";
    private string _lastName = "LAST_NAME";
    private string _email = "TEST_EMAIL@DOMAIN.COM";
    private string _jobTitle = "JOB_TITLE";
    private string _timezoneName = "Mountain Standard Time";
    private byte _locationDisplayTypeId;
    private AssetLabelPreference _assetLabelPreference = AssetLabelPreference.AssetName;   
    private int _units;
    private string _address = "TEST_ADDRESS";
    private string _phoneNumber = "TEST_PHONE_NUMBER";
    private string _globalId = Guid.NewGuid().ToString();
    private string _username = "TEST_USER_NAME_" + IdGen.GetId();
    private string _password = "TEST_PASSWORD";
    private string _passwordSalt = HashUtils.CreateSalt(5);
    private DateTime? _passwordExpirationUtc = DateTime.UtcNow.AddDays(1);
    private int _logOnFailedCount = 0;
    private bool _active = true;
    private DateTime _updateUtc = DateTime.UtcNow;
    private bool _validation = true;
    private DateTime? _lastLoginUtc = null;
    private DateTime? _identityMigrationUtc = null;

    private List<UserFeatureAccess> _userFeatureAccess = new List<UserFeatureAccess>();
    private bool _addUserFeatures = true;

    private Customer _customer;
    private Language _language;
    private bool _isVLLoginID = false;
    private string _userUid;
    private string _createdBy;

    public UserBuilder Id(long id)
    {
      _id = id;
      return this;
    }
    public UserBuilder FirstName(string firstName)
    {
      _firstName = firstName;
      return this;
    }
    public UserBuilder LastName(string lastName)
    {
      _lastName = lastName;
      return this;
    }
    public UserBuilder Email(string email)
    {
      _email = email;
      return this;
    }
    public UserBuilder JobTitle(string jobTitle)
    {
      _jobTitle = jobTitle;
      return this;
    }
    public UserBuilder Timezone(string timezoneName)
    {
      _timezoneName = timezoneName;
      return this;
    }
    public UserBuilder LocationDisplayTypeId(byte locationDisplayTypeId)
    {
      _locationDisplayTypeId = locationDisplayTypeId;
      return this;
    }
    public UserBuilder AssetLabelPreferenceType(AssetLabelPreference assetLabelPreference)
    {
      _assetLabelPreference = assetLabelPreference;
      return this;
    }    
    public UserBuilder Units(int units)
    {
      _units = units;
      return this;
    }
    public UserBuilder Address(string address)
    {
      _address = address;
      return this;
    }
    public UserBuilder PhoneNumber(string phoneNumber)
    {
      _phoneNumber = phoneNumber;
      return this;
    }
    public UserBuilder GlobalId(string globalId)
    {
      _globalId = globalId;
      return this;
    }
    public UserBuilder Username(string username)
    {
      _username = username;
      return this;
    }

    public UserBuilder EmailValidated(bool validation)
    {
      _validation = validation;
      return this;
    }
    public UserBuilder Password(string password)
    {
      _password = password;
      return this;
    }
    public UserBuilder PasswordExpirationUtc(DateTime? passwordExpirationUtc)
    {
      _passwordExpirationUtc = passwordExpirationUtc;
      return this;
    }
    public UserBuilder LogOnFailedCount(int logOnFailedCount)
    {
      _logOnFailedCount = logOnFailedCount;
      return this;
    }
    public UserBuilder Inactive()
    {
      _active = false;
      return this;
    }
    public UserBuilder UpdateUtc(DateTime updateUtc)
    {
      _updateUtc = updateUtc;
      return this;
    }
    public UserBuilder WithFeature(Func<UserFeatureBuilder, object> userFeature) 
    {
      var userFeatureBuilder = (UserFeatureBuilder)userFeature.Invoke(new UserFeatureBuilder(this));
      _userFeatureAccess.Add(userFeatureBuilder.Build());
      return this;
    }
    public UserBuilder WithNoFeatures()
    {
      _addUserFeatures = false;
      return this;
    }
    public UserBuilder ForCustomer(Customer customer)
    {
      _customer = customer;
      return this;
    }
    public UserBuilder WithLanguage(Language language)
    {
      _language = language;
      return this;
    }

    public UserBuilder IsVLLoginID(bool isVLLoginID)
    {
      _isVLLoginID = isVLLoginID;
      return this;
    }

    public UserBuilder UserUID(string userUid)
    {
      _userUid = userUid;
      return this;
    }

    public UserBuilder LastLoginUTC(DateTime lastLoginUTC)
    {
      _lastLoginUtc = lastLoginUTC;
      return this;
    }

    public UserBuilder CreatedBy(string createdBy)
    {
      _createdBy = createdBy;
      return this;
    }

    public UserBuilder IdentityMigrationUtc(DateTime identityMigrationUtc)
    {
      _identityMigrationUtc = identityMigrationUtc;
      return this;
    }

    public User Build()
    {
      var user = new User();

      user.ID = _id;
      user.FirstName = _firstName;
      user.LastName= _lastName;
      user.EmailContact = _email;
      user.JobTitle= _jobTitle;
      user.fk_LanguageID = _language == null ? 0 : _language.ID;
      user.TimezoneName = _timezoneName;
      user.LocationDisplayType = _locationDisplayTypeId;
      user.AssetLabelPreferenceType = (byte)_assetLabelPreference;
      user.Units = _units;
      user.Address = _address;
      user.PhoneNumber = _phoneNumber;
      user.GlobalID = _globalId;
      user.Name = _username;
      user.Salt = _passwordSalt;
      user.PasswordHash = HashUtils.ComputeHash(_password, "SHA1", _passwordSalt);
      user.PwdExpiryUTC = _passwordExpirationUtc;
      user.LogOnFailedCount = _logOnFailedCount;
      user.Active = _active;
      user.UpdateUTC= _updateUtc;
      user.fk_CustomerID = _customer.ID;
      user.IsEmailValidated = _validation;
      user.EmailVerificationGUID = new Guid().ToString();
      user.EmailVerificationUTC = DateTime.UtcNow;
      user.IsVLLoginID = _isVLLoginID;
      user.UserUID = _userUid;
      user.LastLoginUTC = _lastLoginUtc;
      user.Createdby = _createdBy;
      user.IdentityMigrationUTC = _identityMigrationUtc;

      if(_addUserFeatures)
      {
        if(_userFeatureAccess.Count == 0)
        {
          _userFeatureAccess.Add(new UserFeatureBuilder(this).Build());
        }
      }

      foreach (UserFeatureAccess featureAccess in _userFeatureAccess) 
      {
        int featureId = featureAccess.featureApp == 0 
                                     ? (featureAccess.feature == 0 ? (int)featureAccess.featureChild : (int)featureAccess.feature) 
                                     : (int)featureAccess.featureApp;

        UserFeature userFeature = new UserFeature {fk_Feature = featureId, fk_User = user.ID, fk_FeatureAccess = (int) featureAccess.access};
        ContextContainer.Current.OpContext.UserFeature.AddObject(userFeature);
      }

      //Add a row in the 'UserPasswordHistory' table
      UserPasswordHistory passwordHistory = new UserPasswordHistory();
      passwordHistory.PasswordHash = user.PasswordHash;
      passwordHistory.Salt = user.Salt;
      passwordHistory.InsertUTC = DateTime.UtcNow;
      passwordHistory.fk_UserID = user.ID;
      //ContextContainer.Current.OpContext.User.AddObject(user);
      ContextContainer.Current.OpContext.UserPasswordHistory.AddObject(passwordHistory);

      return user;
    }

    public User Save()
    {
      var user = Build();

      ContextContainer.Current.OpContext.User.AddObject(user);
      ContextContainer.Current.OpContext.SaveChanges();

      return user;
    }
  }

  public class UserFeatureBuilder
  {
    protected readonly UserBuilder _userBuilder;

    private FeatureAppEnum _app = FeatureAppEnum.NHWeb;
    private FeatureEnum _feature = FeatureEnum.NHWebAdmin;
    private FeatureChildEnum _featureChild = 0;
    private FeatureAccessEnum _featureAccess = FeatureAccessEnum.Full;

    public UserFeatureBuilder(UserBuilder userBuilder)
    {
      _userBuilder = userBuilder;
    }

    public UserFeatureBuilder App(FeatureAppEnum app)
    {
      _app = app;
      return this;
    }

    public UserFeatureBuilder Feature(FeatureEnum feature)
    {
      _feature = feature;
      return this;
    }
    public UserFeatureBuilder Child(FeatureChildEnum featureChild)
    {
      _featureChild = featureChild;
      return this;
    }
    public UserFeatureBuilder Access(FeatureAccessEnum featureAccess)
    {
      _featureAccess = featureAccess;
      return this;
    }

    public UserFeatureAccess Build()
    {
      var userFeatureAccess = new UserFeatureAccess();

      userFeatureAccess.featureApp = _app;
      userFeatureAccess.feature = _feature;
      userFeatureAccess.featureChild = _featureChild;
      userFeatureAccess.access = _featureAccess;

      return userFeatureAccess;
    }
  }
}
