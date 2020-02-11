using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.Productivity3D.Models.ResultHandling.Coords;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;

namespace TCCToDataOcean.Models
{
  public class CSIBAgent : ICSIBAgent
  {
    private readonly IRestClient RestClient;

    private readonly string raptor3DPMApiUrl;
    private readonly string coordSystemApiUrl;

    public CSIBAgent(IRestClient restClient, IEnvironmentHelper environmentHelper)
    {
      RestClient = restClient;

      raptor3DPMApiUrl = environmentHelper.GetVariable("RAPTOR_3DPM_API_URL", 1).Replace("2.0", "1.0");
      coordSystemApiUrl = environmentHelper.GetVariable("COORDINATE_SERVICE_URL", 1);
    }

    public Task<CSIBResult> GetCSIBForProject(Project project) => RestClient.SendHttpClientRequest<CSIBResult>($"{raptor3DPMApiUrl}/csib?projectUid={project.ProjectUID}", HttpMethod.Get, MediaType.APPLICATION_JSON, MediaType.APPLICATION_JSON, project.CustomerUID);
    public Task<JObject> GetCoordSysInfoFromCSIB64(Project project, string coordSysId) => RestClient.SendHttpClientRequest<JObject>($"{coordSystemApiUrl}/coordinatesystems/imports/csib/base64?csib_64={WebUtility.UrlEncode(coordSysId)}", HttpMethod.Put, MediaType.APPLICATION_JSON, MediaType.APPLICATION_JSON, project.CustomerUID);
    public Task<string> GetCalibrationFileForCoordSysId(Project project, string csib) => RestClient.SendHttpClientRequest<string>($"{coordSystemApiUrl}/coordinatesystems/calibrationFile?id={csib}", HttpMethod.Get, MediaType.APPLICATION_JSON, MediaType.TEXT_PLAIN, project.CustomerUID);
  }
}
