﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TestUtility.Model;
using VSS.Common.Exceptions;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TestUtility
{
  public class ImportFile
  {
    public ImportedFileDescriptorListResult ExpectedImportFileDescriptorsListResult = new ImportedFileDescriptorListResult
    {
      ImportedFileDescriptors = ImmutableList<ImportedFileDescriptor>.Empty
    };

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
      this.uriRoot = uriRoot ?? "api/v4/importedfile";

      ExpectedImportFileDescriptorSingleResult = new ImportedFileDescriptorSingleResult(ImportFileDescriptor);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public ImportedFileDescriptorListResult GetImportedFilesFromWebApiV4(string uri, Guid customerUid, string jwt = null)
    {
      return GetImportedFilesFromWebApi<ImportedFileDescriptorListResult>(uri, customerUid, jwt);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public ImmutableList<DesignDetailV2Result> GetImportedFilesFromWebApiV2(string uri, Guid customerUid, string jwt = null)
    {
      return GetImportedFilesFromWebApi<ImmutableList<DesignDetailV2Result>>(uri, customerUid, jwt);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    public T GetImportedFilesFromWebApi<T>(string uri, Guid customerUid, string jwt = null)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString(), jwt);
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
    public ImportedFileDescriptorSingleResult SendRequestToFileImportV4(TestSupport ts, string[] importFileArray, int row, ImportOptions importOptions = new ImportOptions(), string expectedExceptionMessage = null)
    {
      var uri = ts.GetBaseUri();
      var ed = ts.ConvertImportFileArrayToObject(importFileArray, row);

      ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor = ed;

      var createdDt = ed.FileCreatedUtc.ToUniversalTime().ToString("o");
      var updatedDt = ed.FileUpdatedUtc.ToUniversalTime().ToString("o");

      uri = uri + $"{uriRoot}?projectUid={ed.ProjectUid}&importedFileType={ed.ImportedFileTypeName}&fileCreatedUtc={createdDt:yyyy-MM-ddTHH:mm:ss.fffffff}&fileUpdatedUtc={updatedDt:yyyy-MM-ddTHH:mm:ss.fffffff}";

      switch (ed.ImportedFileTypeName)
      {
        case "SurveyedSurface":
          uri = $"{uri}&SurveyedUtc={ed.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff} ";
          break;
        case "Linework":
          uri = $"{uri}&DxfUnitsType={ed.DxfUnitsType} ";
          break;
        case "ReferenceSurface":
          uri = $"{uri}&ParentUid={ed.ParentUid}&Offset={ed.Offset} ";
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
        uri = ts.GetBaseUri() + $"api/v4/importedfile?projectUid={ed.ProjectUid}&importedFileUid={ImportedFileUid}";
      }

      var response = UploadFilesToWebApi(ed.Name, uri, ed.CustomerUid, importOptions.HttpMethod, ed.ImportedFileTypeName == "ReferenceSurface");
      ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name = Path.GetFileName(ExpectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name);  // Change expected result

      try
      {
        return JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response, new JsonSerializerSettings
        {
          DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
          NullValueHandling = NullValueHandling.Ignore
        });
      }
      catch (Exception exception)
      {
        Console.WriteLine(response);
        if (expectedExceptionMessage != response)
        {
          Assert.Fail(response);
        }
      }

      return null;
    }

    /// <summary>
    /// Add a string array of data 
    /// </summary>
    public string SendImportedFilesToWebApiV2(TestSupport ts, long projectId, string[] importFileArray, int row)
    {
      var uri = ts.GetBaseUri();
      uri = uri + $"api/v2/projects/{projectId}/importedfiles";
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
      string requestJson = JsonConvert.SerializeObject(importedFileTbc, new JsonSerializerSettings { DateTimeZoneHandling = DateTimeZoneHandling.Unspecified });

      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, "PUT", requestJson, HttpStatusCode.OK, "application/json", ts.CustomerUid.ToString());

      return response;
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <returns>Repsonse from web api as string</returns>
    private string UploadFilesToWebApi(string fullFileName, string uri, string customerUid, HttpMethod httpMethod, bool isReferenceSurface)
    {
      try
      {
        var name = new DirectoryInfo(fullFileName).Name;
        //For reference surfaces no file to upload
        Byte[] bytes = isReferenceSurface ? null : File.ReadAllBytes(fullFileName);
        var fileSize = isReferenceSurface ? 0 : bytes.Length;
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
          var currentBytes = isReferenceSurface ? null : bytes.Skip(startByte).Take(currentChunkSize).ToArray();
          string contentType = $"multipart/form-data; boundary={BOUNDARY_START}{boundaryIdentifier}";

          using (var content = new MemoryStream())
          {
            FormatTheContentDisposition(flowFileUpload, currentBytes, name,
              $"{BOUNDARY_START + BOUNDARY_BLOCK_DELIMITER}{boundaryIdentifier}", content);
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
    public string DoHttpRequest(string resourceUri, HttpMethod httpMethod, string payloadData, string customerUid, string contentType, string jwt = null)
    {
      byte[] bytes = new UTF8Encoding().GetBytes(payloadData);
      return DoHttpRequest(resourceUri, httpMethod, bytes, customerUid, contentType, jwt);
    }

    /// <summary>
    /// Send HTTP request for importing a file with binary (file contents) payload
    /// </summary>
    public string DoHttpRequest(string resourceUri, HttpMethod httpMethod, MemoryStream payloadData, string customerUid, string contentType, string jwt = null)
    {
      byte[] bytes = payloadData.ToArray();
      return DoHttpRequest(resourceUri, httpMethod, bytes, customerUid, contentType, jwt);
    }

    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    private static string DoHttpRequest(string resourceUri, HttpMethod httpMethod, byte[] payloadData, string customerUid, string contentType, string jwt = null)
    {
      if (!(WebRequest.Create(resourceUri) is HttpWebRequest request))
      {
        return string.Empty;
      }

      request.Method = httpMethod.Method;
      request.Headers["X-JWT-Assertion"] = jwt ?? RestClientUtil.DEFAULT_JWT;
      request.Headers["X-VisionLink-CustomerUid"] = customerUid; //"87bdf851-44c5-e311-aa77-00505688274d";
      request.Headers["X-VisionLink-ClearCache"] = "true";

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
      if (chunkContent != null)
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
    private static string CallWebApi(string uri, string method, string configJson, string customerUid = null, string jwt = null)
    {
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json",
        customerUid, jwt);
      return response;
    }
  }
}
