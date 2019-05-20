using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TCCToDataOcean.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.Utils
{
  public class WebApiUtils : IWebApiUtils
  {
    private readonly IRestClient RestClient;
    private readonly IConfigurationStore _configuration;

    private readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    public WebApiUtils(IRestClient restClient, IConfigurationStore configurationStore)
    {
      RestClient = restClient;
      _configuration = configurationStore;
    }

    /// <summary>
    /// Update the project via the web api. 
    /// </summary>
    public Task<ProjectDataSingleResult> UpdateProjectCoordinateSystemFile(string uriRoot, Project project, byte[] coordSystemFileContent)
    {
      var updateProjectEvt = new UpdateProjectEvent
      {
        ProjectUID = Guid.Parse(project.ProjectUID),
        ProjectName = project.Name,
        ProjectType = project.ProjectType,
        ProjectEndDate = project.EndDate,
        ProjectTimezone = project.ProjectTimeZone,
        CoordinateSystemFileName = project.CoordinateSystemFileName,
        CoordinateSystemFileContent = coordSystemFileContent,
        ProjectBoundary = project.GeometryWKT,
        Description = project.Description,
        ReceivedUTC = DateTime.UtcNow,
        ActionUTC = project.LastActionedUTC
      };

      var jsonString = JsonConvert.SerializeObject(updateProjectEvt, JsonSettings);

      return RestClient.SendHttpClientRequest<ProjectDataSingleResult>(uriRoot, HttpMethod.Put, Types.MediaType.APPLICATION_JSON, Types.MediaType.APPLICATION_JSON, project.CustomerUID, jsonString);
    }
  }
}
