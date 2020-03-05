using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using VSS.Authentication.JWT;

namespace VSS.MasterData.WebAPI.Device.Filters
{
	public static class UserInfoParser
	{
		public static string Parse(HttpContext httpContext)
		{
			try
			{

				TPaaSJWT jwt = null;
				if (httpContext.Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					jwt = new TPaaSJWT(headerValues);
				}
				string userIdString = null;
				if (jwt != null && jwt.IsApplicationUserToken)
				{
					userIdString = jwt.UserUid.ToString();
				}
				else if (httpContext.Request.Headers.TryGetValue("X-VisionLink-UserUid", out StringValues values))
				{
					userIdString = values.FirstOrDefault();
				}

				return string.IsNullOrEmpty(userIdString)?"NoUserDataAvailable": userIdString;
			}
			catch
			{
				return "Could not parse X-VisionLink-UserUid or X-JWT-Assertion Headers in Request";
			}
		}
	}
}
