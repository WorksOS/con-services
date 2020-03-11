using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using VSS.Authentication.JWT;

namespace CommonApiLibrary.Filters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class UserUidParserAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Action filter method for UserUidParserAttribute
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			try
			{
				StringValues headerValues = new StringValues();
				string userIdString = null;

				//UserType userType;

				var jwt = new TPaaSJWT(actionContext.HttpContext.Request.Headers["X-JWT-Assertion"].ToString());
				//Enum.TryParse(jwt.UserType, out userType);
				if (jwt != null && jwt.IsApplicationUserToken)
					userIdString = jwt.UserUid.ToString();

				else if (actionContext.HttpContext.Request.Headers.TryGetValue("X-VisionLink-UserUid", out headerValues))
				{
					userIdString = headerValues.First();
				}

				actionContext.HttpContext.Request.Headers.Add("UserUID_IdentityAPI", userIdString);
			}
			catch
			{
				throw;
			}
		}
	}
}
