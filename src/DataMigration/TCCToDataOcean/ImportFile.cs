using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using TCCToDataOcean.Types;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class ImportFile : IImportFile
  {
    private readonly IRestClient RestClient;
    private readonly ILogger Log;
    private readonly ILiteDbAgent _migrationDb;

    private const string CONTENT_DISPOSITION = "Content-Disposition: form-data; name=";
    private const string NEWLINE = "\r\n";
    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "-----";
    private const int CHUNK_SIZE = 1024 * 1024;
    private readonly string BearerToken;

    public ImportFile(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication, IRestClient restClient, ILiteDbAgent liteDbAgent)
    {
      Log = loggerFactory.CreateLogger<ImportFile>();
      _migrationDb = liteDbAgent;

      BearerToken = "Bearer " + authentication.GetApplicationBearerToken();

      RestClient = restClient;
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public FileDataResult GetImportedFilesFromWebApi(string uri, Project project)
    {
      Log.LogInformation($"## In ## {nameof(GetImportedFilesFromWebApi)} | Get imported files for {project.ProjectUID}, customer {project.CustomerUID}");

      var response = Task.Run(() => RestClient.SendHttpClientRequest(uri, HttpMethod.Get, null, MediaType.ApplicationJson, MediaType.ApplicationJson, project.CustomerUID)).Result;

      // TODO (Aaron) handle non 200 result codes.

      var receiveStream = response.Content.ReadAsStreamAsync().Result;
      var readStream = new StreamReader(receiveStream, Encoding.UTF8);
      var responseBody = readStream.ReadToEnd();

      Log.LogInformation($"## Out ## {nameof(GetImportedFilesFromWebApi)} | Status code: {response.StatusCode}, {responseBody}");

      return JsonConvert.DeserializeObject<FileDataResult>(responseBody, new JsonSerializerSettings
      {
        Formatting = Formatting.Indented
      });
    }

    /// <summary>
    /// Send request to the FileImportV4 controller
    /// </summary>
    public FileDataSingleResult SendRequestToFileImportV4(string uriRoot, FileData fileDescr, string fullFileName, ImportOptions importOptions = new ImportOptions())
    {
      var createdDt = fileDescr.FileCreatedUtc.ToUniversalTime().ToString("o");
      var updatedDt = fileDescr.FileUpdatedUtc.ToUniversalTime().ToString("o");

      var uri = $"{uriRoot}?projectUid={fileDescr.ProjectUid}&importedFileType={fileDescr.ImportedFileTypeName}&fileCreatedUtc={createdDt:yyyy-MM-ddTHH:mm:ss.fffffff}&fileUpdatedUtc={updatedDt:yyyy-MM-ddTHH:mm:ss.fffffff}";

      switch (fileDescr.ImportedFileType)
      {
        case ImportedFileType.SurveyedSurface:
          uri = $"{uri}&SurveyedUtc={fileDescr.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
          break;
        case ImportedFileType.Linework:
          uri = $"{uri}&DxfUnitsType={fileDescr.DxfUnitsType}";
          break;
      }

      if (importOptions.QueryParams != null)
      {
        foreach (var param in importOptions.QueryParams)
        {
          uri = $"{uri}&{param}";
        }
      }

      var response = UploadFileToWebApi(fullFileName, uri, fileDescr.CustomerUid, importOptions.HttpMethod);

      try
      {
        return JsonConvert.DeserializeObject<FileDataSingleResult>(response, new JsonSerializerSettings
        {
          DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
          NullValueHandling = NullValueHandling.Ignore
        });
      }
      catch (Exception exception)
      {
        Log.LogInformation(response);
        Log.LogError(exception.Message);
      }

      return null;
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <returns>Repsonse from web api as string</returns>
    private string UploadFileToWebApi(string fullFileName, string uri, string customerUid, HttpMethod httpMethod)
    {
      Log.LogInformation($"{nameof(UploadFileToWebApi)}: Filename: {fullFileName}, CustomerUid: {customerUid}");

      try
      {
        var name = new DirectoryInfo(fullFileName).Name;
        byte[] bytes = File.ReadAllBytes(fullFileName);
        var fileSize = bytes.Length;
        var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);
        string result = null;

        for (var offset = 0; offset < chunks; offset++)
        {
          var startByte = offset * CHUNK_SIZE;
          var endByte = Math.Min(fileSize, (offset + 1) * CHUNK_SIZE);
          if (fileSize - endByte < CHUNK_SIZE)
          {
            // The last chunk will be bigger than the chunk size but less than 2*chunkSize
            endByte = fileSize;
          }

          int currentChunkSize = endByte - startByte;
          var boundaryIdentifier = Guid.NewGuid().ToString();
          var flowFileUpload = SetAllAttributesForFlowFile(fileSize, name, offset + 1, chunks, currentChunkSize);
          var currentBytes = bytes.Skip(startByte).Take(currentChunkSize).ToArray();
          string contentType = $"multipart/form-data; boundary={BOUNDARY_START}{boundaryIdentifier}";

          using (var content = new MemoryStream())
          {
            FormatTheContentDisposition(flowFileUpload, currentBytes, name, $"{BOUNDARY_START + BOUNDARY_BLOCK_DELIMITER}{boundaryIdentifier}", content);
            result = DoHttpRequest(uri, httpMethod, content.ToArray(), customerUid, contentType);
          }
        }
        //The last chunk should have the result
        return result;
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    private string DoHttpRequest(string resourceUri, HttpMethod httpMethod, byte[] payloadData, string customerUid, string contentType)
    {
      Log.LogInformation($"{nameof(DoHttpRequest)}: {httpMethod.Method}, Uri: {resourceUri}");

      if (!(WebRequest.Create(resourceUri) is HttpWebRequest request)) { return string.Empty; }

      request.Method = httpMethod.Method;
      request.Headers["Authorization"] = BearerToken;
      request.Headers["X-VisionLink-CustomerUid"] = customerUid;
      request.Headers["X-JWT-Assertion"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=";

      if (payloadData != null)
      {
        request.ContentType = contentType;
        var writeStream = request.GetRequestStreamAsync().Result;
        writeStream.Write(payloadData, 0, payloadData.Length);
      }

      string responseString = string.Empty;

      try
      {
        using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
        {
          Log.LogInformation($"{nameof(DoHttpRequest)}: Response returned status code: {response.StatusCode}");
          responseString = GetStringFromResponseStream(response);
          Log.LogTrace($"{nameof(DoHttpRequest)}: {responseString}");
        }
      }
      catch (AggregateException ex)
      {
        foreach (var e in ex.InnerExceptions)
        {
          if (!(e is WebException)) { continue; }

          var webException = (WebException)e;
          var response = webException.Response as HttpWebResponse;

          if (response == null) { continue; }

          return GetStringFromResponseStream(response);
        }
      }

      return responseString;
    }

    /// <summary>
    /// Sets the attributes for uploading using flow.
    /// </summary>
    private static FlowFileUpload SetAllAttributesForFlowFile(long fileSize, string name, int currentChunkNumber, int totalChunks, int currentChunkSize)
    {
      var flowFileUpload = new FlowFileUpload
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
      return flowFileUpload;
    }

    /// <summary>
    /// Format the Content Disposition. This is very specific / fussy with the boundary
    /// </summary>
    private static void FormatTheContentDisposition(FlowFileUpload flowFileUpload, byte[] chunkContent, string name,
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

      byte[] header = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(header, 0, header.Length);
      resultingStream.Write(chunkContent, 0, chunkContent.Length);

      sb = new StringBuilder();
      sb.Append($"{NEWLINE}{boundary}{BOUNDARY_BLOCK_DELIMITER}{NEWLINE}");
      byte[] tail = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(tail, 0, tail.Length);
    }

    /// <summary>
    /// Get the HTTP Response from the response stream and store in a string variable
    /// </summary>
    private static string GetStringFromResponseStream(HttpWebResponse response)
    {
      var readStream = response.GetResponseStream();

      if (readStream == null) { return string.Empty; }

      using (var reader = new StreamReader(readStream))
      {
        return Regex.Replace(reader.ReadToEnd(), "(?<!\r)\n", "\r\n");
      }
    }
  }
}
