using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VSS.Authentication.JWT;

namespace VSS.MasterData.WebAPI.Preference.Filters
{
	/// <summary>
	/// Handles Parsing UserUID from Header in Request
	/// </summary>
	[ExcludeFromCodeCoverage]
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ParseRequestUserUIDFromHeaderAttribute : ActionFilterAttribute
	{
		private string _parameterName { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parameterName"></param>
		public ParseRequestUserUIDFromHeaderAttribute(string parameterName)
		{
			_parameterName = parameterName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="actionContext"></param>
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			try
			{
				string userIdString = null;
				if (!string.IsNullOrEmpty(actionContext.HttpContext.Request.Headers["X-VisionLink-UserUid"].ToString()))
				{
					userIdString = actionContext.HttpContext.Request.Headers["X-VisionLink-UserUid"].ToString();
				}
				else if (actionContext.HttpContext.Request.Headers.ContainsKey("X-JWT-Assertion"))
				{
					var jwt = new TPaaSJWT(actionContext.HttpContext.Request.Headers["X-JWT-Assertion"]);
					if (jwt != null)
						userIdString = jwt.UserUid.ToString();
				}
				else
					actionContext.Result = new BadRequestObjectResult("Could not find X-VisionLink-UserUid or X-JWT-Assertion Headers in Request");
				if (!actionContext.ActionArguments.ContainsKey(_parameterName))
					actionContext.ActionArguments.Add(_parameterName, string.IsNullOrEmpty(userIdString) ? (Guid?)null : new Guid(userIdString));
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}
}