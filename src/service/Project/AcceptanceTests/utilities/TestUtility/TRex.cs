using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using VSS.Productivity3D.Models.ResultHandling;

namespace TestUtility
{
  public class TRex
  {
    public TRex()
    {
    }

    /// <summary>
    /// Gets a list of designs from TRex. The list includes files of all types.
    /// </summary>
    public DesignListResult GetDesignsFromTrex(string customerUid, string projectUid, string jwt = null)
    {
      string uri = Environment.GetEnvironmentVariable("TREX_IMPORTFILE_R_API_URL") + $"?projectUid={projectUid}";

      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid, jwt);
      var designs = JsonConvert.DeserializeObject<DesignListResult>(response);
      return designs;
    }

    /// <summary>
    /// Call the web api for the imported files
    /// </summary>
    private static string CallWebApi(string uri, string method, string configJson, string customerUid, string jwt = null)
    {
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json",
        customerUid, jwt);
      return response;
    }
  }
}
