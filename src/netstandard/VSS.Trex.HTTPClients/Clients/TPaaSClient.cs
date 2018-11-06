using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using VSS.Trex.HTTPClients.Abstractions;
using VSS.Trex.HTTPClients.Models.Responses;

namespace VSS.Trex.HTTPClients.Clients
{
  public class TPaaSClient
  {
    //private ILogger<ValuesClient> _logger;
    private HttpClient _client;

    private readonly int TOKEN_EXPIRY_GRACE_SECONDS = 60;
    private const string REVOKE_TOKEN_URI = "/revoke";
    private const string GET_TOKEN_URI = "/token";
    private static string TPaaSToken { get; set; } = string.Empty;
    private static string TokenType { get; set; } = string.Empty;
    private static DateTime TPaaSTokenExpiry = DateTime.MinValue;
    private readonly object lockToken = new object();

    public TPaaSClient(HttpClient client)
    {
      _client = client;
      //_logger = logger;
    }

    public string GetBearerToken()
    {
      if (TPaaSTokenExpiry < DateTime.Now)
      {
        //Only one request should update the token
        lock (lockToken)
        {
          //make sure it hasn't been updated if we have been waiting for a lock
          // if it is still expired contine and refresh.
          if (TPaaSTokenExpiry < DateTime.Now)
            RefreshAuthToken();
        }
      }
      return $"{TokenType} {TPaaSToken}";
    }


    /// <summary>
    /// Authenticate with TPaaS
    /// </summary>
    /// <returns>True if authentication was successful</returns>
    protected bool RefreshAuthToken()
    {
      RevokeBearerToken();
      var auth = Authenticate();
      TokenType = auth.TokenType;
      TPaaSToken = auth.AccessToken;
      TPaaSTokenExpiry = DateTime.Now.AddSeconds(auth.TokenExpiry - TOKEN_EXPIRY_GRACE_SECONDS);

      return true;
    }


    private ITPaaSClientCredentialsRawResponse Authenticate()
    {
      var grantMessage = new Dictionary<string, string>();
      grantMessage.Add("grant_type", "client_credentials");      
      var res = _client.PostAsync(GET_TOKEN_URI, new FormUrlEncodedContent(grantMessage));
      return JsonConvert.DeserializeObject<TPaaSClientCredentialsRawResponse>(res.Result.Content.ReadAsStringAsync().Result);
    }

    /// <summary>
    /// Invalidates the bearer token if there is a valid one stored.
    /// </summary>
    /// <returns>True if the bearer token is now invalid</returns>
    private bool RevokeBearerToken()
    {
      if (!string.IsNullOrEmpty(TPaaSToken))
      {
        var revokeMessage = new Dictionary<string, string>();
        revokeMessage.Add("token", TPaaSToken);

        var revokeBody = new FormUrlEncodedContent(revokeMessage);
        return _client.PostAsync(REVOKE_TOKEN_URI, revokeBody).Result.IsSuccessStatusCode;
      }
      return true;

    }
  }
}
