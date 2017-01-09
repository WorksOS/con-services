using System;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace VSS.Project.Service.WebApi.Authentication
{
  /// <summary>
  ///   Model class for Jwt
  /// </summary>
  internal class Jwt
  {
    /// <summary>
    ///   Iss
    /// </summary>
    [JsonProperty(PropertyName = "iss")]
    public string Iss { get; set; }

    /// <summary>
    ///   Exp
    /// </summary>
    [JsonProperty(PropertyName = "exp")]
    public string Exp { get; set; }

    /// <summary>
    ///   Subscriber
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/subscriber")]
    public string Subscriber { get; set; }

    /// <summary>
    ///   ApplicationId
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/applicationid")]
    public int ApplicationId { get; set; }

    /// <summary>
    ///   Application Name
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/applicationname")]
    public string ApplicationName { get; set; }

    /// <summary>
    ///   Application Tier
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/applicationtier")]
    public string ApplicationTier { get; set; }

    /// <summary>
    ///   Api Context
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/apicontext")]
    public string ApiContext { get; set; }

    /// <summary>
    ///   Version
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/version")]
    public string Version { get; set; }

    /// <summary>
    ///   Tier
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/tier")]
    public string Tier { get; set; }

    /// <summary>
    ///   Key Type
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/keytype")]
    public string KeyType { get; set; }

    /// <summary>
    ///   User Type
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/usertype")]
    public string UserType { get; set; }

    /// <summary>
    ///   EndUser
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/enduser")]
    public string EndUser { get; set; }

    /// <summary>
    ///   Tenant Id of the End User
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/enduserTenantId")]
    public string EndUserTenantId { get; set; }

    /// <summary>
    ///   Email
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/emailaddress")]
    public string Email { get; set; }

    /// <summary>
    ///   FirstName
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/givenname")]
    public string GivenName { get; set; }

    /// <summary>
    ///   Last Name
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/lastname")]
    public string LastName { get; set; }

    /// <summary>
    ///   OneTimePassword
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/oneTimePassword")]
    public string Otp { get; set; }

    /// <summary>
    ///   Roles of the User
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/role")]
    public string Role { get; set; }

    /// <summary>
    ///   Unique Id of the User
    /// </summary>
    [JsonProperty(PropertyName = "http://wso2.org/claims/uuid")]
    public string Uuid { get; set; }
  }

  public class JWTToken
  {
    private string jwtToken = String.Empty;
    private Jwt jwt = null;

    public bool IsValidToken { get; private set; } = false;
    public string UserUID { get; private set; }

    public bool SetToken(string validJwt)
    {
      jwtToken = validJwt;
      var isValidJwtToken = false;

      if (!string.IsNullOrEmpty(validJwt))
      {
        var count = validJwt.Count(x => x == '.');

        if (count == 2)
        {
          // split the jwtToken into parts
          var jwtTokenparts = validJwt.Split('.');

          // set isValidjwtToken to true if the jwtToken has three parts
          isValidJwtToken = jwtTokenparts.Length == 3 &&
                            (jwtTokenparts[0].Length % 4 == 0 && jwtTokenparts[1].Length % 4 == 0 &&
                             jwtTokenparts[2].Length % 4 == 0);
        }
      }

      if (!isValidJwtToken)
        return isValidJwtToken;

      if (DecodeJwtToken())
      {
        IsValidToken = isValidJwtToken;
        UserUID = jwt.Uuid;
      }

      return isValidJwtToken;
    }

    private bool DecodeJwtToken()
    {
      if (!string.IsNullOrEmpty(jwtToken))
      {
        try
        {
          // Split the jwtToken into header, claims and signature parts
          var jwtTokenparts = jwtToken.Split('.');

        //select the claims part from jwtTokenParts
        var claimData = jwtTokenparts[1];

          // Convert the claimsPart to Base64 format
          var base64ClaimData = Convert.FromBase64String(claimData);

        // Encode the base64 claim data to UTF8 format string
        var encodedClaimData = Encoding.UTF8.GetString(base64ClaimData);


          // Convert the encoded Claim Data Jwt object
          jwt = JsonConvert.DeserializeObject<Jwt>(encodedClaimData);
        }
        catch (Exception ex)
        {
          // decodedClaimJson will be null in case of exceptions   
          return false;
        }
      }
      return true;
    }
  }
}