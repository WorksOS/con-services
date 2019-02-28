using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TCCToDataOcean.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;
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
    public ProjectDataSingleResult UpdateProjectCoordinateSystemFile(string uriRoot, Project project, byte[] coordSystemFileContent)
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

      var jsonString = JsonConvert.SerializeObject(new { UpdateProjectEvent = updateProjectEvt }, JsonSettings);
      var response = Task.Run(() => RestClient.SendHttpClientRequest(uriRoot, HttpMethod.Put, jsonString, Types.MediaType.ApplicationJson, Types.MediaType.ApplicationJson, project.CustomerUID)).Result;

      var receiveStream = response.Content.ReadAsStreamAsync().Result;
      var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      var responseBody = readStream.ReadToEnd();

      return JsonConvert.DeserializeObject<ProjectDataSingleResult>(responseBody, new JsonSerializerSettings
      {
        Formatting = Formatting.Indented
      });
    }
  }
}
