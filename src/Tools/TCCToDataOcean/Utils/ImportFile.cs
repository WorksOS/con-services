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
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.WebApi.Common;

namespace TCCToDataOcean.Utils
{
  public class ImportFile : IImportFile
  {
    private readonly IRestClient _restClient;
    private readonly ILogger _log;
    private readonly ILiteDbAgent _database;

    private const string CONTENT_DISPOSITION = "Content-Disposition: form-data; name=";
    private const string NEWLINE = "\r\n";
    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "-----";
    private const int CHUNK_SIZE = 1024 * 1024;
    private readonly string _bearerToken;
    private readonly string _jwtToken;
    private readonly int _maxFileSize;

    private long _migrationInfoId = -1;

    private long MigrationInfoId
    {
      get
      {
        if (_migrationInfoId < 0)
        {
          _migrationInfoId = _database.Find<MigrationInfo>(Table.MigrationInfo).Id;
        }

        return _migrationInfoId;
      }
    }

    public ImportFile(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication, IRestClient restClient, ILiteDbAgent databaseAgent, IEnvironmentHelper environmentHelper)
    {
      _log = loggerFactory.CreateLogger<ImportFile>();
      _bearerToken = "Bearer " + authentication.GetApplicationBearerToken();
      _jwtToken = environmentHelper.GetVariable("JWT_TOKEN", 1);
      _restClient = restClient;
      _database = databaseAgent;
      _maxFileSize = int.Parse(environmentHelper.GetVariable("MAX_FILE_SIZE", 1));
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public Task<ImportedFileDescriptorListResult> GetImportedFilesFromWebApi(string uri, Project project) =>
      _restClient.SendHttpClientRequest<ImportedFileDescriptorListResult>(
        uri,
        HttpMethod.Get,
        MediaType.APPLICATION_JSON,
        MediaType.APPLICATION_JSON,
        project.CustomerUID);

    /// <summary>
    /// Send request to the FileImportV4 controller
    /// </summary>
    public FileDataSingleResult SendRequestToFileImportV4(string uriRoot,
                                                          ImportedFileDescriptor fileDescr,
                                                          string fullFileName,
                                                          ImportOptions importOptions = new ImportOptions(),
                                                          bool uploadToTCC = false)
    {
      _log.LogInformation(Method.In());

      var createdDt = fileDescr.FileCreatedUtc.ToUniversalTime().ToString("o");
      var updatedDt = fileDescr.FileUpdatedUtc.ToUniversalTime().ToString("o");

      var uri = $"{uriRoot}?projectUid={fileDescr.ProjectUid}&importedFileType={fileDescr.ImportedFileTypeName}" +
                $"&fileCreatedUtc={createdDt}&fileUpdatedUtc={updatedDt}&uploadToTcc={uploadToTCC}";

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

      var response = UploadFileToWebApi(
        fullFileName,
        uri,
        fileDescr,
        importOptions.HttpMethod);

      try
      {
        if (response != null)
        {
          return JsonConvert.DeserializeObject<FileDataSingleResult>(response, new JsonSerializerSettings
          {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            NullValueHandling = NullValueHandling.Ignore
          });
        }
      }
      catch (Exception exception)
      {
        _log.LogInformation(response);
        _log.LogError(exception.Message);
      }

      return null;
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    private string UploadFileToWebApi(string fullFileName, string uri, ImportedFileDescriptor fileDescriptor, HttpMethod httpMethod)
    {
      _log.LogInformation($"{Method.In()} | Filename: {fullFileName}, CustomerUid: {fileDescriptor.CustomerUid}");

      try
      {
        var name = new DirectoryInfo(fullFileName).Name;
        var bytes = File.ReadAllBytes(fullFileName);
        var fileSize = bytes.Length;
        var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);
        string result = null;

        if (fileSize > _maxFileSize)
        {
          _log.LogWarning($"Skipping file {fullFileName}, exceeds MAX_FILE_SIZE of {_maxFileSize} bytes");
          return null;
        }

        _log.LogInformation($"{Method.Info()} | {httpMethod.Method}, Uri: {uri}");

        for (var offset = 0; offset < chunks; offset++)
        {
          _log.LogInformation($"{Method.Info()} | {fileDescriptor.Name}: {(int)Math.Round((double)(100 * offset) / chunks)}% completed ");

          var startByte = offset * CHUNK_SIZE;
          var endByte = Math.Min(fileSize, (offset + 1) * CHUNK_SIZE);

          if (fileSize - endByte < CHUNK_SIZE)
          {
            // The last chunk will be bigger than the chunk size but less than 2*chunkSize
            endByte = fileSize;
          }

          var currentChunkSize = endByte - startByte;
          var boundaryIdentifier = Guid.NewGuid().ToString();
          var flowFileUpload = SetAllAttributesForFlowFile(fileSize, name, offset + 1, chunks, currentChunkSize);
          var currentBytes = bytes.Skip(startByte).Take(currentChunkSize).ToArray();
          var contentType = $"multipart/form-data; boundary={BOUNDARY_START}{boundaryIdentifier}";

          using (var content = new MemoryStream())
          {
            FormatTheContentDisposition(flowFileUpload, currentBytes, name, $"{BOUNDARY_START + BOUNDARY_BLOCK_DELIMITER}{boundaryIdentifier}", content);
            result = DoHttpRequest(uri, httpMethod, content.ToArray(), fileDescriptor, contentType);
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
    private string DoHttpRequest(string resourceUri, HttpMethod httpMethod, byte[] payloadData, ImportedFileDescriptor fileDescriptor, string contentType)
    {
      _database.Update(MigrationInfoId, (MigrationInfo x) => x.FilesTotal += 1);

      if (!(WebRequest.Create(resourceUri) is HttpWebRequest request)) { return string.Empty; }

      request.Method = httpMethod.Method;
      request.Headers.Add("Authorization", _bearerToken);
      request.Headers.Add("X-VisionLink-CustomerUid", fileDescriptor.CustomerUid);
      request.Headers.Add("X-JWT-Assertion", JWTFactory.CreateToken(_jwtToken, fileDescriptor.ImportedBy));

      if (payloadData != null)
      {
        request.ContentType = contentType;
        var writeStream = request.GetRequestStreamAsync().Result;
        writeStream.Write(payloadData, 0, payloadData.Length);
      }

      var responseString = string.Empty;

      try
      {
        using (var response = (HttpWebResponse)request.GetResponseAsync().Result)
        {
          if (response.StatusCode != HttpStatusCode.Accepted)
          {
            _log.LogInformation($"{nameof(DoHttpRequest)}: Response returned status code: {response.StatusCode}");
          }

          responseString = GetStringFromResponseStream(response);
          _log.LogTrace($"{nameof(DoHttpRequest)}: {responseString}");
        }
      }
      catch (AggregateException ex)
      {
        _log.LogError($"{nameof(DoHttpRequest)}: {ex.Message}");
        foreach (var e in ex.InnerExceptions)
        {
          if (!(e is WebException)) { continue; }

          var webException = (WebException)e;

          if (!(webException.Response is HttpWebResponse response)) { continue; }

          return GetStringFromResponseStream(response);
        }
      }

      _database.Update(MigrationInfoId, (MigrationInfo x) => x.FilesUploaded += 1);

      return responseString;
    }

    /// <summary>
    /// Sets the attributes for uploading using flow.
    /// </summary>
    private static FlowFileUpload SetAllAttributesForFlowFile(long fileSize, string name, int currentChunkNumber, int totalChunks, int currentChunkSize) =>
      new FlowFileUpload
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

      var header = Encoding.UTF8.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(header, 0, header.Length);
      resultingStream.Write(chunkContent, 0, chunkContent.Length);

      sb = new StringBuilder();
      sb.Append($"{NEWLINE}{boundary}{BOUNDARY_BLOCK_DELIMITER}{NEWLINE}");
      var tail = Encoding.UTF8.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
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
