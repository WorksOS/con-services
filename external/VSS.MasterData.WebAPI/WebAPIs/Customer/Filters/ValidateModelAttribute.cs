using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace VSS.MasterData.WebAPI.Customer.Filters
{
	public class ValidateModelAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (context.ActionArguments?.Keys?.FirstOrDefault() != null
				&& context.ActionArguments[context.ActionArguments.Keys.First()] != null
				&& context.ActionArguments[context.ActionArguments.Keys.First()].GetType().BaseType
				== typeof(Newtonsoft.Json.JsonException))
			{
				context.Result = new BadRequestObjectResult("Request is Invalid");
			}
		}
	}
}