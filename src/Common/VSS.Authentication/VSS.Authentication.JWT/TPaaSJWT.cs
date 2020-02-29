using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
// using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
// using System.Security.Cryptography;
// using Microsoft.IdentityModel.Tokens;

namespace VSS.Authentication.JWT
{
	public class TPaaSJWT
	{
		private const string ApplicationUserUserType = "APPLICATION_USER";
		private const string ApplicationUserType = "APPLICATION";

		/// <summary>
		/// The JWT encoded as a 3-part string
		/// </summary>
		public string EncodedJWT { get; private set; }

		/// <summary>
		/// A dictionary of all the claims
		/// </summary>
		public ReadOnlyDictionary<string, object> Claims { get; private set; }

		/// <summary>
		/// Unique id of the user
		/// </summary>
		public Guid UserUid => Guid.Parse(Claims["http://wso2.org/claims/uuid"].ToString());

		/// <summary>
		/// Application name
		/// </summary>
		public string ApplicationName => Claims["http://wso2.org/claims/applicationname"].ToString();

		/// <summary>
		/// Application id
		/// </summary>
		public string ApplicationId => Claims["http://wso2.org/claims/applicationid"].ToString();

		public string EmailAddress => Claims["http://wso2.org/claims/emailaddress"].ToString();

		/// <summary>
		/// The type of user (e.g. APPLICATION_USER or APPLICATION)
		/// </summary>
		public string UserType => Claims["http://wso2.org/claims/usertype"].ToString();

		/// <summary>
		/// True if this token is for a user of an application
		/// </summary>
		public bool IsApplicationUserToken => UserType == ApplicationUserUserType;

		/// <summary>
		/// True if this token is for an application (e.g. a back end service)
		/// </summary>
		public bool IsApplicationToken => UserType == ApplicationUserType;

		/// <summary>
		///   Api Context
		/// </summary>
		public string ApiContext => Claims["http://wso2.org/claims/apicontext"].ToString();

		/// <summary>
		///   Version
		/// </summary>
		public string Version => Claims["http://wso2.org/claims/version"].ToString();

		public TPaaSJWT(HttpRequestHeaders headers)
		{
			try
			{
				EncodedJWT = headers.GetValues("X-JWT-Assertion").FirstOrDefault();
				Claims = DecodeClaims(EncodedJWT);
			}
			catch (Exception)
			{
				throw new ArgumentException("Could not find X-JWT-Assertion header", "headers");
			}
		}

		public TPaaSJWT(string encodedJwt)
		{
			EncodedJWT = encodedJwt;
			Claims = DecodeClaims(EncodedJWT);
		}

		public static TPaaSJWT GenerateFakeApplicationUserJWT(Guid userUid)
		{
			var claimsToken =
				"{\"http://wso2.org/claims/applicationname\":\"VL2.0\",\"http://wso2.org/claims/usertype\":\"APPLICATION_USER\",\"http://wso2.org/claims/uuid\":\"" +
				userUid.ToString() +
				"\"}";
			var middleSection = Convert.ToBase64String(Encoding.UTF8.GetBytes(claimsToken));
			return new TPaaSJWT("xxxx." + middleSection + ".xxxx");
		}

		public static TPaaSJWT GenerateFakeApplicationJWT(string applicationName = "VL2.0")
		{
			var claimsToken =
				"{\"http://wso2.org/claims/applicationname\":\"" + applicationName +
				"\",\"http://wso2.org/claims/usertype\":\"APPLICATION\",\"http://wso2.org/claims/uuid\":\"024b0225-129b-4795-8b07-c897aa01a9f1\"}";
			var middleSection = Convert.ToBase64String(Encoding.UTF8.GetBytes(claimsToken));
			return new TPaaSJWT("xxxx." + middleSection + ".xxxx");
		}

		private static ReadOnlyDictionary<string, object> DecodeClaims(string encodedJwt)
		{
			Dictionary<string, object> claims;

			// Acceptance tests use a self-generated JWT that starts with "xxxx."
			var skipValidation = encodedJwt.StartsWith("xxxx.");

			if (true) // TODO: once validation is implemented, this should be changed to if(skipValidation)
			{
				claims = Jose.JWT.Payload<Dictionary<string, object>>(encodedJwt);
			}
			else
			{
				throw new NotSupportedException(
					"Gokul Somasundaram <gokul_somasundaram@trimble.com> from TPaaS is working on finalizing this implementation.");

				//public key of trimble.com cert in tpaas staging.
				//{ "keys": [ { "kty": "RSA", "n": "yrALPfXMsvjlY4d2QlmRV_K5IH4tQ2Kqn7B2BQPeVk2SKL72ZFQS1kD2P1SzQGIbkTI2qZ9O8o7AkA-yNdwiln3ZidxMmH5Cth68_Df2cwZjMnTV4xEFKuaEdtjMPCJH56vftjXrlu87A4gKyg3XIxMPmX34zjzBBX5WXEt4rLs", "e": "AQAB", "kid": "rsa1", "use": "sig" } ] }
				// string key = "yrALPfXMsvjlY4d2QlmRV_K5IH4tQ2Kqn7B2BQPeVk2SKL72ZFQS1kD2P1SzQGIbkTI2qZ9O8o7AkA-yNdwiln3ZidxMmH5Cth68_Df2cwZjMnTV4xEFKuaEdtjMPCJH56vftjXrlu87A4gKyg3XIxMPmX34zjzBBX5WXEt4rLs";

				// RSA rsa = RSA.Create();
				// rsa.ImportParameters(
				//   new RSAParameters()
				//   {
				//       Modulus = FromBase64Url(key),
				//       Exponent = FromBase64Url("AQAB")
				//   });

				// var validationParameters = new TokenValidationParameters
				// {
				//     RequireExpirationTime = true,
				//     RequireSignedTokens = true,
				//     ValidateAudience = false,
				//     ValidateIssuer = false,
				//     ValidateLifetime = false,
				//     IssuerSigningKey = new RsaSecurityKey(rsa)
				// };


				// try
				// {
				//     SecurityToken validatedSecurityToken = null;
				//     var handler = new JwtSecurityTokenHandler();
				//     handler.ValidateToken(encodedJwt, validationParameters, out validatedSecurityToken);
				//     JwtSecurityToken jwt = validatedSecurityToken as JwtSecurityToken;

				//     var claims = new Dictionary<string, object>();
				//     foreach (var item in jwt.Claims)
				//     {
				//         claims[item.Type] = item.Value;
				//     }

				//     return claims;
				// }
				// catch (Exception e)
				// {
				//     Console.WriteLine(e);
				//     throw;
				// }
			}

			return new ReadOnlyDictionary<string, object>(claims);
		}

		public override string ToString()
		{
			var result = new StringBuilder();
			foreach (var claim in Claims)
			{
				result.AppendLine("[" + claim.Key + ", " + claim.Value + "]\n");
			}

			return result.ToString();
		}
	}
}