﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.WebApi.Common;

namespace TCCToDataOcean
{
  public class RestClient : IRestClient
  {
    private readonly HttpClient httpClient;
    private readonly ILogger Log;

    private static HttpRequestMessage GetRequestMessage(HttpMethod method, string uri) => new HttpRequestMessage(method, new Uri(uri));

    public RestClient(ILoggerFactory loggerFactory, ITPaaSApplicationAuthentication authentication)
    {
      Log = loggerFactory.CreateLogger<HttpClient>();
      Log.LogInformation(Method.In());

      var bearerToken = authentication.GetApplicationBearerToken();

      httpClient = new HttpClient();
      httpClient.DefaultRequestHeaders.Add("pragma", "no-cache");
      httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
    }

    public async Task<TResponse> SendHttpClientRequest<TResponse>(string uri, HttpMethod method, string acceptHeader, string contentType, string customerUid, string payloadData = null) where TResponse : class
    {
      Log.LogInformation($"{Method.In()} URI: {uri}");

      var request = GetRequestMessage(method, uri);

      request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptHeader));
      request.Headers.Add("X-VisionLink-CustomerUid", customerUid);

      if (payloadData != null)
      {
        switch (acceptHeader)
        {
          case MediaType.APPLICATION_JSON:
            {
              request.Content = new StringContent(payloadData, Encoding.UTF8, contentType);
              break;
            }
          //case MediaType.MULTIPART_FORM_DATA:
          //  {
          //    contentType = $"{MediaType.MULTIPART_FORM_DATA}; boundary=-----{Guid.NewGuid().ToString()}";
          //    throw new NotImplementedException();
          //  }
          default:
            {
              throw new Exception($"Unsupported content type '{contentType}'");
            }
        }
      }

      try
      {
        var response = await httpClient.SendAsync(request);

        var receiveStream = response.Content.ReadAsStreamAsync().Result;
        var readStream = new StreamReader(receiveStream, Encoding.UTF8);
        var responseBody = readStream.ReadToEnd();

        if (response.StatusCode == HttpStatusCode.OK)
        {
          Log.LogInformation($"{Method.Info()} Status [{response.StatusCode}] Body: '{responseBody}'");
        }
        else Log.LogDebug($"{Method.Info()} Status [{response.StatusCode}] URI: '{request.RequestUri.AbsoluteUri}', Body: '{responseBody}'");

        switch (response.Content.Headers.ContentType.MediaType)
        {
          case MediaType.APPLICATION_JSON:
            {
              return JsonConvert.DeserializeObject<TResponse>(responseBody);
            }
          case MediaType.TEXT_PLAIN:
          case MediaType.APPLICATION_OCTET_STREAM:
            {
              return await response.Content.ReadAsStringAsync() as TResponse;
            }
          default:
            {
              throw new Exception($"Unsupported content type '{response.Content.Headers.ContentType.MediaType}'");
            }
        }
      }
      catch (Exception exception)
      {
        Log.LogError($"{Method.Info("ERROR")} URI: '{request.RequestUri.AbsoluteUri}', Exception: {exception.GetBaseException()}");
      }

      return null;
    }
  }
}