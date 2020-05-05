using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Json;
using MockProjectWebApi.Services;
using MockProjectWebApi.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockProjectSettingsController : BaseController
  {
    private readonly ProjectService projectService;

    public MockProjectSettingsController(ILoggerFactory loggerFactory, IProjectService projectService)
    : base(loggerFactory)
    {
      this.projectService = (ProjectService)projectService;
    }

    /// <summary>
    /// Gets the project settings targets used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The mocked settings</returns>
    [Route("api/v4/projectsettings/{projectUid}")]
    [HttpGet]
    public ProjectSettingsDataResult GetMockProjectSettingsTargets(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockProjectSettingsTargets)}: projectUid={projectUid}");

      JObject settings = null;

      if (projectUid == ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID)
        settings = JsonConvert.DeserializeObject<JObject>(ProjectService.PROJECT_SETTINGS_TARGETS);
      else if (projectUid == ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_2)
        settings = JsonConvert.DeserializeObject<JObject>(ProjectService.PROJECT_SETTINGS_TARGETS_EX);

      return new ProjectSettingsDataResult { ProjectUid = projectUid, Settings = settings };
    }

    /// <summary>
    /// Gets the project settings colours used in the Raptor service acceptance tests.
    /// The data is mocked.
    /// </summary>
    /// <returns>The mocked settings</returns>
    [Route("api/v4/projectcolors/{projectUid}")]
    [HttpGet]
    public ProjectSettingsDataResult GetMockProjectSettingsColors(string projectUid)
    {
      Logger.LogInformation($"{nameof(GetMockProjectSettingsColors)}: projectUid={projectUid}");

      JObject settings = null;

      switch (projectUid)
      {
        case ConstantsUtil.CUSTOM_SETTINGS_DIMENSIONS_PROJECT_UID:
          {
            settings = JsonConvert.DeserializeObject<JObject>(ProjectService.PROJECT_SETTINGS_COLORS);
            break;
          }
        case ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1:
          {
            settings = JsonResourceHelper.GetColorSettings(ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1);
            break;
          }
      }

      return new ProjectSettingsDataResult { ProjectUid = projectUid, Settings = settings };
    }
  }
}
