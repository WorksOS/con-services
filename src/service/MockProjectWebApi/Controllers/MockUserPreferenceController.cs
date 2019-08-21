using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Json;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockUserPreferenceController : BaseController
  {
    public MockUserPreferenceController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    /// <summary>
    /// Dummies the get user preferences. keyName always equals "global"
    /// </summary>
    [Route("api/v1/mock/preferences/user")]
    [Route("api/v1/user")]
    [HttpGet]
    public UserPreferenceResult DummyGetUserPreferences([FromQuery] string keyName)
    {
      Logger.LogInformation($"{nameof(DummyGetUserPreferences)}");

      return new UserPreferenceResult
      {
        PreferenceKeyName = "global",
        PreferenceJson = JsonResourceHelper.GetUserPreferencesJson("DummyUserPreferences"),
        PreferenceKeyUID = "88c00121-f3e4-4b1e-aef3-f63ff1029223",
        SchemaVersion = "1"
      };
    }
  }
}
