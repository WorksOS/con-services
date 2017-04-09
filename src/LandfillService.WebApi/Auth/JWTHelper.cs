using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace VSS.VisionLink.Utilization.WebApi.Helpers
{
  #region Namespaces

  

  #endregion

  /// <summary>
  ///   Model class for Jwt
  /// </summary>
  public class Jwt
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


  /// <summary>
  /// 
  /// </summary>
  public class JwtHelper
  {
    /// <summary>
    ///   This method is used to get the Jwt Assertion Token string from the HTTP Request Header
    /// </summary>
    /// <param name="httpRequestHeaders">Incoming Request Headers</param>
    /// <param name="jwtToken">Output parameter - Jwt Assetion Token string</param>
    /// <returns>true, if Http Headers contain Jwt; false, otherwise</returns>
    public static bool TryGetJwtToken(HttpRequestHeaders httpRequestHeaders, out string jwtToken)
    {
      jwtToken = null;
      try
      {
        if (httpRequestHeaders.Contains("X-Jwt-Assertion"))
        {
          //if present read the first elemeent from HTTP Request Header X-Jwt-Assertion
          jwtToken = httpRequestHeaders.GetValues("X-Jwt-Assertion").FirstOrDefault();
          return true;
        }
        //If no X-Jwt-Assertion header in the request, then return false with null jwtToken output param
        return false;
      }
      catch
      {
        //If any exceptions in getting X-Jwt-Assertion header, then return false with null jwtToken output param
        return false;
      }
    }

    /// <summary>
    ///   This method is used to check if the Jwt Token is valid or not
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <returns>true, if the Jwt Token is valid; false, otherwise</returns>
    public static bool IsValidJwtToken(string jwtToken)
    {
      var isValidJwtToken = false;

      if (!string.IsNullOrEmpty(jwtToken))
      {
        var count = jwtToken.Count(x => x == '.');

        if (count == 2)
        {
          // split the jwtToken into parts
          var jwtTokenparts = jwtToken.Split('.');

          // set isValidjwtToken to true if the jwtToken has three parts
          //Commented sections below are a result of changes in #TPAAS-4770 the length of the token is now variable
          //this is a temporary fix while a more permanent soultion is developed.
          isValidJwtToken = jwtTokenparts.Length == 3; //&&
                           // (jwtTokenparts[0].Length%4 == 0 && jwtTokenparts[1].Length%4 == 0 &&
                           //  jwtTokenparts[2].Length%4 == 0);
        }
      }
      return isValidJwtToken;
    }

    /// <summary>
    ///   This method is used to decode the validated jwtToken and return the Jwt object
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <returns>returns null if jwtToken is null/invalid else, returns the Jwt object</returns>
    public static Jwt DecodeJwtToken(string jwtToken)
    {
      Jwt jwt = null;

      if (!string.IsNullOrEmpty(jwtToken))
      {
        // Split the jwtToken into header, claims and signature parts
        var jwtTokenparts = jwtToken.Split('.');

        //select the claims part from jwtTokenParts
        var claimData = jwtTokenparts[1];

        //Tempory fix to handle changes in #TPAAS-4770, token length is now variable however 
        // to decode successfully claimData % 4 == 0 must be true therefore padding is added
        claimData = claimData.PadRight(claimData.Length + (4 - claimData.Length % 4), '=');


        // Convert the claimsPart to Base64 format
        var base64ClaimData = Convert.FromBase64String(claimData);

        // Encode the base64 claim data to UTF8 format string
        var encodedClaimData = Encoding.UTF8.GetString(base64ClaimData);

        try
        {
          // Convert the encoded Claim Data Jwt object
          jwt = JsonConvert.DeserializeObject<Jwt>(encodedClaimData);
        }
        catch (Exception ex)
        {
          // decodedClaimJson will be null in case of exceptions   
          throw new FormatException("invalid Jwt Token", ex);
        }
      }
      return jwt;
    }
  }
}