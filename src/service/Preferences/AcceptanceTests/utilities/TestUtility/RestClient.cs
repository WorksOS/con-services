using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestUtility
{
  public class RestClient
  {
    public static readonly string ServiceBaseUrl;

    private static readonly HttpClient httpClient;

    static RestClient()
    {
      httpClient = new HttpClient();

      httpClient.DefaultRequestHeaders.Add("X-VisionLink-ClearCache", "true");
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");

      var _testConfig = new TestConfig(MySqlHelper.PREFERENCE_DB_SCHEMA_NAME);

      if (Debugger.IsAttached || _testConfig.operatingSystem == "Windows_NT")
      {
        ServiceBaseUrl = _testConfig.debugWebApiUri;
      }
      else
      {
        ServiceBaseUrl = _testConfig.webApiUri;
      }
    }

    public static async Task<string> SendHttpClientRequest(string route, HttpMethod method, string acceptHeader, string contentType, string customerUid = null, object payloadData = null, string jwtToken = null, HttpStatusCode expectedHttpCode = HttpStatusCode.OK)
    {
      var requestMessage = new HttpRequestMessage(method, new Uri($"{ServiceBaseUrl}{route}"));

      if (payloadData != null)
      {
        if (contentType == MediaTypes.JSON)
        {
          requestMessage.Content = new StringContent(payloadData.ToString(), Encoding.UTF8, contentType);
        }
        else if (contentType.StartsWith(MediaTypes.MULTIPART_FORM_DATA))
        {
          requestMessage.Content = new ByteArrayContent(((MemoryStream)payloadData).ToArray());

          requestMessage.Content.Headers.Add("Content-Type", contentType);
        }
      }

      requestMessage.Headers.Add("X-JWT-Assertion", string.IsNullOrEmpty(jwtToken)
                                             ? DEFAULT_JWT
                                             : jwtToken);

      requestMessage.Headers.Add("X-VisionLink-CustomerUid", customerUid);
      requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));

      Console.WriteLine($"[{method}] {requestMessage.RequestUri.AbsoluteUri}");

      using (var httpResponseMessage = await httpClient.SendAsync(requestMessage))
      {
        var receiveStream = await httpResponseMessage.Content.ReadAsStreamAsync();

        if (httpResponseMessage.StatusCode != expectedHttpCode)
        {
          throw new Exception($"Actual response status code {httpResponseMessage.StatusCode} doesn't match expected code {expectedHttpCode}");
        }

        using (var readStream = new StreamReader(receiveStream, Encoding.UTF8))
        {
          var responseStream = readStream.ReadToEnd();

          switch (acceptHeader)
          {
            case MediaTypes.JSON:
              {
                Console.WriteLine($"GetProjectDetailsViaWebApiV4. response: {JsonConvert.SerializeObject(responseStream)}");

                return responseStream;
              }
            case MediaTypes.ZIP:
            case MediaTypes.PROTOBUF:
              {
                var byteContent = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                break;
              }
          }

          throw new Exception($"Unsupported ContentType to request: {route}");
        }
      }
    }

    public static Guid DEFAULT_USER = new Guid("98cdb619-b06b-4084-b7c5-5dcccc82af3b");

    //This is UserUID "98cdb619-b06b-4084-b7c5-5dcccc82af3b"
    public const string DEFAULT_JWT =
      "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU";

    //JWT for user "trmbqa93@gmail.com" with UserUID "9CB733FC-7C61-4F9D-BB41-CE9C28C0088B"
    public const string ANOTHER_JWT =
      "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6IjlDQjczM0ZDLTdDNjEtNEY5RC1CQjQxLUNFOUMyOEMwMDg4QiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTUxNTcxMTAwOSwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.dhfLrn3odtygyAxI2lJdqBI6DZKaDLb_QaFQKEz8G5Q";
  }
}
