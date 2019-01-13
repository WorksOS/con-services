using System;
using System.Net.Http;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean
{
  public class WebApiUtils : IWebApiUtils
  {
    private readonly IRestClient RestClient;

    private readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    public WebApiUtils(IRestClient restClient)
    {
      RestClient = restClient;
    }

    /// <summary>
    /// Update the project via the web api. 
    /// </summary>
    public ProjectDataSingleResult UpdateProjectViaWebApi(string uriRoot, Project project, byte[] coordSystemFileContent)
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
      var response = RestClient.DoHttpRequest(uriRoot, HttpMethod.Put.ToString(), jsonString, "application/json", project.CustomerUID);
      return JsonConvert.DeserializeObject<ProjectDataSingleResult>(response);
    }

  }


}
