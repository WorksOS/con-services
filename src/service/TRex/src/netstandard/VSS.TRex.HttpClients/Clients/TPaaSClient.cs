using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.HttpClients.Models;
using VSS.TRex.HttpClients.Models.Responses;
using VSS.TRrex.HttpClients.Abstractions;

namespace VSS.TRex.HttpClients.Clients
{
  public class TPaaSClient : ITPaaSClient
  {
    private ILogger<TPaaSClient> _logger;
    private HttpClient _client;

    private readonly int TOKEN_EXPIRY_GRACE_SECONDS = 60;
    private const string REVOKE_TOKEN_URI = "/revoke";
    private const string GET_TOKEN_URI = "/token";
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    private ITPaaSClientState _state;

    public const string TPAAS_AUTH_URL_ENV_KEY = "TPAAS_AUTH_URL";

    public TPaaSClient(HttpClient client, ILogger<TPaaSClient> logger)
    {
      _client = client;
      _logger = logger;
      _state = TPaaSClientState.Instance;
    }

    public async Task<string> GetBearerTokenAsync()
    {
      if (_state.TPaaSTokenExpiry < DateTime.Now)
      {
        _logger.LogInformation("TPaaS Token has expired retrieving a new one");
        //Only one request should update the token
        await semaphore.WaitAsync();
        try
        {
          {
            //make sure it hasn't been updated if we have been waiting for a lock
            // if it is still expired contine and refresh.
            if (_state.TPaaSTokenExpiry < DateTime.Now)
              await RefreshAuthToken().ConfigureAwait(false);
          }
        }
        finally
        {
          semaphore.Release();
        }
      }
      return $"{_state.TokenType} {_state.TPaaSToken}";
    }


    /// <summary>
    /// Authenticate with TPaaS
    /// </summary>
    /// <returns>True if authentication was successful</returns>
    private async Task RefreshAuthToken()
    {
      _logger.LogInformation("Refreshing TPaaS Token");
      await RevokeBearerToken().ConfigureAwait(false);
      await Authenticate().ConfigureAwait(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private async Task Authenticate()
    {
      _logger.LogInformation("Authenticating with TPaaS");
      var grantMessage = new Dictionary<string, string>
      {
        {"grant_type", "client_credentials"}
      };

      var result = await _client.PostAsync(GET_TOKEN_URI, new FormUrlEncodedContent(grantMessage)).ConfigureAwait(false);

      if (result.Content == null)
      {
        throw new TPaaSAuthenticationException(
          $"No content response from TPaaS while getting token. Result was: {Environment.NewLine} {result}");
      }

      var content = await result.Content.ReadAsStringAsync();
      var auth = JsonConvert.DeserializeObject<TPaaSClientCredentialsRawResponse>(content);
      _state.TokenType = auth.TokenType;
      _state.TPaaSToken = auth.AccessToken;
      _state.TPaaSTokenExpiry = DateTime.Now.AddSeconds(auth.TokenExpiry - TOKEN_EXPIRY_GRACE_SECONDS);

      _logger.LogInformation($"Authenticating with TPaaS was {(!result.IsSuccessStatusCode ? "not " : "")}successful");
    }

    /// <summary>
    /// Invalidates the bearer token if there is a valid one stored.
    /// </summary>
    /// <returns>True if the bearer token is now invalid</returns>
    private async Task RevokeBearerToken()
    {
      if (!string.IsNullOrEmpty(_state.TPaaSToken))
      {
        try
        {
          _logger.LogInformation("Constructing Revoke token request");
          var revokeMessage = new Dictionary<string, string>
          {
            {"token", _state.TPaaSToken}
          };

          var revokeBody = new FormUrlEncodedContent(revokeMessage);
          _logger.LogInformation("Sending Revoke token request");
          var result = await _client.PostAsync(REVOKE_TOKEN_URI, revokeBody);

          if (!result.IsSuccessStatusCode)
          {
            throw new TPaaSAuthenticationException("Error revoking access token", result);
          }
        }
        finally
        {
          _state.TPaaSToken = string.Empty;
          _state.TokenType = string.Empty;
          _state.TPaaSTokenExpiry = DateTime.MinValue;
        }
      }
    }

    public async Task SetState(ITPaaSClientState state)
    {
      //Don't change the state while setting it elsewhere
      await semaphore.WaitAsync();
      _state = state;
      semaphore.Release();
    }
  }
}
