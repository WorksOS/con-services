using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;

namespace TCCToDataOcean.Utils
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
    public Task<ProjectDataSingleResult> UpdateProjectCoordinateSystemFile(string uriRoot, MigrationJob job)
    {
      var filename = !string.IsNullOrEmpty(job.Project.CoordinateSystemFileName)
        ? job.Project.CoordinateSystemFileName
        : "CoordinateData.dc";

      var updateProjectEvt = UpdateProjectRequest.CreateUpdateProjectRequest(
        Guid.Parse(job.Project.ProjectUID),
        job.Project.ProjectType,
        job.Project.Name,
        job.Project.Description,
        job.Project.EndDate,
        filename,
        job.CoordinateSystemFileBytes,
        job.Project.GeometryWKT);

      var jsonString = JsonConvert.SerializeObject(updateProjectEvt, JsonSettings);

      return RestClient.SendHttpClientRequest<ProjectDataSingleResult>(uriRoot, HttpMethod.Put, Types.MediaType.APPLICATION_JSON, Types.MediaType.APPLICATION_JSON, job.Project.CustomerUID, jsonString);
    }
  }
}
