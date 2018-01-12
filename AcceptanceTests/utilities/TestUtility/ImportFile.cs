using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TestUtility.Model;
using VSS.MasterData.Project.WebAPI.Common.Models;

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

    public readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
      DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
      NullValueHandling = NullValueHandling.Ignore
    };

    public ImportFile()
    {
      expectedImportFileDescriptorSingleResult = new ImportedFileDescriptorSingleResult(importFileDescriptor);
    }

    /// <summary>
    /// Gets a list of imported files for a project. The list includes files of all types.
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="customerUid"></param>
    public ImportedFileDescriptorListResult GetImportedFilesFromWebApi(string uri, Guid customerUid, string jwt = null)
    {
      var response = CallWebApi(uri, HttpMethod.Get.ToString(), null, customerUid.ToString(), jwt);
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
      if (ed.ImportedFileTypeName == "Linework")
      {
        uri = uri + $"&DxfUnitsType={ed.DxfUnitsType} ";
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
        var filesResult = JsonConvert.DeserializeObject<ImportedFileDescriptorSingleResult>(response, jsonSettings);
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
    public string DoHttpRequest(string resourceUri, string httpMethod, string payloadData, string customerUid = null, string contentType = null, string jwt = null)
    {
      var request = WebRequest.Create(resourceUri) as HttpWebRequest;
      if (request == null)
      { return string.Empty; }
      request.Method = httpMethod;
      request.Headers["X-JWT-Assertion"] = jwt ?? RestClientUtil.DEFAULT_JWT;
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
        flowIdentifier = filestream.Length + "-" + name.Replace(".", string.Empty),
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
    /// <param name="jwt"></param>
    /// <returns></returns>
    private static string CallWebApi(string uri, string method, string configJson, string customerUid = null, string jwt = null)
    {
      var restClient = new RestClientUtil();
      var response = restClient.DoHttpRequest(uri, method, configJson, HttpStatusCode.OK, "application/json",
        customerUid, jwt);
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
