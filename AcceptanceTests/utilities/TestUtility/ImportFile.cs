using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using TestUtility.Model;

namespace TestUtility
{
  public class ImportFile
  {

    public ImportedFileDescriptor importFileDescriptor = new ImportedFileDescriptor();
    public ImportedFileDescriptorSingleResult expectedImportFileDescriptorSingleResult;
    private const string BOUNDARY = "------WebKitFormBoundarym45GFZc25WVhjtVB";
    private const string BOUNDARY_START = "----WebKitFormBoundarym45GFZc25WVhjtVB";

    public ImportedFileDescriptorListResult expectedImportFileDescriptorsListResult = new ImportedFileDescriptorListResult ()
    {ImportedFileDescriptors = ImmutableList<ImportedFileDescriptor>.Empty};


    public ImportFile()
    {
      expectedImportFileDescriptorSingleResult = new ImportedFileDescriptorSingleResult(importFileDescriptor);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    public ImportedFileDescriptorListResult GetImportedFilesFromWebApi(string uri, Guid customerUid, string projectUid)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(response);
      return filesResult;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="importFileArray"></param>
    /// <returns></returns>
    public ImportedFileDescriptorSingleResult PostImportedFilesToWebApi (TestSupport ts, string [] importFileArray) 
    {
      var uri = ts.GetBaseUri();
      var ed = ts.ConvertImportFileArrayToObject(importFileArray);
      uri = uri + $"api/v4/importedfile?projectUid={ed.ProjectUid}&importedFileType={ed.ImportedFileTypeName}&fileCreatedUtc={ed.FileCreatedUtc.ToUniversalTime()}&fileUpdatedUtc={ed.FileUpdatedUtc.ToUniversalTime()}";
      var response = UploadFilesToWebApi(ed.Name, uri, ed.CustomerUid);
      var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response);
      return filesResult;
    }

    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <param name="fullFileName">Full filename</param>
    /// <param name="uri">Full uri to send it to</param>
    /// <param name="customerUid">Customer Uid</param>
    /// <returns>Repsonse from web api as string</returns>
    public string UploadFilesToWebApi(string fullFileName, string uri, string customerUid)
    {
      try
      {
        var filestream = new FileStream(fullFileName, FileMode.Open);
        var flowFileUpload = SetAllAttributesForFlowFile(filestream);       
        var content = FormatTheContentDisposition(flowFileUpload, filestream);
        var response = DoHttpRequest(uri, "POST", content,customerUid);
        return response;
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
    }

    /// <summary>
    /// Send HTTP request for importing a file
    /// </summary>
    /// <param name="resourceUri">Full URI</param>
    /// <param name="httpMethod">Method to use</param>
    /// <param name="payloadData"></param>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    public string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, string customerUid = null)
    {
      var request = WebRequest.Create(resourceUri) as HttpWebRequest;
      if (request == null)
        { return string.Empty; }
      request.Method = httpMethod;
      request.Headers["X-JWT-Assertion"] ="eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6MTA3OSwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJQcm9qZWN0IE1hc3RlcmRhdGEiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL1Byb2plY3RNRE1lbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxhZGRyZXNzIjoidGVzdFByb2plY3RNRE1AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2dpdmVubmFtZSI6InRlc3QiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdCBNRE0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL29uZVRpbWVQYXNzd29yZCI6bnVsbCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9yb2xlIjoiU3Vic2NyaWJlcixwdWJsaXNoZXIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3V1aWQiOiI5OGNkYjYxOS1iMDZiLTQwODQtYjdjNS01ZGNjY2M4MmFmM2IifQ.bjA1V7k2aP-hPKGcAz8Gq9nEvQrkYvzmkpDOlidOz9I";      
      request.Headers["X-VisionLink-CustomerUid"] = customerUid; //"87bdf851-44c5-e311-aa77-00505688274d";
      if (payloadData != null)
      {
        request.ContentType = "multipart/form-data; boundary=" + BOUNDARY_START;
        var writeStream = request.GetRequestStreamAsync().Result;
        UTF8Encoding encoding = new UTF8Encoding();
        byte[] bytes = encoding.GetBytes(payloadData);
        writeStream.Write(bytes, 0, bytes.Length);
      }

      try
      {
        string responseString;
        using (var response = (HttpWebResponse) request.GetResponseAsync().Result)
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
          var webException = (WebException) e;
          var response = webException.Response as HttpWebResponse;
          if (response == null) continue;
          var resp = GetStringFromResponseStream(response);
          return resp;
        }
        return string.Empty;
      }
      
    }

    /// <summary>
    /// File upload
    /// </summary>
    /// <param name="filestream"></param>
    /// <returns></returns>
    private FlowFileUpload SetAllAttributesForFlowFile(FileStream filestream)
    {
      var name = new DirectoryInfo(filestream.Name).Name;
      var flowFileUpload = new FlowFileUpload
      {
        flowChunkNumber = 1,
        flowChunkSize = 1048576,
        flowCurrentChunkSize = filestream.Length,
        flowTotalSize = filestream.Length,
        flowIdentifier = filestream.Length + "-" + name.Replace(".",""),
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
    private string FormatTheContentDisposition(FlowFileUpload flowFileUpload, FileStream filestream)
    {
      var sb = new StringBuilder();
      var name = new DirectoryInfo(filestream.Name).Name;
      sb.AppendLine();
      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowChunkNumber\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowChunkNumber.ToString());

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowChunkSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowChunkSize.ToString());

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowCurrentChunkSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowCurrentChunkSize.ToString());

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowTotalSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowTotalSize.ToString());

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowIdentifier\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowIdentifier);

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowFilename\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowFilename);

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowRelativePath\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowRelativePath);

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"flowTotalChunks\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowTotalChunks.ToString());

      sb.AppendLine(BOUNDARY);
      sb.AppendLine("Content-Disposition: form-data; name=\"file\"; filename=\"" + name + "\"");
      sb.AppendLine("Content-Type: application/octet-stream");
      sb.AppendLine();

      StreamReader reader = new StreamReader(filestream);
      sb.Append(reader.ReadToEnd());
      sb.AppendLine();
      sb.AppendLine(BOUNDARY + "--");
      return sb.ToString();
    }

    /// <summary>
    /// Get the HTTP Response from the response stream and store in a string variable
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private string GetStringFromResponseStream(HttpWebResponse response)
    {
      var readStream = response.GetResponseStream();

      if (readStream != null)
      {
        var reader = new StreamReader(readStream, Encoding.UTF8);
        var responseString = reader.ReadToEnd();
        return responseString;
      }
      return string.Empty;
    }


    /// <summary>
    /// Call the web api for the imported files
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="method"></param>
    /// <param name="configJson"></param>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    private string CallWebApi(string uri, string method, string configJson, string customerUid = null)
    {
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json",
        customerUid);
      return response;
    }


  }

  /// <summary>
  /// Import file type - Copied from the repos's 
  /// </summary>
  public enum ImportedFileType
  {
    Linework = 0,
    DesignSurface = 1,
    SurveyedSurface = 2,
    Alignment = 3,
    MobileLinework = 4,
    SiteBoundary = 5,
    ReferenceSurface = 6,
    MassHaulPlan = 7
  }
}
