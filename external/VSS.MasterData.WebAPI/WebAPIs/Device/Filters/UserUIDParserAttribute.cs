using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using VSS.Authentication.JWT;
using VSS.MasterData.WebAPI.Helpers;

namespace VSS.MasterData.WebAPI.Filters
{
	/// <summary>
	/// User UID Parser Attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class UserUIDParserAttribute : ActionFilterAttribute
	{
		/// <summary>
		/// Action filter method for UserUidParserAttribute
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			try
			{	
				
				TPaaSJWT jwt = null;
				if (actionContext.HttpContext.Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					jwt = new TPaaSJWT(headerValues);
				}
				string userIdString = null;
				if (jwt != null && jwt.IsApplicationUserToken)
				{
					userIdString = jwt.UserUid.ToString();
				}
				else if (actionContext.HttpContext.Request.Headers.TryGetValue("X-VisionLink-UserUid",out StringValues values))
				{
					userIdString = values.FirstOrDefault();
				}

				actionContext.HttpContext.Request.Headers.Add(Constants.USERUID_APIRequest, userIdString);
			}
			catch
			{
				actionContext.Result = new BadRequestObjectResult("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
			}
		}
	}
}
