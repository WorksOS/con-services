using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ProjectWebApiCommon.Models;
using TestUtility.Model;

namespace TestUtility
{
  public class ImportFile
  {

    public ImportedFileDescriptor importFileDescriptor = new ImportedFileDescriptor();
    public ImportedFileDescriptorSingleResult expectedImportFileDescriptorSingleResult;
    public string importedFileUid;
    private const string BOUNDARY = "------WebKitFormBoundarym45GFZc25WVhjtVB";
    private const string BOUNDARY_START = "----WebKitFormBoundarym45GFZc25WVhjtVB";

    public ImportedFileDescriptorListResult expectedImportFileDescriptorsListResult = new ImportedFileDescriptorListResult()
    { ImportedFileDescriptors = ImmutableList<ImportedFileDescriptor>.Empty };


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
    public ImportedFileDescriptorListResult GetImportedFilesFromWebApi(string uri, Guid customerUid)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorListResult>(response);
      return filesResult;
    }

    public T GetFromWebApi<T>(string uri, Guid customerUid)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<T>(response);
      return filesResult;
    }

    /// <summary>
    /// Add a string array of data 
    /// </summary>
    /// <param name="ts">Test support</param>
    /// <param name="importFileArray">string array of data</param>
    /// <param name="row">Add a single row at a time</param>
    /// <param name="method">HTTP methodf</param>
    /// <returns></returns>
    public ImportedFileDescriptorSingleResult SendToImportedFilesToWebApi(TestSupport ts, string[] importFileArray, int row, string method = "POST")
    {
      var uri = ts.GetBaseUri();

      var ed = ts.ConvertImportFileArrayToObject(importFileArray, row);
      ed.FileCreatedUtc = ed.FileCreatedUtc;
      expectedImportFileDescriptorSingleResult.ImportedFileDescriptor = ed;
      var createdDt = ed.FileCreatedUtc.ToUniversalTime().ToString("o");
      var updatedDt = ed.FileUpdatedUtc.ToUniversalTime().ToString("o");
      uri = uri + $"api/v4/importedfile?projectUid={ed.ProjectUid}&importedFileType={ed.ImportedFileTypeName}&fileCreatedUtc={createdDt:yyyy-MM-ddTHH:mm:ss.fffffff}&fileUpdatedUtc={updatedDt:yyyy-MM-ddTHH:mm:ss.fffffff}";
      if (ed.ImportedFileTypeName == "SurveyedSurface")
      {
        uri = uri + $"&SurveyedUtc={ed.SurveyedUtc:yyyy-MM-ddTHH:mm:ss.fffffff} ";
      }
      if (method == "DELETE")
      {
        uri = ts.GetBaseUri() + $"api/v4/importedfile?projectUid={ed.ProjectUid}&importedFileUid={importedFileUid}";
      }
      var response = UploadFilesToWebApi(ed.Name, uri, ed.CustomerUid, method);
      expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name = Path.GetFileName(expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.Name);  // Change expected result
      expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.FileCreatedUtc = expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.FileCreatedUtc.ToUniversalTime();
      expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.FileUpdatedUtc = expectedImportFileDescriptorSingleResult.ImportedFileDescriptor.FileUpdatedUtc.ToUniversalTime();
      try
      {
        var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response);
        return filesResult;
      }
      catch (Exception)
      {
        Console.WriteLine(response);
        Assert.Fail(response);
        return null;
      }
    }

    /// <summary>
    /// Generate a stream from a string
    /// </summary>
    /// <param name="s"></param>
    /// <returns>stream</returns>
    public Stream GenerateStreamFromString(string s)
    {
      MemoryStream stream = new MemoryStream();
      StreamWriter writer = new StreamWriter(stream);
      writer.Write(s);
      writer.Flush();
      stream.Position = 0;
      return stream;
    }


    /// <summary>
    /// Upload a single file to the web api 
    /// </summary>
    /// <param name="fullFileName">Full filename</param>
    /// <param name="uri">Full uri to send it to</param>
    /// <param name="customerUid">Customer Uid</param>
    /// <param name="method">HTTP method</param>
    /// <returns>Repsonse from web api as string</returns>
    public string UploadFilesToWebApi(string fullFileName, string uri, string customerUid, string method)
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
          var response = DoHttpRequest(uri, method, content, customerUid);

          return response;
        }
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
    /// <param name="contentType"></param>
    /// <returns></returns>
    public string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, string customerUid = null, string contentType = null)
    {
      var request = WebRequest.Create(resourceUri) as HttpWebRequest;
      if (request == null)
      { return string.Empty; }
      request.Method = httpMethod;
      request.Headers["X-JWT-Assertion"] = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IkNvbXBhY3Rpb24tRGV2ZWxvcC1DSSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3lEZXRhaWxzIjoiZXlKMWNHUmhkR1ZrVkdsdFpTSTZNVFE1TVRFM01ERTROamszTWl3aWFHbHpkRzl5ZVNJNld5STJOVE5pWmpJeU9EZzJOamM1TldVd05ERTVNakEyTnpFMFkyVXpNRFpsTURNeVltUXlNalppWkRVMFpqUXpOamcxTkRJME5UZGxaVEl4TURnMU5UQXdJaXdpTWpFMk56ZG1OemxpTlRWbVpqY3pOamxsTVdWbU9EQmhOV0V3WVRGaVpXSTRNamcwWkdJME16WTVNekEzT1RreFpUbGpaRFUzTkRnMk16VmpZVGRsTWlJc0ltTTVOVEF3TURaak5USXpaV0kxT0RkaFpHRXpNRFUxTWpJMFlXUmxabUUzTjJJeE1EYzJZV1JsT1RnMk1qRTBaakpqT0RJek1qWTRNR1l5TnprMk1EVWlYWDA9IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9rZXl0eXBlIjoiUFJPRFVDVElPTiIsInNjb3BlcyI6Im9wZW5pZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW1haWxWZXJpZmllZCI6InRydWUiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT05fVVNFUiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6InB1Ymxpc2hlciIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdFVwZGF0ZVRpbWVTdGFtcCI6IjE0OTcyNzgyMDQ5MjIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FjY291bnR1c2VybmFtZSI6IkRhdmlkX0dsYXNzZW5idXJ5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYWNjb3VudG5hbWUiOiJ0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZmlyc3RuYW1lIjoiVGVzdCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcGFzc3dvcmRQb2xpY3kiOiJISUdIIiwiaXNzIjoid3NvMi5vcmcvcHJvZHVjdHMvYW0iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RuYW1lIjoiUHJvamVjdE1ETSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb25pZCI6IjM3NDMiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJ0ZXN0UHJvamVjdE1ETUB0cmltYmxlLmNvbSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdXVpZCI6Ijk4Y2RiNjE5LWIwNmItNDA4NC1iN2M1LTVkY2NjYzgyYWYzYiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZW5kdXNlclRlbmFudElkIjoiMSIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiRGF2ZSIsImV4cCI6MTQ5ODE4MTI0NCwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvaWRlbnRpdHkvYWNjb3VudExvY2tlZCI6ImZhbHNlIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWRldi1wcm9qZWN0cyIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ5ODE2NTAxOTM3MCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2xhc3RQd2RTZXRUaW1lU3RhbXAiOiIxNDkxMTcwMTg3Mjk3IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6InRlc3RQcm9qZWN0TURNQHRyaW1ibGUuY29tIiwianRpIjoiYTU3ZTYwYWQtY2YzNC00YzY4LTk0YmQtOTQxY2E1NWFkMTVhIiwiaWF0IjoxNDk4MTc3NDc5fQ.cTQq_4hmspQ9ojOXeau1q4ZywCwwC2fIOkY_tESA5FU";
      request.Headers["X-VisionLink-CustomerUid"] = customerUid; //"87bdf851-44c5-e311-aa77-00505688274d";
      request.Headers["X-VisionLink-ClearCache"] = "true";
      if (payloadData != null)
      {
        request.ContentType = contentType ?? "multipart/form-data; boundary=" + BOUNDARY_START;
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

    /// <summary>
    /// Send a PUT request to the api/v4 API.
    /// </summary>
    public object SendPutRequestToWebApi(TestSupport ts, string projectUid, string requestBody, string method = "PUT")
    {
      var uri = ts.GetBaseUri() + $"api/v4/importedfile?projectUid={projectUid}";
      var response = DoHttpRequest(uri, "PUT", requestBody);

      try
      {
        return JsonConvert.DeserializeObject(response);
      }
      catch (Exception)
      {
        Console.WriteLine(response);
        Assert.Fail(response);
      }

      return null;
    }

    /// <summary>
    /// File upload
    /// </summary>
    /// <param name="filestream"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private static FlowFileUpload SetAllAttributesForFlowFile(Stream filestream, string name)
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
    private static string FormatTheContentDisposition(FlowFileUpload flowFileUpload, Stream filestream, string name)
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
    /// Get the HTTP Response from the response stream and store in a string variable
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    private string GetStringFromResponseStream(HttpWebResponse response)
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
    /// <param name="uri"></param>
    /// <param name="method"></param>
    /// <param name="configJson"></param>
    /// <param name="customerUid"></param>
    /// <returns></returns>
    private static string CallWebApi(string uri, string method, string configJson, string customerUid = null)
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
