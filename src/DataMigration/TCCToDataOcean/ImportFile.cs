using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class ImportFile
  {
    private readonly ILogger Log;
    private readonly string uriRoot;
    private readonly string BaseUrl;
    private readonly IConfigurationStore ConfigStore;
    private readonly ITPaaSApplicationAuthentication Authentication;
    private readonly IRestClient RestClient;

    private const string ProjectWebApiKey = "PROJECT_API_URL";

    private const string CONTENT_DISPOSITION = "Content-Disposition: form-data; name=";
    private const string NEWLINE = "\r\n";
    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "-----";
    private const int CHUNK_SIZE = 1024 * 1024;

    public ImportFile(ILoggerFactory loggerFactory, IConfigurationStore configurationStore, ITPaaSApplicationAuthentication authentication, 
      IRestClient restClient, string uriRoot = null)
    {
      this.uriRoot = uriRoot ?? "api/v4/importedfile";
      Log = loggerFactory.CreateLogger<ImportFile>();
      ConfigStore = configurationStore;
      BaseUrl = ConfigStore.GetValueString(ProjectWebApiKey);
      if (string.IsNullOrEmpty(BaseUrl))
      {
        throw new InvalidOperationException($"Mising environment variable {ProjectWebApiKey}");
      }
      Authentication = authentication;
      RestClient = restClient;
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public T GetImportedFilesFromWebApi<T>(string uri, Guid customerUid)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<T>(response);
      return filesResult;
    }

    public T GetFromWebApi<T>(string uri, Guid customerUid)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<T>(response);
      return filesResult;
    }

    /// <summary>
    /// Send request to the FileImportV4 controller
    /// </summary>
    public FileDataSingleResult SendRequestToFileImportV4(FileData fileDescr, ImportOptions importOptions = new ImportOptions())
    {
      var createdDt = fileDescr.FileCreatedUtc.ToUniversalTime().ToString("o");
      var updatedDt = fileDescr.FileUpdatedUtc.ToUniversalTime().ToString("o");

      var uri = BaseUrl + $"{uriRoot}?projectUid={fileDescr.ProjectUid}&importedFileType={fileDescr.ImportedFileTypeName}&fileCreatedUtc={createdDt:yyyy-MM-ddTHH:mm:ss.fffffff}&fileUpdatedUtc={updatedDt:yyyy-MM-ddTHH:mm:ss.fffffff}";

      switch (fileDescr.ImportedFileTypeName)
      {
        case "SurveyedSurface":
          uri = $"{uri}&SurveyedUtc={fileDescr.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
          break;
        case "Linework":
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

      if (importOptions.HttpMethod == HttpMethod.Delete)
      {
        uri = BaseUrl + $"api/v4/importedfile?projectUid={fileDescr.ProjectUid}&importedFileUid={fileDescr.ImportedFileUid}";
      }

      var response = UploadFileToWebApi(fileDescr.Name, uri, fileDescr.CustomerUid, importOptions.HttpMethod);

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
      try
      {
        var name = new DirectoryInfo(fullFileName).Name;
        Byte[] bytes = File.ReadAllBytes(fullFileName);
        var fileSize = bytes.Length;
        var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);
        string result = null;
        for (var offset = 0; offset < chunks; offset++)
        {
          var startByte = offset * CHUNK_SIZE;
          var endByte = Math.Min(fileSize, (offset + 1) * CHUNK_SIZE);
          if (fileSize - endByte < CHUNK_SIZE)
          {
            // The last chunk will be bigger than the chunk size,
            // but less than 2*chunkSize
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
            result = DoHttpRequest(uri, httpMethod, content, customerUid, contentType);
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
    /// Send HTTP request for importing a file with json payload
    /// </summary>
    public string DoHttpRequest(string resourceUri, HttpMethod httpMethod, string payloadData, string customerUid, string contentType)
    {
      byte[] bytes = new UTF8Encoding().GetBytes(payloadData);
      return DoHttpRequest(resourceUri, httpMethod, bytes, customerUid, contentType);
    }

    /// <summary>
    /// Send HTTP request for importing a file with binary (file contents) payload
    /// </summary>
    public string DoHttpRequest(string resourceUri, HttpMethod httpMethod, MemoryStream payloadData, string customerUid, string contentType)
    {
      byte[] bytes = payloadData.ToArray();
      return DoHttpRequest(resourceUri, httpMethod, bytes, customerUid, contentType);
    }

    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    private string DoHttpRequest(string resourceUri, HttpMethod httpMethod, byte[] payloadData, string customerUid, string contentType)
    {
      if (!(WebRequest.Create(resourceUri) is HttpWebRequest request))
      {
        return string.Empty;
      }

      request.Method = httpMethod.Method;
      request.Headers["Authorization"] = Authentication.GetApplicationBearerToken();
      request.Headers["X-VisionLink-CustomerUid"] = customerUid; 
      //request.Headers["X-VisionLink-ClearCache"] = "true";

      if (payloadData != null)
      {
        request.ContentType = contentType;
        var writeStream = request.GetRequestStreamAsync().Result;
        writeStream.Write(payloadData, 0, payloadData.Length);
      }

      try
      {
        string responseString;
        using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
        {
          responseString = GetStringFromResponseStream(response);
        }
        return responseString;
      }
      catch (AggregateException ex)
      {
        foreach (var e in ex.InnerExceptions)
        {
          if (!(e is WebException)) continue;
          var webException = (WebException)e;
          var response = webException.Response as HttpWebResponse;
          if (response == null) continue;
          var resp = GetStringFromResponseStream(response);
          return resp;
        }
        return string.Empty;
      }
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

      if (readStream != null)
      {
        var reader = new StreamReader(readStream);
        var responseString = reader.ReadToEnd();
        reader.Dispose();
        return Regex.Replace(responseString, "(?<!\r)\n", "\r\n");
      }

      return string.Empty;
    }

    /// <summary>
    /// Call the web api for the imported files
    /// </summary>
    private string CallWebApi(string uri, string method, string configJson, string customerUid)
    {
      var response = RestClient.DoHttpRequest(uri, method, configJson, "application/json", customerUid);
      return response;
    }

  }
}
