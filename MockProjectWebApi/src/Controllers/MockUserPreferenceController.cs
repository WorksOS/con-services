using System;
using MasterDataModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace src.Controllers
{
  public class MockUserPreferenceController : Controller
  {
    /// <summary>
    /// Dummies the get user preferences. keyName always equals "global"
    /// </summary>
    [Route("api/v1/mock/preferences/user")]
    [HttpGet]
    public UserPreferenceResult DummyGetUserPreferences([FromQuery] string keyName)
    {
      Console.WriteLine("DummyGetUserPreferences");
      return new UserPreferenceResult
      {
         PreferenceKeyName = "global",
         PreferenceJson = "{\"Timezone\":\"New Zealand Standard Time\",\"Language\":\"en-US\",\"Units\":\"Metric\",\"DateFormat\":\"dd/MM/yy\",\"TimeFormat\":\"HH:mm\",\"AssetLabelDisplay\":\"Asset ID\",\"MeterLabelDisplay\":\"Hour Meter\",\"LocationDisplay\":\"Address\",\"CurrencySymbol\":\"US Dollar\",\"TemperatureUnit\":\"Celsius\",\"PressureUnit\":\"PSI\",\"MapProvider\":\"ALK\",\"BrowserRefresh\":\"Hourly\",\"ThousandsSeparator\":\",\",\"DecimalSeparator\":\".\",\"DecimalPrecision\":\"1\"}",
         PreferenceKeyUID = "88c00121-f3e4-4b1e-aef3-f63ff1029223",
         SchemaVersion = "1"
      };
    }
  }
}
