using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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

    public ImportedFileDescriptorListResult expectedImportFileDescriptorsListResult = new ImportedFileDescriptorListResult
      ()
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
    /// Post 
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="customerUid"></param>
    /// <param name="projectUid"></param>
    /// <param name="fileName"></param>
    public ImportedFileDescriptorSingleResult PostImportedFilesToWebApi(string uri, Guid customerUid, string projectUid,
      string fileName)
    {
      var response = UploadFilesToWebApi(fileName, uri, customerUid.ToString());
      var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response);
      return filesResult;
    }

    private FlowFileUpload SetAllAttributesForFlowFile(FileStream filestream)
    {
      var flowFileUpload = new FlowFileUpload
      {
        flowChunkNumber = 1,
        flowChunkSize = 1048576,
        flowCurrentChunkSize = filestream.Length,
        flowTotalSize = filestream.Length,
        flowIdentifier = filestream.Length + filestream.Name,
        flowFilename = filestream.Name,
        flowRelativePath = filestream.Name,
        flowTotalChunks = 1
      };
      return flowFileUpload;
    }

    private MultipartFormDataContent FormatTheContentDisposition(FlowFileUpload flowFileUpload, FileStream filestream)
    {
      var content = new MultipartFormDataContent();
      var sb = new StringBuilder();
      var encoding = Encoding.UTF8;
      sb.AppendLine("Content-Disposition: form-data; name=\"flowChunkNumber\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowChunkNumber.ToString());

      sb.AppendLine("Content-Disposition: form-data; name=\"flowChunkSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowChunkSize.ToString());

      sb.AppendLine("Content-Disposition: form-data; name=\"flowCurrentChunkSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowCurrentChunkSize.ToString());

      sb.AppendLine("Content-Disposition: form-data; name=\"flowTotalSize\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowTotalSize.ToString());

      sb.AppendLine("Content-Disposition: form-data; name=\"flowIdentifier\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowIdentifier);

      sb.AppendLine("Content-Disposition: form-data; name=\"flowFilename\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowFilename);

      sb.AppendLine("Content-Disposition: form-data; name=\"flowRelativePath\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowRelativePath);

      sb.AppendLine("Content-Disposition: form-data; name=\"flowTotalChunks\"");
      sb.AppendLine();
      sb.AppendLine(flowFileUpload.flowTotalChunks.ToString());

      sb.AppendLine("Content-Disposition: form-data; name=\"file\"; filename=\"" + filestream.Name + "\"");
      sb.AppendLine("Content-Type: application/octet-stream");
      sb.AppendLine();
      MemoryStream stream = new MemoryStream();
      StreamWriter writer = new StreamWriter(stream);
      writer.Write(sb.ToString());
      writer.Flush();
      filestream.CopyTo(stream);
      content.Add(new StreamContent(stream));
      return content;
    }

    public string UploadFilesToWebApi(string fullFileName, string uri, string customerUid)
    {
      try
      {
        var message = new HttpRequestMessage();
        var filestream = new FileStream(fullFileName, FileMode.Open);
        var flowFileUpload = SetAllAttributesForFlowFile(filestream);
        var content = FormatTheContentDisposition(flowFileUpload, filestream);
        var response = DoHttpRequest(uri, "POST", content, HttpStatusCode.OK,"multipart/form-data", customerUid);
        //message.Method = HttpMethod.Post;
        //message.Content = content;
        //message.RequestUri = new Uri(uri); //"http://localhost:3128/api/uploading/");
        //message.Headers.Add("X-JWT-Assertion","eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=");
        //message.Headers.Add("X-VisionLink-CustomerUid",customerUid);
        //var client = new HttpClient();
        //var response = client.SendAsync(message);
        //response.Wait();
        return response.ToString();
      }
      catch (Exception ex)
      {
        return ex.Message;
      }
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


    public string DoHttpRequest(string resourceUri, string httpMethod, MultipartFormDataContent content,HttpStatusCode httpResponseCode = HttpStatusCode.OK, string mediaType = "multipart/form-data;",string customerUid = null)
    {
      Log.Info(resourceUri, Log.ContentType.ApiSend);
      var msg = new Msg();
      var request = InitHttpRequest(resourceUri, httpMethod, mediaType, customerUid); //Initialize the Http Request
      //if (payloadData != null)
      //{
      //  request.ContentType = mediaType;
      //  var writeStream = request.GetRequestStreamAsync().Result;
      //  UTF8Encoding encoding = new UTF8Encoding();
      //  byte[] bytes = encoding.GetBytes(payloadData);
      //  writeStream.Write(bytes, 0, bytes.Length);
      //}

      //Validate the HTTP Response Status Codes for Successful POST HTTP Request
      try
      {
        string responseString = null;
        using (var response = (HttpWebResponse) request.GetResponseAsync().Result)
        {
          responseString = GetStringFromResponseStream(response);
          //  msg.DisplayWebApi(httpMethod, resourceUri, responseString, payloadData);
          Assert.AreEqual(httpResponseCode, response.StatusCode,
            "Expected this response code, " + httpResponseCode + ", but the actual response code was this instead, " +
            response.StatusCode);
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
          //msg.DisplayWebApi(httpMethod, resourceUri, resp, payloadData);      
          return resp;
        }
        msg.DisplayException(ex.Message);
        return string.Empty;
      }
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
    /// Overloaded (no auth): This method sets the Http Request Method, Header, Media Type, and Authentication
    /// </summary>
    /// <param name="resourceUri">This is the resource on the endpoint.This includes the full endpoint URI.</param>
    /// <param name="httpMethod">This is the HTTP Method: GET, PUT, POST</param>
    /// <param name="mediaType">This is the mediaType of the http request which can be json or xml </param>
    /// <param name="customerUid">This is the customer UID for the header for authentication </param>
    /// <returns>This returns the HTTP request</returns>
    private HttpWebRequest InitHttpRequest(string resourceUri, string httpMethod, string mediaType, string customerUid)
    {
      //Initialize the Http Request
      var request = (HttpWebRequest) WebRequest.Create(resourceUri);
      request.Method = httpMethod;
      request.Accept = mediaType;
      //Hardcode authentication for now
      request.Headers["X-JWT-Assertion"] =
        "eyJ0eXAiOiJKV1QiLCJhbGciOiJTSEEyNTZ3aXRoUlNBIiwieDV0IjoiWW1FM016UTRNVFk0TkRVMlpEWm1PRGRtTlRSbU4yWmxZVGt3TVdFelltTmpNVGt6TURFelpnPT0ifQ==.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6IjE0NTU1Nzc4MjM5MzAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N1YnNjcmliZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoxMDc5LCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9ubmFtZSI6IlV0aWxpemF0aW9uIERldmVsb3AgQ0kiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2FwcGxpY2F0aW9udGllciI6IiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBpY29udGV4dCI6Ii90L3RyaW1ibGUuY29tL3V0aWxpemF0aW9uYWxwaGFlbmRwb2ludCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdmVyc2lvbiI6IjEuMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvdGllciI6IlVubGltaXRlZCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3VzZXJ0eXBlIjoiQVBQTElDQVRJT04iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VuZHVzZXIiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbWFpbGFkZHJlc3MiOiJjbGF5X2FuZGVyc29uQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9naXZlbm5hbWUiOiJDbGF5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9sYXN0bmFtZSI6IkFuZGVyc29uIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9vbmVUaW1lUGFzc3dvcmQiOm51bGwsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvcm9sZSI6IlN1YnNjcmliZXIscHVibGlzaGVyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMjM4ODY5YWYtY2E1Yy00NWUyLWI0ZjgtNzUwNjE1YzhhOGFiIn0=.kTaMf1IY83fPHqUHTtVHn6m6aQ9wFch6c0FsNDQ7x1k=";
      if (!string.IsNullOrEmpty(customerUid))
      {
        request.Headers["X-VisionLink-CustomerUid"] = customerUid;
      }
      return request;
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
