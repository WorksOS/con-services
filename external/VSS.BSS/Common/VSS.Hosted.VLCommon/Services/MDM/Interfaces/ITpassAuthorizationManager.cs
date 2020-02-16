using System.Collections.Generic;
using VSS.Hosted.VLCommon.Services.MDM.Models;

namespace VSS.Hosted.VLCommon.Services.MDM.Interfaces
{
  public interface ITpassAuthorizationManager
  {
    KeyValuePair<string, string> GetBearerAuthHeader(string bearerToken);
    KeyValuePair<string, string> GetBasicAuthHeader(AppCredentials appCredentials);
    string GetNewToken(AppCredentials appCredentials, string tokenBaseUrl);
    //bool ValidateTokenCredentials(TokenCredentials credentials);
    bool ValidateAppCredentials(AppCredentials credentials);
  }
}
