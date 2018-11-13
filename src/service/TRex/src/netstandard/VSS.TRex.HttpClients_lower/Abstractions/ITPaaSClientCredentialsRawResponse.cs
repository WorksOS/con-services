using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VSS.TRex.HttpClients.Abstractions
{
  interface ITPaaSClientCredentialsRawResponse
  {

    [JsonProperty("access_token")]
    string AccessToken { get; set; }

    [JsonProperty("token_type")]
    string TokenType { get; set; }

    [JsonProperty("expires_in")]
    int TokenExpiry { get; set; }
    
  }
}
