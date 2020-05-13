using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace VSS.WebApi.Common
{
  public interface ITPaaSApplicationAuthentication
  {
    string GetApplicationBearerToken();
    IHeaderDictionary CustomHeaders();
    IHeaderDictionary CustomHeadersJWT();
    IHeaderDictionary CustomHeadersJWTAndBearer();
  }
}
