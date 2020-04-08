using System.IO;
using CustomerWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.ResultHandling;

namespace CustomerWebApi.Controllers
{
  public class CustomerController : BaseController
  {
    public CustomerController(IConfigurationStore configStore, ILoggerFactory loggerFactory) : base(configStore, loggerFactory)
    {

    }

    [Route("api/v1/Customers/me")]
    [HttpGet]
    public CustomerDataResult GetCustomersForMe()
    {
      var auth = new Authorization(LoggerFactory, Request);
      if (Request.Headers == null)
      {
        return new CustomerDataResult { status = 500, metadata = new Metadata { msg = "Missing Authentication headers" } };
      }
      var cust = new Customer(ConnectionString, Logger,auth);
      var result = cust.getCustomersforUser();
      Logger.LogInformation($"GetCustomersForMe: customerUid {auth.customerUid}. CustomerDataResult {JsonConvert.SerializeObject(result)}");
      return result;
    }

    [Route("accounthierarchy")]
    [HttpGet]
    public string GetAccountHierarchy([FromQuery] bool? toplevelsonly, string targetcustomeruid = null)
    {
      var auth = new Authorization(LoggerFactory, Request);

      if (toplevelsonly == true)
      {
        var cust = new Customer(ConnectionString, Logger, auth);
        var result = cust.getCustomerAccountsforUser();
        var stringresult = JsonConvert.SerializeObject(result);
        Logger.LogInformation($"GetAccountHierarchy: AccountsResult {stringresult}");
        return stringresult;
      }
      // Response {"UserUID":"e55e503c-b7f2-44ec-9c23-b7b0f930b1ee","Customers":[]}
      return  "{\"UserUID\":\"" + auth.userUid + "\"}";
    }

    [Route("v1/HierarchyQueryApi/hierarchyexists")]
    [HttpGet]
    public string HeirachyExists()
    {
      return "{\"status\":200,\"message\":\"Hierarchy Information is not available\",\"hierarchystatus\":false}";
    }

    [Route("UserTOU")]
    [HttpGet]
    public string UserTOU()
    {
      return "{\"isAgreed\":true}";
    }

    [Route("UserLogin/lastLogin")]
    [HttpPut]
    public string UserLogin()
    {
      return "{\"isUpdated\":true}";
    }

    [Route("v1/applications/me")]
    [Route("applications/me")]
    [HttpGet]
    public string applications()
    {
      //   return "{\"applications\":[{\"appUID\":\"697b4048-c00e-11e5-bd1f-022d51c6b7e9\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/unifiedfleet\/\",\"appUrl\":\"generic=https:\/\/alpha1ufl.myvisionlink.com\",\"marketUrl\":\"\/#\/marketing\",\"name\":\"Unified Fleet\",\"enabled\":true,\"displayOrder\":1,\"tpaasAppName\":\"Alpha-VLUnifiedFleet\",\"tpaasAppId\":3402,\"appOwner\":\"1\",\"welcomePageInd\":1},{\"appUID\":\"814940bd-4d80-11e6-ad60-02e61027559d\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/unifiedservice\/\",\"appUrl\":\"generic=https:\/\/alpha-service.myvisionlink.com\",\"marketUrl\":\"https:\/\/alpha1.myvisionlink.com\/\",\"name\":\"Unified Service\",\"enabled\":true,\"displayOrder\":2,\"tpaasAppName\":\"Alpha-VLUnifiedService\",\"tpaasAppId\":3670,\"appOwner\":\"1\",\"welcomePageInd\":0},{\"appUID\":\"c79ad709-625c-414c-a8d2-afa5b6f0be51\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/unifiedproductivity\/\",\"appUrl\":\"generic=https:\/\/alpha-productivity.myvisionlink.com\",\"marketUrl\":\"https:\/\/alpha1.myvisionlink.com\/\",\"name\":\"Unified Productivity\",\"enabled\":true,\"displayOrder\":3,\"tpaasAppName\":\"Alpha-VLUnifiedProductivity\",\"tpaasAppId\":4105,\"appOwner\":\"1\",\"welcomePageInd\":0},{\"appUID\":\"964dca05-5af4-11e6-946d-02a5b89e0f8d\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/administrator\/\",\"appUrl\":\"generic=http:\/\/alpha-vla.myvisionlink.com\/\",\"marketUrl\":\"https:\/\/alpha1.myvisionlink.com\/\",\"name\":\"Administrator\",\"enabled\":true,\"displayOrder\":4,\"tpaasAppName\":\"Alpha-VisionLinkAdministrator\",\"tpaasAppId\":3286,\"appOwner\":\"1\",\"welcomePageInd\":0},{\"appUID\":\"881782fd-c5e3-11e7-a938-028cda19da76\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/3dproductivitymanager\/new\/\",\"appUrl\":\"generic=https:\/\/alpha-3dproductivity.myvisionlink.com\/\",\"marketUrl\":\"\/#\/marketing\",\"name\":\"3D Productivity Manager\",\"enabled\":true,\"displayOrder\":5,\"tpaasAppName\":\"ccss-3d-productivity-alpha\",\"tpaasAppId\":5823,\"appOwner\":\"1\",\"welcomePageInd\":0},{\"appUID\":\"6a0e2c57-c00e-11e5-bd1f-022d51c6b7e9\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/landfill\/\",\"appUrl\":\"generic=http:\/\/alpha-landfill.myvisionlink.com\",\"marketUrl\":\"\/#\/marketing\",\"name\":\"Landfill\",\"enabled\":true,\"displayOrder\":6,\"tpaasAppName\":\"Alpha-VLLandfill\",\"tpaasAppId\":3399,\"appOwner\":\"1\",\"welcomePageInd\":1},{\"appUID\":\"e443cf4f-d6b8-11e8-98b9-02a663d13122\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/catproductivity\/\",\"appUrl\":\"generic=https:\/\/qa-catproductivity.cat.com\",\"marketUrl\":\"https:\/\/qa-catproductivity.cat.com\",\"name\":\"Cat\u00AE Productivity\",\"enabled\":true,\"displayOrder\":7,\"tpaasAppName\":null,\"tpaasAppId\":0,\"appOwner\":\"0\",\"welcomePageInd\":0},{\"appUID\":\"6adda5dc-c00e-11e5-bd1f-022d51c6b7e9\",\"iconUrl\":\"https:\/\/s3.amazonaws.com\/visionlinkassets\/app-icons\/v1\/operatormgmt\/\",\"appUrl\":\"generic=https:\/\/myoperatorsq.cat.com\/\",\"marketUrl\":null,\"name\":\"Operator Management\",\"enabled\":true,\"displayOrder\":8,\"tpaasAppName\":null,\"tpaasAppId\":0,\"appOwner\":\"0\",\"welcomePageInd\":0}]      ,\"status\":200,\"reqId\":\"1ff7ae0e-9670-4285-a765-16440ddb681e\",\"metadata\":{\"msg\":\"Application details retrieved successfully\",\"count\":8}}";
      return "{\"applications\":[]\"status\":200,\"reqId\":\"1ff7ae0e-9670-4285-a765-16440ddb681e\",\"metadata\":{\"msg\":\"Application details retrieved successfully\",\"count\":0}}";
    }

    [Route("user")] //user?keyName=global
    [HttpGet]
    public string Userpreference([FromQuery] string keyName)
    {
      if (keyName == "productivity3d-global-filter")
      {
        return "{\"PreferenceKeyName\":\"productivity3d-global-filter\",\"PreferenceJson\":\"{\"globalFilters\":{\"selectedDisplayType\":{\"type\":\"cutfill\",\"subType\":null},\"isShowQualityMertics\":false,\"mapSettings\":[{\"key\":\"Alignment\",\"localeString\":\"MAP_SETTINGS_ALIGNMENT\",\"isSelected\":true},{\"key\":\"DesignSurface\",\"localeString\":\"MAP_SETTINGS_DESIGN\",\"isSelected\":true},{\"key\":\"Linework\",\"localeString\":\"MAP_SETTINGS_LINEWORK\",\"isSelected\":true},{\"key\":\"ProductionData\",\"localeString\":\"MAP_SETTINGS_PROD_DATA\",\"isSelected\":true},{\"key\":\"CellInformation\",\"localeString\":\"MAP_SETTINGS_CELL_INFORMATION\",\"isSelected\":true},{\"key\":\"Geofence\",\"localeString\":\"MAP_SETTINGS_GEOFENCE\",\"isSelected\":true}],\"mapProfileToolSettings\":[{\"key\":\"Cell Markers\",\"localeString\":\"MAP_CELL_MARKERS\",\"isSelected\":true}],\"useLegacy\":false,\"mapDisplayTypeValue\":0,\"ddvColumnSettings\":{\"columns\":[{\"name\":\"MachineName\",\"visible\":true,\"pinned\":\"left\"},{\"name\":\"DateTime\",\"visible\":true,\"pinned\":\"left\",\"sort\":{\"priority\":0,\"direction\":\"desc\"}},{\"name\":\"DesignName\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Pass\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Passcount\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Lift\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Elevation\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Thickness\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"GpsMode\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"GpsAccuracy\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"GpsTolerance\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"MachineGear\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"MachineSpeed\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Ccv\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"TargetCcv\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Temperature\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"MinTemperature\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"MaxTemperature\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Mdp\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"TargetMdp\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Rmv\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Frequency\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Amplitude\",\"visible\":true,\"pinned\":\"\"},{\"name\":\"Vibration\",\"visible\":true,\"pinned\":\"\"}]}}}\",\"PreferenceKeyUID\":\"cbd467c8-ec50-406c-b3f6-62131ecfdc39\",\"SchemaVersion\":\"v1\"}";
      }

      if (keyName == "productivity3d-projects")
      {
        var sr = new StreamReader("Json/prefprojects.txt");
        var jsonText = sr.ReadToEnd();
        return jsonText;
      }

      return "{\"PreferenceKeyName\":\"global\",\"PreferenceJson\":\"{\"Timezone\":\"Central Standard Time\",\"Language\":\"en-US\",\"Units\":\"Metric\",\"DateFormat\":\"DD/MM/YY\",\"TimeFormat\":\"HH:mm\",\"AssetLabelDisplay\":\"Asset ID\",\"MeterLabelDisplay\":\"Hour Meter\",\"LocationDisplay\":\"Lat/Lon\",\"CurrencySymbol\":\"US Dollar\",\"TemperatureUnit\":\"Celsius\",\"PressureUnit\":\"PSI\",\"MapProvider\":\"ALK\",\"BrowserRefresh\":\"Hourly\",\"ThousandsSeparator\":\",\",\"DecimalSeparator\":\".\",\"DecimalPrecision\":\"1\",\"Theme\":\"Light\"}\",\"PreferenceKeyUID\":\"88c00121-f3e4-4b1e-aef3-f63ff1029223\",\"SchemaVersion\":\"1.1\"}";
    }



    [Route("users/organizations/{organ}/roles")] 
    [HttpGet]
    public string Userorganizations([FromQuery] string organ, string provider_id)
    {
      return "{\"role_list\":[{\"role_id\":893,\"provider_id\":\"ccss-3d-productivity-alpha\",\"role_name\":\"Administrator\",\"description\":\"null\"}]}";
    }

    [Route("users/organizations/{organ}/permissions")] 
    [HttpGet]
    public string UserOrganizationRoles([FromQuery] string organ, string provider_id, int? limit = 10000)
    {
      return "{\"permission_list\":[{\"permission_id\":2070,\"action\":\"Download\",\"resource\":\"Report\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2071,\"action\":\"Manage\",\"resource\":\"Report\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2072,\"action\":\"Schedule\",\"resource\":\"Report\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2073,\"action\":\"Create\",\"resource\":\"Export\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2074,\"action\":\"Create\",\"resource\":\"Project\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2075,\"action\":\"Import\",\"resource\":\"ProductionData\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2076,\"action\":\"import\",\"resource\":\"ProjectFiles\",\"provider_id\":\"ccss-3d-productivity-alpha\"},{\"permission_id\":2077,\"action\":\"Manage\",\"resource\":\"User\",\"provider_id\":\"ccss-3d-productivity-alpha\"}]}";
    }

    [Route("Notification/Count")] ////Notification/Count?notificationStatus=1&notificationUserStatus=1
    [HttpGet]
    public string NotificationCount([FromQuery] int notificationStatus, int notificationUserStatus)
    {
      return "{\"notifications\":[],\"status\":\"SUCCESS\"}";
    }
    
    [Route("Notification/1")]      //Notification/1?notificationStatus=1&notificationUserStatus=1
    [HttpGet]
    public string Notification1([FromQuery] int notificationStatus, int notificationUserStatus)
    {
      return "{\"links\":{},\"total\":{\"items\":0,\"pages\":0},\"notifications\":[],\"status\":\"SUCCESS\"}";
    }

    [Route("v1/logs")]
    [HttpPost]
    public string logs()
    {
      return "{}";
    }

    [Route("NotificationTypes/AlertTypes")]      //Notification/1?notificationStatus=1&notificationUserStatus=1
    [HttpGet]
    public string AlertTypes()
    {
      return "{}";
    }

  }
}


