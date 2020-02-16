using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using log4net;
using VSS.Hosted.VLCommon;
using VSS.Hosted.VLCommon.Services.MDM.Interfaces;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Common
{
  public class HttpRequestWrapper : IHttpRequestWrapper
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
    public ServiceResponseMessage RequestDispatcher(ServiceRequestMessage svcRequestMessage)
    {
      HttpRequestMessage request = new HttpRequestMessage(); 
      try
      {
        using (var client = new HttpClient())
        {
          request = new HttpRequestMessage
          {
            Method = svcRequestMessage.RequestMethod,
            RequestUri = svcRequestMessage.RequestUrl
          };

          if (!string.IsNullOrEmpty(svcRequestMessage.RequestPayload))
          {
            if (string.IsNullOrWhiteSpace(svcRequestMessage.RequestContentType))
            {
              svcRequestMessage.RequestContentType = StringConstants.JsonContentType;
            }
            if (svcRequestMessage.RequestEncoding == null)
            {
              svcRequestMessage.RequestEncoding = Encoding.UTF8;
            }
            request.Content = new StringContent(svcRequestMessage.RequestPayload, svcRequestMessage.RequestEncoding, svcRequestMessage.RequestContentType);
          }
          else
          {
            request.Content = new StringContent("", svcRequestMessage.RequestEncoding, svcRequestMessage.RequestContentType);
          }
          if (request.Method == null || request.RequestUri == null)
          {
            Log.IfError("Invalid Request Method/RequestUri");
            return new ServiceResponseMessage
            {
              StatusCode = HttpStatusCode.BadRequest
            };
          }
          if (svcRequestMessage.RequestHeaders != null && svcRequestMessage.RequestHeaders.Count > 0)
          {
            foreach (var header in svcRequestMessage.RequestHeaders)
              request.Headers.Add(header.Key, header.Value);
          }
          var response = client.SendAsync(request).Result;

          return new ServiceResponseMessage
          {
            Content = response.Content,
            StatusCode = response.StatusCode,
            ContentType = response.Content.Headers.ContentType
          };
        }
      }
      catch (OutOfMemoryException ex)
      {
        Log.IfError(string.Format("Exception Source {0} \n Message {1} Stack Trace {2}",ex.Source,ex.Message,ex.StackTrace));
      }
      catch (Exception ex)
      {
        Log.IfError(string.Format("Event Data {0} \n Request Type {1}  \n {2} \n {3}", request.Content, request.Method, ex.Message, ex.StackTrace));
      }
      return new ServiceResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError
      };
    }
  }
}
