using System;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.ResultHandling.Designs;

namespace TestUtility
{
  public class TRex
  {
    /// <summary>
    /// Gets a list of designs from TRex. The list includes files of all types.
    /// </summary>
    public DesignListResult GetDesignsFromTrex(string customerUid, string projectUid, string jwt = null)
    {
      var uri = Environment.GetEnvironmentVariable("TREX_IMPORTFILE_READ_API_URL") + $"?projectUid={projectUid}";
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid, jwt);

      return JsonConvert.DeserializeObject<DesignListResult>(response);
    }

    /// <summary>
    /// Call the web api for the imported files
    /// </summary>
    private static string CallWebApi(string uri, string method, string configJson, string customerUid, string jwt = null)
    {
      return new RestClientUtil().DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json", customerUid, jwt);
    }
  }
}
