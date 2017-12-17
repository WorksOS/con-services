using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.FlowJSHandler;
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
      FlowFile file, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null)
    {
      FileDataSingleResult response = await SendImportedFileToWebApi(file.path + file.flowFilename, projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, dxfUnitsType, surveyedUtc, customHeaders, "POST");
      log.LogDebug("ImportedFileProxy.CreateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    public async Task<FileDataSingleResult> UpdateImportedFile(
      FlowFile file, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null)
    {
      FileDataSingleResult response = await SendImportedFileToWebApi(file.path + file.flowFilename, projectUid,
        importedFileType, fileCreatedUtc, fileUpdatedUtc, dxfUnitsType, surveyedUtc, customHeaders, "PUT");
      log.LogDebug("ImportedFileProxy.UpdateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }


    public async Task<BaseDataResult> DeleteImportedFile(Guid projectUid, Guid importedFileUid, IDictionary<string, string> customHeaders = null)
    {
      BaseDataResult response = await SendRequest<BaseDataResult>("IMPORTED_FILE_API_URL", null, customHeaders, null, "DELETE");
      log.LogDebug("ImportedFileProxy.DeleteImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    #region FlowJSHandler Implementation/Emulation
    private const string BOUNDARY = "------WebKitFormBoundarym45GFZc25WVhjtVB";
    private const string BOUNDARY_START = "----WebKitFormBoundarym45GFZc25WVhjtVB";

    public async Task<FileDataSingleResult> SendImportedFileToWebApi(string importedFileName, Guid projectUid, ImportedFileType importedFileType,
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
      return await UploadFileToWebApi(importedFileName, queryParameters, method);
    }

    private string FormattedDate(DateTime? utcDate)
    {
      return $"{utcDate:yyyy-MM-ddTHH:mm:ss.fffffff}";
    }

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
      var name = new DirectoryInfo(fullFileName).Name;
      Byte[] bytes = File.ReadAllBytes(fullFileName);
      var inputStream = new MemoryStream(bytes);
      var inputAsString = Convert.ToBase64String(inputStream.ToArray());

      using (var filestream = new MemoryStream(Convert.FromBase64String(inputAsString)))
      {
        var flowFileUpload = SetAllAttributesForFlowFile(filestream, name);
        var content = FormatTheContentDisposition(flowFileUpload, filestream, name);
        if (customHeaders == null)
        {
          customHeaders = new Dictionary<string, string>();
        }
        customHeaders.Add("ContentType", "multipart/form-data; boundary=" + BOUNDARY_START);
        return await SendRequest<FileDataSingleResult>("IMPORTED_FILE_API_URL", content, customHeaders, queryParameters, method);
      }
    }

    /// <summary>
    /// File upload
    /// </summary>
    /// <param name="filestream"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private FlowFileUpload SetAllAttributesForFlowFile(Stream filestream, string name)
    {
      var flowFileUpload = new FlowFileUpload
      {
        flowChunkNumber = 1,
        flowChunkSize = 1048576,
        flowCurrentChunkSize = filestream.Length,
        flowTotalSize = filestream.Length,
        flowIdentifier = filestream.Length + "-" + name.Replace(".", ""),
        flowFilename = name,
        flowRelativePath = name,
        flowTotalChunks = 1
      };
      return flowFileUpload;
    }

    /// <summary>
    /// Format the Content Disposition. This is very specific / fussy with the boundary
    /// </summary>
    /// <param name="flowFileUpload"></param>
    /// <param name="filestream"></param>
    /// <returns></returns>
    private string FormatTheContentDisposition(FlowFileUpload flowFileUpload, Stream filestream, string name)
    {
      var sb = new StringBuilder();
      var nl = "\r\n";
      sb.AppendFormat($"{nl}{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowChunkNumber\"{nl}{nl}{flowFileUpload.flowChunkNumber}{nl}{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowChunkSize\"{nl}{nl}{flowFileUpload.flowChunkSize}{nl}" +
                      $"{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowCurrentChunkSize\"{nl}{nl}{flowFileUpload.flowCurrentChunkSize}{nl}{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowTotalSize\"{nl}{nl}{flowFileUpload.flowTotalSize}{nl}" +
                      $"{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowIdentifier\"{nl}{nl}{flowFileUpload.flowIdentifier}{nl}{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowFilename\"{nl}{nl}{flowFileUpload.flowFilename}{nl}" +
                      $"{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowRelativePath\"{nl}{nl}{flowFileUpload.flowRelativePath}{nl}{BOUNDARY}{nl}Content-Disposition: form-data; name=\"flowTotalChunks\"{nl}{nl}{flowFileUpload.flowTotalChunks}{nl}" +
                      $"{BOUNDARY}{nl}Content-Disposition: form-data; name=\"file\"; filename=\"{name}\"{nl}Content-Type: application/octet-stream{nl}{nl}");

      StreamReader reader = new StreamReader(filestream);
      sb.Append(reader.ReadToEnd());
      sb.Append($"{nl}");
      sb.Append($"{BOUNDARY}--{nl}");
      reader.Dispose();
      return Regex.Replace(sb.ToString(), "(?<!\r)\n", "\r\n");
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
