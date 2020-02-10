using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VSS.Authentication.JWT;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[ExcludeFromCodeCoverage]
	public class UserUIDParserAttribute : ActionFilterAttribute
	{
		private string _parameterName { get; set; }
		public UserUIDParserAttribute(string parameterName)
		{
			_parameterName = parameterName;
		}

		[ExcludeFromCodeCoverage]
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			try
			{
				string userIdString = null;

				TPaaSJWT jwt = null;
				if (actionContext.HttpContext.Request.Headers.TryGetValue("X-JWT-Assertion", out StringValues headerValues))
				{
					jwt = new TPaaSJWT(headerValues);
				}

				if (jwt != null)
					userIdString = jwt.UserUid.ToString();
				else if (actionContext.HttpContext.Request.Headers.TryGetValue("X-VisionLink-UserUid", out headerValues))
				{
					userIdString = headerValues.First();
				}
				actionContext.ActionArguments[_parameterName] = string.IsNullOrEmpty(userIdString) ? (Guid?)null : new Guid(userIdString);
			}
			catch
			{

				actionContext.Result = new BadRequestObjectResult("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
			}
		}
	}
}
