using System;
using System.Net;
using System.Net.Http;
using CoordinateSystemFileResolver.Interfaces;
using CoordinateSystemFileResolver.Types;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using VSS.Productivity3D.Models.ResultHandling.Coords;

namespace CoordinateSystemFileResolver.Utils
{
  public class CSIBAgent : ICSIBAgent
  {
    private readonly ILogger log;
    private readonly IRestClient RestClient;

    private readonly string raptor3DPMApiUrl;
    private readonly string coordSystemApiUrl;

    public CSIBAgent(ILoggerFactory logger, IRestClient restClient, IEnvironmentHelper environmentHelper)
    {
      log = logger.CreateLogger(GetType());

      RestClient = restClient;

      raptor3DPMApiUrl = environmentHelper.GetVariable("RAPTOR_3DPM_API_URL", 1).Replace("2.0", "1.0");
      coordSystemApiUrl = environmentHelper.GetVariable("COORDINATE_SERVICE_URL", 1);
    }

    public CSIBResult GetCSIBForProject(Guid projectUid, Guid customerUid)
    {
      log.LogInformation($"Requesting CSIB from Raptor for Project: {projectUid}, Customer: {customerUid}");

      var response = RestClient.SendHttpClientRequest<CSIBResult>($"{raptor3DPMApiUrl}/csib?projectUid={projectUid.ToString()}", HttpMethod.Get, MediaType.APPLICATION_JSON, MediaType.APPLICATION_JSON, customerUid: customerUid.ToString());

      log.LogInformation($"Recevied response: {response}");

      return response;
    }

    public JObject GetCoordSysInfoFromCSIB64(Guid projectUid, string coordSysId) => RestClient.SendHttpClientRequest<JObject>($"{coordSystemApiUrl}/coordinatesystems/imports/csib/base64?csib_64={WebUtility.UrlEncode(coordSysId)}", HttpMethod.Put, MediaType.APPLICATION_JSON, MediaType.APPLICATION_JSON);

    public string GetCalibrationFileForCoordSysId(Guid projectUid, string csib) => RestClient.SendHttpClientRequest<string>($"{coordSystemApiUrl}/coordinatesystems/calibrationFile?id={csib}", HttpMethod.Get, MediaType.APPLICATION_JSON, MediaType.TEXT_PLAIN);
  }
}
