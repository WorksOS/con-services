using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	[ExcludeFromCodeCoverage]
	public class ParseAssetGuidsFromHeaderAttribute : ActionFilterAttribute
	{
		private string _parameterName { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameterName"></param>
		public ParseAssetGuidsFromHeaderAttribute(string parameterName)
		{
			_parameterName = parameterName;
		}

		[ExcludeFromCodeCoverage]
		public override void OnActionExecuting(ActionExecutingContext actionContext)
		{
			IEnumerable<string> headerValues;
			if (actionContext.HttpContext.Request.Headers.ContainsKey("AssetUIDs"))
			{
				headerValues = actionContext.HttpContext.Request.Headers["AssetUIDs"];
				if (headerValues.Any())
					try
					{
						string assetUIDStrings = headerValues.First();
						Guid[] guids = string.IsNullOrEmpty(assetUIDStrings) ? new Guid[] { } : assetUIDStrings.Split(',').Select(x => new Guid(x)).ToArray();
						if (actionContext.ActionArguments[_parameterName] == null)
							actionContext.ActionArguments[_parameterName] = guids;
					}
					catch (Exception)
					{
						throw new ArgumentException("AssetUIDs header is either empty or does not contain comma seperated Guid values");
					}
			}
		}
	}
}
