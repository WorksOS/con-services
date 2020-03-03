using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[ExcludeFromCodeCoverage]
	public class ParseCustomerGuidFromHeaderAttribute : ActionFilterAttribute
	{
		private string _parameterName { get; set; }

		public ParseCustomerGuidFromHeaderAttribute(string parameterName)
		{
			_parameterName = parameterName;
		}

		[ExcludeFromCodeCoverage]
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			string customerIdString = actionContext.HttpContext.Request.Headers.ContainsKey("X-VisionLink-CustomerUid") ? actionContext.HttpContext.Request.Headers["X-VisionLink-CustomerUid"].FirstOrDefault() : null;

			if (!actionContext.ActionArguments.ContainsKey(_parameterName))
			{
				actionContext.ActionArguments[_parameterName] = string.IsNullOrEmpty(customerIdString)
																? (Guid?)null
																: new Guid(customerIdString);
			}
		}
	}
}