using System.Net.Http;

namespace VSS.TRex.HttpClients.Models
{
  public class TPaaSAuthenticationException : HttpRequestException
  {
    public TPaaSAuthenticationException(string message) :
      base(message)
    { }

    public TPaaSAuthenticationException(string message, HttpResponseMessage result) :
      base($"{message}. {System.Environment.NewLine} Result was: {System.Environment.NewLine} {result.ToString()}" )
    {
    }
  }
}
