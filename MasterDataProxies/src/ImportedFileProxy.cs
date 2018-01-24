using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Proxies
{
  /// <summary>
  /// Proxy for creating, updating and deleting imported files
  /// </summary>
  public class ImportedFileProxy : BaseProxy, IImportedFileProxy
  {
    public ImportedFileProxy(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore, logger)
    { }

    public async Task<FileDataSingleResult> CreateImportedFile(
      string fullFileName, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null)
    {
      FileDataSingleResult response = await SendImportedFileToWebApi($"{fullFileName}", projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, dxfUnitsType, surveyedUtc, customHeaders, "POST");
      log.LogDebug("ImportedFileProxy.CreateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    public async Task<FileDataSingleResult> UpdateImportedFile(
      string fullFileName, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null)
    {
      FileDataSingleResult response = await SendImportedFileToWebApi($"{fullFileName}", projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, dxfUnitsType, surveyedUtc, customHeaders, "PUT");
      log.LogDebug("ImportedFileProxy.UpdateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }


    public async Task<BaseDataResult> DeleteImportedFile(Guid projectUid, Guid importedFileUid, IDictionary<string, string> customHeaders = null)
    {
      BaseDataResult response = await SendRequest<BaseDataResult>("IMPORTED_FILE_API_URL2", Stream.Null, customHeaders, null, "DELETE");
      log.LogDebug("ImportedFileProxy.DeleteImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    #region Flow.js Implementation/Emulation

    private async Task<FileDataSingleResult> SendImportedFileToWebApi(string importedFileName, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null, string method = "POST")
    {
      var queryParameters = $"?projectUid={projectUid}&importedFileType={importedFileType}&fileCreatedUtc={FormattedDate(fileCreatedUtc)}&fileUpdatedUtc={FormattedDate(fileUpdatedUtc)}";
      if (importedFileType == ImportedFileType.SurveyedSurface)
      {
        queryParameters += $"&SurveyedUtc={FormattedDate(surveyedUtc)}";
      }
      if (importedFileType == ImportedFileType.Linework)
      {
        queryParameters += $"&DxfUnitsType={dxfUnitsType}";
      }
      return await UploadFileToWebApi(importedFileName, queryParameters, method, customHeaders);
    }

    private string FormattedDate(DateTime? utcDate)
    {
      return $"{utcDate:yyyy-MM-ddTHH:mm:ss.fffffff}";
    }

    private const string BOUNDARY_BLOCK_DELIMITER = "--";
    private const string BOUNDARY_START = "----WebKitFormBoundary";
    private const string BOUNDARY = BOUNDARY_BLOCK_DELIMITER + BOUNDARY_START;
    private const int CHUNK_SIZE = 1048576; //1024 * 1024

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <param name="fullFileName">Full filename</param>
    /// <param name="queryParameters">Query parameters for the request</param>
    /// <param name="method">HTTP method</param>
    /// <param name="customHeaders">Custom headers for the request</param>
    /// <returns>Repsonse from web api as string</returns>
    public async Task<FileDataSingleResult> UploadFileToWebApi(string fullFileName, string queryParameters, string method, IDictionary<string, string> customHeaders = null)
    {
      if (customHeaders == null)
      {
        customHeaders = new Dictionary<string, string>();
      }

      var name = new DirectoryInfo(fullFileName).Name;
      Byte[] bytes = File.ReadAllBytes(fullFileName);
      var fileSize = bytes.Length;
      var chunks = (int)Math.Max(Math.Floor((double)fileSize / CHUNK_SIZE), 1);
      FileDataSingleResult result = null;
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
        int currentChunkSize = (int) (endByte - startByte);
        var flowId = GenerateId();
        var flowFileUpload = SetAllAttributesForFlowFile(fileSize, name, offset + 1, chunks, currentChunkSize);
        var currentBytes = bytes.Skip(startByte).Take(currentChunkSize).ToArray();
        var content = FormatTheContentDisposition(flowFileUpload, currentBytes, name, $"{BOUNDARY}{flowId}"); 
        customHeaders.Add("Content-Type", $"multipart/form-data; boundary={BOUNDARY_START}{flowId}");
        result = await SendRequest<FileDataSingleResult>("IMPORTED_FILE_API_URL2", content, customHeaders, queryParameters, method);
        customHeaders.Remove("Content-Type");        
      }
      //The last chunk should have the result
      return result;
    }

    /// <summary>
    /// Sets the attributes for uploading using flow.
    /// </summary>
    /// <param name="fileSize"></param>
    /// <param name="name"></param>
    /// <param name="currentChunkNumber"></param>
    /// <param name="totalChunks"></param>
    /// <param name="currentChunkSize"></param>
    /// <returns></returns>
    private FlowFileUpload SetAllAttributesForFlowFile(long fileSize, string name, int currentChunkNumber, int totalChunks, int currentChunkSize)
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
    /// <param name="flowFileUpload"></param>
    /// <param name="chunkContent"></param>
    /// <param name="name"></param>
    /// <param name="boundary"></param>
    /// <returns></returns>
    private MemoryStream FormatTheContentDisposition(FlowFileUpload flowFileUpload, byte[] chunkContent, string name,
      string boundary)
    {
      var sb = new StringBuilder();
      var nl = "\r\n";
      sb.AppendFormat(
        $"{nl}{boundary}{nl}Content-Disposition: form-data; name=\"flowChunkNumber\"{nl}{nl}{flowFileUpload.flowChunkNumber}{nl}{boundary}{nl}Content-Disposition: form-data; name=\"flowChunkSize\"{nl}{nl}{flowFileUpload.flowChunkSize}{nl}" +
        $"{boundary}{nl}Content-Disposition: form-data; name=\"flowCurrentChunkSize\"{nl}{nl}{flowFileUpload.flowCurrentChunkSize}{nl}{boundary}{nl}Content-Disposition: form-data; name=\"flowTotalSize\"{nl}{nl}{flowFileUpload.flowTotalSize}{nl}" +
        $"{boundary}{nl}Content-Disposition: form-data; name=\"flowIdentifier\"{nl}{nl}{flowFileUpload.flowIdentifier}{nl}{boundary}{nl}Content-Disposition: form-data; name=\"flowFilename\"{nl}{nl}{flowFileUpload.flowFilename}{nl}" +
        $"{boundary}{nl}Content-Disposition: form-data; name=\"flowRelativePath\"{nl}{nl}{flowFileUpload.flowRelativePath}{nl}{boundary}{nl}Content-Disposition: form-data; name=\"flowTotalChunks\"{nl}{nl}{flowFileUpload.flowTotalChunks}{nl}" +
        $"{boundary}{nl}Content-Disposition: form-data; name=\"file\"; filename=\"{name}\"{nl}Content-Type: application/octet-stream{nl}{nl}");

      //  UTF8Encoding encoding = new UTF8Encoding();
      //  sb.Append(encoding.GetString(chunkContent));//UTF8 to match GracefulWebRequest
      var resultingStream = new MemoryStream();

      byte[] header = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", nl));
      resultingStream.Write(header,0,header.Length);
      resultingStream.Write(chunkContent,0,chunkContent.Length);
      
      sb = new StringBuilder();  
      sb.Append($"{nl}{boundary}{BOUNDARY_BLOCK_DELIMITER}{nl}");
      byte[] tail = Encoding.ASCII.GetBytes(Regex.Replace(sb.ToString(), "(?<!\r)\n", nl));
      resultingStream.Write(tail, 0, tail.Length);
      return resultingStream;
    }

    /// <summary>
    /// Generate a unique flow identifier for the upload.
    /// </summary>
    /// <returns></returns>
    private string GenerateId()
    {
      //see https://madskristensen.net/blog/generate-unique-strings-and-numbers-in-c/

      long i = 1;
      foreach (byte b in Guid.NewGuid().ToByteArray())
      {
        i *= ((int)b + 1);
      }
      return string.Format("{0:x}", i - DateTime.Now.Ticks);
    }

    private class FlowFileUpload
    {
      public int flowChunkNumber;
      public int flowChunkSize;
      public long flowCurrentChunkSize;
      public long flowTotalSize;
      public string flowIdentifier;
      public string flowFilename;
      public string flowRelativePath;
      public int flowTotalChunks;
    }
    #endregion
  }
}
