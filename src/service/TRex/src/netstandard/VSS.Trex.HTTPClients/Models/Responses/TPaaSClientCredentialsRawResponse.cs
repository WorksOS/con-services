using System;
using System.Collections.Generic;
using System.Text;
using VSS.Trex.HTTPClients.Abstractions;

namespace VSS.Trex.HTTPClients.Models.Responses
{
  public class TPaaSClientCredentialsRawResponse : ITPaaSClientCredentialsRawResponse
  {
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int TokenExpiry { get; set; }
  }
}
