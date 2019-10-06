using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Productivity3D.Models.ResultHandling.Designs;

namespace TestUtility
{
  public class TRex
  {
    /// <summary>
    /// Gets a list of designs from TRex. The list includes files of all types.
    /// </summary>
    public async Task<DesignListResult> GetDesignsFromTrex(string customerUid, string projectUid, string jwt = null)
    {
      var uri = Environment.GetEnvironmentVariable("TREX_IMPORTFILE_READ_API_URL") + $"?projectUid={projectUid}";
      var response = await RestClient.SendHttpClientRequest(uri, HttpMethod.Get, MediaTypes.JSON, MediaTypes.JSON, customerUid, jwt);
      
      return JsonConvert.DeserializeObject<DesignListResult>(response);
    }
  }
}
