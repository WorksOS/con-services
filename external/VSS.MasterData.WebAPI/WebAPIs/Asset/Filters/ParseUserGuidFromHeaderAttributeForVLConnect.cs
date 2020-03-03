using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VSS.Authentication.JWT;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[ExcludeFromCodeCoverage]
	public class ParseUserGuidFromHeaderAttributeForVLConnect : ActionFilterAttribute
	{
		private string _parameterName { get; set; }

		public ParseUserGuidFromHeaderAttributeForVLConnect(string parameterName)
		{
			_parameterName = parameterName;
		}

		[ExcludeFromCodeCoverage]
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			try
			{
				IEnumerable<string> headerValues = new List<string>();
				string userIdString = null;
				TPaaSJWT jwt = null;
				userIdString = actionContext.HttpContext.Request.Headers.ContainsKey("X-VisionLink-UserUid") ? actionContext.HttpContext.Request.Headers["X-VisionLink-UserUid"].FirstOrDefault() : null;
				if (userIdString == null)
				{
					string encodedJwt = actionContext.HttpContext.Request.Headers.ContainsKey("X-JWT-Assertion") ? actionContext.HttpContext.Request.Headers["X-JWT-Assertion"].FirstOrDefault() : null;
					if (encodedJwt != null)
						jwt = new TPaaSJWT(encodedJwt);
				}
				if (jwt != null)
					userIdString = jwt.UserUid.ToString();
				if (!actionContext.ActionArguments.ContainsKey(_parameterName))
				{
					actionContext.ActionArguments[_parameterName] = string.IsNullOrEmpty(userIdString) ? (Guid?)null : new Guid(userIdString);
				}
			}
			catch
			{
				actionContext.Result = new BadRequestObjectResult("Could not validate X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
			}

		}
	}
}
