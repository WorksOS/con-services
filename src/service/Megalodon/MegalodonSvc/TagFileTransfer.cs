using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace MegalodonSvc
{
  public class TagFileTransfer
  {

    private TimeSpan APItimeout = new TimeSpan(0, 1, 0);
    public const string TAGFILE_ROUTE = "/tagfiles/direct";

    private HttpClient GetClient(string prodURL)
    {
      var client = new HttpClient
      {
        Timeout = APItimeout,
        BaseAddress = new Uri(prodURL)
      };

      HttpRequestHeaders defaultHeaders = client.DefaultRequestHeaders;
      var acceptHeaders = defaultHeaders.Accept;
      acceptHeaders.Clear();
      acceptHeaders.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      return client;
    }

    public async Task<ContractExecutionResult> SendTagFile(CompactionTagFileRequest compactionTagFileRequest, string prodHost, string prodBase)
    {
      var route = prodBase + TAGFILE_ROUTE;
      var client = GetClient(prodHost);
      if (client == null)
        return new ContractExecutionResult(-1, "Failed to get HttpClient");

      string jsonTxt = JsonConvert.SerializeObject(compactionTagFileRequest);
      HttpContent requestContent = new StringContent(jsonTxt, Encoding.UTF8, "application/json");
      try
      {
        HttpResponseMessage responseMessage = await client.PostAsync(route, requestContent);
        var result = await responseMessage.Content.ReadAsStringAsync();

        if (responseMessage.StatusCode == HttpStatusCode.OK)
          return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "success");
        else
          return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, result);
      }
      catch (Exception e)
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, e.Message);
      }
    }

  }
}
