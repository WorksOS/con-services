using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CCSS.IntegrationTests.Utils
{
  public class IntegrationTestBase
  {
    protected IRestClient restClient;

    public async Task<T> SendAsync<T>(string url, HttpMethod method, string customerUid, HttpStatusCode expectedStatusCode)
    {
      var response = await restClient.SendAsync(url, method, customerUid: customerUid);

      if (expectedStatusCode != response.StatusCode)
      {
        throw new Exception($"Actual response StatusCode ({response.StatusCode}) doesn't match expected status code ({expectedStatusCode})");
      }

      return await response.ConvertToType<T>();
    }

    protected static async Task<JObject> ReadJsonFile(string folderName, string filename)
    {
      using var file = File.OpenText(Path.Combine("TestData", folderName, filename));
      using var reader = new JsonTextReader(file);

      return (JObject)await JToken.ReadFromAsync(reader);
    }
  }
}
