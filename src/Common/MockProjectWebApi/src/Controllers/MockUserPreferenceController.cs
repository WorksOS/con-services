using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Json;
using System;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
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
      var result = new UserPreferenceResult
      {
        PreferenceKeyName = "global",
        PreferenceJson = JsonResourceHelper.GetUserPreferencesJson("DummyUserPreferences"),
        PreferenceKeyUID = "88c00121-f3e4-4b1e-aef3-f63ff1029223",
        SchemaVersion = "1"
      };

      return result;
    }
  }
}