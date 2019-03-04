using System.Collections.Generic;

namespace VSS.WebApi.Common
{
  public interface ITPaaSApplicationAuthentication
  {
    string GetApplicationBearerToken();
    IDictionary<string, string> CustomHeaders();
  }
}
