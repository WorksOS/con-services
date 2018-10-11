using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;



namespace TRexIgniteTest
{
  /// <summary>
  /// Simple class to help this client talk to the TRex gateway
  /// </summary>
  public class GatewayHelper
  {


    private TimeSpan APItimeout = new TimeSpan(0, 30, 0); // thirty minutes to debug api :)

    public string LastMessage;
    public int LastCode;
    public string LastHttpStatusCode;

    public class TagfileRequest
    {
      public string fileName;
      public byte[] data;
      public String projectUid;
      public String OrgId;
    }

    public class GatewayResponse
    {
      public int Code;
      public string Message;
    }


    private void GetLastResponse(string result)
    {
      GatewayResponse resp = JsonConvert.DeserializeObject<GatewayResponse>(result);
      if (resp != null)
      {
        this.LastMessage = resp.Message;
        this.LastCode = resp.Code;
      }
      else
      {
        this.LastMessage = "No response from request";
        this.LastCode = 0;

      }
    }

    private HttpClient GetClient(string siteUrl, string route)
    {
      var client = new HttpClient
      {
        Timeout = APItimeout,
        BaseAddress = new Uri(siteUrl)
      };

      HttpRequestHeaders defaultHeaders = client.DefaultRequestHeaders;
      var acceptHeaders = defaultHeaders.Accept;
      acceptHeaders.Clear();
      acceptHeaders.Add(new MediaTypeWithQualityHeaderValue("application/json"));
      return client;
    }

    private byte[] FileToByteArray(string input)
    {
      FileStream sourceFile = new FileStream(input, FileMode.Open); //Open streamer
      BinaryReader binReader = new BinaryReader(sourceFile);
      byte[] output = new byte[sourceFile.Length]; //create byte array of size file
      for (long i = 0; i < sourceFile.Length; i++)
        output[i] = binReader.ReadByte(); //read until done
      sourceFile.Close(); //dispose streamer
      binReader.Close(); //dispose reader
      return output;
    }

    /// <summary>
    /// Take a tagfile and submitt it to TRex gateway
    /// </summary>
    /// <param name="siteUrl"></param>
    /// <param name="projectID"></param>
    /// <param name="orgID"></param>
    /// <param name="tagfilePath"></param>
    /// <returns></returns>
    public async Task<long> PostProcessTagFileAsync(string siteUrl, string projectID, string orgID, string tagfilePath)
    {

      string route = "api/v2/tagfiles";
      var client = GetClient(siteUrl, route);
      if (client == null)
      {
        return -1;
      }

      string tagFileName = Path.GetFileName(tagfilePath);

      TagfileRequest request = new TagfileRequest()
      {
        fileName = tagFileName,
        data = FileToByteArray(tagfilePath),
        OrgId = orgID,
        projectUid = projectID
      };

      string jsonTxt = JsonConvert.SerializeObject(request);

      HttpContent requestContent = new StringContent(jsonTxt, Encoding.UTF8, "application/json");

      HttpResponseMessage responseMessage = await client.PostAsync(route, requestContent);
      var result = await responseMessage.Content.ReadAsStringAsync();
      GetLastResponse(result);
      LastHttpStatusCode = responseMessage.StatusCode.ToString();

      return 1;
    }


    /// <summary>
    /// Get tile from Gateway
    /// </summary>
    /// <param name="siteUrl"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    public async Task<byte[]> GetTile(string siteUrl, string json)
    {

      string route = "api/v1/tile/filestream";
      var client = GetClient(siteUrl, route);
      if (client == null)
      {
        return null;
      }

      HttpContent requestContent = new StringContent(json, Encoding.UTF8, "application/json");

      HttpResponseMessage responseMessage = await client.PostAsync(route, requestContent);

      LastHttpStatusCode = responseMessage.StatusCode.ToString();

      if (responseMessage.StatusCode == HttpStatusCode.OK)
      {
        Stream tileStream = await responseMessage.Content.ReadAsStreamAsync();
        using (MemoryStream ms = new MemoryStream())
        {
          tileStream.CopyTo(ms);
          return ms.ToArray();
        }
      }

      return null;
    }
  }
}
