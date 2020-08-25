using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CCSS.IntegrationTests.Utils;
using CCSS.IntegrationTests.Utils.Extensions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.MasterData.ProjectTests.FlowJS.Utils
{
  public class FlowFileUploader
  {
    private const string CONTENT_DISPOSITION = "Content-Disposition: form-data; name=";
    private const string NEWLINE = "\r\n";
    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "-----";
    private const int CHUNK_SIZE = 1024 * 1024;

    private readonly IRestClient _httpClient;
    private readonly string _bearerToken;

    public FlowFileUploader(IRestClient restClient)
    {
      _httpClient = restClient;
      _bearerToken = Guid.NewGuid().ToString();
    }

    public async Task<T> UploadFileData<T>(byte[] fileContent, string filename, string uri) where T : ContractExecutionResult
    {
      var bytes = fileContent;
      var fileSize = bytes.Length;
      var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);

      T result = default;

      for (var offset = 0; offset < chunks; offset++)
      {
        var startByte = offset * CHUNK_SIZE;
        var endByte = Math.Min(fileSize, (offset + 1) * CHUNK_SIZE);

        if (fileSize - endByte < CHUNK_SIZE)
        {
          // The last chunk will be bigger than the chunk size but less than 2*chunkSize
          endByte = fileSize;
        }

        var currentChunkSize = endByte - startByte;
        var boundaryIdentifier = Guid.NewGuid().ToString();
        var flowFileUpload = SetAllAttributesForFlowFile(fileSize, filename, offset + 1, chunks, currentChunkSize);
        var currentBytes = bytes.Skip(startByte).Take(currentChunkSize).ToArray();
        var contentType = $"multipart/form-data; boundary={BOUNDARY_START}{boundaryIdentifier}";

        using var content = new MemoryStream();
        FormatTheContentDisposition(flowFileUpload, currentBytes, filename, $"{BOUNDARY_START + BOUNDARY_BLOCK_DELIMITER}{boundaryIdentifier}", content);

        result = await DoHttpRequest<T>(uri, HttpMethod.Post, content.ToArray(), contentType);
      }

      return result;
    }

    /// <summary>
    /// Sets the attributes for uploading using flow.
    /// </summary>
    private static FlowFileAttributes SetAllAttributesForFlowFile(long fileSize, string name, int currentChunkNumber, int totalChunks, int currentChunkSize) =>
      new FlowFileAttributes
      {
        flowChunkNumber = currentChunkNumber,
        flowChunkSize = CHUNK_SIZE,
        flowCurrentChunkSize = currentChunkSize,
        flowTotalSize = fileSize,
        flowIdentifier = fileSize + "-" + name.Replace(".", ""),
        flowFilename = name,
        flowRelativePath = name,
        flowTotalChunks = totalChunks
      };

    /// <summary>
    /// Format the Content Disposition. This is very specific / fussy with the boundary
    /// </summary>
    private static void FormatTheContentDisposition(FlowFileAttributes flowFileUpload, byte[] chunkContent, string name,
      string boundary, MemoryStream resultingStream)
    {
      var sb = new StringBuilder();
      sb.AppendFormat(
        $"{NEWLINE}{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowChunkNumber\"{NEWLINE}{NEWLINE}{flowFileUpload.flowChunkNumber}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowChunkSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowChunkSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowCurrentChunkSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowCurrentChunkSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowTotalSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowTotalSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowIdentifier\"{NEWLINE}{NEWLINE}{flowFileUpload.flowIdentifier}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowFilename\"{NEWLINE}{NEWLINE}{flowFileUpload.flowFilename}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowRelativePath\"{NEWLINE}{NEWLINE}{flowFileUpload.flowRelativePath}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowTotalChunks\"{NEWLINE}{NEWLINE}{flowFileUpload.flowTotalChunks}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"file\"; filename=\"{name}\"{NEWLINE}Content-Type: application/octet-stream{NEWLINE}{NEWLINE}");

      var header = Encoding.UTF8.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(header, 0, header.Length);
      resultingStream.Write(chunkContent, 0, chunkContent.Length);

      sb = new StringBuilder();
      sb.Append($"{NEWLINE}{boundary}{BOUNDARY_BLOCK_DELIMITER}{NEWLINE}");
      var tail = Encoding.UTF8.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(tail, 0, tail.Length);
    }

    private async Task<T> DoHttpRequest<T>(string url, HttpMethod httpMethod, byte[] payloadData, string contentType) where T : ContractExecutionResult
    {
      var request = new HttpRequestMessage(HttpMethod.Post, url);

      request.Headers.Add("Authorization", "Bearer " + _bearerToken);
      request.Headers.Add("X-VisionLink-CustomerUid", "98cdb619-b06b-4084-b7c5-5dcccc82af3b");
      request.Headers.Add("X-JWT-Assertion", JWTFactory.CreateToken(DEFAULT_JWT, "Project service Integration tests"));

      if (payloadData != null)
      {
        request.Content = new ByteArrayContent(payloadData);
        request.Content.Headers.Add("Content-Type", contentType);
      }

      var response = await _httpClient.SendAsync(request);

      return response.StatusCode switch
      {
        HttpStatusCode.BadRequest => await response.ConvertToType<T>(),
        HttpStatusCode.Accepted => await response.ConvertToType<T>(),
        _ => null
      };
    }

    public async Task<string> ConvertToJson(HttpResponseMessage httpResponseMessage)
    {
      var receiveStream = await httpResponseMessage.Content.ReadAsStreamAsync();

      using var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      return await readStream.ReadToEndAsync();
    }

    public const string DEFAULT_JWT = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU";

  }
}
