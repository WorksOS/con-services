using VSS.Tpaas.Client.Abstractions;

namespace VSS.Tpaas.Client.Models.Responses
{
  public class TPaaSClientCredentialsRawResponse : ITPaaSClientCredentialsRawResponse
  {
    public string AccessToken { get; set; }
    public string TokenType { get; set; }
    public int TokenExpiry { get; set; }
  }
}
