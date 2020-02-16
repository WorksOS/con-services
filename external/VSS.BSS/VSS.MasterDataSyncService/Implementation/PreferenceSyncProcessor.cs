using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Common;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;
using VSS.Nighthawk.MasterDataSync.Models;

namespace VSS.Nighthawk.MasterDataSync.Implementation
{
  internal class UserAndCustomerType : IComparable<UserAndCustomerType>
  {
    public User User { get; set; }
    public int CustomerType { get; set; }

    public int CompareTo(UserAndCustomerType other)
    {
      var ranking = new Dictionary<int, int>()
      {
        { (int)CustomerTypeEnum.Operations, 0 },
        { (int)CustomerTypeEnum.Corporate, 1 },
        { (int)CustomerTypeEnum.Dealer, 2 },
        { (int)CustomerTypeEnum.Customer, 3 },
        { (int)CustomerTypeEnum.Account, 4 }
      };

      return ranking[this.CustomerType].CompareTo(ranking[other.CustomerType]);
    }
  }

  public class PreferenceSyncProcessor : SyncProcessorBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly Uri PreferenceApiEndpointUri;
    private readonly string _taskName;
    private readonly IHttpRequestWrapper _httpRequestWrapper;
    private readonly IConfigurationManager _configurationManager;

    public PreferenceSyncProcessor(string taskName, IHttpRequestWrapper httpRequestWrapper, IConfigurationManager configurationManager, ICacheManager cacheManager, ITpassAuthorizationManager tpassAuthorizationManager)
      : base(configurationManager, httpRequestWrapper, cacheManager, tpassAuthorizationManager)
    {
      _taskName = taskName;
      _httpRequestWrapper = httpRequestWrapper;
      _configurationManager = configurationManager;

      if (string.IsNullOrWhiteSpace(configurationManager.GetAppSetting("PreferenceService.WebAPIURI")))
        throw new ArgumentNullException("PreferenceService.WebAPIURI", "Preference api URL value cannot be empty");

      PreferenceApiEndpointUri = new Uri(_configurationManager.GetAppSetting("PreferenceService.WebAPIURI") + "/user");
    }


    public override bool Process(ref bool isServiceStopped)
    {
      bool isDataProcessed = false;
      if (LockTaskState(_taskName, _taskTimeOutInterval))
      {
        isDataProcessed = ProcessSync(ref isServiceStopped);
        UnLockTaskState(_taskName);
      }
      return isDataProcessed;
    }

    public override bool ProcessSync(ref bool isServiceStopped)
    {
      //MasterData Insertion
      var lastProcessedId = GetLastProcessedId(_taskName);
      var lastInsertUtc = GetLastInsertUTC(_taskName);
      var saveLastUpdateUtcFlag = GetLastUpdateUTC(_taskName) == null;
      var isCreateEventProcessed = ProcessInsertionRecords(lastProcessedId, lastInsertUtc, saveLastUpdateUtcFlag, ref isServiceStopped);


      lastInsertUtc = GetLastInsertUTC(_taskName);
      var lastUpdateUtc = GetLastUpdateUTC(_taskName);
      var isUpdateEventProcessed = ProcessUpdationRecords(lastProcessedId, lastInsertUtc, lastUpdateUtc, ref isServiceStopped);
      return (isCreateEventProcessed || isUpdateEventProcessed);
    }

    private bool ProcessInsertionRecords(long? lastProcessedId, DateTime? lastInsertUtc, bool saveLastUpdateUtcFlag, ref bool isServiceStopped)
    {
      var currentUtc = DateTime.UtcNow;
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          lastProcessedId = lastProcessedId ?? Int32.MinValue;

          Log.IfInfo(string.Format(
            "Started Processing CreateUserPreferenceEvent. LastProcessedId : {0} , LastInsertedUTC : {1}", lastProcessedId,
            lastInsertUtc));
          var users =
            (from user in opCtx.UserReadOnly
             where user.UserUID != null && user.Active && (user.IdentityMigrationUTC > lastInsertUtc || (user.IdentityMigrationUTC == lastInsertUtc && user.ID > lastProcessedId))
             select user)
            .OrderBy(n => n.IdentityMigrationUTC).ThenBy(n => n.ID).Take(BatchSize).ToList();

          if (users.Count < 1)
          {
            Log.IfInfo(string.Format("No {0} data left for creation event", _taskName));
            return false;
          }
          var languageDictionary = GetLanguageDictionary(opCtx);
          var requestHeader = GetRequestHeaderDictionaryWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.ContainsKey(StringConstants.InvalidKey) ||
              requestHeader.ContainsKey(StringConstants.InvalidValue))
          {
            return true;
          }

          foreach (var usr in users)
          {
            //get list having same userUID
            var siblings = (from other in opCtx.UserReadOnly
                            join cust in opCtx.Customer on other.fk_CustomerID equals cust.ID
                            where other.UserUID == usr.UserUID && other.Active
                            select new UserAndCustomerType() { User = other, CustomerType = cust.fk_CustomerTypeID }).ToList();

            if (siblings.Any(x => x.User.IdentityMigrationUTC <= lastInsertUtc)) //already processed
              continue;

            //rank by ops,corporate, dealer, customer
            siblings.Sort();
            UserAndCustomerType userCustomer = siblings.FirstOrDefault();

            requestHeader["X-VisionLink-UserUid"] = userCustomer.User.UserUID;
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            var prefDetails = GetPreferenceEventForUser(opCtx, userCustomer.User, languageDictionary);

            string payload = JsonConvert.SerializeObject(prefDetails, Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var wrapper = new UserPreferenceWrapperEvent()
            {
              PreferenceJson = payload,
              ActionUtc = DateTime.UtcNow
            };

            var svcResponse = ProcessServiceRequestAndResponse(wrapper, _httpRequestWrapper, PreferenceApiEndpointUri,
                requestHeader.ToList(), HttpMethod.Post);
            Log.IfInfo("Create preference "+usr.ID+ " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastProcessedId = usr.ID;
                lastInsertUtc = usr.IdentityMigrationUTC;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderDictionaryWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(prefDetails, _httpRequestWrapper, PreferenceApiEndpointUri,
                  requestHeader.ToList(), HttpMethod.Post);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                {
                  lastProcessedId = usr.ID;
                  lastInsertUtc = usr.IdentityMigrationUTC;
                }
                break;

              case HttpStatusCode.InternalServerError:
                string err = svcResponse.Content.ReadAsStringAsync().Result;
                Log.IfError("Internal server error: " + err);
                return true;

              case HttpStatusCode.BadRequest:
                string modelError = svcResponse.Content.ReadAsStringAsync().Result;
                Log.IfError("Skipping record as we got an error in payload: " + prefDetails + " Received model error: " + modelError);
                lastProcessedId = usr.ID;
                lastInsertUtc = usr.IdentityMigrationUTC;
                break;

              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;

              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. ID = {1}", svcResponse.StatusCode, usr.ID));
                return true;
            }
          }
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} Creation Event {1} \n {2}", _taskName, e.Message,
            e.StackTrace));
        }
        finally
        {
          //Saving last update utc if it is not set
          if (saveLastUpdateUtcFlag)
          {
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = currentUtc;
            opCtx.SaveChanges();
          }
          if (lastInsertUtc != default(DateTime).AddYears(1900))
          {
            //Update the last read utc to masterdatasync
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastProcessedID = lastProcessedId;
            opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastInsertedUTC = lastInsertUtc;
            opCtx.SaveChanges();
            Log.IfInfo(
              string.Format("Completed Processing CreateUserPreference. LastProcessedId : {0} , LastInsertedUTC : {1}",
                lastProcessedId, lastInsertUtc));
          }
          else
          {
            Log.IfInfo(string.Format("No Records Processed "));
          }
        }
      }
      return true;
    }

    private bool ProcessUpdationRecords(long? lastProcessedId, DateTime? lastInsertUtc, DateTime? lastUpdateUtc, ref bool isServiceStopped)
    {
      Log.IfInfo(string.Format("Started Processing Update Preference Event. LastProcessedId : {0} , LastInsertedUTC : {1},LastUpdatedUTC : {2}", lastProcessedId, lastInsertUtc, lastUpdateUtc));
      using (var opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        try
        {
          var currentUtc = DateTime.UtcNow;
         
          int toleranceLimit = 10;

          var usrs =
           (from user in opCtx.UserReadOnly
            where user.UserUID != null && user.Active && (user.IdentityMigrationUTC <= lastInsertUtc || (user.IdentityMigrationUTC == lastInsertUtc && user.ID <= lastProcessedId))
            let maxPreferenceUpdateUTC =
            (from up in opCtx.UserPreferencesReadOnly.Where(x => x.fk_UserID == user.ID).GroupBy(x => x.fk_UserID).
             Select(g => g.Max(item => item.UpdateUTC))
             select up).FirstOrDefault()
            let maxUpdate = new[] { user.UpdateUTC, maxPreferenceUpdateUTC }.Max()

            where (maxUpdate <= currentUtc && maxUpdate > lastUpdateUtc)
            orderby maxUpdate, user.ID
            select new { user, maxUpdate })
           .Take(BatchSize + toleranceLimit).ToList();
          int brkpt = usrs.Count - 1;
          int officialBatchCount = Math.Min(BatchSize, usrs.Count);
          for (int i = officialBatchCount - 1 ; i < usrs.Count - 1; ++i)
          {
            if (i < 0) break;
            if (usrs[i].maxUpdate != usrs[i + 1].maxUpdate)
              brkpt = i;
          }

          var users = usrs.Take(brkpt + 1);
            //.Take(BatchSize).ToDictionary(x => x.ID, y => y);

          //var ups =
          //  (from up in opCtx.UserPreferencesReadOnly
          //   join user in opCtx.UserReadOnly on up.ID equals user.ID
          //   where up.UpdateUTC <= currentUtc && up.UpdateUTC > lastUpdateUtc
          //   && user.UserUID != null && user.Active && (user.IdentityMigrationUTC <= lastInsertUtc || (user.IdentityMigrationUTC == lastInsertUtc && user.ID <= lastProcessedId))
          //   select new { up, user }).Take(BatchSize).ToDictionary(x => x.user.ID, y => new Tuple<User, UserPreferences>(y.user, y.up));

          //var users = new Dictionary<long, Tuple<User, UserPreferences>>();


          //foreach(var usr in usrs)
          //{
          //  users[usr.Key] = new Tuple<User, UserPreferences>(usr.Value, null);
          //}

          //foreach(var u in ups)
          //{
          //  users[u.Key] = u.Value;
          //}




          if (!users.Any())
          {
            lastUpdateUtc = currentUtc;
            Log.IfInfo(string.Format("Current UTC : {0} Updated, No {1} data to left for updation", currentUtc, _taskName));
            return false;
          }
          //todo get using userpreference as well
          var languageDictionary = GetLanguageDictionary(opCtx);
          Dictionary<string, string> requestHeader = GetRequestHeaderDictionaryWithAuthenticationType(isOuthRetryCall: false);

          if (requestHeader.ContainsKey(StringConstants.InvalidKey) ||
              requestHeader.ContainsKey(StringConstants.InvalidValue))
          {
            return true;
          }

          foreach (var user in users)
          {
            var usr = user.user;
            //var usr = user.Value.Item1;                       
            
            requestHeader["X-VisionLink-UserUid"] = usr.UserUID;
            if (isServiceStopped)
            {
              Log.IfInfo("MasterDataSync service is stopping.. Hence saving the last processed state");
              break;
            }
            //var prefDetails = user.Value.Item2 ?? GetCreatePreferenceEventForUser(opCtx, userCustomer.User, languageDictionary);
            var prefDetails = GetPreferenceEventForUser(opCtx, usr, languageDictionary);
            string payload = JsonConvert.SerializeObject(prefDetails, Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var wrapper = new UserPreferenceWrapperEvent()
            {
              PreferenceJson = payload,
              ActionUtc = DateTime.UtcNow
            };

            var svcResponse = ProcessServiceRequestAndResponse(wrapper, _httpRequestWrapper, PreferenceApiEndpointUri,
                requestHeader.ToList(), HttpMethod.Put);
            Log.IfInfo("Update preference "+usr.ID+ " returned " + svcResponse.StatusCode);
            switch (svcResponse.StatusCode)
            {
              case HttpStatusCode.OK:
                lastUpdateUtc = user.maxUpdate;
                break;

              case HttpStatusCode.Unauthorized:
                requestHeader = GetRequestHeaderDictionaryWithAuthenticationType(isOuthRetryCall: true);
                svcResponse = ProcessServiceRequestAndResponse(wrapper, _httpRequestWrapper, PreferenceApiEndpointUri,
                  requestHeader.ToList(), HttpMethod.Put);
                if (svcResponse.StatusCode == HttpStatusCode.OK)
                  lastUpdateUtc = user.maxUpdate;
                
                break;

              case HttpStatusCode.InternalServerError:
                string err = svcResponse.Content.ReadAsStringAsync().Result;
                Log.IfError("Internal server error: " + err);
                return true;

              case HttpStatusCode.BadRequest:
                string modelError = svcResponse.Content.ReadAsStringAsync().Result;
                Log.IfError("Skipping record as we got an error in payload: " + prefDetails + " Received model error: " + modelError);
                lastUpdateUtc = user.maxUpdate;
                break;

              case HttpStatusCode.Forbidden:
                Log.IfError("Forbidden status code received while hitting Tpaas preference service");
                break;

              default:
                Log.IfError(string.Format("StatusCode : {0} Failed to process data. ID = {1}", svcResponse.StatusCode, usr.ID));
                return true;
            }
          }


          
        }
        catch (Exception e)
        {
          Log.IfError(string.Format("Exception in processing {0} updation event {1} \n {2}", _taskName, e.Message, e.StackTrace));
        }
        finally
        {
          //Update the last read utc to masterdatasync
          opCtx.MasterDataSync.Single(t => t.TaskName == _taskName).LastUpdatedUTC = lastUpdateUtc;
          opCtx.SaveChanges();
          Log.IfInfo(string.Format("Completed Processing Update PreferenceEvent. LastProcessedId : {0} , LastInsertUTC : {1} LastUpdateUTC : {2}", lastProcessedId, lastInsertUtc, lastUpdateUtc));
        }
      }
      return true;
    }
    public override ServiceResponseMessage ProcessServiceRequestAndResponse<T>(T requestData, IHttpRequestWrapper _httpRequestWrapper, Uri requestUri, List<KeyValuePair<string, string>> requestHeaders, HttpMethod requestMethod)
    {
      string wrapperString = JsonConvert.SerializeObject(requestData, Newtonsoft.Json.Formatting.None,
  new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

      ServiceRequestMessage svcRequestMessage = new ServiceRequestMessage
      {
        RequestContentType = StringConstants.JsonContentType,
        RequestEncoding = Encoding.UTF8,
        RequestMethod = requestMethod,
        RequestPayload = wrapperString,
        RequestUrl = requestUri,
        RequestHeaders = requestHeaders
      };

      return _httpRequestWrapper.RequestDispatcher(svcRequestMessage);
    }

    private Dictionary<int, string> GetLanguageDictionary(INH_OP opCtx)
    {
      var langDict = (from lang in opCtx.LanguageReadOnly
                      select new { lang.ID, lang.ISOName }).ToDictionary(x => x.ID, y => y.ISOName);
      return langDict;
    }

    private UserPreferenceEvent GetPreferenceEventForUser(INH_OP ctx, User user, Dictionary<int, string> languageDictionary)
    {
      int languageId = user.fk_LanguageID;
      if (user.fk_LanguageID == 0 || !languageDictionary.ContainsKey(user.fk_LanguageID))
        languageId = 1; //default to en-US
      string language = languageDictionary[languageId];
      TemperatureUnitEnum temperatureType = (TemperatureUnitEnum)user.fk_TemperatureUnitID;

      int? meterLabelPrefType = user.MeterLabelPreferenceType;
      byte? assetLabel = user.AssetLabelPreferenceType;
      byte? locationDisplayType = user.LocationDisplayType;
      int? units = user.Units;

      string pressureUnit;
      switch ((PressureUnitEnum)user.fk_PressureUnitID)
      {
        case PressureUnitEnum.BAR:
          pressureUnit = "BAR";
          break;

        case PressureUnitEnum.kPa:
          pressureUnit = "kPa";
          break;

        default:
          pressureUnit = "PSI";
          break;
      }

      AssetLabelPreferenceTypeEnum assetLabelType = assetLabel.HasValue
        ? (AssetLabelPreferenceTypeEnum)assetLabel.Value
        : AssetLabelPreferenceTypeEnum.None;

      string assetLabelDisplay;
      switch (assetLabelType)
      {
        case AssetLabelPreferenceTypeEnum.Both:
          assetLabelDisplay = "Both";
          break;

        case AssetLabelPreferenceTypeEnum.SerialNumber:
          assetLabelDisplay = "SN";
          break;

        default:
          assetLabelDisplay = "Asset ID";
          break;
      }

      LocationDisplayTypeEnum locationDisplayTypeEnum = locationDisplayType.HasValue
        ? (LocationDisplayTypeEnum)locationDisplayType.Value
        : LocationDisplayTypeEnum.None;
      string locationDisplay;
      switch (locationDisplayTypeEnum)
      {
        case LocationDisplayTypeEnum.Address:
          locationDisplay = "Address";
          break;

        case LocationDisplayTypeEnum.LatLong:
          locationDisplay = "Lat/Lon";
          break;

        default:
          locationDisplay = "Site";
          break;
      }

      UnitsTypeEnum unitsTypeEnum = units.HasValue ? (UnitsTypeEnum)units.Value : UnitsTypeEnum.None;
      string unitsString;
      switch (unitsTypeEnum)
      {
        case UnitsTypeEnum.Imperial:
          unitsString = "Imperial";
          break;

        case UnitsTypeEnum.Metric:
          unitsString = "Metric";
          break;

        default:
          unitsString = "US Standard";
          break;
      }
      var evnt = new UserPreferenceEvent()
      {
        TemperatureUnit = temperatureType == TemperatureUnitEnum.Fahrenheit ? "Fahrenheit" : "Celsius",
        PressureUnit = pressureUnit,
        Language = language,
        MeterLabelDisplay = (meterLabelPrefType.HasValue && meterLabelPrefType.Value == 2) ? "odometer" : "hourmeter",
        AssetLabelDisplay = assetLabelDisplay,
        LocationDisplay = locationDisplay,
        Timezone = user.TimezoneName,
        Units = unitsString
      };

      FillUserPreferenceDetailsFromUserPreferenceTable(evnt,user.ID, ctx);

      return evnt;
    }

    private void FillUserPreferenceDetailsFromUserPreferenceTable(UserPreferenceEvent evnt,long userId, INH_OP ctx)
    {
      //Default Values
      string dateFormat = "MM_DD_YY";
      string timeFormat = "hh_mm a";

      string currency = "US Dollar";

      string mapApiProvider = "ALK";

      var supportedCurrencies = new Dictionary<Char, String> { { '$', "US Dollar" }, { '€', "Euro" }, { '£', "British Pound" }, { '¥', "Chinese Yen" } };
      string timeSeperator = "_";
      string dateSeperator = "_";
      string thousandsSeparator = ",";
      string decimalSeparator = ".";
      int decimalPrecision = 1;
      var formattingPreferences = new Dictionary<string, string>();

      var formatingValuesXmlString = (from user in ctx.UserPreferencesReadOnly where user.fk_UserID == userId && user.Key == "formattingValues" select user.ValueXML).FirstOrDefault();
      var mapProvider = (from user in ctx.UserPreferencesReadOnly where user.fk_UserID == userId && user.Key == "mapAPIProvider" select user.ValueXML).FirstOrDefault();
      //Getting Values from Formatting Values Xml
      if (!String.IsNullOrEmpty(formatingValuesXmlString))
      {
        XElement formatingValuesXml = XElement.Parse(formatingValuesXmlString);
        formattingPreferences = (from x in formatingValuesXml.Attributes()
                                 select x).ToDictionary(g => g.Name.ToString(), h => h.Value);

        timeSeperator = formattingPreferences.ContainsKey("timeSeparator") ? formattingPreferences["timeSeparator"] : timeSeperator;
        dateSeperator = formattingPreferences.ContainsKey("dateSeparator") ? formattingPreferences["dateSeparator"] : dateSeperator;

        if (formattingPreferences.ContainsKey("dateFormat") && Char.ToLower(formattingPreferences["dateFormat"].FirstOrDefault()) == 'd' && Regex.IsMatch(formattingPreferences["dateFormat"].ToLower(), "^d{2}.{1}m{2,3}.{1}y{2,4}$"))
        {
          dateFormat = "DD_MM_YY".Replace('_', dateSeperator.FirstOrDefault());
        }
        else
        {
          dateFormat = "MM_DD_YY".Replace('_', dateSeperator.FirstOrDefault());
        }

        char timeFormatFirstChar = Char.ToLower(formattingPreferences["timeFormat"].FirstOrDefault());

        if (formattingPreferences.ContainsKey("timeFormat") && (timeFormatFirstChar == 'h' || timeFormatFirstChar == 'j'))
        {
          timeFormat = "HH_mm".Replace('_', timeSeperator.FirstOrDefault());
        }
        else
        {
          timeFormat = "hh_mm a".Replace('_', timeSeperator.FirstOrDefault());
        }

        if (formattingPreferences.ContainsKey("currencySymbol"))
          currency = supportedCurrencies.ContainsKey(formattingPreferences["currencySymbol"].FirstOrDefault()) ? supportedCurrencies[formattingPreferences["currencySymbol"].FirstOrDefault()] : currency;

        if (formattingPreferences.ContainsKey("decimalPrecision"))
        {
          Int32.TryParse(formattingPreferences["decimalPrecision"], out decimalPrecision);
        }

        thousandsSeparator = formattingPreferences.ContainsKey("thousandsSeparator") ? formattingPreferences["thousandsSeparator"] : thousandsSeparator;

        decimalSeparator = formattingPreferences.ContainsKey("decimalSeparator") ? formattingPreferences["decimalSeparator"] : decimalSeparator;
      }
      if (!String.IsNullOrWhiteSpace(mapProvider))
      {
        mapApiProvider = mapProvider.ToLower() == "google" ? "Google" : "ALK";
      }

      evnt.DateFormat = dateFormat;
      evnt.TimeFormat = timeFormat;
      evnt.DecimalPrecision = decimalPrecision.ToString();
      evnt.ThousandsSeparator = thousandsSeparator;
      evnt.DecimalSeparator = decimalSeparator;
      evnt.CurrencySymbol = currency;

      evnt.MapProvider = mapApiProvider;
    }
  }
}