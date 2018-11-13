using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.HttpClients.Abstractions;

namespace VSS.TRex.HttpClients.Models.Responses
{
  public class TPaaSClientCredentialsRawResponse : ITPaaSClientCredentialsRawResponse
  {
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int TokenExpiry { get; set; }
  }
}
