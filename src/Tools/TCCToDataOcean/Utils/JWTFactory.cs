using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TCCToDataOcean.Utils
{
  public static class JWTFactory
  {
    private static readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;
    
    static JWTFactory()
    {
      _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
    }

    public static string CreateToken(string originalToken, string importedBy)
    {
      var tokenS = _jwtSecurityTokenHandler.ReadToken(originalToken) as JwtSecurityToken;
      var claims = new List<Claim>();

      foreach (var claim in tokenS.Claims)
      {
        if (claim.Type == "http://wso2.org/claims/applicationname")
        {
          // Setting the 'applicationname' claim to hold the ImportedFileDescriptor's ImportedBy field allows us to make re uploaded files
          // appear as if they were uploaded by the original uploader.
          claims.Add(new Claim("http://wso2.org/claims/applicationname", importedBy));

          continue;
        }

        claims.Add(claim);
      }

      var jwt = new JwtSecurityToken(
        issuer: tokenS.Issuer,
        claims: claims,
        notBefore: DateTime.UtcNow,
        signingCredentials: tokenS.SigningCredentials);

      return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(header: tokenS.Header, payload: jwt.Payload));
    }
  }
}
