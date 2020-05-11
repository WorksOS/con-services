using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TestUtility.Model;
using VSS.Productivity3D.Project.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace TestUtility
{
  public class ImportFile
  {
    private static readonly Dictionary<string, byte[]> _importFileCache;

    public ImportedFileDescriptorListResult ExpectedImportFileDescriptorsListResult = new ImportedFileDescriptorListResult
    {
      ImportedFileDescriptors = ImmutableList<ImportedFileDescriptor>.Empty
    };

    static ImportFile()
    {
      _importFileCache = new Dictionary<string, byte[]>();
    }

    public ImportedFileDescriptor ImportFileDescriptor = new ImportedFileDescriptor();
    public ImportedFileDescriptorSingleResult ExpectedImportFileDescriptorSingleResult;
    public string ImportedFileUid;

    private readonly string uriRoot;

    private const string CONTENT_DISPOSITION = "Content-Disposition: form-data; name=";
    private const string NEWLINE = "\r\n";
    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "-----";
    private const int CHUNK_SIZE = 1024 * 1024;

    public ImportFile(string uriRoot = null)
    {
      this.uriRoot = uriRoot ?? "api/v6/importedfile";

      ExpectedImportFileDescriptorSingleResult = new ImportedFileDescriptorSingleResult(ImportFileDescriptor);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public async Task<T> GetImportedFilesFromWebApi<T>(string uri, Guid customerUid, string jwt = null)
    {
      var response = await RestClient.SendHttpClientRequest(uri, HttpMethod.Get, MediaTypes.JSON, MediaTypes.JSON, customerUid.ToString(), null, jwt);

      return JsonConvert.DeserializeObject<T>(response);
    }

    /// <summary>
    /// Send request to the FileImportV4 controller
    /// </summary>
    public async Task<ImportedFileDescriptorSingleResult> SendRequestToFileImportV6(
      TestSupport ts,
      string[] importFileArray,
      int row,
      ImportOptions importOptions = new ImportOptions(),
      HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      var fileDescriptor = ts.ConvertImportFileArrayToObject(importFileArray, row);

      ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor = fileDescriptor;

      var createdDt = fileDescriptor.FileCreatedUtc.ToString(CultureInfo.InvariantCulture);
      var updatedDt = fileDescriptor.FileUpdatedUtc.ToString(CultureInfo.InvariantCulture);

      var uri = $"{uriRoot}?projectUid={fileDescriptor.ProjectUid}&importedFileType={fileDescriptor.ImportedFileTypeName}&fileCreatedUtc={createdDt}&fileUpdatedUtc={updatedDt}";

      switch (fileDescriptor.ImportedFileTypeName)
      {
        case "SurveyedSurface":
          uri = $"{uri}&SurveyedUtc={fileDescriptor.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
          break;
        case "Linework":
          uri = $"{uri}&DxfUnitsType={fileDescriptor.DxfUnitsType}";
          break;
        case "ReferenceSurface":
          uri = $"{uri}&ParentUid={fileDescriptor.ParentUid}&Offset={fileDescriptor.Offset}";
          break;
        case "GeoTiff":
          uri = $"{uri}&SurveyedUtc={fileDescriptor.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
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
        uri = $"api/v6/importedfile?projectUid={fileDescriptor.ProjectUid}&importedFileUid={ImportedFileUid}";
      }

      string response;

      if (fileDescriptor.ImportedFileType == ImportedFileType.ReferenceSurface)
      {
        response = await DoHttpRequest(uri, importOptions.HttpMethod, null, fileDescriptor.CustomerUid, MediaTypes.JSON, statusCode: statusCode);
      }
      else
      {
        response = await UploadFilesToWebApi(fileDescriptor.Name, uri, fileDescriptor.CustomerUid, importOptions.HttpMethod, statusCode);

        if (fileDescriptor.ImportedFileType != ImportedFileType.ReferenceSurface)
          ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name = Path.GetFileName(ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name);  // Change expected result
      }

      return JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response, new JsonSerializerSettings
      {
        DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver()
      });
    }

    /// <summary>
    /// Add a string array of data 
    /// </summary>
    public Task<string> SendImportedFilesToWebApiV5TBC(TestSupport ts, long projectId, string[] importFileArray, int row, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      var uri = $"api/v5/projects/{projectId}/importedfiles";
      var ed = ts.ConvertImportFileArrayToObject(importFileArray, row);

      var importedFileTbc = new ImportedFileTbc
      {
        FileSpaceId = "u710e3466-1d47-45e3-87b8-81d1127ed4ed",
        Path = Path.GetFullPath(ed.Name),
        Name = Path.GetFileName(ed.Name),
        ImportedFileTypeId = ed.ImportedFileType,
        CreatedUtc = ed.FileCreatedUtc,
        AlignmentFile = ed.ImportedFileType == ImportedFileType.Alignment
                        ? new AlignmentFile { Offset = 1 } : null,
        SurfaceFile = ed.ImportedFileType == ImportedFileType.SurveyedSurface
          ? new SurfaceFile { SurveyedUtc = new DateTime() } : null,
        LineworkFile = ed.ImportedFileType == ImportedFileType.Linework
        ? new LineworkFile { DxfUnitsTypeId = DxfUnitsType.Meters } : null
      };

      var requestJson = JsonConvert.SerializeObject(importedFileTbc, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      return RestClient.SendHttpClientRequest(uri, HttpMethod.Put, MediaTypes.JSON, MediaTypes.JSON, ts.CustomerUid.ToString(), requestJson, expectedHttpCode: statusCode);
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <returns>Response from web api as string</returns>
    private async Task<string> UploadFilesToWebApi(string fullFileName, string uri, string customerUid, HttpMethod httpMethod, HttpStatusCode statusCode)
    {
      //For reference surfaces no file to upload
      var fileInfo = new DirectoryInfo(fullFileName);
      var filename = fileInfo.Name;

      byte[] bytes;

      // Resolve the 'real' filename as it appears on disk, but continue the request using the uniquely hashed filename.
      var actualFilename = TestFileResolver.GetRealFilename(filename);

      lock (_importFileCache)
      {
        var alreadyCached = _importFileCache.TryGetValue(actualFilename, out var cachedFileData);

        bytes = alreadyCached
          ? cachedFileData
          : File.ReadAllBytes(TestFileResolver.GetRealFilePath(filename));

        if (!alreadyCached)
        {
          _importFileCache.TryAdd(actualFilename, bytes);
        }
      }

      var fileSize = bytes.Length;
      var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);
      string result = null;

      for (var offset = 0; offset < chunks; offset++)
      {
        var startByte = offset * CHUNK_SIZE;
        var endByte = Math.Min(fileSize, (offset + 1) * CHUNK_SIZE);

        if (fileSize - endByte < CHUNK_SIZE)
        {
          // The last chunk will be bigger than the chunk size, but less than 2*chunkSize.
          endByte = fileSize;
        }

        var currentChunkSize = endByte - startByte;
        var boundaryIdentifier = Guid.NewGuid().ToString();
        var flowFileUpload = SetAllAttributesForFlowFile(fileSize, filename, offset + 1, chunks, currentChunkSize);
        var currentBytes = bytes.Skip(startByte).Take(currentChunkSize).ToArray();
        var contentType = $"multipart/form-data; boundary={BOUNDARY_START}{boundaryIdentifier}";

        using (var content = new MemoryStream())
        {
          FormatTheContentDisposition(flowFileUpload, currentBytes, filename,
            $"{BOUNDARY_START + BOUNDARY_BLOCK_DELIMITER}{boundaryIdentifier}", content);

          result = await RestClient.SendHttpClientRequest(uri, httpMethod, MediaTypes.JSON, contentType, customerUid, content, expectedHttpCode: statusCode);
        }
      }

      //The last chunk should have the result
      return result;
    }

    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    private static Task<string> DoHttpRequest(string resourceUri, HttpMethod httpMethod, byte[] payloadData, string customerUid, string contentType, string jwt = null, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
      return RestClient.SendHttpClientRequest(resourceUri, httpMethod, MediaTypes.JSON, contentType, customerUid, payloadData, jwt, expectedHttpCode: statusCode);
    }

    /// <summary>
    /// Sets the attributes for uploading using flow.
    /// </summary>
    private static FlowFileUpload SetAllAttributesForFlowFile(long fileSize, string name, int currentChunkNumber, int totalChunks, int currentChunkSize)
    {
      return new FlowFileUpload
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
    }

    /// <summary>
    /// Format the Content Disposition. This is very specific / fussy with the boundary
    /// </summary>
    private static void FormatTheContentDisposition(FlowFileUpload flowFileUpload, byte[] chunkContent, string name,
      string boundary, MemoryStream resultingStream)
    {
      var sb = new StringBuilder().AppendFormat(
        $"{NEWLINE}{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowChunkNumber\"{NEWLINE}{NEWLINE}{flowFileUpload.flowChunkNumber}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowChunkSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowChunkSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowCurrentChunkSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowCurrentChunkSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowTotalSize\"{NEWLINE}{NEWLINE}{flowFileUpload.flowTotalSize}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowIdentifier\"{NEWLINE}{NEWLINE}{flowFileUpload.flowIdentifier}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowFilename\"{NEWLINE}{NEWLINE}{flowFileUpload.flowFilename}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowRelativePath\"{NEWLINE}{NEWLINE}{flowFileUpload.flowRelativePath}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"flowTotalChunks\"{NEWLINE}{NEWLINE}{flowFileUpload.flowTotalChunks}{NEWLINE}" +
        $"{boundary}{NEWLINE}{CONTENT_DISPOSITION}\"file\"; filename=\"{name}\"{NEWLINE}Content-Type: application/octet-stream{NEWLINE}{NEWLINE}");

      var header = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(header, 0, header.Length);
      if (chunkContent != null)
        resultingStream.Write(chunkContent, 0, chunkContent.Length);

      sb = new StringBuilder();
      sb.Append($"{NEWLINE}{boundary}{BOUNDARY_BLOCK_DELIMITER}{NEWLINE}");
      var tail = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", NEWLINE));
      resultingStream.Write(tail, 0, tail.Length);
    }
  }
}
