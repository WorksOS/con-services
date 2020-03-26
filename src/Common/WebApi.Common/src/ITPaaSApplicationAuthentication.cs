using System.Collections.Generic;

namespace VSS.WebApi.Common
{
  public interface ITPaaSApplicationAuthentication
  {
    string GetApplicationBearerToken();
    string GetUserBearerToken(string grantType);

    IDictionary<string, string> CustomHeaders();
    IDictionary<string, string> CustomHeadersJWT();
    IDictionary<string, string> CustomHeadersJWTAndBearer();
  }
}
