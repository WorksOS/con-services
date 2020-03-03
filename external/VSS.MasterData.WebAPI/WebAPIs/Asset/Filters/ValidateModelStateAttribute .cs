using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.MasterData.WebAPI.Asset.Filters
{
	[ExcludeFromCodeCoverage]
	public class ValidateModelStateAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				context.Result = new BadRequestObjectResult(context.ModelState); //context.ModelState
			}

			if (context.ActionArguments?.Keys?.FirstOrDefault() != null && context.ActionArguments[context.ActionArguments.Keys.First()]?.GetType().BaseType == typeof(Newtonsoft.Json.JsonException))
			{
				context.Result = new BadRequestObjectResult("Request is Invalid");
			}
		}
	}
}
