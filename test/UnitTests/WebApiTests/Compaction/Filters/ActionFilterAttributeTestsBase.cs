using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;

namespace VSS.Productivity3D.WebApiTests.Compaction.Filters
{
  public class ActionFilterAttributeTestsBase
  {
    protected ActionExecutingContext CallOnActionExecuting<T>(string queryString) where T : ActionFilterAttribute, new()
    {
      var context = CreateActionExecutingContext(new DefaultHttpContext
      {
        Request = {
          QueryString = new QueryString(queryString)
        }
      });

      var validationAttribute = new T();
      validationAttribute.OnActionExecuting(context);

      return context;
    }

    private static ActionExecutingContext CreateActionExecutingContext(HttpContext httpContext)
    {
      return new ActionExecutingContext(
        new ActionContext
        {
          HttpContext = httpContext,
          RouteData = new RouteData(),
          ActionDescriptor = new ActionDescriptor(),
        },
        new List<IFilterMetadata>(),
        new Dictionary<string, object>(),
        new Mock<Controller>().Object);
    }
  }
}