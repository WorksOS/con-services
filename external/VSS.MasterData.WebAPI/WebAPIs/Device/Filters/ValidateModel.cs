using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VSS.MasterData.WebAPI.Filters
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (context.ActionArguments?.Keys?.FirstOrDefault() != null && context.ActionArguments[context.ActionArguments.Keys.First()].GetType().BaseType == typeof(Newtonsoft.Json.JsonException))
			{
				context.Result = new BadRequestObjectResult("Request is Invalid");
			}
		}
	}
}