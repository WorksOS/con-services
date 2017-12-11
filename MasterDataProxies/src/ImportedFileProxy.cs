using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
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
      FileDataSingleResult response = await SendRequest<FileDataSingleResult>("IMPORTEDFILE_API_URL", payload, customHeaders, route, "POST");
      log.LogDebug("ImportedFileProxy.CreateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    public async Task<FileDataSingleResult> UpdateImportedFile(
      FlowFile file, Guid projectUid, ImportedFileType importedFileType,
      DateTime fileCreatedUtc, DateTime fileUpdatedUtc, DxfUnitsType? dxfUnitsType,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders = null)
    {
      FileDataSingleResult response = await SendRequest<FileDataSingleResult>("IMPORTEDFILE_API_URL", payload, customHeaders, route, "PUT");
      log.LogDebug("ImportedFileProxy.UpdateImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }


    public async Task<BaseDataResult> DeleteImportedFile(Guid projectUid, Guid importedFileUid, IDictionary<string, string> customHeaders = null)
    {
      BaseDataResult response = await SendRequest<BaseDataResult>("IMPORTEDFILE_API_URL", null, customHeaders, null, "DELETE");
      log.LogDebug("ImportedFileProxy.DeleteImportedFile: response: {0}", response == null ? null : JsonConvert.SerializeObject(response));

      return response;
    }

    #region FlowJSHandler Implementation/Emulation
    private const string BOUNDARY = "------WebKitFormBoundarym45GFZc25WVhjtVB";
    private const string BOUNDARY_START = "----WebKitFormBoundarym45GFZc25WVhjtVB";

    public FileDataSingleResult SendImportedFileToWebApi(string importedFileName, Guid projectUid, ImportedFileType importedFileType,
      DxfUnitsType dxfUnitsType, DateTime fileCreatedUtc, DateTime fileUpdatedUtc,
      DateTime? surveyedUtc, IDictionary<string, string> customHeaders, string method = "POST")
    {
      var queryParameters = $"?projectUid={projectUid}&importedFileType={importedFileType}&fileCreatedUtc={fileCreatedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}&fileUpdatedUtc={fileUpdatedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
      if (importedFileType == ImportedFileType.SurveyedSurface)
      {
        queryParameters += $"&SurveyedUtc={surveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff}";
      }
      if (importedFileType == ImportedFileType.Linework)
      {
        queryParameters += $"&DxfUnitsType={dxfUnitsType}";
      }
      var response = UploadFileToWebApi(importedFileName, uri, method);
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <param name="fullFileName">Full filename</param>
    /// <param name="uri">Full uri to send it to</param>
    /// <param name="method">HTTP method</param>
    /// <param name="customHeaders">Custom headers for the request</param>
    /// <returns>Repsonse from web api as string</returns>
    public string UploadFileToWebApi(string fullFileName, string uri, string method, IDictionary<string, string> customHeaders = null)
    {
      try
      {
        var name = new DirectoryInfo(fullFileName).Name;
        Byte[] bytes = File.ReadAllBytes(fullFileName);
        var inputStream = new MemoryStream(bytes);
        var inputAsString = Convert.ToBase64String(inputStream.ToArray());

        using (var filestream = new MemoryStream(Convert.FromBase64String(inputAsString)))
        {
          var flowFileUpload = SetAllAttributesForFlowFile(filestream, name);
          var content = FormatTheContentDisposition(flowFileUpload, filestream, name);
          var response = DoHttpRequest(uri, method, content, customHeaders);

          return response;
        }
      }
      catch (Exception ex)
      {
        return ex.Message;
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


    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    /// <param name="resourceUri">Full URI</param>
    /// <param name="httpMethod">Method to use</param>
    /// <param name="payloadData"></param>
    /// <param name="customerUid"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    private string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, IDictionary<string, string> customHeaders = null)
    {
      var request = WebRequest.Create(resourceUri) as HttpWebRequest;
      if (request == null)
      { return string.Empty; }
      request.Method = httpMethod;
      request.Headers = customHeaders;
      if (payloadData != null)
      {
        request.ContentType = "multipart/form-data; boundary=" + BOUNDARY_START;
        var writeStream = request.GetRequestStreamAsync().Result;
        byte[] bytes = new UTF8Encoding().GetBytes(payloadData);
        writeStream.Write(bytes, 0, bytes.Length);
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
