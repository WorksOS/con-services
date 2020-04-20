using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CCSS.IntegrationTests.Utils.Extensions
{
  public static class HttpResponseMessageExtensions
  {
    public static async Task<T> ConvertToType<T>(this HttpResponseMessage httpResponseMessage)
    {
      var responseConent = await httpResponseMessage.Content.ReadAsStringAsync();

      return JsonConvert.DeserializeObject<T>(responseConent);
    }

    public static async Task<string> ConvertToJson(this HttpResponseMessage httpResponseMessage)
    {
      var receiveStream = await httpResponseMessage.Content.ReadAsStreamAsync();

      using var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      var responseStream = await readStream.ReadToEndAsync();

      return responseStream;
    }

    public static Task<byte[]> ToByteArray(this HttpResponseMessage httpResponseMessage) => httpResponseMessage.Content.ReadAsByteArrayAsync();
  }
}
